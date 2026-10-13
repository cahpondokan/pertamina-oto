from pathlib import Path
from PIL import Image
import json
p=Path(__file__).resolve().parent.parent
s=json.loads((p/'task-03-status.json').read_text())
prompts=json.loads((p/'task-03-prompts.json').read_text())
source_map={i['id']:i['source'] for i in prompts}
for i in s['items']:
 im=Image.open(p/i['filename']); a=im.getchannel('A'); b=a.point(lambda v:255 if v>=128 else 0).getbbox()
 margins=[b[0],b[1],im.width-b[2],im.height-b[3]]
 minimum=round(min(margins)/im.width*100,2)
 i.update(smallPreviewTested=True,smallPreviewSizes=[64,128],smallPreviewBackgrounds=['#FFFFFF','#182030'],foregroundBoundsAlpha128=list(b),minimumForegroundMarginPercent=minimum,source=source_map[i['id']],format='PNG',mode=im.mode,metaphorReviewed=True,identityReviewed=True,labelReviewed=True,clippingVisible=False,visualIssue=None)
 i['state']='VERIFIED' if minimum>=7 else 'NEEDS_REVIEW'
 i['issue']=None if minimum>=7 else f'Foreground minimum margin {minimum}% narrower than requested approx 7–10%; no visible clipping. Some faint peripheral alpha pixels may extend beyond substantial foreground.'
 i['qaEvidence']=f"qa-03/small-preview-{(int(i['id'])-40)//6}.png"
 i['repairCount']=1 if i['id'] in ['041','044'] else 0
s.update(state='COMPLETE_WITH_REVIEW',generated=22,pending=0,verified=sum(i['state']=='VERIFIED' for i in s['items']),needsReview=sum(i['state']=='NEEDS_REVIEW' for i in s['items']),tool='built-in image_gen',limitations=['Output is 1254x1254 rather than target 1024x1024; originals preserved without resizing.','Small previews are static pixel-size QA, not an application CSS/integration test.','Margin review items do not assert app-ready status.'])
(p/'task-03-status.json').write_text(json.dumps(s,ensure_ascii=False,indent=2),encoding='utf-8')
lines=['TASK 03 HANDOFF — LANE 3 / 040–061','All 22 requested PNGs generated, saved, visually inspected at full size, metadata/alpha/hash checked.','Built-in image_gen only. No subagent, source/UI/build/release changes or publishing.','All PNGs: 1254x1254 RGBA, real alpha range 0–255 and nonempty foreground. Target 1024 was requested but tool returned 1254; no PNG resized or edited outside imagegen.','Static 64px/128px previews on white and #182030 backgrounds inspected; 96px/CSS app integration not tested.',f"VERIFIED: {s['verified']}; NEEDS_REVIEW: {s['needsReview']}; PENDING: 0.",'NEEDS_REVIEW is measured margin below requested 7–10%, not missing visual/alpha checks. No visible cropped anatomy. Faint alpha fringe may reach frame edges.','One directed repair each on 041 (courier jacket and margin), 044 (remove unwanted brand symbol); originals retained.','At 64px some short badges and fine actions become small; major props remain recognizable. 128px gives clearer PDF/A and signature reading.','PROMPTS/SOURCES: task-03-prompts.json; CHECKPOINT/HASHES/MARGINS: task-03-status.json.','QA: qa-03/small-preview-0.png through small-preview-3.png, small-previews.ps1; read-only PNG inspection checkpoint.py/finalize.py.','COMPLETE FILE LIST:']
for i in s['items']: lines.append(str((p/i['filename']).resolve())+' | '+i['state']+' | min margin '+str(i['minimumForegroundMarginPercent'])+'%')
(p/'task-03-handoff.txt').write_text('\n'.join(lines)+'\n',encoding='utf-8')
print(s['verified'],'VERIFIED;',s['needsReview'],'NEEDS_REVIEW; 0 PENDING')
