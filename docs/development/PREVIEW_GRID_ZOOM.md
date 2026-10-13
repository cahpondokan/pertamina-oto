# Preview grid zoom

Implemented for simultaneous multi-card previews: Merge, Split, Remove, Extract and Organizer. Existing single-page pagers remain single-page pagers.

Shared controls: native Windows ScrollViewer pinch, Ctrl+wheel, minus/plus/reset; scale 70–200%, shared during the current window session. Native pinch scales visually during the gesture, then commits card dimensions and resets native zoom at completion to reflow columns. This is not continuous column reflow on every pinch delta. Sidebar and options are outside the zoom viewport.

Grid cards retain their controls and bitmap sources. Organizer recalculates pitch, columns, positions, drag ghost and insertion gaps without changing the page model. No engine or dependency changes.

Verification: build and --self-test-flow; hardware pinch, visual wrap with populated PDFs, scroll anchoring and drag at zoom limits require interactive acceptance on the user's precision touchpad. Do not infer hardware gesture acceptance from the headless checks. Very large previews reuse thumbnails and can appear softer.
