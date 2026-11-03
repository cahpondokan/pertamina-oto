const test=require('node:test'),assert=require('node:assert/strict');
const {mount}=require('./helpers/paste-controller.cjs');
for(const tool of ['extract','remove','rotate','numbers','watermark','crop']) {
 test(tool+': shared result survives save cancellation and writes the exact PDF before cleanup',async()=>{
  const h=await mount(tool);h.paste();await h.idle();
  if(['extract','remove','rotate'].includes(tool))h.$('pdf-ranges').value='2-3';
  if(tool==='rotate')h.$('pdf-angle').value='180';
  await h.$('pdf-run').click();await h.idle();assert.equal(h.root.dataset.state,'result',h.feedback());
  const blob=h.buffers()[0],bytes=Buffer.from(await blob.arrayBuffer()),pdf=await h.P.PDFDocument.load(bytes);
  assert.equal(pdf.getPageCount(),['extract','remove'].includes(tool)?2:4);
  if(tool==='rotate')assert.deepEqual(pdf.getPages().map(p=>p.getRotation().angle),[0,180,180,0]);
  if(tool==='crop')assert.ok(pdf.getPage(0).getCropBox().width<pdf.getPage(0).getMediaBox().width);
  h.context.showSaveFilePicker=async()=>{throw new DOMException('cancel','AbortError');};
  await h.$('pdf-save').click();assert.equal(h.buffers()[0],blob);assert.equal(h.root.dataset.state,'result');assert.equal(h.$('pdf-download').hidden,false);
  h.controller.deactivate();h.controller.activate();assert.equal(h.buffers()[0],blob);
  const written=[];h.context.showSaveFilePicker=async()=>({createWritable:async()=>new WritableStream({write:b=>written.push(b)})});
  await h.$('pdf-save').click();assert.deepEqual(Buffer.concat(written),bytes);
  assert.equal(h.root.dataset.state,'empty');assert.equal(h.controller.resources().sources.length,0);assert.equal(h.controller.resources().outputBytes,0);assert.equal(h.buffers().length,0);h.controller.dispose();
 });
}
