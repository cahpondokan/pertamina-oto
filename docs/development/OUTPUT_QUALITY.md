# Raster output quality — local development handoff

Task: requested quality policy, 1 October 2026. Baseline HEAD `84cbb0f538020003bcfb6583e94c2fcf6524f87c`, existing dirty `core-functional-first` worktree retained. No Git merge, commit, dependency installation, icon change, reader scroll change, or phase gate change. Scroll chat was read-only checked and completed before shared source edits/builds.

## Active behavior

- Gambar ke PDF and PDF ke JPG have three adjacent toggle segments Rendah / Sedang / Tinggi; default Sedang on opening the workspace. Selection is exclusive, cannot become empty, and checked state updates the processing request. Keyboard focus and accessible names are provided. No volume slider.
- Raster presets: low = longest edge capped at 1200 px / JPEG quality .55; medium = 2400 / .80; high = 3600 / .95. Image inputs never upscale; JPG page rendering uses at most 1/2/3 pixels per PDF point, respectively, within edge caps. JPEG encoding is explicit after lossless PNG page rendering, avoiding double lossy encoding. Rotation is retained.
- Gambar ke PDF offers Asli, passing original images directly to the existing worker without re-encoding. Raster presets flatten transparency against white and disclose it. Source file read locks remain held through worker processing/publication. Temporary converted images are bounded and removed in finally.
- PDF ke JPG labels its setting Kualitas JPG and discloses that JPG does not contain selectable PDF text, links or forms. PDF source is not rewritten.
- Compression audit: current PDFsharp path sets `CompressContentStreams = true`, then saves and reopens; it does not downsample/re-encode embedded images. UI retains Kualitas asli / lossless and explains size can stay unchanged or increase. No false preset controls or whole-PDF rasterization were added. Existing limitations for forms, bookmarks, links and digital signatures remain; these documents are not newly certified by this task.
- Structural/security/edit operations use protocol default `Quality = original`; worker rejects unsupported non-original requests. No implicit Compress result pipeline was added.
- Existing output folder/unique-name publication and source overwrite refusal are retained.

## Reusable contract, inactive scope

`OutputQualityProfile.For` and the `PdfRequest.Quality` field provide a raster-only preset contract. The desktop image pipeline resolves presets, supplies converted staging inputs and sends `original` to the PDF worker; the worker never pretends it applied a preset itself. Unknown presets fail closed. Old callers default to original.

Scan and Office/HTML conversion remain inactive. OCR language/accuracy, PDF-to-Office layout/editability, and PDF/A conformance/validation require separate settings; this contract does not enable or represent those capabilities. Lossy PDF compression is not implemented/admitted. A future implementation needs a bounded engine strategy, dependency admission if applicable, semantic/security preservation tests and appropriate authority; flattening arbitrary PDFs is not an acceptable substitute.

## Changed paths owned by this task

- Desktop: `MainWindow.cs`, `Program.cs`, `SimpleJpgWorkspace.cs`, `PdfJpgExport.cs`, `ToolCatalog.cs`, new `OutputQuality.cs`, new `OutputQualityAudit.cs`.
- Worker: appended quality protocol field in `Protocol.cs`; unsupported quality guard in `PdfOperations.cs`.
- Delivery: `eng/Build-LocalUpdate.ps1` now gates publication on successful quality process exit, at least 30 PASS checks and no FAIL checks. Publication itself was blocked before script execution.
- This handoff document. All other pre-existing dirty changes are outside this task's ownership.

## Verification

Final clean Rebuild command: `D:/DevTools/VS2026Community/MSBuild/Current/Bin/MSBuild.exe src/Hosts/Pertamina.Oto.Desktop.App/Pertamina.Oto.Desktop.App.csproj /t:Rebuild /p:Configuration=Debug /p:DesktopUiOutputPath=<worktree>/artifacts/quality-final/ /v:minimal /nologo` — exit 0.

