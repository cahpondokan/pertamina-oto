#!/usr/bin/env python3
"""Static verifier for the Wave 0G-B security execution-freeze packet.

This script reads and hashes files only. It does not launch a candidate, scanner,
sandbox, restore, build, engine, or PDF corpus operation.
"""

from __future__ import annotations

import argparse
import copy
import hashlib
import json
import subprocess
import sys
from pathlib import Path


HERE = Path(__file__).resolve().parent
REPOSITORY_ROOT = HERE.parents[3]
PACKET_PATH = HERE / "security-review.v1.json"


def fail(message: str) -> None:
    raise AssertionError(message)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def assert_core(packet: dict) -> None:
    metadata = packet.get("metadata", {})
    if metadata.get("taskId") != "P2-W0G-B-A6-SECURITY-EXECUTION-FREEZE-REVIEW":
        fail("task identity drift")
    if metadata.get("runId") != "P02-20260905-1434":
        fail("run identity drift")
    if metadata.get("baselineCommit") != "f246fd03b26a39afef958174cd12105a78d0a259":
        fail("baseline binding drift")
    if metadata.get("disposition") != "BLOCKED_SECURITY":
        fail("only BLOCKED_SECURITY is supported by this packet")
    if metadata.get("executionApproval") is not False or metadata.get("selfApproval") is not False:
        fail("packet must not grant execution or self approval")
    if metadata.get("additionalCost") != "ZERO_ADDITIONAL_COST":
        fail("cost boundary drift")

    allowlist = packet.get("effectiveAllowlist", {})
    expected_empty = (
        "candidateFiles", "restoreCommands", "buildCommands", "launchCommands",
        "runtimeProcesses", "runtimeModules", "networkDestinations",
    )
    for key in expected_empty:
        if allowlist.get(key) != []:
            fail(f"effective allowlist must remain empty: {key}")

    future = packet.get("futureEligibilityProfiles", [])
    expected_profiles = {
        "FE-ENV-OBSERVE", "FE-PDFSHARP-BOOTSTRAP-RESTORE",
        "FE-PDFSHARP-LOCKED-RESTORE", "FE-PDFSHARP-BUILD",
        "FE-PDFSHARP-RUN", "FE-QPDF-ORACLE",
    }
    if {item.get("id") for item in future} != expected_profiles:
        fail("future eligibility profile set drift")
    if any(item.get("status") != "BLOCKED_NOT_AUTHORIZED" for item in future):
        fail("a future profile became authorized")

    expected_candidates = {
        ("PDFsharp", "6.2.4", 4625326, "5647882339b327cbf6da9ea3f882efb335f8d602e6fd45323adf9e78da53cf67"),
        ("Microsoft.Extensions.Logging.Abstractions", "8.0.3", 802188, "e4c498d5a13051b4577a148f1d8c3470167215c507e2392069b75dc61322bb74"),
        ("System.Security.Cryptography.Pkcs", "8.0.1", 1078389, "28c348909df243fe4ee962213e3c8021aaecbc8321929dba03a69bcb92a49de5"),
        ("Microsoft.Extensions.DependencyInjection.Abstractions", "8.0.2", 278862, "51f2df1100245f10da54f0bb7e813f277155117777d4fbbab902214e27372606"),
        ("qpdf-msvc64", "12.4.1", 28165367, "3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6"),
    }
    observed_candidates = {
        (item.get("id"), item.get("version"), item.get("bytes"), item.get("sha256"))
        for item in packet.get("frozenCandidateIdentities", [])
    }
    if observed_candidates != expected_candidates:
        fail("frozen candidate identity drift")

    coverage = packet.get("peerPortfolioCoverage", [])
    expected_peer_pocs = {
        "df5430a14b432ed5ad18b0799512f4d1bd5d8dda": {
            "POC-CONV-001", "POC-CONV-002", "POC-OCR-001", "POC-OCR-002",
            "POC-CONV-003", "POC-PDF-003", "POC-CONV-004",
        },
        "ee7f86735131302482af7524baf5422e081b73c9": {
            "POC-SCAN-001", "POC-INTEL-001", "POC-INTEL-002", "POC-CONV-004",
            "POC-AI-LOCAL-001",
        },
    }
    if {item.get("commit") for item in coverage} != set(expected_peer_pocs):
        fail("peer portfolio commit set drift")
    for item in coverage:
        if item.get("reviewResult") != "COVERED_STATIC_ONLY_REMAINS_BLOCKED":
            fail("peer portfolio security disposition drift")
        if set(item.get("pocIds", [])) != expected_peer_pocs[item["commit"]]:
            fail("peer portfolio PoC coverage drift")

    pre = packet.get("preGateStatus", {})
    for number in range(3, 12):
        if pre.get(f"PRE-{number:03d}") != "BLOCKED":
            fail(f"PRE-{number:03d} must remain blocked")
    if pre.get("PRE-012") != "HUMAN_ONLY_NOT_APPROVED":
        fail("PRE-012 must remain human-only and unapproved")

    gaps = packet.get("severityRankedGaps", [])
    high = [gap for gap in gaps if gap.get("severity") == "HIGH"]
    if len(high) < 9:
        fail("fewer than nine high-severity gaps")
    if {gap.get("id") for gap in gaps} != {f"SEC-G-{number:03d}" for number in range(1, 12)}:
        fail("severity-ranked gap set drift")

    references = {item.get("id") for item in packet.get("officialReferenceSnapshot", [])}
    required_refs = {"OFF-PDFSHARP-361", "OFF-QPDF-1739", "OFF-QPDF-1688"}
    if not required_refs.issubset(references):
        fail("required official issue references missing")

    plans = packet.get("evidencePlans", {})
    for key in ("processModuleDllSearch", "networkEgress", "tempResourceCancelCleanupCanary"):
        if not isinstance(plans.get(key), list) or len(plans[key]) < 5:
            fail(f"evidence plan incomplete: {key}")
    if len(packet.get("failClosedStopRules", [])) < 16:
        fail("fail-closed stop-rule set incomplete")

    boundary = packet.get("verificationBoundary", {})
    if boundary.get("staticReadOnlyOnly") is not True:
        fail("static verification boundary missing")
    prohibited = (
        "scannerExecuted", "sandboxLaunched", "candidateExecuted", "engineExecuted",
        "restoreExecuted", "buildExecuted", "pdfCorpusOpened",
        "downloadOrInstallPerformed", "systemChanged",
    )
    for key in prohibited:
        if boundary.get(key) is not False:
            fail(f"prohibited-action boundary drift: {key}")


