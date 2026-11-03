/* One disposable worker per operation; no worker reuse or unbounded queued jobs. */
(function(root){
  'use strict';
  const Capacity=typeof module!=='undefined'?require('./pdf-capacity.js'):root.OtoPdfCapacity;
  function start(request, WorkerType, timeout=Capacity.bounded.timeout,onProgress=()=>{}) {
    let cancel;
    const promise=new Promise((resolve,reject)=>{
      let worker, settled=false;
      try {worker=new WorkerType('/js/pdf-worker.js');} catch {reject(Error('WORKER'));return;}
      const timer=setTimeout(()=>finish(Error('TIMEOUT')),timeout);
      function finish(error,value){
        if(settled)return;
        settled=true;clearTimeout(timer);worker.terminate();
        if(error)reject(error);else resolve(value);
      }
      cancel=()=>finish(Error('CANCELLED'));
      worker.onerror=event=>{event.preventDefault?.();finish(Error('WORKER'));};
      worker.onmessage=event=>{if(settled)return;if(event.data.progress){onProgress(event.data.progress);return;}event.data.ok?finish(null,event.data):finish(Error(event.data.code));};
      try {worker.postMessage(request);} catch {finish(Error('WORKER'));}
    });
    return {promise,cancel:()=>cancel?.()};
  }
  if(typeof module!=='undefined')module.exports={start};else root.OtoPdfJob={start};
})(typeof self!=='undefined'?self:globalThis);
