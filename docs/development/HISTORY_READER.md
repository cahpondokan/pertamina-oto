# Riwayat and internal PDF reader

> Layout below is historical. The 17 September 2026 user-approved interactive design and its native audit evidence are documented in `HISTORY_APPROVED_UI.md`. The new page no longer uses the old outer StackPanel/scroll shell, and card mode is not forced off when preview is open.

## Split-view revision

### Screenshot-driven layout correction

The mixed date/document GridView used a uniform wrap layout, which caused row clipping. Detail mode now explicitly uses ItemsStackPanel, with stretched containers and minimum 88px document rows. Card mode retains ItemsWrapGrid. Search/filter move inside the top of the left panel; Buka PDF remains in the page header. Both panels occupy a bounded, top-aligned shell. Narrow preview-list rows retain two lines of metadata and larger thumbnails. The reader toolbar is below the page, and Fit uses both width and height (including the PDF page aspect ratio), rather than width alone. No screenshot-based pixel-match verification is claimed by the headless flow test.

Initial history now defaults to compact detail rows with first-page thumbnails, filename/tool, separate page count and file size, job timestamp, and overflow actions. Groups are Hari ini/Kemarin/date; name sort uses a single group. Timestamp is the recorded job time, not claimed to be the last-open time. Search, filter and newest/oldest/name sort are available. Cards and native pinch remain supported.

Opening a PDF from the loaded history page preserves the list on the left and embeds the reader on the right. Close preview restores full-width history; expand toggles the left panel. The narrow list deliberately hides extra columns. Data storage format is unchanged. Layout and physical gesture acceptance still require direct user testing; automated flow/storage checks are not visual acceptance.

History uses local first-page previews, filenames, stored job details/date, folder and metadata-removal actions. Multi-output jobs retain access to additional files in the overflow menu. Missing files remain recorded and are labelled. Non-PDF results are accessed through their folder; the internal reader only opens PDFs.

Native pinch changes history card scale (50–180%); at 68% and below it switches to detail rows. Explicit Kartu/Detail and +/- controls are provided. Wide windows show inline detail fields; narrow windows use compact stacked fields. Resize keeps card identity and attempts first-visible-card scroll anchoring. Search/filter rebuild the visible results. Back from the reader restores the same history control tree including its selection/scroll.

Reader opens local PDFs up to 100 MB / 1000 pages, with password retry, lazy navigation thumbnails, page-number entry, native pinch/zoom, fit, and use-in-tools handoff. Rendering uses the existing Windows PDF renderer and semaphore. No external viewer, downloads, uploads, editing, text extraction or search inside PDF is introduced. Main page renders at 1600px; very high zoom may soften text. This is a basic visual reader, not a full accessibility/text-selection PDF engine.

Root KeyboardAcceleratorPlacementMode is Hidden to suppress the automatic window-wide Ctrl+V tooltip. Ctrl+V handlers remain unchanged.

Verification: Debug build and existing flow/library self-tests. Touchpad gestures, real rendered layout, scroll anchoring across mode transitions and password UX require interactive acceptance. No production-readiness claim.
