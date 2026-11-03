/* A terminated job owns no parser/renderer/buffer. Only a completed result's
 * OPFS file survives until save is confirmed or the user discards it. */
(function(root){
 'use strict';
 const Capacity=typeof module!=='undefined'?require('./pdf-capacity.js'):root.OtoPdfCapacity;
 const PREFIX='oto-merge-v1-',locks=new Map();
 async function cleanStale(){
  if(!navigator.storage?.getDirectory||!navigator.locks)return;
  const dir=await navigator.storage.getDirectory();
  for await(const [name,entry] of dir.entries())if(name.startsWith(PREFIX)&&entry.kind==='directory'){
   await navigator.locks.request(name,{ifAvailable:true},async lock=>{if(lock)await dir.removeEntry(name,{recursive:true});});
  }
 }
 const ready=typeof navigator!=='undefined'?cleanStale().catch(()=>{}):Promise.resolve();
 async function create(){
  await ready;
  if(!navigator.storage?.getDirectory||!navigator.locks)throw Error('STORAGE_BROWSER');
  const name=PREFIX+crypto.randomUUID();
  await new Promise((resolve,reject)=>{navigator.locks.request(name,async()=>{await new Promise(release=>{locks.set(name,release);resolve();});}).catch(reject);});
  try{await (await navigator.storage.getDirectory()).getDirectoryHandle(name,{create:true});return name;}
  catch(e){locks.get(name)?.();locks.delete(name);throw e;}
 }
 async function remove(name){
  if(!name)return;
  try{const dir=await navigator.storage.getDirectory();for(let attempt=0;;attempt++){try{await dir.removeEntry(name,{recursive:true});break;}catch(e){if(e.name==='NotFoundError')break;if(attempt>=5)throw e;await new Promise(resolve=>setTimeout(resolve,100));}}}
  catch(e){if(e.name!=='NotFoundError')throw e;}
  finally{locks.get(name)?.();locks.delete(name);}
 }
 async function result(name,index){if(index!==undefined&&(!Number.isInteger(index)||index<0||index>=Capacity.split.outputs))throw Error('SPLIT');const d=await (await navigator.storage.getDirectory()).getDirectoryHandle(name);return (await d.getFileHandle(index===undefined?'result.pdf':'part-'+index+'.pdf')).getFile();}
 function startAt(script,request,progress,WorkerType,timeout){
  let cancel,worker,timer;
  const promise=new Promise((resolve,reject)=>{
   let settled=false;
   const finish=(error,value)=>{if(settled)return;settled=true;clearTimeout(timer);if(worker){worker.onmessage=worker.onerror=null;worker.terminate();worker=null;}if(error)reject(error);else resolve(value);};
   cancel=()=>finish(Error('CANCELLED'));
   try{worker=new WorkerType(script);timer=setTimeout(()=>finish(Error('TIMEOUT')),timeout);
    worker.onmessage=e=>{if(e.data.progress){progress(e.data.progress);return;}e.data.ok?finish(null,e.data):finish(Error(e.data.code));};
    worker.onerror=e=>{e.preventDefault?.();finish(Error('WORKER'));};worker.postMessage(request);
   }catch{finish(Error('WORKER'));}
  });
  return {promise,cancel:()=>cancel?.()};
 }
 const start=(request,progress=()=>{},WorkerType=Worker,timeout=Capacity.merge.timeout)=>startAt('/js/pdf-merge-worker.js',request,progress,WorkerType,timeout);
 const split=(request,progress=()=>{},WorkerType=Worker,timeout=Capacity.split.timeout)=>startAt('/js/pdf-split-worker.js',request,progress,WorkerType,timeout);
 const file=(request,progress=()=>{},WorkerType=Worker,timeout=Capacity.input(request.tool).timeout)=>startAt('/js/pdf-split-worker.js',request,progress,WorkerType,timeout);
 const api={create,remove,result,start,split,file};if(typeof module!=='undefined')module.exports=api;else root.OtoMergeJob=api;
})(typeof self!=='undefined'?self:globalThis);
