# Window minimum — 8 October 2026

Status: native minimum and local 0.2.3 delivery verified; ZIP/EXE prepared and verified locally. Full server activation archive remains pending reviewed web input. No remote upload or user-installed activation.

Latest 9 October: explicit human instruction now authorizes upload and activation of the existing development website/feed. The full server archive is prepared, uploaded and verified in private staging. Native PowerShell activation terminal is open and requires the operator's sudo password; public activation has not yet been proved. See the latest checkpoint below; earlier local-only statements remain historical.

Scope: bounded native sizing policy, directly demonstrated shared header reflow correction, dedicated DEBUG audits/tests, approved 0.2.3 development release preparation. Existing renderer, search/highlight, zoom, processing, editing/export/undo, Print submission, restart/update, and result-action implementations remain outside this change.

## Discovery evidence

Evidence root: `artifacts/window-minimum-20261008`. Native monitor actually exercised: 1920 × 1080 at 100%; work area 1920 × 1032. Restored native chrome adds 16 × 39 physical pixels. Client includes the desktop menu; root content is below that menu. Maximized window coordinates may include the normal offscreen frame; screenshot capture bounds are not a substitute for AppWindow geometry.

Independent width, height, and combined probes precede enforcement. At 480 × 480 and 640 × 560 client DIP, editor/reader space was inadequate. Expanded real search (nine positioned hits in a synthetic three-page PDF) further reduced useful reading space: 520 × 640 gave 15 DIP, 640 × 720 gave 137 DIP, 640 × 740 gave 157 DIP, 640 × 760 gave 177 DIP, and 640 × 800 gave 217 DIP. Zero clipping counters alone are insufficient: the 480 × 640 expanded reader had zero reading height despite reachable controls.

Candidate: **640 × 760 client DIP**, corresponding to **656 × 799 outer physical pixels at this actual 100% monitor**. Width 600 produces the same vertical reflow but gives 40 DIP less horizontal margin. Height 760 provides 17 DIP beyond a 160-DIP useful-document floor. Native observation at 100% document zoom shows several complete table rows, highlighted search target, horizontal/vertical scrolling, search navigation, page controls, and zoom controls together. This is a minimum for a bounded task, not a recommendation for long reading or editing; maximizing remains preferable.

At 800 × 760, the old numeric header breakpoint allowed actions to squeeze the document title to a few characters and wrap metadata into many lines. The shared helper now also budgets the unwrapped action width and a 200-DIP title column before choosing one row. Native before/after images demonstrate the title restoration and increased reading space. Existing controls and their order are preserved.

## Native policy

`WindowSizeBounds` converts client DIP into physical pixels with upward rounding and restored native frame size. `WindowSizePolicy` reads current HWND DPI and uses `AdjustWindowRectExForDpi` with transient maximize/minimize style bits removed. It sets the SDK 1.7 overlapped presenter's native minimum width/height; ordinary resize events do not trigger correction resizes. Display/work-area and XamlRoot changes refresh the environment. A changed environment fits the restored rectangle to the nearest work area once while preserving maximized/minimized state.

When the work area cannot contain the nominal outer minimum, effective dimensions are capped independently to that work area. This prevents a minimum larger than the screen. It does **not** establish comfortable reading or full capability on every small/high-DPI display. Any unsupported layout observations remain limitations or blockers; they are not counted as successful native DPI proof.

## Verification distinctions

`math-checks/matrix.json` covers 35 resolution/scale combinations: 1366 × 768, 1600 × 900, 1920 × 1080/1200, 2560 × 1440/1600, and 3840 × 2160 at 100/125/150/175/200%. It explicitly models a 48-DIP taskbar and scaled restored chrome. This is **simulated geometry**, not native hardware/DPI evidence. Half-screen support is reported independently; emergency work-area capping must not be reported as nominal-minimum support.

Initial full minimum inventory exercised all routes/intakes, available empty tool workspaces, loaded reader/editor/organizer, populated history/preview, and single/multiple result panels. Final frozen DLL gates, Print simulated states, native pointer/keyboard interactions, regression results, and package identity are pending.

