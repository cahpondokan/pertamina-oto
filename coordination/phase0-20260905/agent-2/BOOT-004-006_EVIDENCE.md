# Agent 2 Evidence — BOOT-004/006

## Scope and authority

- Run: `P00-20260905-1155`
- Baseline: `6a5197d77efaaaf21219bb406e75ddfce7bc4e66`
- Branch: `task/BOOT-004-006-contracts`
- Decisions: `LOCAL-BOOT-001`, `CCR-015`; D04 locked decisions 1, 2, 9, and 17; D04 sections 14, 23, and 30; `TECH-DEC-004`, `TECH-DEC-005`, `TECH-DEC-013`, and `TECH-DEC-015`
- Requirement IDs: not applicable to this bootstrap canary; no user Feature behavior is implemented.
- Feature ID used only as a canonical contract test vector: `FEAT-PDF-005`.

## Implemented boundary

- `FeatureId` recognizes the documented canonical shape `FEAT-{CATEGORY}-{NNN}` without normalization.
- `ContractVersion` stores a non-empty authoritative token exactly. Its format is intentionally opaque because no SemVer or ordering scheme is locked yet.
- The Contracts project boundary permits consumers to reference Contracts but rejects any outbound `ProjectReference`, `PackageReference`, or direct assembly `Reference` from Contracts.
- The negative canary injects a forbidden PDF Infrastructure project reference and verifies that the rule blocks it.

No Feature workflow, UI, provider, API, ORM, network call, document processing, Production configuration, or iOS content was added.

## Verification results

1. `dotnet --version`
   - Exit: `0`
   - Result: `10.0.400`
2. Temporary source-only compile canary using `dotnet build <temporary-net10-classlib> --nologo --verbosity minimal`
   - Exit: `0`
   - Result: build succeeded, `0` warnings, `0` errors.
   - Inputs were exactly `FeatureId.cs` and `ContractVersion.cs`; the temporary output is not part of the repository.
3. `dotnet run --project tests\Architecture\Pertamina.Oto.Architecture.Tests\Pertamina.Oto.Architecture.Tests.csproj -- --filter 'FullyQualifiedName~Negative_canary_rejects_a_forbidden_module_reference' --minimum-expected-tests 1 --progress off --output Detailed`
   - Exit: `0`
   - Result: total `1`, succeeded `1`, failed `0`, skipped `0`.
4. Static privacy/boundary scan over `src` and `tests`
   - Patterns included credential markers, private-key headers, EF Core, WinUI, ASP.NET Core, `HttpClient`, SQL connection, iOS, and Production terms.
   - Result: no matches.
5. `git diff --check`
   - Exit: `0`.

## Integration prerequisites

Agent 1 / Agent 0 must provide and integrate:

- `global.json` with SDK `10.0.400`;
- root build and NuGet policy;
- `src/BuildingBlocks/Pertamina.Oto.Contracts/Pertamina.Oto.Contracts.csproj`;
- solution membership for both Agent 2 test projects.

Agent 6 / Agent 0 must align the repository test entry with Microsoft Testing Platform on .NET 10. Direct `dotnet test` used the legacy VSTest target before root MTP configuration existed and was rejected; the MTP executable command above is independently green.

After integration, run both full suites without a filter and require non-zero discovery:

```powershell
dotnet run --project tests\Contract\Pertamina.Oto.Contracts.Tests\Pertamina.Oto.Contracts.Tests.csproj -- --minimum-expected-tests 1 --progress off
dotnet run --project tests\Architecture\Pertamina.Oto.Architecture.Tests\Pertamina.Oto.Architecture.Tests.csproj -- --minimum-expected-tests 2 --progress off
```

The first architecture test is intentionally pending integration because it scans the real Contracts project file, which Agent 1 owns.
