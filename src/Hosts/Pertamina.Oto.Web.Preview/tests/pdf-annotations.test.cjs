'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const {annotated,P,key}=require('./annotation-fixtures.cjs'),E=require('../wwwroot/js/pdf-engine.js'),A=require('../wwwroot/js/pdf-annotations.js'),T=require('../wwwroot/js/pdf-tags.js');
const inspect=b=>E.run(P,{action:'inspect',buffers:[b]});
const cases=[['merge',{two:true},4],['organize',{pages:[1,0].map(index=>({source:0,index,rotation:90}))},2],['split',{ranges:'1;2'},1],['remove',{ranges:'2'},1],['extract',{ranges:'2'},1],['rotate',{ranges:'1-2',angle:90},2],['numbers',{options:{start:1,size:12,margin:12,position:'bottom-center',format:'number'}},2],['watermark',{options:{text:'DRAF',size:30,opacity:18,angle:45,color:'blue'}},2],['crop',{options:{top:5,right:5,bottom:5,left:5}},2]];
for(const tags of [false,true])for(const [tool,extra,count] of cases)test((tags?'tagged':'plain')+' annotations survive '+tool+' and reimport',async()=>{
  const a=await annotated({tags}),before=Buffer.from(a),r=await E.run(P,{tool,buffers:extra.two?[a,a]:[a],...extra});assert.deepEqual(Buffer.from(a),before);
  fs.writeFileSync(path.join(process.env.OTO_PDF_TEST_DIR,'annot-'+(tags?'tagged':'plain')+'-source.pdf'),a);
  for(let i=0;i<r.results.length;i++){
    const bytes=r.results[i].bytes,d=await P.PDFDocument.load(bytes),m=A.read(P,d);
    assert.equal(d.getPageCount(),count);assert.equal(m.records.size,count*8);assert.deepEqual((await inspect(bytes)).counts,[count]);
    // Copier must not retain hidden source Page objects through annotation /P.
    assert.equal(d.context.enumerateIndirectObjects().filter(([,o])=>o instanceof P.PDFDict&&o.get(key('Type'))===key('Page')).length,count);
    for(const x of m.records.values()){
      assert.equal(m.resolve(x.d.get(key('P'))),d.getPages()[x.page].node);
      assert.match(x.d.lookup(key('Contents')).decodeText(),/^Synthetic /);
      if(x.dest)assert.equal(x.dest.page,x.page);
    }
    if(tags){const t=T.read(P,d);assert.equal(t.objectOwners.size,count*7);for(const [o,n] of t.objectOwners)assert.equal(t.get(n.d,'Alt').decodeText(),'Accessible '+m.records.get(o).type);}
    const again=await E.run(P,{tool:'rotate',buffers:[bytes],ranges:'1',angle:90});assert.equal(A.read(P,await P.PDFDocument.load(again.results[0].bytes)).records.size,count*8);
    fs.writeFileSync(path.join(process.env.OTO_PDF_TEST_DIR,`annot-${tags?'tagged':'plain'}-${tool}-${i}.pdf`),bytes);
  }
});
for(const tags of [false,true])for(const named of [false,true])test(`internal ${named?'named':'explicit'} links remap across reorder/merge, tags=${tags}`,async()=>{
  const a=await annotated({tags,cross:true,named});
  for(const tool of ['merge','organize']){
    const r=await E.run(P,{tool,buffers:tool==='merge'?[a,a]:[a],pages:[1,0].map(index=>({source:0,index,rotation:0}))}),d=await P.PDFDocument.load(r.results[0].bytes),m=A.read(P,d);
    for(const x of m.records.values())if(x.dest)assert.equal(x.dest.page,x.page^1);
    fs.writeFileSync(path.join(process.env.OTO_PDF_TEST_DIR,`annot-${tags?'tagged':'plain'}-${named?'named':'explicit'}-${tool}.pdf`),r.results[0].bytes);
  }
});
for(const tags of [false,true])for(const request of [{tool:'extract',ranges:'1'},{tool:'remove',ranges:'2'},{tool:'split',ranges:'1;2'}])test(`missing link target refuses ${request.tool}, tags=${tags}`,async()=>{await assert.rejects(E.run(P,{...request,buffers:[await annotated({tags,cross:true})]}),/^Error: LINK_TARGET$/);});
test('ambiguous duplication of annotated pages is refused',async()=>{await assert.rejects(E.run(P,{tool:'organize',buffers:[await annotated()],pages:[0,0].map(index=>({source:0,index,rotation:0}))}),/^Error: ANNOTATION_OPERATION$/);});
const bad=[
  ['script action',(d,s)=>s.records.find(r=>r.subtype==='Link').dict.set(key('A'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('sentinel')})),'UNSUPPORTED_ACTION'],
  ['action chain',(d,s)=>s.records.find(r=>r.subtype==='Link').dict.lookup(key('A'),P.PDFDict).set(key('Next'),d.context.obj({S:'URI',URI:P.PDFString.of('https://example.org')})),'UNSUPPORTED_ANNOTATIONS'],
  ['additional action',(d,s)=>s.records[0].dict.set(key('AA'),d.context.obj({})),'UNSUPPORTED_ACTION'],
  ['rich text',(d,s)=>s.records[0].dict.set(key('RC'),P.PDFString.of('<body>not admitted</body>')),'UNSUPPORTED_ANNOTATIONS'],
  ['unknown extension',(d,s)=>s.records[0].dict.set(key('PrivatePayload'),d.context.obj({})),'UNSUPPORTED_ANNOTATIONS'],
  ['malformed coordinates',(d,s)=>s.records[0].dict.set(key('Rect'),d.context.obj([0,0,1])),'UNSUPPORTED_ANNOTATIONS'],
  ['wrong owner page',(d,s)=>s.records[0].dict.set(key('P'),s.pages[1].ref),'UNSUPPORTED_ANNOTATIONS'],
  ['duplicate annotation',(d,s)=>s.pages[1].node.addAnnot(s.records[0].ref),'UNSUPPORTED_ANNOTATIONS'],
  ['cyclic replies',(d,s)=>s.records[0].dict.set(key('IRT'),s.records[2].ref),'UNSUPPORTED_ANNOTATIONS'],
  ['orphan popup',(d,s)=>s.records[1].dict.delete(key('Parent')),'UNSUPPORTED_ANNOTATIONS'],
  ['external AP stream',(d,s)=>s.records[3].dict.lookup(key('AP'),P.PDFDict).lookup(key('N'),P.PDFRawStream).dict.set(key('F'),P.PDFString.of('file.pdf')),'UNSUPPORTED_ACTION'],
  ['script in AP resources',(d,s)=>s.records[3].dict.lookup(key('AP'),P.PDFDict).lookup(key('N'),P.PDFRawStream).dict.set(key('AA'),d.context.obj({})),'UNSUPPORTED_ACTION'],
  ['detached URI action',d=>d.context.register(d.context.obj({S:'URI',URI:P.PDFString.of('https://example.org')})),'UNSUPPORTED_ACTION'],
  ['base URI',d=>d.catalog.set(key('URI'),d.context.obj({Base:P.PDFString.of('https://example.org')})),'UNSUPPORTED_ACTION'],
  ['unknown destination',(d,s)=>s.records[6].dict.lookup(key('A'),P.PDFDict).set(key('D'),P.PDFString.of('missing')),'LINK_TARGET'],
  ['dangling destination',(d,s)=>s.records[6].dict.lookup(key('A'),P.PDFDict).set(key('D'),d.context.obj([P.PDFRef.of(9999),'Fit'])),'INVALID'],
  ['unsafe omitted page',(d,s)=>s.records[8].dict.set(key('AA'),d.context.obj({})),'UNSUPPORTED_ACTION']
];
for(const subtype of ['Widget','FileAttachment','Sound','Movie','Screen','RichMedia','3D','Redact','Unknown'])bad.push([subtype,(d,s)=>s.records[0].dict.set(key('Subtype'),key(subtype)),'UNSUPPORTED_ANNOTATIONS']);
for(const action of ['Launch','GoToR','GoToE','Named','SetOCGState','Rendition','SubmitForm','ResetForm','Hide','ImportData','Unknown'])bad.push([action,(d,s)=>s.records[5].dict.set(key('A'),d.context.obj({S:action})),'UNSUPPORTED_ACTION']);
for(const uri of ['javascript:alert(1)','data:text/html,x','file:///C:/test.pdf','\\\\server\\file','/relative','https://user:pass@example.org','https://example.org/%0d%0aX','https://example.org/\n','https://example.org\\evil','mailto:a@example.org?body=x','tel:123'])bad.push([uri,(d,s)=>s.records[5].dict.lookup(key('A'),P.PDFDict).set(key('URI'),P.PDFHexString.fromText(uri)),'UNSAFE_LINK']);
for(const [label,edit,code] of bad)test('refuses '+label+' without mutating input',async()=>{const a=await annotated({edit}),before=Buffer.from(a);await assert.rejects(E.run(P,{tool:'extract',buffers:[a],ranges:'1'}),new RegExp('^Error: '+code+'$'));assert.deepEqual(Buffer.from(a),before);});
for(const uri of ['https://example.org/a#b','http://example.org','mailto:reader@example.org'])test('preserves safe URI '+uri,async()=>{const a=await annotated({edit:(d,s)=>s.records[5].dict.lookup(key('A'),P.PDFDict).set(key('URI'),P.PDFString.of(uri))});const r=await E.run(P,{tool:'rotate',buffers:[a],ranges:'1',angle:90}),m=A.read(P,await P.PDFDocument.load(r.results[0].bytes));assert.equal([...m.records.values()][5].action.lookup(key('URI')).decodeText(),uri);});
for(const [label,edit] of [
  ['wrong annotation parent',(d,s)=>s.records[0].dict.set(key('StructParent'),P.PDFNumber.of(0))],
  ['missing object parent',(d,s)=>s.records[0].dict.delete(key('StructParent'))],
  ['wrong OBJR owner',(d,s)=>{const t=T.read(P,d),n=t.objectOwners.get(s.records[0].dict);n.d.lookup(key('K'),P.PDFDict).set(key('Pg'),s.pages[1].ref);}]
])test('tagged annotation rejects '+label,async()=>{await assert.rejects(inspect(await annotated({tags:true,edit})),/^Error: TAGGED_STRUCTURE$/);});
test('annotation limit is enforced',async()=>{const d=await P.PDFDocument.create(),p=d.addPage();for(let i=0;i<2001;i++)p.node.addAnnot(d.context.register(d.context.obj({Type:'Annot',Subtype:'Text',Rect:[0,0,10,10]})));await assert.rejects(inspect(await d.save({useObjectStreams:false})),/^Error: LIMIT$/);});

