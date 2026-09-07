"""Static JSON/text verifier. It never opens a document or loads an engine."""

import copy
import json
from pathlib import Path
import re
import sys

sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parent


def require(condition, code):
    if not condition:
        raise ValueError(code)


def load_json(name):
    with (ROOT / name).open("r", encoding="utf-8") as stream:
        return json.load(stream)


def validate_schema(value, schema):
    supported = {
        "schemaId", "schemaVersion", "type", "additionalProperties",
        "required", "properties", "const", "pattern", "minItems"
    }
    require(not (set(schema) - supported), "SCHEMA_UNSUPPORTED_KEYWORD")
    expected_type = schema.get("type")
    checks = {
        "object": lambda item: type(item) is dict,
        "array": lambda item: type(item) is list,
        "string": lambda item: type(item) is str,
    }
    require(expected_type in checks and checks[expected_type](value), "SCHEMA_TYPE")
    if "const" in schema:
        require(value == schema["const"], "SCHEMA_CONST")
    if expected_type == "string" and "pattern" in schema:
        require(re.fullmatch(schema["pattern"], value, flags=re.ASCII) is not None, "SCHEMA_PATTERN")
    if expected_type == "array" and "minItems" in schema:
        require(len(value) >= schema["minItems"], "SCHEMA_MIN_ITEMS")
    if expected_type == "object" and "properties" in schema:
        properties = schema["properties"]
        require(set(schema.get("required", [])) <= set(value), "SCHEMA_REQUIRED")
        if schema.get("additionalProperties") is False:
            require(not (set(value) - set(properties)), "SCHEMA_ADDITIONAL")
        for key, child in value.items():
            validate_schema(child, properties[key])


EXPECTED_FEATURES = {
    "POC-CONV-001": (["FEAT-CONV-001", "FEAT-CONV-002", "FEAT-CONV-003"], []),
    "POC-CONV-002": (["FEAT-CONV-004", "FEAT-CONV-005", "FEAT-CONV-006"], []),
    "POC-OCR-001": (["FEAT-OCR-001", "FEAT-OCR-002", "FEAT-OCR-003", "FEAT-OCR-004", "FEAT-OCR-005", "FEAT-OCR-009", "FEAT-OCR-010", "FEAT-OCR-011"], []),
    "POC-OCR-002": (["FEAT-OCR-006", "FEAT-OCR-012", "FEAT-OCR-013"], ["FEAT-CONV-005", "FEAT-CONV-021", "FEAT-CONV-022", "FEAT-CONV-025", "FEAT-CONV-036"]),
    "POC-CONV-003": (["FEAT-CONV-017", "FEAT-CONV-018"], []),
    "POC-PDF-003": (["FEAT-PDF-075"], []),
    "POC-CONV-004": (["FEAT-CONV-036"], ["FEAT-OCR-005", "FEAT-OCR-006", "FEAT-OCR-012", "FEAT-OCR-013"]),
}

REQUIRED_CONTROLS = {
    "POC-CONV-001": {"OFFICE_MACRO_VBA_OLE_DDE_EXECUTION_DENY", "OFFICE_EXTERNAL_LINK_TEMPLATE_NETWORK_FETCH_DENY"},
    "POC-CONV-002": {"HIDDEN_UPLOAD_DENY", "SPREADSHEET_FORMULA_INJECTION_NEUTRALIZATION", "GENERATED_OFFICE_MACRO_AND_EXTERNAL_LINK_DENY"},
    "POC-OCR-001": {"PUBLIC_AI_AND_HIDDEN_UPLOAD_DENY", "OCR_CONTENT_EXCLUDED_FROM_ORDINARY_LOGS_TELEMETRY_INDEX"},
    "POC-OCR-002": {"SPREADSHEET_FORMULA_INJECTION_NEUTRALIZATION", "GENERATED_ACTIVE_LINK_MACRO_EXTERNAL_FETCH_DENY"},
    "POC-CONV-003": {"HTML_JAVASCRIPT_AND_ACTIVE_CONTENT_DENY", "HTML_NETWORK_EGRESS_DENY", "HTML_SSRF_AND_ARBITRARY_REMOTE_URL_DENY"},
    "POC-PDF-003": {"INDEPENDENT_PDFA_CONFORMANCE_VALIDATOR_REQUIRED", "CONVERTER_AND_VALIDATOR_IDENTITY_INDEPENDENCE_REQUIRED"},
    "POC-CONV-004": {"DETERMINISTIC_TEXT_LAYOUT_EXTRACTION_FIRST", "OCR_FALLBACK_SEPARATELY_GOVERNED_AND_ONLY_WHEN_NEEDED", "FORMULA_LINK_AND_CONTENT_SANITIZATION"},
}


