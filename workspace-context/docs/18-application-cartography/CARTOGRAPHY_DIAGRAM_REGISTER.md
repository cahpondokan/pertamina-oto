# CARTOGRAPHY DIAGRAM REGISTER
## PERTAMINA OTO — Document 18

> Versi: FINAL v1.0
> Format: Mermaid + Markdown table + ASCII fallback
> Template resmi: REQUIRES_PERTAMINA_CARTOGRAPHY_TEMPLATE_CONFIRMATION

Setiap rendered view harus mempertahankan source diagram, version, evidence source, owner, dan validation status.

| Diagram ID | Title | Purpose / Scope | Legend | Source Documents | Last Updated | Owner | Validation Status |
|---|---|---|---|---|---|---|---|
| CART-DGM-001 | System Context | Actor, OTO, external dependencies | Actor/system/external | 01–05,08,17 | 2026-09-01 | TBD | SOURCE_VALIDATED |
| CART-DGM-002 | Application/Container | Clients, hosts, stores, providers | Client/host/store | 04–08,17 | 2026-09-01 | TBD | ARCHITECTURE_VALIDATED |
| CART-DGM-003 | Module/Component | 12 module, building blocks, components | Module/component/contract | 03–05,07 | 2026-09-01 | TBD | ARCHITECTURE_VALIDATED |
| CART-DGM-004 | Core Tool Map | 33 card to Feature/module/runtime | Core/local/hybrid/server | 03,06,09–16 | 2026-09-01 | TBD | SOURCE_VALIDATED |
| CART-DGM-005 | Feature Family | 550 prefix/count/owner overlay | Prefix/count/module | 03–04 | 2026-09-01 | TBD | OWNER_PENDING |
| CART-DGM-006 | Integration | REST/SignalR/IPC/events/providers | Protocol/direction/authority | 04,06,08,10 | 2026-09-01 | TBD | SECURITY_VALIDATED |
| CART-DGM-007 | Local Processing | Local application/Worker/Workspace | sync/async/SOT | 04–08,17 | 2026-09-01 | TBD | SECURITY_VALIDATED |
| CART-DGM-008 | Server Processing | Upload/Job/Worker/provider/result | Job/state/storage | 06–08,17 | 2026-09-01 | TBD | DEPLOYMENT_VALIDATED |
| CART-DGM-009 | Data Store | SQL/SQLite/Workspace/storage planes | owner/write/SOT | 07,10,17 | 2026-09-01 | TBD | SOURCE_VALIDATED |
| CART-DGM-010 | Trust Boundary | Conceptual zones and controls | zone/boundary/control | 08–10,17 | 2026-09-01 | TBD | PERTAMINA_CONFIRMATION_REQUIRED |
| CART-DGM-011 | Environment/Deployment | Local Dev→Production placeholder | logical/physical/env | 17 | 2026-09-01 | TBD | DEPLOYMENT_VALIDATED |
| CART-DGM-012 | Hostinger Temporary | Logical separation on one non-Prod node | process/data/failure domain | 17 | 2026-09-01 | TBD | DEPLOYMENT_VALIDATED |
| CART-DGM-013 | Production Placeholder | Logical Production dependencies | placeholder/confirmation | 08,17 | 2026-09-01 | TBD | PERTAMINA_CONFIRMATION_REQUIRED |
| CART-DGM-014 | Ownership/Support | Functional owner and L1/L2/L3 | accountable/support/TBD | 09,15–17 | 2026-09-01 | TBD | OWNER_PENDING |
| CART-DGM-015 | Traceability | Requirement→Feature→runtime→evidence | edge status | 01–18 | 2026-09-01 | TBD | SOURCE_VALIDATED |
| CART-DGM-016 | Dependency/SPOF | Critical dependency, impact, fallback | required/optional/fallback | 04–08,16–17 | 2026-09-01 | TBD | PERTAMINA_CONFIRMATION_REQUIRED |

## Diagram quality gate

- Satu diagram tidak menampilkan seluruh sistem.
- Maksimum detail sebatas component berguna; tidak menggambar setiap class.
- Label Feature/API/schema/permission memakai identity canonical.
- Official network/product/owner tidak dikarang.
- Source dan rendered diagram diperbarui bersama.
- Diagram stale tidak boleh dipakai untuk coding/deployment decision.

