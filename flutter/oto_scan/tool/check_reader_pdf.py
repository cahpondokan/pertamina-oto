"""Desktop reference measurements, never Android/phone performance claims."""
from pathlib import Path
from io import BytesIO
import hashlib,json,math,time
import numpy as np
from PIL import Image
from pypdf import PdfReader,PdfWriter
import pypdfium2 as pdfium
root=Path(__file__).resolve().parents[1]
out=root/'evidence/r11-reader';out.mkdir(exist_ok=True)
source=root/'evidence/r05-pdf/compact-colour.pdf'
results=[]
for count in (1,2,10,20):
    writer=PdfWriter();template=PdfReader(source)
    for i in range(count): writer.add_page(template.pages[0])
    path=out/f'{count}-pages.pdf'
    with path.open('wb') as f:writer.write(f)
    before=hashlib.sha256(path.read_bytes()).hexdigest()
    start=time.perf_counter();doc=pdfium.PdfDocument(path);open_ms=(time.perf_counter()-start)*1000
    def render(index,width,detail=False,old=False):
        started=time.perf_counter();p=doc[index];w,h=p.get_size()
        cap=8_000_000 if detail or old else 2_000_000
        scale=min(width/w,(3800 if detail or old else 2200)/max(w,h),math.sqrt(cap/(w*h)))
        bitmap=p.render(scale=scale);im=bitmap.to_pil().convert('RGB');stream=BytesIO()
        im.save(stream,format='PNG' if old else 'JPEG',**({} if old else {'quality':98 if detail else 94}))
        data=stream.getvalue()
        if old:
            cache=out/'old-preview.png';cache.write_bytes(data);data=cache.read_bytes()
        loaded=Image.open(BytesIO(data));loaded.load()
        elapsed=(time.perf_counter()-started)*1000
        bitmap.close();p.close()
        return im,data,elapsed
    _,old,old_ms=render(0,3800,old=True)
    screen,data,first_ms=render(0,1080)
    cache=out/f'{count}-screen.jpg';cache.write_bytes(data)
    start=time.perf_counter();warm=cache.read_bytes();image=Image.open(BytesIO(warm));image.load();warm_ms=(time.perf_counter()-start)*1000
    sequential=[]
    for index in range(1,count):
        _,_,elapsed=render(index,1080);sequential.append(elapsed)
    detail,high,zoom_ms=render(0,3800,detail=True)
    decoded=np.asarray(Image.open(BytesIO(high))).astype(np.int16);ref=np.asarray(detail).astype(np.int16)
    ink=np.mean(ref,axis=2)<80
    retention=float(np.mean(np.mean(decoded,axis=2)[ink]<110))
    mae=float(np.mean(np.abs(ref-decoded)))
    assert retention>.995 and mae<1.5
    if count==2:
        screen.save(out/'screen-page.png')
        detail.save(out/'detail-reference.png')
        Image.open(BytesIO(high)).save(out/'detail-preview.png')
        # Actual scaled 6-12pt text region; side-by-side lossless render vs preview JPEG.
        w,h=detail.size;box=(int(w*.05),int(h*.085),int(w*.75),int(h*.23))
        a=detail.crop(box);b=Image.open(BytesIO(high)).crop(box)
        comparison=Image.new('RGB',(a.width,a.height*2+8),'#0a2740');comparison.paste(a,(0,0));comparison.paste(b,(0,a.height+8));comparison.save(out/'zoom-quality.png')
    doc.close();assert hashlib.sha256(path.read_bytes()).hexdigest()==before
    results.append({'pages':count,'pdf_bytes':path.stat().st_size,'pdf_sha256':before,'open_ms':round(open_ms,2),
        'old_first_8mp_png_ms':round(old_ms,2),'screen_first_ms':round(first_ms,2),'warm_screen_decode_ms':round(warm_ms,2),
        'screen_raster':list(screen.size),'screen_decoded_bytes':screen.width*screen.height*4,'screen_encoded_bytes':len(data),
        'detail_raster':list(detail.size),'detail_decoded_bytes':detail.width*detail.height*4,'detail_encoded_bytes':len(high),
        'zoom_ms':round(zoom_ms,2),'other_page_mean_ms':round(sum(sequential)/len(sequential),2) if sequential else None,
        'dark_ink_retention':retention,'mean_channel_error':mae,'pdf_unchanged':True})
report={'scope':'Synthetic desktop PDFium/Pillow reference, not Android PdfRenderer or phone measurements',
    'fixtures':'existing synthetic scanned A4, no user documents','results':results,
    'passes':['all 1/2/10/20-page fixtures open and render','source PDF hashes unchanged','detail dark ink retention >99.5%','mean channel error <1.5/255']}
(out/'measurements.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
