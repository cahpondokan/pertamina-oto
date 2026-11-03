'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js'),E=require('../wwwroot/js/pdf-engine.js');
const key=name=>P.PDFName.of(name);
const inspect=bytes=>E.run(P,{action:'inspect',tool:'numbers',buffers:[bytes]});
async function fixture(edit=()=>{}) {
  const doc=await P.PDFDocument.create(),page=doc.addPage([300,400]);
  page.drawText('OTO synthetic compatibility fixture',{x:20,y:350,size:10});
  // pdf-lib creates /Annots [] during drawText; retain it for all tests unless
  // deliberately replaced. No cleanup/flattening is used to make fixtures pass.
  await edit(doc,page);
  return doc.save({useObjectStreams:false});
}
const accepted=[
  ['ordinary text PDF produced by pdf-lib',()=>{}],
  ['link annotation',(d,p)=>p.node.set(key('Annots'),d.context.obj([d.context.register(d.context.obj({Type:'Annot',Subtype:'Link',Rect:[1,1,20,20],A:{S:'URI',URI:P.PDFString.of('https://example.com')}}))]))],
  ['text annotation',(d,p)=>p.node.set(key('Annots'),d.context.obj([{Type:'Annot',Subtype:'Text',Rect:[1,1,20,20]}]))],
  ['no annotation key',(d,p)=>p.node.delete(key('Annots'))],
  ['direct empty annotation array',(d,p)=>p.node.set(key('Annots'),d.context.obj([]))],
  ['indirect empty annotation array',(d,p)=>p.node.set(key('Annots'),d.context.register(d.context.obj([])))],
  ['explicit null annotations',(d,p)=>p.node.set(key('Annots'),P.PDFNull)],
  ['indirect null annotations',(d,p)=>p.node.set(key('Annots'),d.context.register(P.PDFNull))],
  ['empty AcroForm dictionary',d=>d.catalog.set(key('AcroForm'),d.context.obj({}))],
  ['empty Fields array',d=>d.catalog.set(key('AcroForm'),d.context.obj({Fields:[]}))],
  ['indirect empty Fields and form',d=>d.catalog.set(key('AcroForm'),d.context.register(d.context.obj({Fields:d.context.register(d.context.obj([]))})))],
  ['explicit null AcroForm',d=>d.catalog.set(key('AcroForm'),P.PDFNull)],
  ['indirect null AcroForm',d=>d.catalog.set(key('AcroForm'),d.context.register(P.PDFNull))]
];
for(const [name,edit] of accepted)test('accepts '+name+' without changing source bytes',async()=>{
  const bytes=await fixture(edit),before=Buffer.from(bytes);
  assert.deepEqual((await inspect(bytes)).counts,[1]);assert.deepEqual(Buffer.from(bytes),before);
});
const rejected=[
  ['annotation array with null item',(d,p)=>p.node.set(key('Annots'),d.context.obj([null])),'UNSUPPORTED_ANNOTATIONS'],
  ['annotation wrong type dictionary',(d,p)=>p.node.set(key('Annots'),d.context.obj({})),'UNSUPPORTED_ANNOTATIONS'],
  ['annotation wrong type string',(d,p)=>p.node.set(key('Annots'),P.PDFString.of('')),'UNSUPPORTED_ANNOTATIONS'],
  ['dangling annotation reference',(d,p)=>p.node.set(key('Annots'),P.PDFRef.of(9999)),'INVALID'],
  ['cyclic annotation reference',(d,p)=>{const r=d.context.nextRef();d.context.assign(r,r);p.node.set(key('Annots'),r);},'INVALID'],
  ['overlong annotation reference chain',(d,p)=>{let r=d.context.register(d.context.obj([]));for(let i=0;i<33;i++)r=d.context.register(r);p.node.set(key('Annots'),r);},'INVALID'],
  ['real text form',d=>d.catalog.set(key('AcroForm'),d.context.obj({Fields:[{FT:'Tx',T:P.PDFString.of('fixture')}]})),'UNSUPPORTED_FORM'],
  ['empty form with extra property',d=>d.catalog.set(key('AcroForm'),d.context.obj({Fields:[],NeedAppearances:true})),'UNSUPPORTED_FORM'],
  ['form wrong type',d=>d.catalog.set(key('AcroForm'),d.context.obj([])),'UNSUPPORTED_FORM'],
  ['form with null Fields',d=>d.catalog.set(key('AcroForm'),d.context.obj({Fields:null})),'UNSUPPORTED_FORM'],
  ['dangling form reference',d=>d.catalog.set(key('AcroForm'),P.PDFRef.of(9999)),'INVALID'],
  ['dangling Fields reference',d=>d.catalog.set(key('AcroForm'),d.context.obj({Fields:P.PDFRef.of(9999)})),'INVALID'],
  ['XFA',d=>d.catalog.set(key('XFA'),P.PDFString.of('synthetic')),'UNSUPPORTED_FORM'],
  ['OpenAction',d=>d.catalog.set(key('OpenAction'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('synthetic')})),'UNSUPPORTED_ACTION'],
  ['additional actions',d=>d.catalog.set(key('AA'),d.context.obj({})),'UNSUPPORTED_ACTION'],
  ['JavaScript name tree',d=>d.catalog.set(key('Names'),d.context.obj({JavaScript:[]})),'UNSUPPORTED_ACTION'],
  ['embedded file name tree',d=>d.catalog.set(key('Names'),d.context.obj({EmbeddedFiles:[]})),'UNSUPPORTED_ATTACHMENT'],
  ['associated file',d=>d.catalog.set(key('AF'),d.context.obj([])),'UNSUPPORTED_ATTACHMENT'],
  ['embedded file stream',d=>d.context.register(d.context.stream('synthetic',{Type:'EmbeddedFile'})),'UNSUPPORTED_ATTACHMENT'],
  ['file specification type',d=>d.context.register(d.context.obj({Type:'Filespec'})),'UNSUPPORTED_ATTACHMENT'],
  ['certification permissions',d=>d.catalog.set(key('Perms'),d.context.obj({})),'UNSUPPORTED_SIGNATURE'],
  ['signature dictionary type',d=>d.context.register(d.context.obj({Type:'Sig'})),'UNSUPPORTED_SIGNATURE'],
  ['signature field type',d=>d.context.register(d.context.obj({FT:'Sig'})),'UNSUPPORTED_SIGNATURE'],
  ['incomplete tagged semantic tree',d=>d.catalog.set(key('StructTreeRoot'),d.context.obj({Type:'StructTreeRoot'})),'TAGGED_STRUCTURE'],
  ['unreachable script with empty annotations',d=>d.context.register(d.context.obj({S:'JavaScript',JS:P.PDFString.of('synthetic')})),'UNSUPPORTED_ACTION'],
  ['unreachable script with empty form',d=>{d.catalog.set(key('AcroForm'),d.context.obj({Fields:[]}));d.context.register(d.context.obj({JS:P.PDFString.of('synthetic')}));},'UNSUPPORTED_ACTION'],
  ['active payload in stream dictionary',d=>d.context.register(d.context.stream('synthetic',{AA:{}})),'UNSUPPORTED_ACTION']
];
for(const action of ['JavaScript','Launch','GoToR','SubmitForm','ImportData']){
  rejected.push([action+' action with indirect name',d=>d.context.register(d.context.obj({S:d.context.register(key(action))})),'UNSUPPORTED_ACTION']);
}
for(const [name,edit,code] of rejected)test('still refuses '+name,async()=>{
  const bytes=await fixture(edit),before=Buffer.from(bytes);
  await assert.rejects(inspect(bytes),new RegExp('^Error: '+code+'$'));assert.deepEqual(Buffer.from(bytes),before);
});
test('encrypted, corrupt and resource-limited files retain their separate refusals',async()=>{
  const dir=process.env.OTO_PDF_TEST_DIR;assert.ok(dir);
  await assert.rejects(inspect(new Uint8Array(fs.readFileSync(path.join(dir,'encrypted.pdf')))),/^Error: ENCRYPTED$/);
  await assert.rejects(inspect(new TextEncoder().encode('%PDF-invalid-content')),/^Error: INVALID$/);
  await assert.rejects(inspect(new Uint8Array(E.LIMITS.fileBytes+1)),/^Error: LIMIT$/);
});
const requests=[
  ['merge',{two:true},[6]],
  ['organize',{pages:[{source:0,index:2,rotation:90},{source:0,index:0,rotation:0}]},[2]],
  ['split',{ranges:'1-2;3-4'},[2,2]],
  ['remove',{ranges:'2-3'},[2]],
  ['extract',{ranges:'4,1'},[2]],
  ['rotate',{ranges:'1-4',angle:90},[4]],
  ['numbers',{options:{start:1,size:12,margin:12,position:'bottom-center',format:'number'}},[4]],
  ['watermark',{options:{text:'DRAF',size:30,opacity:18,angle:45,color:'blue'}},[4]],
  ['crop',{options:{top:5,right:5,bottom:5,left:5}},[4]]
];
for(const [tool,extra,counts] of requests)test(tool+' outputs re-enter the input gate and run through another tool',async()=>{
  const dir=process.env.OTO_PDF_TEST_DIR;assert.ok(dir);
  const a=new Uint8Array(fs.readFileSync(path.join(dir,'A.pdf'))),b=new Uint8Array(fs.readFileSync(path.join(dir,'B.pdf'))),before=Buffer.from(a);
  const result=await E.run(P,{action:'run',tool,buffers:extra.two?[a,b]:[a],name:'roundtrip-'+tool,...extra});
  assert.deepEqual(result.results.map(r=>r.count),counts);
  for(let i=0;i<result.results.length;i++){
    const output=result.results[i].bytes,original=Buffer.from(output);
    assert.deepEqual((await inspect(output)).counts,[counts[i]]);
    const again=await E.run(P,{action:'run',tool:'rotate',buffers:[output],ranges:'1',angle:90,name:'repeat'});
    assert.deepEqual((await inspect(again.results[0].bytes)).counts,[counts[i]]);
    assert.deepEqual(Buffer.from(output),original);
    fs.writeFileSync(path.join(dir,'roundtrip-'+tool+'-'+(i+1)+'.pdf'),output);
  }
  assert.deepEqual(Buffer.from(a),before);
});
test('images-pdf output re-enters gate (native bitmap/canvas codec stubbed only)',async()=>{
  const dir=process.env.OTO_PDF_TEST_DIR;assert.ok(dir);
  const jpg=new Uint8Array(fs.readFileSync(path.join(dir,'portrait.jpg'))),header=E.imageHeader(jpg);
  const oldBitmap=global.createImageBitmap,oldCanvas=global.OffscreenCanvas;let closed=0;
  global.createImageBitmap=async()=>({width:header.width,height:header.height,close(){closed++;}});
  global.OffscreenCanvas=class {
    constructor(w,h){this.width=w;this.height=h;}
    getContext(){return {fillRect(){},drawImage(){},fillStyle:''};}
    async convertToBlob(){return new Blob([jpg],{type:'image/jpeg'});}
  };
  try {
    const result=await E.run(P,{action:'run',tool:'images-pdf',buffers:[jpg],options:{layout:'auto',margin:10},name:'image-roundtrip'});
    assert.equal(closed,1);assert.deepEqual((await inspect(result.results[0].bytes)).counts,[1]);
    const numbered=await E.run(P,{action:'run',tool:'numbers',buffers:[result.results[0].bytes],options:{start:1,size:12,margin:12,position:'bottom-center',format:'number'}});
    assert.deepEqual((await inspect(numbered.results[0].bytes)).counts,[1]);
    fs.writeFileSync(path.join(dir,'roundtrip-images-pdf-1.pdf'),result.results[0].bytes);
  } finally { if(oldBitmap===undefined)delete global.createImageBitmap;else global.createImageBitmap=oldBitmap;if(oldCanvas===undefined)delete global.OffscreenCanvas;else global.OffscreenCanvas=oldCanvas; }
});
test('number -> watermark -> organize -> merge -> number remains reusable without flattening',async()=>{
  let bytes=await fixture();const before=Buffer.from(bytes);
  for(const [tool,extra] of [
    ['numbers',{options:{start:3,size:12,margin:12,position:'bottom-center',format:'number'}}],
    ['watermark',{options:{text:'DRAF',size:24,opacity:18,angle:45,color:'blue'}}],
    ['organize',{pages:[{source:0,index:0,rotation:90},{source:0,index:0,rotation:0}]}],
    ['merge',{two:true}],
    ['numbers',{options:{start:5,size:12,margin:12,position:'bottom-center',format:'number'}}]
  ]){
    const input=Buffer.from(bytes),result=await E.run(P,{action:'run',tool,buffers:extra.two?[bytes,bytes]:[bytes],...extra});
    assert.deepEqual(Buffer.from(bytes),input);bytes=result.results[0].bytes;assert.ok((await inspect(bytes)).counts[0]>0);
  }
  assert.equal((await inspect(bytes)).counts[0],4);assert.ok(before.length>0);
});
