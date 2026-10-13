# Compact favorites — 2026-09-09

User approved the compact list mockup. Agent0 changed only ShowFavorites in WorkspaceScreens.cs and documentation. Existing working changes preserved; no engine/dependency/availability change.

One52px-minimum row per favorite, small icon, Indonesian name, category and open cue, independent accessible star removal button. A single Add favorites button opens catalog. Maximum list width850px; category hidden below520px row width. Empty list guidance and session-only persistence disclosure. Opening uses OpenHomeTool to retain checked home inputs; removal affects only favorites, not files or installed tools. Tool descriptions available as tooltips.

Offline VS MSBuild Development.slnx Debug with DesktopUiOutputPath=<worktree>/artifacts/desktop-favorites/ completed exit0. git diff --check exit0 (existing lock-file line-ending warning). No GUI runtime acceptance performed; empty/nonempty/remove-last/add/open/resize and keyboard behavior remain manual checks. Earlier crash report unresolved. Native small icons use existing icon font, not pixel-identical generated artwork. No engine tests rerun, new cost, main promotion or phase completion.
