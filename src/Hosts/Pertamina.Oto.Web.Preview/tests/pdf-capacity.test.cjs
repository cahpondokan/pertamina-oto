'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const C=require('../wwwroot/js/pdf-capacity.js'),E=require('../wwwroot/js/pdf-engine.js'),M=require('../wwwroot/js/pdf-merge-engine.js'),P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js');
const Messages=require('../wwwroot/js/pdf-document-messages.js'),web=path.resolve(__dirname,'..');
test('capacity refactor preserves every existing numeric engine budget',()=>{
 assert.deepEqual(E.LIMITS,{fileBytes:10485760,totalBytes:31457280,files:10,pages:200,outputs:20,outputBytes:41943040,timeout:30000});
 assert.deepEqual(M.LIMITS,{files:500,totalBytes:4000000000,chunk:1048576,metadataBytes:16777216,objects:20000,inputObjects:40000,outputStructureVisits:160000,timeout:1800000});
 assert.deepEqual(E.IMAGE_LIMITS,{pixels:8000000,totalPixels:16000000,dimension:6000});
 assert.equal(E.LIMITS,C.bounded);assert.equal(M.LIMITS,C.merge);
});
test('generated classic-worker envelope equals canonical JSON and is immutable after admission',()=>{
 const ctx=vm.createContext({});for(const file of ['pdf-capacity-data.js','pdf-capacity.js'])vm.runInContext(fs.readFileSync(path.join(web,'wwwroot/js',file),'utf8'),ctx);
 const manifest=JSON.parse(fs.readFileSync(path.join(web,'wwwroot/js/pdf-capacity.json'),'utf8'));
 assert.deepEqual(JSON.parse(JSON.stringify(ctx.OtoPdfCapacityData)),manifest);
 assert.equal(ctx.OtoPdfCapacity.input('split').files,1);
 assert.ok(vm.runInContext('Object.isFrozen(OtoPdfCapacityData.bounded)&&Object.isFrozen(OtoPdfCapacityData.profiles.single)',ctx));
 assert.throws(()=>vm.runInContext('"use strict";OtoPdfCapacityData.bounded.fileBytes=999999999',ctx),/read only/);
});
test('all and only admitted catalog tools have capacity profiles',()=>{
 const catalog=fs.readFileSync(path.join(web,'Models/ToolCatalog.cs'),'utf8');
 const ids=[...catalog.matchAll(/new\("([a-z-]+)"[^\n]*BrowserToolProfile\./g)].map(m=>m[1]);
 assert.deepEqual([...C.tools].sort(),ids.sort());
 for(const id of ['edit','ocr','compress','unknown','__proto__','constructor'])assert.throws(()=>C.input(id),/^Error: TOOL$/);
 for(const id of C.tools)assert.ok(Object.isFrozen(C.input(id)));
});
test('multi-file and single-file input counts are distinct from output count',()=>{
 for(const id of ['extract','remove','rotate','numbers','watermark','crop']){
  const v=C.input(id);assert.equal(v.files,1);assert.equal(v.fileBytes,4000000000);assert.equal(v.totalBytes,4000000000);assert.equal(v.pages,200);
 }
 assert.equal(C.input('split').fileBytes,4000000000);assert.equal(C.input('split').files,1);assert.equal(C.input('split').pages,200);
 assert.equal(C.input('organize').files,10);assert.equal(C.input('organize').totalBytes,4000000000);
 assert.equal(C.input('images-pdf').files,10);assert.equal(C.splitMinimumParts,2);assert.equal(C.bounded.outputs,20);
 assert.equal(C.input('merge').files,500);assert.equal(C.input('merge').minimum,2);assert.equal(C.input('merge').totalBytes,4000000000);
 assert.equal(C.bytes(10485760),'10 MiB');assert.equal(C.bytes(31457280),'30 MiB');assert.equal(C.bytes(4000000000),'4 GB');
});
test('single PDF worker inspection rejects multiple documents before parsing',async()=>{
 const d=await P.PDFDocument.create();d.addPage();const b=await d.save();
 for(const tool of ['split','extract','remove','rotate','numbers','watermark','crop']){
  await assert.rejects(E.run(P,{tool,action:'inspect',buffers:[b,b]}),/^Error: SINGLE$/);
  const r=await E.run(P,{tool,action:'inspect',buffers:[b]});assert.deepEqual(r.counts,[1]);
 }
 assert.deepEqual((await E.run(P,{tool:'organize',action:'inspect',buffers:[b,b]})).counts,[1,1]);
});
test('bounded file boundary accepts exact bytes and rejects one byte beyond',async()=>{
 const d=await P.PDFDocument.create();d.addPage();const b=await d.save({useObjectStreams:false});
 const padded=new Uint8Array(C.bounded.fileBytes);padded.fill(32);padded.set(b);
 assert.deepEqual((await E.run(P,{action:'inspect',tool:'split',buffers:[padded]})).counts,[1]);
 await assert.rejects(E.run(P,{action:'inspect',tool:'split',buffers:[new Uint8Array(C.bounded.fileBytes+1)]}),/^Error: LIMIT$/);
});
test('merge admission remains decimal 4 GB and 500 documents, not a successful 4 GB processing test',()=>{
 assert.equal(M.admission([{size:3999999992},{size:8}],2),4000000000);
 assert.throws(()=>M.admission([{size:3999999993},{size:8}],2),/^Error: TOTAL_BYTES$/);
 assert.equal(M.admission(Array.from({length:500},()=>({size:8}))),4000);
 assert.throws(()=>M.admission(Array.from({length:501},()=>({size:8}))),/^Error: FILE_COUNT$/);
});
test('messages use the same budgets, including session, image and output constraints',()=>{
 const single=Messages.browser(false,'extract'),organize=Messages.browser(false,'organize'),image=Messages.browser(true,'images-pdf');
 assert.match(single.errors.LIMIT,/1 PDF, 4 GB/);assert.match(organize.errors.LIMIT,/10 PDF, 4 GB per file, 4 GB total, 200 halaman/);
 assert.match(image.errors.LIMIT,/10 gambar/);assert.match(single.errors.SPLIT,/2–200 bagian/);
 assert.match(single.errors.SESSION_LIMIT,/30 MiB input dan 40 MiB hasil/);
 assert.match(Messages.merge().errors.TOTAL_BYTES,/4 GB \(4\.000\.000\.000 byte\)/);
});
test('both workers and both hosts include the capacity contract before engines',()=>{
 for(const name of ['pdf-worker.js','pdf-merge-worker.js']){
  const source=fs.readFileSync(path.join(web,'wwwroot/js',name),'utf8');
  assert.ok(source.indexOf('/js/pdf-capacity-data.js')<source.indexOf('/js/pdf-capacity.js'));
  assert.ok(source.indexOf('/js/pdf-capacity.js')<source.indexOf('/js/pdf-engine.js'));
 }
 for(const name of ['Pertamina.Oto.Web.Preview.csproj','../Pertamina.Oto.Downloads/Pertamina.Oto.Downloads.csproj'])assert.match(fs.readFileSync(path.join(web,name),'utf8'),/Import Project="[^"]*BuildPdfCapacity.targets"/);
 const view=fs.readFileSync(path.join(web,'Views/Shared/_PdfWorkspaceAssets.cshtml'),'utf8');
 assert.ok(view.indexOf('/js/pdf-capacity-data.js')<view.indexOf('/js/pdf-capacity.js'));
 assert.ok(view.indexOf('/js/pdf-capacity.js')<view.indexOf('/js/pdf-engine.js'));
});