The preceding inventory statement records the pre-freeze checkpoint; the final accepted gates and package identity are recorded below.

Print audit uses an already installed virtual driver only for settings/preview; every submission state is replaced by a DEBUG mock. No physical or virtual printer job is authorized or required by this sizing audit.

## Baselines and authorization

Local feed baseline: 0.2.2 / `20261008-084354-201e4764`; installed user pointer read independently: 0.2.0 / `20261007-141314-96a5fba4`. Public HTTPS catalog read independently: 0.1.0 / `20261002-194211-38d3de98`. These are separate states.

Human approval for intentional 0.2.3 metadata is preserved in `release-approval.json` and PRODUCT_VERSION.md. Safe local package publication and preparation of upload assets are authorized after gates. Installed activation, uploading/Check, remote activation, and Production promotion are not performed by this task. A full server activation archive requires fresh verified VPS baseline and reviewed website content; historical script defaults/pins must not be reused as current evidence.

Agent execution: one executor (task04); no specialist/subagent waves. Central phase state/register/log are unchanged because this is a bounded development task, not phase execution.

## Frozen verification checkpoint — 9 October 2026

The normal analyzer-enabled Rebuild produced product 0.2.3 with Desktop DLL SHA256 `053781FA261C1DD8873748AE8CE0390CA1E92BC640AB94AB1BB699E3F833E9EF`. The source manifest SHA256 is `02D94048DFF0EFD9EAA5AC49DE66918DE024EA6E0F05F430E8B3731B720D4A6B`. `reuse-certified-build.json` binds the 390 clean payloads to the native-tested copy; it is not a release acceptance certificate.

The dedicated frozen native audit completed with **107 PASS / 0 FAIL**, including loaded reader/search, editor draft preservation, result panels, history, organizer, route/intake inventory, and six mocked Print states. Evidence: `native-tested/window-size-audit/audit.txt` and `observations.json`. The first attempt is retained in `window-size-audit-attempt1`: its final visual checkpoint timed out, so that attempt is not reported as passing.

Requests one physical pixel below, exactly at, and one pixel above the native outer minimum were checked. Eight inward pointer drags exercised four borders and four corners. The first width/height shrink reached the minimum; subsequent shrink attempts stayed there. Native maximize, restore, minimize, and restoration preserved the synthetic unsaved draft and selection. No outward pointer drag is claimed. Actual left half-screen Snap was observed at 960 × 1032 visible pixels. Actual right half-screen Snap remains unproved; simulation is not substituted for it.

Independent checks passed: minimum geometry **52**, release notes **44**, product version **25**, and legacy updater compatibility **13**. Their reports are under this evidence root. Eight native regression gates now passed on the same frozen DLL: startup 15, UI regression 2718, reader 479, read previews 73, zoom 63, zoom smoothness 53, responsive 462, and responsive dialogs 27; total **3890 PASS / 0 FAIL**, recorded in `native-commands.json`. `native-proof.json` deliberately retains `Pass: false` until actual right Snap is proved.

Task05 released its capture batch after its computer-use runtime failed before any input/process launch; no new documentation screenshots were produced. Task04 reclaimed the native slot and completed the eight regressions. A persistent synthetic preview now allows the missing right Snap observation without rerunning the entire minimum audit. The native Snap Layout popup appeared, but the helper rejected a click because the target was `explorer.exe` PopupHost rather than OTO, including one refresh/retry. The popup is absent from returned targetable windows. Human manual right Snap was requested; Windows-key automation remains prohibited by the computer-use skill. No passing Snap evidence is inferred from a highlighted layout cell.

Fresh read-only VPS evidence on 9 October confirms active/enabled service, unchanged unit/Nginx pins, and public product 0.1.0/build `20261002-194211-38d3de98`. The active symlink text is `/opt/oto-downloads/oto-update-20261002-194211-38d3de98/web`, replacing the obsolete historical baseline path. Regular-operator `readlink -f` did not return a path; this is recorded rather than interpreted as a successful resolved-path check. `server-baseline-20261009.json` binds these observations. No upload, sudo, server change, local feed publication, or installed activation occurred.

