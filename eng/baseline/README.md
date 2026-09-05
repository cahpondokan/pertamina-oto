# Phase 0 Baseline Gates

This directory contains executable gates for the provisional local bootstrap authorized by `LOCAL-BOOT-001`, `P0-D06`, and `CCR-015`.

- `phase0-dependency-policy.json` is the machine-readable approved source/dependency allowlist.
- `Assert-ApprovedDependencies.ps1` rejects an extra NuGet source, source credential, source-mapping change, versioned project SDK, direct package reference, or central package version that is not admitted by that policy.
- `Assert-Phase0Scope.ps1` rejects Feature, host, module, infrastructure, deployment, or other product implementation outside the two approved Phase 0 BuildingBlocks paths.
- `Test-BaselineGates.ps1` proves that the approved fixture passes while an unapproved dependency and a forbidden Feature path are blocked in an isolated temporary fixture.

These controls are Phase 0 controls. Agent 0 must replace or version them through an approved change when later phases intentionally admit projects or dependencies.
