const test=require('node:test'),assert=require('node:assert/strict'),{mount}=require('./helpers/paste-controller.cjs');
async function ready(){const h=await mount('merge');h.paste([h.file,h.file]);await h.idle();await h.$('merge-run').click();await h.idle();assert.equal(h.buffers().length,1);return h;}
const resources=h=>JSON.parse(h.root.dataset.resources);
test('single save action falls back to a browser download and retains an unconfirmed result',async()=>{
 const h=await ready(),before=h.buffers()[0],data=Buffer.from(await before.arrayBuffer());
 h.$('merge-result-name').value='Dokumen final.pdf';await h.$('merge-save').click();
 assert.equal(h.buffers()[0],before);assert.deepEqual(Buffer.from(await before.arrayBuffer()),data);
 assert.equal(resources(h).objectUrls,1);assert.match(h.$('merge-save-note').textContent,/Unduhan dimulai/);assert.doesNotMatch(h.$('merge-save-note').textContent,/berhasil|sudah tersimpan/i);
 assert.equal(h.$('merge-download').hidden,true);await h.$('merge-save').click();assert.equal(resources(h).objectUrls,1,'repeat download reuses its URL');
 h.paste();await h.idle();assert.equal(h.buffers()[0],before,'download does not unlock paste over the result');
 h.controller.dispose();await h.idle();assert.equal(h.buffers().length,0);
});
test('native save releases output only after the write and close have completed',async()=>{
 const h=await ready(),before=h.buffers()[0],chunks=[];let closed=false;
 h.$('merge-result-name').value='Nama baru.pdf';
 h.context.showSaveFilePicker=async options=>{assert.equal(options.suggestedName,'Nama baru.pdf');return {createWritable:async()=>new WritableStream({write:b=>chunks.push(b),close:()=>{assert.equal(h.buffers()[0],before);closed=true;}})};};
 await h.$('merge-save').click();assert.equal(closed,true);assert.deepEqual(Buffer.concat(chunks),Buffer.from(await before.arrayBuffer()));assert.equal(h.buffers().length,0);assert.equal(resources(h).directories,0);assert.equal(h.root.dataset.state,'empty');
 h.controller.dispose();await h.idle();
});
for(const stage of ['cancel','permission','write'])test(`native save ${stage} preserves output and offers direct download`,async()=>{
 const h=await ready(),before=h.buffers()[0];
 h.context.showSaveFilePicker=async()=>{if(stage==='cancel')throw new DOMException('Cancelled','AbortError');return {createWritable:async()=>{if(stage==='permission')throw new DOMException('Denied','NotAllowedError');return new WritableStream({write(){throw Error('disk full');}});}};};
 await h.$('merge-save').click();assert.equal(h.buffers()[0],before);assert.equal(h.root.dataset.state,'result');assert.equal(h.$('merge-download').hidden,false);assert.match(h.$('merge-save-note').textContent,stage==='cancel'?/dibatalkan/:/Belum tersimpan/);
 h.$('merge-download').click();assert.equal(h.buffers()[0],before);assert.equal(resources(h).objectUrls,1);h.controller.dispose();await h.idle();
});
test('invalid result name cannot trigger either save path',async()=>{
 const h=await ready(),before=h.buffers()[0];let calls=0;h.context.showSaveFilePicker=async()=>{calls++;};
 h.$('merge-result-name').value='bad/name.pdf';await h.$('merge-save').click();h.$('merge-download').click();assert.equal(calls,0);assert.equal(resources(h).objectUrls,0);assert.equal(h.buffers()[0],before);assert.equal(h.$('merge-result-name-error').hidden,false);h.controller.dispose();await h.idle();
});
test('new work requires confirmation and cancellation preserves the result',async()=>{
 const h=await ready(),before=h.buffers()[0];await h.$('merge-save').click();
 let pending=h.$('merge-new').click();assert.equal(h.$('merge-confirm').open,true);h.$('merge-stay').click();await pending;assert.equal(h.buffers()[0],before);
 pending=h.controller.beforeLeave();h.$('merge-stay').click();assert.equal(await pending,false);assert.equal(h.buffers()[0],before);
 pending=h.$('merge-new').click();h.$('merge-confirm-yes').click();await pending;assert.equal(h.buffers().length,0);assert.equal(resources(h).objectUrls,0);assert.equal(resources(h).directories,0);h.controller.dispose();await h.idle();
});
test('document notices remain available in a collapsed disclosure',async()=>{
 const h=await mount('merge'),doc=await h.P.PDFDocument.create(),page=doc.addPage();page.drawText('Navigation sample');doc.catalog.set(h.P.PDFName.of('OpenAction'),doc.context.obj([page.ref,h.P.PDFName.of('Fit')]));
 const file=new File([await doc.save({useObjectStreams:false})],'Navigation.pdf',{type:'application/pdf'});h.paste([file,h.file]);await h.idle();await h.$('merge-run').click();await h.idle();
 assert.equal(h.buffers().length,1);assert.equal(h.$('merge-result-notes').hidden,false);assert.equal(h.$('merge-result-notes').open,false);assert.equal(h.$('merge-result-notes-summary').textContent,'1 catatan dokumen');assert.match(h.$('merge-result-warning').textContent,/Bookmark/);h.controller.dispose();await h.idle();
});
