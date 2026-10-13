# EXTERNAL DEPENDENCY REGISTER
## PERTAMINA OTO — Document 18

> Versi: FINAL v1.0
> Status: PLACEHOLDER-GOVERNED

Tidak ada provider/vendor/product yang dianggap approved hanya karena tercantum.

| ExternalDependencyId | Name/Placeholder | Purpose / Type | Required? / Data shared | Authentication / Network need | Impact / Fallback | Security/License/PoC | Owner / Environment / Confirmation / Status |
|---|---|---|---|---|---|---|---|
| CART-EXT-001 | Pertamina Identity/SSO | User/workload identity; enterprise service | Required online; identity claims only | Protocol/profile/endpoint TBD | Online auth unavailable; eligible local offline only | Security review required; license N/A; integration PoC | IAM owner TBD; all online env; SSO/network confirmation; PENDING |
| CART-EXT-002 | Azure DevOps | Repository/pipeline/artifact; supply chain | Required build/release; source/artifact/evidence | Enterprise identity/network TBD | No new trusted release; retained verified artifact only | Access/SBOM/retention review | DevOps owner TBD; all release env; infrastructure confirmation; PENDING |
| CART-EXT-003 | Approved Secure File/Object Storage | Server payload; storage service | Required server payload; restricted bytes | Workload identity/private route TBD | Server file flow unavailable; local result where eligible | Security/license/restore PoC required | Storage owner TBD; isolated per env; product confirmation; PENDING |
| CART-EXT-004 | Approved PDF Engine | PDF capability; library/provider | Required Core PDF; job-scoped content | Local package or internal route | Disable affected capability or approved fallback | License/SBOM/security/fidelity/resource PoC | Pdf owner TBD; profile per env; provider confirmation; POC_REQUIRED |
| CART-EXT-005 | Approved Conversion Engine | Office/format conversion | Conditional/required per Feature; content | Local/internal approved route | Feature unavailable or local/server approved alternative | License/security/fidelity/active-content PoC | Conversion owner TBD; per env; provider confirmation; POC_REQUIRED |
| CART-EXT-006 | Approved OCR/Scanner Provider | OCR/device capability | Conditional; image/document | Device/local/internal route | Feature unavailable; no unapproved upload | License/security/language/fidelity/device PoC | ScannerOcr owner TBD; per env/device; POC_REQUIRED |
| CART-EXT-007 | Approved Internal AI/Translation Provider | Summarize/translate | Optional/required for INTEL profile; minimized content | Workload identity/allowlisted internal route | Local approved provider or explicit unavailable; never public AI | Security/data/license/retention/grounding PoC | ContentIntelligence owner TBD; profile per env; POC_REQUIRED |
| CART-EXT-008 | Application Insights/APM path | Technical observability | Operationally required; safe telemetry only | Exporter identity/network TBD | Reduced visibility; bounded safe buffer/degraded state | Privacy/access/retention review | Observability owner TBD; isolated streams; monitoring confirmation; PENDING |
| CART-EXT-009 | Enterprise Network/Infrastructure | DNS/TLS/time/ingress/private paths | Required online; packets/metadata | Enterprise control TBD | Online services unavailable; local-only continues | Network/security/VA-PT review | Infrastructure owner TBD; all env; network/infrastructure confirmation; PENDING |
| CART-EXT-010 | Enterprise Distribution/Signing | Desktop/Mobile trusted install/update | Required client release; packages/metadata | Signing identity/channel TBD | No trusted update/install; retain supported package | Code-signing/security/license review | Release owner TBD; Pilot/Production; signing/distribution confirmation; PENDING |
| CART-EXT-011 | Backup/Recovery Location | Independent recovery copy | Required by approved DR; encrypted backup data | Backup identity/private route TBD | Recovery incomplete; active-node copy not accepted | Security/restore/legal/data residency review | Backup owner TBD; isolated per env; DR confirmation; PENDING |

## Admission gate

Dependency hanya menjadi APPROVED bila owner, exact product/version, purpose, data, identity, network, security, license, PoC, monitoring, fallback, support, retention, incident, exit/removal, environment, dan approval evidence tersedia.

