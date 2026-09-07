#!/usr/bin/env python3
"""Static verifier for the Wave 0G-C governance decision packet.

This script parses JSON and hashes declared text/static evidence only. It does
not invoke a scanner, Sandbox, candidate, oracle, restore, build, or PDF tool.
"""

from __future__ import annotations

import argparse
import copy
import hashlib
import json
import re
import sys
from pathlib import Path


HERE = Path(__file__).resolve().parent
REPOSITORY_ROOT = HERE.parents[3]
PACKET_PATH = HERE / "governance-decision-packet.v1.json"
SCHEMA_PATH = HERE / "governance-decision-packet.schema.v1.json"


def fail(message: str) -> None:
    raise AssertionError(message)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def assert_schema_shape(packet: dict, schema: dict) -> None:
    if schema.get("schemaId") != "P02-W0G-C-GOVERNANCE-DECISION-PACKET-SCHEMA-1.0":
        fail("schema identity drift")
    if schema.get("additionalProperties") is not False:
        fail("schema must reject unknown top-level properties")
    required = schema.get("required")
    properties = schema.get("properties")
    if not isinstance(required, list) or not isinstance(properties, dict):
        fail("schema required/properties missing")
    if set(required) != set(properties):
        fail("schema required/property sets differ")
    if set(packet) != set(required):
        fail("packet top-level fields differ from closed schema")

    expected_types = {
        "object": dict,
        "array": list,
        "string": str,
        "boolean": bool,
        "integer": int,
    }
    for name, rule in properties.items():
        expected = expected_types.get(rule.get("type"))
        if expected is not None and not isinstance(packet[name], expected):
            fail(f"schema type mismatch: {name}")
        if "const" in rule and packet[name] != rule["const"]:
            fail(f"schema const mismatch: {name}")
        if isinstance(packet[name], list):
            if len(packet[name]) < rule.get("minItems", 0):
                fail(f"schema minItems mismatch: {name}")
            if "maxItems" in rule and len(packet[name]) > rule["maxItems"]:
                fail(f"schema maxItems mismatch: {name}")
        if isinstance(packet[name], dict) and rule.get("additionalProperties") is False:
            nested_required = rule.get("required", [])
            if set(packet[name]) != set(nested_required):
                fail(f"closed nested schema mismatch: {name}")


