# PERTAMINA OTO — Phase 2 Wave 0D Checkpoint Report

| Field | Value |
|---|---|
| Run | `P02-20260905-1434` |
| Checkpoint time | 2026-09-06 01:44:56 +07:00 |
| Final status | `BLOCKED_PENDING_RESTART_AND_ACQUISITION_AUTHORITY` |
| Product main | Clean `main` @ `5769ccce917d03905d07a1440ee527cb653c9cc4` |
| Integration checkpoint | `integration/P2-proof-capability` @ `38d5494050f62caea2427c546125791d53c73ca9` |
| Cost | `ZERO_ADDITIONAL_COST` |
| PDF processing | `NOT_RUN` |
| Sandbox launch / restart | `NOT_RUN` / `NOT_RUN` |

## Outcome

Wave 0D is integrated and independently reverified. Windows optional feature
`Containers-DisposableClientVM` is already `Enabled`; the approved helper issued no
restart command. The Sandbox launcher is still absent and Windows reports CBS and
pending-file-rename reboot indicators. Phase 2 therefore cannot perform isolated
document processing until a separate human restart approval is given and the host is
reverified after restart.

No package, installer, source archive, paid license, service, or infrastructure was
acquired in Wave 0D. No real or synthetic PDF was passed to PDFsharp, Poppler, qpdf,
MuPDF, or any other provider. No change was merged to product `main`.

## Agents and actual waves

| Wave | Specialists actually used | Result |
|---|---|---|
| Readiness | Agent 1–7 in seven separate sidebar tasks | Complete, read-only handoffs |
| 0A | Agent 1, 2, 3 | Harness, PDFsharp intake, and synthetic corpus/oracle foundation |
| 0B | Agent 6, 7 | Independent security and governance review |
| 0C | Agent 1, 2, 3 | No-launch Sandbox profile, dependency metadata closure, oracle research |
| 0D | Agent 3, 6, 7 | Local Poppler inventory and independent security/governance decision packet |
| Integration | Agent 0 | Audited and integrated every accepted handoff in dependency order |

Agent 0 integrated Agent 3 commits `86adad8` and `74e93c1`, Agent 6 commit
`cb49469b9e91121a3e2a0240fc62516bf27dbf33`, and Agent 7 commit
`e6dd6bbac626422e3feeef951787a30615fc7e62`. The resulting integration commits are
`3656d50`, `b11ef68`, `b657708`, and `38d5494`.

## Verification evidence

- Windows Sandbox template static validator: 15/15 PASS; launch authorized `false`;
  effective network denial is not yet proven.
- Dependency metadata verifier using `pwsh`: PASS; four exact nodes, three edges,
  deterministic text-only manifest. `closure.json` SHA-256:
  `3130E92422B3449A26D30B80B3EE3030CC1699F8810D439AFB54A767CCEA857A`.
- Local Poppler evidence binding: 9/9 evidence entries matched.
- Local Poppler current-byte recheck: 445/445 files matched inventory; no missing,
  changed, or extra file at audit time.
- Review input bindings: Agent 6 seven inputs and Agent 7 four inputs matched; zero
  checksum mismatch.
- Integration worktree and product `main`: clean after verification.

The dependency verifier is authored for PowerShell 7 (`pwsh`). A diagnostic run with
legacy Windows PowerShell produced a byte-format comparison failure; the required
`pwsh` execution passed. This does not change the frozen manifest hash.

## Component disposition

| Component | Current disposition | Cost/license observation |
|---|---|---|
| PDFsharp 6.2.4 | Acquired earlier, static intake only; no restore/build/run | MIT metadata; local nupkg already hash-bound |
| Logging.Abstractions 8.0.3 | Ready to request acquisition-only authority | Exact NuGet metadata proposes MIT/no license fee |
| Cryptography.Pkcs 8.0.1 | Ready to request acquisition-only authority | Exact NuGet metadata proposes MIT/no license fee |
| DependencyInjection.Abstractions 8.0.2 | Ready to request acquisition-only authority | Exact NuGet metadata proposes MIT/no license fee |
| qpdf 12.4.1 | Ready for a separate acquisition-only human decision | Exact official MSVC64 ZIP URL and advertised SHA-256 frozen in `PHASE_02_QPDF_12_4_1_ACQUISITION_PACKET.md`; Apache-2.0 metadata; not downloaded |
| MuPDF 1.28.3 | Deferred | Free AGPL route has unresolved source/build/compliance scope; paid route excluded |
| Existing Poppler 26.07.0 | Hold use/copy/redistribution; inventory-only | Provenance, notices, full SBOM, lifecycle, and security freshness unresolved |

Poppler inventory records x64 version 26.07.0, 445 files, 123 PE files, and a static
`poppler.dll` dependency on `libcurl.dll`. That edge does not prove network activity,
but it makes effective no-egress proof mandatory. The existing runtime is not treated
as product redistribution authority or a stable product dependency.

## Exact next acquisition decision

The next minimal package is acquisition-only for exactly these three official NuGet
artifacts:

1. `Microsoft.Extensions.Logging.Abstractions` 8.0.3 —
   `https://api.nuget.org/v3-flatcontainer/microsoft.extensions.logging.abstractions/8.0.3/microsoft.extensions.logging.abstractions.8.0.3.nupkg`
2. `System.Security.Cryptography.Pkcs` 8.0.1 —
   `https://api.nuget.org/v3-flatcontainer/system.security.cryptography.pkcs/8.0.1/system.security.cryptography.pkcs.8.0.1.nupkg`
3. `Microsoft.Extensions.DependencyInjection.Abstractions` 8.0.2 —
   `https://api.nuget.org/v3-flatcontainer/microsoft.extensions.dependencyinjection.abstractions/8.0.2/microsoft.extensions.dependencyinjection.abstractions.8.0.2.nupkg`

If approved, acquisition remains limited to the run evidence intake directory and
static inspection: source/time/size, catalog-hash comparison, local SHA-256,
signature/trust limitation, license/notices, archive paths, analyzers, build targets,
native/runtime assets, and dependency surprises. It does not authorize restore,
build, source-generator or assembly execution, PDF processing, product references,
extra dependencies, qpdf/MuPDF acquisition, or paid components. Any mismatch,
unexpected dependency, redirect outside the approved source, or cost stops the lane.

## Exit gate and blockers

Phase 2 exit gate does not pass. Mandatory blockers are:

1. separate user approval to restart Windows, followed by launcher and effective
   Sandbox control verification;
2. separate acquisition-only approval for the three exact no-cost NuGet artifacts;
3. completed archive/license/signature/security review and exact dependency freeze;
4. admitted independent structural/render oracle;
5. frozen Sandbox run/export/retention/safety policy and explicit document-processing
   authorization.

Phase 3 remains `BLOCKED_BY_SEQUENCE`. Program progress remains 2/12 phases = 16.67%.

After the Wave 0D integration, Agent 0 also verified the official qpdf 12.4.1 release
and expanded-assets metadata. The exact `qpdf-12.4.1-msvc64.zip` URL and upstream-
advertised SHA-256 are now frozen for a separate acquisition-only decision; no qpdf
bytes were downloaded.
