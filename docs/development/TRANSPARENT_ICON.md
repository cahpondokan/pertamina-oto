# Transparent vector icon — 1 October 2026

Task: improve existing OTO owl icon clarity and remove solid background. No branding redesign or phase gate changes. Baseline commit: 84cbb0f538020003bcfb6583e94c2fcf6524f87c; existing dirty worktree retained.

Changed files: eng/Build-OtoIcon.ps1; eng/Test-OtoIcon.ps1; eng/Update-LocalShortcutIcon.ps1; src/Hosts/Pertamina.Oto.Desktop.App/Assets/oto.ico; Assets/oto.png; this document.

The existing XAML vector geometry and red/white/blue/green colors are unchanged. Removed navy Canvas background. Render each 16/24/32/48/64/128/256 px frame directly with WPF layout scaling rather than the old VisualBrush intermediary. PNG is the transparent 256 px master export. Existing app project, AppWindow and launcher ApplicationIcon already consume oto.ico; their code required no changes.

Verification:
- Build-OtoIcon.ps1: exit 0.
- Test-OtoIcon.ps1: exit 0; seven correctly sized 32-bit PNG-backed ICO frames, transparent corners, opaque interior and antialiased alpha verified. Evidence: artifacts/icon-audit-results.txt.
- Visually inspected artifacts/icon-audit/light-dark-native-sizes.png and oto.png. Small sizes keep owl silhouette; white OTO shapes inherently have reduced contrast on very light backgrounds. No outline/colors added to avoid redesign.
- LocalLauncher.csproj build: exit 0, artifacts/icon-launcher. Installed launcher binaries were not replaced.
- Build-LocalUpdate.ps1: exit 0; app rebuild, 41 worker checks, 63 editor checks, 25 search tests and isolated PDF opener self-test passed. Evidence: artifacts/icon-update-build.log and artifacts/pdf-association-verification.
- Published version: 20261001-092910-960a246e, 369 files in workspace-root releases/windows-local.
- Packaged ICO hash equals source: 814AC8044849945A34F1AE7AE6DFA297E16CC05CAD19CA6D960D2833AC85E1E0.

Icon-only shortcut maintenance executed with exit 0. Existing Desktop shortcut target unchanged; IconLocation now points to installed oto-icon-814AC8044849945A.ico. Backup of original shortcut saved in install root as shortcut-before-icon-ddff4db4cb614a98b3ae84b7984e8f80.lnk. No duplicate Desktop shortcut, no deleted version, no application closure, no user-data changes. The installed launcher retains its previous embedded icon; Desktop shortcut overrides it. Future launcher builds use the new icon.

Installed application pointer remains 20260930-141956-a5a4707b (read-only audit). User installs application update through Pengaturan > Periksa update > Pasang update, saves drafts, closes normally and reopens the same shortcut. Windows may retain a cached icon until it refreshes/reopens; Explorer was not restarted. Existing pinned shortcuts may retain their own previous icon. No full interactive update UAT claimed.