def assert_core(packet: dict) -> None:
    metadata = packet["metadata"]
    if metadata["taskId"] != "P2-W0G-C-A7-GOVERNANCE-EVIDENCE-REVIEW":
        fail("task identity drift")
    if metadata["runId"] != "P02-20260905-1434":
        fail("run identity drift")
    if metadata["baselineCommit"] != "df37fd77aafd569082a568556e75e7e452d3596b":
        fail("baseline commit drift")
    if metadata["status"] != "DECISION_PACKET_COMPLETE_GATES_BLOCKED":
        fail("packet status must remain blocked")
    if metadata["securityDisposition"] != "BLOCKED_SECURITY":
        fail("Agent 6 BLOCKED_SECURITY disposition was weakened")
    if metadata["upstreamSecurityReplay"] != "FAIL_INPUT_BINDING_DRIFT":
        fail("security replay drift must remain explicit")
    for key in ("executionApproval", "selfApproval", "engineDispositionGranted"):
        if metadata[key] is not False:
            fail(f"forbidden approval flag: {key}")

    boundary = packet["authorityBoundary"]
    if boundary["staticReviewOnly"] is not True:
        fail("review must remain static only")
    for key, value in boundary.items():
        if key != "staticReviewOnly" and value is not False:
            fail(f"authority boundary must remain false: {key}")

    findings = {item.get("id"): item for item in packet["sourceIntegrityFindings"]}
    if set(findings) != {"GOV-C-001", "GOV-C-002"}:
        fail("source/schema integrity finding set drift")
    if any(item.get("severity") != "BLOCKER" for item in findings.values()):
        fail("integrity findings must remain blockers")
    drift_text = json.dumps(findings["GOV-C-001"], sort_keys=True)
    for token in (
        "e734889428dd9986810be44b6f3a6bd2bb63cdc756b304a2c7dbdbd10cb907e3",
        "076047bfe559f53396f705e3ac46d41a27302526ca22a75e409d294fd0ad5d06",
        "BLOCKED_SECURITY",
    ):
        if token not in drift_text:
            fail(f"security binding drift evidence incomplete: {token}")

    decision = packet["candidateAndLicenseDecision"]
    if decision["evaluationBoundary"]["status"] != "BLOCKED_PENDING_HUMAN_ACCEPTANCE_AND_SECURITY_CLOSURE":
        fail("evaluation boundary was opened")
    if decision["evaluationBoundary"]["copyIntoDisposableGuest"] != "NOT_AUTHORIZED":
        fail("guest copying was authorized")
    if decision["redistributionBoundary"]["status"] != "EXCLUDED_NOT_REVIEWED_NOT_AUTHORIZED":
        fail("redistribution boundary was opened")
    for key in ("desktopPackaging", "shippingToUsers", "thirdPartyConveyance"):
        if decision["redistributionBoundary"][key] != "DENIED":
            fail(f"redistribution denial drift: {key}")

    expected_components = {
        ("PDFsharp", "6.2.4", 4625326, "5647882339b327cbf6da9ea3f882efb335f8d602e6fd45323adf9e78da53cf67"),
        ("Microsoft.Extensions.Logging.Abstractions", "8.0.3", 802188, "e4c498d5a13051b4577a148f1d8c3470167215c507e2392069b75dc61322bb74"),
        ("System.Security.Cryptography.Pkcs", "8.0.1", 1078389, "28c348909df243fe4ee962213e3c8021aaecbc8321929dba03a69bcb92a49de5"),
        ("Microsoft.Extensions.DependencyInjection.Abstractions", "8.0.2", 278862, "51f2df1100245f10da54f0bb7e813f277155117777d4fbbab902214e27372606"),
        ("qpdf-msvc64", "12.4.1", 28165367, "3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6"),
    }
    observed_components = {
        (item.get("id"), item.get("version"), item.get("bytes"), item.get("sha256"))
        for item in decision["components"]
    }
    if observed_components != expected_components:
        fail("candidate/component identity drift")
    if any(item.get("securityDecision") != "BLOCKED_SECURITY" for item in decision["components"]):
        fail("component security status was weakened")

    expected_license_gaps = {f"GOV-LIC-{number:03d}" for number in range(1, 7)}
    if {item.get("id") for item in decision["componentAndNoticeGaps"]} != expected_license_gaps:
        fail("license/notice gap set drift")
    if decision["sbom"]["currentStatus"] != "BLOCKED_FORMAT_AND_CONTENT_NOT_APPROVED":
        fail("SBOM status was opened")
    if decision["sbom"]["formatAndVersion"] != "REQUIRED_NOT_SET":
        fail("SBOM format was invented or approved")
    if len(decision["sbom"]["requiredCoverage"]) < 7:
        fail("SBOM coverage incomplete")
    for key in ("internalSupportOwner", "patchAndAdvisoryOwner", "removalAndFallbackOwner"):
        if decision["support"][key] != "REQUIRED_NOT_SET":
            fail(f"support owner must remain unresolved: {key}")

    scope = packet["futureExecutionScope"]
    if scope["status"] != "BLOCKED_NOT_AUTHORIZED":
        fail("future execution scope was authorized")
    if scope["pocId"] != "POC-PDF-001" or scope["featureIds"] != ["FEAT-PDF-005"]:
        fail("future PoC/Feature scope drift")
    expected_experiments = {f"PDF001-E{number:02d}" for number in range(13)}
    if set(scope["experimentIds"]) != expected_experiments or len(scope["experimentIds"]) != 13:
        fail("experiment set must be exact E00..E12")
    expected_unit = {f"UT-MERGE-{number:03d}" for number in range(1, 51)}
    if set(scope["unitTraceIds"]) != expected_unit or len(scope["unitTraceIds"]) != 50:
        fail("unit trace set must be exact UT-MERGE-001..050")
    expected_system = {f"TP-MRG-{number:03d}" for number in range(1, 37)}
    if set(scope["systemTraceIds"]) != expected_system or len(scope["systemTraceIds"]) != 36:
        fail("system trace set must be exact TP-MRG-001..036")
    expected_profiles = {
        "FE-ENV-OBSERVE",
        "FE-PDFSHARP-BOOTSTRAP-RESTORE",
        "FE-PDFSHARP-LOCKED-RESTORE",
        "FE-PDFSHARP-BUILD",
        "FE-PDFSHARP-RUN",
        "FE-QPDF-ORACLE",
    }
    if {item.get("id") for item in scope["futureProfiles"]} != expected_profiles:
        fail("future profile set drift")
    if any(item.get("status") != "BLOCKED_NOT_AUTHORIZED" for item in scope["futureProfiles"]):
        fail("future profile was authorized")
    if sorted(item.get("order") for item in scope["futureProfiles"]) != list(range(1, 7)):
        fail("future profile order drift")
    if any(value != "REQUIRED_NOT_SET" for value in scope["taskCardFields"].values()):
        fail("future execution task-card field was invented")
    exclusions = " ".join(scope["explicitExclusions"])
    for token in ("product source", "all PoCs except POC-PDF-001", "Poppler MuPDF iText", "redistribution"):
        if token not in exclusions:
            fail(f"explicit exclusion missing: {token}")

    custody = packet["custodyAndRetentionDecision"]
    unresolved_fields = {
        "corpus": (
            "classificationAuthority", "classification", "owner", "reviewer",
            "access", "retention", "expiryAsiaJakarta", "disposalMethod", "disposalOwner",
        ),
        "environment": (
            "environmentOwner", "leastPrivilegeIdentityClass", "networkAndEgressObserver",
            "resourceAndCleanupCeilings", "recoveryAndDisposalOwner",
        ),
        "evidence": (
            "classification", "custodian", "writerIdentity", "readReviewAccess",
            "sanitizationReviewer", "retention", "expiryAsiaJakarta", "disposalMethod",
            "disposalOwner", "writeOnceOrImmutabilityMechanism", "manifestAndIndexSchema",
            "sbomFormatAndVersion",
        ),
    }
    for section, fields in unresolved_fields.items():
        for field in fields:
            if not custody[section][field].startswith("REQUIRED"):
                fail(f"unresolved custody field was invented: {section}.{field}")
    if custody["corpus"]["type"] != "SYNTHETIC_ONLY":
        fail("corpus boundary drift")
    if len(custody["corpus"]["missingGroups"]) != 5:
        fail("corpus gap set incomplete")

    reviewers = packet["reviewersAndOwners"]
    required_roles = {
        "Technical Owner", "Product Owner", "QA and Corpus Authority",
        "License and Cost Reviewer", "Security Approver and Stop Authority",
        "Environment Owner", "Evidence Owner and Custodian", "Execution Approver",
        "Independent Security Reviewer", "Independent PDF Quality and Oracle Reviewer",
    }
    if {item.get("role") for item in reviewers} != required_roles:
        fail("reviewer/owner role set drift")
    if any(item.get("namedHuman") != "REQUIRED_NOT_SET" for item in reviewers):
        fail("human identity was invented")
    if any(item.get("mayBeAgent") is not False for item in reviewers):
        fail("an agent was allowed to replace a human authority")

    cost = packet["zeroCostProof"]
    if cost["boundary"] != "ZERO_ADDITIONAL_COST":
        fail("cost boundary drift")
    if cost["humanAcceptance"] != "REQUIRED_NOT_SET":
        fail("human cost acceptance was invented")
    if len(cost["excludedCostClasses"]) < 8:
        fail("cost exclusion set incomplete")

    preconditions = packet["preconditionMatrix"]
    if len(preconditions) != 10:
        fail("PRE-003..012 matrix length drift")
    observed_pre = {item.get("id"): item for item in preconditions}
    expected_pre = {f"PRE-{number:03d}" for number in range(3, 13)}
    if set(observed_pre) != expected_pre:
        fail("PRE-003..012 matrix identity drift")
    for number in range(3, 12):
        item = observed_pre[f"PRE-{number:03d}"]
        expected_status = "BLOCKED_SECURITY" if number == 6 else "BLOCKED"
        if item.get("gateStatus") != expected_status:
            fail(f"gate status weakened: PRE-{number:03d}")
        if item.get("decisionRowComplete") is not True:
            fail(f"decision row incomplete: PRE-{number:03d}")
        if not item.get("blockers") or not item.get("owners") or not item.get("exactHumanAction"):
            fail(f"decision row lacks blocker/owner/action: PRE-{number:03d}")
    if observed_pre["PRE-012"].get("gateStatus") != "HUMAN_ONLY_NOT_APPROVED":
        fail("PRE-012 must remain human-only and unapproved")
    pre006 = json.dumps(observed_pre["PRE-006"], sort_keys=True)
    for token in ("GOV-C-001", "SEC-G-001", "BLOCKED_SECURITY", "re-review"):
        if token not in pre006:
            fail(f"PRE-006 does not preserve security blocker: {token}")

    stop_ids = {item.get("id") for item in packet["stopConditions"]}
    if stop_ids != {f"STOP-{number:03d}" for number in range(1, 25)}:
        fail("stop-condition set must be exact STOP-001..024")

    portfolio = packet["portfolioIsolation"]
    expected_portfolio = {
        "conversion-ocr-disposition.proposal.v1.json": {
            "POC-CONV-001", "POC-CONV-002", "POC-OCR-001", "POC-OCR-002",
            "POC-CONV-003", "POC-PDF-003", "POC-CONV-004",
        },
        "device-ai-disposition.proposal.v1.json": {
            "POC-SCAN-001", "POC-INTEL-001", "POC-INTEL-002",
            "POC-CONV-004", "POC-AI-LOCAL-001",
        },
    }
    if {item.get("packet") for item in portfolio} != set(expected_portfolio):
        fail("portfolio packet set drift")
    for item in portfolio:
        if set(item.get("pocIds", [])) != expected_portfolio[item["packet"]]:
            fail(f"portfolio PoC set drift: {item['packet']}")
        if item.get("status") != "COVERED_STATIC_ONLY_REMAINS_BLOCKED":
            fail(f"portfolio status weakened: {item['packet']}")
        if item.get("inheritPocPdf001Authority") is not False:
            fail(f"portfolio inherited authority: {item['packet']}")

    human = packet["humanDecisionPacket"]
    if human["currentDecisionStatus"] != "READY_FOR_HUMAN_BLOCKER_DISPOSITION_NOT_EXECUTION_APPROVAL":
        fail("human packet status drift")
    allowed_current = {
        "REVISION_REQUIRED_ZERO_COST_REMEDIATION",
        "DEFER_POC_PDF_001_WITH_OWNER_AND_REOPEN_CONDITION",
        "REJECT_CURRENT_CANDIDATE_SET_AND_REQUIRE_NEW_INTAKE",
    }
    if set(human["selectOneCurrentDisposition"]) != allowed_current:
        fail("current human disposition choices drift")
    if human["futureExecutionAuthorizationEligible"] is not False:
        fail("future execution authorization became eligible")
    current_wording = human["safeCurrentDecisionWording"]
    for token in ("PRE-003 sampai PRE-011 tetap BLOCKED", "PRE-012 tetap HUMAN_ONLY_NOT_APPROVED", "BLOCKED_SECURITY", "tidak ada izin"):
        if token not in current_wording:
            fail(f"safe current decision wording incomplete: {token}")
    future_wording = human["futureExecutionAuthorizationWording"]
    for token in (
        "JANGAN GUNAKAN SEBELUM", "POC-PDF-001", "FEAT-PDF-005",
        "PDF001-E00 sampai PDF001-E12", "ZERO_ADDITIONAL_COST",
        "STOP-001 sampai STOP-024", "Tidak ada",
    ):
        if token not in future_wording:
            fail(f"future authorization wording incomplete: {token}")

    verify = packet["verificationBoundary"]
    if verify["staticReadOnlyOnly"] is not True:
        fail("verification boundary is not static")
    if verify["inputHashesRecomputed"] is not True or verify["schemaChecked"] is not True:
        fail("verification claims incomplete")
    if verify["securityVerifierReplayExitCode"] != 1:
        fail("security verifier drift was hidden")
    if verify["securityVerifierReplayDisposition"] != "EXPECTED_BLOCKER_RECORDED_NOT_OVERRIDDEN":
        fail("security replay disposition drift")
    for key in (
        "scannerExecuted", "sandboxLaunched", "candidateExecuted", "oracleExecuted",
        "restoreExecuted", "buildExecuted", "pdfOpened",
        "downloadOrInstallPerformed", "paidCommitmentMade", "systemChanged",
    ):
        if verify[key] is not False:
            fail(f"prohibited verification action claimed: {key}")


