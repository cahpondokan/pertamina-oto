"""Independent pixel/PDF checks of synthetic fixtures; never connects to a phone."""
from pathlib import Path
import json, hashlib, subprocess, io, zlib
import numpy as np
from PIL import Image, ImageDraw, ImageFont
from pypdf import PdfReader
import pypdfium2 as pdfium

root = Path(__file__).resolve().parents[1]
folder = root / 'evidence/r13-quality'
checks = []
def check(name, passed):
    checks.append({'name': name, 'pass': bool(passed)})
    if not passed: print('FAIL', name)

def raster(obj):
    if obj['/Filter'] == '/DCTDecode':
        return np.asarray(Image.open(io.BytesIO(obj._data)).convert('RGB'))
    data = obj.get_data()
    return np.frombuffer(data, np.uint8).reshape(int(obj['/Height']), int(obj['/Width']), 3 if obj['/ColorSpace']=='/DeviceRGB' else 1)

for name in ['text', 'shadow', 'colour', 'photo']:
    path = folder / f'{name}-new.pdf'
    reader = PdfReader(path, strict=True)
    check(f'{name} valid single page', len(reader.pages)==1)
    page = reader.pages[0]
    check(f'{name} no active content', not any(key in reader.trailer['/Root'] for key in ['/OpenAction','/AA','/Names','/AcroForm']))
    source = np.asarray(Image.open(folder/f'{name}-processed.png').convert('RGB'))
    xo = page['/Resources']['/XObject']
    base = xo['/Im0'].get_object()
    check(f'{name} export keeps pixel dimensions', (int(base['/Width']),int(base['/Height']))==source.shape[1::-1])
    check(f'{name} page aspect unchanged', abs(float(page.mediabox.width)/float(page.mediabox.height)-source.shape[1]/source.shape[0])<1e-5)
    if base['/Filter']=='/FlateDecode':
        decoded = raster(base)
        check(f'{name} every RGB pixel retained exactly', np.array_equal(source,decoded))
        check(f'{name} pale 6-12pt ink and colour retained', np.array_equal(source[300:3200],decoded[300:3200]))
    doc=pdfium.PdfDocument(path); p=doc[0]; bitmap=p.render(scale=1.25)
    image=bitmap.to_pil().convert('RGB');image.save(folder/f'{name}-render.png')
    check(f'{name} independent PDFium render', image.width>700 and image.getextrema()[0][0]<100)
    bitmap.close();p.close();doc.close()

for name,count in [('three-pages.pdf',3),('twenty-pages.pdf',20)]:
    path=root/'evidence/r13-legacy-pdf'/name
    reader=PdfReader(path,strict=True)
    check(name+' page tree and xref',len(reader.pages)==count)
    doc=pdfium.PdfDocument(path)
    for index in range(count):
        pg=doc[index];bitmap=pg.render(scale=.3)
        check(name+f' page {index} renders',bitmap.width>0 and bitmap.height>0)
        bitmap.close();pg.close()
    doc.close()

reader=PdfReader(folder/'forced-layers.pdf',strict=True)
xo=reader.pages[0]['/Resources']['/XObject']; fg=xo['/Ink'].get_object();alpha=fg['/SMask'].get_object()
ink=raster(fg);mask=raster(alpha)[:,:,0];source=np.asarray(Image.open(folder/'text-processed.png').convert('RGB'))
check('MRC mask contains only transparent/opaque values', np.isin(mask,[0,255]).all())
check('MRC foreground pixels exactly match source including RGB', np.array_equal(ink[mask==255],source[mask==255]))
check('MRC includes fine strokes', ((mask==255)&(source[:,:,0]<80)).sum()==(source[:,:,0]<80).sum())
check('MRC lossless foreground preserves faint pencil', ((mask[2780:]==255)&(source[2780:,:,0]<230)).any())

def render_full(name):
    doc=pdfium.PdfDocument(folder/f'{name}.pdf');page=doc[0]
    bitmap=page.render(scale=2480/595);image=np.asarray(bitmap.to_pil().convert('RGB')).copy()
    bitmap.close();page.close();doc.close();return image
reference=render_full('forced-lossless').astype(np.int16)
layered=render_full('forced-layers').astype(np.int16)
pencil=source[2800:2870,150:1300,0]<235
error=np.abs(reference[2800:2870,150:1300]-layered[2800:2870,150:1300])[pencil]
check('MRC rendered pale ink has no dark mask halo', float(error.mean())<2)

# Independent Poppler rendering of the mixed layer and text output catches mask
# polarity, bad xrefs, predictors and compositing issues missed by stream checks.
poppler=Path('C:/Users/cahpo/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/poppler/Library/bin/pdftoppm.exe')
for name in ['text-new','forced-layers','adaptive-bw']:
    subprocess.run([str(poppler),'-f','1','-singlefile','-r','150','-png',str(folder/f'{name}.pdf'),str(folder/f'{name}-poppler')],check=True,capture_output=True)
    image=Image.open(folder/f'{name}-poppler.png').convert('RGB')
    check(f'{name} independent Poppler visible ink', np.count_nonzero(np.mean(np.asarray(image),axis=2)<100)>2000)

binary=Image.open(folder/'adaptive-bw.png').convert('1')
xo=PdfReader(folder/'adaptive-bw.pdf',strict=True).pages[0]['/Resources']['/XObject']['/Im0'].get_object()
check('adaptive BW compression retains all binary pixels', xo.get_data()==binary.tobytes())
tiff=io.BytesIO();binary.save(tiff,format='TIFF',compression='group4')
comparison={'flate_stream_bytes':len(xo._data),'ccitt_g4_tiff_bytes_including_container':len(tiff.getvalue()),'note':'Desktop codec comparison; no new Android dependency. Flate retained.'}

# Render three fixed zoom regions at 200% and 400%, not a huge full-page bitmap.
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',23)
comparison_image=Image.new('RGB',(1560,680),'white');draw=ImageDraw.Draw(comparison_image)
for col,(name,title) in enumerate([('text-old','Previous JPEG90'),('text-new','New lossless text'),('forced-layers','Mixed layer text')]):
    doc=pdfium.PdfDocument(folder/f'{name}.pdf');page=doc[0]
    bitmap=page.render(scale=2480/float(page.get_width()))
    im=bitmap.to_pil().convert('RGB')
    for row,(y,label) in enumerate([(295,'6 pt, 200%'),(2790,'Pale pencil, 400%')]):
        crop=im.crop((160,y,420,y+100)).resize((520,200),Image.Resampling.NEAREST)
        if row==1:crop=im.crop((160,y,290,y+50)).resize((520,200),Image.Resampling.NEAREST)
        draw.text((col*520+8,row*320+8),title+' / '+label,fill='black',font=font)
        comparison_image.paste(crop,(col*520,row*320+55))
    bitmap.close();page.close();doc.close()
comparison_image.save(folder/'zoom-comparison.png')
result={'scope':'Synthetic desktop Java ImageIO + Poppler/PDFium. Not Android camera/encoder/device performance.', 'checks':checks,'passed':sum(c['pass'] for c in checks),'total':len(checks),'binary_codec_comparison':comparison,'compression':json.loads((folder/'compression.json').read_text(encoding='utf-8')),'pdf_sha256':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in folder.glob('*.pdf')}}
(folder/'verification.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in result.items() if k not in ['checks','pdf_sha256']},indent=2))
if not all(c['pass'] for c in checks):raise SystemExit(1)
