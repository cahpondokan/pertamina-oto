'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const {tagged,P,key}=require('./tagged-fixtures.cjs'),E=require('../wwwroot/js/pdf-engine.js'),T=require('../wwwroot/js/pdf-tags.js');
const inspect=b=>E.run(P,{action:'inspect',tool:'organize',buffers:[b]});
function snapshot(m){return [...m.nodes.values()].map(n=>({role:n.role,pages:[...n.pages],alt:m.get(n.d,'Alt')?.decodeText(),actual:m.get(n.d,'ActualText')?.decodeText(),lang:m.get(n.d,'Lang')?.decodeText()}));}
const cases=[
 ['merge',{two:true},[6]],['organize',{pages:[3,1,0].map(index=>({source:0,index,rotation:90}))},[3]],
 ['split',{ranges:'1-2;3-4'},[2,2]],['extract',{ranges:'4,1'},[2]],['remove',{ranges:'2-3'},[2]],['rotate',{ranges:'1-4',angle:90},[4]],
 ['numbers',{options:{start:1,size:12,margin:12,position:'bottom-center',format:'total'}},[4]],
 ['watermark',{options:{text:'DRAF',size:30,opacity:18,angle:45,color:'blue'}},[4]],['crop',{options:{top:5,right:5,bottom:5,left:5}},[4]]
];
for(const [tool,extra,counts] of cases)test('tagged '+tool+' preserves hierarchy/MCIDs/languages/attributes and reopens',async()=>{
 const a=await tagged(),b=await tagged({name:'B',count:2}),before=Buffer.from(a),result=await E.run(P,{tool,action:'run',buffers:extra.two?[a,b]:[a],...extra});
 assert.deepEqual(result.results.map(r=>r.count),counts);assert.deepEqual(Buffer.from(a),before);
 const dir=process.env.OTO_PDF_TEST_DIR;assert.ok(dir);
 fs.writeFileSync(path.join(dir,'tagged-A.pdf'),a);fs.writeFileSync(path.join(dir,'tagged-B.pdf'),b);
 for(let i=0;i<result.results.length;i++){
  const bytes=result.results[i].bytes;assert.deepEqual((await inspect(bytes)).counts,[counts[i]]);
  const d=await P.PDFDocument.load(bytes,{updateMetadata:false}),m=T.read(P,d),snap=snapshot(m);
  assert.equal(m.owners.reduce((n,a)=>n+a.size,0),counts[i]*(tool==='watermark'?4:3));
  assert.equal(snap.filter(n=>n.role==='Figure'&&n.alt?.includes('accessible figure')).length,counts[i]);
  assert.equal(snap.filter(n=>n.role==='P'&&n.actual?.includes('paragraph')&&n.lang==='id-ID').length,counts[i]);
  assert.equal(snap.filter(n=>n.role==='H1').length,counts[i]);assert.equal(m.ids.size,counts[i]);assert.ok(m.labels);
  if(tool==='merge'){assert.equal(new Set([...m.ids.keys()]).size,6);assert.equal(m.classMap.entries().length,2);}
  if(tool==='watermark')assert.equal(snap.filter(n=>n.role==='Note'&&n.alt==='Tanda air: DRAF').length,counts[i]);
  const again=await E.run(P,{action:'run',tool:'rotate',buffers:[bytes],ranges:'1',angle:90});assert.deepEqual((await inspect(again.results[0].bytes)).counts,[counts[i]]);
  fs.writeFileSync(path.join(dir,'tagged-'+tool+'-'+(i+1)+'.pdf'),bytes);
 }
});
test('empty referenced semantic cell survives full-document merge',async()=>{
 const a=await tagged({edit:(d,{document,docRef})=>{const empty=d.context.obj({Type:'StructElem',S:'TD',P:docRef,K:[],ActualText:P.PDFString.of('empty cell')});document.lookup(key('K'),P.PDFArray).push(d.context.register(empty));}});
 const r=await E.run(P,{tool:'merge',buffers:[a,await tagged({count:1})]});const d=await P.PDFDocument.load(r.results[0].bytes);assert.equal(snapshot(T.read(P,d)).filter(n=>n.actual==='empty cell').length,1);
});
test('table header ID and Headers references renamed together across sources',async()=>{
 const a=await tagged({edit:(d,{sections})=>{for(let i=0;i<sections.length;i++){const para=sections[i].dict.lookup(key('K'),P.PDFArray).lookup(1,P.PDFDict);para.set(key('A'),d.context.obj({O:'Table',Headers:[P.PDFString.of('heading-'+i)]}));}}});
 const r=await E.run(P,{tool:'merge',buffers:[a,a]});const d=await P.PDFDocument.load(r.results[0].bytes),m=T.read(P,d);
 for(const n of m.nodes.values())if(n.d.has(key('A'))){const a=m.get(n.d,'A');if(a.has(key('Headers')))for(const id of a.lookup(key('Headers'),P.PDFArray).asArray())assert.ok(m.ids.has(m.bytes(id)));}
});
test('nested ParentTree number tree and ID name tree are accepted',async()=>{
 const a=await tagged({edit:(d,{root})=>{for(const keyName of ['ParentTree','IDTree']){const old=root.lookup(key(keyName),P.PDFDict);root.set(key(keyName),d.context.obj({Kids:[d.context.register(old)]}));}}});assert.deepEqual((await inspect(a)).counts,[4]);
});
test('whole multi-page semantic table retained during merge',async()=>{
 const a=await tagged({spanningTable:true}),r=await E.run(P,{tool:'merge',buffers:[a,a]});assert.deepEqual((await inspect(r.results[0].bytes)).counts,[8]);
 const d=await P.PDFDocument.load(r.results[0].bytes),m=T.read(P,d);
 assert.equal([...m.nodes.values()].filter(n=>n.role==='TH').length,8);assert.equal([...m.nodes.values()].filter(n=>n.role==='TD').length,16);
 fs.writeFileSync(path.join(process.env.OTO_PDF_TEST_DIR,'tagged-table-merge.pdf'),r.results[0].bytes);
});
for(const [label,request] of [
 ['extract part of table',{tool:'extract',ranges:'1'}],['remove part of table',{tool:'remove',ranges:'2'}],
 ['split table',{tool:'split',ranges:'1-2;3-4'}],['reverse table',{tool:'organize',pages:[3,2,1,0].map(index=>({source:0,index,rotation:0}))}],
 ['duplicate tagged page',{tool:'organize',pages:[0,0].map(index=>({source:0,index,rotation:0}))}]
])test('refuses semantic-breaking edit: '+label,async()=>{await assert.rejects(E.run(P,{...request,buffers:[await tagged({spanningTable:true})]}),/^Error: TAGGED_OPERATION$/);});
test('refuses interleaving source structures',async()=>{const a=await tagged({count:2});await assert.rejects(E.run(P,{tool:'organize',buffers:[a,a],pages:[{source:0,index:0,rotation:0},{source:1,index:0,rotation:0},{source:0,index:1,rotation:0}]}),/^Error: TAGGED_OPERATION$/);});
test('mixed merge retains tags without pretending all pages are accessible',async()=>{const d=await P.PDFDocument.create();d.addPage();const r=await E.run(P,{tool:'merge',buffers:[await tagged(),await d.save()]});const out=await P.PDFDocument.load(r.results[0].bytes),m=T.read(P,out);assert.equal(out.getPageCount(),5);assert.equal(m.get(m.mark,'Marked'),P.PDFBool.False);assert.equal(m.owners.reduce((n,x)=>n+x.size,0),12);assert.equal(m.owners[4].size,0);assert.ok(r.warnings.includes('PARTIAL_TAGS'));assert.deepEqual((await inspect(r.results[0].bytes)).counts,[5]);});
test('refuses conflicting custom role meanings',async()=>{const b=await tagged({edit:(d,{root})=>root.lookup(key('RoleMap'),P.PDFDict).set(key('CustomP'),key('H2'))});await assert.rejects(E.run(P,{tool:'merge',buffers:[await tagged(),b]}),/^Error: TAGGED_CONFLICT$/);});
test('refuses removal of referenced tag even when its page was selected out',async()=>{const a=await tagged({edit:(d,{sections})=>{const heading=sections[0].dict.lookup(key('K'),P.PDFArray).get(0),p=sections[1].dict.lookup(key('K'),P.PDFArray).lookup(1,P.PDFDict);p.set(key('Ref'),d.context.obj([heading]));}});await assert.rejects(E.run(P,{tool:'extract',buffers:[a],ranges:'2'}),/^Error: TAGGED_OPERATION$/);});
const bad=[
 ['missing parent tree',(d,{root})=>root.delete(key('ParentTree')),'TAGGED_STRUCTURE'],
 ['wrong parent owner',(d,{root})=>root.lookup(key('ParentTree'),P.PDFDict).lookup(key('Nums'),P.PDFArray).lookup(1,P.PDFArray).set(0,P.PDFNull),'TAGGED_STRUCTURE'],
 ['missing parent pointer',(d,{sections})=>sections[0].dict.delete(key('P')),'TAGGED_STRUCTURE'],
 ['duplicate structure child',(d,{document,sections})=>document.lookup(key('K'),P.PDFArray).push(sections[0].ref),'TAGGED_STRUCTURE'],
 ['cyclic tag child',(d,{document,docRef})=>document.set(key('K'),docRef),'TAGGED_STRUCTURE'],
 ['foreign page ref',(d,{sections})=>sections[0].dict.set(key('Pg'),P.PDFRef.of(9999)),'INVALID'],
 ['duplicate page parent key',d=>d.getPages()[1].node.set(key('StructParents'),P.PDFNumber.of(0)),'TAGGED_STRUCTURE'],
 ['namespace',(d,{root})=>root.set(key('Namespaces'),d.context.obj([])),'UNSUPPORTED_TAGGED'],
 ['stream tagging',d=>d.context.register(d.context.stream('synthetic',{StructParents:100})),'UNSUPPORTED_TAGGED'],
 ['MCR stream reference',(d,{sections})=>sections[0].dict.lookup(key('K'),P.PDFArray).lookup(1,P.PDFDict).lookup(key('K'),P.PDFDict).set(key('Stm'),d.context.register(d.context.stream('x'))),'UNSUPPORTED_TAGGED'],
 ['class metadata tree reference',(d,{root,sections})=>root.lookup(key('ClassMap'),P.PDFDict).set(key('Bad'),sections[0].ref),'TAGGED_STRUCTURE'],
 ['unmapped role',(d,{sections})=>sections[0].dict.set(key('S'),key('UnknownRole')),'UNSUPPORTED_TAGGED'],
 ['script in tagged source',d=>d.catalog.set(key('OpenAction'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('synthetic')})),'UNSUPPORTED_ACTION'],
 ['attachment in tagged source',d=>d.catalog.set(key('AF'),d.context.obj([])),'UNSUPPORTED_ATTACHMENT'],
 ['signature in tagged source',d=>d.catalog.set(key('Perms'),d.context.obj({})),'UNSUPPORTED_SIGNATURE'],
 ['real annotation in tagged source',d=>d.getPages()[0].node.set(key('Annots'),d.context.obj([{Type:'Annot',Subtype:'Link'}])),'UNSUPPORTED_ANNOTATIONS'],
 ['form in tagged source',d=>d.catalog.set(key('AcroForm'),d.context.obj({Fields:[{FT:'Tx'}]})),'UNSUPPORTED_FORM']
];
for(const [label,edit,code] of bad)test('rejects '+label+' without changing input',async()=>{const bytes=await tagged({edit}),before=Buffer.from(bytes);await assert.rejects(inspect(bytes),new RegExp('^Error: '+code+'$'));assert.deepEqual(Buffer.from(bytes),before);});
