# PDF default application — scoped task card

- Agent 0 only; user asks for an in-app setting to use PERTAMINA OTO as the default PDF opener.
- Baseline: existing dirty `worktrees/core-functional-first`, `task/core-functional-first` at `84cbb0f`; local update `20260930-135728-08b297b4`. Preserve previous changes and stable desktop launcher.
- Inputs: shared contract, state/decisions, LOCAL_UPDATES, SettingsWorkspace, reader startup, local launcher and update implementation; Microsoft default-app platform, registration and settings URI documentation.
- Allowed paths: settings/association/startup implementation, new PDF opener helper and focused tests, desktop project/build packaging, task-specific artifacts, documentation/coordination and verified local development feed.
- Do not touch: Windows UserChoice/default hashes or other apps' registrations, HKLM, documents, existing installed launcher, production/dependency admission/phase gates.
- Design: explicit settings click registers OTO for .pdf under HKCU, then opens the documented Windows default-app settings page. Windows UI owns final default choice. A small immutable per-user helper resolves current.json on every open and passes the PDF to --open-reader via ArgumentList. Deploy helper on settings click; do not pin association to an obsolete application version.
- Acceptance: setting with clear instructions and accurate status; Open With registration plan covers .pdf; spaces/Unicode paths routed safely; helper follows updated current pointer; clear error on missing/unsupported input; existing reader/search behavior retained; no automatic default takeover.
- Verification: compile; helper dispatch/validation and isolated registry registration tests, no live default mutations; native settings/reader audits and visual captures; existing regression/build package gates. Manual Windows selection remains user action.
- Stop: new dependency, unsafe overwrite, unexpected ownership conflict, test failure, or unavailable explicit authority for an external mutation. No Phase N execution or Production claim.
- Delivery: existing local update flow after verification; installed/running version and shortcut preserved. User uses Settings to perform registration/default selection after applying update.

## Completion evidence

- Implementation and package prepared: local development update `20260930-141956-a5a4707b`.
- Native reader44/helper41/worker41/editor63/search25 PASS; total214. Manifest and368/368 payload lengths/hashes PASS; desktop/worker/helper DLLs match the tested build.
- Details, source paths, evidence and limitations: `worktrees/core-functional-first/docs/development/PDF_DEFAULT_APP.md`.
- Final Windows default selection and installed Explorer click-through remain user action/manual UAT. No live default or installed version was changed during this task; no phase exit is claimed.
