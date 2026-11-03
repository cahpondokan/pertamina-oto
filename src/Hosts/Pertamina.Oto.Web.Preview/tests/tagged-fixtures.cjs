'use strict';
const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js');
const key=s=>P.PDFName.of(s);
async function tagged({name='A',count=4,spanningTable=false,edit=()=>{}}={}){
 const d=await P.PDFDocument.create(),c=d.context,root=c.obj({Type:'StructTreeRoot'}),rootRef=c.register(root);
 const document=c.obj({Type:'StructElem',S:'Document',P:rootRef}),docRef=c.register(document),top=[],parents=[],ids=[],sections=[];
 d.catalog.set(key('StructTreeRoot'),rootRef);d.catalog.set(key('MarkInfo'),c.obj({Marked:true}));d.catalog.set(key('Lang'),P.PDFString.of('id-ID'));d.setTitle('Synthetic tagged '+name);
 root.set(key('K'),docRef);root.set(key('RoleMap'),c.obj({CustomP:'P'}));root.set(key('ClassMap'),c.obj({Body:{O:'Layout',TextAlign:'Start'}}));
 d.catalog.set(key('PageLabels'),c.obj({Nums:[0,{S:'r',St:1}]}));
 const font=await d.embedFont(P.StandardFonts.Helvetica);
 function node(role,parent,page,kids,extra={}){const dict=c.obj({Type:'StructElem',S:role,P:parent,Pg:page.ref,K:kids,...extra});const ref=c.register(dict);if(extra.ID)ids.push([extra.ID,ref]);return {dict,ref};}
 for(let i=0;i<count;i++){
  const page=d.addPage([400,500]),owners=[];page.node.set(key('StructParents'),P.PDFNumber.of(i));
  const sect=node('Sect',docRef,page,[]);sections.push(sect);top.push(sect.ref);
  const id=P.PDFString.of('heading-'+i),heading=node('H1',sect.ref,page,0,{ID:id,T:P.PDFString.of('Heading '+i)});
  const paragraph=node('CustomP',sect.ref,page,c.obj({Type:'MCR',Pg:page.ref,MCID:1}),{C:'Body',Lang:P.PDFString.of('id-ID'),ActualText:P.PDFString.of(name+' paragraph '+(i+1)),Ref:[heading.ref]});
  const figure=node('Figure',sect.ref,page,2,{Alt:P.PDFString.of(name+' accessible figure '+(i+1)),A:{O:'Layout',BBox:[40,220,120,280]}});
  sect.dict.set(key('K'),c.obj([heading.ref,paragraph.ref,figure.ref]));owners.push(heading.ref,paragraph.ref,figure.ref);
  for(const [mcid,role,text,y] of [[0,'H1','TAG '+name+' PAGE '+(i+1),440],[1,'P','Text paragraph '+(i+1)+' remains selectable.',390],[2,'Figure','[SYNTHETIC FIGURE]',250]]){
   page.pushOperators(P.PDFOperator.of('BDC',[key(role),c.obj({MCID:mcid})]));page.drawText(text,{x:40,y,size:mcid===0?18:12,font});page.pushOperators(P.PDFOperator.of('EMC'));
  }
  parents.push(i,c.obj(owners));
 }
 if(spanningTable){
  const table=c.obj({Type:'StructElem',S:'Table',P:docRef,K:top}),tableRef=c.register(table);
  for(let i=0;i<sections.length;i++){
   const sect=sections[i];sect.dict.set(key('S'),key('TR'));sect.dict.set(key('P'),tableRef);
   const cells=sect.dict.lookup(key('K'),P.PDFArray);
   for(let j=0;j<cells.size();j++){
    const cell=cells.lookup(j,P.PDFDict);cell.set(key('S'),key(j===0?'TH':'TD'));
    cell.set(key('A'),j===0?c.obj({O:'Table',Scope:'Row'}):c.obj({O:'Table',Headers:[P.PDFString.of('heading-'+i)]}));
   }
  }
  document.set(key('K'),c.obj([tableRef]));
 }else document.set(key('K'),c.obj(top));
 root.set(key('ParentTree'),c.obj({Nums:parents}));root.set(key('ParentTreeNextKey'),P.PDFNumber.of(count));root.set(key('IDTree'),c.obj({Names:ids.sort((a,b)=>a[0].toString().localeCompare(b[0].toString())).flat()}));
 await edit(d,{root,rootRef,document,docRef,sections,parents,ids});
 return d.save({useObjectStreams:false});
}
module.exports={tagged,P,key};
