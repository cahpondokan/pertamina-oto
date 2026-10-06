# Global document paste — 2026-09-16

Ctrl+V now routes through a single root keyboard accelerator instead of registering competing accelerators on individual cards/buttons. Edit > Paste uses the same router. Loaded editors use their existing import validation; organizer and merge append, single-file tools use their existing replacement behavior. Non-editor pages route to Home. Text/password focus retains normal text paste. Busy processing, import, dialog and repeated-paste guards prevent conflicting input. Inactive tools remain unavailable.

Build: artifacts/desktop-global-paste/Pertamina.Oto.Desktop.App.exe. VS2026 MSBuild Debug PASS and git diff --check PASS (existing newline warning). Actual Explorer Ctrl+C / Alt+Tab / Ctrl+V GUI acceptance still needs user verification. No upload or new dependency.
