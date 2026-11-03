"""Small synthetic image fixtures and independent PDF output checks; no user files."""
import sys, json
from pathlib import Path
from PIL import Image, ImageDraw
from pypdf import PdfReader
root=Path(sys.argv[1])
root.mkdir(parents=True,exist_ok=True)
if len(sys.argv)<3:
    im=Image.new('RGB',(400,600),'#eef4ff')
    d=ImageDraw.Draw(im);d.rectangle((20,20,380,110),fill='#075bcf');d.text((40,55),'OTO SYNTHETIC PORTRAIT',fill='white');d.rectangle((40,170,360,530),outline='#075bcf',width=5)
    im.save(root/'portrait.jpg',quality=95)
    im.convert('CMYK').save(root/'cmyk.jpg')
    alpha=Image.new('RGBA',(400,600),(0,0,0,0));ad=ImageDraw.Draw(alpha);ad.ellipse((40,100,360,420),fill=(0,160,110,190));ad.text((90,450),'TRANSPARENT PNG',fill='black');alpha.save(root/'transparent.png')
    land=Image.new('RGB',(600,400),'#fff3cf');ld=ImageDraw.Draw(land);ld.text((50,50),'OTO LANDSCAPE',fill='black');ld.rectangle((50,100,550,350),outline='#a45a00',width=5);land.save(root/'landscape.jpg',quality=95)
    exif=im.getexif();exif[274]=6;im.save(root/'orientation6.jpg',exif=exif)
    print('Created synthetic JPG, PNG, CMYK, landscape and EXIF fixtures.')
else:
    verified=[]
    for tool in ['numbers','watermark','crop']:
        for suffix in ['', '-rotated']:
            r=PdfReader(root/(tool+suffix+'.pdf'))
            assert len(r.pages)==4
            for i,p in enumerate(r.pages):
                text=p.extract_text()
                assert f'OTO TEST A{i+1}' in text
                if tool=='numbers':assert f'{i+1} / 4' in text
                if tool=='watermark':assert 'DRAF OTO' in text
                if tool=='crop':assert float(p.cropbox.width)<float(p.mediabox.width)
                if suffix:assert p.rotation==[0,90,180,270][i]
            verified.append({'file':tool+suffix+'.pdf','pages':4,'sourceTextPreserved':True})
    print(json.dumps({'independentReader':'pypdf','verified':verified},indent=2))