Final build worker `--self-test`: 41 PASS, exit 0; `--self-test-editor`: 63 PASS, exit 0; `--self-test-search`: 25 PASS, exit 0. Fixture evidence directories reported by these commands: `%TEMP%/oto-functional-test-944dfd64c9ee4f9196b946a9761d20a7`, `%TEMP%/oto-editor-test-8fb1bec5f3c94bf6b34e4d320442701b`.

Final desktop `--audit-quality`: process exit 0, 39 PASS, no FAIL. Evidence: `artifacts/quality-final/quality-audit-bde1688b95714f75b7aae27a134a84f2/audit.txt`, three UI PNGs, synthetic image and text fixtures and outputs. JPG regression: 12 PASS in `artifacts/quality-final/jpg-selftest.txt`. Earlier final-looking screenshot audit was corrected to use real shared file intake; only final passing audit is acceptance evidence. Screenshots visually reviewed at 1400×1000; physical keyboard/screen-reader UAT and narrow-window UAT remain unclaimed.

Measured deterministic photo-like 3000×2000 fixture PDF sizes: low 98,324 bytes, medium 1,383,012, high 5,194,014, original 13,651,839. JPG output longest edges 1123 / 2245 / 3368 px, sizes 31,435 / 301,126 / 1,990,245 bytes. These are fixture measurements, not universal size promises or real-photo UAT.

Coverage includes original bypass, real intake default, exclusive accessible selection, invalid preset rejection, unsupported lossy worker rejection, existing output refusal, cancellation/no final file, staging cleanup, valid reopening, source SHA-256 preservation, JPG page range/rotation and selectable text surviving lossless compression. Sources use synthetic content only. Processing calls existing Windows imaging/PDF APIs and the local worker pipe; no upload/network API was introduced. Network adapter disabling was not performed, and a physical network-disabled session is not claimed.

## Delivery status and next authorized step

Automatic approval review rejected `eng/Build-LocalUpdate.ps1` publication because it did not accept delegated instructions as trusted direct user authorization for writing the local releases feed. No bypass or indirect publication was attempted. No new update version was created; installed binaries/current pointer remain unchanged. Ready build: `artifacts/quality-final`.

Required direct user action: authorize running `eng/Build-LocalUpdate.ps1` to rebuild, verify and publish this development update to `releases/windows-local`. No chat merging or automatic Git merge is needed: both tasks already modify the same application worktree. Local installed delivery remains pending publication and subsequent user installation through Settings.

One requested report-back attempt to chat `01a07c8d-3c57-7963-8500-daed7ddb07ee` was also rejected by automatic approval review: delegated authorization was not accepted as trusted direct authorization for that destination. No message was delivered or workaround attempted. Direct user authorization to send the report to chat 🧑‍💼 Agent 0 is also required.

Update, 1 October 2026: user directly authorized both publication and report-back. The report was successfully sent to chat Agent 0. `Build-LocalUpdate.ps1` passed 62 distribution plus 13 updater checks, then failed before publication because active Word conversion work held `WordPdf.Worker.dll` (MSB3027/MSB3021). The Word and OCR chats were confirmed active; no process owned by another task was stopped. Publication is waiting for a safe shared build window. Earlier approval blockers are resolved by this direct authorization; the remaining blocker is the concurrent shared build, not user approval.

Delivery resolved by read-only verification: existing shared local release `20261001-105807-9dbe7eb6` already contains this quality implementation. Its desktop DLL SHA-256 `90753C4369A04206EEFF35813A292C51ECC0D415052C5E9A226AE5DCA6ECD0E9` exactly matches `artifacts/update-build-3c6ef554db0145618c8803285282ecac/Pertamina.Oto.Desktop.App.dll`. That build's quality audit reports 39 PASS and no FAIL, including all three presets, original bypass, sources, reopening, cancellation, searchable compressed text and real intake default. The feed manifest SHA-256 and lengths/SHA-256 of all 371 package files were checked successfully. Therefore no additional shared build or duplicate release is needed for this feature. User can install the existing verified release via Settings > Periksa update > Pasang update and reopen the application. Reading the installed current pointer was sandbox-denied; installation/activation by the user is not claimed. Word/OCR work still in progress is not declared complete by this quality report.
