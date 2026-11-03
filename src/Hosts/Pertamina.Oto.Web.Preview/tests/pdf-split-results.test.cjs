const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const {mount}=require('./helpers/paste-controller.cjs'),Z=require('../wwwroot/js/pdf-result-archive.js');
async function ready(){const h=await mount('split');h.paste();await h.idle();h.$('pdf-split-tab-pages').click();await h.$('pdf-run').click();await h.idle();return h;}
async function bytes(stream){const out=[];for await(const b of stream)out.push(Buffer.from(b));return Buffer.concat(out);}
test('all 200 pages become 200 distinct numbered PDFs even with a long source name',async()=>{
 const h=await mount('split'),doc=await h.P.PDFDocument.create();for(let i=0;i<200;i++)doc.addPage([200+i,400]);
 h.paste([new File([await doc.save()], 'x'.repeat(100)+'.pdf')]);await h.idle();h.$('pdf-split-tab-pages').click();await h.$('pdf-run').click();await h.idle();
 assert.equal(h.buffers().length,200);const names=h.$('pdf-result-select').children.map(e=>e.textContent);assert.equal(new Set(names.map(n=>n.split(' · ')[0].replace(/^\d+\. /,''))).size,200);assert.match(names[0],/_001.pdf/);assert.match(names[199],/_200.pdf/);
 for(const [i,f]of h.buffers().entries()){const d=await h.P.PDFDocument.load(await f.arrayBuffer());assert.equal(d.getPageCount(),1);assert.equal(d.getPage(0).getWidth(),200+i);}h.controller.dispose();
});
test('per-page mode preserves supported tags and ordinary annotations and links',async()=>{
 const h=await mount('split'),source=await require('./annotation-fixtures.cjs').annotated({tags:true,count:4});h.paste([new File([source],'tagged.pdf')]);await h.idle();h.$('pdf-split-tab-pages').click();await h.$('pdf-run').click();await h.idle();
 assert.equal(h.buffers().length,4);for(const f of h.buffers()){const doc=await h.P.PDFDocument.load(await f.arrayBuffer());assert.ok(require('../wwwroot/js/pdf-tags.js').read(h.P,doc));assert.equal(require('../wwwroot/js/pdf-annotations.js').read(h.P,doc).records.size,8);}h.controller.dispose();
});
test('default Split emits every page; next/previous and native saves advance without losing results',async()=>{
 const h=await ready();assert.equal(h.$('pdf-split-tab-pages').attributes['aria-selected'],'true');assert.equal(h.buffers().length,4);
 const source=await h.P.PDFDocument.load(await h.file.arrayBuffer());
 for(const [i,file] of h.buffers().entries()){const d=await h.P.PDFDocument.load(await file.arrayBuffer());assert.equal(d.getPageCount(),1);const stream=(doc,n)=>doc.context.lookup(doc.getPage(n).node.Contents().asArray()[0]).getContents();assert.deepEqual(stream(d,0),stream(source,i));}
 h.$('pdf-result-next').click();assert.equal(h.$('pdf-result-select').value,'1');h.$('pdf-result-prev').click();assert.equal(h.$('pdf-result-select').value,'0');
 const names=[];h.context.showSaveFilePicker=async opts=>({createWritable:async()=>new WritableStream({close:()=>names.push(opts.suggestedName)})});
 for(let i=0;i<4;i++)await h.$('pdf-save').click();assert.equal(new Set(names).size,4);assert.equal(h.$('pdf-result-select').children.filter(e=>e.textContent.includes('tersimpan')).length,4);assert.equal(h.buffers().length,4);h.controller.dispose();
});
test('ZIP contains all results, download remains unconfirmed, and result bytes stay available',async()=>{
 const h=await ready(),files=h.buffers();let zip,released=0;
 h.context.OtoPdfArchive.temporary=async (p,o)=>{zip=await bytes(Z.stream(p,o));return {file:new Blob([zip]),release:async()=>released++};};
 await h.$('pdf-save-all').click();assert.doesNotMatch(h.$('pdf-result-select').textContent,/tersimpan/);assert.deepEqual(h.buffers(),files);assert.equal(h.root.dataset.state,'result');assert.match(h.$('pdf-archive-note').textContent,/Unduhan ZIP dimulai/);
 const dir=process.env.OTO_SPLIT_RESULT_EVIDENCE;if(dir){fs.mkdirSync(dir,{recursive:true});fs.writeFileSync(path.join(dir,'all-results.zip'),zip);for(let i=0;i<files.length;i++)fs.writeFileSync(path.join(dir,`expected-${i+1}.pdf`),Buffer.from(await files[i].arrayBuffer()));}
 h.controller.dispose();await new Promise(setImmediate);assert.equal(released,1);
});
for(const mode of ['cancel','quota','disk-full'])test('ZIP '+mode+' keeps all results unsaved and individually selectable',async()=>{
 const h=await ready(),files=h.buffers();h.context.OtoPdfArchive.temporary=async()=>{if(mode==='cancel')throw new DOMException('cancel','AbortError');if(mode==='quota')throw Error('STORAGE');throw Error('disk-full');};
 await h.$('pdf-save-all').click();assert.deepEqual(h.buffers(),files);assert.doesNotMatch(h.$('pdf-result-select').textContent,/tersimpan/);assert.match(h.$('pdf-archive-note').textContent,/dibatalkan|tidak cukup|belum tersedia/);h.$('pdf-result-next').click();assert.equal(h.$('pdf-result-select').value,'1');h.controller.dispose();
});
test('archive validates names, counts and byte limit before reading',()=>{
 const f=size=>({size,slice(){throw Error('must not read');}});
 assert.ok(Z.plan([{name:'large.pdf',file:f(4000000000)}]).size<2**32);
 for(const items of [[{name:'x.pdf',file:f(4000000001)}],[{name:'../x.pdf',file:f(1)}],[{name:'x.pdf',file:f(1)},{name:'X.pdf',file:f(1)}],Array.from({length:201},(_,i)=>({name:i+'.pdf',file:f(1)}))])assert.throws(()=>Z.plan(items),/ZIP_/);
});
test('archive chunk reads are bounded and cancellation/truncated reads fail, not partial success',async()=>{
 const b=new Blob([new Uint8Array(2*1024*1024+17)]);let max=0;const file={size:b.size,slice(a,z){max=Math.max(max,z-a);return b.slice(a,z);}};
 const p=Z.plan([{name:'Üji.pdf',file}]),out=await bytes(Z.stream(p));assert.equal(out.length,p.size);assert.ok(max<=1024*1024);
 const abort=new AbortController();abort.abort();await assert.rejects(bytes(Z.stream(p,{signal:abort.signal})),{name:'AbortError'});
 await assert.rejects(bytes(Z.stream(Z.plan([{name:'bad.pdf',file:{size:8,slice:()=>new Blob([new Uint8Array(1)])}}]))),/ZIP_READ/);
});
test('temporary ZIP checks storage and releases its owned directory on write failure',async()=>{
 const p=Z.plan([{name:'a.pdf',file:new Blob(['pdf bytes'])}]);let created=0,removed=0;
 const jobs={create:async()=>{created++;return 'owned';},remove:async n=>{assert.equal(n,'owned');removed++;}};
 await assert.rejects(Z.temporary(p,{},jobs,{estimate:async()=>({quota:2,usage:1})}),/STORAGE/);assert.equal(created,0);
 const storage={estimate:async()=>({quota:1e8,usage:0}),getDirectory:async()=>({getDirectoryHandle:async()=>({getFileHandle:async()=>({createWritable:async()=>new WritableStream({write(){throw Error('disk full');}})})})})};
 await assert.rejects(Z.temporary(p,{},jobs,storage),/disk full/);assert.equal(created,1);assert.equal(removed,1);
});
