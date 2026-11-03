/* Anonymous, bounded operational evidence. Never serialize requests, files,
 * DOM, error messages/stacks, URLs, options or browser identifiers. */
(function(root,factory){
 'use strict';
 const api=factory();
 if(typeof module==='object'&&module.exports)module.exports=api;
 else if(root.document?.querySelector('meta[name="oto-diagnostics"]')?.content==='/api/diagnostics/events'){
  const client=api.create(root);root.OtoDiagnostics=client;
  root.addEventListener('error',event=>client.runtime(event.error));
  root.addEventListener('unhandledrejection',event=>client.runtime(event.reason));
  root.addEventListener('online',()=>client.flush());
 }
})(typeof globalThis!=='undefined'?globalThis:this,function(){
 'use strict';
 const VERSION='web-audit-1',ENDPOINT='/api/diagnostics/events';
 const FEATURES=new Set(['shell','merge','organize','split','images-pdf','numbers','watermark','remove','extract','rotate','crop']);
 const STAGES=new Set(['input','process','save','runtime','cleanup','preview','cloud']);
 const EVENTS=new Set(['started','succeeded','failed','cancelled','download_requested','save_succeeded','save_cancelled','save_failed','runtime_error']);
 const CODES=new Set(['NONE','UNKNOWN','CANCELLED','WORKER','TIMEOUT','VERIFY','INVALID','ENCRYPTED','LIMIT','SESSION_LIMIT','TYPE','SINGLE','MERGE','EMPTY','SPLIT','PARTITION','RANGE','OPTIONS','TEXT','LAYOUT','IMAGE_FORMAT','IMAGE_LIMIT','IMAGE_BROWSER','FILE_COUNT','TOTAL_BYTES','STORAGE_BROWSER','STORAGE','UNSUPPORTED','UNSUPPORTED_FORM','UNSUPPORTED_SIGNATURE','UNSUPPORTED_ATTACHMENT','UNSUPPORTED_ACTION','UNSUPPORTED_ANNOTATIONS','UNSUPPORTED_TAGGED','UNSAFE_LINK','LINK_TARGET','ANNOTATION_OPERATION','TAGGED_STRUCTURE','TAGGED_OPERATION','TAGGED_MIXED','TAGGED_CONFLICT','CLOUD_CONFIG','CLOUD_LOAD','CLOUD_AUTH','CLOUD_DOWNLOAD','CLOUD_RESPONSE','CLOUD_CHANGED','TYPE_ERROR','RANGE_ERROR','REFERENCE_ERROR','SYNTAX_ERROR','QUOTA','NOT_ALLOWED','NOT_READABLE','SECURITY','NETWORK','SERVER_ERROR']);
 function code(error){
  // Exact closed membership; never retain the original message or exception.
  try{
   if(error?.name==='AbortError'||error?.message==='CANCELLED')return 'CANCELLED';
   if(CODES.has(error?.message)&&error.message!=='NONE')return error.message;
   const names={TypeError:'TYPE_ERROR',RangeError:'RANGE_ERROR',ReferenceError:'REFERENCE_ERROR',SyntaxError:'SYNTAX_ERROR',QuotaExceededError:'QUOTA',NotAllowedError:'NOT_ALLOWED',NotReadableError:'NOT_READABLE',SecurityError:'SECURITY',NetworkError:'NETWORK'};
   return Object.hasOwn(names,error?.name)?names[error.name]:'UNKNOWN';
  }catch{return 'UNKNOWN';}
 }
 function browser(env){
  // Only a coarse family; never send or retain the UA string/version/device.
  const ua=env.navigator?.userAgent||'';
  return /Edg\//.test(ua)?'edge':/Firefox\//.test(ua)?'firefox':/Chrome\//.test(ua)?'chromium':/Safari\//.test(ua)?'safari':'other';
 }
 function create(env){
  const queue=[],family=browser(env);let timer=null,sending=false,retries=0,dropped=0,disposed=false;
  const now=()=>Date.now();
  const uuid=()=>env.crypto?.randomUUID?.();
  function schedule(delay=1500){if(!timer&&!disposed&&queue.length){timer=env.setTimeout(()=>{timer=null;void flush();},delay);timer?.unref?.();}}
  function push(processId,feature,stage,event,errorCode,elapsedMs){
   try{
    if(disposed||!FEATURES.has(feature)||!STAGES.has(stage)||!EVENTS.has(event)||!CODES.has(errorCode))return;
    const id=uuid();if(!id||!processId)return;
    if(queue.length>=128){dropped++;return;}
    queue.push(Object.freeze({id,processId,event,feature,stage,code:errorCode,elapsedMs:Math.max(0,Math.min(3600000,Math.round(elapsedMs))),occurredAt:now(),clientVersion:VERSION,browser:family}));
    schedule();
   }catch{/* Diagnostics must not break the document operation. */}
  }
  function begin(feature,stage='process'){
   let processId;try{processId=uuid();}catch{}const started=now();let ended=false;
   push(processId,feature,stage,'started','NONE',0);
   const record=(event,eventStage,errorCode='NONE')=>push(processId,feature,eventStage,event,errorCode,now()-started);
   return Object.freeze({
    success(){if(!ended){ended=true;record('succeeded',stage);}},
    fail(error){if(!ended){ended=true;const reason=code(error);record(reason==='CANCELLED'?'cancelled':'failed',stage,reason);}},
    record(event,eventStage,error){record(event,eventStage,error?code(error):'NONE');}
   });
  }
  async function flush(){
   if(sending||disposed||!queue.length)return;
   if(timer){env.clearTimeout(timer);timer=null;}
   if(env.navigator?.onLine===false){schedule(30000);return;}
   sending=true;const batch=queue.slice(0,16),abort=new env.AbortController();
   const deadline=env.setTimeout(()=>abort.abort(),4000);deadline?.unref?.();
   try{
    const response=await env.fetch(ENDPOINT,{method:'POST',credentials:'omit',mode:'same-origin',redirect:'error',referrerPolicy:'no-referrer',cache:'no-store',headers:{'Content-Type':'application/json','X-Oto-Diagnostics':'1'},body:JSON.stringify({version:1,events:batch}),signal:abort.signal});
    if(response.status===204){queue.splice(0,batch.length);retries=0;}
    else if([400,403,413,415,409].includes(response.status)){queue.splice(0,batch.length);dropped+=batch.length;retries=0;}
    else throw new Error('delivery');
   }catch{
    retries++;if(retries>=6){queue.splice(0,batch.length);dropped+=batch.length;retries=0;}
   }finally{env.clearTimeout(deadline);sending=false;schedule(retries?Math.min(60000,2000*2**retries):1500);}
  }
  function runtime(error){try{const id=uuid();push(id,'shell','runtime','runtime_error',code(error),0);}catch{}}
  return Object.freeze({begin,runtime,flush,status:()=>({queued:queue.length,dropped,sending}),dispose(){disposed=true;if(timer)env.clearTimeout(timer);timer=null;queue.length=0;}});
 }
 return {create,code,FEATURES,STAGES,EVENTS,CODES,VERSION,ENDPOINT};
});