for(const [type,fields] of Object.entries({
 Text:{Name:'Note'},Link:{Dest:'self'},FreeText:{DA:P.PDFString.of('/Helv 10 Tf 0 g')},
 Line:{L:[10,10,30,30],LE:['None','OpenArrow']},Square:{IC:[1,0,0],BS:{W:1,S:'D',D:[2,2]}},Circle:{BE:{S:'C',I:1}},
 Polygon:{Vertices:[10,10,20,30,30,10]},PolyLine:{Vertices:[10,10,20,30,30,10],LE:['None','ClosedArrow']},
 Highlight:{QuadPoints:[10,30,30,30,10,10,30,10]},Underline:{QuadPoints:[10,30,30,30,10,10,30,10]},
 Squiggly:{QuadPoints:[10,30,30,30,10,10,30,10]},StrikeOut:{QuadPoints:[10,30,30,30,10,10,30,10]},
 Stamp:{Name:'Approved'},Caret:{Sy:'P',RD:[0,0,0,0]},Ink:{InkList:[[10,10,20,20,30,10]]}
}))test(type+' supported static dictionary survives exact semantic copy',async()=>{
 const d=await P.PDFDocument.create(),p=d.addPage(),props={...fields};if(props.Dest==='self')props.Dest=d.context.obj([p.ref,'Fit']);
 const annotation=d.context.obj({Type:'Annot',Subtype:type,Rect:[10,10,30,30],Contents:P.PDFHexString.fromText('Catatan (aman) \\ Unicode ✓'),...props});p.node.addAnnot(d.context.register(annotation));
 const a=await d.save({useObjectStreams:false}),r=await E.run(P,{tool:'rotate',buffers:[a],ranges:'1',angle:90}),m=A.read(P,await P.PDFDocument.load(r.results[0].bytes)),out=[...m.records.values()][0].d;
 assert.equal(out.lookup(key('Contents')).decodeText(),'Catatan (aman) \\ Unicode ✓');assert.equal(out.get(key('Subtype')).decodeText(),type);
 for(const [k,v] of annotation.entries())if(!['P','Dest'].includes(k.decodeText()))assert.equal(out.get(k).toString(),v.toString());
});
test('legacy named destination becomes equivalent explicit destination',async()=>{
 const a=await annotated({edit:(d,s)=>{d.catalog.set(key('Dests'),d.context.obj({Target:d.context.obj([s.pages[1].ref,'FitH',null])}));s.records[6].dict.delete(key('A'));s.records[6].dict.set(key('Dest'),key('Target'));}});
 const r=await E.run(P,{tool:'organize',buffers:[a],pages:[1,0].map(index=>({source:0,index,rotation:0}))}),m=A.read(P,await P.PDFDocument.load(r.results[0].bytes));assert.equal(m.byPage[1][6].dest.page,0);assert.equal(m.byPage[1][6].dest.tail[0].decodeText(),'FitH');
});
