from pathlib import Path
import zipfile,xml.etree.ElementTree as E,posixpath as P,hashlib,json,re
base=Path('artifacts/user-guide-20261008'); build=base/'ppt-build'
source=Path('D:/Download/PCP P-OTO v1.pptx')
ns={'p':'http://schemas.openxmlformats.org/presentationml/2006/main','a':'http://schemas.openxmlformats.org/drawingml/2006/main','r':'http://schemas.openxmlformats.org/officeDocument/2006/relationships'}
R='http://schemas.openxmlformats.org/package/2006/relationships';C='http://schemas.openxmlformats.org/package/2006/content-types'
for k,v in ns.items():E.register_namespace(k,v)
def xml(x):
 tree=E.fromstring(x)
 # Preserve declarations referenced by compatibility attributes (mc:Ignorable).
 for prefix,uri in re.findall(rb'xmlns:([\w]+)="([^"]+)"',x):
  key=prefix.decode(); val=uri.decode()
  if val not in ns.values():tree.set('xmlns:'+key,val)
 return tree
def serial(x):
 uri=x.tag.split('}')[0][1:]
 if uri in [R,C]:E.register_namespace('',uri)
 return E.tostring(x,encoding='utf-8',xml_declaration=True)
with zipfile.ZipFile(source) as z: original={n:z.read(n) for n in z.namelist()}
parts=dict(original);pres=xml(parts['ppt/presentation.xml']);rels=xml(parts['ppt/_rels/presentation.xml.rels']);types=xml(parts['[Content_Types].xml'])
sz=pres.find('p:sldSz',ns);cx,cy=int(sz.get('cx')),int(sz.get('cy'))
lst=pres.find('p:sldIdLst',ns);old_count=len(lst);sid=max(int(s.get('id')) for s in lst)
used={r.get('Id') for r in rels}
def new_rel(typ,target):
 n=1
 while 'rId'+str(n) in used:n+=1
 rid='rId'+str(n);used.add(rid);E.SubElement(rels,'{'+R+'}Relationship',Id=rid,Type=ns['r']+'/'+typ,Target=target);return rid
