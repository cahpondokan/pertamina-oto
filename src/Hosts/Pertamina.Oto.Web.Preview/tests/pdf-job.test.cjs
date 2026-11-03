const test=require('node:test'), assert=require('node:assert/strict'), {start}=require('../wwwroot/js/pdf-job.js');
let last;
class WorkerStub {
 constructor(url){assert.equal(url,'/js/pdf-worker.js');last=this;this.terminated=0;}
 postMessage(request){this.request=request;}
 terminate(){this.terminated++;}
}
test('worker success closes exactly once',async()=>{const h=start({action:'inspect'},WorkerStub);last.onmessage({data:{ok:true,counts:[2]}});assert.deepEqual((await h.promise).counts,[2]);h.cancel();assert.equal(last.terminated,1);});
test('cancel actually terminates pending worker',async()=>{const h=start({},WorkerStub);h.cancel();await assert.rejects(h.promise,/CANCELLED/);assert.equal(last.terminated,1);});
test('timeout actually terminates pending worker',async()=>{const h=start({},WorkerStub,5);await assert.rejects(h.promise,/TIMEOUT/);assert.equal(last.terminated,1);});
test('worker failure closes and rejects',async()=>{const h=start({},WorkerStub);last.onerror({preventDefault(){}});await assert.rejects(h.promise,/WORKER/);assert.equal(last.terminated,1);});
test('parser rejection closes worker',async()=>{const h=start({},WorkerStub);last.onmessage({data:{ok:false,code:'ENCRYPTED'}});await assert.rejects(h.promise,/ENCRYPTED/);assert.equal(last.terminated,1);});
test('clone failure closes worker',async()=>{class F extends WorkerStub{postMessage(){throw Error('fixture');}}const h=start({},F);await assert.rejects(h.promise,/WORKER/);assert.equal(last.terminated,1);});
