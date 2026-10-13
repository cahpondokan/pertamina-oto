# Default PDF opener from Settings

30 September 2026 — user requested an in-app setting to use PERTAMINA OTO as the default PDF reader, with a Windows Open With screenshot showing OTO absent. Agent 0 implemented this scoped local development feature. Phase gates are unchanged.

## User flow

1. Apply the local update through **Pengaturan → Periksa update → Pasang update**, save work, and reopen the stable desktop shortcut.
2. Select **Pengaturan → Pembuka PDF default → Jadikan default**.
3. In the Windows page, choose **.pdf → PERTAMINA OTO → Tetapkan default / Selalu**. If Windows shows the general default-app page, search for PERTAMINA OTO or .pdf. The registered application is also available through **Buka dengan**.
4. Returning to OTO refreshes the status from the actual Windows association. Opening a local PDF through the registered handler routes directly to Pembaca PDF.

The action requires an existing local installation. Merely visiting Settings or installing an update does not register or change the default. Windows owns the final choice. If registration succeeds but the Settings page cannot open, OTO gives manual navigation instructions without claiming registration failed.

## Implementation

- `PdfDefaultSettings.cs` adds the accessible button, progress/result status and activation refresh. `PdfAssociation.cs` reads effective association through `AssocQueryStringW(ASSOCSTR_PROGID)`.
- Explicit click writes only per-user HKCU registration: OTO ProgID, Applications entry, `.pdf/OpenWithProgids` membership and RegisteredApplications/Capabilities. No HKLM, UserChoice, protected hashes, other applications' entries or extension default value are written. Shell notification refreshes application registration.
- The existing installed desktop launcher remains intact. The packaged `pdf-opener` helper is copied on the explicit settings action to `%LOCALAPPDATA%/Programs/PertaminaOTO/pdf-open/<content-hash>/`. Payloads are verified and immutable; existing files are never overwritten, and old helper directories remain. A single-writer lock guards helper deployment and local update path validation rejects reparse/traversal paths.
- Registered open command quotes the helper and `%1`. The helper resolves the validated `current.json` version each time and launches that version with separate `ArgumentList` entries `--open-reader` and the PDF path. It continues to follow future local updates without a version-pinned desktop application path.
- Accepts exactly one existing absolute local `.pdf`; rejects UNC/network URLs, relative paths, alternate streams, embedded quotes and extra arguments. No new package or runtime dependency was acquired; the helper uses the existing .NET 10 runtime. `Program.cs` also accepts a single direct absolute `.pdf` argument.
- Source PDF content is not changed by activation. Existing reader size/page/password/error handling applies. This helper does not add printing, OCR, multiple-document tabs or network-file support.

## Verification

- Final desktop build exit 0. Native `--audit-reader-ui`: **44 PASS**, including settings button/status, search regression and direct opening of a synthetic PDF with spaces/Unicode/shell characters. Evidence: `artifacts/desktop-pdf-default/reader-ui-audit-b4e1bcc68f354f27bbfbca4cdda6a41c/audit.txt`. Settings screenshots `07-settings-default-pdf.png` and `08-settings-default-pdf-compact.png` were visually inspected from the preceding equivalent-layout audit `reader-ui-audit-a742034544d9472390ba631a8ee3a8f3`.
- Helper `--self-test`: **41 PASS** initially at `artifacts/pdf-association-verification/pdf-association-0076c8756a8e42ddbc60820bb8c2b22a/audit.txt`. Tests round-trip real registry values only under a fresh inert GUID subtree below `HKCU/Software/PertaminaOTO/AssociationTests`, then remove only that subtree. They verify quoted commands, per-user capabilities, REG_NONE membership, unrelated-entry preservation, pointer updates, input rejection, helper integrity and source preservation. Fixture executables are inert and never executed.
- `Build-LocalUpdate.ps1` now gates publication on the helper suite as well as worker operations, editor and search regressions. Package build evidence: `artifacts/pdf-association-verification/package-build.txt`.
- No live PDF registration/default was changed during testing. End-to-end Windows selection and Explorer double-click with the installed version remain manual UAT. The native audit tests the reader destination and the helper tests activation command construction; neither is presented as a completed physical Windows click-through.

## Microsoft platform references

- [Default apps platform](https://learn.microsoft.com/en-us/windows/apps/develop/windows-integration/default-apps-platform): default selection belongs to the system UI.
- [Launch default apps settings](https://learn.microsoft.com/en-us/windows/apps/develop/launch/launch-default-apps-settings): per-user `registeredAppUser` deep link; generic page fallback.
- [Application registration](https://learn.microsoft.com/en-us/windows/win32/shell/app-registration): application and Open With registration.
- [AssocQueryStringW](https://learn.microsoft.com/en-us/windows/win32/api/shlwapi/nf-shlwapi-assocquerystringw): effective association lookup.

## Delivery

Published local development update **20260930-141956-a5a4707b**, 368 manifest entries. Standard clean rebuild and helper41/worker41/editor63/search25 checks passed; with the final native reader44 audit, 214 checks passed. Final helper report: `artifacts/pdf-association-verification/pdf-association-e957130744cd44dca1273fdbc2df6790/audit.txt`.

Read-only package verification exit 0: manifest SHA-256 `5A73503D305F8001E0492D8B6F9FDA30A005F3B17A73BD62FA7761019708F2FB`; all 368 file lengths/hashes match, zero failures. Desktop, worker and PDF helper DLLs are byte-identical to the tested build. Evidence: `artifacts/pdf-association-verification/package-integrity.txt`. Previous feed `20260930-135728-08b297b4` remains retained.

Running/installed versions and the stable shortcut were not changed by publishing this local update. Installation and the final Windows default selection remain user actions. This is not a Production release or a phase exit approval.
