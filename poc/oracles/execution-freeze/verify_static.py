"""Text/JSON-only freeze verifier. It must never open or process a PDF."""

import copy
import hashlib
import json
from pathlib import Path
import re
import sys

sys.dont_write_bytecode = True
POC = Path(__file__).resolve().parents[2]
REPO = POC.parent
CORPUS_DIR = POC / "corpus" / "execution-freeze"
ORACLE_DIR = POC / "oracles" / "execution-freeze"
HASH_RE = re.compile(r"^[0-9a-f]{64}$", re.ASCII)


def load_json(path):
    with path.open("r", encoding="utf-8") as stream:
        return json.load(stream)


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def require(condition, code):
    if not condition:
        raise ValueError(code)


def validate_schema(value, schema):
    supported = {"schemaId", "schemaVersion", "type", "additionalProperties", "required", "properties", "const", "pattern", "minItems"}
    require(not (set(schema) - supported), "SCHEMA_UNSUPPORTED_KEYWORD")
    kind = schema.get("type")
    checks = {"object": lambda item: type(item) is dict, "array": lambda item: type(item) is list, "string": lambda item: type(item) is str}
    require(kind in checks and checks[kind](value), "SCHEMA_TYPE")
    if "const" in schema:
        require(value == schema["const"], "SCHEMA_CONST")
    if kind == "string" and "pattern" in schema:
        require(re.fullmatch(schema["pattern"], value, flags=re.ASCII) is not None, "SCHEMA_PATTERN")
    if kind == "array" and "minItems" in schema:
        require(len(value) >= schema["minItems"], "SCHEMA_MIN_ITEMS")
    if kind == "object" and "properties" in schema:
        properties = schema["properties"]
        require(set(schema.get("required", [])) <= set(value), "SCHEMA_REQUIRED")
        if schema.get("additionalProperties") is False:
            require(not (set(value) - set(properties)), "SCHEMA_ADDITIONAL")
        for key, child in value.items():
            validate_schema(child, properties[key])


def verify_bindings(document):
    seen = set()
    for record in document["sourceBindings"]:
        require(set(record) == {"path", "sha256", "role"}, "BINDING_FIELDS")
        require(record["path"] not in seen and record["path"].startswith("poc/"), "BINDING_PATH")
        path = REPO / Path(record["path"])
        require(path.is_file() and path.suffix.lower() not in {".pdf", ".zip", ".exe", ".dll"}, "BINDING_FILE")
        require(HASH_RE.fullmatch(record["sha256"]) is not None and digest(path) == record["sha256"], "BINDING_HASH")
        seen.add(record["path"])


