# Approved home design — 8 September 2026

Agent0, continuation `task/core-functional-first` baseline 84cbb0f + preserved UI-first working changes. User approved generated three-panel home design. Allowed paths: Desktop.App UI, development/coordination notes; no worker, package, main or server changes.

Implemented: pastel blue document card, white search card, pastel purple Assistant; large icons, flat navy navigation, six compact quick tools; three-column desktop and stacked narrow layout. Native vector corrections for compress/scissors/sparkle/stamp, no icon dependency. Clipboard accepts File Explorer file references, not arbitrary text/bitmap. Picker/drop add up to 50 PDF/JPG/PNG files with per-file size filter 100MB and no UNC; paths remain session-only. Choosing a compatible available tool transfers the selection without running it. Single-file operations reject multi-selection. Original files untouched. Assistant is read-only and disabled until a model is admitted.

Search uses a deterministic alias dictionary, not AI/fuzzy search: gabung/gabungkan/satukan/marge/merge, pisah, kecilkan ukuran, putar, etc. Each typed word must occur in name/category/aliases. No arbitrary natural-language understanding claim.

Verification: offline MSBuild Development.slnx Debug with DesktopUiOutputPath pointing to artifacts/desktop-home-v2 PASS (after CA1861 fix); screenshot and accessibility inspection confirmed three panels/controls at desktop size. User interaction prevented automated search test, no claim it passed. Final vector corrections build separately in artifacts/desktop-home-final to avoid overwriting the running app. Final icons/resize/paste/drop end-to-end still need visual/manual verification; no PDF processing tests or new network-disabled proof on this turn. No new cost, package, Sandbox, model or deployment.

Open: pixel-level native hover/disabled styling differs from mockup; full platform accessibility and mobile/Web not part of this checkpoint. No phase completion or Production approval.
