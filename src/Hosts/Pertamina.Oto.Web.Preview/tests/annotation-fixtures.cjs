'use strict';
const {P,key,tagged}=require('./tagged-fixtures.cjs');
async function annotated({tags=false,count=2,cross=false,named=false,edit=()=>{}}={}){
  let state;
  const d=tags?await P.PDFDocument.load(await tagged({count,edit:(doc,s)=>{state=s;}})):await P.PDFDocument.create();
  if(!tags)for(let i=0;i<count;i++){const p=d.addPage([400,500]);p.drawText('ANNOTATED PAGE '+(i+1),{x:40,y:440,size:18});p.drawText('Original selectable text.',{x:40,y:390,size:12});}
  const c=d.context,records=[],pages=d.getPages(),root=tags?d.catalog.lookup(key('StructTreeRoot'),P.PDFDict):null;
  let next=count;
  const font=await d.embedFont(P.StandardFonts.Helvetica);
  function add(page,subtype,rect,attrs={}){const dict=c.obj({Type:'Annot',Subtype:subtype,Rect:rect,P:page.ref,F:4,Contents:P.PDFString.of('Synthetic '+subtype),NM:P.PDFString.of('a-'+records.length),...attrs}),ref=c.register(dict);page.node.addAnnot(ref);const r={dict,ref,page:pages.indexOf(page),subtype};records.push(r);return r;}
  for(let i=0;i<count;i++){
    const p=pages[i];
    const note=add(p,'Text',[330,400,350,420],{T:P.PDFString.of('Synthetic reviewer'),Name:'Comment',Open:false});
    const popup=add(p,'Popup',[180,340,320,430],{Parent:note.ref,Open:false});note.dict.set(key('Popup'),popup.ref);
    add(p,'Text',[355,400,375,420],{IRT:note.ref,RT:'R',T:P.PDFString.of('Synthetic reply'),Name:'Note',Open:false});
    const ap=c.register(c.stream('q 1 0.9 0.2 rg 0 0 220 20 re f Q',{Type:'XObject',Subtype:'Form',BBox:[0,0,220,20],Resources:{}}));
    add(p,'Highlight',[40,310,260,330],{QuadPoints:[40,330,260,330,40,310,260,310],C:[1,.9,.2],AP:{N:ap}});
    add(p,'Square',[280,300,350,345],{C:[0,.3,.8],BS:{W:2,S:'S'},AP:{N:c.register(c.stream('q 0 0.3 0.8 RG 2 w 1 1 68 43 re S Q',{Type:'XObject',Subtype:'Form',BBox:[0,0,70,45],Resources:{}}))}});
    add(p,'Link',[40,170,240,190],{Border:[0,0,1],C:[0,0,1],A:{S:'URI',URI:P.PDFString.of('https://example.org/guide?page='+i)}});
    const dest=c.obj([pages[cross?(i+1)%count:i].ref,'XYZ',null,450,null]);
    const name='target-'+i;if(named){let names=d.catalog.lookupMaybe(key('Names'),P.PDFDict);if(!names){names=c.obj({Dests:{Names:[]}});d.catalog.set(key('Names'),names);}names.lookup(key('Dests'),P.PDFDict).lookup(key('Names'),P.PDFArray).push(P.PDFString.of(name));names.lookup(key('Dests'),P.PDFDict).lookup(key('Names'),P.PDFArray).push(c.obj({D:dest}));}
    add(p,'Link',[40,120,240,140],{Border:[0,0,1],C:[0,0,1],A:{S:'GoTo',D:named?P.PDFString.of(name):dest}});
    const freeAp=c.register(c.stream('BT /F1 12 Tf 0 0.3 0.8 rg 2 8 Td (Preserved comment) Tj ET',{Type:'XObject',Subtype:'Form',BBox:[0,0,220,28],Resources:{Font:{F1:font.ref}}}));
    add(p,'FreeText',[40,60,260,88],{DA:P.PDFString.of('/Helv 12 Tf 0 g'),AP:{N:freeAp},Q:0});
    if(tags){
      const document=root.lookup(key('K'),P.PDFDict),section=document.lookup(key('K'),P.PDFArray).lookup(i,P.PDFDict),sectionRef=c.getObjectRef(section);
      for(const r of records.filter(r=>r.page===i&&r.subtype!=='Popup')){
        const node=c.obj({Type:'StructElem',S:r.subtype==='Link'?'Link':'Annot',P:sectionRef,Pg:p.ref,Alt:P.PDFString.of('Accessible '+r.subtype),K:{Type:'OBJR',Pg:p.ref,Obj:r.ref}}),ref=c.register(node);
        section.lookup(key('K'),P.PDFArray).push(ref);r.dict.set(key('StructParent'),P.PDFNumber.of(next));const nums=root.lookup(key('ParentTree'),P.PDFDict).lookup(key('Nums'),P.PDFArray);nums.push(P.PDFNumber.of(next++));nums.push(ref);
      }
    }
  }
  if(tags)root.set(key('ParentTreeNextKey'),P.PDFNumber.of(next));
  await edit(d,{records,pages,root});return d.save({useObjectStreams:false});
}
module.exports={annotated,P,key};
