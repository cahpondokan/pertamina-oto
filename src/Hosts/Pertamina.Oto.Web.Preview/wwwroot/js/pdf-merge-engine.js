/* Shared bounded random-access adapter for the admitted pdf-lib build.
 * No vendor changes. Source stream payloads remain File slices, never aggregate
 * ArrayBuffers. Stream payloads use 1 MiB writes; metadata is bounded separately.
 * Per-file object/decompression guards remain unchanged. Combined input and
 * output-verification work have separate, finite budgets. */
(function(root){
 'use strict';
 const Capacity=typeof module!=='undefined'?require('./pdf-capacity.js'):root.OtoPdfCapacity;
 const LIMITS=Capacity.merge;
 const fail=code=>{throw Error(code);};
 function admission(files,minimum=1){
  if(!Array.isArray(files)||files.length<minimum)fail('MERGE');
  if(files.length>LIMITS.files)fail('FILE_COUNT');
  let total=0;
  for(const f of files){if(!f||!Number.isSafeInteger(f.size)||f.size<8)fail('INVALID');total+=f.size;}
  if(total>LIMITS.totalBytes)fail('TOTAL_BYTES');return total;
 }
 function range(text,count){
  if(!text?.trim())return Array.from({length:count},(_,i)=>i);
  if(typeof text!=='string'||text.length>1000)fail('RANGE');
  const result=[],seen=new Set();
  for(const t of text.split(',')){
   const m=/^\s*(\d{1,7})(?:\s*-\s*(\d{1,7}))?\s*$/.exec(t);if(!m)fail('RANGE');
   const a=Number(m[1]),b=Number(m[2]||m[1]);if(a<1||b<a||b>count)fail('RANGE');
   for(let i=a-1;i<b;i++){if(seen.has(i))fail('RANGE');seen.add(i);result.push(i);}
  }return result;
 }
 class Slice {
  constructor(file,start,end,read){this.file=file;this.start=start;this.end=end;this.length=end-start;this.read=read;}
  slice(){return this;}
  bytes(){if(this.length>LIMITS.metadataBytes)fail('LIMIT');return this.read(this.file,this.start,this.end);}
 }
 class FileBytes {
  constructor(file,read){this.file=file;this.read=read;this.length=file.size;this.idx=0;this.line=0;this.column=0;this.start=-1;this.cache=new Uint8Array();this.scanned=0;}
  moveTo(offset){if(!Number.isSafeInteger(offset)||offset<0||offset>this.length)fail('INVALID');this.idx=offset;}
  peekAt(offset){if(offset<0||offset>=this.length)return undefined;if(offset<this.start||offset>=this.start+this.cache.length){this.start=Math.floor(offset/65536)*65536;this.cache=this.read(this.file,this.start,Math.min(this.length,this.start+65536));}return this.cache[offset-this.start];}
  peek(){return this.peekAt(this.idx);} peekAhead(n){return this.peekAt(this.idx+n);}
  next(){if(++this.scanned>LIMITS.metadataBytes)fail('LIMIT');const b=this.peekAt(this.idx++);if(b===10){this.line++;this.column=0;}else this.column++;return b;}
  assertNext(expected){if(this.peek()!==expected)fail('INVALID');return this.next();}
  done(){return this.idx>=this.length;} offset(){return this.idx;}
  slice(start,end){if(!Number.isSafeInteger(start)||!Number.isSafeInteger(end)||start<0||end<start||end>this.length)fail('INVALID');return new Slice(this.file,start,end,this.read);}
  position(){return {line:this.line,column:this.column,offset:this.idx};}
 }
 function adapter(P){
  class LazyStream extends P.PDFRawStream {
   constructor(dict,slice){super(dict,new Uint8Array());this.sourceSlice=slice;Object.defineProperty(this,'contents',{get:()=>this.sourceSlice.bytes()});}
   clone(context){return new LazyStream(this.dict.clone(context),this.sourceSlice);}
   getContentsSize(){return this.sourceSlice.length;}
   getContents(){return this.sourceSlice.bytes();}
   asUint8Array(){return this.getContents();}
   getContentsString(){fail('UNSUPPORTED');}
  }
  return LazyStream;
 }
 async function load(P,E,file,read,visitLimit=80000){
  const bytes=new FileBytes(file,read);
  if(String.fromCharCode(...read(file,0,5))!=='%PDF-')fail('INVALID');
  const parser=P.PDFParser.forBytesWithOptions(new Uint8Array(),100,true,false);
  parser.bytes=bytes;
  const original=parser.parseDictOrStream,LazyStream=adapter(P);
  parser.parseDictOrStream=function(){const v=original.call(this);return v instanceof P.PDFRawStream&&v.contents instanceof Slice?new LazyStream(v.dict,v.contents):v;};
  let doc;
  try {const context=await parser.parseDocument();doc=new P.PDFDocument(context,false,false);}
  catch(e){if(e instanceof P.EncryptedPDFError||e?.message?.startsWith('Input document to `PDFDocument.load` is encrypted.'))fail('ENCRYPTED');if(['LIMIT','OTO_LIMIT'].includes(e.message))fail('LIMIT');fail('INVALID');}
  if(doc.isEncrypted)fail('ENCRYPTED');let count;try{count=doc.getPageCount();}catch{fail('INVALID');}if(!count)fail('EMPTY');
  const annotations=E.inspectStructures(P,doc,visitLimit),tags=(typeof module!=='undefined'?require('./pdf-tags.js'):root.OtoPdfTags).read(P,doc);
  // Parser caches are no longer retained once the model owns its lazy slices.
  return {doc,count,annotations,tags,objects:doc.context.enumerateIndirectObjects().length};
 }
 function warnings(model,source=false){return [...(model.tags?.partial?['PARTIAL_TAGS']:[]),...(model.tags?.orphans.length?['ORPHAN_TAG']:[]),...(model.annotations.relativeLinks?['RELATIVE_LINKS']:[]),...(source&&model.annotations.sourceNavigation?['SOURCE_NAVIGATION']:[])];}
 async function serialize(P,doc,sink,progress=()=>{}){
  await doc.flush();
  const plan=await P.PDFWriter.forContext(doc.context,100).computeBufferSize();
  await sink.prepare?.(plan.size);
  let written=0;
  const write=async value=>{const b=typeof value==='string'?new TextEncoder().encode(value):value;await sink.write(b);written+=b.length;};
  const object=async value=>{const size=value.sizeInBytes();if(size>LIMITS.metadataBytes)fail('LIMIT');const b=new Uint8Array(size);if(value.copyBytesInto(b,0)!==size)fail('VERIFY');await write(b);};
  await object(plan.header);await write('\n\n');
  for(let i=0;i<plan.indirectObjects.length;i++){
   const [ref,value]=plan.indirectObjects[i];await write(`${ref.objectNumber} ${ref.generationNumber} obj\n`);
   if(value.sourceSlice){
    value.updateDict();await object(value.dict);await write('\nstream\n');
    const s=value.sourceSlice;
    for(let at=s.start;at<s.end;at+=LIMITS.chunk){await write(s.read(s.file,at,Math.min(s.end,at+LIMITS.chunk)));progress({stage:'write',done:written,total:plan.size});}
    await write('\nendstream');
   }else if(value instanceof P.PDFRawStream){
    value.updateDict();await object(value.dict);await write('\nstream\n');
    const bytes=value.getContents();for(let at=0;at<bytes.length;at+=LIMITS.chunk)await write(bytes.subarray(at,at+LIMITS.chunk));
    await write('\nendstream');
   }else await object(value);
   await write('\nendobj\n\n');
   if(i%100===0)progress({stage:'write',done:written,total:plan.size});
  }
  await object(plan.xref);await write('\n');await object(plan.trailerDict);await write('\n\n');await object(plan.trailer);
  if(written!==plan.size)fail('VERIFY');return written;
 }
 async function run(P,E,request,io){
  const files=request.files;admission(files,request.action==='inspect'?1:Capacity.input('merge').minimum);
  const models=[],inspected=[];let objectCount=0;
  for(let i=0;i<files.length;i++){
   io.progress?.({stage:'inspect',done:i,total:files.length});
   const model=await load(P,E,files[i],io.read);objectCount+=model.objects;if(objectCount>LIMITS.inputObjects)fail('LIMIT');
   if(request.action==='inspect')inspected.push({count:model.count,tagged:!!model.tags,warnings:warnings(model,true)});
   else models.push(model);
  }
  if(request.action==='inspect')return {sources:inspected};
  const options=request.options||{},blank=options.blank||'none';
  if(!['none','always','odd'].includes(blank)||typeof options.bookmarks!=='boolean')fail('OPTIONS');
  const items=request.items;
  if(!Array.isArray(items)||items.length!==files.length)fail('OPTIONS');
  const groups=[],bookmarks=[];let totalPages=0;
  for(let source=0;source<models.length;source++){
   const item=items[source];if(![0,90,180,270].includes(item.rotation))fail('OPTIONS');
   const chosen=range(item.range,models[source].count);
   bookmarks.push({title:files[source].name||`Dokumen ${source+1}`,index:totalPages});
   groups.push(...chosen.map(index=>({source,index,rotation:item.rotation})));totalPages+=chosen.length;
  }
  const docs=models.map(m=>m.doc),tagged=models.map(m=>m.tags),annotations=models.map(m=>m.annotations);
  const T=typeof module!=='undefined'?require('./pdf-tags.js'):root.OtoPdfTags,A=typeof module!=='undefined'?require('./pdf-annotations.js'):root.OtoPdfAnnotations;
  io.progress?.({stage:'copy',done:0,total:totalPages});
  const output=tagged.some(Boolean)?await T.copy(P,docs,tagged,groups):annotations.some(a=>a.records.size)?await A.copy(P,docs,annotations,groups):await P.PDFDocument.create();
  if(!tagged.some(Boolean)&&!annotations.some(a=>a.records.size))for(let source=0;source<docs.length;source++){
   const selected=groups.filter(p=>p.source===source),copied=await output.copyPages(docs[source],selected.map(p=>p.index));
   copied.forEach((page,i)=>{page.setRotation(P.degrees((page.getRotation().angle+selected[i].rotation)%360));output.addPage(page);});
   io.progress?.({stage:'copy',done:output.getPageCount(),total:totalPages});
  }
  E.copyEmptyForms(P,docs,groups,output);output.setProducer('PERTAMINA OTO Web · local merge');
  // Blank pages follow each selected document (including the final one).
  let offset=0;
  for(let source=0;source<models.length;source++){
   bookmarks[source].index+=offset;
   const count=groups.filter(p=>p.source===source).length;
   if(blank==='always'||blank==='odd'&&count%2){const end=bookmarks[source].index+count;const page=output.getPage(end-1);output.insertPage(end,[page.getWidth(),page.getHeight()]);offset++;}
  }
  if(options.bookmarks){
   const name=s=>P.PDFName.of(s),context=output.context,outline=context.obj({Type:'Outlines'}),outlineRef=context.register(outline);
   const entries=bookmarks.map(b=>context.obj({Title:P.PDFHexString.fromText(b.title.replace(/\.pdf$/i,'').slice(0,200)),Parent:outlineRef,Dest:[output.getPage(b.index).ref,'Fit']}));
   const refs=entries.map(e=>context.register(e));entries.forEach((e,i)=>{if(i)e.set(name('Prev'),refs[i-1]);if(i+1<refs.length)e.set(name('Next'),refs[i+1]);});
   outline.set(name('First'),refs[0]);outline.set(name('Last'),refs.at(-1));outline.set(name('Count'),P.PDFNumber.of(refs.length));output.catalog.set(name('Outlines'),outlineRef);
  }
  const size=await serialize(P,output,io.sink,io.progress);
  const file=await io.sink.file();io.progress?.({stage:'verify',done:0,total:1});
  const check=await load(P,E,file,io.read,LIMITS.outputStructureVisits);
  if(check.count!==totalPages+offset)fail('VERIFY');
  return {name:E.cleanName(request.name),count:check.count,size,warnings:[...new Set([...warnings(check),...models.flatMap(m=>m.annotations.sourceNavigation?['SOURCE_NAVIGATION']:[])])]};
 }
 const api={LIMITS,admission,range,FileBytes,load,serialize,run};
 if(typeof module!=='undefined')module.exports=api;else root.OtoMergeEngine=api;
})(typeof self!=='undefined'?self:globalThis);
