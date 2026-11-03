const test=require('node:test'),assert=require('node:assert/strict');
const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js'),E=require('../wwwroot/js/pdf-engine.js');
async function source(count=7){const d=await P.PDFDocument.create();for(let i=0;i<count;i++)d.addPage([240+i*10,360]).drawText('SOURCE PAGE '+(i+1));return d.save({useObjectStreams:false});}
async function run(bytes,extra){return E.run(P,{action:'run',tool:'split',buffers:[bytes],name:'parts.pdf',...extra});}
test('custom ranges support partial coverage, one output and combined output without altering pages',async()=>{
 const b=await source(6),r=await run(b,{splitMode:'custom',ranges:'1-2;3-6'});assert.deepEqual(r.results.map(x=>x.count),[2,4]);
 const one=await run(b,{splitMode:'custom',ranges:'2-4'});assert.equal(one.results.length,1);assert.equal(one.results[0].count,3);
 const joined=await run(b,{splitMode:'custom',ranges:'1-2;3-4',combineRanges:true});assert.equal(joined.results.length,1);assert.equal(joined.results[0].count,4);
});
test('fixed groups include an uneven remainder',async()=>{const r=await run(await source(),{splitMode:'fixed',pagesPerFile:3});assert.deepEqual(r.results.map(x=>x.count),[3,3,1]);});
test('extract all makes one file per page; select mode keeps order and combines on request',async()=>{
 const b=await source(6),all=await run(b,{splitMode:'extract-all'});assert.deepEqual(all.results.map(x=>x.count),[1,1,1,1,1,1]);
 const selected=await run(b,{splitMode:'select',selectedPages:[0,2,3,4]});assert.deepEqual(selected.results.map(x=>x.count),[1,1,1,1]);
 const joined=await run(b,{splitMode:'select',selectedPages:[0,2,3,4],combineSelected:true});assert.equal(joined.results.length,1);assert.equal(joined.results[0].count,4);
});
for(const [label,request,code]of [
 ['overlapping custom ranges',{splitMode:'custom',ranges:'1-3;3-4'},'PARTITION'],
 ['empty custom range',{splitMode:'custom',ranges:'1-2;'},'RANGE'],
 ['out of bounds',{splitMode:'custom',ranges:'1-7'},'RANGE'],
 ['invalid fixed size',{splitMode:'fixed',pagesPerFile:0},'RANGE'],
 ['empty selection',{splitMode:'select',selectedPages:[]},'RANGE'],
 ['duplicate selection',{splitMode:'select',selectedPages:[0,0]},'RANGE'],
 ['out of range selection',{splitMode:'select',selectedPages:[7]},'RANGE']
])test('rejects '+label,async()=>assert.rejects(run(await source(6),request),new RegExp('^Error: '+code+'$')));
