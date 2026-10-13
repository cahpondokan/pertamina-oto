from pathlib import Path
from PIL import Image
import json, hashlib, sys
p=Path(__file__).resolve().parent.parent
s=json.loads((p/'task-03-status.json').read_text(encoding='utf-8-sig'))
for i in s['items']:
 f=p/i['filename']
 if f.exists():
  im=Image.open(f); a=im.getchannel('A'); bbox=a.getbbox()
  i.update(dimensions=list(im.size),alphaVerified=a.getextrema()==(0,255),fileHash=hashlib.sha256(f.read_bytes()).hexdigest(),alphaBounds=list(bbox),visualReviewed=True,state='NEEDS_REVIEW',smallPreviewTested=False)
  i.setdefault('issue','64–128px light/dark inspection pending')
(p/'task-03-status.json').write_text(json.dumps(s,indent=2,ensure_ascii=False),encoding='utf-8')
print('Checkpoint:',sum((p/i['filename']).exists() for i in s['items']),'of',len(s['items']))