def assert_bindings(packet: dict, coordination_root: Path) -> None:
    bindings = packet["sourceBindings"]
    if len(bindings) != 39:
        fail(f"expected 39 source bindings, got {len(bindings)}")
    roots = {"repository": REPOSITORY_ROOT, "coordination": coordination_root}
    seen: set[tuple[str, str]] = set()
    for binding in bindings:
        if set(binding) != {"root", "path", "bytes", "sha256"}:
            fail(f"source binding shape invalid: {binding!r}")
        root_name = binding["root"]
        relative = binding["path"]
        if root_name not in roots or not isinstance(relative, str):
            fail(f"source binding root/path invalid: {binding!r}")
        key = (root_name, relative)
        if key in seen:
            fail(f"duplicate source binding: {root_name}:{relative}")
        seen.add(key)
        path = roots[root_name] / Path(relative)
        if not path.is_file():
            fail(f"bound source missing: {path}")
        if path.stat().st_size != binding["bytes"]:
            fail(f"bound source byte-length drift: {path}: {path.stat().st_size}")
        observed_hash = sha256(path)
        if observed_hash != binding["sha256"]:
            fail(f"bound source hash drift: {path}: {observed_hash}")
        if not re.fullmatch(r"[0-9a-f]{64}", binding["sha256"]):
            fail(f"source hash is not lowercase SHA-256: {relative}")


