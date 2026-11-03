/* Synthetic UI acceptance. Set OTO_PDF_AUTO_FIXTURES, OTO_PDF_TEST_OUTPUT,
 * OTO_PLAYWRIGHT_MODULE; optional OTO_LOCAL_BASE. No user documents. */
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const fixtures=process.env.OTO_PDF_AUTO_FIXTURES,output=process.env.OTO_PDF_TEST_OUTPUT,base=process.env.OTO_LOCAL_BASE||'http://127.0.0.1:5278';
assert.ok(fixtures&&output);const {chromium}=require(process.env.OTO_PLAYWRIGHT_MODULE||'playwright');fs.mkdirSync(output,{recursive:true});
const hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex'),tools=['split','merge','organize','extract','remove','rotate','numbers','watermark','crop'];
const options={numbers:{position:'bottom-center',start:1,size:12,format:'number',margin:12},watermark:{text:'DRAF',size:48,opacity:18,angle:45,color:'blue'},crop:{top:5,right:5,bottom:5,left:5}};
async function pick(p,selector,files){const next=p.waitForEvent('filechooser',{timeout:5000});await p.locator(selector).click();const chooser=await next;assert.equal(await p.locator('dialog[open]:not(.pdf-compatible-dialog)').count(),0);await chooser.setFiles(files);return chooser;}
async function drop(p,files){const data=await p.evaluateHandle(items=>{const transfer=new DataTransfer();for(const item of items){transfer.items.add(new File([Uint8Array.from(atob(item.bytes),c=>c.charCodeAt(0))],item.name,{type:'application/pdf'}));}return transfer;},files.map(file=>({name:path.basename(file),bytes:fs.readFileSync(file).toString('base64')})));await p.dispatchEvent('[data-browser-tool]','drop',{dataTransfer:data});await data.dispose();}
async function loaded(p,tool){const prefix=tool==='merge'?'merge':'pdf';await p.waitForFunction(prefix=>{const root=document.querySelector('[data-browser-tool]');if(root.getAttribute('aria-busy')!=='false')return false;return prefix==='merge'?Number(document.querySelector('#merge-count').textContent)>0:document.querySelector('#pdf-empty').hidden===true;},prefix,{timeout:15000});}
async function ready(p,prefix){await p.waitForFunction(id=>!document.getElementById(id).disabled,prefix+'-run',{timeout:15000});}
(async()=>{const browser=await chromium.launch({channel:'msedge',headless:true}),results=process.env.OTO_BROWSER_RESUME==='1'?JSON.parse(fs.readFileSync(path.join(output,'browser-results.json'),'utf8')):[];
try{
 for(const tool of tools)for(const name of ['signature','empty-signature','form','blank-form','scan-two-signatures']){
  if(results.some(r=>r.tool===tool&&r.name===name))continue;
  const context=await browser.newContext({acceptDownloads:true,viewport:{width:1440,height:1000}}),p=await context.newPage(),errors=[],remote=[];p.on('pageerror',e=>errors.push(e.message));p.on('request',r=>{if(/^https?:/.test(r.url())&&!r.url().startsWith(base+'/'))remote.push(r.url());});
  await p.addInitScript(()=>{window.showSaveFilePicker=undefined;window.copyPrompts=[];new MutationObserver(()=>{for(const d of document.querySelectorAll('.pdf-compatible-dialog:not([data-progress])')){const t=d.querySelector('h2')?.textContent;if(t&&!window.copyPrompts.includes(t))window.copyPrompts.push(t);}}).observe(document,{childList:true,subtree:true});});
  const file=path.join(fixtures,name+'.pdf'),before=hash(file),prefix=tool==='merge'?'merge':'pdf',primary=tool==='merge'?'#merge-pick':'#pdf-empty-pick';
  await p.goto(base+'/alat/'+tool);assert.equal(await p.locator('#merge-sources,#merge-google,#merge-dropbox').count(),0);
  if(name==='signature'){await pick(p,primary,[]);assert.equal(await p.locator('#'+prefix+'-run').isDisabled(),true);}
  const chooser=await pick(p,primary,tool==='merge'?[file,file]:file);assert.equal(chooser.isMultiple(),['merge','organize'].includes(tool));await loaded(p,tool);
  if(['extract','remove','rotate'].includes(tool))await p.locator('#pdf-ranges').fill(tool==='remove'?'2':'1');
  if(tool==='rotate')await p.locator('#pdf-angle').selectOption('90');
  for(const [key,value]of Object.entries(options[tool]||{})){const field=p.locator('[data-option="'+key+'"]'),details=field.locator('xpath=ancestor::details[1]');if(await details.count()&&!await details.evaluate(e=>e.open))await details.locator('summary').click();if(await field.evaluate(e=>e.tagName==='SELECT'))await field.selectOption(String(value));else await field.fill(String(value));}
  await ready(p,prefix);assert.deepEqual(await p.evaluate(()=>window.copyPrompts),[]);
  await p.locator('#'+prefix+'-run').click();await p.waitForFunction(id=>document.getElementById(id).hidden===false,prefix+'-result',{timeout:15000});
  const download=p.waitForEvent('download');await p.locator('#'+prefix+(tool==='split'?'-save-all':'-save')).click();await(await download).saveAs(path.join(output,tool+'-'+name+'.pdf'));
  assert.equal(hash(file),before);assert.deepEqual(errors,[]);assert.deepEqual(remote,[]);results.push({tool,name,pass:true,nativePicker:true,copyPrompts:0,sourceUnchanged:true});
  if(tool==='split'&&name==='signature'){
   await pick(p,'#pdf-result-new',path.join(fixtures,'form.pdf'));await ready(p,'pdf');assert.deepEqual(await p.evaluate(()=>window.copyPrompts),[]);results.push({case:'split replace from results via device picker',pass:true});
  }
  await context.close();console.log(tool+' / '+name+': OK');
 }
 // All PDF workspaces receive signed/form files from drag and drop as well.
 for(const tool of tools){const context=await browser.newContext(),p=await context.newPage();await p.goto(base+'/alat/'+tool);await drop(p,tool==='merge'?[path.join(fixtures,'signature.pdf'),path.join(fixtures,'form.pdf')]:[path.join(fixtures,'form.pdf')]);await loaded(p,tool);assert.equal(await p.locator('.pdf-compatible-dialog form').count(),0);results.push({case:'drop '+tool,pass:true});await context.close();}
 // Additional files remain a single device-picker click for multi-file tools.
 for(const tool of ['merge','organize','images-pdf']){const context=await browser.newContext(),p=await context.newPage();await p.goto(base+'/alat/'+tool);const image=tool==='images-pdf',file=path.join(fixtures,image?'scan-1.png':'signature.pdf'),prefix=tool==='merge'?'merge':'pdf';await pick(p,tool==='merge'?'#merge-pick':'#pdf-empty-pick',file);await loaded(p,tool);await pick(p,'#'+prefix+'-add',path.join(fixtures,image?'scan-2.png':'form.pdf'));await loaded(p,tool);await ready(p,prefix);results.push({case:'add '+tool,pass:true});await context.close();}
 const context=await browser.newContext({acceptDownloads:true}),p=await context.newPage();await p.addInitScript(()=>window.showSaveFilePicker=undefined);await p.goto(base+'/alat/images-pdf');const c=await pick(p,'#pdf-empty-pick',path.join(fixtures,'scan-1.png'));assert.equal(c.isMultiple(),true);assert.match(await p.locator('#pdf-files').getAttribute('accept'),/png/);await ready(p,'pdf');await p.locator('#pdf-run').click();await p.waitForFunction(()=>document.querySelector('#pdf-result').hidden===false);const d=p.waitForEvent('download');await p.locator('#pdf-save').click();await(await d).saveAs(path.join(output,'images-pdf.pdf'));results.push({case:'image device picker and download',pass:true});await context.close();
}finally{fs.writeFileSync(path.join(output,'browser-results.json'),JSON.stringify(results,null,2));await browser.close();}
assert.equal(results.length,59);console.log('59 browser checks passed');
})().catch(e=>{console.error(e);process.exitCode=1;});
