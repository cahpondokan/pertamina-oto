const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const {readerRegion,rasterVariant,READER_PIXELS,READER_EDGE,LIMIT}=require('../wwwroot/js/pdf-preview.js');
test('reader supplies screen pixels at 50–300% zoom and common DPI without a scale-2 ceiling',()=>{
 for(const zoom of [.5,1,1.2,2,3])for(const dpr of [1,1.25,1.5,2]){
  const w=Math.round(500*zoom),h=Math.round(w*1.414),r=readerRegion(w,h,0,0,1180,620,dpr);
  assert.ok(Math.abs(r.pixelWidth/r.width-dpr)<.01);assert.ok(Math.abs(r.pixelHeight/r.height-dpr)<.01);
 }
 const r=readerRegion(1500,2121,250,600,1100,620,2);assert.ok(r.pixelWidth>1400);assert.ok(r.y>0);assert.ok(r.height<2121);
});
test('visible crops cover scrolled viewport, stay in page, and ignore offscreen pages',()=>{
 for(const [w,h]of [[500,707],[1500,2121],[1500,1061],[390,10000]])for(const left of [0,100,w-10])for(const top of [0,234,h-10]){
  const r=readerRegion(w,h,left,top,380,600,2);assert.ok(r.x<=left&&r.y<=top);assert.ok(r.x+r.width>=Math.min(w,left+380));assert.ok(r.y+r.height>=Math.min(h,top+600));assert.ok(r.x+r.width<=w&&r.y+r.height<=h);
 }
 assert.equal(readerRegion(500,700,0,701,500,600),null);assert.equal(readerRegion(500,700,0,-601,500,600),null);
});
test('extreme viewport/DPI uses bounded canvas dimensions and cache bytes',()=>{
 for(const [w,h,dpr]of [[2280,3224,4],[10000,12000,10],[100000,10,100],[10,100000,100]]){
  const r=readerRegion(w,h,0,0,w,h,dpr);assert.ok(r.pixelWidth*r.pixelHeight<=READER_PIXELS);assert.ok(r.pixelWidth<=READER_EDGE&&r.pixelHeight<=READER_EDGE);assert.ok(r.pixelWidth*r.pixelHeight*4<=LIMIT);
 }
});
test('raster identity includes zoom, DPI and scroll crop; tiny scrolls reuse the same region',()=>{
 const a=readerRegion(1500,2121,0,0,1000,600,1),b=readerRegion(1500,2121,0,1,1000,600,1);
 assert.equal(rasterVariant(a),rasterVariant(b));
 for(const r of [readerRegion(1500,2121,0,0,1000,600,2),readerRegion(1500,2121,0,500,1000,600,1),readerRegion(1000,1414,0,0,1000,600,1)])assert.notEqual(rasterVariant(a),rasterVariant(r));
});
test('invalid reader size and DPI are rejected before allocating pixels',()=>{
 for(const n of [NaN,Infinity,0,-1])assert.throws(()=>readerRegion(500,700,0,0,500,600,n),/PREVIEW_SIZE/);
 assert.throws(()=>readerRegion(500,700,NaN,0,500,600),/PREVIEW_SIZE/);
});

