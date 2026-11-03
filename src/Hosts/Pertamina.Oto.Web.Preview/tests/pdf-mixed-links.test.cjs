'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const {tagged,P,key}=require('./tagged-fixtures.cjs'),{annotated}=require('./annotation-fixtures.cjs'),E=require('../wwwroot/js/pdf-engine.js'),T=require('../wwwroot/js/pdf-tags.js'),A=require('../wwwroot/js/pdf-annotations.js');
const inspect=b=>E.run(P,{action:'inspect',tool:'organize',buffers:[b]});
async function orphan(edit=()=>{}){return tagged({count:2,edit:(d,x)=>{
 const kids=x.sections[0].dict.lookup(key('K'),P.PDFArray),ref=kids.get(0),heading=d.context.lookup(ref,P.PDFDict);kids.remove(0);heading.delete(key('P'));heading.delete(key('ID'));heading.set(key('K'),d.context.obj([0]));
 const names=x.root.lookup(key('IDTree'),P.PDFDict).lookup(key('Names'),P.PDFArray);names.remove(0);names.remove(0);
 return edit(d,{...x,heading,ref});
}});}
async function relative(uri='../Folder A/Sheet 1.xlsx',edit=()=>{}){return annotated({count:2,edit:(d,s)=>{s.records[5].dict.lookup(key('A'),P.PDFDict).set(key('URI'),P.PDFHexString.fromText(uri));edit(d,s);}});}
function uriList(d){return [...A.read(P,d).records.values()].filter(r=>r.action&&r.action.has(key('URI'))).map(r=>r.action.lookup(key('URI')).asBytes()).map(b=>Buffer.from(b).toString('hex'));}
test('orphan heading is retained without an invented parent or logical order',async()=>{
 const source=await orphan(),before=Buffer.from(source),sm=T.read(P,await P.PDFDocument.load(source));assert.equal(sm.orphans.length,1);assert.equal(sm.orphans[0].d.has(key('P')),false);
 const r=await E.run(P,{tool:'organize',buffers:[source],pages:[0,1].map(index=>({source:0,index,rotation:0}))});
 assert.ok(r.warnings.includes('ORPHAN_TAG'));assert.ok(r.warnings.includes('PARTIAL_TAGS'));
 const d=await P.PDFDocument.load(r.results[0].bytes),m=T.read(P,d);assert.equal(m.orphans.length,1);assert.equal(m.get(m.mark,'Marked'),P.PDFBool.False);
 assert.equal(m.orphans[0].d.has(key('P')),false);assert.equal(m.orphans[0].s,'H1');assert.deepEqual(m.orphans[0].kids,sm.orphans[0].kids);assert.equal(m.get(m.orphans[0].d,'T').decodeText(),sm.get(sm.orphans[0].d,'T').decodeText());assert.equal(m.owners.reduce((n,x)=>n+x.size,0),6);
 assert.deepEqual((await inspect(r.results[0].bytes)).counts,[2]);assert.deepEqual(Buffer.from(source),before);
 fs.writeFileSync(path.join(process.env.OTO_PDF_TEST_DIR,'orphan-preserved.pdf'),r.results[0].bytes);
});
for(const [name,edit] of [
 ['parent already declared',(d,x)=>x.heading.set(key('P'),x.docRef)],
 ['null parent',(d,x)=>x.heading.set(key('P'),P.PDFNull)],
 ['wrong page',(d,x)=>x.heading.set(key('Pg'),d.getPages()[1].ref)],
 ['unknown orphan role',(d,x)=>x.heading.set(key('S'),key('P'))],
 ['unknown content owner',(d,x)=>x.heading.set(key('K'),d.context.obj([1]))],
 ['multiple content owners',(d,x)=>x.heading.set(key('K'),d.context.obj([0,3]))],
 ['duplicate orphan page binding',(d,x)=>x.parents[3].set(0,x.ref)],
 ['hidden script',(d,x)=>x.heading.set(key('AA'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('x')}))],
])test('detached-tag preservation refuses '+name,async()=>{await assert.rejects(inspect(await orphan(edit)),/^Error: (TAGGED_STRUCTURE|UNSUPPORTED_ACTION)$/);});
test('partial metadata flag is accepted only with valid structure and preserved',async()=>{
 const a=await tagged({edit:d=>d.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Marked'),P.PDFBool.False)});assert.ok((await inspect(a)).warnings.includes('PARTIAL_TAGS'));
 const r=await E.run(P,{tool:'rotate',ranges:'1',angle:90,buffers:[a]}),m=T.read(P,await P.PDFDocument.load(r.results[0].bytes));assert.equal(m.get(m.mark,'Marked'),P.PDFBool.False);
 await assert.rejects(inspect(await tagged({edit:(d,x)=>{d.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Marked'),P.PDFBool.False);x.root.delete(key('ParentTree'));}})),/^Error: TAGGED_STRUCTURE$/);
});
for(const order of [0,1])test('mixed merge keeps all tag/annotation/content edges in order '+order,async()=>{
 const t=await tagged({count:2}),p=await relative(),buffers=order?[p,t]:[t,p],before=buffers.map(b=>Buffer.from(b));
 const r=await E.run(P,{tool:'merge',buffers}),d=await P.PDFDocument.load(r.results[0].bytes),m=T.read(P,d);
 assert.equal(d.getPageCount(),4);assert.equal(m.get(m.mark,'Marked'),P.PDFBool.False);assert.equal(m.owners.reduce((n,x)=>n+x.size,0),6);assert.equal(A.read(P,d).records.size,16);
 assert.deepEqual(uriList(d),uriList(await P.PDFDocument.load(p)));assert.ok(r.warnings.includes('PARTIAL_TAGS'));assert.ok(r.warnings.includes('RELATIVE_LINKS'));
 const empty=order?[0,1]:[2,3];for(const i of empty){assert.equal(m.owners[i].size,0);assert.equal(d.getPages()[i].node.has(key('StructParents')),false);}
 for(const [i,b] of buffers.entries())assert.deepEqual(Buffer.from(b),before[i]);assert.deepEqual((await inspect(r.results[0].bytes)).counts,[4]);
 const again=await E.run(P,{tool:'merge',buffers:[r.results[0].bytes,t]});assert.deepEqual((await inspect(again.results[0].bytes)).counts,[6]);assert.ok(again.warnings.includes('PARTIAL_TAGS'));
 const extract=await E.run(P,{tool:'extract',ranges:empty.map(i=>i+1).join(','),buffers:[r.results[0].bytes]});assert.deepEqual((await inspect(extract.results[0].bytes)).counts,[2]);assert.deepEqual(uriList(await P.PDFDocument.load(extract.results[0].bytes)),uriList(await P.PDFDocument.load(p)));
 fs.writeFileSync(path.join(process.env.OTO_PDF_TEST_DIR,'mixed-preserved-'+order+'.pdf'),r.results[0].bytes);
});
test('orphan plus untagged source remains processable across operations',async()=>{
 const d=await P.PDFDocument.create();d.addPage().drawText('SYNTHETIC UNTAGGED');const r=await E.run(P,{tool:'merge',buffers:[await orphan(),await d.save()]});
 for(const request of [{tool:'rotate',ranges:'1',angle:90},{tool:'numbers',options:{start:1,size:12,margin:12,position:'bottom-center',format:'number'}},{tool:'watermark',options:{text:'DRAF',size:30,opacity:18,angle:45,color:'blue'}},{tool:'extract',ranges:'3'}]){
  const next=await E.run(P,{...request,buffers:[r.results[0].bytes]});assert.equal((await inspect(next.results[0].bytes)).counts.length,1);
 }
});
for(const uri of ['Sheet 1.xlsx','../Folder/Sheet 1.xlsx','../../Folder A/(Sheet)+A&B.xlsx'])test('preserves exact passive relative Excel URI: '+uri,async()=>{
 const b=await relative(uri),r=await E.run(P,{tool:'rotate',ranges:'1',angle:90,buffers:[b]});assert.ok(r.warnings.includes('RELATIVE_LINKS'));assert.deepEqual(uriList(await P.PDFDocument.load(r.results[0].bytes)),uriList(await P.PDFDocument.load(b)));assert.deepEqual((await inspect(r.results[0].bytes)).counts,[2]);
});
for(const uri of ['file:///C:/a.xlsx','C:/a.xlsx','\\\\server\\a.xlsx','//server/a.xlsx','/a.xlsx','javascript:x.xlsx','data:x.xlsx','a%2fx.xlsx','a%252fx.xlsx','a.xlsx?x=1','a.xlsx#x','a.xlsm','a.exe.xlsx:stream','a/../b.xlsx','CON.xlsx','folder./a.xlsx','folder /a.xlsx','a.xlsx\u0000','a.xlsx\n','a//b.xlsx','a|b.xlsx','a\\b.xlsx','../'.repeat(9)+'a.xlsx'])test('refuses active/ambiguous relative URI '+JSON.stringify(uri),async()=>{await assert.rejects(inspect(await relative(uri)),/^Error: UNSAFE_LINK$/);});
for(const [label,edit,code] of [
 ['Base',d=>d.catalog.set(key('URI'),d.context.obj({Base:P.PDFString.of('https://example.org/')})),'UNSUPPORTED_ACTION'],
 ['Launch',(d,s)=>s.records[5].dict.set(key('A'),d.context.obj({S:'Launch',F:P.PDFString.of('test.xlsx')})),'UNSUPPORTED_ACTION'],
 ['Next',(d,s)=>s.records[5].dict.lookup(key('A'),P.PDFDict).set(key('Next'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('x')})),'UNSUPPORTED_ACTION'],
 ['IsMap',(d,s)=>s.records[5].dict.lookup(key('A'),P.PDFDict).set(key('IsMap'),P.PDFBool.True),'UNSAFE_LINK'],
 ['attachment',d=>d.catalog.set(key('AF'),d.context.obj([])),'UNSUPPORTED_ATTACHMENT'],
 ['signature',d=>d.catalog.set(key('Perms'),d.context.obj({})),'UNSUPPORTED_SIGNATURE'],
 ['form',d=>d.catalog.set(key('AcroForm'),d.context.obj({Fields:[{FT:'Tx'}]})),'UNSUPPORTED_FORM'],
])test('relative URI support does not bypass '+label,async()=>{await assert.rejects(inspect(await relative('a.xlsx',edit)),new RegExp('^Error: '+code+'$'));});
test('browser-global worker preserves mixed, detached tags and relative links with networking disabled',async()=>{
 const vm=require('node:vm'),web=path.resolve(__dirname,'../wwwroot'),sandbox={setTimeout,clearTimeout,TextEncoder,TextDecoder,URL,console:{log(){},warn(){},error(){}}},context=vm.createContext(sandbox);
 vm.runInContext('self=globalThis; self.close=()=>{}; fetch=()=>{throw Error("NETWORK_FORBIDDEN")}; XMLHttpRequest=WebSocket=class{constructor(){throw Error("NETWORK_FORBIDDEN")}};',context);
 sandbox.importScripts=(...urls)=>{for(const url of urls){assert.ok(['/js/pdf-capacity-data.js','/js/pdf-capacity.js','/vendor/pdf-lib-1.17.1/pdf-lib.js','/js/pdf-annotations.js','/js/pdf-tags.js','/js/pdf-engine.js'].includes(url));vm.runInContext(fs.readFileSync(path.join(web,url),'utf8'),context,{timeout:10000});}};
 vm.runInContext(fs.readFileSync(path.join(web,'js/pdf-worker.js'),'utf8'),context,{timeout:10000});sandbox.inputs=[Array.from(await orphan()),Array.from(await relative())];let result;sandbox.postMessage=v=>{result=v;};
 await vm.runInContext('self.onmessage({data:{action:"run",tool:"merge",buffers:inputs.map(x=>new Uint8Array(x))}})',context,{timeout:10000});
 assert.equal(result.ok,true,JSON.stringify(result));for(const code of ['PARTIAL_TAGS','ORPHAN_TAG','RELATIVE_LINKS'])assert.ok(result.warnings.includes(code));
 sandbox.inputs=[Array.from(result.results[0].bytes)];await vm.runInContext('self.onmessage({data:{action:"inspect",buffers:inputs.map(x=>new Uint8Array(x))}})',context,{timeout:10000});assert.equal(result.ok,true);assert.equal(result.counts[0],4);
 const ui=fs.readFileSync(path.join(web,'js/browser-pdf.js'),'utf8'),view=fs.readFileSync(path.resolve(web,'../Views/Shared/_PageWorkspace.cshtml'),'utf8')+fs.readFileSync(path.resolve(web,'../Views/Shared/_PdfResultPanel.cshtml'),'utf8').replaceAll('@(Model.Prefix)','pdf')+fs.readFileSync(path.resolve(web,'../Views/Shared/_PdfToolSupport.cshtml'),'utf8');
 const messages=require('../wwwroot/js/pdf-document-messages.js').browser();
 for(const code of ['PARTIAL_TAGS','ORPHAN_TAG','RELATIVE_LINKS'])assert.ok(messages.compatibility([code]).length>0);
 for(const id of ['pdf-compatibility','pdf-result-notes','pdf-result-warning'])assert.ok(view.includes('id="'+id+'"'));
 assert.ok(view.includes('tidak dibuka, dicari, atau diunggah'));assert.ok(ui.includes('warnings:result.warnings||[]'));assert.ok(ui.includes('resultView.present(output)'));
});
