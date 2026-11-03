"""Local diagnostics only: retain numeric evidence, never document content."""
import json, pathlib, hashlib, math
from PIL import Image
from pypdf import PdfReader
import pypdfium2 as pdfium
root=pathlib.Path(__file__).resolve().parents[1]
source=Image.open(root/'evidence/device/r03-source.jpg')
crop=Image.open(root/'evidence/device/r03-crop.jpg')
page=json.loads((root/'evidence/camera-page-r03.json').read_text())
pdfpath=root/'evidence/device/r03-saved.pdf'
reader=PdfReader(pdfpath)
pdf=pdfium.PdfDocument(str(pdfpath))
render=pdf[0].render(scale=1).to_pil()
# Approximate corners observed in the source rendered at 1368 x 1824.
# This is visual review (about +/-4 source pixels), not ground truth calibration.
oracle=[(319,326),(1084,367),(1130,1481),(270,1498)]
oracle=[(x*source.width/1368,y*source.height/1824) for x,y in oracle]
actual=[(x*(source.width-1),y*(source.height-1)) for x,y in page['corners']]
errors=[math.dist(a,b) for a,b in zip(oracle,actual)]
report={'auto_detected':page['detected'],'manual_corner_edits':0,'rotation_edits':page['turns'],
 'source_dimensions':list(source.size),'crop_dimensions':list(crop.size),
 'visual_corner_error_px':[round(e,2) for e in errors], 'visual_annotation_uncertainty_px':4,
 'pdf_pages':len(reader.pages), 'pdf_bytes':pdfpath.stat().st_size,
 'pdf_sha256':hashlib.sha256(pdfpath.read_bytes()).hexdigest(),
 'independent_pdf_render_size':list(render.size),
 'pdf_image_dimensions':[[i.image.width,i.image.height] for i in reader.pages[0].images],
 'checks':{'auto_crop':page['detected'] and page['turns']==0,
  'corners_within_15px_of_visual_annotation':max(errors)<15,
  'pdf_one_page':len(reader.pages)==1,'independent_render_nonempty':render.getextrema()!=((255,255),(255,255),(255,255)),
  'pdf_preserves_crop_pixels':any(i.image.size==crop.size for i in reader.pages[0].images)}}
assert all(report['checks'].values()),report
(root/'evidence/camera-verification-r03.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
pdf.close()
