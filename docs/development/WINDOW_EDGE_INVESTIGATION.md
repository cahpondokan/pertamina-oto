# Window edge discrepancy — investigation, 7 October 2026

Status: NEEDS_REPRODUCTION. Human authorized investigating/fixing the title-bar/content right-edge discrepancy and reporting to Agent 0. No application source, release feed, installed pointer or phase records changed. One executor, no subagents.

Input: cropped screenshot codex-clipboard-670b86ec-d4b4-47a9-85ef-7f098e04e429.png. Caption extends approximately 8 physical pixels farther right than blank white client content. Crop omits application identity, entire window, window state and timing; attribution to a particular OTO route is unconfirmed.

Observed installed version: 20261007-141314-96a5fba4, product 0.2.0, manifest A982FA6D7BA69455D9318D9ABDD769F18E187FB27BA183F7D607553080A5B474. Native computer-use inspection after explicitly activating the installed OTO window showed aligned caption/content edges on the populated restored window. The first capture returned the foreground Codex image instead of the selected target; it was discarded as target evidence. No Codex UI input performed.

Ran existing --audit-startup-window on the isolated hash-certified Print test copy with process-local OTO_STARTUP_WINDOW_VISUAL=1. Completed 15 PASS / 0 FAIL; report copied to artifacts/window-edge-20261007/startup-audit.txt. Audit checks maximization, caption/border/control availability, restore/resize, state preservation across navigation/dialog/document, minimize/restore. Its RenderTargetBitmap images cover client content only and do not prove caption alignment. Native capture during the isolated audit could not reliably distinguish the same-title foreground installed window; no whole-frame claim is made for that audit. Isolated audit exited normally; installed user window retained.

Source inspection: default native caption, no custom title-bar extension or nonclient width override. Root/shell uses stretch layout. MainWindow initially resizes during construction, startup prepares/maximizes before activation, and several DEBUG audits subsequently resize the native window or constrain root dimensions for simulated content cases. These are investigation leads, not demonstrated causes.

Do not implement a hardcoded 8-pixel margin or change maximize/restore behavior based solely on the crop. Microsoft documents that native maximized window bounds can include offscreen borders: https://devblogs.microsoft.com/oldnewthing/20150304-00/?p=44543%2F . This does not establish the cause of this user's screenshot.

Pending user clarification: discrepancy only on transient blank audit windows, or persistent on the populated OTO page? For persistent occurrence, obtain a full-window screenshot and launch/resize steps, then reproduce on an isolated build before fixing and publishing a local update. Current diagnosis is incomplete; do not mark the visual defect fixed or issue a replacement package yet.
