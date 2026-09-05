# Phase 0 Dependency and Source Record

| Field | Approved bootstrap value |
|---|---|
| Decision references | `LOCAL-BOOT-001`, `CCR-015` |
| SDK | .NET SDK `10.0.400` |
| Target framework | `net10.0` |
| Package source | NuGet.org v3 (`https://api.nuget.org/v3/index.json`) |
| Source mapping | `Microsoft.*`, `MSTest.*` only |
| Runtime package dependencies | None |
| Bootstrap test SDK | MSTest.Sdk `4.4.0`, only in test projects that require it |
| Production feed or credential | None |

## Admission policy

1. Clear inherited package sources and use only the source declared in `NuGet.Config`.
2. Admit a new package only after owner approval and provenance, license, and security review.
3. Pin the approved version centrally when the dependency is a `PackageReference`.
4. Commit lock files for every active project and restore using locked mode.
5. Do not store feed credentials, tokens, certificates, or package contents in source control.

At the Agent 1 foundation checkpoint, both active projects rely only on the .NET shared framework and introduce no external runtime package.
