"""Desktop synthetic fixture and independent PDF oracle; never reads the phone."""
from pathlib import Path
import sys, json, hashlib
from PIL import Image, ImageDraw, ImageFont
import numpy as np

root=Path(__file__).resolve().parents[1]
folder=root/'evidence/r05-pdf'
folder.mkdir(exist_ok=True)
if sys.argv[1]=='prepare':
    from reportlab.pdfgen import canvas
    from reportlab.lib.utils import ImageReader
    w,h=2480,3508
    rng=np.random.default_rng(505)
    pixels=np.empty((h,w,3),dtype=np.uint8)
    for y in range(h):
        row=np.clip(244+7*y/h+rng.integers(-3,4,(w,1)),0,255).astype(np.uint8)
        pixels[y]=row
    image=Image.fromarray(pixels)
    d=ImageDraw.Draw(image)
    font=lambda size:ImageFont.truetype('C:/Windows/Fonts/arial.ttf',size)
    d.text((170,120),'OTO SCAN - UJI KOMPRESI SINTETIS',fill=(10,35,60),font=font(62))
    d.text((170,215),'Bukan dokumen pengguna. Tulisan 6, 8, 10, dan 12 pt pada raster 300 dpi.',fill=(25,25,25),font=font(33))
    for i,points in enumerate([6,8,10,12]):
        y=340+i*125
        d.text((170,y),f'{points} pt: Nomor 0123456789 | B8 O0 I1 l | Qty 15,25 | Teknik dan dokumen',fill=(12,12,12),font=font(round(points*300/72)))
    for y in range(900,2700,95):
        d.line((170,y,2310,y),fill=(70,70,70),width=2)
        d.text((190,y+18),f'{(y-900)//95+1:02d}  Pemeriksaan peralatan  |  128/80  |  Normal  |  Rp 1.234.567,89',fill=(20,20,20),font=font(31))
    d.rectangle((1750,2850,2300,3210),outline=(180,32,45),width=5)
    d.text((1810,2910),'STEMPEL UJI',fill=(180,32,45),font=font(43))
    d.text((1810,3000),'Warna dipertahankan',fill=(45,80,160),font=font(26))
    for i in range(7):
        d.line((180,2900+i*35,1450,2900+i*35),fill=(20,20,20),width=i+1)
    d.text((170,3360),'Uji detail kecil dan garis tipis - halaman sintetis 1',fill=(30,30,30),font=font(28))
    image.save(folder/'source.png')
    image.convert('L').point(lambda v:255 if v>=155 else 0,mode='1').save(folder/'binary.png')
    c=canvas.Canvas(str(folder/'lossless-reference.pdf'),pagesize=(595,595*h/w),pageCompression=1)
    c.drawImage(ImageReader(image),0,0,width=595,height=595*h/w);c.showPage();c.save()
    orient=Image.new('RGB',(600,400),'white');od=ImageDraw.Draw(orient)
    for box,color in [((0,0,299,199),(255,0,0)),((300,0,599,199),(0,255,0)),((0,200,299,399),(0,0,255))]:
        od.rectangle(box,fill=color)
    orient.save(folder/'orientation.png')
    print('Prepared synthetic 2480x3508 A4 fixture, binary counterpart, landscape orientation and lossless reference.')
    sys.exit(0)

from pypdf import PdfReader
import pypdfium2 as pdfium
checks=[]
def check(label,condition):
    checks.append({'name':label,'pass':bool(condition)})
    if not condition: print('FAIL',label)
