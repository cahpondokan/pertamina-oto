OTO WEB — LOCAL BROWSER PDF DEVELOPMENT PREVIEW (2026-10-17)

CURRENT CAPACITY/STORAGE UPDATE (2026-10-27)
The historical implementation notes below describe earlier bounded-memory engines.
Current active browser tools accept at most 4,000,000,000 input bytes per job:
Merge 500 PDFs; Organize 10 PDFs; single-PDF tools 1 PDF; Images 10 JPG/PNG.
The non-Merge PDF page ceiling remains 200; Split outputs remain 2-20 parts.
Pixel, structure, encryption, signature, active-content and tag/link checks remain.
Inputs stay as File references; PDF streams use bounded reads. New PDF results use
temporary OPFS files on the user's device, not server storage or uploads. No 4 GB
reservation. Web Locks protect job ownership and stale cleanup. Reload is not
session recovery. Save all results before closing; interrupted cleanup retries on
startup. Native image decoding still uses RAM and keeps the existing pixel limits.
Worker timeout: 30 minutes. Non-Merge result ceiling: 4 GB total. Total normalized
JPEG bytes remain capped at 40 MiB. Detailed current implementation/evidence:
docs/development/WEB_FILE_BACKED_CAPACITY_20261027.md (repository root).
869 regression tests passed. Preview: http://127.0.0.1:53970/alat/split
No public deployment or Windows update is part of this change.

HISTORICAL NOTES (the capacity/storage update above supersedes old limits below)

Run Start-Preview.ps1 -Website from the preview host directory, or:
dotnet run --project src/Hosts/Pertamina.Oto.Web.Preview --urls http://127.0.0.1:5266 --Preview:CatalogOnly=true
The historical CatalogOnly flag now selects the website catalog with ten real
browser tools. The older UI-only prototype remains separate when the flag is false.
Do not mistake prototype output for real processing.

Active: merge, organize, split, remove, extract, rotate, images-pdf, numbers,
watermark, crop. Inactive: the other eight catalog tools. Their routes return 404.
No sidebar navigation. Biblo icons are the existing feature-specific collection.
Output is a new PDF; original files are never overwritten.

PRIVACY
Files and results reside only in tab/worker memory. There is no upload endpoint,
document database, server processing, document localStorage, IndexedDB, or cache.
Preferences/favorites may remain in localStorage via the existing shared shell.
The website initially needs HTTP access to obtain code/assets. This is not a PWA
and is not advertised as a fully offline-installable website.
PDF computation needs no network; worker CSP connect-src is none. Worker and
scripts are self-hosted; no CDN or third-party API.
HTTP requests other than GET/HEAD return 405. The preview is loopback-only.
Leaving/reloading loses tab data; downloads require an explicit click.

SUPPORT BOUNDARY
10 files / 10 MiB each / 30 MiB combined / 200 source pages, maximum 20 split
results, 40 MiB cumulative serialized output, 30 seconds per worker operation.
Output serialization may allocate before the output-size check; these safeguards
do not constitute an absolute browser heap limit or assurance against all hostile
input. DecodeStream is capped at 16 MiB per stream / 64 MiB cumulative allocations;
indirect objects are capped at 20,000/context. Every operation gets a fresh worker.
Use trusted copies for this development preview.

Encrypted PDFs, forms, signatures, attachments and active actions remain refused.
Supported static annotations and safe URI/internal links are now preserved. Source
bookmarks/metadata are not copied. No guarantee of PDF/A conformance.
Compatibility correction (2026-10-19): a verified empty /Annots array or explicit
null is accepted, including indirect references. Minimal inert /AcroForm {} or
{Fields: []}, or explicit null, is also accepted. Extra form properties, populated
fields and active scripts/actions remain refused. Non-empty annotations use the
bounded preservation path below, not an exception to structure validation.
No annotations, forms, signatures or accessibility tags are silently stripped.
Dangling/cyclic/overlong indirect-reference chains fail closed. Every indirect
object is still checked, including unreachable objects and stream dictionaries.
Rejections use bounded category codes/messages; document contents are not logged.

TAGGED PDF PRESERVATION (2026-10-19)
Supported PDF 1.7 page MCIDs/MCRs retain structure hierarchy, reciprocal parent
tree, page pointers, Alt/ActualText/Lang/T/E, attributes, class/role maps and IDs.
Page labels and document language/title are retained for tagged sources; archive
metadata, bookmarks, source PDF/UA/PDF/A certification claims are not copied.
Destination parent keys are rebuilt; source IDs/class names are qualified when
combining documents. Conflicting custom role meanings fail, not override.
Reorder/extract/split/remove may prune or move complete semantic units only.
Edits splitting multi-page paragraphs/tables, references into removed tags,
duplicate tagged pages, source interleaving, mixed tagged/untagged PDFs and
unsupported namespace/XObject relationships fail closed with specific codes.
Empty semantic cells are not silently dropped. Security scanning still covers
every parsed indirect object; tagged support does not admit forms or active actions.
Supported annotation OBJRs retain reciprocal StructParent/ParentTree edges.
Page numbers are pagination artifacts with matching accessible page labels.
Meaningful watermark text is added as
tagged Note content with its own MCID, not hidden as an artifact. No flattening.
Every serialized output is checked again for security and structure reciprocity.
This is bounded preservation, not automatic tag repair or PDF/UA certification.

