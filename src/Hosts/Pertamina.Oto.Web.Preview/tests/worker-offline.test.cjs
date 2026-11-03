'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),{Worker}=require('node:worker_threads'),fs=require('node:fs'),path=require('node:path');
const dir=process.env.OTO_PDF_TEST_DIR, web=path.resolve(__dirname,'../wwwroot');
const A=new Uint8Array(fs.readFileSync(path.join(dir,'A.pdf'))),B=new Uint8Array(fs.readFileSync(path.join(dir,'B.pdf')));
// Runs the actual browser worker entry, with all network transports made unavailable.
// This proves computation without network. It is NOT a service-worker/PWA/offline-page test.
const harness=`
 const {parentPort,workerData}=require('node:worker_threads');
 const deny=()=>{throw Error('NETWORK_FORBIDDEN');};
 global.fetch=deny;global.XMLHttpRequest=class{constructor(){deny();}};global.WebSocket=class{constructor(){deny();}};
 require('node:http').request=deny;require('node:http').get=deny;
 require('node:https').request=deny;require('node:https').get=deny;
 require('node:net').connect=deny;require('node:net').createConnection=deny;
 require('node:dns').lookup=deny;
 global.self=global;self.close=()=>parentPort.close();self.postMessage=(v,t)=>parentPort.postMessage(v,t);
 global.importScripts=(...urls)=>{for(const u of urls){
  if(u==='/js/pdf-capacity-data.js')require(workerData.web+u);
  else if(u==='/js/pdf-capacity.js')global.OtoPdfCapacity=require(workerData.web+u);
  else if(u==='/vendor/pdf-lib-1.17.1/pdf-lib.js')global.PDFLib=require(workerData.web+u);
  else if(u==='/js/pdf-annotations.js')global.OtoPdfAnnotations=require(workerData.web+u);
  else if(u==='/js/pdf-tags.js')global.OtoPdfTags=require(workerData.web+u);
  else if(u==='/js/pdf-engine.js')global.OtoPdf=require(workerData.web+u);
  else throw Error('UNEXPECTED_IMPORT');
 }};
 require(workerData.web+'/js/pdf-worker.js');
 parentPort.on('message',v=>self.onmessage({data:v}));
`;
async function run(request){
 const w=new Worker(harness,{eval:true,workerData:{web}});
 try{return await new Promise((resolve,reject)=>{const timer=setTimeout(()=>reject(Error('TIMEOUT')),30000);w.on('error',e=>{clearTimeout(timer);reject(e);});w.on('message',v=>{if(v.progress)return;clearTimeout(timer);resolve(v);});w.postMessage(request);});}
 finally{await w.terminate();}
}
for(const [tool,extra,counts] of [
 ['merge',{buffers:[A,B]},[6]],
 ['organize',{pages:[{source:0,index:3,rotation:90},{source:0,index:0,rotation:0}]},[2]],
 ['split',{ranges:'1-2;3-4'},[2,2]],
 ['remove',{ranges:'2-3'},[2]],
 ['extract',{ranges:'2-4'},[3]],
 ['rotate',{ranges:'1-4',angle:180},[4]],
 ['numbers',{options:{start:1,size:12,margin:12,position:'bottom-center',format:'total'}},[4]],
 ['watermark',{options:{text:'DRAF OTO',size:48,opacity:18,angle:45,color:'blue'}},[4]],
 ['crop',{options:{top:5,right:5,bottom:5,left:5}},[4]]
])test(tool+' actual worker succeeds and reimports its output with network transports denied',async()=>{
 const result=await run({action:'run',tool,buffers:[A],name:tool,...extra});
 assert.equal(result.ok,true);assert.deepEqual(result.results.map(r=>r.count),counts);assert.ok(result.results.every(r=>r.bytes.length>0));
 for(const output of result.results){const reopened=await run({action:'inspect',tool:'numbers',buffers:[output.bytes]});assert.equal(reopened.ok,true);assert.deepEqual(reopened.counts,[output.count]);}
});