## Accepted native minimum and delivery — 9 October 2026

The human manually placed the persistent synthetic preview into right Snap. Fresh native capture verified 960 × 1032 visible pixels at screen origin 960,0; `final-snap-right-native.jpg` is clean synthetic reader evidence. The actor is explicitly human, with task04 performing verification. `native-proof.json` now has `Pass: true` and both actual half-screen Snap sides. This acceptance is for the observed 1920 × 1080/100% environment; the 35-case support matrix remains simulated.

The existing local delivery pipeline completed successfully using the exact already analyzed/Rebuilt/native-tested payloads through the hash-enforcing `Verified-MSBuild.ps1` adapter. It retained its mandatory release notes, product version, inert package, distribution, PDF worker 41, editor 102, search 67, Word 27, native Word 39, native quality 39, and PDF opener gates. No renderer/editor/Print pipeline source was changed. `local-delivery-run.txt` and `prepublication-gate.json` record the command and source/copy binding.

Local release **0.2.3 / Pengembangan**, immutable build **20261009-035333-e218f12d**, contains **392 files**. Manifest SHA256: `76065EE2DD5BE1B7AFD8CB9396968B5148BCBB923D5B229E9ED26CB726F874D0`. Packaged Desktop DLL equals the frozen native-tested hash above. The prior 0.2.2 manifest remains unchanged. The final read-only installed pointer observes build `20261008-084354-201e4764` (0.2.2), differing from the initial 0.2.0 baseline; its change actor is not established by this task. Task04 did not run user installation/activation of 0.2.3. `local-release-handoff.json` and `final-preservation-check.json` provide the evidence. Local availability does not imply installed activation or human acceptance of the resulting application.

Final upload assets are under `upload-assets-20261009-035333-e218f12d`:

| Asset | Bytes | SHA256 |
|---|---:|---|
| ZIP | 38268555 | `2D93DB74B702EE966B689B2C4EFACA7C3112925E4C3317612EA3C3CA5D013872` |
| EXE | 38392832 | `8342E3EAC0C54555C32B01A38BC6F113A1DEBAA98666F08D04374454B530F392` |

Full ZIP verification passed 2 checks, including every manifest payload hash. The final ZIP bootstrapped an **isolated evidence installation** and passed 7 checks, including actual native startup/render/reader 15 checks and unchanged 392 payloads after startup; no user installation was modified. Evidence path is in `bootstrap-evidence-path.txt`.

Installer self-test passed 54 and native wizard audit passed 26745 checks. Wizard work-area/scale/text variants are simulated; no physical DPI/UAC/runtime install/application install occurred. The standard setup script passed these gates, then stopped because its requested final output already contained the immutable ZIP. `Finish-TestedSetup.ps1` recovered only final copying/catalog generation into a fresh folder, checked the existing gate reports, reverified the embedded core installer, and confirmed the embedded DLL against the native-tested hash. It reused the exact tested EXE; no test was weakened or failing binary published. `setup-finish-evidence.json` retains this recovery history.

The upload asset inventory verifies 14 local files, including ZIP, EXE, catalog, feed manifest, known_hosts, fresh read-only baseline, and unchanged existing operator helper sources. `Prepare-OperatorBundle.ps1` prepares a full archive locally only after a reviewed website folder is supplied. **The full server activation archive remains pending** because no reviewed website snapshot for this delivery has been accepted and the current operator cannot read active web file hashes. The historical website is not silently reused. `OPERATOR.md` distinguishes local VerifyOnly, uploading Check, privileged Activate, and rollback. None of Check/upload/Activate/Rollback/sudo was executed; public catalog remains the observed 0.1.0. Task05's broader screenshots/PPT remain its separate pending scope.

Supplementary capture `final-reader-maximized-clean-native.jpg` provides an actual clean full-window reader at 1920 × 1032, origin 0,0, from the same frozen DLL. It contains the synthetic three-page report and no personal folder field. `clean-max-capture-provenance.json` binds hash, process, geometry and fixture. This supplements task05's minimum/right-Snap images; it does not represent completion of its full feature inventory or PPT.

