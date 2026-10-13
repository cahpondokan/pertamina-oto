# Portable public development integration — 2 October 2026

Scope: user-authorized portable ZIP, Indonesian MULAI-DI-SINI.txt, separate required licenses, and existing oto.ayani.id public development download. No phase promotion, Production release, installation, signing, or security-policy bypass.

## Baseline and ownership

- Source worktree: core-functional-first, HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`, existing dirty/untracked changes preserved. HEAD alone does not identify the shipped source.
- Product identity: `0.1.0`, `Pengembangan`; eng/ProductRelease.props unchanged.
- Existing local candidate: `20261001-153307-07f0be4a`, manifest SHA256 `2A195972778937DE5FC4E91BB84E3E9F6C809C15FC5946828091069F4334FD99`. Current pointer observed during preflight; historical tests are not acceptance of a new ZIP.
- Package writer `01a0fa43-c0ed-7531-b673-c3b8b82bc402`: eng/Portable*.ps1, eng/portable/**, PORTABLE_PACKAGE.md, artifacts/portable-01a0fa43-c0ed/**. Existing Package-Portable.ps1 initially read-only; shared product paths require a sequential ownership assignment.
- Independent QA `01a0fa43-d973-7911-b663-1a86cdd2c1f5`: tests/portable/**, PORTABLE_QA.md, artifacts/portable-qa-01a0fa43-d973/**. No product/package fixes or acceptance of its own implementation.
- Website writer `01a0fa43-ef69-74b2-bb16-cbfea4107f2c`: src/Hosts/Pertamina.Oto.Downloads/**, eng/Test-DownloadPortal.ps1, new eng/hosting/*Portable* files, PORTABLE_WEB.md, artifacts/portable-web-01a0fa43-ef69/**. Sole deployment writer, after release gate.
- Integrator `01a0fa43-2418-7732-9e30-2350bd032eb5`: this record, shared integration after handoff, release evidence audit and report to parent. Central phase records remain untouched.

## Checkpoint protocol

Package and website preparation may run concurrently within ownership. One shared desktop build at a time; source changes stop during snapshot/build. Package must record exact source inventory, admitted component/runtime inventory, required legal text mapping, immutable ZIP SHA256/size and payload manifest. QA executes the exact extracted ZIP with synthetic data in isolated paths. Changed bytes invalidate acceptance; never inherit PASS after rebuilding/repacking. Website preserves installer/update API compatibility and old releases, publishing only accepted bytes with reversible atomic metadata.

## Release gate

Required evidence: package and ZIP integrity, runtime isolation, safe startup/diagnostics, user-data isolation, basic document correctness/source preservation/failure behavior, legal notices and distribution admission, honest development/unsigned/Word/OCR limitations. Independent QA handoff is required. Missing mandatory security/license/correctness evidence blocks publication; any required human risk disposition must remain human-owned. Clean-PC and physical UAT gaps must be explicitly recorded and dispositioned, never presented as PASS on a developer host.

Known security limitation: reported friend-device CodeIntegrity3077 and FileLoadException0x800711C7 indicate Smart App Control blocking the unsigned launcher. Portable packaging does not prove this restriction is removed; no disabling/bypass guidance is authorized.

## Pre-build checkpoint

Parent confirmed its own core work is idle. Compact snapshots individually checked Word, product version, OCR and updater chats: all notLoaded, with completed last turns. This is a point-in-time observation, not a global lock; lane1 rechecks available tasks and hashes inputs before/after the isolated build.

Lane1 additionally owns eng/Build-PortablePackage.ps1 and approved minimal core paths: PortableProfile.cs, PersistentLibrary.cs, LocalDiagnostics.cs, SettingsWorkspace.cs, OnlineUpdateNotifications.cs, LocalAppUpdate.cs, PdfAssociation.cs, PdfDefaultSettings.cs, UpdateSettings.cs and limited pre-WinRT portable startup validation in Program.cs. Installed behavior must remain unchanged. Shared desktop build slot reserved to lane1; integrator does not run another build.

QA and website outbound handoff messages were rejected by automatic approval review for specific-thread authorization. Their written handoffs/read_thread are used transparently; rejection is not bypassed. Inbound integrator instructions succeeded. QA initial verifier regression: four discovered tests pass; no product runtime acceptance yet. Web preparation reports 42 loopback checks, with final build identity and browser verification still being reconciled.

Unresolved gates: runtime10.0.12 intake explicitly covers the unmodified vendor installer, not embedding; older installed10.0.11 has no established bundle admission. Windows App SDK1.7.250909003 license3b(ii–iii) requires external-user protective terms/distributor obligations; copying legal texts alone does not disposition these human obligations. Candidate preparation proceeds without implying either gate is approved.

## Frozen integration candidate

Product `0.1.0 · Pengembangan`, build `20261002-085443-ef3f12a3`. Internal framework-dependent ZIP: `artifacts/portable-01a0fa43-c0ed/candidate-03/PERTAMINA-OTO-Portable-Windows-x64-20261002-085443-ef3f12a3.zip`, 37,362,220 bytes. SHA256 `9BC67C5F3DFA1022FD26AF329497A6653150AF39A72B01D45AE577D7BE86CE16`; manifest `5397E5688772B29EC5371B5E02794B5B45E77299D9E661E447031061B44A5D09`. Integrator independently matched both hashes, packaged source inventory `1FACB98D1BAF03B7045C62C35B4CDC5A1DF472DB73F056029B2CD849D61AFE99`, and runtime assessment packet `0DEA90BABD9B9BAC54B9F559D3D5BBE7CB2A2762F273D9C43341A26BC921305B`.

400 declared payload files plus manifest include root launcher, Indonesian guide, separate lisensi directory and marker-driven profile. Runtime is **not bundled**. It needs existing .NET10 x64 and does not fulfill the requested portable experience without separately available runtime. No public ZIP URL exists. Source HEAD is historical; relative selected source hashes and actual binary hashes bind this candidate. Independent complete build reproduction/SBOM is not claimed.

Candidate01 failed long-path verification. Candidate02 was superseded because guide changed during packaging; its diagnostic tests are not inherited. Candidate03 inputs are frozen, producer reports zero source changes after build and exact desktop/PDF/Word DLL equality with tested build. No rebuild or repack follows acceptance. Three actual specialist chats prepared package and website concurrently; independent QA followed frozen artifact. No new task/subagent or phase promotion occurred.

## Evidence audit

| Scope | Result and limits | Evidence |
|---|---|---|
| Desktop build and profile | Rebuild exit0 after CA1806 fix, no analyzer waiver; 13 profile checks | PORTABLE_PACKAGE.md; lane1 build-retry.log/profile.log |
| Existing operation regressions | Producer PDF41/editor63/search25/Word27, Distribution62/updater13 PASS; synthetic, bounded | lane1 worker-self-test*.log/word.log/distribution.log |
| Package boundary | Producer seven integrity/negative checks PASS; missing runtime admission stops before output creation | lane1 integrity-results.txt |
| Independent exact ZIP | Full400 payload+manifest equality/hash; eight extracted-candidate checks PASS, including missing/changed marker refusal and synthetic two-page merge/source preservation/no invalid-input output | PORTABLE_QA.md; artifacts/portable-qa-01a0fa43-d973/final-03/results.json |
| Native extracted desktop | Library4 PASS/exit0. Reader48 PASS then one FAIL/exit1: installed-mode audit expects default-PDF association action intentionally unavailable in portable. Suite is not PASS; portable-specific remaining audit is unverified. Integrator inspected reader and portable Settings screenshots. | final-03/native-library.json and native-reader.json; synthetic screenshots |
| Legal inventory | Twenty legal files; seventeen explicit mapped records across seven package components match payload hashes; exact retained PDFsharp MIT present. Full shipped native identity/obligations and human distribution disposition remain HOLD. | final-03/legal-inventory.json |
| Portal compatibility | Final portal DLL `45372872A89BB646FC32E7F7875792E26FB9B52F02B1CFBFAC86E8099FEC54C9` independently matched; 45 HTTP checks PASS; 17 additional checks on exact candidate03 including fullGET/hash/HEAD/range, separate installer feed | PORTABLE_WEB.md; lane3 checks-03 and actual-candidate03 |
| Browser | Desktop1280 and mobile390 observed without horizontal overflow, inert fixture only | PORTABLE_WEB.md absolute screenshot paths |
| Server | Read-only HTTPS/SSH: old102920 portal active; current root-owned targets; sudo-n requires operator password | PORTABLE_WEB.md |

Actual local candidate download matched frozen ZIP; this is not a public-download verification. Metadata cold179ms/hot average2.49ms, fullGET270ms, HEAD69ms are developer-host observations, not concurrency/Production performance acceptance. Python standard extraction hit a deep-path limitation; extended-path test extraction and launcher succeeded without OS policy edits. User guide recommends short writable local paths; physical Explorer extraction remains unproven.

## Disposition and next required evidence

1. Runtime admission owner (Technical/Security plus distribution authority): choose and verify the exact redistribution candidate, provenance/acquisition binding, contemporary security assessment, required notices and bounded authority. Existing10.0.11 packet has193 files/80,458,478 bytes and valid Microsoft host/fxr/coreclr signatures but is NOT_ADMITTED. The10.0.12 installer-only intake cannot authorize embedding. No acquisition or runtime bundling occurred.
2. Human distributor/license authority: resolve Windows App SDK/.NET Library protective external-user terms and distributor obligations, with complete shipped native/managed legal mapping. Twenty legal files are evidence, not clearance; agents do not accept human indemnity obligations.
3. Security/release acceptance owner: disposition unsigned public development risk and record an exact-artifact trust path. Smart App Control may still block portable; no disabling or bypass is an accepted resolution. Normal GUI startup/readiness, Save As/cancel/read-only Data behavior, clean Windows/physical UAT and network-denied desktop proof remain unproven. Technical gaps require evidence, not a generic approval checkbox.
4. Server operator: after package gate passes, provide direct trusted authority for the bounded existing-service update workflow and operator sudo execution. Automatic approval review rejected creating that executable workflow from delegated authority; no workaround was created. Existing rollback design is review-only. Publishing must use a new immutable admitted-runtime candidate, independently tested, and verify its full publicGET hash afterward.

All three specialist chats reached final handoff/idle; independent QA verdict is HOLD. Integrator read the lane reports, exact result files and final snapshots, verified cross-lane metadata compatibility through actual-ZIP local HTTP evidence and independently matched final hashes. No cross-lane patch or further rebuild was needed; no Git commit, branch merge or public release occurred.

Current status: **BLOCKED / public release HOLD**. Safe local preparation and bounded candidate verification completed; complete standalone runtime packaging, public deployment and installed-user delivery are not complete. Old website, local feed, user installation/data and central phase records remain unchanged. Read [PORTABLE_PACKAGE.md](PORTABLE_PACKAGE.md), [PORTABLE_QA.md](PORTABLE_QA.md) and [PORTABLE_WEB.md](PORTABLE_WEB.md) for lane handoffs. Continue the same three lanes only after exact blocker disposition; do not infer Production/stable acceptance.