const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js');
for(const [label,edit,code] of [
 ['annotation',(d,p)=>p.node.set(P.PDFName.of('Annots'),d.context.obj([{Type:'Annot',Subtype:'Text'}])),'UNSUPPORTED_ANNOTATIONS'],
 ['form',d=>d.catalog.set(P.PDFName.of('AcroForm'),d.context.obj({Fields:[{FT:'Tx'}]})),'UNSUPPORTED_FORM'],
 ['script',d=>d.catalog.set(P.PDFName.of('OpenAction'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('PRIVATE_SYNTHETIC_SENTINEL')})),'UNSUPPORTED_ACTION'],
 ['attachment',d=>d.catalog.set(P.PDFName.of('AF'),d.context.obj([])),'UNSUPPORTED_ATTACHMENT'],
 ['signature',d=>d.catalog.set(P.PDFName.of('Perms'),d.context.obj({})),'UNSUPPORTED_SIGNATURE'],
 ['tags',d=>d.catalog.set(P.PDFName.of('StructTreeRoot'),d.context.obj({})),'TAGGED_STRUCTURE']
])test('worker reports bounded '+label+' reason without document text',async()=>{
 const d=await P.PDFDocument.create(),p=d.addPage([300,400]);p.drawText('PRIVATE_SYNTHETIC_SENTINEL');edit(d,p);
 const result=await run({action:'inspect',tool:'numbers',buffers:[await d.save({useObjectStreams:false})]});
 assert.deepEqual(result,{ok:false,code});assert.equal(JSON.stringify(result).includes('PRIVATE_SYNTHETIC_SENTINEL'),false);
 const messages=require('../wwwroot/js/pdf-document-messages.js').browser();assert.ok(messages.errors[code]);assert.equal(messages.error(Error(code)),messages.errors[code]);
});

test('worker imports a plain text PDF with empty annotation array offline',async()=>{
 const d=await P.PDFDocument.create();d.addPage().drawText('Synthetic PDF');
 const result=await run({action:'inspect',tool:'numbers',buffers:[await d.save({useObjectStreams:false})]});
 assert.deepEqual(result,{ok:true,counts:[1]});
});

const {tagged}=require('./tagged-fixtures.cjs'),Tags=require('../wwwroot/js/pdf-tags.js');
for(const [tool,extra] of [
 ['merge',{two:true}],['organize',{pages:[1,0].map(index=>({source:0,index,rotation:90}))}],
 ['split',{ranges:'1;2'}],['extract',{ranges:'2'}],['remove',{ranges:'1'}],['rotate',{ranges:'1-2',angle:90}],
 ['numbers',{options:{start:1,size:12,margin:12,position:'bottom-center',format:'total'}}],
 ['watermark',{options:{text:'DRAF',size:30,opacity:18,angle:45,color:'blue'}}],['crop',{options:{top:5,right:5,bottom:5,left:5}}]
])test(tool+' tagged PDF in actual worker retains readable structure offline',async()=>{
 const a=await tagged({count:2}),result=await run({action:'run',tool,buffers:extra.two?[a,a]:[a],...extra});assert.equal(result.ok,true,JSON.stringify(result));
 for(const output of result.results){const d=await P.PDFDocument.load(output.bytes),m=Tags.read(P,d);assert.ok(m);assert.equal(m.pages.length,output.count);const reopened=await run({action:'inspect',tool:'organize',buffers:[output.bytes]});assert.equal(reopened.ok,true);}
});
for(const [name,request,code] of [
 ['tag duplication',async a=>({tool:'organize',buffers:[a],pages:[{source:0,index:0,rotation:0},{source:0,index:0,rotation:0}]}),'TAGGED_OPERATION'],
 ['role conflict',async a=>({tool:'merge',buffers:[a,await tagged({edit:(d,{root})=>root.lookup(P.PDFName.of('RoleMap'),P.PDFDict).set(P.PDFName.of('CustomP'),P.PDFName.of('H2'))})]}),'TAGGED_CONFLICT']
])test('worker safely reports '+name,async()=>{const result=await run(await request(await tagged()));assert.deepEqual(result,{ok:false,code});});
test('worker preserves mixed tag coverage and warns offline',async()=>{const result=await run({tool:'merge',buffers:[await tagged(),A]});assert.equal(result.ok,true);assert.ok(result.warnings.includes('PARTIAL_TAGS'));const reopened=await run({action:'inspect',buffers:[result.results[0].bytes]});assert.equal(reopened.ok,true);assert.ok(reopened.warnings.includes('PARTIAL_TAGS'));const d=await P.PDFDocument.load(result.results[0].bytes),m=Tags.read(P,d);assert.equal(m.get(m.mark,'Marked'),P.PDFBool.False);assert.equal(m.owners.reduce((n,x)=>n+x.size,0),12);});

