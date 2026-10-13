"""Private local fixture check: no source text is printed or copied to repository evidence."""
import hashlib
import json
import subprocess
import sys
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw
from pypdf import PdfReader
import pdfplumber

worker, source, directory = map(Path, sys.argv[1:4])
directory.mkdir(parents=True, exist_ok=True)
before_hash = hashlib.sha256(source.read_bytes()).digest()

def execute(request):
    process = subprocess.run([str(worker.resolve())], input=json.dumps(request), text=True, capture_output=True)
    reply = json.loads(process.stdout)
    assert process.returncode == 0 and reply['Success'], reply['Message']
    return reply

inspection = execute(dict(Tool='edit-text-check', Inputs=[str(source)], Output=str(directory / 'unused.pdf'), Rotation=0))
runs = inspection['TextRuns']
run = next(r for r in runs if r['MatchingLocalFont'] and r['Width'] > 200 and not r['Text'].isspace())
replacement = 'UJI FONT Zqx 789'
output = directory / 'local-font-check.pdf'
execute(dict(Tool='edit', Inputs=[str(source)], Output=str(output), Marks=[dict(
    Page=0, Kind='replace-text', X=run['X'], Y=run['Y'], Width=run['Width'], Height=run['Height'],
    Size=run['Size'], Text=replacement, SourceId=run['Id'], OriginalText=run['Text'], LocalFont=run['MatchingLocalFont'])]))
before, after = PdfReader(source), PdfReader(output)
assert len(before.pages) == len(after.pages)
assert run['Text'] not in after.pages[0].extract_text()
assert replacement in after.pages[0].extract_text()
with pdfplumber.open(output) as pdf:
    words = pdf.pages[0].extract_words(extra_attrs=['size', 'fontname'])
    word = next(w for w in words if w['text'] == 'UJI')
    assert abs(word['x0'] - run['X']) < .1
    assert abs(word['size'] - run['Size']) < .1
    # PDFsharp uses family/style names with spaces; the original Word PDF uses PostScript names.
    assert word['fontname'].split('+')[-1].replace(' ', '') in ('TimesNewRoman,Bold', 'TimesNewRomanPS-BoldMT')
for label, pdf in [('before', source), ('after', output)]:
    subprocess.run(['pdftoppm', '-r', '96', '-f', '1', '-singlefile', '-png', str(pdf), str(directory / label)], check=True, capture_output=True)
before_image = Image.open(directory / 'before.png').convert('RGB')
after_image = Image.open(directory / 'after.png').convert('RGB')
assert ImageChops.difference(before_image, after_image).getbbox() is not None
mask = (int((run['X'] - 2) * 96/72), int((run['Y'] - 2) * 96/72), int((run['X'] + run['Width'] + 2) * 96/72), int((run['Y'] + run['Height'] + 2) * 96/72))
ImageDraw.Draw(before_image).rectangle(mask, fill='white')
ImageDraw.Draw(after_image).rectangle(mask, fill='white')
assert ImageChops.difference(before_image, after_image).getbbox() is None, 'Changes outside target text region'
assert hashlib.sha256(source.read_bytes()).digest() == before_hash
print(json.dumps(dict(pass_result=True, detected_runs=len(runs), new_characters_saved=True, old_text_removed=True,
                     font_and_position_verified=True, pixels_outside_target_identical=True, source_hash_unchanged=True)))
