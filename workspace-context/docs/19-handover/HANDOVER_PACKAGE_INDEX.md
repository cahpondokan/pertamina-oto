# HANDOVER PACKAGE INDEX
## PERTAMINA OTO — Document 19

> Versi: FINAL v1.0
> Secret Included: NO untuk seluruh item
> Actual package transfer: NOT_STARTED

Setiap item mengikuti schema: HandoverItemId, Category, Title, Description, Source Document, Artifact/Resource, Current Version, Location, Owner, Recipient, Reviewer, Required Permission, Sensitive, Secret Included, Evidence, Dependency, Risk if Missing, Status, Gap, Target, Acceptance Criteria, Notes, Last Updated.

## Documentation assets

| ID | Title/version | Location / source | Owner→Recipient / Permission / Sensitive / Secret | Evidence / Dependency / Risk | Status / Gap / Acceptance |
|---|---|---|---|---|---|
| HO-DOC-001 | Documents 01–05 FINAL | root canonical files | TBD→TBD; docs read; Internal; NO | Hash/version manifest; Documentation owner | AVAILABLE; owner/reviewer pending; accepted after canonical review |
| HO-DOC-002 | Documents 06–10 FINAL | docs/06-api..10 | TBD→TBD; docs read; Internal; NO | Hash/version manifest | AVAILABLE; canonical path review required |
| HO-DOC-003 | Documents 11–15 FINAL | docs/11..15 | TBD→TBD; docs read; Internal; NO | Hash/version; RC evidence dependency | AVAILABLE pre-implementation; not execution/release evidence |
| HO-DOC-004 | Documents 16–19 FINAL | docs/16..19 | TBD→TBD; docs read; Internal; NO | Hash/version/audit | AVAILABLE pre-implementation; actual ops/handover pending |
| HO-DOC-005 | Master Project Summary v4.2 | MASTER_PROJECT_SUMMARY.md | Documentation owner TBD→all receivers; Internal; NO | Current index/version | AVAILABLE; owner review pending |

## Technical and release assets

| ID | Title | Source/resource/location | Owner→Recipient / Sensitive / Secret | Evidence required | Status / Gap / Acceptance |
|---|---|---|---|---|---|
| HO-CODE-001 | Official source repository | Azure DevOps URL TBD | Repo owner TBD→Maintainer; Internal; NO | access, branch policy, commit/tag, clean clone | BLOCKED; repository absent |
| HO-CODE-002 | .NET solution/projects | Official repo TBD | Technical owner→Maintainer; Internal; NO | restore/build/test/package PASS | BLOCKED; files absent |
| HO-CODE-003 | Build instructions/toolchain | Repo/pipeline TBD | DevOps owner→Maintainer; Internal; NO | pinned SDK/feed/dependency + independent run | BLOCKED |
| HO-ART-001 | Web/API/Worker artifacts | Artifact store TBD | Release owner→Operations; Restricted; NO | build/commit/hash/SBOM/manifest | NOT_STARTED |
| HO-ART-002 | Desktop/Local Worker cohort | Artifact store TBD | Release owner→Client operations; Restricted; NO | package/signing/compatibility/install/rollback | NOT_STARTED |
| HO-ART-003 | Mobile package | Artifact store TBD | Mobile owner→Distribution; Restricted; NO | signing/channel/device/update evidence | NOT_STARTED |
| HO-ART-004 | DB migration bundle | Artifact store TBD | DB/module owner→Deployment; Restricted; NO | migration/backup/apply/verify/recover | NOT_STARTED |
| HO-ART-005 | OpenAPI/contracts | Artifact store TBD | API owner→Consumers; Internal; NO | generated version/hash/diff/contract tests | NOT_STARTED |
| HO-ART-006 | SBOM/license/security evidence | Evidence store TBD | Security/license owner→Reviewers; Restricted; NO | exact artifact/dependency/tool/version/decision | NOT_STARTED |
| HO-ART-007 | Test/UAT evidence | Evidence store TBD | QA/UAT owner→Approvers; Restricted; NO | exact RC/environment/data/result/sign-off | NOT_STARTED |
| HO-ART-008 | Deployment/rollback package | Artifact/evidence store TBD | Release owner→Operations; Restricted; NO | same artifact/config/schema/attempt evidence | NOT_STARTED |

## Environment, data, operations, and acceptance assets

| ID | Title | Resource | Owner→Recipient / Sensitive / Secret | Evidence required | Status / Gap / Acceptance |
|---|---|---|---|---|---|
| HO-ENV-001 | Environment inventory | Local/Dev/QA/Pilot/Prod | Env owner→Operations; Restricted; NO | asset/topology/data/config/owner/access | REQUIRES_CONFIRMATION |
| HO-ACCESS-001 | Access package | IAM/CMDB refs | Security owner→Receivers; Restricted; NO | grant/scope/expiry/test/revoke | REQUIRES_CONFIRMATION |
| HO-CONFIG-001 | Configuration catalog | Repo/config system TBD | Config owner→Operations; Restricted; NO | schema/version/provenance/rollback | NOT_STARTED |
| HO-CONFIG-002 | Secret-reference inventory | Custody refs only | Custodian→Workloads; Highly Restricted; NO | ref/owner/purpose/rotation/access procedure | REQUIRES_CONFIRMATION |
| HO-DATA-001 | SQL/SQLite/data dictionary | DB repo/evidence TBD | Data/DB owner→DBA/Maintainer; Restricted; NO | schema/migration/permission/restore | NOT_STARTED |
| HO-DATA-002 | Storage/backup/restore package | Storage/recovery refs TBD | Storage/backup owner→Operations; Restricted; NO | inventory/digest/access/restore reconciliation | NOT_STARTED |
| HO-SEC-001 | Security acceptance package | Restricted evidence store TBD | Security owner→Approvers; Restricted; NO | tests/scan/VA-PT/findings/retest/decision | NOT_STARTED |
| HO-OPS-001 | Monitoring/alert/runbook package | Observability/KB TBD | Ops owner→Support/Ops; Restricted; NO | dashboards/access/alerts/owners/drills | NOT_STARTED |
| HO-SUP-001 | Support/Incident/KB package | Service Desk/KB TBD | Support owner→L1/L2/L3; Restricted; NO | workflow/access/sample/tabletop/acceptance | REQUIRES_CONFIRMATION |
| HO-KT-001 | KT package | Approved training/evidence store TBD | Giver→Receiver; Internal/Restricted; NO | 16 sessions + teach-back + acceptance | PLANNED |
| HO-RISK-001 | Risks/issues/debt/PoC package | Open-item register | Owners TBD→Receivers; Restricted as needed; NO | owner/impact/control/target/acceptance | AVAILABLE as baseline; ownership pending |
| HO-OWN-001 | Ownership/responsibility package | Registers | Giver→Receiver; Internal; NO | official assignment/access/acknowledgement | REQUIRES_CONFIRMATION |
| HO-ACC-001 | Acceptance/sign-off package | Signoff register | Giver/Receiver/Approver TBD; Restricted; NO | gate evidence/decision/date/conditions | PENDING |
| HO-CLOSE-001 | Closure/archive package | Archive location TBD | Closure authority→Records owner; Restricted; NO | G0–G10, archive, observation, approval | PENDING |

## Package rule

Satu item baru dapat ACCEPTED jika exact resource accessible, current, reviewed, evidence-linked, secret-free, owner/recipient acknowledged, dependencies met, gaps/risks accepted by proper authority, and acceptance criteria pass.

