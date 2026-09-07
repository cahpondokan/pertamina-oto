"""Static-only consistency checks for the Device/AI disposition packet.

This module reads JSON/text only. It does not import provider, model, device,
document, PDF, build, restore, Sandbox, qpdf, or PDFsharp tooling.
"""

from __future__ import annotations

import copy
import json
import re
from pathlib import Path


HERE = Path(__file__).resolve().parent
PACKET_PATH = HERE / "device-ai-disposition.proposal.v1.json"
SCHEMA_PATH = HERE / "device-ai-disposition.schema.v1.json"

EXPECTED_FEATURES = {
    "POC-SCAN-001": ["FEAT-SCAN-014", "FEAT-SCAN-018"],
    "POC-INTEL-001": ["FEAT-INTEL-001"],
    "POC-INTEL-002": ["FEAT-INTEL-002"],
    "POC-CONV-004": ["FEAT-CONV-036"],
    "POC-AI-LOCAL-001": ["FEAT-ASSIST-026"],
}

EXPECTED_REQUIREMENTS = {
    "POC-SCAN-001": ["FR-SCAN-001", "FR-SCAN-002", "FR-PROC-001", "FR-PROC-002", "FR-PROC-003", "FR-PROC-004", "FR-PROC-005"],
    "POC-INTEL-001": ["FR-INTEL-001", "FR-PROC-001", "FR-PROC-002", "FR-PROC-003", "FR-PROC-004", "FR-PROC-005", "SR-SEC-021"],
    "POC-INTEL-002": ["FR-INTEL-002", "FR-PROC-001", "FR-PROC-002", "FR-PROC-003", "FR-PROC-004", "FR-PROC-005", "SR-SEC-021"],
    "POC-CONV-004": ["FR-INTEL-003", "FR-PROC-001", "FR-PROC-002", "FR-PROC-003", "FR-PROC-004", "FR-PROC-005", "SR-SEC-021"],
    "POC-AI-LOCAL-001": [
        "FR-ASSIST-001", "FR-ASSIST-002", "FR-ASSIST-003", "FR-ASSIST-004", "FR-ASSIST-005",
        "FR-ASSIST-006", "FR-ASSIST-007", "FR-ASSIST-008", "FR-ASSIST-009", "FR-ASSIST-010",
        "FR-ASSIST-011", "FR-ASSIST-012", "FR-ASSIST-013", "FR-ASSIST-014", "FR-ASSIST-015",
        "SR-SEC-013", "SR-SEC-014", "SR-SEC-015", "SR-SEC-016", "SR-SEC-017", "SR-SEC-018",
    ],
}

FALSE_AUTHORITY_FIELDS = (
    "executionAuthorized",
    "endpointCallAuthorized",
    "downloadAuthorized",
    "installAuthorized",
    "restoreAuthorized",
    "buildAuthorized",
    "modelAcquisitionAuthorized",
    "deviceAccessAuthorized",
    "documentProcessingAuthorized",
    "iosScopeChangeAuthorized",
    "sandboxLaunchAuthorized",
    "qpdfExecutionAuthorized",
    "pdfsharpLoadOrExecutionAuthorized",
)


def _require(condition: bool, message: str) -> None:
    if not condition:
        raise ValueError(message)


