#!/usr/bin/env python3
"""Static verifier for the Wave 0G-D governance decision recheck.

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
MUTABLE_COORDINATION_PATHS = {
    "docs/project-management/PROJECT_STATE.md",
    "docs/project-management/PHASE_DECISION_REGISTER.md",
    "docs/project-management/PHASE_EXECUTION_LOG.md",
}


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
    if metadata["taskId"] != "P2-W0G-D-A7-GOVERNANCE-RECHECK":
        fail("task identity drift")
    if metadata["runId"] != "P02-20260905-1434":
        fail("run identity drift")
    if metadata["baselineCommit"] != "62d8a843771e7ac4fa4cad965f9489329e52847c":
        fail("baseline commit drift")
    if metadata["status"] != "DECISION_PACKET_COMPLETE_GATES_BLOCKED":
        fail("packet status must remain blocked")
    if metadata["securityDisposition"] != "BLOCKED_SECURITY":
        fail("Agent 6 BLOCKED_SECURITY disposition was weakened")
    if metadata["upstreamSecurityReplay"] != "PASS_SEMANTIC_FAIL_CLOSED_BLOCKED_SECURITY_PRESERVED":
        fail("security remediation replay result drift")
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
    if findings["GOV-C-001"].get("severity") != "RESOLVED_CONTROL_GAP":
        fail("GOV-C-001 remediation status drift")
    if findings["GOV-C-002"].get("severity") != "BLOCKER":
        fail("GOV-C-002 must remain a blocker")
    drift_text = json.dumps(findings["GOV-C-001"], sort_keys=True)
    for token in (
        "6a5f8c312f11d92330b5fb3080d1dc2f776afdcb85b2c93bd278e9fe84a27174",
        "327071c61a6863cbe58c1d3b875e36b7d816db3d1c73fbbac4bd738f89b9e61d",
        "BLOCKED_SECURITY",
    ):
        if token not in drift_text:
            fail(f"security remediation evidence incomplete: {token}")

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
    for token in ("SEC-G-001", "SEC-G-011", "BLOCKED_SECURITY", "reverify"):
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
    if verify["securityVerifierReplayExitCode"] != 0:
        fail("security verifier remediation replay did not pass")
    if verify["securityVerifierReplayDisposition"] != "PASS_BINDING_REMEDIATION_ONLY_BLOCKED_SECURITY_PRESERVED":
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
    if len(bindings) != 38:
        fail(f"expected 38 immutable source bindings, got {len(bindings)}")
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
        if root_name == "coordination" and relative in MUTABLE_COORDINATION_PATHS:
            fail(f"mutable coordination file incorrectly strict-bound: {relative}")
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


def read_mutable_coordination(packet: dict, coordination_root: Path) -> tuple[dict[str, str], int]:
    snapshots = packet["mutableCoordinationSnapshots"]
    if len(snapshots) != 3:
        fail("expected exactly three mutable coordination snapshots")
    observed_paths = {item.get("path") for item in snapshots}
    if observed_paths != MUTABLE_COORDINATION_PATHS:
        fail("mutable coordination snapshot path set drift")
    texts: dict[str, str] = {}
    snapshot_matches = 0
    for item in snapshots:
        if set(item) != {"root", "path", "reviewedBytes", "reviewedSha256"}:
            fail(f"mutable snapshot shape invalid: {item!r}")
        if item["root"] != "coordination":
            fail(f"mutable snapshot root invalid: {item['path']}")
        if not re.fullmatch(r"[0-9a-f]{64}", item["reviewedSha256"]):
            fail(f"mutable snapshot hash invalid: {item['path']}")
        path = coordination_root / item["path"]
        if not path.is_file():
            fail(f"mutable coordination source missing: {path}")
        texts[item["path"]] = path.read_text(encoding="utf-8")
        if path.stat().st_size == item["reviewedBytes"] and sha256(path) == item["reviewedSha256"]:
            snapshot_matches += 1
    return texts, snapshot_matches


def assert_coordination_policy(packet: dict) -> None:
    policy = packet["coordinationSemanticPolicy"]
    expected = {
        "mode": "CURRENT_SEMANTICS_FAIL_CLOSED_SNAPSHOT_HASHES_AUDIT_ONLY",
        "snapshotHashesInformational": True,
        "failClosedOnMissingAmbiguousOrPermissive": True,
        "phase2MustRemainNonComplete": True,
        "executionMustRemainRestricted": True,
        "pre012MustRemainUnapproved": True,
        "zeroAdditionalCostMustRemainEnforced": True,
        "blockedSecurityMustRemainControlling": True,
    }
    if policy != expected:
        fail("mutable coordination semantic policy drift")


def _unique_line(text: str, pattern: str, label: str) -> str:
    matches = re.findall(pattern, text, flags=re.MULTILINE | re.IGNORECASE)
    if len(matches) != 1:
        fail(f"mutable coordination {label} must occur exactly once, got {len(matches)}")
    return matches[0]


def assert_coordination_semantics(texts: dict[str, str]) -> None:
    state = texts["docs/project-management/PROJECT_STATE.md"]
    decisions = texts["docs/project-management/PHASE_DECISION_REGISTER.md"]
    log = texts["docs/project-management/PHASE_EXECUTION_LOG.md"]

    phase_active = _unique_line(state, r"^\*\*Phase aktif:\*\*[^\r\n]*$", "active phase line")
    if "phase 2" not in phase_active.casefold() or "no candidate/sandbox execution" not in phase_active.casefold():
        fail("current active phase does not retain the no-execution boundary")

    phase2_row = _unique_line(state, r"^\|\s*2\s*\|[^\r\n]*$", "Phase 2 row")
    cells = [cell.strip().strip("`").upper() for cell in phase2_row.strip().strip("|").split("|")]
    if len(cells) < 5 or cells[2] not in {"NOT_STARTED", "READY", "ACTIVE", "BLOCKED", "PAUSED", "FAILED"}:
        fail("Phase 2 status is missing, ambiguous, COMPLETE, or permissive")
    phase2_folded = phase2_row.casefold()
    for token in ("sandbox/candidate launch", "restore/build", "pdf processing", "unauthorized"):
        if token not in phase2_folded:
            fail(f"Phase 2 execution restriction missing: {token}")

    authority = _unique_line(state, r"^\|\s*Execution authority\s*\|[^\r\n]*$", "execution authority row")
    authority_folded = authority.casefold()
    for token in ("static/read-only", "no authority", "sandbox", "qpdf/pdfsharp", "restore/build", "process pdf", "zero additional cost"):
        if token not in authority_folded:
            fail(f"current execution authority restriction missing: {token}")
    if re.search(r"\b(execution|launch|restore|build|processing)\s+(is\s+)?authorized\b", authority_folded):
        fail("current execution authority became permissive")

    nearest_gate = _unique_line(state, r"^\*\*Gate terdekat:\*\*[^\r\n]*$", "nearest gate line")
    if "pre-012" not in nearest_gate.casefold() or "human-only" not in nearest_gate.casefold():
        fail("current PRE-012 human-only status missing")

    for decision_id in ("P2-D01", "P2-D02", "P2-D03"):
        row = _unique_line(decisions, rf"^\|\s*{re.escape(decision_id)}\s*\|[^\r\n]*$", f"{decision_id} row")
        if "`proposed`" not in row.casefold():
            fail(f"{decision_id} is no longer explicitly unapproved/PROPOSED")
    d01 = _unique_line(decisions, r"^\|\s*P2-D01\s*\|[^\r\n]*$", "P2-D01 execution row")
    if "no provider/oracle execution or engine selection approved" not in d01.casefold():
        fail("candidate/oracle decision became permissive or ambiguous")
    d03 = _unique_line(decisions, r"^\|\s*P2-D03\s*\|[^\r\n]*$", "P2-D03 cost row")
    if "no-license-fee" not in d03.casefold() or "remain open" not in d03.casefold():
        fail("license/cost boundary became permissive or ambiguous")

    sections = re.split(r"(?m)(?=^###\s+)", log)
    run_sections = [section for section in sections if "P02-20260905-1434" in section]
    if not run_sections:
        fail("current run has no execution-log section")
    latest = run_sections[-1].casefold()
    for token in ("blocked_security", "pre-012", "human-only/unapproved", "no execution, acquisition, paid action", "not execution approval"):
        if token not in latest:
            fail(f"latest run log lost fail-closed semantic: {token}")


def assert_coordination_fixtures(packet: dict, texts: dict[str, str]) -> tuple[int, int]:
    safe = copy.deepcopy(texts)
    for path in MUTABLE_COORDINATION_PATHS:
        safe[path] += "\n<!-- harmless editorial note -->\n"
    assert_coordination_semantics(safe)

    state_path = "docs/project-management/PROJECT_STATE.md"
    decision_path = "docs/project-management/PHASE_DECISION_REGISTER.md"
    log_path = "docs/project-management/PHASE_EXECUTION_LOG.md"
    mutations: list[tuple[str, dict[str, str]]] = []

    changed = copy.deepcopy(texts)
    changed[state_path] = re.sub(r"(^\|\s*2\s*\|[^|]*\|\s*)`ACTIVE`", r"\1`COMPLETE`", changed[state_path], count=1, flags=re.MULTILINE)
    mutations.append(("phase2-complete", changed))

    changed = copy.deepcopy(texts)
    phase2_row = _unique_line(changed[state_path], r"^\|\s*2\s*\|[^\r\n]*$", "fixture Phase 2 row")
    changed[state_path] += f"\n{phase2_row}\n"
    mutations.append(("ambiguous-phase2-row", changed))

    changed = copy.deepcopy(texts)
    changed[state_path] = re.sub(r"^\|\s*Execution authority\s*\|[^\r\n]*$", "| Execution authority | Candidate execution, restore, build, and paid processing are authorized |", changed[state_path], count=1, flags=re.MULTILINE | re.IGNORECASE)
    mutations.append(("permissive-execution-authority", changed))

    changed = copy.deepcopy(texts)
    changed[state_path] = re.sub(r"^\|\s*Execution authority\s*\|[^\r\n]*\r?\n?", "", changed[state_path], count=1, flags=re.MULTILINE | re.IGNORECASE)
    mutations.append(("missing-execution-authority", changed))

    changed = copy.deepcopy(texts)
    changed[state_path] = changed[state_path].replace("zero additional cost", "paid actions allowed", 1)
    mutations.append(("zero-cost-removed", changed))

    changed = copy.deepcopy(texts)
    changed[decision_path] = changed[decision_path].replace("| P2-D01 | 2 | Approved PoC candidate list, exact versions, dan evaluation priority | `PROPOSED` |", "| P2-D01 | 2 | Approved PoC candidate list, exact versions, dan evaluation priority | `APPROVED` |", 1)
    mutations.append(("candidate-decision-approved", changed))

    changed = copy.deepcopy(texts)
    changed[decision_path] = changed[decision_path].replace("No provider/oracle execution or engine selection approved", "Provider/oracle execution and engine selection approved", 1)
    mutations.append(("candidate-execution-approved", changed))

    changed = copy.deepcopy(texts)
    changed[log_path] += "\n### 2026-09-07 23:59:59 +07:00 — P02-20260905-1434 — Permissive drift\n\n- PRE-012 approved; candidate execution and paid action authorized; BLOCKED_SECURITY cleared.\n"
    mutations.append(("latest-log-permissive", changed))

    for label, mutation in mutations:
        try:
            assert_coordination_semantics(mutation)
        except AssertionError:
            continue
        fail(f"permissive coordination fixture was not rejected: {label}")
    return len(mutations), 1


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
    assert_coordination_policy(packet)
    assert_bindings(packet, coordination_root)
    coordination_texts, snapshot_matches = read_mutable_coordination(packet, coordination_root)
    assert_coordination_semantics(coordination_texts)
    assert_negative_fixtures(packet, schema, coordination_root)
    coordination_negative_count, safe_drift_count = assert_coordination_fixtures(packet, coordination_texts)

    print("GOVERNANCE_FREEZE_STATIC_VERIFICATION=PASS")
    print(f"IMMUTABLE_BINDINGS=38;MUTABLE_COORDINATION=3;SNAPSHOT_MATCHES={snapshot_matches};SCHEMA=1;NEGATIVE_FIXTURES={10 + coordination_negative_count};SAFE_DRIFT_FIXTURES={safe_drift_count};LICENSE_NOTICE_GAPS=6;STOP_RULES=24")
    print("PRE003_011_BLOCKED=9;PRE012=HUMAN_ONLY_NOT_APPROVED;SECURITY=BLOCKED_SECURITY;UPSTREAM_REPLAY=PASS_BINDING_REMEDIATION_ONLY")
    print("EXECUTION_APPROVAL=false;SELF_APPROVAL=false;ENGINE_DISPOSITION=false;COST=ZERO_ADDITIONAL_COST")
    print("SCANNERS=0;SANDBOX_LAUNCHES=0;RESTORES=0;BUILDS=0;CANDIDATE_EXECUTIONS=0;ORACLE_EXECUTIONS=0;PDF_FILES_OPENED=0;ACQUISITIONS=0")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (AssertionError, json.JSONDecodeError, OSError) as error:
        print(f"GOVERNANCE_FREEZE_STATIC_VERIFICATION=FAIL: {error}", file=sys.stderr)
        raise SystemExit(1)