for name,count in [('compact-colour.pdf',1),('compact-mono.pdf',1),('three-pages.pdf',3),('twenty-pages.pdf',20)]:
    reader=PdfReader(folder/name,strict=True)
    check(name+' strict page count',len(reader.pages)==count)
    check(name+' no actions or attachments',not any(k in reader.trailer['/Root'] for k in ['/OpenAction','/AA','/Names','/AcroForm']))
    doc=pdfium.PdfDocument(folder/name)
    for i,p in enumerate(reader.pages):
        xo=p['/Resources']['/XObject']['/Im0'].get_object()
        w,h=int(xo['/Width']),int(xo['/Height'])
        check(f'{name} page {i+1} aspect ratio',abs(float(p.mediabox.width)/float(p.mediabox.height)-w/h)<.00001)
        check(f'{name} page {i+1} colour/bits/filter', (xo['/Filter']=='/DCTDecode' and xo['/BitsPerComponent']==8 and xo['/ColorSpace']=='/DeviceRGB') or (xo['/Filter']=='/FlateDecode' and xo['/BitsPerComponent']==1 and xo['/ColorSpace']=='/DeviceGray'))
        page=doc[i];bitmap=page.render(scale=1);render=bitmap.to_pil().convert('RGB')
        check(f'{name} page {i+1} independent rendering',render.getextrema()[0][0]<180)
        if name=='three-pages.pdf' and i==2:
            for j,(fx,fy,color) in enumerate([(.25,.25,(255,0,0)),(.75,.25,(0,255,0)),(.25,.75,(0,0,255)),(.75,.75,(255,255,255))]):
                actual=render.getpixel((int(render.width*fx),int(render.height*fy)))
                check('landscape orientation quadrant '+str(j),max(abs(a-b) for a,b in zip(actual,color))<6)
        if name in ['compact-colour.pdf','compact-mono.pdf']:render.save(folder/(name+'.png'))
        bitmap.close();page.close()
    doc.close()
source=np.asarray(Image.open(folder/'source.png').convert('RGB')).astype(np.int16)
jpeg=np.asarray(Image.open(folder/'encoded.jpg').convert('RGB')).astype(np.int16)
mae=float(np.mean(np.abs(source-jpeg)))
ink=np.mean(source[300:850],axis=2)<80
ink_recall=float(np.mean(np.mean(jpeg[300:850],axis=2)[ink]<110))
check('JPEG keeps original 300dpi dimensions',source.shape==jpeg.shape)
check('dense small-text dark ink retention above 99 percent',ink_recall>.99)
check('average RGB error below 3 levels out of 255',mae<3)
reader=PdfReader(folder/'compact-colour.pdf',strict=True)
xo=reader.pages[0]['/Resources']['/XObject']['/Im0'].get_object()
check('JPEG embedded byte-for-byte without another recompression',xo._data==(folder/'encoded.jpg').read_bytes())
reader=PdfReader(folder/'compact-mono.pdf',strict=True)
xo=reader.pages[0]['/Resources']['/XObject']['/Im0'].get_object()
expected=Image.open(folder/'binary.png').convert('1')
check('lossless monochrome retains all source pixels',xo.get_data()==expected.tobytes())
sizes={p.name:p.stat().st_size for p in folder.glob('*.pdf')}
check('colour PDF smaller than lossless RGB reference',sizes['compact-colour.pdf']<sizes['lossless-reference.pdf']*.5)
check('binary PDF smaller than 150KB',sizes['compact-mono.pdf']<150_000)
doc=pdfium.PdfDocument(folder/'compact-colour.pdf')
page=doc[0];bitmap=page.render(scale=2480/595)
full=bitmap.to_pil().convert('RGB')
full.crop((140,315,2350,840)).save(folder/'text-zoom.png')
full.crop((1720,2820,2330,3240)).save(folder/'colour-zoom.png')
bitmap.close();page.close();doc.close()
result={'scope':'Desktop synthetic fixtures and Java ImageIO encoder; not Android camera or device encoder performance','checks':checks,'passed':sum(c['pass'] for c in checks),'total':len(checks),'all_passed':all(c['pass'] for c in checks),'sizes_bytes':sizes,'compression_percent_vs_lossless_reference':round(100*(1-sizes['compact-colour.pdf']/sizes['lossless-reference.pdf']),2),'rgb_mean_absolute_error':mae,'small_text_dark_ink_retention':ink_recall,'sha256':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in folder.glob('*.pdf')}}
(folder/'verification.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in result.items() if k not in ['checks','sha256']},indent=2))
sys.exit(0 if result['all_passed'] else 1)
