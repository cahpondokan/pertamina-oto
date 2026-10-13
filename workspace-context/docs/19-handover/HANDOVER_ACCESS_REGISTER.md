# HANDOVER ACCESS REGISTER
## PERTAMINA OTO — Document 19

> Versi: FINAL v1.0
> Credential/secret value: NONE
> Actual grants: NOT_EVIDENCED

| AccessId | System / Environment | Role/permission scope | Current→Target Owner | Approval / Grant / Revoke | MFA/Reauth | Status / Evidence |
|---|---|---|---|---|---|---|
| HO-ACCESS-001 | Azure DevOps Repository / all | read/contribute/PR/policy admin separated | TBD→Repository Owner TBD | approved request; least scope; revoke role/task end | Enterprise policy | REQUIRES_CONFIRMATION |
| HO-ACCESS-002 | Azure Pipeline / per env | view/run/edit/approve/service connection separated | TBD→Pipeline Owner TBD | environment-scoped grant/revoke | Privileged reauth | REQUIRES_CONFIRMATION |
| HO-ACCESS-003 | Artifact/Feed / build-release | publish/read/promote/retain/delete separated | TBD→Artifact/Feed Owner | controlled publication and revocation | Policy | REQUIRES_CONFIRMATION |
| HO-ACCESS-004 | Shared Dev | Dev deploy/observe only | TBD→Dev Env Owner | task-bound, no Production inheritance | Policy | NOT_PROVISIONED |
| HO-ACCESS-005 | QA | deploy/test/observe; destructive test scoped | TBD→QA Owner | campaign/window/data scoped | High-risk as required | NOT_PROVISIONED |
| HO-ACCESS-006 | Pilot | approved rollout/observe/support scope | TBD→Pilot Owner | cohort/window scoped | Required by policy | REQUIRES_CONFIRMATION |
| HO-ACCESS-007 | Production | deploy/operate/approve separated; no developer implicit access | TBD→Production Authority | formal time-bound/JIT process if approved | REQUIRED | REQUIRES_PERTAMINA_PRODUCTION_ACCEPTANCE |
| HO-ACCESS-008 | SQL Server / per env | runtime/migration/backup/admin/read separated | TBD→DB Owner/DBA | purpose DB role; no client access | Privileged required | REQUIRES_CONFIRMATION |
| HO-ACCESS-009 | Secure Storage / per env/zone | exact workload object lifecycle; no browsing | TBD→Storage Owner | purpose/object/zone scope | Privileged policy | REQUIRES_CONFIRMATION |
| HO-ACCESS-010 | Secret/Certificate Store | resolve/manage/rotate/revoke separated | TBD→Security Custodian | approved identity/purpose; no value in ticket | REQUIRED | REQUIRES_CONFIRMATION |
| HO-ACCESS-011 | Monitoring/APM | safe dashboard/read/config separated | TBD→Observability Owner | environment/safe-field scoped | Policy | NOT_PROVISIONED |
| HO-ACCESS-012 | Audit Console | audit list/read/export in AUDIT_SCOPE | TBD→Audit Owner/Auditor | explicit scope and periodic review | Export reauth policy | REQUIRES_CONFIRMATION |
| HO-ACCESS-013 | Support Console | support Issue/note/attachment/export exact scope | TBD→Support Owner | queue/unit/assignment scope; no general content | Sensitive action policy | REQUIRES_CONFIRMATION |
| HO-ACCESS-014 | SSO Registration | register/read/update client metadata | TBD→IAM Owner | official IAM process | REQUIRED | REQUIRES_PERTAMINA_ACCESS_CONFIRMATION |
| HO-ACCESS-015 | Desktop Signing | package submit; custody admin separate | TBD→Signing Custodian | pipeline submit only; no material retrieval | REQUIRED | REQUIRES_CONFIRMATION |
| HO-ACCESS-016 | Mobile Signing/Distribution | package signing/publish/withdraw/admin separated | TBD→Mobile Custodian | controlled build/channel process | REQUIRED | REQUIRES_CONFIRMATION |
| HO-ACCESS-017 | Backup/Restore | backup catalog/execute/restore approve separated | TBD→Backup Owner | Incident/change-bound scope and revoke | REQUIRED | REQUIRES_CONFIRMATION |
| HO-ACCESS-018 | Incident/Service Desk | intake/triage/escalation/communication scoped | TBD→Support/Incident Owner | official support access lifecycle | Policy | REQUIRES_PERTAMINA_SERVICE_DESK_CONFIRMATION |

## Access acceptance

TRANSFERRED memerlukan approved record, exact scope/environment, effective access test, denied over-scope test, expiry/review, MFA/reauth evidence, giver/receiver acknowledgement, and revoke evidence. Register ini tidak memberi access.

