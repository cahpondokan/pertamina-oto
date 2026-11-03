const test=require('node:test'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const D=require('../wwwroot/js/web-diagnostics.js');
const {mount}=require('./helpers/paste-controller.cjs');
function harness(response=async()=>({status:204})){
 const requests=[],timers=new Map();let number=0;
 const env={crypto,AbortController,navigator:{onLine:true,userAgent:'Chrome/999 SECRET_UA'},setTimeout(fn,delay){timers.set(++number,{fn,delay});return number;},clearTimeout:id=>timers.delete(id),fetch:async(url,options)=>{requests.push({url,options});return response();}};
 const client=D.create(env);return {client,env,requests,timers,events:()=>requests.flatMap(r=>JSON.parse(r.options.body).events)};
}
test('strict outbound projection never serializes error payload, document, name, URL, stack or UA',async()=>{
 const h=harness(),scope=h.client.begin('merge');
 scope.fail(Object.assign(new TypeError('SECRET_PDF_TEXT filename.pdf C:\\private\\file.pdf token=SECRET'),{stack:'SECRET_STACK',file:new Blob(['%PDF SECRET_CONTENT'])}));
 await h.client.flush();const e=h.events();assert.equal(e.length,2);assert.equal(e[1].code,'TYPE_ERROR');assert.equal(e[1].event,'failed');
 const wire=JSON.stringify(h.requests);assert.doesNotMatch(wire,/SECRET|filename|private|%PDF|stack|userAgent/);assert.equal(e[0].processId,e[1].processId);
 const req=h.requests[0];assert.equal(req.url,'/api/diagnostics/events');assert.equal(req.options.credentials,'omit');assert.equal(req.options.redirect,'error');assert.equal(req.options.referrerPolicy,'no-referrer');assert.equal(req.options.mode,'same-origin');
 assert.deepEqual(Object.keys(e[0]).sort(),'id processId event feature stage code elapsedMs occurredAt clientVersion browser'.split(' ').sort());
});
test('success/cancel/failure finish once; unrelated work uses different random IDs',async()=>{
 const h=harness(),a=h.client.begin('split'),b=h.client.begin('rotate');a.success();a.fail(Error('VERIFY'));b.fail(new DOMException('secret','AbortError'));await h.client.flush();
 assert.equal(h.events().length,4);assert.equal(h.events()[3].code,'CANCELLED');assert.notEqual(h.events()[0].processId,h.events()[1].processId);
});
test('offline work completes and reconnect sends exact same queued events',async()=>{
 const h=harness();h.env.navigator.onLine=false;h.client.begin('merge').success();await h.client.flush();assert.equal(h.requests.length,0);assert.equal(h.client.status().queued,2);
 h.env.navigator.onLine=true;await h.client.flush();assert.equal(h.events().length,2);assert.equal(h.client.status().queued,0);
});
test('lost acknowledgement reuses event IDs; bounded retries discard only failed batch',async()=>{
 const h=harness(async()=>{throw Error('SECRET_NETWORK');});h.client.begin('merge').success();await h.client.flush();await h.client.flush();assert.deepEqual(JSON.parse(h.requests[0].options.body),JSON.parse(h.requests[1].options.body));
 for(let n=0;n<4;n++)await h.client.flush();assert.equal(h.client.status().queued,0);assert.equal(h.client.status().dropped,2);assert.equal(h.requests.length,6);
});
test('flood, invalid enums and batch limits are bounded',async()=>{
 const h=harness();h.client.begin('SECRET_FEATURE','SECRET_STAGE').fail(Error('SECRET'));assert.equal(h.client.status().queued,0);
 for(let n=0;n<200;n++)h.client.runtime(Error('SECRET'));assert.equal(h.client.status().queued,128);assert.equal(h.client.status().dropped,72);
 await h.client.flush();assert.equal(h.events().length,16);assert.ok(h.requests[0].options.body.length<12288);h.client.dispose();assert.equal(h.client.status().queued,0);
});
test('client validation rejection is not retried and unknown exceptions use fixed code',async()=>{
 const h=harness(async()=>({status:400}));h.client.runtime({name:'SECRET',message:'SECRET'});await h.client.flush();assert.equal(h.events()[0].code,'UNKNOWN');assert.equal(h.client.status().dropped,1);await h.client.flush();assert.equal(h.requests.length,1);
});
test('diagnostics code has no document serialization or upload transport',()=>{
 const source=require('node:fs').readFileSync(require.resolve('../wwwroot/js/web-diagnostics.js'),'utf8');
 for(const token of ['FileReader','arrayBuffer(','FormData','sendBeacon','XMLHttpRequest','localStorage','sessionStorage','.stack','location.href','document.cookie'])assert.ok(!source.includes(token),token);
});
test('hostile error property names and throwing getters never escape closed error codes',()=>{
 for(const name of ['constructor','__proto__','toString','PRIVATE'])assert.equal(D.code({name,message:'PRIVATE'}),'UNKNOWN');
 assert.equal(D.code({get name(){throw Error('PRIVATE');}}),'UNKNOWN');
 const h=harness();h.env.crypto={randomUUID(){throw Error('unavailable');}};assert.doesNotThrow(()=>h.client.runtime(Error('PRIVATE')));assert.doesNotThrow(()=>h.client.begin('merge').success());assert.equal(h.client.status().queued,0);
});
test('oversize input is recorded by the real controller without reading its bytes',async()=>{
 const h=await mount('rotate'),d=harness();h.context.OtoDiagnostics=d.client;
 const huge=new File(['small'],'PRIVATE.pdf',{type:'application/pdf'});Object.defineProperty(huge,'size',{value:4000000001});h.paste([huge]);await h.idle();await d.client.flush();assert.equal(h.jobs(),0);assert.ok(d.events().some(e=>e.code==='LIMIT'));h.controller.dispose();d.client.dispose();
});
for(const tool of ['merge','organize','split','remove','extract','rotate','numbers','watermark','crop'])test(tool+': real controller/engine audit and local result, no source or options in network',async()=>{
 const h=await mount(tool),d=harness();h.context.OtoDiagnostics=d.client;
 const doc=await h.P.PDFDocument.create();for(let i=0;i<4;i++)doc.addPage().drawText('PRIVATE_CONTENT_CANARY');
 const file=new File([await doc.save({useObjectStreams:false})],'PRIVATE_FILENAME_CANARY.pdf',{type:'application/pdf'});
 h.paste(tool==='merge'?[file,file]:[file]);await h.idle();
 if(tool==='split')h.$('pdf-ranges').value='1-2;3-4';if(['remove','extract','rotate'].includes(tool))h.$('pdf-ranges').value='2';
 const prefix=tool==='merge'?'merge':'pdf';await h.$(prefix+'-run').click();await h.idle();assert.ok(h.buffers().length,h.feedback());
 h.context.showSaveFilePicker=async()=>{throw new DOMException('PRIVATE_PICKER_CANARY','AbortError');};await h.$(prefix+'-save').click();
 h.$(prefix+'-download').click();const written=[];h.context.showSaveFilePicker=async()=>({createWritable:async()=>new WritableStream({write:b=>written.push(b)})});await h.$(prefix+'-save').click();
 await d.client.flush();const events=d.events(),wire=JSON.stringify(d.requests);assert.doesNotMatch(wire,/PRIVATE_|%PDF|\.pdf|DRAF|1-2;3-4/);
 assert.ok(events.some(e=>e.event==='succeeded'&&e.stage==='process'));assert.ok(events.some(e=>e.event==='save_cancelled'));assert.ok(events.some(e=>e.event==='download_requested'));assert.ok(events.some(e=>e.event==='save_succeeded'));
 const ids=new Set(events.filter(e=>e.stage==='process'||e.stage==='save').map(e=>e.processId));assert.equal(ids.size,1);assert.ok(Buffer.concat(written).includes(Buffer.from('%PDF')));h.controller.dispose();d.client.dispose();await h.idle();
});
test('real processing failure retains reason code and input without exposing exception text',async()=>{
 const h=await mount('merge'),d=harness();h.context.OtoDiagnostics=d.client;h.paste([h.file,h.file]);await h.idle();
 h.context.OtoMergeJob.create=async()=>{throw new DOMException('PRIVATE_STORAGE_LOCATION','QuotaExceededError');};await h.$('merge-run').click();await h.idle();await d.client.flush();assert.ok(d.events().some(e=>e.event==='failed'&&e.code==='QUOTA'));assert.doesNotMatch(JSON.stringify(d.requests),/PRIVATE_STORAGE/);assert.equal(h.buffers().length,0);h.controller.dispose();d.client.dispose();
});
