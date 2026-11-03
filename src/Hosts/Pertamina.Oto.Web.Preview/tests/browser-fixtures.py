"""Synthetic, non-sensitive documents. Independent ReportLab writer / pypdf reader."""
import sys, json
from pathlib import Path
from reportlab.pdfgen.canvas import Canvas
from pypdf import PdfReader, PdfWriter
root = Path(sys.argv[1])
root.mkdir(parents=True, exist_ok=True)
if len(sys.argv) < 3:
    for label, count in [('A',4),('B',2)]:
        c = Canvas(str(root / (label+'.pdf')), pagesize=(300,420))
        for page in range(1,count+1):
            c.setFillColorRGB(.02,.18,.4)
            c.rect(0,330,300,90,fill=1,stroke=0)
            c.setFillColorRGB(1,1,1)
            c.setFont('Helvetica-Bold',24)
            c.drawString(25,365,f'OTO TEST {label}{page}')
            c.setFillColorRGB(0,0,0)
            c.setFont('Helvetica',12)
            c.drawString(25,300,'Synthetic PDF - no real user data')
            c.drawString(25,270,f'Original page: {page}')
            c.rect(25,40,250,180)
            c.showPage()
        c.save()
    writer = PdfWriter(clone_from=root/'A.pdf')
    writer.encrypt('fixture-only')
    writer.write(root/'encrypted.pdf')
    c = Canvas(str(root/'interactive.pdf'))
    c.drawString(20,750,'Synthetic form')
    c.acroForm.textfield(name='test',x=20,y=700,width=100,height=20)
    c.showPage()
    c.save()
    (root/'bad.pdf').write_bytes(b'%PDF-1.7 broken')
    print('Created synthetic fixtures A(4), B(2), encrypted, interactive, bad.')
else:
    expected={'merge':['A1','A2','A3','A4','B1','B2'],'organize':['B2','A3','A1'],'extract':['A4','A1','A2'],'remove':['A1','A3'],'rotate':['A1','A2','A3','A4'],'split_01':['A1','A2'],'split_02':['A3','A4']}
    checked=[]
    for name, ids in expected.items():
        reader=PdfReader(root/(name+'.pdf'))
        assert len(reader.pages)==len(ids),name
        for page,marker in zip(reader.pages,ids):
            assert 'OTO TEST '+marker in page.extract_text(),(name,marker)
            assert abs(float(page.mediabox.width)-300)<.01 and abs(float(page.mediabox.height)-420)<.01
        if name=='rotate': assert [p.rotation for p in reader.pages]==[0,90,90,0]
        if name=='organize': assert [p.rotation for p in reader.pages]==[90,0,180]
        checked.append({'file':name+'.pdf','pages':len(ids),'contentOrder':ids})
    print(json.dumps({'independentReader':'pypdf','pass':checked},indent=2))