ANNOTATION/LINK PRESERVATION (2026-10-19)
Allowlisted PDF 1.7 Text, Popup, Link, FreeText, Line, Square, Circle, Polygon,
PolyLine, Highlight, Underline, Squiggly, StrikeOut, Stamp, Caret and Ink retain
comments/authors, properties, AP appearance streams/resources, popup/reply edges
and their tagged OBJR owners when present. No flattening or deletion.
HTTP/HTTPS absolute URLs and simple mailto: addresses are retained unchanged;
no URL is requested, executed or reputationally assessed. Do not infer that an
external website is safe just because its URL syntax is supported.
Explicit and legacy/name-tree internal destinations become equivalent explicit
output-page destinations. Pages are pre-mapped before copying to prevent orphan
pages, stale /P references or cross-document destination aliasing.
If any retained link targets an omitted page, the whole operation fails
LINK_TARGET. Annotated page duplication fails ANNOTATION_OPERATION (ambiguous
links/annotation identity), rather than removing annotations. 2,000 annotations
per parsed document, with bounded name-tree/coordinate/metadata traversal.
Unknown annotation keys/types, rich text, optional-content annotations, widgets,
redactions, attachments, multimedia, chained/additional/automatic actions,
remote-file/script/launch actions and unsafe URI schemes remain refused.
External streams, reference XObjects and OPI are refused. Existing encryption,
signature, input/output size, page, decompression, object and timeout gates stay.
All indirect objects are scanned, including unselected pages/unreachable payloads.

Independent pypdf checks and Poppler rendering use synthetic fixtures; real-file,
screen-reader and cross-browser UAT remain required for broader certification.
Outputs from the ten tools are regression-tested through the actual input gate
and subsequent operations, within the existing input size/page limits. This is
not support for every PDF variant or a bypass of the 10 MiB per-input-file limit.
Page cards show source identity (A · 1 etc.), not rendered page-content previews.
Organize supports drag, arrow alternatives, rotation, duplication and removal.
Workspace UI follows the Windows Merge/Organizer visual pattern: a single choose
action when empty, document/page cards on the left and source files/output settings
on the right after selection. No navigation sidebar or separate paste button.
Shortcuts appear as tooltips only. Card size changes presentation, not PDF contents.
Merge has one card per source; drag/arrows reorder the actual output documents.
Organizer preserves original source/page identity separately from output position.
Card controls are available on hover/focus and remain visible on narrow/touch UI.
Results are a separate download screen; returning to the editor preserves a valid
result until documents/options change. Closing confirms before clearing tab data.
Single-PDF tools require closing the existing document before choosing another.
Split requires a complete non-overlapping partition (example 1-2;3-4).
Numbers: selected/all pages, six positions, start number, number/last-number
format, size and margin. Existing numbers are not removed.
Watermark: single-line WinAnsi text, color, opacity, horizontal/diagonal, selected
pages. Font auto-fits when needed. No image watermark, arbitrary Unicode, or
security/redaction guarantee. It is an overlay, not protection.
Crop: numeric margins relative to visible page edges including rotation and
nonzero crop origin. Changes CropBox ONLY: hidden content is still in the PDF.
Not redaction. No visual crop editor yet. Non-unit UserUnit refused.
Images: JPG RGB/grayscale or static 8-bit PNG; at most 8 MP/image, 16 MP total,
6,000 pixels/side, other file/byte/time limits still apply. Header limits are
checked before native createImageBitmap decoding. OffscreenCanvas normalizes
EXIF orientation and flattens transparency onto white, then encodes JPEG 0.92.
No raw user PNG is passed into pdf-lib's PNG inflater. Output is A4, automatic/
portrait/landscape, 0-30 mm margin, one image/page, aspect fit without cropping.
Image cards have sanitized JPEG thumbnails, stable source labels, drag/arrow
reordering. URL blobs are revoked when removed/closed/leaving. No SVG, HEIC,
CMYK, APNG, PNG 16-bit, or unsupported-browser fallback to server uploads.
Native image codecs still require patched browsers. Pixel/byte limits mitigate
risk; they are not a hard browser heap or hostile-file safety guarantee.
OS-native clipboard/file drag behavior has not been certified across all browsers.

DEPENDENCY
Official pdf-lib 1.17.1 UMD, pinned and downloaded after user authorization and
license/advisory review. All notices are in wwwroot/vendor/pdf-lib-1.17.1.
Runtime SHA256 after explicit OTO guards:
6CB6876951638723EFC01CB4860C44EB69FFC1ED56C731EBE892E73D8775749F
No install scripts, npm resolution or paid services used.
OSV exact-version lookup returned no matches; this is not a security guarantee.
Related-fork advisory GHSA-vrqm-gvq7-rrwh informed the DecodeStream guards.
No formal corporate legal/security/Production sign-off is claimed.

TESTS
Set OTO_PDF_TEST_DIR to a synthetic fixture directory.
python tests/browser-fixtures.py <directory>
python tests/decoration-fixtures.py <directory>
node --test tests/pdf-engine.test.cjs tests/pdf-decoration.test.cjs tests/pdf-job.test.cjs tests/worker-offline.test.cjs tests/workspace-model.test.cjs tests/pdf-workspace-model.test.cjs tests/pdf-compatibility.test.cjs tests/pdf-tags.test.cjs tests/pdf-annotations.test.cjs
python tests/browser-fixtures.py <directory> verify
python tests/decoration-fixtures.py <directory> verify
PowerShell: tests/Verify-BrowserPdfHttp.ps1 (with localhost server running).
Use the bundled Python/Node runtimes if not on PATH.

Evidence:
Baseline: artifacts/web-mvc-preview-20261016/browser-pdf/
Extension: artifacts/web-free-tools-20261017/
Workspace redesign: artifacts/web-workspace-ui-20261017/
Compatibility fix: artifacts/web-pdf-compatibility-20261019/
Tagged structure preservation: artifacts/web-tagged-pdf-20261019/
Annotation/link preservation: artifacts/web-annotations-20261019/
No desktop installation, release feed change, public VPS deployment, or phase
completion is included. Review locally before considering public publication.
