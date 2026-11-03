'use strict';
const test=require('node:test'), assert=require('node:assert/strict'), fs=require('node:fs'), path=require('node:path'), crypto=require('node:crypto');
const E=require('../wwwroot/js/pdf-engine.js'), P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js');
const dir=process.env.OTO_PDF_TEST_DIR;
assert.ok(dir,'OTO_PDF_TEST_DIR required');
const read=n=>new Uint8Array(fs.readFileSync(path.join(dir,n+'.pdf')));
const A=read('A'),B=read('B'), initial=crypto.createHash('sha256').update(A).digest('hex');
const request=(tool,extra={})=>({action:'run',tool,buffers:[A],name:tool,...extra});
test('inspect real PDF page counts',async()=>assert.deepEqual((await E.run(P,{action:'inspect',buffers:[A,B]})).counts,[4,2]));
for(const [tool,extra,counts] of [
  ['merge',{buffers:[A,B]},[6]],
  ['organize',{buffers:[A,B],pages:[{source:1,index:1,rotation:90},{source:0,index:2,rotation:0},{source:0,index:0,rotation:180}]},[3]],
  ['split',{ranges:'1-2;3-4'},[2,2]],
  ['remove',{ranges:'2,4'},[2]],
  ['extract',{ranges:'4,1-2'},[3]],
  ['rotate',{ranges:'2-3',angle:90},[4]]
]){
 test(tool+' produces reopenable output',async()=>{
   const {results}=await E.run(P,request(tool,extra));
   assert.deepEqual(results.map(r=>r.count),counts);
   for(const r of results){assert.equal((await P.PDFDocument.load(r.bytes)).getPageCount(),r.count);fs.writeFileSync(path.join(dir,r.name),r.bytes);}
   assert.equal(crypto.createHash('sha256').update(A).digest('hex'),initial);
 });
}
for(const [label,req,error] of [
 ['encrypted',{action:'inspect',buffers:[read('encrypted')]},'ENCRYPTED'],
 // Forms stay rejected by their canonical AcroForm guard before annotation admission.
 ['interactive form',{action:'inspect',buffers:[read('interactive')]},'UNSUPPORTED_FORM'],
 ['malformed',{action:'inspect',buffers:[read('bad')]},'INVALID'],
 ['non PDF',{action:'inspect',buffers:[new TextEncoder().encode('not a PDF file')]},'INVALID'],
 ['oversized',{action:'inspect',buffers:[new Uint8Array(E.LIMITS.fileBytes+1)]},'LIMIT'],
 ['too many sources',{action:'inspect',buffers:Array(11).fill(A)},'LIMIT'],
 ['total size',{action:'inspect',buffers:Array(4).fill(new Uint8Array(E.LIMITS.fileBytes))},'LIMIT'],
 ['one merge file',request('merge'),'MERGE'],
 ['empty removal',request('remove',{ranges:'1-4'}),'EMPTY'],
 ['duplicate range',request('extract',{ranges:'1,1'}),'RANGE'],
 ['out of range',request('extract',{ranges:'9'}),'RANGE'],
 ['zero range',request('extract',{ranges:'0'}),'RANGE'],
 ['reverse range',request('extract',{ranges:'3-1'}),'RANGE'],
 ['invalid angle',request('rotate',{ranges:'1',angle:45}),'RANGE'],
 ['split overlap',request('split',{ranges:'1-3;3-4'}),'PARTITION'],
 ['multi source extract',request('extract',{buffers:[A,B],ranges:'1'}),'SINGLE'],
 ['invalid organization',request('organize',{pages:[{source:4,index:0,rotation:0}]}),'RANGE'],
 ['unknown tool',request('ocr'),'TOOL']
])test('rejects '+label,async()=>assert.rejects(E.run(P,req),new RegExp('^Error: '+error+'$')));
test('filename is bounded and safe',()=>{assert.equal(E.cleanName('../x.pdf'),'.._x.pdf');assert.equal(E.cleanName('CON'),'hasil.pdf');assert.ok(E.cleanName('a'.repeat(200)).length<=104);});
test('active action rejected',async()=>{
 const d=await P.PDFDocument.load(A);d.catalog.set(P.PDFName.of('OpenAction'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('fixture')}));await assert.rejects(E.run(P,{action:'inspect',buffers:[await d.save()]}),/UNSUPPORTED/);
});
test('excess pages rejected',async()=>{const d=await P.PDFDocument.create();for(let i=0;i<201;i++)d.addPage();await assert.rejects(E.run(P,{action:'inspect',buffers:[await d.save()]}),/LIMIT/);});
test('decompression guard fails closed before unbounded output',()=>{
 const c=P.PDFContext.create(), stream=c.flateStream(new Uint8Array(17*1024*1024));
 assert.throws(()=>P.decodePDFRawStream(stream).decode(),/OTO_LIMIT/);
});
test('object allocation guard',()=>{
 const c=P.PDFContext.create();for(let i=0;i<20000;i++)c.register(P.PDFNumber.of(i));
 assert.throws(()=>c.register(P.PDFNumber.of(1)),/OTO_LIMIT/);
});
