"""Bounded synthetic regression, independent extraction/operator/raster verification.

No dependency downloads, app profile changes or source modifications. Test only the
four-page synthetic Uji-Edit-Teks-Berbagai-Font.pdf, never confidential documents.
"""
import hashlib
import io
import json
import subprocess
import sys
from pathlib import Path
from PIL import Image, ImageChops
from pypdf import PdfReader
from pypdf.generic import ContentStream

worker, source, evidence = map(Path, sys.argv[1:4])
evidence.mkdir(parents=True, exist_ok=False)
source_hash = hashlib.sha256(source.read_bytes()).hexdigest()
assert source_hash == "a4e475d5b5cd1601766b0702c73523e1c3a377139ced424a364e295a44201c3b"
report = {"source_sha256": source_hash, "worker_sha256": hashlib.sha256(worker.with_suffix(".dll").read_bytes()).hexdigest(), "checks": []}
def check(value, label):
    assert value, label
    report["checks"].append(label)
def call(tool, file, page=0, marks=None, output=None):
    req = dict(Tool=tool, Inputs=[str(file.resolve())], Output=str((output or evidence/"unused.pdf").resolve()), Rotation=page)
    if marks is not None: req["Marks"] = marks
    p = subprocess.run([str(worker.resolve())], input=json.dumps(req)+"\n", capture_output=True, text=True, timeout=30)
    return p.returncode, json.loads(p.stdout)
def inspect(file, page):
    code, reply = call("edit-text-check", file, page)
    assert code == 0 and reply["Success"], reply
    return reply["TextRuns"]
def render(file, page):
    p = subprocess.run(["pdftoppm", "-r", "96", "-f", str(page+1), "-l", str(page+1), str(file)], capture_output=True, timeout=30)
    assert p.returncode == 0, p.stderr
    return Image.open(io.BytesIO(p.stdout)).convert("RGB")
def mark(run, page, text):
    return dict(Page=page, Kind="replace-text", X=run["X"], Y=run["Y"], Width=run["Width"], Height=run["Height"], Text=text, Size=run["Size"], SourceId=run["Id"], OriginalText=run["Text"])
before = PdfReader(source)
original_renders = [render(source,p) for p in range(4)]
allruns = [inspect(source,p) for p in range(4)]
report["runs_per_page"] = [len(x) for x in allruns]
check(all(len(r)>0 for r in allruns), "all four original fixture pages contain editable runs")
check(len([r for r in allruns[0] if r["Text"]=="Nama: Budi Santoso | Nomor: 12345"])==6, "all six base font samples discovered")
check(len([r for r in allruns[1] if r["Text"]=="Nama: Budi Santoso | Nomor: 12345"])==8, "all eight Windows font samples discovered without changing PDF encoding")
check(not any(r["Text"]=="KODE UJI 123" for r in allruns[3]), "rotated text stays excluded")
cases = [(p,r) for p,rs in enumerate(allruns) for r in rs if
         (p in (0,1) and r["Text"]=="Nama: Budi Santoso | Nomor: 12345") or
         (p==2 and "+" in r["Font"]) or
         (p==3 and "+" in r["Font"])]
for index,(page,run) in enumerate(cases):
    # Same exact source codes, shortened visible text; no font substitution.
    replacement = run["Text"][:-4]
    output = evidence/f"roundtrip-{index:02}.pdf"
    code, reply = call("edit", source, marks=[mark(run,page,replacement)], output=output)
    check(code==0 and reply["Success"], f"{index}: actual save succeeds")
    after = PdfReader(output)
    newruns = inspect(output,page)
    edited = next(r for r in newruns if r["Id"]==run["Id"])
    # PDFsharp serializes page dimensions to 0.001 pt; Y is derived from that
    # dimension. Require unchanged actual position operators below and <=0.001 pt.
    check(edited["Text"]==replacement and edited["Font"]==run["Font"] and all(abs(edited[k]-run[k])<=0.001 for k in ("X","Y","Size")), f"{index}: reopen text/font/position within page serialization precision")
    for p in range(4):
        a = ContentStream(before.pages[p].get_contents(),before).operations
        b = ContentStream(after.pages[p].get_contents(),after).operations
        diffs = [(x,y) for x,y in zip(a,b) if x!=y]
        check(len(a)==len(b) and len(diffs)==(1 if p==page else 0), f"{index}: page {p+1} only target operand changes; no overlays")
        if p==page:
            check(diffs[0][0][1]==diffs[0][1][1]==b"Tj", f"{index}: source Tj rewritten")
            check(replacement in after.pages[p].extract_text(), f"{index}: independent Unicode extraction")
        image = render(output,p)
        delta = ImageChops.difference(original_renders[p],image).getbbox()
        if p!=page:
            check(delta is None, f"{index}: page {p+1} pixels unchanged")
        else:
            scale=96/72
            left,top,right,bottom = delta
            check(left >= (run["X"]-4)*scale and top >= (run["Y"]-4)*scale and right <= (run["X"]+run["Width"]+4)*scale and bottom <= (run["Y"]+run["Height"]+4)*scale, f"{index}: changed pixels confined to edited line")
            if index in (0,6,14):
                image.save(evidence/f"visual-{index:02}.png")
    # Guard semantics on the real input, not only minimal generated unit fixtures.
    if index==0:
        for label,bad in [("overflow", mark(run,page,"W"*200)),("missing-glyph",mark(run,page,"Ω")),("stale",dict(mark(run,page,"Budi"),OriginalText="stale"))]:
            badfile=evidence/f"reject-{label}.pdf"
            code,reply=call("edit",source,marks=[bad],output=badfile)
            check(code!=0 and not reply["Success"] and not badfile.exists(),f"unsafe {label} rejected without output")
check(hashlib.sha256(source.read_bytes()).hexdigest()==source_hash,"original PDF unchanged")
report["roundtrips"]=len(cases)
report["pass_count"]=len(report["checks"])
(evidence/"results.json").write_text(json.dumps(report,indent=2),encoding="utf-8")
print(json.dumps({k:v for k,v in report.items() if k!="checks"},indent=2))