def assert_bindings(packet: dict, coordination_root: Path) -> None:
    bindings = packet.get("sourceBindings", [])
    if len(bindings) != 30:
        fail(f"expected 30 source bindings, got {len(bindings)}")
    seen: set[tuple[str, str]] = set()
    roots = {"repository": REPOSITORY_ROOT, "coordination": coordination_root}
    for binding in bindings:
        root_name = binding.get("root")
        relative = binding.get("path")
        if root_name not in roots or not isinstance(relative, str):
            fail(f"invalid source binding: {binding!r}")
        key = (root_name, relative)
        if key in seen:
            fail(f"duplicate source binding: {root_name}:{relative}")
        seen.add(key)
        path = roots[root_name] / Path(relative)
        if not path.is_file():
            fail(f"bound source missing: {path}")
        size = path.stat().st_size
        if size != binding.get("bytes"):
            fail(f"bound source byte-length drift: {path}: {size}")
        digest = sha256(path)
        if digest != binding.get("sha256"):
            fail(f"bound source hash drift: {path}: {digest}")


def assert_peer_git_bindings(packet: dict) -> None:
    bindings = packet.get("peerPortfolioBindings", [])
    if len(bindings) != 8:
        fail(f"expected 8 peer Git-blob bindings, got {len(bindings)}")
    seen: set[tuple[str, str]] = set()
    for binding in bindings:
        commit = binding.get("commit")
        relative = binding.get("path")
        key = (commit, relative)
        if key in seen:
            fail(f"duplicate peer binding: {key}")
        seen.add(key)
        try:
            resolved = subprocess.run(
                ["git", "rev-parse", f"{commit}:{relative}"],
                cwd=REPOSITORY_ROOT,
                check=True,
                capture_output=True,
                text=True,
            ).stdout.strip()
            content = subprocess.run(
                ["git", "cat-file", "blob", resolved],
                cwd=REPOSITORY_ROOT,
                check=True,
                capture_output=True,
            ).stdout
        except subprocess.CalledProcessError as error:
            fail(f"peer Git object unavailable: {commit}:{relative}: {error}")
        if resolved != binding.get("gitBlobOid"):
            fail(f"peer Git blob drift: {commit}:{relative}: {resolved}")
        if len(content) != binding.get("bytes"):
            fail(f"peer Git blob length drift: {commit}:{relative}: {len(content)}")
        digest = hashlib.sha256(content).hexdigest()
        if digest != binding.get("sha256"):
            fail(f"peer Git blob hash drift: {commit}:{relative}: {digest}")


