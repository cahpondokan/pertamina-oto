# Hapus halaman PDF — compact preview

Each source page has a thumbnail and original page number. Hover/keyboard focus reveals the trash glyph; selection retains that glyph with a transparent red tint. Clicking again cancels selection. Shared preview zoom is retained, no rotate/reorder/blank-page controls are introduced.

Right panel: compact total/deleted/remaining summary; optional numeric range expander; clear selection; optional password; automatic output filename; single folder-settings control; dynamic “Hapus N halaman & simpan” button. Selecting all pages disables processing and explains that one page must remain. Original PDF is not modified; existing worker and unique-output path flow are unchanged.

Verification: Debug build and --self-test-flow covering navigation, selection count, clear selection and all-pages guard. Visual hover, real touchpad and final PDF output still require interactive acceptance; no production approval implied.
