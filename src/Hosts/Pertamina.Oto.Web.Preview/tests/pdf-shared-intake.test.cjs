const test=require('node:test'),assert=require('node:assert/strict'),{mount}=require('./helpers/paste-controller.cjs');
function event(target,type,props){const e=new Event(type,{cancelable:true});for(const[k,v]of Object.entries(props))Object.defineProperty(e,k,{value:v});target.dispatchEvent(e);return e;}
for(const tool of ['merge','organize','split','numbers','remove','extract','rotate','watermark','crop'])test(tool+': shared picker/drop guard retains actual PDF result bytes',async()=>{
 const h=await mount(tool);h.paste(tool==='merge'?[h.file,h.file]:[h.file]);await h.idle();await h.$(tool==='merge'?'merge-run':'pdf-run').click();await h.idle();
 const before=h.buffers(),bytes=await Promise.all(before.map(async b=>Buffer.from(await b.arrayBuffer()))),jobs=h.jobs();assert.ok(before.length);
 const field=h.$(tool==='merge'?'merge-files':'pdf-files');event(field,'change',{target:{files:[h.file],value:'test'}});
 event(h.context,'drop',{dataTransfer:{files:[h.file],types:['Files']}});await h.idle();
 assert.equal(h.jobs(),jobs);assert.deepEqual(h.buffers(),before);for(let i=0;i<before.length;i++)assert.deepEqual(Buffer.from(await before[i].arrayBuffer()),bytes[i]);
 assert.match(h.feedback(),/Hasil sebelumnya tetap tersedia/);h.controller.dispose();await h.idle();
});
test('image workspace uses the shared profile and controller with native text paste preserved',async()=>{
 const h=await mount('images-pdf');assert.equal(h.controller.profile.images,true);assert.equal(h.controller.profile.multiple,true);assert.equal(h.controller.profile.transferSources,false);
 const text=h.paste([],h.$('pdf-output-name'),'Photo name');assert.equal(text.defaultPrevented,false);h.controller.deactivate();const inactive=h.paste([new File(['x'],'photo.jpg')]);assert.equal(inactive.defaultPrevented,false);
 h.controller.dispose();assert.equal(h.controller.disposed,true);
});