test('browser-global importScripts path works without CommonJS or network',async()=>{
 const vm=require('node:vm'),sandbox={setTimeout,clearTimeout,TextEncoder,TextDecoder,URL,console:{log(){},warn(){},error(){}}};
 const context=vm.createContext(sandbox);vm.runInContext('self=globalThis; self.close=()=>{}; fetch=()=>{throw Error("NETWORK_FORBIDDEN")};',context);
 sandbox.importScripts=(...urls)=>{for(const url of urls){assert.ok(['/js/pdf-capacity-data.js','/js/pdf-capacity.js','/vendor/pdf-lib-1.17.1/pdf-lib.js','/js/pdf-annotations.js','/js/pdf-tags.js','/js/pdf-engine.js'].includes(url));vm.runInContext(fs.readFileSync(path.join(web,url),'utf8'),context,{timeout:10000});}};
 vm.runInContext(fs.readFileSync(path.join(web,'js/pdf-worker.js'),'utf8'),context,{timeout:10000});
 sandbox.input=Array.from(await require('./annotation-fixtures.cjs').annotated({tags:true,count:2}));let result;sandbox.postMessage=v=>{result=v;};
 await vm.runInContext('self.onmessage({data:{action:"run",tool:"merge",buffers:[new Uint8Array(input),new Uint8Array(input)]}})',context,{timeout:10000});
 assert.equal(result.ok,true,JSON.stringify(result));assert.equal(result.results[0].count,4);
 const d=await P.PDFDocument.load(new Uint8Array(result.results[0].bytes));assert.ok(Tags.read(P,d));
 const layout=fs.readFileSync(path.resolve(web,'../Views/Shared/_Layout.cshtml'),'utf8');assert.ok(layout.includes('<partial name="_PdfWorkspaceAssets"/>'));
 const view=fs.readFileSync(path.resolve(web,'../Views/Shared/_PdfWorkspaceAssets.cshtml'),'utf8');assert.ok(view.indexOf('/js/pdf-tags.js')>=0&&view.indexOf('/js/pdf-tags.js')<view.indexOf('/js/pdf-engine.js'));
});

const {annotated}=require('./annotation-fixtures.cjs'),Annotations=require('../wwwroot/js/pdf-annotations.js');
for(const tags of [false,true])for(const [tool,extra] of [
 ['merge',{two:true}],['organize',{pages:[1,0].map(index=>({source:0,index,rotation:90}))}],
 ['split',{ranges:'1;2'}],['extract',{ranges:'2'}],['remove',{ranges:'1'}],['rotate',{ranges:'1-2',angle:90}],
 ['numbers',{options:{start:1,size:12,margin:12,position:'bottom-center',format:'total'}}],
 ['watermark',{options:{text:'DRAF',size:30,opacity:18,angle:45,color:'blue'}}],['crop',{options:{top:5,right:5,bottom:5,left:5}}]
])test(tool+' annotations in actual worker, tags='+tags+', no network',async()=>{
 const a=await annotated({tags}),r=await run({action:'run',tool,buffers:extra.two?[a,a]:[a],...extra});assert.equal(r.ok,true,JSON.stringify(r));
 for(const o of r.results){const d=await P.PDFDocument.load(o.bytes);assert.equal(Annotations.read(P,d).records.size,o.count*8);if(tags)assert.equal(Tags.read(P,d).objectOwners.size,o.count*7);assert.equal((await run({action:'inspect',buffers:[o.bytes]})).ok,true);}
});
for(const [code,make] of [
 ['UNSAFE_LINK',async()=>({action:'inspect',buffers:[await annotated({edit:(d,s)=>s.records[5].dict.lookup(P.PDFName.of('A'),P.PDFDict).set(P.PDFName.of('URI'),P.PDFString.of('file:///private-sentinel'))})]})],
 ['LINK_TARGET',async()=>({tool:'extract',ranges:'1',buffers:[await annotated({cross:true})]})],
 ['ANNOTATION_OPERATION',async()=>({tool:'organize',pages:[0,0].map(index=>({source:0,index,rotation:0})),buffers:[await annotated()]})]
])test('worker gives bounded '+code+' without leaking URL or contents',async()=>{assert.deepEqual(await run(await make()),{ok:false,code});});
