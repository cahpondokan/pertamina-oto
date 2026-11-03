"""Synthetic images only: no real identities, camera pictures or private data."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageFilter
import json

root = Path(__file__).resolve().parents[1]/'evidence/mlkit-trial/fixtures'
root.mkdir(parents=True, exist_ok=True)
font_path = 'C:/Windows/Fonts/arial.ttf'
bold_path = 'C:/Windows/Fonts/arialbd.ttf'
records=[]
for name, size, box in [
    ('portrait',(1800,2400),(220,200,1580,2200)),
    ('landscape',(2400,1800),(200,220,2200,1580)),
    ('card',(1800,2400),(280,810,1520,1590)),
]:
    desk=Image.new('RGB',size,(48,61,69))
    paper=Image.new('RGB',(box[2]-box[0],box[3]-box[1]),'white')
    d=ImageDraw.Draw(paper)
    d.text((70,70),'OTO SCAN - DOKUMEN UJI',font=ImageFont.truetype(bold_path,48),fill='#152838')
    d.text((70,144),'DATA BUATAN | TIDAK BERISI DATA PRIBADI',font=ImageFont.truetype(font_path,25),fill='#333333')
    width,height=paper.size
    for row,y in enumerate(range(230,height-80,62)):
        d.text((70,y),f'{row+1:02}. Tulisan uji ABCDEFG 0123456789',font=ImageFont.truetype(font_path,29),fill='black')
        d.line((70,y+44,width-70,y+44),fill='#999999',width=2)
    # Mild lighting gradient checks enhancement without destroying known text.
    pixels=paper.load()
    for y in range(height):
        shade=0.84+0.16*y/max(1,height-1)
        for x in range(width):
            pixels[x,y]=tuple(round(v*shade) for v in pixels[x,y])
    desk.paste(paper,box[:2])
    desk.save(root/(name+'.jpg'),quality=98,subsampling=0)
    paper.save(root/(name+'-paper.png'))
    records.append({'name':name,'size':size,'paperBox':box,'paperSize':paper.size})
(root/'manifest.json').write_text(json.dumps(records,indent=2))
print(json.dumps(records))
