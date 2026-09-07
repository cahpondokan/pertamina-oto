# Device, platform, and local-AI execution-freeze packet

Task `P2-W0G-B-A5-DEVICE-AI-DISPOSITION`, run
`P02-20260905-1434`, produces a no-execution matrix for five future PoCs. The
packet is a proposal for independent security/governance review. It does not
grant one of the four formal PoC dispositions and does not authorize `PRE-012`.

## Exact scope

| PoC | Exact Feature IDs | Current state | Non-binding recommendation |
|---|---|---|---|
| `POC-SCAN-001` | `FEAT-SCAN-014`, `FEAT-SCAN-018` | `NOT_STARTED_BLOCKED_PRE_EXECUTION` | Conditional Wave 2 or governed defer after an approved physical device/interface matrix. Deferring the physical path must not silently delete the Core Scan to PDF outcome. |
| `POC-INTEL-001` | `FEAT-INTEL-001` | `NOT_STARTED_BLOCKED_PRE_EXECUTION` | Wave 2 only after provider, data, model, corpus, quality, license, resource, and security gates. |
| `POC-INTEL-002` | `FEAT-INTEL-002` | `NOT_STARTED_BLOCKED_PRE_EXECUTION` | Wave 2 only after the same gates plus exact language/fidelity scope. |
| `POC-CONV-004` | `FEAT-CONV-036` | `NOT_STARTED_BLOCKED_PRE_EXECUTION` | Deterministic text-layer extraction first; a local OCR fallback is separate and must be explicitly approved. |
| `POC-AI-LOCAL-001` | `FEAT-ASSIST-026` | `NOT_STARTED_BLOCKED_PRE_EXECUTION` | Governed defer is recommended, but not decided, until Product schedules this optional `Could / R3` capability. |

The physical-scanner PoC maps to `FEAT-SCAN-014` because its observable product
outcome is Scan to PDF and to `FEAT-SCAN-018` because that is the exact physical
scanner feasibility gate. It does not open `FEAT-SCAN-001..013`, Android camera
capture, Web device access, or any mobile implementation.

The optional local-AI PoC maps only to `FEAT-ASSIST-026`. Passing that PoC would
not approve `FEAT-INTEL-001`, `FEAT-INTEL-002`, `FEAT-CONV-036`, a provider, a
model, or an execution route by implication.

## Locked boundaries

- Windows Desktop offline-first is the first-release priority, but the exact
  device/environment matrix is still `TBC`.
- Android and Web remain later curated targets and are not opened here.
- iOS remains `DEFERRED_NOT_IN_FIRST_RELEASE`; this packet does not reopen it.
- Public AI and public translation endpoints are denied. Only an exact approved
  local or enterprise-internal provider class could be evaluated later; no
  specific provider/model/runtime/device is approved now.
- A local route may claim offline only after network-disabled proof. An approved
  enterprise-internal route is online by definition and must become unavailable
  or pending when offline; it must never trigger silent upload or rerouting.
- Real/Production/personal documents are not permitted. A future exact synthetic
  corpus, classification, access, retention, expiry, disposal, and sanitization
  policy remain mandatory.
- The cost boundary is `ZERO_ADDITIONAL_COST`. Stop before a paid API, model,
  language pack, SDK, device component, commercial license, support agreement,
  subscription, or billable infrastructure commitment.

All provider/model/device/version/digest/license/resource threshold fields remain
explicitly `TBC`. The matrix lists the human owner and required value for every
reopen gate. Readiness recommendations are not human decisions, so every record
has `formalGovernedDisposition = NONE_NOT_GRANTED`.

## Static verification only

The verifier parses the two JSON files, checks the exact PoC/Feature mapping,
requires all execution/acquisition flags to remain false, enforces public-AI
denial and iOS deferral, and rejects four in-memory negative mutations. It uses
only Python standard-library JSON/text handling.

```text
python -B poc/portfolio/execution-freeze/device-ai/verify_static.py
```

The verifier does not launch Sandbox; inspect, open, generate, render, or process
a PDF; load or execute qpdf/PDFsharp/another engine; restore/build a project;
access a scanner; acquire or load a model; call an endpoint; or download/install
anything. Source SHA-256 bindings in the proposal were collected by read-only
`Get-FileHash` against the canonical workspace files and are evidence bindings,
not provider or execution approval.
