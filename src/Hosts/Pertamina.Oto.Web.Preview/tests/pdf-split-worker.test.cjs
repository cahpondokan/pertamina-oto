'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js'),T=require('../wwwroot/js/pdf-tags.js'),A=require('../wwwroot/js/pdf-annotations.js');
const {annotated}=require('./annotation-fixtures.cjs'),base=path.resolve(__dirname,'../wwwroot');
// Actual browser worker entry, with explicit local IO ports and no network transports.
async function worker(bytes,options={}){
 const files=new Map(),progress=[],reads=[];let result,closed=0,active=0;
 const file=bytes=>({size:bytes.length,slice:(a,b)=>{reads.push(b-a);return bytes.slice(a,b);}});
 const native={getFileHandle:async name=>({createSyncAccessHandle:async()=>{
  let chunks=[],ended=false;if(active)throw Error('TWO_OPEN_OUTPUTS');active++;
  return {truncate:()=>{chunks=[];},write:(b)=>{if(options.failWrite&&name==='part-1.pdf')throw new DOMException('quota','QuotaExceededError');const n=options.shortWrites?Math.min(7,b.length):b.length;chunks.push(Buffer.from(b.subarray(0,n)));return n;},flush:()=>files.set(name,Buffer.concat(chunks)),close:()=>{if(!ended){ended=true;active--;closed++;}}};
 },getFile:async()=>file(files.get(name))})};
 const deny=()=>{throw Error('NETWORK_FORBIDDEN');};
 const ctx=vm.createContext({Uint8Array,console:{log(){},warn(){},error(){}},FileReaderSync:class{readAsArrayBuffer(b){return b.buffer.slice(b.byteOffset,b.byteOffset+b.byteLength);}},navigator:options.unsupported?{}:{locks:{request:async(_name,_opt,action)=>action(options.busy?null:{})},storage:{getDirectory:async()=>({getDirectoryHandle:async()=>native}),estimate:async()=>({quota:options.full?1:1e9,usage:0})}},fetch:deny,XMLHttpRequest:class{constructor(){deny();}},WebSocket:class{constructor(){deny();}}});
 ctx.self=ctx;ctx.close=()=>{};ctx.postMessage=v=>{if(v.progress)progress.push(v.progress);else result=v;};
 ctx.importScripts=(...urls)=>{for(const url of urls){if(url.endsWith('pdf-capacity-data.js'))continue;const names={'/js/pdf-capacity.js':'OtoPdfCapacity','/vendor/pdf-lib-1.17.1/pdf-lib.js':'PDFLib','/js/pdf-annotations.js':'OtoPdfAnnotations','/js/pdf-tags.js':'OtoPdfTags','/js/pdf-engine.js':'OtoPdf','/js/pdf-merge-engine.js':'OtoMergeEngine','/js/pdf-split-engine.js':'OtoSplitEngine','/js/pdf-file-engine.js':'OtoFileEngine','/js/pdf-file-images.js':'OtoFileImages'};assert.ok(names[url]);ctx[names[url]]=require(path.join(base,url));}};
 vm.runInContext(fs.readFileSync(path.join(base,'js/pdf-split-worker.js'),'utf8'),ctx);
 await ctx.onmessage({data:{action:'run',tool:'split',files:[file(bytes)],ranges:'1;2',name:'parts',directory:'oto-merge-v1-00000000-0000-4000-a000-000000000000'}});
 return {result,files,progress,closed,active,reads};
}
test('actual split worker sequential OPFS writes reopen with tags and annotations preserved offline',async()=>{const r=await worker(await annotated({tags:true}),{shortWrites:true});assert.equal(r.result.ok,true);assert.equal(r.closed,2);assert.equal(r.active,0);assert.equal(r.files.size,2);for(const bytes of r.files.values()){const d=await P.PDFDocument.load(bytes);assert.equal(d.getPageCount(),1);assert.equal(A.read(P,d).records.size,8);assert.ok(T.read(P,d));}assert.deepEqual(r.progress.at(-1),{stage:'verify',done:2,total:2});assert.ok(r.reads.every(n=>n<=1048576));});
for(const [mode,code]of [['full','STORAGE'],['failWrite','STORAGE'],['busy','BUSY'],['unsupported','STORAGE_BROWSER']])test('actual split worker '+mode+' does not return partial output metadata',async()=>{const r=await worker(await annotated(),{[mode]:true});assert.deepEqual(JSON.parse(JSON.stringify(r.result)),{ok:false,code});assert.equal(r.active,0);assert.equal(r.result.results,undefined);});
test('worker preserves bounded error messages for active-content rejection',async()=>{const d=await P.PDFDocument.create();d.addPage();d.addPage();d.catalog.set(P.PDFName.of('OpenAction'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('private sentinel')}));const r=await worker(await d.save());assert.deepEqual(JSON.parse(JSON.stringify(r.result)),{ok:false,code:'UNSUPPORTED_ACTION'});assert.equal(r.files.size,0);assert.ok(!JSON.stringify(r.result).includes('sentinel'));});