const tick=()=>new Promise(resolve=>setImmediate(resolve));
function harness({delayPage=false,delayBlob=false}={}){
 const state={canvases:[],renders:[],urls:[],revoked:[],cleanups:0,terminated:0,loads:0,bytes:null};let pageResolve;
 const pdfPage={rotate:0,getViewport:({scale})=>({width:600*scale,height:800*scale,scale}),cleanup(){state.cleanups++;},render(options){let resolve,reject;const promise=new Promise((a,b)=>{resolve=a;reject=b;});const task={promise,cancel(){state.cancelled=(state.cancelled||0)+1;reject(Error('RenderingCancelledException'));}};state.renders.push({options,resolve,reject,task});return task;}};
 const pdfDoc={getPage:async()=>delayPage?new Promise(resolve=>{pageResolve=resolve;}):pdfPage};
 const ctx={console,Blob,Uint8Array,setTimeout,clearTimeout,Worker:class{terminate(){state.terminated++;}},URL:{createObjectURL(){const url='blob:'+state.urls.length;state.urls.push(url);return url;},revokeObjectURL(url){state.revoked.push(url);}},document:{createElement(tag){assert.equal(tag,'canvas');const canvas={width:0,height:0,getContext:()=>({}),toBlob(cb){if(delayBlob)state.finishBlob=()=>cb(new Blob(['pixels']));else cb(new Blob(['pixels']));}};state.canvases.push(canvas);return canvas;}},pdfjs:{PDFWorker:class{destroy(){}},getDocument(options){state.bytes=options.data;state.loads++;return {promise:Promise.resolve(pdfDoc),destroy:async()=>{pageResolve?.(pdfPage);}};}}};
 ctx.window=ctx;
 const source=fs.readFileSync(path.join(__dirname,'../wwwroot/js/pdf-preview.js'),'utf8').replace("import(BASE+'build/pdf.min.mjs')",'Promise.resolve(globalThis.pdfjs)').replace("import(BASE+'binary-data.mjs')",'Promise.resolve({})').replace('root.OtoPdfPreview={create};','root.testing={raster,stop,cancelStale,cache};');
 vm.runInNewContext(source,ctx);return {...ctx.testing,state,pageReady:()=>pageResolve(pdfPage)};
}
test('cancel while getPage is pending allocates no canvas and preserves original bytes',async()=>{
 const h=harness({delayPage:true}),bytes=new Uint8Array([1,2,3]);let valid=true;
 const pending=h.raster({bytes},0,0,readerRegion(1500,2000,0,0,1000,600,2),()=>valid);const rejected=assert.rejects(pending,/PREVIEW_CANCELLED/);
 await tick();valid=false;h.cancelStale();h.pageReady();await rejected;assert.equal(h.state.canvases.length,0);assert.notEqual(h.state.bytes,bytes);assert.deepEqual([...bytes],[1,2,3]);await h.stop();
});
test('stale render is cancelled before next render; zoom crop transform and zeroed canvas are correct',async()=>{
 const h=harness(),bytes=new Uint8Array([1,2,3]);let valid=true;
 const pending=h.raster({bytes},0,0,true,()=>valid),rejected=assert.rejects(pending,/Cancelled/);await tick();valid=false;h.cancelStale();
 const r=readerRegion(1500,2000,200,400,1000,600,2),fresh=h.raster({bytes},0,0,r);await rejected;await tick();assert.equal(h.state.renders.length,2);assert.equal(h.state.loads,1);
 const render=h.state.renders[1];assert.equal(render.options.viewport.scale,2.5);assert.deepEqual([...render.options.transform],[2,0,0,2,-r.x*2||0,-r.y*2||0]);render.resolve();const result=await fresh;
 assert.equal(result.width,r.pixelWidth);assert.equal(h.state.cancelled,1);assert.ok(h.state.canvases.every(c=>c.width===0&&c.height===0));assert.equal(h.state.cleanups,2);await h.stop();h.cache.clear();assert.deepEqual(h.state.revoked,h.state.urls);
});
test('close during PNG encoding publishes no stale object URL or cache entry',async()=>{
 const h=harness({delayBlob:true}),pending=h.raster({bytes:new Uint8Array([1])},0),rejected=assert.rejects(pending,/PREVIEW_CANCELLED/);await tick();h.state.renders[0].resolve();await tick();await h.stop();h.state.finishBlob();await rejected;
 assert.equal(h.state.urls.length,0);assert.equal(h.cache.bytes,0);assert.equal(h.state.canvases[0].width,0);assert.equal(h.state.terminated,1);
});
test('queued obsolete requests do not render; identical live crop is cached',async()=>{
 const h=harness(),source={bytes:new Uint8Array([1])},r=readerRegion(1500,2000,0,0,1000,600,2),first=h.raster(source,0,0,r);
 const obsolete=h.raster(source,1,0,r,()=>false),rejected=assert.rejects(obsolete,/PREVIEW_CANCELLED/);await tick();h.state.renders[0].resolve();const a=await first;await rejected;assert.equal((await h.raster(source,0,0,r)).url,a.url);assert.equal(h.state.renders.length,1);await h.stop();h.cache.clear();
});
test('renderer failure frees pixels and next request succeeds without poisoning the queue',async()=>{
 const h=harness(),source={bytes:new Uint8Array([1])},failed=h.raster(source,0),rejected=assert.rejects(failed,/RENDER_FAILURE/);await tick();h.state.renders[0].reject(Error('RENDER_FAILURE'));await rejected;
 assert.equal(h.state.urls.length,0);assert.equal(h.state.canvases[0].width,0);assert.equal(h.cache.bytes,0);
 const next=h.raster(source,1);await tick();h.state.renders[1].resolve();await next;assert.equal(h.state.urls.length,1);assert.equal(h.state.cleanups,2);await h.stop();h.cache.clear();
});
