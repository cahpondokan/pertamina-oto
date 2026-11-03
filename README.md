# Cadangan Android Google ML Kit — 1.0.1+19

Snapshot 3 November 2026. Baca [BACKUP_MLKIT.md](BACKUP_MLKIT.md) untuk paket, flag build Google, dan pemisahan dari versi algoritma sendiri. Catatan lama di bawah adalah riwayat.

---

# PERTAMINA OTO

PERTAMINA OTO is currently in **Phase 0 — local bootstrap**. The first release is a Windows Desktop, offline-first application. This repository is non-Production and contains no product Feature behavior yet.

## Current bootstrap scope

- exact .NET SDK `10.0.400` targeting `net10.0`;
- deterministic root build configuration;
- NuGet.org v3 with source mapping limited to approved `Microsoft.*` and `MSTest.*` packages;
- the minimum active project graph: Contracts and Processing Abstractions;
- local restore/build entry point without deployment.

The Hostinger VPS and `oto.ayani.id` are candidates for later non-Production infrastructure. They are not dependencies of local Core processing. Azure migration and Production deployment require separate IT approval. iOS is deferred from the first release.

## Prerequisites

Install the .NET SDK version pinned in `global.json`. Do not add feeds or packages until their provenance, license, security, and owner approval have been recorded.

## Build

From the repository root on PowerShell:

```powershell
./eng/build/Build.ps1
```

Equivalent commands:

```powershell
dotnet restore ./Pertamina.Oto.sln --configfile ./NuGet.Config --locked-mode
dotnet build ./Pertamina.Oto.sln --configuration Release --no-restore
```

Tests and cross-cutting gates are added by the Phase 0 quality and pipeline lanes. A zero-test result must not be interpreted as a successful final Phase 0 gate.

## Active project graph

```text
Pertamina.Oto.Contracts
        ^
        |
Pertamina.Oto.Processing.Abstractions
```

`Pertamina.Oto.Contracts` has no project dependency. `Pertamina.Oto.Processing.Abstractions` may depend only on Contracts at bootstrap. Neither project contains UI, provider adapters, persistence, network calls, or product workflows.

## Dependency and source policy

See `eng/build/DEPENDENCY_SOURCE_RECORD.md`. Never commit passwords, tokens, certificates, production data, document content, or local `.env` files.

## Contribution workflow

Use one task branch and one worktree per active lane. Agents and contributors must follow the allowed-path ownership in the approved task card. Only the human integrator may authorize promotion to the default branch.
