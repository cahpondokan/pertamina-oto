"""Independent verification for the synthetic original-text fixture; not a product dependency."""
import json
import subprocess
import sys
from pathlib import Path
from PIL import Image, ImageChops
from pypdf import PdfReader

source, edited, output = map(Path, sys.argv[1:4])
output.mkdir(parents=True, exist_ok=True)
before, after = PdfReader(source), PdfReader(edited)
assert len(before.pages) == len(after.pages) == 2
assert "PERTAMINA OTO - EDITOR TEST" in before.pages[0].extract_text()
text = after.pages[0].extract_text()
assert "PERTAMINA OTO" in text and "PERTAMINA OTO - EDITOR TEST" not in text
assert "Original source content stays here." in text
a, b = before.pages[0].get_contents().operations, after.pages[0].get_contents().operations
assert len(a) == len(b), "Replacement must not add overlay drawing operators"
changes = [(x, y) for x, y in zip(a, b) if x != y]
assert len(changes) == 1 and changes[0][0][1] == changes[0][1][1] == b"Tj"
fonts = before.pages[0]["/Resources"]["/Font"]
identity_h = any(f.get_object().get("/Encoding") == "/Identity-H" for f in fonts.values())
if identity_h:
    # This synthetic fixture replaces a heading with its exact 13-character prefix.
    # Identity-H uses two-byte CIDs: require the EXACT original glyph bytes, not
    # merely a changed string, and independently require exact decoded text below.
    old_operand, new_operand = changes[0][0][0][0], changes[0][1][0][0]
    assert new_operand.original_bytes == old_operand.original_bytes[:len("PERTAMINA OTO") * 2]
else:
    assert changes[0][1][0][0] == "PERTAMINA OTO"
assert text.splitlines()[0] == "PERTAMINA OTO"
for label, pdf in (("source", source), ("result", edited)):
    for page in (1, 2):
        subprocess.run(["pdftoppm", "-r", "96", "-f", str(page), "-l", str(page), "-singlefile", "-png", str(pdf), str(output / f"{label}-{page}")], check=True, capture_output=True)
src, dst = [Image.open(output / f"{label}-1.png").convert("RGB") for label in ("source", "result")]
bounds = ImageChops.difference(src, dst).getbbox()
assert bounds and bounds[1] >= 50 and bounds[3] <= 82, bounds
src, dst = [Image.open(output / f"{label}-2.png").convert("RGB") for label in ("source", "result")]
assert ImageChops.difference(src, dst).getbbox() is None
report = dict(original_operand_replaced=True, no_overlay_operators=True, old_text_absent=True,
              unchanged_page_pixel_identical=True, changed_pixels_within_target_line=list(bounds),
              engines="pypdf / Poppler")
(output / "report.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report))
