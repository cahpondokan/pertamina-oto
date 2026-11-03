/* Local File-based operations. Shared random-access parser and streaming writer;
 * the byte ceiling never reserves input-size memory. No network or persistence API. */
(function(root){
 'use strict';
 const local=typeof module!=='undefined',C=local?require('./pdf-capacity.js'):root.OtoPdfCapacity;
 const F=local?require('./pdf-merge-engine.js'):root.OtoMergeEngine;
 const T=local?require('./pdf-tags.js'):root.OtoPdfTags,A=local?require('./pdf-annotations.js'):root.OtoPdfAnnotations;
 const I=local?require('./pdf-file-images.js'):root.OtoFileImages;
 const fail=code=>{throw Error(code);};
 function admission(files,tool){
  const limit=C.input(tool);if(tool==='merge')fail('TOOL');
  if(!Array.isArray(files)||!files.length)fail('EMPTY');
  if(!limit.multiple&&files.length!==1)fail('SINGLE');
  if(files.length>limit.files)fail('FILE_COUNT');
  let total=0;for(const file of files){if(!Number.isSafeInteger(file?.size)||file.size<8)fail('INVALID');total+=file.size;if(file.size>limit.fileBytes||total>limit.totalBytes)fail('TOTAL_BYTES');}
  return total;
 }
 function splitGroups(E,text,count){
  if(typeof text!=='string'||text.length>1000||!text.trim())fail('RANGE');
  const parts=text.split(';');if(parts.length>C.split.outputs)fail('SPLIT');
  const indices=parts.map(part=>E.range(part,count)),flat=indices.flat();
  if(!flat.length||new Set(flat).size!==flat.length)fail('PARTITION');
  return indices.map(part=>part.map(index=>({source:0,index,rotation:0})));
 }
 function selection(E,request,counts){
  const tool=request.tool,limit=C.input(tool);
  let all=counts.flatMap((count,source)=>Array.from({length:count},(_,index)=>({source,index,rotation:0})));
  if(tool==='split'){
   const mode=request.splitMode||'ranges';let groups;
   if(mode==='pages'||mode==='all'||mode==='extract-all')groups=all.map(page=>[page]);
   else if(mode==='ranges'||mode==='custom')groups=splitGroups(E,request.ranges,counts[0]);
   else if(mode==='fixed'){
    const size=request.pagesPerFile;if(!Number.isInteger(size)||size<1||size>counts[0])fail('RANGE');
    groups=[];for(let i=0;i<counts[0];i+=size)groups.push(all.slice(i,Math.min(i+size,counts[0])));
   }else if(mode==='select'){
    const chosen=request.selectedPages;
    if(!Array.isArray(chosen)||!chosen.length||chosen.length>C.split.pages||chosen.some(i=>!Number.isInteger(i)||i<0||i>=counts[0])||new Set(chosen).size!==chosen.length)fail('RANGE');
    const selected=chosen.map(i=>all[i]);groups=request.combineSelected?[selected]:selected.map(page=>[page]);
   }else fail('SPLIT');
   if(request.combineRanges&&['ranges','custom'].includes(mode))groups=[groups.flat()];
   if(!groups.length||groups.length>C.split.outputs)fail('SPLIT');
   return groups;
  }
  if(tool==='organize'){
   all=request.pages;if(!Array.isArray(all)||!all.length||all.length>limit.pages)fail('EMPTY');
   for(const p of all)if(!p||!Number.isInteger(p.source)||!Number.isInteger(p.index)||!counts[p.source]||p.index<0||p.index>=counts[p.source]||![0,90,180,270].includes(p.rotation))fail('RANGE');
  }else if(['extract','remove','rotate'].includes(tool)){
   const chosen=E.range(request.ranges,counts[0]);
   if(tool==='extract')all=chosen.map(i=>all[i]);
   if(tool==='remove')all=all.filter(p=>!chosen.includes(p.index));
   if(tool==='rotate'){if(![90,180,270].includes(request.angle))fail('RANGE');all=all.map(p=>({...p,rotation:chosen.includes(p.index)?request.angle:0}));}
   if(!all.length)fail('EMPTY');
  }
  return [all];
 }
 const geometry=doc=>doc.getPages().map(p=>({width:p.getCropBox().width,height:p.getCropBox().height,rotation:p.getRotation().angle}));
 const notices=(m,source=false)=>[...(m.tags?.partial?['PARTIAL_TAGS']:[]),...(m.tags?.orphans.length?['ORPHAN_TAG']:[]),...(m.annotations.relativeLinks?['RELATIVE_LINKS']:[]),...(source&&m.annotations.sourceNavigation?['SOURCE_NAVIGATION']:[])];
 async function run(P,E,request,io){
  if(!C.tools.includes(request.tool)||request.tool==='merge'||!['inspect','run'].includes(request.action))fail('TOOL');
  admission(request.files,request.tool);const limit=C.input(request.tool),models=[];let pages=0,objects=0;
  if(request.tool==='images-pdf'){
   const image=await I.run(P,E,request,io);
   if(request.action==='inspect')return image;
   return writeOutputs(P,E,request,io,[{selection:Array.from({length:image.count}),build:async()=>image.document}],new Set());
  }
  io.progress?.({stage:'inspect',done:0,total:request.files.length});
  for(const file of request.files){
   const model=await F.load(P,E,file,io.read);pages+=model.count;objects+=model.objects;
   if(pages>limit.pages||objects>F.LIMITS.inputObjects)fail('LIMIT');
   models.push(model);io.progress?.({stage:'inspect',done:models.length,total:request.files.length});
  }
  if(request.action==='inspect')return {counts:models.map(m=>m.count),sources:models.map(m=>({tagged:!!m.tags,duplicable:!m.tags&&!m.annotations.records.size,geometry:geometry(m.doc),warnings:notices(m,true)}))};
  const docs=models.map(m=>m.doc),tags=models.map(m=>m.tags),annotations=models.map(m=>m.annotations);
  const parts=selection(E,request,models.map(m=>m.count)).map(items=>({selection:items,build:async()=>{
   const tagged=items.some(p=>tags[p.source]),annotated=items.some(p=>annotations[p.source].records.size);
   let copied=0;io.progress?.({stage:'copy',done:0,total:items.length});
   const out=tagged?await T.copy(P,docs,tags,items):annotated?await A.copy(P,docs,annotations,items):await P.PDFDocument.create();
   if(!tagged&&!annotated)for(const p of items){const [page]=await out.copyPages(docs[p.source],[p.index]);page.setRotation(P.degrees((page.getRotation().angle+p.rotation)%360));out.addPage(page);io.progress?.({stage:'copy',done:++copied,total:items.length});}
   io.progress?.({stage:'copy',done:items.length,total:items.length});
   E.copyEmptyForms(P,docs,items,out);out.setProducer('PERTAMINA OTO Web · local PDF');
   if(['numbers','watermark','crop'].includes(request.tool))await E.decorate(P,out,request);
   return out;
  }}));
  return writeOutputs(P,E,request,io,parts,new Set(models.some(m=>m.annotations.sourceNavigation)?['SOURCE_NAVIGATION']:[]));
 }
 async function writeOutputs(P,E,request,io,parts,warnings){
  const limit=C.input(request.tool),results=[];let total=0;
  for(let index=0;index<parts.length;index++){
   io.progress?.({stage:'copy',done:index,total:parts.length});
   const part=await (async()=>{
    const out=await parts[index].build(),sink=await io.output(index);
    const bounded={prepare:async size=>{if(!Number.isSafeInteger(size)||size<1||total+size>limit.outputBytes)fail('OUTPUT_LIMIT');await sink.prepare?.(size);},write:async bytes=>{if(total+bytes.length>limit.outputBytes)fail('OUTPUT_LIMIT');await sink.write(bytes);total+=bytes.length;}};
    const size=await F.serialize(P,out,bounded,io.progress),file=await sink.file();
    io.progress?.({stage:'verify',done:index,total:parts.length});
    const check=await F.load(P,E,file,io.read,F.LIMITS.outputStructureVisits);
    if(check.count!==parts[index].selection.length||file.size!==size)fail('VERIFY');
    notices(check).forEach(w=>warnings.add(w));
    const base=E.cleanName(request.name).replace(/\.pdf$/i,'');
    const suffix=parts.length>1?'_'+String(index+1).padStart(Math.max(2,String(parts.length).length),'0'):'';
    return {name:E.cleanName(base.slice(0,100-suffix.length)+suffix),count:check.count,size,geometry:geometry(check.doc)};
   })();
   results.push(part);io.progress?.({stage:'verify',done:index+1,total:parts.length});
  }
  return {results,warnings:[...warnings]};
 }
 const api={admission,splitGroups,run};if(local)module.exports=api;else root.OtoFileEngine=api;
})(typeof self!=='undefined'?self:globalThis);
