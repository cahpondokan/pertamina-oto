# Global document paste audit — 2026-09-26

Scope: Ctrl+V routing and visible feedback. No processing limits or PDF geometry support changed. Drag/drop remains on its existing surfaces; this increment does not claim global drag/drop support.

## Findings and implementation

The root keyboard accelerator excluded TextBox/PasswordBox focus before checking clipboard content. Thus search/input focus prevented file paste. Busy guards returned without feedback. Editor result presentation hid notices used by its paste handler. Several page files contain unused local accelerator objects; the active global registration is in AddWorkspaceShortcuts.

File paste is now handled on root PreviewKeyDown before child text controls. File clipboard data takes precedence regardless of input focus; ordinary text clipboard data in TextBox/RichEditBox/PasswordBox/NumberBox (including their descendants) remains native. The old active Ctrl+V accelerator is removed to prevent duplicate dispatch. Ctrl+Z/Y behavior is unchanged. Existing modal dialogs retain control.

## Page routing audit (source inspection)

| Page/state | Route and behavior |
|---|---|
| Home | PasteHomeAsync; existing validation, document selection and feedback |
| Catalog, Favorites, History, secured files, assistant, administration, help, settings | Non-tool fallback opens Home and receives supported files; no automatic processing |
| Empty active tool | PasteEmptyToolAsync; existing format/count/size validation |
| Empty unavailable tool | Visible unavailable-tool dialog |
| Merge workspace | PasteMergeAsync; adds documents using existing duplicate/format checks |
| Organizer | PasteOrganizerAsync; existing source/page import |
| Remove / extract workspace | Dedicated paste handler |
| Other active source tools including split, compression, rotation, conversions/security | PasteSourceAsync; existing tool-specific input route |
| Live editor | PasteAsync; removes completed-result overlay so notices are visible; same-file notice; clean-document replacement confirmation; existing dirty-draft confirmation |
| Shared result page | Confirmation before returning to Home; saved output untouched |
| Active worker/import/paste/editor work | Visible wait-or-cancel dialog instead of silent rejection |
| No file clipboard outside typing | Visible clipboard guidance |

No user clipboard contents or real document data were written into audit logs. No persistent settings changed.

## Verification and limits

Full MSBuild Rebuild passed. Final native audit exited 0 with 135 PASS: `artifacts/desktop-global-paste/live-editor-audit-319f5d61f08a45fa848ef6269d249cca/audit.txt`. All three new focus-classification checks are present. An earlier incremental run had only the old 132 checks and is not used as evidence for this increment. `git diff --check` passed (existing CRLF normalization notice only).

Build output: `artifacts/desktop-global-paste`. Native editor regression includes new text-focus classification tests. This is source-level routing audit plus native automated regression, not physical Ctrl+V UAT on every page. Manual checks still required: copy a PDF in Explorer, Ctrl+V with focus in sidebar/search/blank area/canvas on each route; paste text in an active editor; cancel replacement prompts; try busy state, unsupported input and same file. Verify only one import per keystroke. File-size, rotation/crop and font restrictions remain unchanged.