hashnames={hashlib.sha256(b).hexdigest():n for n,b in parts.items() if n.startswith('ppt/media/')}
for index,src in enumerate([build/'divider.pptx',base/'ppt/OTO-Dokumentasi-16-Tampilan-Final.pptx'],1):
 with zipfile.ZipFile(src) as z: annex={n:z.read(n) for n in z.namelist()}
 ap=xml(annex['ppt/presentation.xml']);ars=xml(annex['ppt/_rels/presentation.xml.rels']);asz=ap.find('p:sldSz',ns);factor=cx/int(asz.get('cx'))
 assert abs(factor-cy/int(asz.get('cy')))<1e-8
 mapping={}
 for n,b in annex.items():
  if not n.startswith('ppt/') or n in ['ppt/presentation.xml','ppt/_rels/presentation.xml.rels']:continue
  dest=P.join(P.dirname(n),'oto_appendix'+str(index)+'_'+P.basename(n))
  if n.endswith('.rels'):
   dest=P.join(P.dirname(n),'oto_appendix'+str(index)+'_'+P.basename(n))
  if n.startswith('ppt/media/'):
   h=hashlib.sha256(b).hexdigest();dest=hashnames.get(h,dest);hashnames[h]=dest
  mapping[n]=dest
 for n,dest in mapping.items():
  if dest in parts:continue
  data=annex[n]
  if n.endswith('.rels'):
   tree=xml(data);owner=P.join(P.dirname(P.dirname(n)),P.basename(n)[:-5]);new_owner=P.join(P.dirname(P.dirname(dest)),P.basename(dest)[:-5])
   for r in tree:
    if r.get('TargetMode')=='External':continue
    target=P.normpath(P.join(P.dirname(owner),r.get('Target'))).lstrip('/');newtarget=mapping.get(target)
    if newtarget:r.set('Target',P.relpath(newtarget,P.dirname(new_owner)))
   data=serial(tree)
  elif n.endswith('.xml') and any(s in n for s in ['/slides/','/slideLayouts/','/slideMasters/']):
   tree=xml(data)
   for el in tree.iter():
    local=el.tag.split('}')[-1]
    if local in ['off','ext','chOff','chExt','pt']:
     for attr in ['x','y','cx','cy']:
      if attr in el.attrib:el.set(attr,str(round(int(el.get(attr))*factor)))
    if local in ['rPr','defRPr','endParaRPr']:
     for attr in ['sz','kern']:
      if attr in el.attrib:el.set(attr,str(round(int(el.get(attr))*factor)))
    if local=='bodyPr':
     for attr in ['lIns','rIns','tIns','bIns']:
      if attr in el.attrib:el.set(attr,str(round(int(el.get(attr))*factor)))
   data=serial(tree)
  parts[dest]=data
 # Content types for mapped parts
 existing={e.get('PartName') or e.get('Extension') for e in types}
 for e in xml(annex['[Content_Types].xml']):
  if e.tag.endswith('Override'):
   n=e.get('PartName').lstrip('/');dest=mapping.get(n)
   if dest and '/'+dest not in existing:E.SubElement(types,'{'+C+'}Override',PartName='/'+dest,ContentType=e.get('ContentType'));existing.add('/'+dest)
  elif e.get('Extension') not in existing:E.SubElement(types,'{'+C+'}Default',Extension=e.get('Extension'),ContentType=e.get('ContentType'));existing.add(e.get('Extension'))
 amap={r.get('Id'):P.normpath(P.join('ppt',r.get('Target'))).lstrip('/') for r in ars}
 for s in ap.find('p:sldIdLst',ns):
  sid+=1;rid=new_rel('slide',P.relpath(mapping[amap[s.get('{'+ns['r']+'}id')]],'ppt'));E.SubElement(lst,'{'+ns['p']+'}sldId',{'id':str(sid),'{'+ns['r']+'}id':rid})
 for tag,reltype,idbase in [('sldMasterIdLst','slideMaster',2147484000),('notesMasterIdLst','notesMaster',None)]:
  al=ap.find('p:'+tag,ns)
  if al is None:continue
  pl=pres.find('p:'+tag,ns)
  if pl is None:
   pl=E.Element('{'+ns['p']+'}'+tag);pres.insert(1,pl)
  for s in al:
   rid=new_rel(reltype,P.relpath(mapping[amap[s.get('{'+ns['r']+'}id')]],'ppt'));attrs={'{'+ns['r']+'}id':rid}
   if idbase:attrs['id']=str(idbase+index)
   E.SubElement(pl,'{'+ns['p']+'}'+('sldMasterId' if idbase else 'notesMasterId'),attrs)
parts['ppt/presentation.xml']=serial(pres);parts['ppt/_rels/presentation.xml.rels']=serial(rels);parts['[Content_Types].xml']=serial(types)
if 'docProps/app.xml' in parts:
 app=xml(parts['docProps/app.xml'])
 for el in app.iter():
  if el.tag.endswith('}Slides'):el.text=str(len(lst))
 parts['docProps/app.xml']=serial(app)
target=build/'merged-candidate.pptx'
with zipfile.ZipFile(target,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for n,b in parts.items():z.writestr(n,b)
unchanged=[n for n in original if n not in ['ppt/presentation.xml','ppt/_rels/presentation.xml.rels','[Content_Types].xml','docProps/app.xml']]
assert all(parts[n]==original[n] for n in unchanged)
report={'sourceSlides':old_count,'finalSlides':len(lst),'addedDivider':1,'addedDocumentation':3,'sourcePartsBytePreserved':len(unchanged),'dimensionsEmu':[cx,cy],'losslessZipCompression':True,'sourceBytes':source.stat().st_size,'documentationBytes':(base/'ppt/OTO-Dokumentasi-16-Tampilan-Final.pptx').stat().st_size,'combinedBytes':target.stat().st_size,'allScreenshotBytesPreserved':True}
(build/'merge-report.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))


