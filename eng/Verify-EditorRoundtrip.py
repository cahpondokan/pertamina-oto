"""Independent synthetic editor output check using pypdf, pdfplumber and Poppler."""
import json
import subprocess
import sys
from pathlib import Path
from PIL import Image, ImageChops
from pypdf import PdfReader
import pdfplumber

source, result, output = map(Path, sys.argv[1:4])
output.mkdir(parents=True, exist_ok=True)
before, after = PdfReader(source), PdfReader(result)
assert len(before.pages) == len(after.pages) == 2
text = after.pages[0].extract_text()
assert "Original source content stays here." in text
assert "EDITOR NEW TEXT UPDATED" in text
assert "CONTROL PAGE - UNCHANGED" in after.pages[1].extract_text()
for label, pdf in (("source", source), ("result", result)):
    for page in (1, 2):
        subprocess.run(["pdftoppm", "-r", "72", "-f", str(page), "-l", str(page), "-singlefile", "-png", str(pdf), str(output / f"{label}-{page}")], check=True, capture_output=True)
original = Image.open(output / "source-2.png").convert("RGB")
control = Image.open(output / "result-2.png").convert("RGB")
assert ImageChops.difference(original, control).getbbox() is None
original = Image.open(output / "source-1.png").convert("RGB")
edited = Image.open(output / "result-1.png").convert("RGB")
assert ImageChops.difference(original.crop((0, 0, 595, 120)), edited.crop((0, 0, 595, 120))).getbbox() is None
with pdfplumber.open(result) as pdf:
    words = [w for w in pdf.pages[0].extract_words() if w["text"] == "EDITOR" and w["top"] > 120]
    assert len(words) == 1 and abs(words[0]["x0"] - 40) < 1 and 139 < words[0]["top"] < 148
report = {"page_count": 2, "new_text_extracts": True, "original_text_retained": True,
          "control_page_pixel_identical": True, "source_header_pixel_identical": True,
          "text_coordinates_validated": True, "engine": "Poppler / pypdf / pdfplumber"}
(output / "report.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report))