def verify(corpus, oracle):
    require(all(value is False for value in corpus["activationBoundary"].values()), "CORPUS_ACTIVATION")
    require(all(value is False for value in oracle["activationBoundary"].values()), "ORACLE_ACTIVATION")
    verify_bindings(corpus)
    verify_bindings(oracle)
    require(corpus["authority"]["executionAuthorized"] is False and corpus["authority"]["materializationAuthorized"] is False, "CORPUS_AUTHORITY")
    require(corpus["policy"]["corpusType"] == "SYNTHETIC_ONLY" and corpus["policy"]["productionOrPersonalDocumentsPermitted"] is False, "CORPUS_POLICY")
    for key in ("classificationDecision", "owner", "reviewer", "access", "retention", "expiry", "disposal", "provenanceAndUseAuthority"):
        require(corpus["policy"][key].startswith("TBC_"), "POLICY_PLACEHOLDER")
    require(corpus["foundation"]["materializationState"] == "NOT_PRESENT_NOT_REGENERATED_BY_THIS_TASK", "MATERIALIZATION_STATE")

    base_manifest_path = POC / "corpus" / "manifest.v1.json"
    base_expected_path = POC / "corpus" / "expected.v1.json"
    base_manifest = load_json(base_manifest_path)
    base_expected = load_json(base_expected_path)
    require(corpus["foundation"]["foundationManifestSha256"] == digest(base_manifest_path), "MANIFEST_DIGEST")
    require(corpus["foundation"]["foundationExpectedSha256"] == digest(base_expected_path), "EXPECTED_DIGEST")
    base_items = {item["id"]: item for item in base_manifest["items"]}
    proposed_items = {item["id"]: item for item in corpus["foundation"]["artifactRecords"]}
    require(set(base_items) == {"A", "B", "C"} == set(proposed_items), "ITEM_SET")
    for item_id in base_items:
        for key in ("pageCount", "byteCount", "sha256", "markers"):
            require(proposed_items[item_id][key] == base_items[item_id][key], "ITEM_DRIFT")
    base_recipes = {item["id"]: item for item in base_manifest["recipes"]}
    expected_recipes = {item["id"]: item for item in base_expected["recipes"]}
    proposed_recipes = {item["id"]: item for item in corpus["foundation"]["recipes"]}
    require(set(proposed_recipes) == {"CONTROL_AB", "ORDER_CAB", "DUPLICATE_AA", "REMOVE_B"}, "RECIPE_SET")
    for recipe_id in proposed_recipes:
        require(proposed_recipes[recipe_id]["inputs"] == base_recipes[recipe_id]["inputs"], "RECIPE_INPUT_DRIFT")
        require(proposed_recipes[recipe_id]["expectedPageCount"] == expected_recipes[recipe_id]["pageCount"], "RECIPE_COUNT_DRIFT")
    require(all(item["status"].startswith("BLOCKED_") and item["numericTargetsInvented"] is False for item in corpus["coverageGaps"]), "CORPUS_GAPS")

    require(oracle["scope"]["corpusManifestId"] == corpus["manifestId"], "CROSS_BINDING")
    plan = oracle["independentOraclePlan"]
    require(plan["structural"]["assetSha256"] == "3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6", "QPDF_HASH")
    require(plan["structural"]["executionStatus"] == "NOT_RUN_NOT_AUTHORIZED", "QPDF_STATUS")
    require(plan["render"]["candidateId"].startswith("TBC_") and plan["render"]["numericTolerance"] == "TBC_NO_TOLERANCE_APPROVED", "RENDER_GAP")
    require(plan["content"]["candidateId"].startswith("TBC_"), "CONTENT_GAP")
    require(plan["foundationComparator"]["publishAuthorized"] is False, "COMPARATOR_AUTHORITY")
    measurement = oracle["measurementPlanProposal"]
    require(measurement["status"] == "BLOCKED_PENDING_HUMAN_RUN_POLICY", "MEASUREMENT_STATUS")
    require(measurement["numericAcceptanceTargets"].startswith("TBC_") and measurement["repetitionCount"].startswith("TBC_"), "MEASUREMENT_PLACEHOLDER")
    require(measurement["statusBeforeNumericTargetsExist"] == "MEASURED_BASELINE_AND_BLOCKED_PENDING_TARGET_APPROVAL", "MEASUREMENT_RESULT_CLASS")
    evidence = oracle["evidencePolicyProposal"]
    require(evidence["status"] == "BLOCKED_PENDING_RUN_SPECIFIC_POLICY", "EVIDENCE_STATUS")
    for key in ("access", "retention", "expiry", "disposal", "sanitizationReviewer", "immutabilityMechanism", "sbomFormat"):
        require(evidence[key].startswith("TBC_"), "EVIDENCE_PLACEHOLDER")
    experiments = {item["id"]: item for item in oracle["experimentSpecifications"]}
    require(set(experiments) == {f"PDF001-E{number:02d}" for number in range(1, 11)}, "EXPERIMENT_SET")
    require(all(item["status"] == "NOT_RUN" or item["status"].startswith("BLOCKED_") for item in experiments.values()), "EXPERIMENT_STATUS")
    for item in experiments.values():
        for trace_id in item["traceIds"]:
            require(re.fullmatch(r"(UT-MERGE|TP-MRG)-[0-9]{3}", trace_id, flags=re.ASCII) is not None, "TRACE_ID")
    gates = {item["id"]: item for item in oracle["gateDispositions"]}
    require(set(gates) == {"PRE-007", "PRE-008", "PRE-010", "PRE-011"} and all(item["status"] == "BLOCKED" for item in gates.values()), "GATE_STATUS")


def main():
    corpus_schema = load_json(CORPUS_DIR / "corpus-freeze.schema.v1.json")
    corpus = load_json(CORPUS_DIR / "corpus-freeze.proposal.v1.json")
    oracle_schema = load_json(ORACLE_DIR / "oracle-freeze.schema.v1.json")
    oracle = load_json(ORACLE_DIR / "oracle-freeze.proposal.v1.json")
    validate_schema(corpus, corpus_schema)
    validate_schema(oracle, oracle_schema)
    verify(corpus, oracle)
    mutations = [
        lambda c, _: c["activationBoundary"].__setitem__("pdfGeneration", True),
        lambda c, _: c["policy"].__setitem__("retention", "30_DAYS"),
        lambda c, _: c["foundation"]["artifactRecords"][0].__setitem__("sha256", "0" * 64),
        lambda _, o: o["independentOraclePlan"]["structural"].__setitem__("executionStatus", "PASS"),
        lambda _, o: o["independentOraclePlan"]["render"].__setitem__("numericTolerance", "0.01"),
        lambda _, o: o["gateDispositions"][1].__setitem__("status", "PASS"),
        lambda c, _: c.__setitem__("unexpected", "field"),
    ]
    for mutation in mutations:
        changed_corpus, changed_oracle = copy.deepcopy(corpus), copy.deepcopy(oracle)
        mutation(changed_corpus, changed_oracle)
        try:
            validate_schema(changed_corpus, corpus_schema)
            validate_schema(changed_oracle, oracle_schema)
            verify(changed_corpus, changed_oracle)
        except ValueError:
            continue
        raise ValueError("NEGATIVE_FIXTURE_ACCEPTED")
    print("STATIC_FREEZE_VERIFICATION=PASS")
    print("SCHEMAS=2;SOURCE_BINDINGS=14;NEGATIVE_FIXTURES=7;EXPERIMENTS=10;BLOCKED_GATES=4")
    print("PDF_FILES_OPENED=0;PDF_GENERATED=0;PARSER_RENDERER_ENGINE_EXECUTIONS=0")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