## Authorized server publication checkpoint — 9 October 2026

Human directly requested: “upload dan aktifkan di server dan perbarui di website.” This supersedes the prior local-only upload HOLD for this exact development release. Target is the existing `oto.ayani.id` download portal/feed, product 0.2.3/build `20261009-035333-e218f12d`; it is not a Phase 11 Production sign-off or user installation instruction.

Fresh server preflight confirms old public build `20261002-194211-38d3de98`, exact previous web symlink, active/enabled service and unchanged unit/Nginx hashes. Sudo noninteractive authorization is unavailable (`sudo: a password is required`). New web-only publish of the existing portal source passed 35 regression checks and 8 checks against the real 0.2.3 catalog. All 22 static assets match the previously reviewed guide; no redesign or app source change was made. Helper tests passed 20; the first restricted fixture run failed due to sandbox temp permissions and the elevated fixture retry passed without modifying tests.

`server-publication-20261009/deployment` contains the full immutable archive, metadata, strict ED25519 known_hosts and unchanged canonical activation/rollback helpers. Actual inventory verification covers 33 payloads. Archive SHA256 `08b68541baf634c1cd7cdbe587fa61436d94c802d64e921dcaecc33cf27e2f7c`; metadata SHA256 `831590dbccbcba6923034391ed2d05f7dcb8d4855ce3af0c81b82191d781dbc9`.

Initial scp/check execution ended with a stdout logging path error after the helper changed its working directory, leaving a partial archive. The 28999680-byte remote prefix matched the exact local prefix SHA256 `485599c56c5895e726bc67b7a1592d92ef622d60de214c1da01eeb7508955bbc`. Existing OpenSSH SFTP resumed those bytes; strict host checking was preserved. The complete remote archive, unpack control, bounded extraction, activation helper and exact inventory subsequently passed. Evidence: `staging-recovery.json`, `resumed-upload.txt`, `staging-remote-proof.txt`. No partial archive was activated.

Verified private remote stage: `/home/aynadmin/oto-updates/oto-update-20261009-035333-e218f12d-08b68541baf6`. `Launch-ActivationTerminal.ps1` checks exact local pins and actual remote staging evidence before opening a visible PowerShell with the canonical pinned SSH sudo helper command. It does not change the Restricted execution policy or record the password. Terminal PID3500 and its SSH child were observed alive. The operator must enter sudo password directly in that terminal. Public HTTPS still reported 0.1.0 at the last check; therefore no activation success is claimed.

After the terminal completes, `Verify-Public.ps1` must pass against the exact 0.2.3 catalog, homepage/guide, complete ZIP/EXE SHA256, HEAD/range behavior and all published static assets. Existing helper automatically restores old web/catalog if post-switch validation fails; explicit rollback remains available for the exact successful publication. Publication state and pending operator step: `server-publication-20261009/publication-state.json`. No user-installed activation occurred.

## Publication completed — 9 October 2026

Development version 0.2.3, build `20261009-035333-e218f12d`, was activated by the human operator through the pinned canonical helper. Independent verification passed 65 public HTTPS checks, including complete ZIP and EXE SHA256, exact release metadata, website/download notes, guide, and static assets. Service is active/enabled at the new release symlink; service unit and Nginx hashes are unchanged. Prior release is retained. Evidence: `artifacts/window-minimum-20261008/server-publication-20261009/publication-state.json`, `post-activation-service.txt`, and `public-verification.txt`.

Initial static verification incorrectly expected `.br` internal sidecars to be public. Corrected verification confirms their 404 responses and exact bytes for served assets, including `.gz`. Initial failure is retained; previously successful full artifact hashes were reused with fresh matching metadata. No website/server configuration change was needed. Earlier pending-activation checkpoints are historical and superseded by this result. Task04 did not install or activate 0.2.3 on the user's computer. Publication is a development release and does not establish Production phase approval.
