# Local development delivery

Parent workspace AGENTS.md remains authoritative; this file adds only the user's local delivery preference of 28 September 2026.

- The user wants one stable PERTAMINA OTO Desktop shortcut and local in-app updates, not another separately launched artifacts executable after every change.
- After a requested application change is implemented and relevant verification passes, prepare its local update with `eng/Build-LocalUpdate.ps1`. Read `docs/development/LOCAL_UPDATES.md` before deployment/update work.
- Feed is workspace-root `releases/windows-local`; this is local development delivery only. Do not publish a failing/unverified build, add network distribution, claim Production readiness, or change phase gates.
- Do not overwrite the installed running version, delete versions, overwrite user data, or force-close unsaved work. Keep the stable launcher/desktop shortcut intact. Installer runs once; subsequent updates use Settings > Periksa update.
- Report the published version and actual tests. If packaging fails, report that the installed version remains unchanged rather than claiming the user received the change.
