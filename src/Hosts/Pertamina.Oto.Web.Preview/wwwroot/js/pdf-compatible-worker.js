'use strict';
console.log=console.warn=console.error=function(){};
importScripts('/js/pdf-capacity-data.js','/js/pdf-capacity.js','/vendor/pdf-lib-1.17.1/pdf-lib.js','/js/pdf-annotations.js','/js/pdf-tags.js','/js/pdf-engine.js','/js/pdf-compatible-copy.js');
self.onmessage=async event=>{
 try{
  const r=event.data;let bytes;
  if(r.action==='blank')bytes=await OtoCompatibleCopy.blank(PDFLib);
  else if(r.action==='raster')bytes=await OtoCompatibleCopy.rasterDocument(PDFLib,OtoPdf,r.pages);
  else if(r.action==='normalize'){
   if(!(r.file instanceof Blob)||r.file.size>OtoCompatibleCopy.MAX_BYTES)throw Error('COPY_LIMIT');
   bytes=await OtoCompatibleCopy.normalize(PDFLib,OtoPdf,new Uint8Array(await r.file.arrayBuffer()),{dropLast:r.dropLast||0});
  }else throw Error('INVALID');
  self.postMessage({ok:true,bytes},[bytes.buffer]);
 }catch(e){self.postMessage({ok:false,code:['COPY_LIMIT','COPY_XFA','LIMIT','VERIFY'].includes(e.message)?e.message:'COPY_RENDER'});}
 self.close();
};
