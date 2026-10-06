# Split visual modes

Three icon choices: Rentang, Halaman, Ukuran. Ukuran is explicitly disabled until a byte-validated split executor exists; Cerdas is omitted. Existing split engine and direct-PDF export remain unchanged.

Rentang uses visible choices for custom ranges and every N pages. Preview shows a labeled output group, exact page description, first/last page and ellipsis. Hover highlights only a card; custom group hover highlights its matching settings row. Halaman shows all pages or explicit selection with checkmarks, numeric input and combine option. Selection changes preserve card identity. Zoom controls apply to the preview area only.

Verification: Debug build, workspace flow self-test (including new visual-mode/selection checks). Native hover, pinch and clipping at small window widths still require user acceptance. Size splitting is not implemented or advertised as active.
