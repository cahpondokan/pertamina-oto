/* ZIP (stored entries, UTF-8) of already-validated results. PDF bytes are copied
 * unchanged in bounded chunks; no network, compression, or new dependency. */
(function(root){
 'use strict';
 const C=typeof module==='object'?require('./pdf-capacity.js'):root.OtoPdfCapacity;
 const CHUNK=1024*1024,encoder=new TextEncoder();
 const table=Uint32Array.from({length:256},(_,n)=>{for(let k=0;k<8;k++)n=n&1?0xedb88320^(n>>>1):n>>>1;return n>>>0;});
 const crc=(bytes,value)=>{for(const b of bytes)value=table[(value^b)&255]^(value>>>8);return value>>>0;};
 const cancelled=signal=>{if(signal?.aborted)throw new DOMException('Cancelled','AbortError');};
 function plan(results){
  if(!Array.isArray(results)||!results.length||results.length>C.split.outputs)throw Error('ZIP_LIMIT');
  const seen=new Set();let offset=0,payload=0;
  const entries=results.map(result=>{
   const name=result.name,file=result.file,key=String(name).toLowerCase();
   if(typeof name!=='string'||!name.trim()||/[\\/:*?"<>|\x00-\x1f]/.test(name)||!name.toLowerCase().endsWith('.pdf')||seen.has(key))throw Error('ZIP_NAME');
   seen.add(key);const encoded=encoder.encode(name);
   if(encoded.length>1024||!Number.isSafeInteger(file?.size)||file.size<1)throw Error('ZIP_LIMIT');
   payload+=file.size;if(payload>C.split.outputBytes)throw Error('ZIP_LIMIT');
   const entry={name:encoded,file,offset};offset+=30+encoded.length+file.size+16;return entry;
  });
  const central=offset,centralSize=entries.reduce((n,e)=>n+46+e.name.length,0),size=central+centralSize+22;
  // 4 GB decimal payload + bounded headers fits classic ZIP; never wrap uint32.
  if(size>=0xffffffff)throw Error('ZIP_LIMIT');
  return {entries,central,centralSize,size};
 }
 function header(length,signature){const bytes=new Uint8Array(length),v=new DataView(bytes.buffer);v.setUint32(0,signature,true);return {bytes,v};}
 async function* chunks(p,{signal,progress}={}){
  let written=0;const checks=[];
  for(const entry of p.entries){
   cancelled(signal);const h=header(30,0x04034b50);h.v.setUint16(4,20,true);h.v.setUint16(6,0x808,true);h.v.setUint16(12,33,true);h.v.setUint16(26,entry.name.length,true);
   yield h.bytes;yield entry.name;written+=30+entry.name.length;let checksum=0xffffffff;
   for(let at=0;at<entry.file.size;at+=CHUNK){
    cancelled(signal);const end=Math.min(at+CHUNK,entry.file.size),bytes=new Uint8Array(await entry.file.slice(at,end).arrayBuffer());
    if(bytes.length!==end-at)throw Error('ZIP_READ');cancelled(signal);checksum=crc(bytes,checksum);yield bytes;written+=bytes.length;progress?.(written,p.size);
    // Allow paint/cancellation between chunks even when local reads resolve quickly.
    await new Promise(resolve=>setTimeout(resolve,0));
   }
   checksum=(checksum^0xffffffff)>>>0;checks.push(checksum);const d=header(16,0x08074b50);d.v.setUint32(4,checksum,true);d.v.setUint32(8,entry.file.size,true);d.v.setUint32(12,entry.file.size,true);yield d.bytes;written+=16;
  }
  for(let i=0;i<p.entries.length;i++){
   cancelled(signal);const e=p.entries[i],h=header(46,0x02014b50);h.v.setUint16(4,20,true);h.v.setUint16(6,20,true);h.v.setUint16(8,0x808,true);h.v.setUint16(14,33,true);h.v.setUint32(16,checks[i],true);h.v.setUint32(20,e.file.size,true);h.v.setUint32(24,e.file.size,true);h.v.setUint16(28,e.name.length,true);h.v.setUint32(42,e.offset,true);yield h.bytes;yield e.name;written+=46+e.name.length;
  }
  cancelled(signal);const end=header(22,0x06054b50);end.v.setUint16(8,p.entries.length,true);end.v.setUint16(10,p.entries.length,true);end.v.setUint32(12,p.centralSize,true);end.v.setUint32(16,p.central,true);yield end.bytes;written+=22;
  if(written!==p.size)throw Error('ZIP_VERIFY');progress?.(written,p.size);
 }
 function stream(p,options){const iterator=chunks(p,options);return new ReadableStream({async pull(controller){try{const next=await iterator.next();next.done?controller.close():controller.enqueue(next.value);}catch(e){controller.error(e);}},async cancel(){await iterator.return();}});}
 async function temporary(p,options,jobs=root.OtoMergeJob,storage=navigator.storage){
  let directory,writable;
  try{
   cancelled(options.signal);const estimate=await storage.estimate();if(Number.isFinite(estimate.quota)&&Number.isFinite(estimate.usage)&&estimate.quota-estimate.usage<p.size+1024*1024)throw Error('STORAGE');
   directory=await jobs.create();cancelled(options.signal);
   const dir=await (await storage.getDirectory()).getDirectoryHandle(directory),handle=await dir.getFileHandle('results.zip',{create:true});
   writable=await handle.createWritable();await stream(p,options).pipeTo(writable,{signal:options.signal});writable=null;cancelled(options.signal);
   const file=await handle.getFile();if(file.size!==p.size)throw Error('ZIP_VERIFY');
   const owned=directory;directory=null;return {file,release:()=>jobs.remove(owned)};
  }finally{try{await writable?.abort();}catch{}if(directory)await jobs.remove(directory);}
 }
 const api={plan,stream,temporary};if(typeof module==='object')module.exports=api;else root.OtoPdfArchive=api;
})(typeof globalThis!=='undefined'?globalThis:this);
