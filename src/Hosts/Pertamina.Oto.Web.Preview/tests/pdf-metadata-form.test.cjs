'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const {tagged,P,key}=require('./tagged-fixtures.cjs'),E=require('../wwwroot/js/pdf-engine.js'),T=require('../wwwroot/js/pdf-tags.js');
const inspect=b=>E.run(P,{action:'inspect',tool:'organize',buffers:[b]});
function form(d){const c=d.context,encoding=c.register(c.obj({Type:'Encoding',Differences:[24,'breve','caron',32,'space']}));
 return c.obj({Fields:c.register(c.obj([])),DA:P.PDFString.of('/Helv 0 Tf 0 g'),DR:{Font:{Helv:c.register(c.obj({Type:'Font',Subtype:'Type1',BaseFont:'Helvetica',Name:'Helv',Encoding:encoding})),ZaDb:{Type:'Font',Subtype:'Type1',BaseFont:'ZapfDingbats'}},Encoding:{PDFDocEncoding:encoding}}});}
async function fixture(edit=()=>{}){const d=await P.PDFDocument.create();d.addPage([300,400]).drawText('Synthetic empty form fixture',{x:20,y:350,size:12});d.addPage([300,400]);const f=form(d);d.catalog.set(key('AcroForm'),d.context.register(f));await edit(d,f);return d.save({useObjectStreams:false});}
function canonical(d,v){v=d.context.lookup(v);if(v instanceof P.PDFDict)return v.entries().sort((a,b)=>a[0].toString().localeCompare(b[0].toString())).map(([k,x])=>[k.toString(),canonical(d,x)]);if(v instanceof P.PDFArray)return v.asArray().map(x=>canonical(d,x));return v.toString();}
const operations=[['merge',{two:true}],['organize',{pages:[{source:0,index:1,rotation:0},{source:0,index:0,rotation:0}]}],['split',{ranges:'1;2'}],['extract',{ranges:'2'}],['remove',{ranges:'1'}],['rotate',{ranges:'1',angle:90}],['numbers',{options:{start:1,size:12,margin:12,position:'bottom-center',format:'number'}}],['watermark',{options:{text:'DRAF',size:30,opacity:18,angle:45,color:'blue'}}],['crop',{options:{top:5,right:5,bottom:5,left:5}}]];
for(const [tool,options] of operations)test('empty form defaults survive '+tool+' and reimport',async()=>{
 const a=await fixture(),before=Buffer.from(a),d=await P.PDFDocument.load(a),expected=canonical(d,d.catalog.get(key('AcroForm')));
 const r=await E.run(P,{tool,buffers:options.two?[a,a]:[a],...options});
 for(let i=0;i<r.results.length;i++){const bytes=r.results[i].bytes;await inspect(bytes);const out=await P.PDFDocument.load(bytes);assert.deepEqual(canonical(out,out.catalog.get(key('AcroForm'))),expected);const again=await E.run(P,{tool:'rotate',buffers:[bytes],ranges:'1',angle:90});await inspect(again.results[0].bytes);
 fs.writeFileSync(path.join(process.env.OTO_PDF_TEST_DIR,'empty-form-'+tool+'-'+i+'.pdf'),bytes);}
 assert.deepEqual(Buffer.from(a),before);
});
test('merging full defaults with minimal or absent forms retains full defaults',async()=>{const a=await fixture(),b=await fixture((d,f)=>{f.delete(key('DA'));f.delete(key('DR'));}),c=await fixture(d=>d.catalog.delete(key('AcroForm')));for(const buffers of [[a,b,c],[c,b,a]]){const r=await E.run(P,{tool:'merge',buffers}),out=await P.PDFDocument.load(r.results[0].bytes);assert.equal(out.catalog.lookup(key('AcroForm'),P.PDFDict).lookup(key('DA'),P.PDFString).decodeText(),'/Helv 0 Tf 0 g');}});
test('conflicting empty form defaults are refused rather than discarded',async()=>{const a=await fixture(),b=await fixture((d,f)=>f.set(key('DA'),P.PDFString.of('/Helv 12 Tf 0 g')));await assert.rejects(E.run(P,{tool:'merge',buffers:[a,b]}),/^Error: UNSUPPORTED_FORM$/);});
test('tagged metadata plus empty form preserve tags, IDs, Alt and defaults together',async()=>{const a=await tagged({edit:(d,{root})=>{d.catalog.set(key('AcroForm'),form(d));d.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Type'),key('MarkInfo'));root.lookup(key('ParentTree'),P.PDFDict).set(key('Type'),key('ParentTree'));}});const r=await E.run(P,{tool:'merge',buffers:[a,a]});const out=await P.PDFDocument.load(r.results[0].bytes),model=T.read(P,out);assert.equal(model.ids.size,8);assert.equal([...model.nodes.values()].filter(n=>model.get(n.d,'Alt')).length,8);assert.equal(model.mark.lookup(key('Type'),P.PDFName).decodeText(),'MarkInfo');assert.equal(model.get(model.tree,'ParentTree').lookup(key('Type'),P.PDFName).decodeText(),'ParentTree');assert.ok(out.catalog.has(key('AcroForm')));await inspect(r.results[0].bytes);fs.writeFileSync(path.join(process.env.OTO_PDF_TEST_DIR,'metadata-tagged-form-merge.pdf'),r.results[0].bytes);});
for(const [label,edit] of [
 ['direct Type',d=>d.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Type'),key('MarkInfo'))],
 ['indirect Type',d=>d.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Type'),d.context.register(key('MarkInfo')))],
 ['typed parent tree',(d,{root})=>root.lookup(key('ParentTree'),P.PDFDict).set(key('Type'),key('ParentTree'))],
 ['nested typed parent tree',(d,{root})=>{const old=root.lookup(key('ParentTree'),P.PDFDict);old.set(key('Type'),key('ParentTree'));root.set(key('ParentTree'),d.context.obj({Type:'ParentTree',Kids:[d.context.register(old)]}));}],
 ['boolean flags',d=>{const m=d.catalog.lookup(key('MarkInfo'),P.PDFDict);m.set(key('Type'),key('MarkInfo'));m.set(key('Suspects'),P.PDFBool.True);m.set(key('UserProperties'),P.PDFBool.True);}]
])test('accepts bounded metadata: '+label,async()=>{const a=await tagged({edit}),before=Buffer.from(a);await inspect(a);const r=await E.run(P,{tool:'rotate',buffers:[a],ranges:'1',angle:90});await inspect(r.results[0].bytes);assert.deepEqual(Buffer.from(a),before);});
const badMetadata=[
 ['wrong discriminator',d=>d.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Type'),key('OtherType')),'TAGGED_STRUCTURE'],
 ['string discriminator',d=>d.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Type'),P.PDFString.of('MarkInfo')),'TAGGED_STRUCTURE'],
 ['dictionary discriminator',d=>d.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Type'),d.context.obj({})),'TAGGED_STRUCTURE'],
 ['unknown flag',d=>d.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Anything'),P.PDFBool.False),'UNSUPPORTED_TAGGED'],
 ['numeric flag',d=>d.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Suspects'),P.PDFNumber.of(0)),'TAGGED_STRUCTURE'],
 ['Marked wrong type with tree',d=>d.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Marked'),P.PDFString.of('false')),'TAGGED_STRUCTURE'],
 ['wrong parent discriminator',(d,{root})=>root.lookup(key('ParentTree'),P.PDFDict).set(key('Type'),key('OtherType')),'TAGGED_STRUCTURE'],
 ['string parent discriminator',(d,{root})=>root.lookup(key('ParentTree'),P.PDFDict).set(key('Type'),P.PDFString.of('ParentTree')),'TAGGED_STRUCTURE'],
 ['corrupt parent with valid discriminator',(d,{root})=>{const p=root.lookup(key('ParentTree'),P.PDFDict);p.set(key('Type'),key('ParentTree'));p.lookup(key('Nums'),P.PDFArray).lookup(1,P.PDFArray).set(0,P.PDFNull);},'TAGGED_STRUCTURE']
];
for(const [label,edit,code] of badMetadata)test('metadata still fails closed: '+label,async()=>{const a=await tagged({edit});await assert.rejects(inspect(a),new RegExp('^Error: '+code+'$'));});
const badForms=[
 ['real field',(d,f)=>f.lookup(key('Fields'),P.PDFArray).push(d.context.obj({FT:'Tx'}))],
 ['XFA',(d,f)=>f.set(key('XFA'),P.PDFString.of('fixture'))],
 ['signature flag',(d,f)=>f.set(key('SigFlags'),P.PDFNumber.of(1))],
 ['NeedAppearances',(d,f)=>f.set(key('NeedAppearances'),P.PDFBool.True)],
 ['calculation order',(d,f)=>f.set(key('CO'),d.context.obj([]))],
 ['missing Fields',(d,f)=>f.delete(key('Fields'))],
 ['null Fields',(d,f)=>f.set(key('Fields'),P.PDFNull)],
 ['wrong resource type',(d,f)=>f.set(key('DR'),P.PDFString.of(''))],
 ['unknown resource category',(d,f)=>f.lookup(key('DR'),P.PDFDict).set(key('XObject'),d.context.obj({}))],
 ['resource containing page',(d,f)=>f.lookup(key('DR'),P.PDFDict).lookup(key('Font'),P.PDFDict).set(key('Helv'),d.getPages()[0].ref)],
 ['nonstandard font',(d,f)=>f.lookup(key('DR'),P.PDFDict).lookup(key('Font'),P.PDFDict).lookup(key('Helv'),P.PDFDict).set(key('BaseFont'),key('UnknownFont'))],
 ['stream resource',(d,f)=>f.lookup(key('DR'),P.PDFDict).lookup(key('Font'),P.PDFDict).set(key('Helv'),d.context.register(d.context.stream('fixture')))],
 ['encoding cycle',(d,f)=>{const e=f.lookup(key('DR'),P.PDFDict).lookup(key('Encoding'),P.PDFDict).lookup(key('PDFDocEncoding'),P.PDFDict);e.set(key('BaseEncoding'),d.context.register(e));}],
 ['bad difference range',(d,f)=>{const e=f.lookup(key('DR'),P.PDFDict).lookup(key('Encoding'),P.PDFDict).lookup(key('PDFDocEncoding'),P.PDFDict);e.set(key('Differences'),d.context.obj([256,'a']));}],
 ['wrong DA type',(d,f)=>f.set(key('DA'),d.context.obj({}))],
 ['unresolved font DA',(d,f)=>f.set(key('DA'),P.PDFString.of('/Unknown 12 Tf 0 g'))],
 ['unknown DA operator',(d,f)=>f.set(key('DA'),P.PDFString.of('/Helv 12 Tf 0 g q'))],
 ['oversized DA',(d,f)=>f.set(key('DA'),P.PDFString.of(' '.repeat(2049)))],
 ['out of range DA',(d,f)=>f.set(key('DA'),P.PDFString.of('/Helv -12 Tf 0 g'))],
 ['invalid color DA',(d,f)=>f.set(key('DA'),P.PDFString.of('/Helv 12 Tf 2 g'))]
];
for(const [label,edit] of badForms)test('empty form still fails closed: '+label,async()=>{const a=await fixture(edit),before=Buffer.from(a);await assert.rejects(inspect(a),/^Error: (UNSUPPORTED_FORM|UNSUPPORTED_ACTION|UNSUPPORTED_SIGNATURE)$/);assert.deepEqual(Buffer.from(a),before);});
for(const [label,edit,code] of [
 ['detached JavaScript',d=>d.context.register(d.context.obj({S:'JavaScript',JS:P.PDFString.of('fixture')})),'UNSUPPORTED_ACTION'],
 ['additional actions',d=>d.catalog.set(key('AA'),d.context.obj({})),'UNSUPPORTED_ACTION'],
 ['signature',d=>d.context.register(d.context.obj({Type:'Sig'})),'UNSUPPORTED_SIGNATURE'],
 ['attachment',d=>d.context.register(d.context.stream('fixture',{Type:'EmbeddedFile'})),'UNSUPPORTED_ATTACHMENT'],
 ['widget',d=>d.getPages()[0].node.set(key('Annots'),d.context.obj([{Type:'Annot',Subtype:'Widget',Rect:[0,0,20,20]}])),'UNSUPPORTED_ANNOTATIONS'],
 ['partial tag structure',d=>{d.catalog.set(key('StructTreeRoot'),d.context.obj({Type:'StructTreeRoot',K:[],ParentTree:{Nums:[]}}));d.catalog.set(key('MarkInfo'),d.context.obj({Marked:false}));},'TAGGED_STRUCTURE']
])test('expanded empty form cannot mask '+label,async()=>{const a=await fixture(edit);await assert.rejects(inspect(a),new RegExp('^Error: '+code+'$'));});