def verify(packet):
    boundary = packet["authorityBoundary"]
    require(boundary["documentationOnly"] is True and boundary["zeroAdditionalCost"] is True, "BOUNDARY_MODE")
    denied = [key for key in boundary if key not in {"documentationOnly", "zeroAdditionalCost"}]
    require(denied and all(boundary[key] is False for key in denied), "AUTHORITY_NOT_DENIED")
    isolation = packet["approvalIsolation"]
    require(all(isolation[key] is False for key in (
        "pocPdf001ApprovalMayBeInherited",
        "pocPdf001ResultMayProveThesePocs",
        "pocPdf001CandidateMayBeAssumedCompatible",
        "pocPdf001CorpusOracleMayBeReusedWithoutSeparateReview",
    )), "PDF001_APPROVAL_LEAK")
    require(packet["plannedDecisionStatuses"] == [
        "APPROVED_ENGINE", "APPROVED_WITH_LIMITATION",
        "BLOCKED_ALTERNATIVE_REQUIRED", "DEFERRED_BY_GOVERNED_DECISION"
    ], "DECISION_ENUM")
    gates = {item["gate"]: item for item in packet["commonDecisionGates"]}
    require(set(gates) == {"P2-D01", "P2-D02", "P2-D03", "PER_POC_SECURITY_REVIEW", "PER_POC_ENVIRONMENT_MEASUREMENT_EVIDENCE_FREEZE", "PER_POC_HUMAN_EXECUTION_AUTHORIZATION"}, "GATE_SET")
    require(gates["P2-D01"]["currentStatus"] == "PROPOSED", "P2_D01_STATUS")
    require(gates["P2-D02"]["currentStatus"] == "PROPOSED", "P2_D02_STATUS")
    require(gates["P2-D03"]["currentStatus"] == "PROPOSED", "P2_D03_STATUS")
    require(gates["PER_POC_HUMAN_EXECUTION_AUTHORIZATION"]["currentStatus"] == "NOT_AUTHORIZED", "EXECUTION_GATE")

    records = {item["pocId"]: item for item in packet["pocRecords"]}
    require(len(records) == len(packet["pocRecords"]) == 7 and set(records) == set(EXPECTED_FEATURES), "POC_SET")
    for poc_id, record in records.items():
        expected_direct, expected_dependencies = EXPECTED_FEATURES[poc_id]
        require(record["featureMapping"]["directFeatureIds"] == expected_direct, f"{poc_id}_DIRECT_FEATURES")
        require(record["featureMapping"]["dependentFeatureIds"] == expected_dependencies, f"{poc_id}_DEPENDENT_FEATURES")
        require(record["featureMapping"]["explicitlyNotProven"], f"{poc_id}_NOT_PROVEN")
        state = record["currentState"]
        require(state["lifecycleStatus"] == "NOT_STARTED", f"{poc_id}_LIFECYCLE")
        require(state["scopeApproval"] == "NOT_APPROVED", f"{poc_id}_SCOPE")
        require(state["executionAuthorization"] == "NOT_AUTHORIZED", f"{poc_id}_EXECUTION")
        require(state["governedDisposition"] == "NO_GOVERNED_DISPOSITION", f"{poc_id}_DISPOSITION")
        require(state["candidate"] == "TBD_APPROVED_CANDIDATE" and state["vendorRecommendation"] == "NONE", f"{poc_id}_VENDOR")
        require(REQUIRED_CONTROLS[poc_id] <= set(record["mandatoryControls"]), f"{poc_id}_CONTROLS")
        for field in ("canonicalRequirementIds", "oracleNeeds", "resourceNeeds", "failureNeeds", "prerequisites", "decisionOwners", "reopenConditions"):
            require(type(record[field]) is list and record[field], f"{poc_id}_{field.upper()}")
        require(len(record["canonicalRequirementIds"]) == len(set(record["canonicalRequirementIds"])), f"{poc_id}_DUPLICATE_REQUIREMENT")
        require(len(record["featureMapping"]["directFeatureIds"]) == len(set(record["featureMapping"]["directFeatureIds"])), f"{poc_id}_DUPLICATE_FEATURE")

    html = records["POC-CONV-003"]["scopeBoundary"]
    require(html["validatedLocalOrCapturedHtmlArtifactOnly"] is True, "HTML_LOCAL_SCOPE")
    require(all(html[key] is False for key in html if key != "validatedLocalOrCapturedHtmlArtifactOnly"), "HTML_ACTIVE_OR_NETWORK_ALLOWED")
    pdfa = records["POC-PDF-003"]["independentValidatorRequirement"]
    require(pdfa["required"] is True and pdfa["converterMaySelfValidateAsSoleOracle"] is False, "PDFA_INDEPENDENCE")
    require(pdfa["executionStatus"] == "NOT_RUN_NOT_AUTHORIZED", "PDFA_EXECUTION_CLAIM")


def main():
    schema = load_json("conversion-ocr-disposition.schema.v1.json")
    packet = load_json("conversion-ocr-disposition.proposal.v1.json")
    validate_schema(packet, schema)
    verify(packet)
    mutations = [
        lambda item: item["authorityBoundary"].__setitem__("candidateExecutionAuthorized", True),
        lambda item: item["approvalIsolation"].__setitem__("pocPdf001ApprovalMayBeInherited", True),
        lambda item: item["pocRecords"][0]["currentState"].__setitem__("executionAuthorization", "AUTHORIZED"),
        lambda item: item["pocRecords"][0]["currentState"].__setitem__("vendorRecommendation", "SOME_VENDOR"),
        lambda item: item["pocRecords"][4]["scopeBoundary"].__setitem__("networkAccessAllowedDuringRendering", True),
        lambda item: item["pocRecords"][5]["independentValidatorRequirement"].__setitem__("converterMaySelfValidateAsSoleOracle", True),
        lambda item: item["pocRecords"].pop(),
        lambda item: item.__setitem__("unexpected", "field"),
    ]
    for mutation in mutations:
        changed = copy.deepcopy(packet)
        mutation(changed)
        try:
            validate_schema(changed, schema)
            verify(changed)
        except (KeyError, ValueError):
            continue
        raise ValueError("NEGATIVE_FIXTURE_ACCEPTED")
    print("STATIC_CONVERSION_OCR_DISPOSITION_VERIFICATION=PASS")
    print("POCS=7;SCHEMA=1;NEGATIVE_FIXTURES=8;EXECUTION_AUTHORIZATIONS=0;VENDOR_RECOMMENDATIONS=0")
    print("DOCUMENTS_OPENED=0;PDF_PROCESSING=0;ENGINE_LOADS=0;NETWORK_ACQUISITIONS=0")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
