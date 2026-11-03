const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const app=path.resolve(__dirname,'..');
const P=require(app+'/wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js'),E=require(app+'/wwwroot/js/pdf-engine.js'),C=require('../wwwroot/js/pdf-compatible-copy.js'),UI=require('../wwwroot/js/pdf-compatibility.js');
const k=P.PDFName.of,hash=b=>crypto.createHash('sha256').update(b).digest('hex'),dir=process.env.OTO_COMPAT_TEST_DIR||fs.mkdtempSync(path.join(require('node:os').tmpdir(),'oto-compat-'));fs.mkdirSync(dir,{recursive:true});
const ap=d=>d.context.register(d.context.stream('0.2 0.3 0.7 RG 3 w 0 0 m 100 40 l 50 0 l S',{Type:'XObject',Subtype:'Form',BBox:[0,0,100,40],Resources:{}}));
async function fixture(edit=()=>{}){const d=await P.PDFDocument.create(),p=d.addPage([300,420]);p.drawText('OTO COMPATIBILITY TEST',{x:20,y:380,size:12});d.addPage([420,300]).drawText('Landscape source page',{x:20,y:260,size:12});await edit(d,p);return d.save({useObjectStreams:false});}
const cases=[
 ['signature',(d,p)=>{const sig=d.context.register(d.context.obj({Type:'Sig',ByteRange:[0,1,2,3],Contents:P.PDFHexString.of('00')}));const a=d.context.register(d.context.obj({Type:'Annot',Subtype:'Widget',FT:'Sig',V:sig,T:P.PDFString.of('test'),Rect:[30,80,180,140],AP:{N:ap(d)}}));p.node.addAnnot(a);d.catalog.set(k('AcroForm'),d.context.obj({Fields:[a],SigFlags:3}));}],
 ['empty-signature',(d,p)=>{const a=d.context.register(d.context.obj({Type:'Annot',Subtype:'Widget',FT:'Sig',Rect:[30,80,180,140],T:P.PDFString.of('empty')}));p.node.addAnnot(a);d.catalog.set(k('AcroForm'),d.context.obj({Fields:[a]}));}],
 ['form',(d,p)=>{const f=d.getForm();const t=f.createTextField('name');t.setText('VALUE 123');t.addToPage(p,{x:20,y:250,width:180,height:30});const c=f.createCheckBox('yes');c.addToPage(p,{x:20,y:200,width:20,height:20});c.check();}],
 ['attachment',async d=>{await d.attach(new Uint8Array([84,69,83,84]),'synthetic.txt',{mimeType:'text/plain'});}],
 ['script',d=>d.catalog.set(k('OpenAction'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('globalThis.OTO_SENTINEL_EXECUTED=true;')}))],
 ['3d',(d,p)=>p.node.addAnnot(d.context.register(d.context.obj({Type:'Annot',Subtype:'3D',Rect:[30,80,180,140],AP:{N:ap(d)},'3DD':d.context.register(d.context.stream('synthetic only'))})))],
 ['multimedia',(d,p)=>p.node.addAnnot(d.context.register(d.context.obj({Type:'Annot',Subtype:'RichMedia',Rect:[30,80,180,140],AP:{N:ap(d)}})))],
 ['redact',(d,p)=>p.node.addAnnot(d.context.register(d.context.obj({Type:'Annot',Subtype:'Redact',Rect:[30,80,180,140],AP:{N:ap(d)}})))],
 ['static',()=>{}]
];
for(const [name,edit] of cases)test('derivative '+name+' keeps source intact and strict output gate',async()=>{const bytes=await fixture(edit),before=hash(bytes);fs.writeFileSync(path.join(dir,name+'.pdf'),bytes);
 if(name==='redact'){await assert.rejects(C.normalize(P,E,bytes),/^Error: COPY_RENDER$/);return;}
 const out=await C.normalize(P,E,bytes);assert.equal(hash(bytes),before);const d=await P.PDFDocument.load(out);assert.equal(d.getPageCount(),2);E.inspectStructures(P,d);assert.equal(d.getPage(0).getWidth(),300);assert.equal(d.getPage(1).getWidth(),420);assert.equal(d.catalog.has(k('AcroForm')),false);assert.equal(d.catalog.has(k('Perms')),false);assert.equal(d.getPage(0).node.has(k('Annots')),false);fs.writeFileSync(path.join(dir,name+'-copy.pdf'),out);
});
test('temporary encryption-export page is removed exactly',async()=>{const b=await fixture(d=>d.addPage([10,10]));const out=await C.normalize(P,E,b,{dropLast:1});assert.equal((await P.PDFDocument.load(out)).getPageCount(),2);await assert.rejects(C.normalize(P,E,b,{dropLast:2}),/^Error: COPY_LIMIT$/);});
test('source limit and page raster bounds are explicit',async()=>{await assert.rejects(C.normalize(P,E,new Uint8Array(C.MAX_BYTES+1)),/^Error: COPY_LIMIT$/);for(const [w,h]of[[595,842],[842,595],[14400,14400]]){const s=UI.pixelSize(w,h);assert.ok(s.width*s.height<=UI.MAX_PIXELS);assert.ok(s.width<=8192&&s.height<=8192);}assert.throws(()=>UI.pixelSize(0,10));});
test('normal engine still refuses special source and accepts its copy',async()=>{const b=await fixture(cases[0][1]);await assert.rejects(E.run(P,{tool:'extract',action:'inspect',buffers:[b]}),/^Error: UNSUPPORTED_SIGNATURE$/);const out=await C.normalize(P,E,b);assert.deepEqual((await E.run(P,{tool:'extract',action:'inspect',buffers:[out]})).counts,[2]);});
test('XFA never silently becomes a blank vector success',async()=>{const b=await fixture(d=>d.catalog.set(k('AcroForm'),d.context.obj({XFA:d.context.register(d.context.stream('<test/>'))})));await assert.rejects(C.normalize(P,E,b),/^Error: COPY_XFA$/);});

test('raster copy embeds resolvable images and retains portrait/landscape dimensions',async()=>{
 const png=new Uint8Array(Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVQIHWP4z8DwHwAFgAI/ScLttAAAAABJRU5ErkJggg==','base64'));
 const bytes=await C.rasterDocument(P,E,[{width:300,height:420,bytes:png},{width:420,height:300,bytes:png}]);
 const doc=await P.PDFDocument.load(bytes);E.inspectStructures(P,doc);
 assert.deepEqual(doc.getPages().map(p=>[p.getWidth(),p.getHeight()]),[[300,420],[420,300]]);
 for(const page of doc.getPages())assert.ok(page.node.Resources().lookup(k('XObject')).entries().length>0);
 await assert.rejects(C.rasterDocument(P,E,[]),/^Error: COPY_LIMIT$/);
});
