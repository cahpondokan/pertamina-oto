# Unified result panel — 2026-09-16

All active success routes now use UnifiedResult: generic PDF tools and Merge, dedicated Remove/Extract/Organizer, Split batch, and JPG. White compact bordered panel, green success heading, filename summary, adjacent open-file/open-folder actions, Back to edit and Start new, abbreviated folder action and expandable selectable details/copy summary. Multiple files use a scrollable list with per-file open actions and primary open-folder action.

Back restores retained editor controls rather than reimporting documents. Password fields remain cleared by existing executors. Start new uses the existing confirmation; disk files are not deleted. Failed exports do not call the success panel. Shell launch failures are reported and logged without document contents.

Build artifacts/desktop-unified-results: VS2026 MSBuild Debug passed; diff whitespace check passed. Interactive visual acceptance and narrow-window behavior still require direct testing. No claim that the generated mockup has been matched pixel-for-pixel; no production or phase gate change.
