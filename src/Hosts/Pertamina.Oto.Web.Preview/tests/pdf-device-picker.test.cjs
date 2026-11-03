const test=require('node:test'),assert=require('node:assert/strict'),{mount}=require('./helpers/paste-controller.cjs');
for(const tool of ['merge','organize','split','extract','remove','rotate','numbers','watermark','crop','images-pdf'])test(tool+': primary picker and Ctrl+O directly activate the device input',async()=>{
 const h=await mount(tool),prefix=tool==='merge'?'merge':'pdf';let calls=0;
 h.$(prefix+'-files').onclick=()=>calls++;
 await h.$(tool==='merge'?'merge-pick':'pdf-empty-pick').click();assert.equal(calls,1);
 const e=new Event('keydown',{cancelable:true});Object.defineProperties(e,{key:{value:'o'},ctrlKey:{value:true}});h.root.ownerDocument.dispatchEvent(e);
 assert.equal(calls,2);assert.equal(e.defaultPrevented,true);assert.equal(h.root.ownerDocument.querySelector('dialog[open]'),null);
 if(tool==='merge')assert.equal(h.$('merge-sources'),undefined);
 h.controller.dispose();
});
