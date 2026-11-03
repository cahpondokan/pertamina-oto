'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const {tagged,P,key}=require('./tagged-fixtures.cjs'),E=require('../wwwroot/js/pdf-engine.js'),T=require('../wwwroot/js/pdf-tags.js');
const inspect=b=>E.run(P,{action:'inspect',tool:'organize',buffers:[b]});
async function repair(edit=()=>{},missing=true,stale=true){return tagged({count:1,edit:(d,x)=>{
 const kids=x.sections[0].dict.lookup(key('K'),P.PDFArray),heading=kids.lookup(0,P.PDFDict);
 if(missing){heading.delete(key('P'));heading.set(key('K'),d.context.obj([0]));}
 if(stale)x.parents[1].push(kids.get(2));
 return edit(d,{...x,kids,heading});
}});}
async function inert(edit=()=>{}){return tagged({count:1,edit:(d,x)=>{
 const c=d.context;x.root.lookup(key('RoleMap'),P.PDFDict).set(key('Artifact'),key('Sect'));
 const container=c.obj({Type:'StructElem',S:'Artifact',P:x.docRef,K:[]}),ref=c.register(container);
 const leaf=c.obj({Type:'StructElem',S:'',P:ref,K:[]});container.set(key('K'),c.obj([c.register(leaf)]));
 x.document.lookup(key('K'),P.PDFArray).push(ref);return edit(d,{...x,leaf,container});
}});}
function rawContent(d,text){d.getPages()[0].node.set(key('Contents'),d.context.register(d.context.stream(text)));}
function semantics(m){return [...m.nodes.values()].filter(n=>n.role!=='Document').map(n=>({s:n.s,alt:m.get(n.d,'Alt')?.decodeText(),actual:m.get(n.d,'ActualText')?.decodeText(),id:n.id,kids:n.kids.filter(k=>k.mcid!==undefined).map(k=>[k.page,k.mcid])}));}
for(const [name,make,recovered] of [['empty spreadsheet placeholder',inert,false],['scanner parent only',()=>repair(()=>{},true,false),true],['scanner stale index only',()=>repair(()=>{},false,true),true],['scanner parent and stale index',repair,true]]){
 test(name+' preserves tags and reopens after processing',async()=>{
  const a=await make(),before=Buffer.from(a),source=T.read(P,await P.PDFDocument.load(a));
  assert.equal(source.recovered,recovered);const r=await E.run(P,{tool:'organize',buffers:[a],pages:[{source:0,index:0,rotation:0}]});
  const b=r.results[0].bytes,d=await P.PDFDocument.load(b),model=T.read(P,d);
  assert.deepEqual(semantics(model),semantics(source));assert.deepEqual((await inspect(b)).counts,[1]);assert.deepEqual(Buffer.from(a),before);
  assert.equal(model.recovered,false);assert.equal(model.get(model.mark,'Suspects'),P.PDFBool.True);
  for(const n of model.nodes.values())assert.ok(n.d.get(key('P')) instanceof P.PDFRef);
  if(!recovered){const leaf=[...model.nodes.values()].find(n=>n.inert);assert.ok(leaf);assert.equal(leaf.d.has(key('Pg')),false);assert.equal(leaf.kids.length,0);}
  fs.writeFileSync(path.join(process.env.OTO_PDF_TEST_DIR,recovered?'scanner-recovered.pdf':'spreadsheet-placeholder.pdf'),b);
 });
}
for(const [name,edit] of [
 ['null parent',(d,x)=>x.heading.set(key('P'),P.PDFNull)],
 ['conflicting parent',(d,x)=>x.heading.set(key('P'),x.docRef)],
 ['missing page',(d,x)=>x.heading.delete(key('Pg'))],
 ['multiple content children',(d,x)=>x.heading.set(key('K'),d.context.obj([0,3]))],
 ['non-heading missing parent',(d,x)=>x.heading.set(key('S'),key('H2'))],
 ['duplicate forward edge',(d,x)=>x.kids.push(x.kids.get(0))],
 ['unreachable parent-tree owner',(d,x)=>x.parents[1].set(3,d.context.register(d.context.obj({Type:'StructElem',S:'H1',Pg:d.getPages()[0].ref,K:3})))],
 ['non-figure stale entry',(d,x)=>x.parents[1].set(3,x.kids.get(1))],
 ['real inline MCID',d=>rawContent(d,'/Span <</MCID 3>> BDC EMC')],
 ['escaped MCID name',d=>rawContent(d,'/Span <</MC#49D +3.0>> BDC EMC')],
 ['MCID inside comment conservative',d=>rawContent(d,'% /MCID 3\n')],
 ['indirect MCID operand conservative',d=>rawContent(d,'/Span <</MCID 18 0 %comment\nR>> BDC EMC')],
 ['split content name',d=>d.getPages()[0].node.set(key('Contents'),d.context.obj([d.context.register(d.context.stream('/Span <</MC')),d.context.register(d.context.stream('ID 3>> BDC EMC'))]))],
 ['split escaped content name',d=>d.getPages()[0].node.set(key('Contents'),d.context.obj([d.context.register(d.context.stream('/Span <</MC#4')),d.context.register(d.context.stream('9D 3>> BDC EMC'))]))],
 ['Form XObject MCID',d=>d.context.register(d.context.stream('/Span <</MCID 3>> BDC EMC',{Type:'XObject',Subtype:'Form',BBox:[0,0,10,10]}))],
 ['direct named property list',d=>d.getPages()[0].node.Resources().set(key('Properties'),d.context.obj({Prop1:{MCID:3}}))],
 ['indirect named property list',d=>d.getPages()[0].node.Resources().set(key('Properties'),d.context.register(d.context.obj({Prop1:{MCID:3}})))],
 ['ancestor named property list',d=>d.getPages()[0].node.lookup(key('Parent'),P.PDFDict).set(key('Resources'),d.context.obj({Properties:{Prop1:{MCID:3}}}))],
 ['unreferenced named property list conservative',d=>d.context.register(d.context.obj({Resources:{Properties:{Prop1:{MCID:3}}}}))],
])test('refuses ambiguous recovery: '+name,async()=>{const a=await repair(edit),before=Buffer.from(a);await assert.rejects(inspect(a),/^Error: TAGGED_STRUCTURE$/);assert.deepEqual(Buffer.from(a),before);});
test('bounded recovery refuses oversized decoded content',async()=>{const a=await repair(d=>rawContent(d,' '.repeat(4*1024*1024+1)));await assert.rejects(inspect(a),/^Error: LIMIT$/);});
for(const [name,edit] of [
 ['semantic text',(d,x)=>x.leaf.set(key('Alt'),P.PDFString.of('do not drop'))],
 ['content child',(d,x)=>x.leaf.set(key('K'),d.context.obj([3]))],
 ['explicit page',(d,x)=>x.leaf.set(key('Pg'),d.getPages()[0].ref)],
 ['null children',(d,x)=>x.leaf.set(key('K'),P.PDFNull)],
 ['missing children',(d,x)=>x.leaf.delete(key('K'))],
 ['different container',(d,x)=>x.container.set(key('S'),key('Sect'))],
 ['different role map',(d,x)=>x.root.lookup(key('RoleMap'),P.PDFDict).set(key('Artifact'),key('P'))],
 ['arbitrary unknown role',(d,x)=>x.leaf.set(key('S'),key('UnknownRole'))],
])test('does not generalize empty-placeholder exception: '+name,async()=>{await assert.rejects(inspect(await inert(edit)),/^Error: UNSUPPORTED_TAGGED$/);});
for(const [name,edit,code] of [
 ['script',d=>d.catalog.set(key('OpenAction'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('synthetic')})),'UNSUPPORTED_ACTION'],
 ['signature',d=>d.catalog.set(key('Perms'),d.context.obj({})),'UNSUPPORTED_SIGNATURE'],
 ['attachment',d=>d.catalog.set(key('AF'),d.context.obj([])),'UNSUPPORTED_ATTACHMENT'],
 ['real form',d=>d.catalog.set(key('AcroForm'),d.context.obj({Fields:[{FT:'Tx'}]})),'UNSUPPORTED_FORM'],
])test('repair never bypasses '+name+' guard',async()=>{await assert.rejects(inspect(await repair(edit)),new RegExp('^Error: '+code+'$'));});
for(const [name,make] of [['scanner recovery',repair],['spreadsheet placeholder',inert]])test(name+' works through browser-global worker without CommonJS/network',async()=>{
 const vm=require('node:vm'),web=path.resolve(__dirname,'../wwwroot'),sandbox={setTimeout,clearTimeout,TextEncoder,TextDecoder,URL,console:{log(){},warn(){},error(){}}},context=vm.createContext(sandbox);
 vm.runInContext('self=globalThis; self.close=()=>{}; fetch=()=>{throw Error("NETWORK_FORBIDDEN")}; XMLHttpRequest=WebSocket=class{constructor(){throw Error("NETWORK_FORBIDDEN")}};',context);
 sandbox.importScripts=(...urls)=>{for(const url of urls){assert.ok(['/js/pdf-capacity-data.js','/js/pdf-capacity.js','/vendor/pdf-lib-1.17.1/pdf-lib.js','/js/pdf-annotations.js','/js/pdf-tags.js','/js/pdf-engine.js'].includes(url));vm.runInContext(fs.readFileSync(path.join(web,url),'utf8'),context,{timeout:10000});}};
 vm.runInContext(fs.readFileSync(path.join(web,'js/pdf-worker.js'),'utf8'),context,{timeout:10000});
 sandbox.input=Array.from(await make());let result;sandbox.postMessage=v=>{result=v;};
 await vm.runInContext('self.onmessage({data:{action:"run",tool:"organize",pages:[{source:0,index:0,rotation:0}],buffers:[new Uint8Array(input)]}})',context,{timeout:10000});
 assert.equal(result.ok,true,JSON.stringify(result));assert.equal(result.results[0].count,1);
 sandbox.input=Array.from(result.results[0].bytes);
 await vm.runInContext('self.onmessage({data:{action:"inspect",tool:"organize",buffers:[new Uint8Array(input)]}})',context,{timeout:10000});
 assert.equal(result.ok,true);assert.equal(result.counts[0],1);
});
