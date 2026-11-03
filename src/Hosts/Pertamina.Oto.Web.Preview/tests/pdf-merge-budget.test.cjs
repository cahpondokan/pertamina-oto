const test=require('node:test'),assert=require('node:assert/strict');
const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js'),E=require('../wwwroot/js/pdf-engine.js'),M=require('../wwwroot/js/pdf-merge-engine.js');
const key=P.PDFName.of,read=(f,a,b)=>f.bytes.subarray(a,b),file=bytes=>({bytes,size:bytes.length,name:'budget.pdf'});
async function objectFile(count){const d=await P.PDFDocument.create();d.addPage();const initial=d.context.enumerateIndirectObjects().length;for(let i=initial;i<count;i++)d.context.register(P.PDFNumber.of(i));return file(await d.save({useObjectStreams:false}));}
async function wideFile(count){const d=await P.PDFDocument.create(),p=d.addPage();p.node.set(key('BudgetFixture'),d.context.obj(Array.from({length:count},(_,i)=>i)));return file(await d.save({useObjectStreams:false}));}
async function merge(files){const chunks=[],sink={write:async b=>chunks.push(b),file:async()=>file(Buffer.concat(chunks))};const result=await M.run(P,E,{files,items:files.map(()=>({rotation:0,range:''})),options:{bookmarks:false,blank:'none'}},{read,sink});return {...result,bytes:Buffer.concat(chunks)};}
test('combined-input budget admits exactly 40,000 and rejects 40,001 while each file stays below 20,000',async()=>{
 const a=await objectFile(10000),b=await objectFile(10001);
 assert.equal((await M.load(P,E,a,read)).objects,10000);
 const accepted=await M.run(P,E,{action:'inspect',files:[a,a,a,a]},{read});assert.deepEqual(accepted.sources.map(s=>s.count),[1,1,1,1]);
 await assert.rejects(M.run(P,E,{action:'inspect',files:[a,a,a,b]},{read}),/^Error: LIMIT$/);
 await assert.rejects(merge([a,a,a,b]),/^Error: LIMIT$/);
});
test('more than 20,000 input objects can produce a valid result within the per-document guard',async()=>{
 const a=await objectFile(7000);const result=await merge([a,a,a]);assert.equal((await P.PDFDocument.load(result.bytes)).getPageCount(),3);
});
test('source object allocation and default inspection guards remain enforced',async()=>{
 const context=P.PDFContext.create();for(let i=0;i<20000;i++)context.register(P.PDFNumber.of(i));assert.throws(()=>context.register(P.PDFNumber.of(1)),/OTO_LIMIT/);
 const a=await wideFile(81000);await assert.rejects(M.load(P,E,a,read),/^Error: LIMIT$/);
 assert.equal(E.LIMITS.fileBytes,10*1024*1024);assert.equal(E.LIMITS.pages,200);
});
test('merge verifier handles the combined traversal while keeping every page value',async()=>{
 const a=await wideFile(45000),result=await merge([a,a]),out=await P.PDFDocument.load(result.bytes);
 assert.equal(out.getPageCount(),2);for(const page of out.getPages()){const values=page.node.lookup(key('BudgetFixture'),P.PDFArray);assert.equal(values.size(),45000);assert.equal(values.lookup(44999,P.PDFNumber).asNumber(),44999);}
 assert.throws(()=>E.inspectStructures(P,out),/^Error: LIMIT$/);assert.doesNotThrow(()=>E.inspectStructures(P,out,M.LIMITS.outputStructureVisits));
});
test('merged output above the bounded traversal still fails verification',async()=>{
 const a=await wideFile(60000);await assert.rejects(merge([a,a,a]),/^Error: LIMIT$/);
});
test('larger verifier scans active content beyond the old cutoff, never skips it',async()=>{
 const d=await P.PDFDocument.create(),p=d.addPage(),values=d.context.obj(Array.from({length:81000},(_,i)=>i));values.set(0,d.context.obj({S:'JavaScript',JS:P.PDFString.of('synthetic sentinel')}));p.node.set(key('BudgetFixture'),values);
 assert.throws(()=>E.inspectStructures(P,d,M.LIMITS.outputStructureVisits),/^Error: UNSUPPORTED_ACTION$/);
 for(const limit of [0,-1,1.5,NaN,Infinity,160001])assert.throws(()=>E.inspectStructures(P,d,limit),/^Error: OPTIONS$/);
});
