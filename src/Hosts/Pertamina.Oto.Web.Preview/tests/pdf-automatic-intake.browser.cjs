/* Usage: set OTO_PDF_AUTO_FIXTURES, OTO_PDF_TEST_OUTPUT, OTO_PLAYWRIGHT_MODULE.
 * Synthetic fixtures: signature, empty-signature, form, blank-form,
 * scan-two-signatures; each has two pages. Run against the local Preview host. */
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const fixtures=process.env.OTO_PDF_AUTO_FIXTURES,output=process.env.OTO_PDF_TEST_OUTPUT;
assert.ok(fixtures&&output,'Set synthetic fixture and evidence directories');
const {chromium}=require(process.env.OTO_PLAYWRIGHT_MODULE||'playwright');
const base=process.env.OTO_LOCAL_BASE||'http://127.0.0.1:5278',hash=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
fs.mkdirSync(output,{recursive:true});
(async()=>{
const browser=await chromium.launch({channel:'msedge',headless:true}),results=[];
try{
for(const tool of ['split','merge','organize'])for(const name of ['signature','empty-signature','form','blank-form','scan-two-signatures']){
 const context=await browser.newContext({acceptDownloads:true,viewport:{width:1440,height:1000}}),p=await context.newPage(),errors=[],network=[];
 p.on('pageerror',e=>errors.push(e.message));p.on('request',r=>{if(/^https?:/.test(r.url())&&!r.url().startsWith(base+'/'))network.push(r.url());});
 await p.addInitScript(()=>{window.showSaveFilePicker=undefined;window.copyPrompts=[];new MutationObserver(()=>{for(const d of document.querySelectorAll('.pdf-compatible-dialog:not([data-progress])')){const text=d.querySelector('h2')?.textContent;if(text&&!window.copyPrompts.includes(text))window.copyPrompts.push(text);}}).observe(document,{childList:true,subtree:true});});
 const input=path.join(fixtures,name+'.pdf'),before=hash(input),prefix=tool==='merge'?'merge':'pdf';
 await p.goto(base+'/alat/'+tool);
 await p.locator('#'+prefix+'-files').setInputFiles(tool==='merge'?[input,input]:input);
 await p.waitForFunction(id=>!document.getElementById(id).disabled,prefix+'-run',{timeout:15000});
 assert.deepEqual(await p.evaluate(()=>window.copyPrompts),[],'Signed/form PDF must enter without a copy confirmation');
 await p.locator('#'+prefix+'-run').click();await p.waitForFunction(id=>document.getElementById(id).hidden===false,prefix+'-result',{timeout:15000});
 if(tool==='split'&&name==='signature')await p.screenshot({path:path.join(output,'accepted-signed-result.png')});
 const download=p.waitForEvent('download');await p.locator('#'+prefix+(tool==='split'?'-save-all':'-save')).click();await(await download).saveAs(path.join(output,tool+'-'+name+'.pdf'));
 assert.equal(hash(input),before,'original file changed');assert.deepEqual(errors,[]);assert.deepEqual(network,[]);
 results.push({tool,name,pass:true,confirmations:0,sourceUnchanged:true,networkRequests:network.length});await context.close();
 console.log(tool+' / '+name+': accepted and downloaded');
}
// A worker that has not started is cancellable; releasing the origin lock
// lets a second tab accept its form without any confirmation.
const context=await browser.newContext(),a=await context.newPage(),b=await context.newPage();let release;const gate=new Promise(r=>release=r);
await a.route('**/js/pdf-compatible-worker.js',async r=>{await gate;await r.continue().catch(()=>{});});
for(const p of [a,b])await p.goto(base+'/alat/split');
await a.locator('#pdf-files').setInputFiles(path.join(fixtures,'signature.pdf'));await a.locator('.pdf-compatible-dialog[data-progress]').waitFor();
await b.locator('#pdf-files').setInputFiles(path.join(fixtures,'form.pdf'));await b.waitForFunction(()=>document.querySelector('[data-browser-tool]').getAttribute('aria-busy')==='false');
assert.equal(await b.locator('#pdf-run').isDisabled(),true);assert.match(await b.locator('#pdf-feedback').innerText(),/pekerjaan lain/);
await a.locator('#pdf-compat-cancel').click();release();await a.waitForFunction(()=>document.querySelector('[data-browser-tool]').getAttribute('aria-busy')==='false');
assert.equal(await a.locator('#pdf-run').isDisabled(),true);
await b.locator('#pdf-files').setInputFiles(path.join(fixtures,'form.pdf'));await b.waitForFunction(()=>!document.querySelector('#pdf-run').disabled);
results.push({case:'automatic preparation cancel and cross-tab retry',pass:true});await context.close();
}finally{fs.writeFileSync(path.join(output,'browser-results.json'),JSON.stringify(results,null,2));await browser.close();}
assert.equal(results.length,16);console.log('16 browser checks passed');
})().catch(e=>{console.error(e);process.exitCode=1;});
