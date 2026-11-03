'use strict';
const test=require('node:test'),assert=require('node:assert/strict');
const {P,key,tagged}=require('./tagged-fixtures.cjs'),E=require('../wwwroot/js/pdf-engine.js'),M=require('../wwwroot/js/pdf-merge-engine.js'),T=require('../wwwroot/js/pdf-tags.js');
async function fixture(edit=()=>{}){const d=await P.PDFDocument.create();d.addPage().drawText('NAVIGATION FIXTURE');d.addPage();edit(d);return d.save({useObjectStreams:false});}
function outline(d,action={S:'GoTo',D:[d.getPage(0).ref,'Fit']}){const c=d.context,r=c.obj({Type:'Outlines'}),ref=c.register(r),item=c.obj({Title:P.PDFString.of('Chapter'),Parent:ref,A:action}),itemRef=c.register(item);r.set(key('First'),itemRef);r.set(key('Last'),itemRef);d.catalog.set(key('Outlines'),ref);return {r,ref,item,itemRef};}
async function inspect(b){return E.run(P,{action:'inspect',buffers:[b]});}
async function merge(b){const chunks=[],f={size:b.length,bytes:b},sink={write:async v=>chunks.push(v),file:async()=>{const bytes=Buffer.concat(chunks);return {size:bytes.length,bytes};}};const r=await M.run(P,E,{files:[f,f],items:[{range:'',rotation:0},{range:'',rotation:0}],options:{bookmarks:false},name:'navigation.pdf'},{read:(f,a,b)=>f.bytes.slice(a,b),sink});return {...r,bytes:Buffer.concat(chunks)};}
for(const [mode,params]of [['Fit',[]],['FitB',[]],['FitH',[null]],['FitBH',[42]],['FitV',[0]],['FitBV',[null]],['XYZ',[null,null,1]],['FitR',[0,0,200,300]]])test('opening destination '+mode+' is admitted, never executed or copied as an opening action',async()=>{
 const b=await fixture(d=>d.catalog.set(key('OpenAction'),d.context.obj([d.getPage(0).ref,mode,...params]))),before=Buffer.from(b);
 assert.ok((await inspect(b)).warnings.includes('SOURCE_NAVIGATION'));
 const r=await merge(b),out=await P.PDFDocument.load(r.bytes);assert.equal(out.getPageCount(),4);assert.equal(out.catalog.has(key('OpenAction')),false);assert.ok(r.warnings.includes('SOURCE_NAVIGATION'));assert.deepEqual(Buffer.from(b),before);
});
for(const name of ['direct','indirect','legacy name','name tree'])test('owned internal GoTo '+name+' and bookmark navigation are accepted',async()=>{
 const b=await fixture(d=>{let dest=d.context.obj([d.getPage(1).ref,'Fit']);if(name==='legacy name'){d.catalog.set(key('Dests'),d.context.obj({target:dest}));dest=key('target');}if(name==='name tree'){d.catalog.set(key('Names'),d.context.obj({Dests:{Names:[P.PDFString.of('target'),dest]}}));dest=P.PDFString.of('target');}let a=d.context.obj({Type:'Action',S:'GoTo',D:dest});if(name==='indirect')a=d.context.register(a);d.catalog.set(key('OpenAction'),a);outline(d);});
 assert.deepEqual((await inspect(b)).counts,[2]);const r=await E.run(P,{tool:'rotate',buffers:[b],ranges:'1',angle:90});const out=await P.PDFDocument.load(r.results[0].bytes);assert.equal(out.catalog.has(key('OpenAction')),false);assert.equal(out.catalog.has(key('Outlines')),false);assert.ok(r.warnings.includes('SOURCE_NAVIGATION'));
});
const unsafe=[
 ['script',d=>d.catalog.set(key('OpenAction'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('sentinel')}))],
 ['external action',d=>d.catalog.set(key('OpenAction'),d.context.obj({S:'GoToR',D:[0,'Fit'],F:P.PDFString.of('elsewhere.pdf')}))],
 ['URI opening',d=>d.catalog.set(key('OpenAction'),d.context.obj({S:'URI',URI:P.PDFString.of('https://example.invalid')}))],
 ['action chain',d=>d.catalog.set(key('OpenAction'),d.context.obj({S:'GoTo',D:[d.getPage(0).ref,'Fit'],Next:{S:'GoTo',D:[d.getPage(1).ref,'Fit']}}))],
 ['foreign OpenAction',d=>d.context.register(d.context.obj({OpenAction:[d.getPage(0).ref,'Fit']}))],
 ['unowned GoTo',d=>d.context.register(d.context.obj({S:'GoTo',D:[d.getPage(0).ref,'Fit']}))],
 ['missing page',d=>d.catalog.set(key('OpenAction'),d.context.obj([P.PDFRef.of(99999),'Fit']))],
 ['integer page target',d=>d.catalog.set(key('OpenAction'),d.context.obj([0,'Fit']))],
 ['extra destination operand',d=>d.catalog.set(key('OpenAction'),d.context.obj([d.getPage(0).ref,'Fit',0]))],
 ['bookmark script',d=>outline(d,{S:'JavaScript',JS:P.PDFString.of('sentinel')})],
 ['bookmark cycle',d=>{const x=outline(d);x.item.set(key('Next'),x.itemRef);} ],
 ['bookmark wrong parent',d=>{const x=outline(d);x.item.set(key('Parent'),x.itemRef);} ],
 ['bookmark action and destination',d=>{const x=outline(d);x.item.set(key('Dest'),d.context.obj([d.getPage(0).ref,'Fit']));}],
 ['bookmark missing last',d=>{const x=outline(d);x.r.delete(key('Last'));}]
];
for(const [label,edit]of unsafe)test('navigation still refuses '+label,async()=>{const b=await fixture(edit);await assert.rejects(inspect(b),/^Error: (UNSUPPORTED_ACTION|INVALID|LINK_TARGET)$/);await assert.rejects(merge(b),/^Error: (UNSUPPORTED_ACTION|INVALID|LINK_TARGET)$/);});
test('deep bookmark nesting is bounded before any copy',async()=>{
 const b=await fixture(d=>{const x=outline(d);let parent=x.item,ref=x.itemRef;for(let i=0;i<34;i++){const child=d.context.obj({Title:P.PDFString.of('nested'),Parent:ref,Dest:[d.getPage(0).ref,'Fit']}),next=d.context.register(child);parent.set(key('First'),next);parent.set(key('Last'),next);parent=child;ref=next;}});
 await assert.rejects(inspect(b),/^Error: LIMIT$/);await assert.rejects(merge(b),/^Error: LIMIT$/);
});
for(const kind of ['absent MarkInfo','absent Marked','absent StructElem Type'])test(kind+' follows PDF defaults while preserving all semantic tags',async()=>{
 const b=await tagged({edit:d=>{if(kind==='absent MarkInfo')d.catalog.delete(key('MarkInfo'));else if(kind==='absent Marked')d.catalog.lookup(key('MarkInfo'),P.PDFDict).delete(key('Marked'));else for(const [,o]of d.context.enumerateIndirectObjects())if(o instanceof P.PDFDict&&o.get(key('Type'))===key('StructElem'))o.delete(key('Type'));}});
 const source=T.read(P,await P.PDFDocument.load(b)),r=await merge(b),out=T.read(P,await P.PDFDocument.load(r.bytes));assert.equal(out.owners.reduce((n,m)=>n+m.size,0),24);assert.equal(out.ids.size,8);assert.ok(out.nodes.size>=source.nodes.size*2);if(kind!=='absent StructElem Type'){assert.equal(out.get(out.mark,'Marked'),P.PDFBool.False);assert.ok(r.warnings.includes('PARTIAL_TAGS'));}
});
async function metadataFixture(edit=()=>{}){return tagged({edit:(d,x)=>{d.catalog.delete(key('MarkInfo'));for(const [,o]of d.context.enumerateIndirectObjects())if(o instanceof P.PDFDict&&o.get(key('Type'))===key('StructElem'))o.delete(key('Type'));const node=x.sections[0].dict;node.set(key('Info'),d.context.obj({Creator:P.PDFString.of('synthetic producer')}));node.set(key('Metadata'),d.context.register(d.context.stream('<x:xmpmeta xmlns:x="adobe:ns:meta/">synthetic</x:xmpmeta>',{Type:'Metadata',Subtype:'XML'})));outline(d);d.catalog.set(key('OpenAction'),d.context.obj({S:'GoTo',D:[d.getPage(0).ref,'FitBH',0]}));edit(d,node);}});}
test('combined scan metadata is preserved byte-for-byte with tags and no automatic action',async()=>{
 const b=await metadataFixture(),before=Buffer.from(b),r=await merge(b),out=await P.PDFDocument.load(r.bytes),model=T.read(P,out);
 const nodes=[...model.nodes.values()].filter(n=>n.d.has(key('Metadata')));assert.equal(nodes.length,2);for(const n of nodes){assert.equal(model.get(n.d,'Info').lookup(key('Creator'),P.PDFString).decodeText(),'synthetic producer');assert.equal(Buffer.from(model.get(n.d,'Metadata').getContents()).toString(),'<x:xmpmeta xmlns:x="adobe:ns:meta/">synthetic</x:xmpmeta>');}
 assert.equal(out.getPageCount(),8);assert.equal(out.catalog.has(key('OpenAction')),false);assert.deepEqual(Buffer.from(b),before);assert.ok(r.warnings.includes('PARTIAL_TAGS'));assert.ok(r.warnings.includes('SOURCE_NAVIGATION'));
});
for(const [label,edit]of [
 ['invalid MarkInfo',d=>d.catalog.set(key('MarkInfo'),P.PDFNumber.of(1))],
 ['invalid Marked',d=>d.catalog.set(key('MarkInfo'),d.context.obj({Marked:1}))],
 ['wrong Type',(_d,n)=>n.set(key('Type'),key('Page'))],
 ['Info page reference',(d,n)=>n.set(key('Info'),d.context.obj({Creator:d.getPage(0).ref}))],
 ['Info extra action',(d,n)=>n.set(key('Info'),d.context.obj({JS:P.PDFString.of('sentinel')}))],
 ['non metadata stream',(d,n)=>n.set(key('Metadata'),d.context.register(d.context.stream('x',{Type:'XObject',Subtype:'Form'})))],
 ['external stream',(d,n)=>n.set(key('Metadata'),d.context.register(d.context.stream('x',{Type:'Metadata',Subtype:'XML',F:P.PDFString.of('external')})))],
 ['oversized metadata',(d,n)=>n.set(key('Metadata'),d.context.register(d.context.stream(new Uint8Array(1024*1024+1),{Type:'Metadata',Subtype:'XML'})))],
 ['missing required parent',(_d,n)=>n.delete(key('P'))],
 ['wrong reverse-index owner',(d)=>{const tree=d.catalog.lookup(key('StructTreeRoot'),P.PDFDict);tree.set(key('ParentTree'),d.context.obj({Nums:[]}));}]
])test('combined scan still rejects '+label,async()=>{const b=await metadataFixture(edit);await assert.rejects(merge(b),/^Error: (TAGGED_STRUCTURE|UNSUPPORTED_TAGGED|UNSUPPORTED_ACTION|LIMIT)$/);});
