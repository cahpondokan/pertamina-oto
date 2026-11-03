'use strict';
console.log=console.warn=console.error=function(){};
importScripts('/js/pdf-capacity-data.js','/js/pdf-capacity.js','/vendor/pdf-lib-1.17.1/pdf-lib.js','/js/pdf-annotations.js','/js/pdf-tags.js','/js/pdf-engine.js','/js/pdf-merge-engine.js');
self.onmessage=async function(event){
 let handle;let last=0;
 const reader=new FileReaderSync();
 const progress=value=>{const now=Date.now();if(now-last>150||value.done===0||value.done===value.total){last=now;self.postMessage({progress:value});}};
 try{
  const request=event.data;
  const io={read:(file,start,end)=>new Uint8Array(reader.readAsArrayBuffer(file.slice(start,end))),progress};
  if(request.action!=='inspect'){
   const dir=await (await navigator.storage.getDirectory()).getDirectoryHandle(request.directory);
   const output=await dir.getFileHandle('result.pdf',{create:true});
   handle=await output.createSyncAccessHandle();let offset=0;
   io.sink={write:async bytes=>{let done=0;while(done<bytes.length){const n=handle.write(bytes.subarray(done),{at:offset});if(!n)throw Error('STORAGE');offset+=n;done+=n;}},file:async()=>{handle.flush();handle.close();handle=null;return output.getFile();}};
  }
  const result=await OtoMergeEngine.run(PDFLib,OtoPdf,request,io);
  self.postMessage({ok:true,...result});
 }catch(e){
  const known=['FILE_COUNT','TOTAL_BYTES','LIMIT','RANGE','INVALID','ENCRYPTED','UNSUPPORTED','UNSUPPORTED_ANNOTATIONS','UNSAFE_LINK','LINK_TARGET','ANNOTATION_OPERATION','UNSUPPORTED_FORM','UNSUPPORTED_ACTION','UNSUPPORTED_ATTACHMENT','UNSUPPORTED_SIGNATURE','UNSUPPORTED_TAGGED','TAGGED_STRUCTURE','TAGGED_OPERATION','TAGGED_MIXED','TAGGED_CONFLICT','MERGE','EMPTY','VERIFY','OPTIONS','STORAGE'];
  const code=e.message==='OTO_LIMIT'?'LIMIT':e.name==='QuotaExceededError'?'STORAGE':e.message;
  self.postMessage({ok:false,code:known.includes(code)?code:'INVALID'});
 }finally{try{handle?.close();}catch{}self.close();}
};
