const test=require('node:test'),assert=require('node:assert/strict'),J=require('../wwwroot/js/pdf-merge-job.js');
for(const kind of ['success','failure','cancel','timeout','post-error','worker-error'])test('merge job disposes worker on '+kind,async()=>{
 let instance,terminated=0;
 class W{constructor(){instance=this;}postMessage(){if(kind==='post-error')throw Error();if(kind==='success')queueMicrotask(()=>{this.onmessage({data:{progress:{stage:'copy',done:1,total:2}}});this.onmessage({data:{ok:true,count:2}});});if(kind==='failure')queueMicrotask(()=>this.onmessage({data:{ok:false,code:'INVALID'}}));if(kind==='worker-error')queueMicrotask(()=>this.onerror({preventDefault(){}}));}terminate(){terminated++;}}
 const job=J.start({},()=>{},W,20);if(kind==='cancel')job.cancel();
 if(kind==='success')assert.equal((await job.promise).count,2);else await assert.rejects(job.promise);
 assert.equal(terminated,1);assert.equal(instance.onmessage,null);assert.equal(instance.onerror,null);job.cancel();assert.equal(terminated,1);
});
