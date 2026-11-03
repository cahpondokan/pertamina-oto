'use strict';
console.log=console.warn=console.error=function(){};
importScripts('/js/pdf-capacity-data.js','/js/pdf-capacity.js','/vendor/pdf-lib-1.17.1/pdf-lib.js','/js/pdf-annotations.js','/js/pdf-tags.js','/js/pdf-engine.js','/js/pdf-merge-engine.js','/js/pdf-file-images.js','/js/pdf-file-engine.js','/js/pdf-split-engine.js');
self.onmessage=async function(event){
 let handle,last=0;
 const reader=new FileReaderSync(),progress=value=>{const now=Date.now();if(now-last>150||value.done===0||value.done===value.total){last=now;self.postMessage({progress:value});}};
 try{
  if(!navigator.storage?.getDirectory||!navigator.locks)throw Error('STORAGE_BROWSER');
  // One file-backed computation per origin, including tabs; no unbounded queue.
  await navigator.locks.request('oto-file-processing-v1',{ifAvailable:true},async lock=>{
   if(!lock)throw Error('BUSY');
   const request=event.data;
   const io={read:(file,start,end)=>new Uint8Array(reader.readAsArrayBuffer(file.slice(start,end))),progress};
   if(request.action==='run'){
    if(typeof request.directory!=='string'||!/^oto-merge-v1-[a-f0-9-]{36}$/.test(request.directory))throw Error('STORAGE');
    const dir=await (await navigator.storage.getDirectory()).getDirectoryHandle(request.directory);
    io.output=async index=>{
     if(!Number.isInteger(index)||index<0||index>=OtoPdfCapacity.split.outputs)throw Error('SPLIT');
     const output=await dir.getFileHandle('part-'+index+'.pdf',{create:true});
     handle=await output.createSyncAccessHandle();handle.truncate(0);let offset=0;
     return {
      prepare:async size=>{const estimate=await navigator.storage.estimate?.().catch(()=>null);if(estimate&&Number.isFinite(estimate.quota)&&Number.isFinite(estimate.usage)&&size>estimate.quota-estimate.usage)throw Error('STORAGE');},
      write:async bytes=>{let done=0;while(done<bytes.length){const n=handle.write(bytes.subarray(done),{at:offset});if(!n)throw Error('STORAGE');offset+=n;done+=n;}},
      file:async()=>{handle.flush();handle.close();handle=null;return output.getFile();}
     };
    };
   }
   const result=await OtoFileEngine.run(PDFLib,OtoPdf,request,io);
   self.postMessage({ok:true,...result});
  });
 }catch(e){
  const known=['FILE_COUNT','IMAGE_LIMIT','IMAGE_FORMAT','IMAGE_BROWSER','LAYOUT','TEXT','SINGLE','TOTAL_BYTES','OUTPUT_LIMIT','LIMIT','RANGE','INVALID','ENCRYPTED','UNSUPPORTED','UNSUPPORTED_ANNOTATIONS','UNSAFE_LINK','LINK_TARGET','ANNOTATION_OPERATION','UNSUPPORTED_FORM','UNSUPPORTED_ACTION','UNSUPPORTED_ATTACHMENT','UNSUPPORTED_SIGNATURE','UNSUPPORTED_TAGGED','TAGGED_STRUCTURE','TAGGED_OPERATION','TAGGED_MIXED','TAGGED_CONFLICT','EMPTY','VERIFY','OPTIONS','STORAGE','STORAGE_BROWSER','BUSY','TOOL','SPLIT','PARTITION'];
  const code=e.message==='OTO_LIMIT'?'LIMIT':e.name==='QuotaExceededError'?'STORAGE':e.message;
  self.postMessage({ok:false,code:known.includes(code)?code:'INVALID'});
 }finally{try{handle?.close();}catch{}self.close();}
};
