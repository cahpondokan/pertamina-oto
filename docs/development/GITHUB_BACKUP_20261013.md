# Private project backup — 13 October 2026

User requested GitHub backup before broader disk cleanup. Destination is the existing private repository `cahpondokan/pertamina-oto`. This snapshot preserves the current development checkout without promotion to main or a Production claim.

## Contents and recovery

- Repository root: current source, tests, build scripts, admitted dependency lockfiles and development documentation.
- `workspace-context/`: authoritative parent-workspace Markdown documents, phase prompts/state/decision history, and AGENTS instructions. These retain their original content; this folder is context, not a replacement for repository-root instructions.
- `assets/biblo-reference/`: the complete existing `output/imagegen/` collections. Preserve these originals for future UI work.
- `backup-evidence/`: final package manifest, verification summaries, exact build/source identities and relevant physical audit results.
- A private draft GitHub Release stores the final Windows local package separately from Git source history. The uploaded ZIP must be verified against its local SHA256 before calling the binary backup complete.

Final product is 0.3.0 / Pengembangan, build `20261013-114257-465ae8c8`. Manifest SHA256 is `75AF3F6C879A33DA3CBE01EA268BFCBE0D5C65A6BF63B0182A3AF6B76970FB84`. Installed selection remains independent; no installation is performed by backup.

The snapshot uses a temporary Git index and a separate backup reference. It does not stage or discard the developer's current index, change the development branch, reset files, merge into a protected branch, or alter repository visibility. Keep the working source checkout and installed application locally for continued Windows development; GitHub is the off-device recovery copy.

## Cleanup boundary

After remote source and binary verification, obsolete local release folders can be proposed for deletion while preserving the final build and `20261013-002005-94b886e8` as rollback. Temporary publish output and build intermediates can be regenerated. Do not delete source, Git history, project decisions, admitted dependency/license evidence, original Biblo assets, user documents, or an entire dirty worktree. Destructive cleanup requires approval for its exact path list.

No claim of complete backup is valid until the remote commit, Biblo tree and uploaded package asset have been verified. Completion evidence is recorded separately in the local backup report.
