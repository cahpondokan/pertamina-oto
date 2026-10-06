# PDF opener failure diagnosis and repair — 4 October 2026

User authorized diagnosis and repair in this chat. Scoped helper changes only; no phase execution, default takeover, installation, registry repair, user-document access or forced application close.

## Evidence

The exact screenshot string was the catch-all message in eng/PdfFileLauncher/Program.cs. The reader landing task's READER_LANDING_DEFAULT_APP.md records a malformed isolated helper invocation: --self-test without its required evidence argument reached normal PDF argument rejection and displayed that same dialog. This is a strong explanation for the observation, but the human screenshot trigger has not been independently confirmed.

Read-only installed audit found a valid current pointer 20261002-194211-38d3de98, existing target executable and registered helper, correctly quoted open command, correct stable shortcut, and all four installed launcher hashes matching restart-capability.json. The registered September 30 helper differs from the installed app's helper; that alone is not proof of failure. Registry UserChoice read was MSEdgePDF while the other task's effective association query reported OTO; do not equate those observations or change defaults based on them. No running OTO executable was returned by the targeted process snapshot. Existing reader-open FileNotFoundException log events are separate and cannot establish the helper dialog's cause.

## Repair

- Malformed --self-test exits 2 without a user PDF dialog; valid tests keep their isolated route.
- PdfOpenCommand reports input/file/installation validation stages; executable start has a launch stage. No fallback to previous versions, network locations, or unsafe arguments.
- Distinct actionable messages include stage and HRESULT, without exception messages or document names. Missing/unavailable file wording reflects File.Exists uncertainty rather than claiming deletion.
- Best-effort bounded pdf-opener.log records timestamp, stage, build module ID, exception type and HRESULT only. Logging failure does not hide the original error. Successful Process.Start does not prove subsequent application startup/read success.

## Verification

dotnet build eng/PdfFileLauncher/PdfFileLauncher.csproj --no-restore -o artifacts/pdf-opener-repair/build: exit 0, zero warnings/errors. Malformed self-test native process returned 2 promptly (no dialog blocking). Correct native self-test returned 0: 46 PASS, under artifacts/pdf-opener-repair/tests/pdf-association-*/audit.txt. Fixtures test missing PDF, malformed pointer, missing application and distinct private-data-safe messages. Tests use inert executables and a fresh isolated registry subtree, not live defaults. No user PDFs were opened. Initial build failed on CA1861; fixed with static stage array, then rerun successfully.

Standard local update gate log: artifacts/pdf-opener-repair/package-build.log. Delivery result must be read from that log/latest.json; a build artifact alone does not change the installed helper. Existing per-user registration switches helpers only on the user's explicit registration action; no live registration maintenance was performed here.

Final standard gate completed successfully (exit 0). Local development feed now selects 20261004-061343-e4e4827c, 389 payload files. Tested desktop build: artifacts/update-build-7695db84f96142ac9db449413c02fa54. This is local package publication only, not an installed update or public/Production release. User applies the package through Settings; an existing immutable registered helper requires the explicit PDF registration action to select the newly packaged helper. Windows final default choice remains unchanged by this work.
