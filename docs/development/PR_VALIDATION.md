# Phase 0 PR Validation

## Purpose and authority

`pipelines/Invoke-PRValidation.ps1` is the provisional local PR-equivalent gate authorized by `LOCAL-BOOT-001`, `P0-D06`, `P0-D07`, `P0-D08`, and `CCR-015`. It validates the temporary offline-first repository. It is not a Production pipeline and contains no build publication, deployment, service connection, credential, Hostinger change, or Azure change.

The fixed fail-closed order is:

1. Restore
2. Build
3. Unit
4. Architecture
5. Contract
6. Dependency Audit
7. Secret Scan
8. Static Analysis
9. Safe Reports

A failed stage ends the run; later stages remain `NOT_RUN`. Zero discovered/executed tests, an unapproved source/dependency, an architecture or contract failure, a suspected secret, a static finding, formatting drift, or an invalid evidence manifest blocks validation.

## Local command

From a clean integrated repository at the commit being reviewed:

```powershell
./pipelines/Invoke-PRValidation.ps1 -Configuration Release
```

The runner requires PowerShell 7, Git, .NET SDK `10.0.400`, restored access to the approved NuGet.org v3 source, and the integrated Agent 1/2/6 files listed by the preflight. Generated output is placed below `artifacts/evidence/<run-id>/`; source control must ignore `artifacts/`.

The pipeline executes every test project found under each of `tests/Unit`, `tests/Architecture`, and `tests/Contract`, then applies the non-zero TRX guard to every project separately. It runs only already-built MTP applications from the current configuration.

## Controlled blocking proof

Run the isolated engine test:

```powershell
./pipelines/Test-PRValidationBlocking.ps1
```

The script starts a child PowerShell process in a unique temporary directory, injects a controlled failure at `Contract`, verifies a non-zero exit, verifies all later stages remained `NOT_RUN`, then runs an all-green control. The environment flag required by engine self-test is process-scoped and restored. Temporary canary output is deleted only after its resolved path is proven to be below the operating-system temporary directory.

This proves orchestration blocking, not that the integrated product build is green. Agent 0 must run the complete pipeline after integrating Agent 1, 2, 6, and 7.

## Gitea/Hostinger compatibility boundary

`.gitea/workflows/pr-validation.yml` is an optional, provisional Gitea Actions definition. A Gitea instance may run on Hostinger, while the least-privilege self-hosted runner is selected by the `oto-dotnet-10` label. The runner must be provisioned and approved separately. The workflow deliberately has read-only repository permission and disables persisted checkout credentials.

Before enabling remote CI, IT/Repository Integrator must verify the Gitea version, runner isolation, network egress, checkout-action provenance/pinning, protected-branch rule, artifact retention, log access, and runner cleanup. No remote CI or Hostinger deployment is claimed by the committed definition.

## Evidence interpretation

`pipeline-summary.json` records ordered stage states. `manifest.json` binds evidence file digests to the exact Git commit, run ID, task IDs, and decision IDs. Known local, user-profile, and temporary prefixes are replaced in stage logs. Do not place real document content or credentials in tests or evidence.

Evidence does not authorize a merge. Agent 0 audits the result; the workspace user retains provisional merge/promotion authority for Phase 0.
