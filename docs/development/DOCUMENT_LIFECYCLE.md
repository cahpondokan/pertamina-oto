# Document lifecycle update — 2026-09-16

Added shared Close document confirmation and Start new on five PDF result flows. Original and exported files are never deleted. Existing Back to edit retains editor controls; Split now hides its input/options after success and restores them on Back to edit, with selectable output paths and per-file open actions.

Organizer source rows expose an accessible hover/focus remove button. Removing a source removes all its page placements, retains provenance for Undo, hides zero-page source rows, and returns to the empty picker if the layout becomes empty. Undo restores the layout. Export compacts referenced source indices so removed source files are not needed for mixed-source output (blank-only output retains the existing worker source requirement).

Build: artifacts/desktop-lifecycle. VS MSBuild Debug passed. GUI lifecycle acceptance remains pending; build does not prove dialogs, Undo and back navigation interact correctly. No phase gate change, release, dependency install or file deletion.
