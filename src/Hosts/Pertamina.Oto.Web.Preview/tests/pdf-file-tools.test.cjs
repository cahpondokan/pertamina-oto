'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js'),E=require('../wwwroot/js/pdf-engine.js'),F=require('../wwwroot/js/pdf-file-engine.js'),C=require('../wwwroot/js/pdf-capacity.js');
const T=require('../wwwroot/js/pdf-tags.js'),A=require('../wwwroot/js/pdf-annotations.js'),{annotated}=require('./annotation-fixtures.cjs');
const {virtualPdf,virtualRead}=require('./helpers/virtual-large-pdf.cjs'),{mount}=require('./helpers/paste-controller.cjs');
const tools=['organize','extract','remove','rotate','numbers','watermark','crop'];
const options={numbers:{position:'bottom-center',start:1,size:12,format:'number',margin:12},watermark:{text:'DRAF',size:48,opacity:18,angle:45,color:'blue'},crop:{top:5,right:5,bottom:5,left:5}};
const wrap=bytes=>({bytes,size:bytes.length}),hash=b=>crypto.createHash('sha256').update(b).digest('hex');
const request=tool=>({tool,action:'run',ranges:tool==='remove'?'2':'1',angle:90,name:'hasil.pdf',options:options[tool],pages:[{source:0,index:1,rotation:90},{source:0,index:0,rotation:0}]});
const read=(file,a,b)=>file.bytes?file.bytes.slice(a,b):virtualRead(file,a,b);
async function run(tool,files,override={}){const outputs=[];const result=await F.run(P,E,{...request(tool),files,...override},{read,output:async index=>{const chunks=[];return {write:async b=>chunks.push(Buffer.from(b)),file:async()=>outputs[index]=wrap(Buffer.concat(chunks))};}});return {result,outputs};}
function fingerprint(doc){return doc.getPages().map(p=>({box:p.getCropBox(),rotation:p.getRotation().angle,content:p.node.Contents()?.asArray().map(r=>hash(doc.context.lookup(r).getContents()))||[]}));}
for(const tool of tools)test(tool+' preserves supported tags/links and agrees with the established operation on small files',async()=>{
 const bytes=await annotated({tags:true}),before=hash(bytes),req=request(tool);
 // Tagged source order may only be moved as a complete document; rotation remains allowed.
 if(tool==='organize')req.pages=[{source:0,index:0,rotation:90},{source:0,index:1,rotation:0}];
 const old=await E.run(P,{...req,buffers:[bytes]}),{outputs,result}=await run(tool,[wrap(bytes)],req);
 assert.equal(result.results.length,1);const expected=await P.PDFDocument.load(old.results[0].bytes),actual=await P.PDFDocument.load(outputs[0].bytes);
 assert.deepEqual(fingerprint(actual),fingerprint(expected));assert.equal(A.read(P,actual).records.size,A.read(P,expected).records.size);assert.ok(T.read(P,actual));assert.equal(hash(bytes),before);
 assert.deepEqual((await F.run(P,E,{tool,action:'inspect',files:outputs},{read})).counts,[actual.getPageCount()]);
});
for(const tool of tools)test(tool+' processes a 32 MiB input beyond the old ceiling using bounded slices',async()=>{
 const f=virtualPdf(32*1048576),outputs=[];let maxRead=0,maxWrite=0;
 const r=await F.run(P,E,{...request(tool),files:[f]},{read:(f,a,b)=>{maxRead=Math.max(maxRead,b-a);return read(f,a,b);},output:async i=>{
  const parts=[];let size=0;const spaces=Buffer.alloc(1048576,32);
  return {write:async b=>{maxWrite=Math.max(maxWrite,b.length);const bytes=Buffer.from(b.buffer,b.byteOffset,b.byteLength),fill=bytes.equals(spaces.subarray(0,b.length));parts.push({offset:size,length:b.length,...(fill?{fill:32}:{bytes:Buffer.from(b)})});size+=b.length;},file:async()=>outputs[i]={size,parts}};
 }});
 assert.equal(r.results.length,1);assert.ok(outputs[0].size>10*1048576);assert.ok(maxRead<=1048576&&maxWrite<=1048576);
});
test('all active tools share 4 GB input while file counts remain tool-specific',()=>{
 for(const tool of C.tools){assert.equal(C.input(tool).totalBytes,4e9);if(tool==='merge')continue;assert.equal(F.admission([{size:4e9}],tool),4e9);assert.throws(()=>F.admission([{size:4e9+1}],tool),/^Error: TOTAL_BYTES$/);}
 for(const tool of ['organize','images-pdf']){assert.equal(F.admission(Array.from({length:10},()=>({size:8})),tool),80);assert.throws(()=>F.admission(Array.from({length:11},()=>({size:8})),tool),/^Error: FILE_COUNT$/);assert.throws(()=>F.admission([{size:4e9},{size:8}],tool),/^Error: TOTAL_BYTES$/);}
 for(const tool of tools.filter(t=>t!=='organize'))assert.throws(()=>F.admission([{size:8},{size:8}],tool),/^Error: SINGLE$/);
});
test('organizer combines tagged and untagged sources and keeps source/page identity',async()=>{
 const tagged=await annotated({tags:true}),plain=await P.PDFDocument.create();plain.addPage([222,444]);const b=await plain.save();
 const {outputs,result}=await run('organize',[wrap(tagged),wrap(b)],{pages:[{source:1,index:0,rotation:0},{source:0,index:0,rotation:90},{source:0,index:1,rotation:0}]});
 const d=await P.PDFDocument.load(outputs[0].bytes);assert.equal(d.getPageCount(),3);assert.equal(d.getPage(0).getWidth(),222);assert.equal(d.getPage(1).getRotation().angle,90);assert.equal(A.read(P,d).records.size,16);assert.ok(T.read(P,d));assert.ok(result.warnings.includes('PARTIAL_TAGS'));
});
test('all PDF tools independently reject signature and script before writing',async()=>{
 for(const [key,value,code]of [['Perms',{},'UNSUPPORTED_SIGNATURE'],['OpenAction',{S:'JavaScript',JS:P.PDFString.of('private sentinel')},'UNSUPPORTED_ACTION']]){
  const d=await P.PDFDocument.create();d.addPage();d.addPage();d.catalog.set(P.PDFName.of(key),d.context.obj(value));const f=wrap(await d.save());
  for(const tool of tools){let opened=0;await assert.rejects(F.run(P,E,{...request(tool),files:[f]},{read,output:async()=>{opened++;}}),new RegExp('^Error: '+code+'$'));assert.equal(opened,0);}
 }
});
test('extract/remove refuse to lose a retained link target',async()=>{const f=wrap(await annotated({cross:true}));for(const tool of ['extract','remove'])await assert.rejects(run(tool,[f]),/^Error: LINK_TARGET$/);});
test('organizer validates source index, page index, rotation, duplication and page ceiling',async()=>{
 const f=wrap(await annotated({tags:true}));for(const pages of [[],[{source:1,index:0,rotation:0}],[{source:0,index:9,rotation:0}],[{source:0,index:0,rotation:1}],Array.from({length:201},()=>({source:0,index:0,rotation:0}))])await assert.rejects(run('organize',[f],{pages}),/^Error: (EMPTY|RANGE)$/);
 await assert.rejects(run('organize',[f],{pages:[{source:0,index:0,rotation:0},{source:0,index:0,rotation:0}]}),/^Error: (TAGGED_OPERATION|ANNOTATION_OPERATION)$/);
});
for(const tool of tools)test(tool+' UI keeps File references, revalidates and exports through local file worker',async()=>{
 const h=await mount(tool,{session:{canAdd:()=>{throw Error('OLD_MEMORY_ADMISSION');},canResult:()=>{throw Error('OLD_MEMORY_RESULT');}}});h.paste();await h.idle();
 assert.equal(h.controller.resources().sources.length,1);assert.ok(h.controller.resources().sources[0].bytes instanceof Blob);await h.$('pdf-run').click();assert.equal(h.buffers().length,1,h.feedback());
 const d=await P.PDFDocument.load(await h.buffers()[0].arrayBuffer());assert.ok(d.getPageCount()>0);h.controller.dispose();
});
