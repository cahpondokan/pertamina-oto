'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js'),E=require('../wwwroot/js/pdf-engine.js'),F=require('../wwwroot/js/pdf-file-engine.js'),I=require('../wwwroot/js/pdf-file-images.js');
const dir=process.env.OTO_PDF_TEST_DIR,read=(f,a,b)=>f.bytes.slice(a,b),wrap=bytes=>({bytes,size:bytes.length});
const jpg=()=>new Uint8Array(fs.readFileSync(path.join(dir,'portrait.jpg')));
test('JPG/PNG headers use short random reads even when virtual input size is 4 GB',()=>{
 for(const name of ['portrait.jpg','transparent.png']){const bytes=new Uint8Array(fs.readFileSync(path.join(dir,name))),file={size:4e9};let max=0,total=0;
  const h=I.header(file,(_f,a,b)=>{max=Math.max(max,b-a);total+=b-a;return bytes.slice(a,b);});assert.equal(h.width,400);assert.equal(h.height,600);assert.ok(max<=33);assert.ok(total<1000);
 }
});
test('malformed, animated and excessive-dimension headers are refused before native decoding',()=>{
 const png=new Uint8Array(fs.readFileSync(path.join(dir,'transparent.png')));
 const huge=png.slice();new DataView(huge.buffer).setUint32(16,6001);assert.throws(()=>I.header(wrap(huge),read),/^Error: IMAGE_LIMIT$/);
 const truncated=png.slice(0,33);assert.throws(()=>I.header(wrap(truncated),read),/^Error: IMAGE_FORMAT$/);
 const animated=png.slice();animated.set(Buffer.from('acTL'),37);assert.throws(()=>I.header(wrap(animated),read),/^Error: IMAGE_FORMAT$/);
 assert.throws(()=>I.header(wrap(Buffer.from('<svg>unsafe file</svg>')),read),/^Error: IMAGE_FORMAT$/);
 const cmyk=new Uint8Array(fs.readFileSync(path.join(dir,'cmyk.jpg')));assert.throws(()=>I.header(wrap(cmyk),read),/^Error: IMAGE_FORMAT$/);
});
async function nativePort(action){const oldBitmap=global.createImageBitmap,oldCanvas=global.OffscreenCanvas;let closed=0,cleared=0,received=[];const jpeg=jpg();
 global.createImageBitmap=async file=>{received.push(file);return {width:400,height:600,close(){closed++;}};};
 global.OffscreenCanvas=class{constructor(w,h){this.width=w;this.height=h;}getContext(){return {fillRect(){},drawImage(){}};}set width(v){this.w=v;if(v===1)cleared++;}get width(){return this.w;}async convertToBlob(){return new Blob([jpeg],{type:'image/jpeg'});}};
 try{await action({stats:()=>({closed,cleared,received}),jpeg});}finally{if(oldBitmap===undefined)delete global.createImageBitmap;else global.createImageBitmap=oldBitmap;if(oldCanvas===undefined)delete global.OffscreenCanvas;else global.OffscreenCanvas=oldCanvas;}
}
test('image inspection and PDF creation retain File handles and release native resources',async()=>nativePort(async({stats,jpeg})=>{
 const bytes=Buffer.alloc(12*1048576);bytes.set(jpeg);const file=wrap(bytes);file.arrayBuffer=()=>{throw Error('FULL_SOURCE_READ');};
 const request={tool:'images-pdf',files:[file],options:{layout:'auto',margin:10},name:'images'};
 const inspected=await F.run(P,E,{...request,action:'inspect'},{read});assert.deepEqual(inspected.counts,[1]);assert.ok(inspected.thumbnails[0].length);assert.equal(stats().received[0],file);
 let resultFile;const result=await F.run(P,E,{...request,action:'run'},{read,output:async()=>{const chunks=[];return {write:async b=>chunks.push(b),file:async()=>resultFile=wrap(Buffer.concat(chunks))};}});
 assert.equal(result.results[0].count,1);assert.deepEqual((await F.run(P,E,{tool:'rotate',action:'inspect',files:[resultFile]},{read})).counts,[1]);assert.equal(stats().closed,2);assert.equal(stats().cleared,2);
}));
test('pixel limit and total encoded JPEG guard stay enforced, including decoder failure cleanup',async()=>nativePort(async({stats,jpeg})=>{
 const file=wrap(jpeg),req={tool:'images-pdf',action:'inspect',files:[file]};
 global.createImageBitmap=async()=>({width:6001,height:10,close(){file.closed=true;}});await assert.rejects(F.run(P,E,req,{read}),/^Error: IMAGE_LIMIT$/);assert.equal(file.closed,true);
 global.createImageBitmap=async()=>({width:400,height:600,close(){file.closed2=true;}});
 global.OffscreenCanvas=class{getContext(){return {fillRect(){},drawImage(){}};}async convertToBlob(){return {type:'image/jpeg',size:41943041,arrayBuffer(){throw Error('MUST_NOT_READ');}};}};
 await assert.rejects(F.run(P,E,req,{read}),/^Error: IMAGE_LIMIT$/);assert.equal(file.closed2,true);
}));
