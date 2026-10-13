# Gabung PDF redesign — 16 September 2026

- One compact card per source PDF: first page, short filename, colored outline, full name/page count/size tooltip.
- Rotate/remove are revealed on hover or keyboard focus. Rotation applies to all source pages; removal only affects the session list.
- Native GridView reorder replaces custom drop/rebuild. Existing cards and images are retained; Ctrl+Left/Right is an alternative. Shared preview zoom remains available.
- Right panel retains automatic editable output name, one output-folder settings icon, collapsed password option, primary merge button, and a link to Organizer.
- Organizer handoff preloads page counts within 100 MB / 1000 pages, then transfers document order and rotation. Existing organizer work prompts before replacement. It does not change source files. Source-color palette is shared; source indices are reassigned to the transferred order.

Verification: Debug build and --self-test-flow. Tests cover navigation ownership and card identity on reorder/removal without rendering real files. Real touchpad, native drag animation, keyboard focus reveal, encrypted-file transfer and final PDF content require interactive acceptance. No release or production-readiness claim.