def assert_negative_fixtures(packet: dict) -> None:
    mutations = []

    changed = copy.deepcopy(packet)
    changed["metadata"]["disposition"] = "FREEZE_READY_FOR_GOVERNANCE_REVIEW"
    mutations.append(changed)

    changed = copy.deepcopy(packet)
    changed["metadata"]["executionApproval"] = True
    mutations.append(changed)

    changed = copy.deepcopy(packet)
    changed["effectiveAllowlist"]["restoreCommands"] = ["dotnet restore"]
    mutations.append(changed)

    changed = copy.deepcopy(packet)
    changed["preGateStatus"]["PRE-003"] = "PASS"
    mutations.append(changed)

    changed = copy.deepcopy(packet)
    changed["verificationBoundary"]["engineExecuted"] = True
    mutations.append(changed)

    changed = copy.deepcopy(packet)
    changed["peerPortfolioCoverage"][0]["reviewResult"] = "APPROVED_SECURITY"
    mutations.append(changed)

    for index, mutation in enumerate(mutations, start=1):
        try:
            assert_core(mutation)
        except AssertionError:
            continue
        fail(f"negative fixture {index} was not rejected")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--coordination-root", required=True, type=Path)
    args = parser.parse_args()
    coordination_root = args.coordination_root.resolve()
    if not coordination_root.is_dir():
        fail(f"coordination root does not exist: {coordination_root}")

    packet = json.loads(PACKET_PATH.read_text(encoding="utf-8"))
    assert_core(packet)
    assert_bindings(packet, coordination_root)
    assert_peer_git_bindings(packet)
    assert_negative_fixtures(packet)

    high_count = sum(
        gap.get("severity") == "HIGH" for gap in packet["severityRankedGaps"]
    )
    print("SECURITY_FREEZE_STATIC_VERIFICATION=PASS")
    print(
        f"SOURCE_BINDINGS={len(packet['sourceBindings'])};PEER_BLOBS={len(packet['peerPortfolioBindings'])};"
        f"NEGATIVE_FIXTURES=6;"
        f"HIGH_GAPS={high_count};PRE003_011_BLOCKED=9"
    )
    print(
        "DISPOSITION=BLOCKED_SECURITY;EXECUTION_APPROVAL=false;"
        "SELF_APPROVAL=false;COST=ZERO_ADDITIONAL_COST"
    )
    print(
        "SCANNERS=0;SANDBOX_LAUNCHES=0;RESTORES=0;BUILDS=0;"
        "CANDIDATE_EXECUTIONS=0;PDF_FILES_OPENED=0"
    )
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (AssertionError, json.JSONDecodeError, OSError) as error:
        print(f"SECURITY_FREEZE_STATIC_VERIFICATION=FAIL: {error}", file=sys.stderr)
        raise SystemExit(1)
