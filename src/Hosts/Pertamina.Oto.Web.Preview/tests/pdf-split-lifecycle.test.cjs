'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),{mount}=require('./helpers/paste-controller.cjs');
const J=require('../wwwroot/js/pdf-merge-job.js'),Session=require('../wwwroot/js/pdf-session.js');
const tick=()=>new Promise(resolve=>setImmediate(resolve));
test('file-backed input does not consume the bounded-memory session budget',()=>{const bytes=new File([new Uint8Array(8)],'x.pdf'),buffer=new Uint8Array(12);assert.deepEqual(Session.usage([()=>({sources:[{bytes}],outputBytes:4e9,fileBacked:true}),()=>({sources:[{bytes:buffer}],outputBytes:30})]),{files:1,inputBytes:12,outputBytes:30});});
test('split input UI keeps File references and bypasses only bounded-memory session checks',async()=>{
 let received;const h=await mount('split',{session:{canAdd:()=>{throw Error('WRONG_MEMORY_BUDGET');}},configure:ctx=>{ctx.OtoMergeJob.split=request=>{received=request;return {promise:Promise.resolve({counts:[2],sources:[{geometry:[],tagged:false,warnings:[]}]}),cancel(){}};};ctx.OtoPdfJob.start=()=>{throw Error('WRONG_WORKER');};}});
 const file={name:'large.pdf',size:11000000,arrayBuffer(){throw Error('WHOLE_FILE_READ');}};h.paste([file]);await h.idle();assert.equal(received.files[0],file);assert.equal(h.controller.resources().sources[0].bytes,file);assert.equal(h.controller.resources().fileBacked,true);h.controller.dispose();
});
test('file-backed transfer over the old 10 MiB ceiling is revalidated without trusting source metadata',async()=>{
 const initial=await mount('split'),small=new Uint8Array(await initial.file.arrayBuffer());
 const bytes=new Uint8Array(11*1048576);bytes.fill(32);bytes.set(small);const file=new File([bytes],'large.pdf');
 const h=await mount('extract',{seed:[{name:file.name,bytes:file,count:999}]});await h.idle();
 assert.equal(h.controller.resources().sources[0].count,4);assert.ok(h.controller.resources().sources[0].bytes instanceof Blob);assert.equal(h.jobs(),1);
 initial.controller.dispose();h.controller.dispose();
});
test('invalid transferred PDF is rejected by the receiving tool even with a claimed count',async()=>{
 const file=new File(['%PDF-invalid'],'invalid.pdf');const h=await mount('extract',{seed:[{name:file.name,bytes:file,count:2}]});await h.idle();
 assert.equal(h.controller.resources().sources.length,0);assert.equal(h.jobs(),1);assert.match(h.feedback(),/tidak valid/);h.controller.dispose();
});
test('cancel/dispose while storage creation is pending removes the newly created directory',async()=>{
 const h=await mount('split');h.paste();await h.idle();let finish,removed=[],started=0;
 h.context.OtoMergeJob.create=()=>new Promise(r=>finish=r);h.context.OtoMergeJob.remove=async name=>removed.push(name);h.context.OtoMergeJob.split=()=>{started++;throw Error('MUST_NOT_START');};
 h.$('pdf-split-tab-pages').click();const running=h.$('pdf-run').click();await tick();assert.equal(typeof finish,'function');h.controller.dispose();finish('owned-test-directory');await running;assert.equal(started,0);assert.deepEqual(removed,['owned-test-directory']);assert.equal(h.buffers().length,0);
});
test('worker failure removes partial OPFS output while retaining source and allowing retry',async()=>{
 const h=await mount('split');h.paste();await h.idle();h.$('pdf-split-tab-pages').click();const original=h.context.OtoMergeJob.split,remove=h.context.OtoMergeJob.remove;let removed=0;
 h.context.OtoMergeJob.remove=async name=>{removed++;await remove(name);};h.context.OtoMergeJob.split=()=>({promise:Promise.reject(Error('STORAGE')),cancel(){}});
 await h.$('pdf-run').click();assert.equal(h.buffers().length,0);assert.equal(removed,1);assert.equal(h.controller.resources().sources.length,1);assert.match(h.feedback(),/penyimpanan/);
 h.context.OtoMergeJob.split=original;await h.$('pdf-run').click();assert.equal(h.buffers().length,4);h.controller.dispose();await tick();assert.equal(removed,2);
});
for(const kind of ['success','failure','cancel','timeout','post-error','worker-error'])test('split job disposes actual worker port on '+kind,async()=>{
 let instance,terminated=0;
 class W{constructor(url){assert.equal(url,'/js/pdf-split-worker.js');instance=this;}postMessage(){if(kind==='post-error')throw Error();if(kind==='success')queueMicrotask(()=>this.onmessage({data:{ok:true,results:[]}}));if(kind==='failure')queueMicrotask(()=>this.onmessage({data:{ok:false,code:'STORAGE'}}));if(kind==='worker-error')queueMicrotask(()=>this.onerror({preventDefault(){}}));}terminate(){terminated++;}}
 const job=J.split({},()=>{},W,20);if(kind==='cancel')job.cancel();if(kind==='success')await job.promise;else await assert.rejects(job.promise);assert.equal(terminated,1);assert.equal(instance.onmessage,null);assert.equal(instance.onerror,null);job.cancel();assert.equal(terminated,1);
});
