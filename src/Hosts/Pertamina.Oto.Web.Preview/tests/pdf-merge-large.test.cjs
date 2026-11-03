'use strict';
const test=require('node:test'),assert=require('node:assert/strict');
const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js'),E=require('../wwwroot/js/pdf-engine.js'),M=require('../wwwroot/js/pdf-merge-engine.js');
const file=(bytes,name='sample.pdf')=>({bytes,size:bytes.length,name});
const read=(f,a,b)=>f.bytes.slice(a,b);
async function sample(count,compressed=false){const d=await P.PDFDocument.create();for(let i=0;i<count;i++){const p=d.addPage();p.drawText('PAGE '+(i+1));}return file(await d.save({useObjectStreams:compressed}));}
async function merge(files,items=files.map(()=>({range:'',rotation:0})),options={bookmarks:false,blank:'none'}){const chunks=[];const sink={write:async b=>chunks.push(b),file:async()=>file(Buffer.concat(chunks))};const result=await M.run(P,E,{files,items,options,name:'result.pdf'}, {read,sink});return {...result,bytes:(await sink.file()).bytes};}
test('decimal 4 GB / 500 boundary is metadata admission only',()=>{
 assert.equal(M.admission([{size:3999999992},{size:8}],2),4000000000);
 assert.throws(()=>M.admission([{size:3999999993},{size:8}]),/TOTAL_BYTES/);
 assert.equal(M.admission(Array(500).fill({size:8})),4000);
 assert.throws(()=>M.admission(Array(501).fill({size:8})),/FILE_COUNT/);
 assert.equal(E.LIMITS.totalBytes,30*1024*1024);
});
test('streamed output, ranges, rotation, bookmarks and blanks reopen',async()=>{
 const a=await sample(4),b=await sample(2,true);
 const result=await merge([a,b],[{range:'3,1',rotation:90},{range:'2',rotation:180}],{bookmarks:true,blank:'odd'});
 const d=await P.PDFDocument.load(result.bytes);assert.equal(d.getPageCount(),4);assert.equal(d.getPage(0).getRotation().angle,90);assert.equal(d.getPage(2).getRotation().angle,180);assert.ok(d.catalog.has(P.PDFName.of('Outlines')));
});
test('558 + 712 actual pages',async()=>{const result=await merge([await sample(558),await sample(712)]);assert.equal((await P.PDFDocument.load(result.bytes)).getPageCount(),1270);});
test('500 actual PDF inputs',async()=>{const f=await sample(1);const result=await merge(Array(500).fill(f));assert.equal((await P.PDFDocument.load(result.bytes)).getPageCount(),500);});
test('range negatives and extended page numbers',()=>{assert.deepEqual(M.range('10000',10000),[9999]);for(const s of ['0','2-1','1,1','1-3','abc'])assert.throws(()=>M.range(s,2),/RANGE/);});
test('writer errors are propagated without success',async()=>{await assert.rejects(M.run(P,E,{files:[await sample(1),await sample(1)],items:[{rotation:0},{rotation:0}],options:{bookmarks:false}}, {read,sink:{write:async()=>{throw Error('DISK_FULL');}}}),/DISK_FULL/);});
test('non PDF rejected before parsing',async()=>{await assert.rejects(M.load(P,E,file(Buffer.from('not a pdf file')),read),/INVALID/);});
module.exports={file,read,merge,sample};
