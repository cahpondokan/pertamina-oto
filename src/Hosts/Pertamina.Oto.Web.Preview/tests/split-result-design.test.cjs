const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const {mount}=require('./helpers/paste-controller.cjs');
async function ready(){const h=await mount('split');h.paste();await h.idle();h.$('pdf-split-tab-pages').click();await h.$('pdf-run').click();await h.idle();return h;}
function action(h,index,name){return h.$('pdf-result-list').children[index].querySelectorAll('button').find(b=>b.dataset.resultAction===name);}
function menuAction(h,index,name){return h.$('pdf-result-list').children[index].querySelector('.split-file-menu-wrap').querySelectorAll('button').find(b=>b.textContent===name);}
test('desktop result screen shows real totals, source, cards and mode-specific next tools',async()=>{
 const h=await ready(),panel=fs.readFileSync(path.join(__dirname,'../Views/Shared/_PdfResultPanel.cshtml'),'utf8');assert.equal(h.$('pdf-result').hidden,false);assert.match(panel,/PDF siap diunduh/);assert.match(h.$('pdf-result-total').textContent,/4 file PDF · 4 halaman ·/);assert.equal(h.$('pdf-result-source').textContent,'Hasil dari Paste-test.pdf');
 assert.equal(h.$('pdf-result-list').children.length,4);assert.match(h.$('pdf-result-list').children[2].textContent,/Halaman sumber 3/);assert.equal(h.$('pdf-next-tools').children.length,3);assert.deepEqual(h.$('pdf-next-tools').children.map(a=>a.href),['/alat/merge','/alat/organize','/alat/numbers']);assert.equal(h.context.document.activeElement,h.$('pdf-save-all'));h.controller.dispose();
});
test('custom partial ranges show accurate source page ranges and totals',async()=>{
 const h=await mount('split');h.paste();await h.idle();const row=h.$('pdf-split-range-rows').children[0],fields=row.querySelectorAll('input');fields[0].value='1';fields[1].value='2';fields[0].dispatchEvent(new Event('input'));h.$('pdf-split-add-range').click();const next=h.$('pdf-split-range-rows').children[1].querySelectorAll('input');next[0].value='3';next[1].value='4';next[0].dispatchEvent(new Event('input'));await h.$('pdf-run').click();await h.idle();
 assert.equal(h.$('pdf-result-list').children.length,2);assert.match(h.$('pdf-result-list').children[0].textContent,/Halaman sumber 1–2/);assert.match(h.$('pdf-result-list').children[1].textContent,/Halaman sumber 3–4/);assert.match(h.$('pdf-result-total').textContent,/2 file PDF · 4 halaman/);h.controller.dispose();
});
test('combining split ranges yields one direct-download PDF and one-file next tools',async()=>{
 const h=await mount('split');h.paste();await h.idle();const fields=h.$('pdf-split-range-rows').children[0].querySelectorAll('input');fields[0].value='1';fields[1].value='2';fields[0].dispatchEvent(new Event('input'));h.$('pdf-split-add-range').click();const next=h.$('pdf-split-range-rows').children[1].querySelectorAll('input');next[0].value='3';next[1].value='4';h.$('pdf-split-combine-ranges').checked=true;h.$('pdf-split-combine-ranges').dispatchEvent(new Event('change'));await h.$('pdf-run').click();await h.idle();
 assert.equal(h.$('pdf-result-list').children.length,1);assert.equal(h.$('pdf-save-all').textContent,'↓ Unduh PDF');assert.deepEqual(h.$('pdf-next-tools').children.map(a=>a.href),['/alat/organize','/alat/numbers','/alat/watermark']);assert.match(h.$('pdf-result-list').children[0].textContent,/Halaman sumber 1–4/);h.controller.dispose();
});
test('result preview opens the selected PDF and returns focus to a live card action',async()=>{
 const h=await ready();action(h,1,'preview').click();assert.equal(h.$('pdf-result-select').value,'1');assert.equal(h.previewOps.opens.length,1);assert.equal(h.previewOps.opens[0].source.bytes,h.buffers()[1]);assert.ok(h.previewOps.opens[0].target.isConnected);h.controller.dispose();
});
test('per-file rename rejects unsafe, empty and duplicate names, then applies to result and ZIP entry',async()=>{
 const h=await ready();menuAction(h,0,'Ubah nama').click();let input=h.$('pdf-result-list').children[0].querySelector('.split-file-menu-wrap').querySelector('input');input.value='bagian khusus.pdf';h.$('pdf-result-list').children[0].querySelector('.split-file-menu-wrap').querySelectorAll('button').find(b=>b.textContent==='Terapkan nama').click();assert.match(h.$('pdf-result-list').children[0].textContent,/bagian khusus\.pdf/);
 menuAction(h,1,'Ubah nama').click();const menu=h.$('pdf-result-list').children[1].querySelector('.split-file-menu-wrap');input=menu.querySelector('input');input.value='bagian khusus.pdf';menu.querySelectorAll('button').find(b=>b.textContent==='Terapkan nama').click();assert.equal(input.attributes['aria-invalid'],'true');assert.equal(h.$('pdf-result-select').children[1].textContent.includes('Paste-test_split_02.pdf'),true);
 menuAction(h,1,'Ubah nama').click();const invalid=h.$('pdf-result-list').children[1].querySelector('.split-file-menu-wrap');invalid.querySelector('input').value='../tak-valid.pdf';invalid.querySelectorAll('button').find(b=>b.textContent==='Terapkan nama').click();assert.equal(invalid.querySelector('input').attributes['aria-invalid'],'true');assert.equal(h.buffers().length,4);h.controller.dispose();
});
test('per-file download targets exact bytes and reports only that a browser download started',async()=>{
 const h=await ready(),expected=h.buffers()[2],captured=[],original=h.context.URL.createObjectURL;h.context.URL.createObjectURL=file=>{captured.push(file);return original(file);};action(h,2,'download').click();assert.equal(captured.at(-1),expected);assert.match(h.$('pdf-archive-note').textContent,/Unduhan Paste-test_split_03.pdf dimulai/);assert.match(h.$('pdf-result-list').children[2].textContent,/Unduhan dimulai/);assert.doesNotMatch(h.$('pdf-result-select').textContent,/tersimpan/);h.controller.dispose();
});
test('save picker cancellation keeps results and tells the user which direct download remains available',async()=>{
 const h=await ready();h.context.showSaveFilePicker=async()=>{throw new DOMException('cancel','AbortError');};menuAction(h,0,'Simpan sebagai…').click();await h.idle();assert.equal(h.buffers().length,4);assert.match(h.$('pdf-save-note').textContent,/Penyimpanan dibatalkan/);assert.match(h.$('pdf-save-note').textContent,/unduhan PDF langsung/);h.controller.dispose();
});
test('new PDF picker cancellation or invalid selection preserves the previous result set',async()=>{
 const h=await ready(),files=h.buffers();h.$('pdf-result-new').click();assert.deepEqual(h.buffers(),files);const pick=h.$('pdf-new-files');pick.files=[new File(['bad'],'notes.txt',{type:'text/plain'})];pick.dispatchEvent(new Event('change'));await h.idle();assert.deepEqual(h.buffers(),files);assert.equal(h.$('pdf-result').hidden,false);h.controller.dispose();
});
test('editing page choices keeps the original input, mode and existing outputs until an edit is made',async()=>{
 const h=await ready(),files=h.buffers(),source=h.controller.resources().sources[0];h.$('pdf-edit-split').click();assert.equal(h.root.dataset.state,'editing');assert.deepEqual(h.buffers(),files);assert.equal(h.controller.resources().sources[0],source);assert.equal(h.$('pdf-show-result').hidden,false);
 h.$('pdf-show-result').click();assert.equal(h.root.dataset.state,'result');h.$('pdf-edit-split').click();h.$('pdf-split-tab-ranges').click();assert.equal(h.buffers().length,0);assert.equal(h.controller.resources().sources[0],source);h.controller.dispose();
});
