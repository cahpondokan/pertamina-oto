/* File-backed JPG/PNG input. Inspect headers with bounded reads before native
 * decoding, keep pixel budgets, and release each bitmap/canvas before the next. */
(function(root){
 'use strict';
 const local=typeof module!=='undefined',C=local?require('./pdf-capacity.js'):root.OtoPdfCapacity;
 const fail=code=>{throw Error(code);},mm=n=>n*72/25.4;
 function header(file,read){
  let visits=0,metadata=0;
  function at(offset,size){if(!Number.isSafeInteger(offset)||offset<0||offset+size>file.size)fail('IMAGE_FORMAT');const b=read(file,offset,offset+size);if(b.length!==size)fail('IMAGE_FORMAT');return b;}
  const first=at(0,Math.min(33,file.size));let width,height,type;
  const view=b=>new DataView(b.buffer,b.byteOffset,b.byteLength),word=b=>String.fromCharCode(...b);
  if(first.length===33&&[137,80,78,71,13,10,26,10].every((n,i)=>first[i]===n)){
   if(view(first).getUint32(8)!==13||word(first.subarray(12,16))!=='IHDR'||first[24]!==8||![0,2,3,4,6].includes(first[25]))fail('IMAGE_FORMAT');
   width=view(first).getUint32(16);height=view(first).getUint32(20);type='image/png';
   let offset=8,ended=false,data=false;
   while(offset+12<=file.size){
    if(++visits>C.merge.objects)fail('IMAGE_LIMIT');
    const chunk=at(offset,8),length=view(chunk).getUint32(0),kind=word(chunk.subarray(4));
    if(length>file.size-offset-12||kind==='acTL')fail('IMAGE_FORMAT');
    if(kind==='IDAT')data=true;else {metadata+=length+12;if(metadata>C.merge.metadataBytes)fail('IMAGE_LIMIT');}
    offset+=length+12;if(kind==='IEND'){if(length!==0)fail('IMAGE_FORMAT');ended=true;break;}
   }
   if(!ended||!data)fail('IMAGE_FORMAT');
  }else if(first.length>12&&first[0]===255&&first[1]===216){
   type='image/jpeg';let offset=2;
   while(offset+4<=file.size){
    if(++visits>C.merge.objects||metadata>C.merge.metadataBytes)fail('IMAGE_LIMIT');
    if(at(offset++,1)[0]!==255)fail('IMAGE_FORMAT');
    let marker=at(offset++,1)[0];while(marker===255){if(++visits>C.merge.objects)fail('IMAGE_LIMIT');marker=at(offset++,1)[0];}
    if(marker===218||marker===217)break;
    if(marker===1||(marker>=208&&marker<=215))continue;
    const length=view(at(offset,2)).getUint16(0);if(length<2||offset+length>file.size)fail('IMAGE_FORMAT');
    metadata+=length;
    if([192,193,194].includes(marker)){
     if(length<8)fail('IMAGE_FORMAT');const sof=at(offset,8);
     if(sof[2]!==8||![1,3].includes(sof[7]))fail('IMAGE_FORMAT');height=view(sof).getUint16(3);width=view(sof).getUint16(5);break;
    }
    offset+=length;
   }
  }else fail('IMAGE_FORMAT');
  if(!width||!height)fail('IMAGE_FORMAT');checkSize(width,height);return {width,height,type};
 }
 function checkSize(width,height){if(!Number.isInteger(width)||!Number.isInteger(height)||width<1||height<1||width>C.images.dimension||height>C.images.dimension||width*height>C.images.pixels)fail('IMAGE_LIMIT');}
 async function run(P,E,request,io){
  const headers=request.files.map(file=>header(file,io.read));
  if(headers.reduce((n,h)=>n+h.width*h.height,0)>C.images.totalPixels)fail('IMAGE_LIMIT');
  if(typeof createImageBitmap!=='function'||typeof OffscreenCanvas!=='function')fail('IMAGE_BROWSER');
  const inspecting=request.action==='inspect',sizes=[],thumbnails=[];let encoded=0,actualPixels=0;
  const options=request.options||{},layout=options.layout,margin=mm(options.margin);
  if(!inspecting&&(!['auto','portrait','landscape'].includes(layout)||typeof options.margin!=='number'||!Number.isFinite(margin)||options.margin<0||options.margin>30))fail('OPTIONS');
  const doc=inspecting?null:await P.PDFDocument.create();doc?.setProducer('PERTAMINA OTO Web · local images');
  for(let i=0;i<request.files.length;i++){
   let bitmap,canvas;io.progress?.({stage:'inspect',done:i,total:request.files.length});
   try{
    // Pass the file handle to the native decoder; never request its full ArrayBuffer.
    bitmap=await createImageBitmap(request.files[i]);checkSize(bitmap.width,bitmap.height);
    actualPixels+=bitmap.width*bitmap.height;if(actualPixels>C.images.totalPixels)fail('IMAGE_LIMIT');
    sizes.push({width:bitmap.width,height:bitmap.height});
    const scale=inspecting?Math.min(1,240/Math.max(bitmap.width,bitmap.height)):1;
    canvas=new OffscreenCanvas(Math.max(1,Math.round(bitmap.width*scale)),Math.max(1,Math.round(bitmap.height*scale)));
    const ctx=canvas.getContext('2d',{alpha:false});if(!ctx)fail('IMAGE_BROWSER');
    ctx.fillStyle='#fff';ctx.fillRect(0,0,canvas.width,canvas.height);ctx.drawImage(bitmap,0,0,canvas.width,canvas.height);
    const blob=await canvas.convertToBlob({type:'image/jpeg',quality:inspecting?.8:.92});if(blob.type!=='image/jpeg')fail('IMAGE_BROWSER');
    encoded+=blob.size;if(encoded>C.bounded.outputBytes)fail('IMAGE_LIMIT');
    const bytes=new Uint8Array(await blob.arrayBuffer());
    if(inspecting)thumbnails.push(bytes);
    else{
     const image=await doc.embedJpg(bytes),landscape=layout==='landscape'||layout==='auto'&&bitmap.width>bitmap.height;
     const [width,height]=landscape?[mm(297),mm(210)]:[mm(210),mm(297)],s=Math.min((width-2*margin)/image.width,(height-2*margin)/image.height);
     doc.addPage([width,height]).drawImage(image,{x:(width-image.width*s)/2,y:(height-image.height*s)/2,width:image.width*s,height:image.height*s});
    }
   }catch(e){if(['IMAGE_LIMIT','IMAGE_BROWSER','OPTIONS'].includes(e.message))throw e;fail('IMAGE_FORMAT');}
   finally{bitmap?.close();if(canvas){canvas.width=1;canvas.height=1;}}
  }
  return inspecting?{counts:sizes.map(()=>1),sizes,thumbnails}:{document:doc,count:request.files.length};
 }
 const api={header,run};if(local)module.exports=api;else root.OtoFileImages=api;
})(typeof self!=='undefined'?self:globalThis);
