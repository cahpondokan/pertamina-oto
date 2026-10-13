# Stability hardening — 2026-09-16

Windows Application log confirmed three OTO crashes (September 15 15:20/15:24 and September 16 11:41), Microsoft.UI.Xaml.dll, exception 0xc000027b. These identify a crash class, not its precise cause. WER report archive access denied; no crash stack recovered.

Changes: defer native Closing confirmation onto DispatcherQueue and gate repeated requests; catch/log failures at close-document, paste and message-dialog boundaries; register diagnostic observers for XAML, runtime and unobserved-task errors. Unknown global errors are NOT marked handled.

Local diagnostics: LocalApplicationData/PertaminaOTO/Logs/errors.log; rotates at 512 KiB into one previous log. Contains UTC timestamp, fixed operation identifier, build module ID, exception type/HResult and method-only frames. No exception messages, file paths, arguments, document content or passwords. Log writing itself is guarded. Native fail-fast may bypass these observers.

VS2026 MSBuild Debug succeeded; git diff --check succeeded. Build artifacts/desktop-stability. Reproduction of the original crash and interactive close-dialog acceptance remain pending; this is not proof all crashes are fixed. No dependency, OS configuration, phase gate or release changes.
