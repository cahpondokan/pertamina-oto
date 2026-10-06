# Portable public development download — lane 3

Task: PORTABLE-PUBLIC-DEV-20261002. Baseline HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`, existing dirty work preserved. Sole writer scope assigned by integrator: Downloads host, Test-DownloadPortal.ps1, this document and lane artifacts. No central phase record or desktop code changed.

## Contract

The existing installer `Distribution:Root/current.json`, `/api/releases/windows-x64/latest` and `/downloads` contract is preserved. Portable uses separate `Distribution:PortableRoot/current.json` (default content-root/portable-releases), `/api/releases/portable/windows-x64/latest` and `/portable-downloads`. Missing, invalid, corrupt or reparse-point catalog/package fails closed. Each download rehashes the open stream before serving; GET/HEAD/ranges are supported, no public write API added.

Portable schema 1 requires version `yyyyMMdd-HHmmss-xxxxxxxx`, productVersion `0.1.0`, channel `development`, exact filename `PERTAMINA-OTO-Portable-Windows-x64-<version>.zip`, size (1–2147483648), sha256, publishedAt, runtime (`self-contained` or `dotnet-10-x64-required`) and entryPoint `PERTAMINA-OTO.exe`. Lane 1 must align these exact values or report a controlled contract change before packaging. No real portable metadata is published yet. Runtime claim must match independently verified artifact; valid JSON alone does not establish acceptance.

The page includes public development status, bytes/MB/hash/build, extract-all instruction, guide/licenses, optional Word, inactive OCR, unsigned/SAC/IT disclosure and no protection-disable guidance. Runtime copy is conditional on the actual approved catalog. Existing installer presentation remains available.

## Verification

`dotnet publish src/Hosts/Pertamina.Oto.Downloads/Pertamina.Oto.Downloads.csproj --no-restore -c Release -o artifacts/portable-web-01a0fa43-ef69/publish` — exit 0, no dependency acquisition. Sandbox cache access initially failed; approved local build elevation succeeded.

Final verification: `./eng/Test-DownloadPortal.ps1 -Publish artifacts/portable-web-01a0fa43-ef69/publish -Port 5163 -Evidence artifacts/portable-web-01a0fa43-ef69/checks-03` — exit0,45 PASS on final DLL `45372872A89BB646FC32E7F7875792E26FB9B52F02B1CFBFAC86E8099FEC54C9`. Evidence contains synthetic inert fixtures only. Full GET/hash, HEAD/range, disclosure, traversal, wrong hash/runtime/malformed or missing portable metadata, and existing installer compatibility passed. Responsive browser checks completed as recorded below.

Historical build only: checks-01 exit0,42 PASS on earlier DLL `8E9250EB7002F7E56C2F58638DD80462FD3F27D80EA9438E7859C28BB26A2EB2`, before metadata caching change. Superseded by final45 checks; not evidence for current DLL.

Read-only public HTTPS observation 2 October 2026: homepage 200, no portable section, legacy catalog version `20261001-102920-701ec3e1`; ZIP 37462679 bytes SHA256 `D39F9803B95FBA5F612912BEF3209071C7C34FFFE8E7D708DB2C808C72C5B9A7`; EXE 37575168 bytes SHA256 `E6704300B8E3D40FE8E60F228CB942CF2C06E72622D4594859BCCB7A46C139B1`. No full legacy binary download repeated; historical full GET proof is not portable QA.

## Deployment constraints and order

Integrator review follow-up: metadata verification now caches the fully verified catalog record against metadata/package timestamps and lengths, revalidating safe paths each time. Actual download still rehashes its opened stream. This avoids repeated full ZIP hashing for unchanged page/metadata calls; first admission plus download can still hash twice. Real candidate throughput/HEAD cost remains unmeasured; do not claim a performance budget PASS. Product-version validation is coupled to current 0.1.0 policy and requires a future controlled update when that policy changes. Final portal DLL SHA256 `45372872A89BB646FC32E7F7875792E26FB9B52F02B1CFBFAC86E8099FEC54C9`, superseding earlier build hash. Final tests `checks-03/test-results.txt`: 45 PASS, including missing publication date/entrypoint and unknown schema. Build exit 0; final test exit 0.

Existing target is `/opt/oto-downloads/current`, runtime isolated under `/opt/oto-downloads/runtime/10.0.12`, legacy data `/var/lib/oto-downloads/releases`, systemd oto-downloads and Nginx proxy. Existing activation helper requires root/sudo and rejects an unexpected current deployment. Do not reuse first-activation logic to overwrite the active deployment. General hosting scripts remain read-only. Fresh SSH read-only inspection completed below: current service active/enabled, targets root-owned and operator sudo password required. No update workflow executed.

After exact ZIP QA and integrator release clearance: freeze its bytes/hash/size; stage a new portal build and separate portable release directory; verify complete hashes; preserve current application target and both catalog baselines; prepare a bounded administrator update that changes only the OTO app pointer/service and portable catalog. Do not change DNS, firewall, global Nginx, accounts, legacy feed or old releases. Upload ZIP first, verify server size/SHA256, then atomic same-filesystem catalog rename. App swap requires existing operator authority and reversible service restart. Rollback restores prior app pointer and prior portable catalog while preserving immutable bytes and old releases. Verify public page, metadata, HEAD/range and full downloaded ZIP SHA256 against QA-approved bytes. No rebuild/repack after signoff.

## Handoff and blockers

Follow-up 2 October 2026: browser preview used inert 58-byte fixture only, not a released ZIP. Desktop viewport 1280x900: document scrollWidth1265 <= innerWidth1280; mobile390x844: scrollWidth375 <= innerWidth390. Both rendered portable CTA, extraction instructions and public-development disclosure visibly. Screenshots saved in the permitted visualization root: `C:/Users/cahpo/.codex/visualizations/2026/10/02/01a0fa43-ef69-74b2-bb16-cbfea4107f2c/portable-desktop.jpg` and `portable-mobile.jpg`. Temporary viewport reset. Final test result remains45 PASS, DLL hash45372872...; earlier42/hash8E9250... is superseded.

Fresh SSH read-only, strict existing ED25519 known_hosts, BatchMode (no credential prompt): service active/enabled; current symlink `/opt/oto-downloads/oto-20261001-102920-701ec3e1-v1/web`; `/opt/oto-downloads` root:root755; current root:root777 (symlink mode); releases root:oto-web750. `sudo -n true` exit1, password required. No server changes made. A staged regular-user upload cannot write these root-owned targets or restart the service without operator privilege.

Operator review packet: exact portal manifest and ZIP hash/size must be pinned; no new runtime is part of this lane; archive/stage links, path traversal, duplicate or unlisted files must be rejected. Guard exact current symlink baseline and existing OTO service; copy web to a fresh immutable versioned directory with root ownership; retain previous app target and previous separate portable catalog; do not touch legacy installer catalog. Verify staged ZIP before introducing its catalog by atomic same-filesystem rename. On failed loopback/public verification restore prior app target/catalog and restart only OTO; retain failed bytes and old releases. External full downloaded ZIP SHA256 must match QA approval before claiming completion. This is a review plan, not an executable or tested deployment workflow.

Attempt to create `eng/hosting/Portable-Update.sh` was rejected by automatic approval review before file creation. Reason: executable root-level deployment workflow could replace active symlink/release data/restart service; quoted delegated authorization was not accepted as direct trusted user authority. No workaround or alternate executable created. Direct trusted user approval to prepare this bounded server-update workflow is required; this is separate from repeating portable/public-download intent. Deployment execution also remains held behind QA/license/runtime/integrator gates and operator sudo.

Preview cleanup: initial PID/name-only stop was rejected because generic dotnet could target an unrelated process. Read-only CIM then proved PID33600 exact lane DLL, port5163 and checks-03 fixture paths. A fresh guarded check matched all identities before stopping only that process, exit0. User apps untouched. `Portable-Update.sh` confirmed absent. Screenshot capture initially could not write the worktree artifacts path, so the documented permitted visualization root contains the evidence; no permission settings changed.

Files: PortableCatalog.cs (new), Program.cs, Pages/Index.cshtml.cs, Pages/Index.cshtml, eng/Test-DownloadPortal.ps1 and this file. No publishing or real metadata writes performed. Safe prep is complete; public activation HOLD pending real immutable ZIP/license/runtime QA, real-candidate resource measurement, integrator release gate, direct trusted approval for bounded server-update workflow and operator sudo. Browser checks and read-only server constraints are complete. Exact package filename/entrypoint contract must match lane1 artifact before real metadata creation.

## Final source and evidence inventory

Paths below are relative to `D:/AFT Ahmad Yani Semarang/Teknik/Dokumentasi Pertamina OTO/worktrees/core-functional-first`; SHA256 binds the final implementation, not a future build.

| Path | SHA256 |
|---|---|
| src/Hosts/Pertamina.Oto.Downloads/PortableCatalog.cs | 6BDEDDF78B32086D16240F52589FE8A4780B9F9547F5C9AF6F4B04A5610E036E |
| src/Hosts/Pertamina.Oto.Downloads/Program.cs | DB6D2D46FA490C68BA3DB17D1B9857108AF4B06473571429064C6BBDD1CDC163 |
| src/Hosts/Pertamina.Oto.Downloads/Pages/Index.cshtml.cs | C37F45FE835BAD29B057C6A0332F2A5AA000E88D263D3BA5BB97FE21BD405212 |
| src/Hosts/Pertamina.Oto.Downloads/Pages/Index.cshtml | 99346BA0D7268EBAD3EE3B08198D9D49C0627D38CA3ACFA7AE7E96CC08F69EC7 |
| eng/Test-DownloadPortal.ps1 | C266DBF2AA34ACEE45B5AE7564E960A1FD44C9AB8D20588AA3561E10BD03BD30 |
| artifacts/portable-web-01a0fa43-ef69/checks-03/test-results.txt | 0471398428A1E22E9711F8D3575944019686EE85146BA01A5762D51663B327AC |

Absolute screenshot evidence:
- `C:/Users/cahpo/.codex/visualizations/2026/10/02/01a0fa43-ef69-74b2-bb16-cbfea4107f2c/portable-desktop.jpg`
- `C:/Users/cahpo/.codex/visualizations/2026/10/02/01a0fa43-ef69-74b2-bb16-cbfea4107f2c/portable-mobile.jpg`

This final handoff clarification changes documentation only. No build, test repetition or deployment performed.

## Lane1 immutable internal candidate reference

Received lane1 candidate03 reference (not independent QA acceptance): folder `D:/AFT Ahmad Yani Semarang/Teknik/Dokumentasi Pertamina OTO/worktrees/core-functional-first/artifacts/portable-01a0fa43-c0ed/candidate-03`; version `20261002-085443-ef3f12a3`; filename `PERTAMINA-OTO-Portable-Windows-x64-20261002-085443-ef3f12a3.zip`; size37362220 bytes; SHA256 `9BC67C5F3DFA1022FD26AF329497A6653150AF39A72B01D45AE577D7BE86CE16`; productVersion0.1.0; entryPoint `PERTAMINA-OTO.exe`; runtime `dotnet-10-x64-required`. Product label Pengembangan maps to portable catalog machine channel `development`, as existing web schema requires. If eventually admitted, page will truthfully require pre-existing .NET10 Runtime x64; it must not claim included runtime.

Status remains `HOLD_RUNTIME_ADMISSION_AND_DISTRIBUTION_DISPOSITION`, plus independent exact-artifact QA/integrator gate. This candidate does not yet satisfy the requested standalone portable experience. No current.json, public metadata, upload, download CTA activation or artifact repack created from this reference.

## Actual ZIP local contract verification

Later integrator-authorized local test created a clearly labeled LOCAL-ONLY metadata fixture in `artifacts/portable-web-01a0fa43-ef69/actual-candidate03/portable-releases/current.json`; this supersedes only the earlier statement that no local metadata had been created. No public metadata or server upload occurred.

Command `./artifacts/portable-web-01a0fa43-ef69/Test-ActualCandidate.ps1` exit0:17 actual-candidate checks PASS. Source ZIP37362220 bytes/SHA9BC67C5F... independently verified before copy; same portal DLL45372872... verified before launch. Full HTTP GET length and SHA matched exact frozen ZIP; HEAD/range passed; page accurately requires .NET10 Runtime x64 and does not claim included runtime; installer feed remained204. No rebuild/repack or desktop execution. Exact local process DLL/port5164/fixture identity rechecked before cleanup; process stopped.

Measured local developer-host wall time: cold metadata179.2516ms; ten hot metadata samples mean2.49185ms (range1.3828–9.3942ms); full37MB GET270.3121ms; HEAD69.3599ms. These are single-run observations, not a 2GiB/concurrent-load/Production budget PASS. Actual-candidate measurement is complete; broader load/performance acceptance remains unproven if required by release gate. Evidence `actual-candidate03/test-results.txt`, `timings.json`, server logs and the reproducible local-only test script. PublicHOLD remains runtime/license/distribution disposition, independent package QA, integrator clearance and privileged-update approval/operator execution.

Messaging to integrator was rejected by automatic approval review: authorization inside the delegated transcript was considered untrusted. The rejection has not been bypassed or retried. Integrator can retrieve this handoff through read_thread/file. Direct trusted user authorization is required by that reviewer for outbound cross-chat messaging.