def assert_negative_fixtures(packet: dict, schema: dict, coordination_root: Path) -> None:
    mutations: list[tuple[str, dict, bool]] = []

    changed = copy.deepcopy(packet)
    changed["metadata"]["executionApproval"] = True
    mutations.append(("execution-approval", changed, False))

    changed = copy.deepcopy(packet)
    changed["metadata"]["securityDisposition"] = "READY"
    mutations.append(("security-clearance", changed, False))

    changed = copy.deepcopy(packet)
    changed["authorityBoundary"]["redistributionAuthorized"] = True
    mutations.append(("redistribution-authority", changed, False))

    changed = copy.deepcopy(packet)
    changed["preconditionMatrix"][3]["gateStatus"] = "PASS"
    mutations.append(("pre006-pass", changed, False))

    changed = copy.deepcopy(packet)
    changed["preconditionMatrix"][0]["blockers"] = []
    mutations.append(("missing-blocker", changed, False))

    changed = copy.deepcopy(packet)
    changed["futureExecutionScope"]["futureProfiles"][0]["status"] = "AUTHORIZED"
    mutations.append(("future-profile-authorized", changed, False))

    changed = copy.deepcopy(packet)
    changed["humanDecisionPacket"]["futureExecutionAuthorizationEligible"] = True
    mutations.append(("pre012-eligible", changed, False))

    changed = copy.deepcopy(packet)
    changed["zeroCostProof"]["boundary"] = "PAID_ALLOWED"
    mutations.append(("paid-boundary", changed, False))

    changed = copy.deepcopy(packet)
    changed["portfolioIsolation"][0]["inheritPocPdf001Authority"] = True
    mutations.append(("portfolio-inheritance", changed, False))

    changed = copy.deepcopy(packet)
    changed["sourceBindings"][0]["sha256"] = "0" * 64
    mutations.append(("source-hash-drift", changed, True))

    for label, mutation, binding_only in mutations:
        try:
            if binding_only:
                assert_bindings(mutation, coordination_root)
            else:
                assert_schema_shape(mutation, schema)
                assert_core(mutation)
        except AssertionError:
            continue
        fail(f"negative fixture was not rejected: {label}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--coordination-root", required=True, type=Path)
    args = parser.parse_args()
    coordination_root = args.coordination_root.resolve()
    if not coordination_root.is_dir():
        fail(f"coordination root does not exist: {coordination_root}")

    packet = json.loads(PACKET_PATH.read_text(encoding="utf-8"))
    schema = json.loads(SCHEMA_PATH.read_text(encoding="utf-8"))
    assert_schema_shape(packet, schema)
    assert_core(packet)
    assert_bindings(packet, coordination_root)
    assert_negative_fixtures(packet, schema, coordination_root)

    print("GOVERNANCE_FREEZE_STATIC_VERIFICATION=PASS")
    print("SOURCE_BINDINGS=39;SCHEMA=1;NEGATIVE_FIXTURES=10;LICENSE_NOTICE_GAPS=6;STOP_RULES=24")
    print("PRE003_011_BLOCKED=9;PRE012=HUMAN_ONLY_NOT_APPROVED;SECURITY=BLOCKED_SECURITY;UPSTREAM_REPLAY=FAIL_INPUT_BINDING_DRIFT")
    print("EXECUTION_APPROVAL=false;SELF_APPROVAL=false;ENGINE_DISPOSITION=false;COST=ZERO_ADDITIONAL_COST")
    print("SCANNERS=0;SANDBOX_LAUNCHES=0;RESTORES=0;BUILDS=0;CANDIDATE_EXECUTIONS=0;ORACLE_EXECUTIONS=0;PDF_FILES_OPENED=0;ACQUISITIONS=0")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (AssertionError, json.JSONDecodeError, OSError) as error:
        print(f"GOVERNANCE_FREEZE_STATIC_VERIFICATION=FAIL: {error}", file=sys.stderr)
        raise SystemExit(1)
