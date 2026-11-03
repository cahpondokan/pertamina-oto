const test=require('node:test'),assert=require('node:assert/strict'),{mount}=require('./helpers/paste-controller.cjs');
const tools=['merge','organize','split','numbers','remove','extract','rotate','watermark','crop'];
for(const tool of tools)test(tool+': real controller accepts pasted PDF, creates valid output and preserves it on another paste',async()=>{
 const h=await mount(tool),name=h.$(tool==='merge'?'merge-name':'pdf-output-name');
 // Reproduce focus left on a name field before leaving and returning to a tab.
 name.focus();const event=h.paste(undefined,name);assert.equal(event.defaultPrevented,true);
 await h.idle();assert.equal(h.jobs(),1,h.feedback());
 const count=()=>tool==='merge'?JSON.parse(h.root.dataset.resources).sourceFiles:h.controller.resources().sources.length;
 assert.equal(count(),1,h.feedback());
 if(['merge','organize'].includes(tool)){h.paste();await h.idle();assert.equal(count(),2);}
 else{h.paste();await h.idle();assert.equal(count(),1);assert.match(h.feedback(),/satu PDF/);}
 await h.$(tool==='merge'?'merge-run':'pdf-run').click();await h.idle();
 const before=h.buffers();assert.ok(before.length>0,h.feedback());
 const bytes=await Promise.all(before.map(async b=>Buffer.from(await b.arrayBuffer())));
 const expected=tool==='merge'?8:tool==='organize'?8:tool==='remove'?3:4;
 let pages=0;for(const b of bytes)pages+=(await h.P.PDFDocument.load(b)).getPageCount();assert.equal(pages,expected);
 const jobs=h.jobs();h.paste();await h.idle();assert.equal(h.jobs(),jobs);assert.deepEqual(h.buffers(),before);assert.match(h.feedback(),/Hasil sebelumnya tetap tersedia/);
 for(let i=0;i<before.length;i++)assert.deepEqual(Buffer.from(await h.buffers()[i].arrayBuffer()),bytes[i]);
 if(tool!=='merge'){h.$('pdf-new').click();h.$('pdf-stay').click();h.paste();await h.idle();assert.deepEqual(h.buffers(),before,'result retained after cancelling Mulai baru');}
 const text=h.paste([],name,'Nama hasil');assert.equal(text.defaultPrevented,false);assert.deepEqual(h.buffers(),before);
 h.controller.dispose();await h.idle();const last=h.jobs();h.paste();assert.equal(h.jobs(),last,'listener detached on dispose');
});
for(const tool of ['merge','organize'])test(tool+': input limits, invalid PDF and busy state still protect current inputs',async()=>{
 const h=await mount(tool);h.paste();h.paste();await h.idle();assert.equal(h.jobs(),1,'second paste while busy must not start a job');
 const count=()=>tool==='merge'?JSON.parse(h.root.dataset.resources).sourceFiles:h.controller.resources().sources.length;
 assert.equal(count(),1);
 h.paste([new File(['bad'],'bad.txt')]);await h.idle();assert.equal(count(),1);assert.match(h.feedback(),/PDF saja/);
 h.paste([new File(['bad'],'bad.pdf')]);await h.idle();assert.equal(count(),1);assert.match(h.feedback(),/rusak|valid/);
 h.paste(Array(tool==='merge'?500:10).fill(h.file));await h.idle();assert.equal(count(),1);assert.match(h.feedback(),/500|batas/);
 h.controller.dispose();await h.idle();
});