def validate(packet: dict, schema: dict) -> None:
    _require(set(packet) == set(schema["properties"]), "top-level fields do not match schema")
    _require(schema.get("$id") == "urn:pertamina-oto:poc:device-ai-disposition:1.0", "schema id mismatch")
    _require(packet.get("schemaVersion") == "1.0", "schemaVersion mismatch")
    _require(packet.get("packetId") == "P2-W0G-B-DEVICE-AI-DISPOSITION-PROPOSAL-001", "packetId mismatch")
    _require(packet.get("taskId") == "P2-W0G-B-A5-DEVICE-AI-DISPOSITION", "taskId mismatch")
    _require(packet.get("runId") == "P02-20260905-1434", "runId mismatch")
    _require(re.fullmatch(r"[0-9a-f]{40}", packet.get("baselineCommit", "")) is not None, "invalid baseline commit")
    _require(packet.get("status") == "PROPOSED_UNEXECUTED_BLOCKED", "packet must remain proposed/unexecuted/blocked")

    authority = packet["authorityBoundary"]
    _require(authority.get("preparationAuthorized") is True, "static preparation authority must be explicit")
    for field in FALSE_AUTHORITY_FIELDS:
        _require(authority.get(field) is False, f"{field} must remain false")
    _require(authority.get("costBoundary") == "ZERO_ADDITIONAL_COST_STOP_BEFORE_ANY_PAID_COMMITMENT", "cost boundary mismatch")
    ai_boundary = authority["aiBoundary"]
    _require(ai_boundary.get("publicAi") == "DENIED", "public AI must be denied")
    _require(ai_boundary.get("publicTranslation") == "DENIED", "public translation must be denied")
    _require(ai_boundary.get("specificProviderApproved") is False, "no provider may be implied approved")
    _require(ai_boundary.get("permittedProviderClasses") == ["APPROVED_LOCAL", "APPROVED_ENTERPRISE_INTERNAL"], "provider class boundary mismatch")

    scope = packet["scope"]
    _require(scope.get("releaseProfile") == "WINDOWS_DESKTOP_OFFLINE_FIRST", "release profile mismatch")
    _require(scope["platformPolicy"].get("ios") == "DEFERRED_NOT_IN_FIRST_RELEASE_NOT_REOPENED", "iOS must remain deferred")

    records = packet["portfolio"]
    _require(len(records) == len(EXPECTED_FEATURES), "portfolio must contain exactly five PoCs")
    by_id = {record["pocId"]: record for record in records}
    _require(len(by_id) == len(records), "duplicate PoC id")
    _require(set(by_id) == set(EXPECTED_FEATURES), "PoC coverage mismatch")
    mapping_by_id = {item["pocId"]: item["exactFeatureIds"] for item in scope["exactFeatureMappings"]}
    _require(mapping_by_id == EXPECTED_FEATURES, "scope exact feature map mismatch")

    for poc_id, expected_features in EXPECTED_FEATURES.items():
        record = by_id[poc_id]
        _require(set(record) == set(schema["$defs"]["pocRecord"]["properties"]), f"{poc_id} fields do not match schema")
        _require(record["exactFeatureIds"] == expected_features, f"{poc_id} exact Feature IDs mismatch")
        _require(record["exactRequirementIds"] == EXPECTED_REQUIREMENTS[poc_id], f"{poc_id} exact Requirement IDs mismatch")
        _require(record["currentDisposition"] == "NOT_STARTED_BLOCKED_PRE_EXECUTION", f"{poc_id} current disposition mismatch")
        _require(record["formalGovernedDisposition"] == "NONE_NOT_GRANTED", f"{poc_id} must not imply human disposition")
        _require(record["aiBoundary"].get("publicEndpoint") == "DENIED", f"{poc_id} public endpoint must be denied")
        _require(record["platformExposure"].get("ios") == "DEFERRED_NOT_REOPENED", f"{poc_id} must not reopen iOS")
        _require(record["model"].get("candidate") == "TBC_APPROVED_CANDIDATE", f"{poc_id} must not assume a model/provider candidate")
        _require(record["device"].get("approvedMatrix") == "TBC_APPROVED_DEVICE_ENVIRONMENT_MATRIX", f"{poc_id} must not assume a device")
        _require(record["license"].get("decision") == "TBC_HUMAN_LICENSE_COST_REVIEW", f"{poc_id} license decision must remain TBC")
        _require(len(record["prerequisites"]) > 0, f"{poc_id} prerequisites missing")
        for prerequisite in record["prerequisites"]:
            _require(set(prerequisite) == set(schema["$defs"]["prerequisite"]["properties"]), f"{poc_id} prerequisite fields do not match schema")
            _require(prerequisite["status"] != "APPROVED", f"{poc_id} may not invent approval")
            _require(bool(prerequisite["owner"].strip()), f"{poc_id} prerequisite owner missing")
        _require(len(record["reopenConditions"]) > 0, f"{poc_id} reopen conditions missing")
        _require(len(record["stopConditions"]) > 0, f"{poc_id} stop conditions missing")

    hashes = [binding["sha256"] for binding in packet["sourceBindings"]]
    paths = [binding["path"] for binding in packet["sourceBindings"]]
    _require(len(paths) == len(set(paths)), "duplicate source binding path")
    _require(all(re.fullmatch(r"[0-9a-f]{64}", digest) for digest in hashes), "invalid source SHA-256")

    gate = packet["gateSummary"]
    _require(gate.get("formalPocDispositionGranted") is False, "formal PoC disposition must remain ungranted")
    _require(gate.get("pre012ExecutionAuthority") is False, "PRE-012 must remain false")


def _expect_rejected(packet: dict, schema: dict, mutate, label: str) -> str:
    candidate = copy.deepcopy(packet)
    mutate(candidate)
    try:
        validate(candidate, schema)
    except ValueError:
        return label
    raise ValueError(f"negative fixture unexpectedly accepted: {label}")


def main() -> int:
    packet = json.loads(PACKET_PATH.read_text(encoding="utf-8"))
    schema = json.loads(SCHEMA_PATH.read_text(encoding="utf-8"))
    validate(packet, schema)

    rejected = [
        _expect_rejected(packet, schema, lambda value: value["authorityBoundary"].__setitem__("executionAuthorized", True), "execution-authority-true"),
        _expect_rejected(packet, schema, lambda value: value["authorityBoundary"]["aiBoundary"].__setitem__("publicAi", "ALLOWED"), "public-ai-enabled"),
        _expect_rejected(packet, schema, lambda value: value["portfolio"][0].__setitem__("exactFeatureIds", ["FEAT-SCAN-001"]), "feature-map-drift"),
        _expect_rejected(packet, schema, lambda value: value["scope"]["platformPolicy"].__setitem__("ios", "OPEN"), "ios-reopened"),
    ]
    print(json.dumps({"result": "PASS", "packet": PACKET_PATH.name, "pocRecords": 5, "negativeFixturesRejected": rejected}, separators=(",", ":")))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
