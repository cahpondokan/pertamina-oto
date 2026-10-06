"""Synthetic-only probe using existing bundled tooling, not an app dependency."""
import ctypes, hashlib, json, subprocess, sys
from pathlib import Path
from datetime import datetime
import pypdfium2 as pdfium
import pypdfium2.raw as raw
import pypdfium2_raw as raw_package
from pypdf import PdfReader
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from PIL import Image, ImageChops

out = Path(sys.argv[1]).resolve()/datetime.now().strftime('%Y%m%d-%H%M%S-%f')
out.mkdir(parents=True, exist_ok=False)
checks = []
result = {'runtime':str(pdfium.PDFIUM_INFO),'wrapper':str(pdfium.PYPDFIUM_INFO),'checks':checks}
def check(ok, label):
    checks.append({'pass':bool(ok),'check':label})
    if not ok: raise AssertionError(label)
def digest(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def make(p, subset=False):
    c=canvas.Canvas(str(p),pagesize=(595,842),invariant=1)
    c.setFont('Helvetica-Bold',20); c.drawString(48,785,'PERTAMINA OTO - UJI MESIN PDF')
    c.setFont('Helvetica',11); c.drawString(48,755,'Dokumen sintetis. Bukan laporan operasional.')
    c.setStrokeColorRGB(.2,.4,.7); c.rect(42,660,510,65)
    c.setFont('ProbeArial' if subset else 'Helvetica',16)
    c.drawString(55,685,'ABBA' if subset else 'Status pompa: perlu perbaikan')
    c.setFont('Helvetica',11); c.drawString(48,615,'Nomor uji: 12345 - bagian ini tidak berubah.')
    c.showPage(); c.setFont('Helvetica',16)
    c.drawString(48,760,'HALAMAN KONTROL - TIDAK DIUBAH'); c.rect(48,640,200,60)
    c.showPage(); c.save()
def edit(src,dest,old,new):
    assert not dest.exists()
    doc=pdfium.PdfDocument(src); page=doc[0]; tp=page.get_textpage()
    objects=list(page.get_objects(textpage=tp))
    matches=[o for o in objects if o.type==raw.FPDF_PAGEOBJ_TEXT and o.extract()==old]
    check(len(matches)==1,'single target '+src.stem)
    count=raw.FPDFPage_CountObjects(page)
    data=new.encode('utf-16-le')+b'\x00\x00'
    buf=(ctypes.c_ushort*(len(data)//2)).from_buffer_copy(data)
    check(raw.FPDFText_SetText(matches[0],buf),'SetText success '+src.stem)
    check(raw.FPDFPage_CountObjects(page)==count,'no overlay objects added '+src.stem)
    tp.close(); page.gen_content(); doc.save(dest); page.close(); doc.close()
    text=PdfReader(dest).pages[0].extract_text()
    return {'text':text,'replacement_present':new in text,'old_absent':old not in text,'objects':count}
def render(p):
    prefix=p.with_suffix('')
    subprocess.run(['pdftoppm','-r','96','-png',str(p),str(prefix)],check=True,timeout=30,capture_output=True)
    return [Path(str(prefix)+'-1.png'),Path(str(prefix)+'-2.png')]
try:
    result['dll_sha256']=digest(Path(raw_package.__file__).parent/'pdfium.dll')
    src,dest=out/'before.pdf',out/'after.pdf'
    make(src); baseline=digest(src)
    result['basic']=edit(src,dest,'Status pompa: perlu perbaikan','Status pompa: sudah diperbaiki')
    check(result['basic']['replacement_present'] and result['basic']['old_absent'],'independent pypdf confirms replacement')
    check('Nomor uji: 12345' in result['basic']['text'],'neighbor retained')
    a,b=render(src),render(dest)
    with Image.open(a[1]) as x,Image.open(b[1]) as y:
        check(ImageChops.difference(x.convert('RGB'),y.convert('RGB')).getbbox() is None,'Poppler control page identical')
    with Image.open(a[0]) as x,Image.open(b[0]) as y:
        box=ImageChops.difference(x.convert('RGB'),y.convert('RGB')).getbbox()
        result['changed_pixel_bounds']=box
        check(box is not None and box[1]>180 and box[3]<225,'changes confined to target line')
    check(digest(src)==baseline,'source hash unchanged')
    pdfmetrics.registerFont(TTFont('ProbeArial','C:/Windows/Fonts/arial.ttf'))
    src,dest=out/'subset-before.pdf',out/'subset-after.pdf'
    make(src,True); baseline=digest(src)
    result['subset']=edit(src,dest,'ABBA','\u03a9CDEF')
    render(src); render(dest)
    check(digest(src)==baseline,'subset source hash unchanged')
    result['status']='BASIC_PASS_SUBSET_'+('PASS' if result['subset']['replacement_present'] else 'LIMITATION')
except Exception as exc:
    result['status']='FAIL';result['error']=repr(exc);raise
finally:
    (out/'results.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
    print(out);print(json.dumps(result,indent=2))
