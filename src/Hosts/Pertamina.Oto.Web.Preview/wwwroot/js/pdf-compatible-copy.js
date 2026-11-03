/* Explicit derivative copies only. Original file bytes are never written.
 * The ordinary strict input validator remains the final admission authority. */
(function(root){'use strict';
 const MAX_BYTES=128*1024*1024,MAX_PAGES=200,MAX_PIXELS=16*1024*1024;
 const fail=code=>{throw Error(code);};
 async function blank(P){const d=await P.PDFDocument.create();d.addPage([10,10]);return d.save({useObjectStreams:false});}
 function num(P,v){if(!(v instanceof P.PDFNumber)||!Number.isFinite(v.asNumber()))fail('COPY_RENDER');return v.asNumber();}
 function appearance(P,d,page,a){
  const k=P.PDFName.of,lookup=(dict,key)=>dict?.lookup(k(key));
  const subtype=lookup(a,'Subtype')?.decodeText?.(),flags=lookup(a,'F')?.asNumber?.()||0;
  // Hidden/invisible annotations and navigation have no page appearance to copy.
  if(flags&3||['Link','Popup'].includes(subtype))return;
  const rect=lookup(a,'Rect');if(!(rect instanceof P.PDFArray)||rect.size()!==4)fail('COPY_RENDER');
  const [x0,y0,x1,y1]=rect.asArray().map(v=>num(P,d.context.lookup(v))),x=Math.min(x0,x1),y=Math.min(y0,y1),w=Math.abs(x1-x0),h=Math.abs(y1-y0);
  if(!w||!h)return;
  let ap=lookup(lookup(a,'AP'),'N');
  if(ap instanceof P.PDFDict){const state=lookup(a,'AS');ap=state instanceof P.PDFName?ap.lookup(state):ap.lookup(k('Off'));}
  if(!(ap instanceof P.PDFRawStream)){
   // An empty/invisible signature has no visible signature to flatten.
   let field=a;const seen=new Set();for(let i=0;i<32&&field instanceof P.PDFDict&&!seen.has(field);i++){
    seen.add(field);if(lookup(field,'FT')===k('Sig')&&!lookup(field,'V'))return;field=lookup(field,'Parent');
   }
   fail('COPY_RENDER');
  }
  if(subtype==='Redact')fail('COPY_RENDER');
  const bbox=lookup(ap.dict,'BBox'),mat=lookup(ap.dict,'Matrix');
  if(!(bbox instanceof P.PDFArray)||bbox.size()!==4||mat&&(!(mat instanceof P.PDFArray)||mat.size()!==6))fail('COPY_RENDER');
  const b=bbox.asArray().map(v=>num(P,d.context.lookup(v))),m=mat?mat.asArray().map(v=>num(P,d.context.lookup(v))):[1,0,0,1,0,0];
  const points=[[b[0],b[1]],[b[0],b[3]],[b[2],b[1]],[b[2],b[3]]].map(([u,v])=>[m[0]*u+m[2]*v+m[4],m[1]*u+m[3]*v+m[5]]);
  const bx=Math.min(...points.map(p=>p[0])),by=Math.min(...points.map(p=>p[1])),bw=Math.max(...points.map(p=>p[0]))-bx,bh=Math.max(...points.map(p=>p[1]))-by;
  if(!(bw>0&&bh>0)||[x,y,w,h,bx,by,bw,bh].some(v=>Math.abs(v)>1e7))fail('COPY_RENDER');
  const name=page.node.newXObject('OtoStatic',d.context.register(ap));
  page.pushOperators(P.pushGraphicsState(),P.concatTransformationMatrix(w/bw,0,0,h/bh,x-bx*w/bw,y-by*h/bh),P.drawObject(name),P.popGraphicsState());
 }
 function clean(P,d){
  const forbidden=new Set('AA JavaScript JS OpenAction EmbeddedFiles Filespec EF AF Perms Sig XFA OPI AcroForm StructTreeRoot StructParents StructParent'.split(' '));
  const seen=new Set(),queue=[d.catalog];let visits=0;
  while(queue.length){let v=queue.pop();if(v instanceof P.PDFRef)v=d.context.lookup(v);if(!v||seen.has(v))continue;seen.add(v);if(++visits>80000)fail('LIMIT');
   if(v instanceof P.PDFRawStream)queue.push(v.dict);
   else if(v instanceof P.PDFDict)for(const [k,value] of v.entries()){if(forbidden.has(k.decodeText()))v.delete(k);else queue.push(value);}
   else if(v instanceof P.PDFArray)queue.push(...v.asArray());
  }
 }
 async function normalize(P,E,bytes,{dropLast=0}={}){
  if(!(bytes instanceof Uint8Array)||bytes.length>MAX_BYTES)fail('COPY_LIMIT');
  const d=await P.PDFDocument.load(bytes,{ignoreEncryption:false,updateMetadata:false,throwOnInvalidObject:true}),k=P.PDFName.of;
  const count=d.getPageCount()-dropLast;if(!Number.isInteger(count)||count<1||count>MAX_PAGES||![0,1].includes(dropLast))fail('COPY_LIMIT');
  const form=d.catalog.lookup(k('AcroForm'));
  if(form instanceof P.PDFDict&&form.has(k('XFA')))fail('COPY_XFA');
  // Populate missing field appearances using existing form values. Existing APs
  // are kept unless the form explicitly requests new appearances.
  if(form instanceof P.PDFDict){
   try{const f=d.getForm();if(form.lookup(k('NeedAppearances'))===P.PDFBool.True)for(const field of f.getFields())f.markFieldAsDirty(field.ref);f.updateFieldAppearances();}
   catch{fail('COPY_RENDER');}
  }
  for(const page of d.getPages().slice(0,count)){
   const annots=page.node.lookup(k('Annots'));
   if(annots instanceof P.PDFArray)for(const ref of annots.asArray()){
    const a=d.context.lookup(ref);if(!(a instanceof P.PDFDict))continue;
    appearance(P,d,page,a);
   }
   // Page trees are rebuilt below. Annotations, actions and accessibility tags
   // are explicitly excluded from this disclosed non-interactive derivative.
   for(const name of ['Annots','AA','Metadata','StructParents','PresSteps','B','VP'])page.node.delete(k(name));
  }
  const out=await P.PDFDocument.create();
  for(const page of await out.copyPages(d,Array.from({length:count},(_,i)=>i)))out.addPage(page);
  clean(P,out);out.setProducer('PERTAMINA OTO Web - salinan noninteraktif');
  E.inspectStructures(P,out);
  const output=await out.save({useObjectStreams:false,addDefaultPage:false});if(output.length>MAX_BYTES)fail('COPY_LIMIT');
  const verified=await P.PDFDocument.load(output,{throwOnInvalidObject:true});E.inspectStructures(P,verified);if(verified.getPageCount()!==count)fail('VERIFY');
  return output;
 }
 async function rasterDocument(P,E,pages){
  if(!Array.isArray(pages)||!pages.length||pages.length>MAX_PAGES)fail('COPY_LIMIT');
  const d=await P.PDFDocument.create();let total=0;
  for(const p of pages){
   if(!Number.isFinite(p.width)||!Number.isFinite(p.height)||p.width<=0||p.height<=0||p.width>14400||p.height>14400||!(p.bytes instanceof Uint8Array)||(total+=p.bytes.length)>MAX_BYTES)fail('COPY_LIMIT');
   const image=await d.embedPng(p.bytes);if(image.width*image.height>MAX_PIXELS)fail('COPY_LIMIT');
   d.addPage([p.width,p.height]).drawImage(image,{x:0,y:0,width:p.width,height:p.height});
  }
  d.setProducer('PERTAMINA OTO Web - salinan tampilan statis');await d.flush();E.inspectStructures(P,d);
  const bytes=await d.save({useObjectStreams:false,addDefaultPage:false});if(bytes.length>MAX_BYTES)fail('COPY_LIMIT');return bytes;
 }
 const api={MAX_BYTES,MAX_PAGES,MAX_PIXELS,blank,normalize,rasterDocument};if(typeof module!=='undefined')module.exports=api;else root.OtoCompatibleCopy=api;
})(typeof self!=='undefined'?self:globalThis);
