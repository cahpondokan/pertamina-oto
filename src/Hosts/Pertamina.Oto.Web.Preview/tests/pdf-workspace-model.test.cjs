'use strict';
const test=require('node:test'),assert=require('node:assert/strict');
const M=require('../wwwroot/js/pdf-workspace-model.js');
const E=require('../wwwroot/js/pdf-engine.js');
const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js');
const sources=[{id:1,label:'A',name:'a.pdf',count:2,bytes:new Uint8Array([1])},{id:2,label:'B',name:'b.pdf',count:1,bytes:new Uint8Array([2])}];
const pages=[{id:1,sourceId:1,index:0,rotation:0},{id:2,sourceId:1,index:1,rotation:90},{id:3,sourceId:2,index:0,rotation:0}];
test('source labels remain unique past Z',()=>assert.deepEqual([1,2,26,27,28,52,53].map(M.sourceLabel),['A','B','Z','AA','AB','AZ','BA']));
test('move returns new order without mutating source records',()=>{
  const moved=M.move(sources,2,-1);assert.deepEqual(moved.map(s=>s.label),['B','A']);
  assert.deepEqual(sources.map(s=>s.label),['A','B']);assert.equal(moved[0],sources[1]);
});
test('invalid moves do not lose or duplicate items',()=>{
  for(const [id,pos] of [[99,0],[1,-1],[1,99],[1,0.5],[1,0]])assert.deepEqual(M.moveTo(sources,id,pos),sources);
});
test('merge renders one card per source, not one per page',()=>{
  const cards=M.cards('merge',sources,pages);assert.equal(cards.length,2);assert.deepEqual(cards.map(c=>c.count),[2,1]);assert.deepEqual(cards.map(c=>c.identity),['A','B']);
});
test('page move keeps original identity separate from result position',()=>{
  const cards=M.cards('organize',sources,M.moveTo(pages,2,0));
  assert.equal(cards[0].identity,'A · 2');assert.equal(cards[0].position,0);assert.equal(cards[0].page.rotation,90);
  assert.equal(cards[1].identity,'A · 1');
});
test('duplicated page keeps source identity but gets its own action target',()=>{
  const duplicate={...pages[1],id:4};const cards=M.cards('organize',sources,[pages[1],duplicate]);
  assert.deepEqual(cards.map(c=>c.identity),['A · 2','A · 2']);assert.deepEqual(cards.map(c=>c.id),[2,4]);
});
test('source removal never silently remaps an orphaned page',()=>{
  assert.throws(()=>M.cards('organize',[sources[0]],pages),/INVALID/);
  assert.throws(()=>M.input('organize',[sources[0]],pages),/INVALID/);
});
test('merge reordering changes request buffer order',()=>{
  const request=M.input('merge',M.move(sources,2,-1),pages);
  assert.equal(request.buffers[0],sources[1].bytes);assert.equal(request.buffers[1],sources[0].bytes);
});
test('organizer requests remap source indices without changing source page numbers',()=>{
  const request=M.input('organize',[sources[1],sources[0]],M.moveTo(pages,2,0));
  assert.deepEqual(request.pages,[{source:1,index:1,rotation:90},{source:1,index:0,rotation:0},{source:0,index:0,rotation:0}]);
});
test('image buffers follow card order, not source list order',()=>{
  const request=M.input('images-pdf',sources,[pages[2],pages[0]]);
  assert.equal(request.buffers[0],sources[1].bytes);assert.equal(request.buffers[1],sources[0].bytes);
});
test('states cover empty, editing, processing, result and return to editor',()=>{
  const base={busy:false,showResult:false,resultCount:0,sourceCount:0};
  assert.equal(M.state(base),'empty');assert.equal(M.state({...base,sourceCount:2}),'editing');
  assert.equal(M.state({...base,busy:true}),'processing');
  assert.equal(M.state({...base,sourceCount:2,showResult:true,resultCount:1}),'result');
  assert.equal(M.state({...base,sourceCount:2,showResult:false,resultCount:1}),'editing');
  assert.equal(M.state({...base,showResult:true}),'empty');
});
test('readiness enforces document and minimum merge/split requirements',()=>{
  assert.equal(M.readiness('merge',[],[]).ready,false);
  assert.equal(M.readiness('merge',[sources[0]],pages.slice(0,2)).ready,false);
  assert.equal(M.readiness('merge',sources,pages).ready,true);
  assert.equal(M.readiness('organize',sources,[]).ready,false);
  assert.equal(M.readiness('split',[sources[1]],[pages[2]]).ready,true);
  assert.equal(M.readiness('split',[sources[0]],pages.slice(0,2)).ready,true);
});
test('real merged PDF follows reordered source cards and originals stay untouched',async()=>{
  const a=await P.PDFDocument.create();a.addPage([300,400]);a.addPage([310,410]);
  const b=await P.PDFDocument.create();b.addPage([500,600]);
  const aa=await a.save({useObjectStreams:false}),bb=await b.save({useObjectStreams:false});
  const originalA=Buffer.from(aa),originalB=Buffer.from(bb);
  const actual=[{...sources[0],bytes:aa},{...sources[1],bytes:bb}];
  const request=M.input('merge',M.move(actual,2,-1),pages);
  const result=await E.run(P,{action:'run',tool:'merge',...request,name:'reordered.pdf'});
  const pdf=await P.PDFDocument.load(result.results[0].bytes);
  assert.deepEqual(pdf.getPages().map(p=>p.getWidth()),[500,300,310]);
  assert.deepEqual(Buffer.from(aa),originalA);assert.deepEqual(Buffer.from(bb),originalB);
});
test('real organized PDF follows page moves, rotation and duplicate identity',async()=>{
  const a=await P.PDFDocument.create();a.addPage([300,400]);a.addPage([310,410]);
  const b=await P.PDFDocument.create();b.addPage([500,600]);
  const actual=[{...sources[0],bytes:await a.save()},{...sources[1],bytes:await b.save()}];
  const chosen=[pages[1],pages[2],{...pages[1],id:4}];
  const result=await E.run(P,{action:'run',tool:'organize',...M.input('organize',actual,chosen),name:'organized.pdf'});
  const pdf=await P.PDFDocument.load(result.results[0].bytes);
  assert.deepEqual(pdf.getPages().map(p=>[p.getWidth(),p.getRotation().angle]),[[310,90],[500,0],[310,90]]);
});
