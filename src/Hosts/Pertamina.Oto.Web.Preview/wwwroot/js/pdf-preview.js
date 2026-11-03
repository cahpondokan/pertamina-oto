/* Read-only PDF raster previews. No PDF writes, uploads, viewer scripting or URL input.
 * One active renderer, bounded LRU rasters, lazy visible pages, cancellable workers.
 * PDF.js receives a COPY: original buffers remain owned by the validated workspace. */
(function(root){
 'use strict';
 const BASE='/vendor/pdfjs-6.4.299/',LIMIT=24*1024*1024,MAX_ENTRIES=64;
 function dimensions(width,height,maxWidth,maxHeight){
  if(![width,height,maxWidth,maxHeight].every(n=>Number.isFinite(n)&&n>0))throw Error('PREVIEW_SIZE');
  const scale=Math.min(maxWidth/width,maxHeight/height,2);
  return {scale,width:Math.max(1,Math.min(Math.floor(maxWidth),Math.ceil(width*scale))),height:Math.max(1,Math.min(Math.floor(maxHeight),Math.ceil(height*scale)))};
 }
 // Render only the visible part of a zoomed page, with a quantized scroll margin.
 // A reader raster fits the existing 24 MiB cache even on extreme DPI/screens.
 const READER_PIXELS=6*1024*1024,READER_EDGE=4096;
 function readerRegion(width,height,left,top,viewWidth,viewHeight,dpr=1){
  if(![width,height,viewWidth,viewHeight,dpr].every(n=>Number.isFinite(n)&&n>0)||![left,top].every(Number.isFinite))throw Error('PREVIEW_SIZE');
  if(left>=width||top>=height||left+viewWidth<=0||top+viewHeight<=0)return null;
  const step=128,x=Math.max(0,Math.floor(left/step)*step-step),y=Math.max(0,Math.floor(top/step)*step-step);
  const right=Math.min(width,Math.ceil((left+viewWidth)/step)*step+step),bottom=Math.min(height,Math.ceil((top+viewHeight)/step)*step+step);
  const w=right-x,h=bottom-y,ratio=Math.min(dpr,READER_EDGE/w,READER_EDGE/h,Math.sqrt(READER_PIXELS/(w*h)));
  const pixelWidth=Math.max(1,Math.floor(w*ratio)),pixelHeight=Math.max(1,Math.floor(h*ratio));
  return {x,y,width:w,height:h,pixelWidth,pixelHeight,displayWidth:width,displayHeight:height};
 }
 function rasterVariant(request){return typeof request==='object'?JSON.stringify(request):request?'large':'card';}
 class RasterCache{
  constructor(limit=LIMIT,max=MAX_ENTRIES,release=()=>{}){this.limit=limit;this.max=max;this.release=release;this.items=new Map();this.bytes=0;}
  get(key){const v=this.items.get(key);if(v){this.items.delete(key);this.items.set(key,v);}return v;}
  set(key,value){if(value.cost>this.limit)return false;this.delete(key);this.items.set(key,value);this.bytes+=value.cost;while(this.bytes>this.limit||this.items.size>this.max)this.delete(this.items.keys().next().value);return true;}
  delete(key){const v=this.items.get(key);if(v){this.items.delete(key);this.bytes-=v.cost;this.release(v);}}
  clear(){for(const key of this.items.keys())this.delete(key);}
 }
 // A reader may show one PDF or an ordered view of pages from several PDFs.
 // Reading rotation is added to each page's own edit rotation, never to its source.
 function readerPage(source,index,viewRotation=0){
  const page=source.pages?.[index]||{source,index,rotation:source.rotation||0};
  return {...page,rotation:((page.rotation||0)+viewRotation)%360};
 }
 function readerLayout(source,width,viewRotation=0){
  let top=0;return Array.from({length:source.count},(_,index)=>{
   const page=readerPage(source,index,viewRotation),geometry=page.source.geometry?.[page.index];
   const ratio=geometry&&geometry.width>0&&geometry.height>0?geometry.height/geometry.width:1.414;
   const angle=(page.rotation+(geometry?.rotation||0))%180;
   const height=Math.round(width*(angle?1/ratio:ratio)),layout={top,height,...page};top+=height+12;return layout;
  });
 }
 if(typeof module!=='undefined'){module.exports={readerPage,readerLayout,dimensions,readerRegion,rasterVariant,RasterCache,LIMIT,MAX_ENTRIES,READER_PIXELS,READER_EDGE};return;}
 let library,resources,documentJob=null,renderTask=null,abortRender=null,renderValid=null,sequence=0,id=0,dialogId=0,serial=Promise.resolve();
 const ids=new WeakMap(),cache=new RasterCache(LIMIT,MAX_ENTRIES,v=>URL.revokeObjectURL(v.url));
 function key(bytes,page,rotation,request){if(!ids.has(bytes))ids.set(bytes,++id);return `${ids.get(bytes)}:${page}:${rotation}:${rasterVariant(request)}`;}
 async function closeDocument(){
  const old=documentJob;documentJob=null;
  if(old){let timer;try{await Promise.race([old.loading.destroy().catch(()=>{}),new Promise(r=>{timer=setTimeout(r,200);})]);}finally{clearTimeout(timer);old.worker.destroy();old.native.terminate();}}
 }
 function stop(){
  sequence++;abortRender?.();renderTask=null;
  return closeDocument();
 }
 function cancelStale(){if(renderValid&&!renderValid())abortRender?.();}
 async function load(bytes){
  if(documentJob?.bytes===bytes)return documentJob.loading.promise;
  const ticket=sequence;
  await closeDocument();
  library ||= import(BASE+'build/pdf.min.mjs');resources ||= import(BASE+'binary-data.mjs');
  const [pdfjs,binary]=await Promise.all([library,resources]);
  let input;
  if(bytes instanceof Blob){
   const initial=new Uint8Array(await bytes.slice(0,65536).arrayBuffer());
   if(ticket!==sequence)throw Error('PREVIEW_CANCELLED');
   const transport=new pdfjs.PDFDataRangeTransport(bytes.size,initial);
   let aborted=false;transport.abort=()=>{aborted=true;};
   transport.requestDataRange=(begin,end)=>{void bytes.slice(begin,end).arrayBuffer().then(data=>{if(!aborted)transport.onDataRange(begin,new Uint8Array(data));}).catch(()=>{if(!aborted)void stop();});};
   input={range:transport,length:bytes.size,rangeChunkSize:65536,disableRange:false};
  }else input={data:new Uint8Array(bytes.slice(0)),disableRange:true};
  if(ticket!==sequence)throw Error('PREVIEW_CANCELLED');
  const native=new Worker(BASE+'build/pdf.worker.min.mjs',{type:'module'});
  const worker=new pdfjs.PDFWorker({port:native});
  class LocalBinaryData{fetch({kind,filename}){return binary.read(kind,filename);}}
  const loading=pdfjs.getDocument({
   worker,BinaryDataFactory:LocalBinaryData,
   useWorkerFetch:false,useWasm:false,disableFontFace:true,useSystemFonts:true,
   enableXfa:false,enableScripting:false,isEvalSupported:false,stopAtErrors:true,
   disableAutoFetch:true,disableStream:true,...input,
   isImageDecoderSupported:false,isOffscreenCanvasSupported:false,
   maxImageSize:32*1024*1024,canvasMaxAreaInBytes:8*1024*1024,
   wasmUrl:BASE+'wasm/',cMapPacked:true,verbosity:0
  });
  documentJob={bytes,native,worker,loading};
  return loading.promise;
 }
 function raster(source,index,rotation=0,large=false,valid=()=>true){
  const run=async()=>{
   if(!valid())throw Error('PREVIEW_CANCELLED');
   const cacheKey=key(source.bytes,index,rotation,large),hit=cache.get(cacheKey);if(hit)return hit;
   let canvas,page,timer,cancel,task,cancelled=false,timedOut=false;
   const check=()=>{if(cancelled||!valid())throw Error(timedOut?'PREVIEW_TIMEOUT':'PREVIEW_CANCELLED');};
   try{
     cancel=()=>{if(cancelled)return;cancelled=true;task?.cancel();};abortRender=cancel;renderValid=valid;
     timer=setTimeout(()=>{timedOut=true;void stop();},15000);
     const doc=await load(source.bytes);check();
     const token=sequence;page=await doc.getPage(index+1);check();
     const original=page.getViewport({scale:1,rotation:(page.rotate+rotation)%360});
     const region=typeof large==='object'?large:null;
     const size=region?{width:region.pixelWidth,height:region.pixelHeight,scale:Math.min(region.displayWidth/original.width,region.displayHeight/original.height)}:dimensions(original.width,original.height,large?1400:420,large?1600:520);
     canvas=document.createElement('canvas');canvas.width=size.width;canvas.height=size.height;
     const context=canvas.getContext('2d',{alpha:false});
     const transform=region?[region.pixelWidth/region.width,0,0,region.pixelHeight/region.height,((region.displayWidth-original.width*size.scale)/2-region.x)*region.pixelWidth/region.width,((region.displayHeight-original.height*size.scale)/2-region.y)*region.pixelHeight/region.height]:undefined;
     task=page.render({canvasContext:context,viewport:page.getViewport({scale:size.scale,rotation:(page.rotate+rotation)%360}),transform,background:'rgb(255,255,255)'});renderTask=task;
     await task.promise;check();
     if(token!==sequence)throw Error('PREVIEW_CANCELLED');
     const blob=await new Promise(resolve=>canvas.toBlob(resolve,'image/png'));
     check();if(!blob||token!==sequence)throw Error('PREVIEW_CANCELLED');
     const value={url:URL.createObjectURL(blob),width:canvas.width,height:canvas.height,cost:canvas.width*canvas.height*4};
     cache.set(cacheKey,value);return value;
   }finally{clearTimeout(timer);if(abortRender===cancel){abortRender=null;renderValid=null;}if(renderTask===task)renderTask=null;page?.cleanup();if(canvas){canvas.width=0;canvas.height=0;}}
  };
  const pending=serial.then(run,run);serial=pending.catch(()=>{});return pending;
 }
 function create(screen,options={}){
  if(options.merge||options.continuous)return createContinuous(screen,options);
  let generation=0,active=true,observer=null,items=[],current=null,returnFocus=null,dialogGeneration=0;
  const make=(tag,cls,text)=>{const e=document.createElement(tag);e.className=cls;if(text)e.textContent=text;return e;};
  const dialog=make('dialog','pdf-page-dialog'),title=make('h2','','Pratinjau halaman');title.id='pdf-real-preview-title-'+(++dialogId);dialog.setAttribute('aria-labelledby',title.id);
  const head=make('div','pdf-page-dialog-head'),close=make('button','button','Tutup');close.type='button';head.append(title,close);
  const description=make('p','pdf-hint','Pratinjau dokumen sumber. Tautan tidak dijalankan; file asli tidak berubah.');
  const body=make('div','pdf-page-dialog-body'),status=make('p','pdf-preview-status','Memuat halaman…');status.setAttribute('role','status');
  const picture=make('img','pdf-full-page');picture.alt='';picture.hidden=true;body.append(status,picture);
  const nav=make('div','pdf-page-dialog-nav'),previous=make('button','button','← Sebelumnya'),next=make('button','button','Berikutnya →'),position=make('span','');
  previous.type=next.type='button';nav.append(previous,position,next);dialog.append(head,description,body,nav);screen.append(dialog);
  function clearPicture(){picture.removeAttribute('src');picture.hidden=true;}
  async function showPage(){
   const token=++dialogGeneration;clearPicture();status.hidden=false;status.textContent='Memuat halaman…';
   const {source,index,rotation}=current;title.textContent=source.name;position.textContent=`Sumber ${source.label} · Halaman ${index+1} dari ${source.count}`;
   previous.disabled=index===0;next.disabled=index>=source.count-1;
   try{const image=await raster(source,index,rotation,true,()=>active&&screen.isConnected&&dialog.open&&token===dialogGeneration);if(token!==dialogGeneration)return;picture.src=image.url;picture.alt=`Halaman ${index+1}, sumber ${source.label}`;picture.hidden=false;status.hidden=true;}
   catch{if(token===dialogGeneration){status.textContent='Pratinjau belum dapat ditampilkan. File asli tetap utuh.';}}
  }
  function open(source,index,rotation,button){
   if(document.querySelector('dialog[open]'))return;
   returnFocus=button;current={source,index,rotation};dialog.showModal();close.focus();void showPage();
  }
  close.onclick=()=>dialog.close();dialog.addEventListener('close',()=>{dialogGeneration++;current=null;clearPicture();void stop();if(returnFocus?.isConnected)returnFocus.focus({preventScroll:true});});
  previous.onclick=()=>{if(current.index>0){current.index--;current.rotation=0;void showPage();}};
  next.onclick=()=>{if(current.index<current.source.count-1){current.index++;current.rotation=0;void showPage();}};
  async function paint(item,token){
   const {target,source,index,rotation}=item;
   const paintToken=++item.paintToken;
   const valid=()=>active&&screen.isConnected&&token===generation&&target.isConnected&&item.visible&&paintToken===item.paintToken;
   if(!valid())return;target.dataset.preview='loading';
   try{const value=await raster(source,index,rotation,false,valid);if(!valid())return;
    const image=make('img','');image.src=value.url;image.alt=`Halaman ${index+1} dokumen ${source.label}`;image.draggable=false;
    target.replaceChildren(image);target.dataset.preview='ready';
   }catch{if(valid()){target.replaceChildren(make('span','pdf-preview-status','Pratinjau tidak tersedia'));target.dataset.preview='error';}}
  }
  function observe(){
   observer?.disconnect();if(!active||!screen.isConnected)return;
   const token=generation;
   observer=new IntersectionObserver(entries=>{for(const entry of entries){
    const item=items.find(x=>x.target===entry.target);if(!item)continue;
    item.visible=entry.isIntersecting;
    if(item.visible)void paint(item,token);
    else{item.paintToken++;item.target.replaceChildren(make('span','pdf-preview-status','Memuat halaman…'));item.target.dataset.preview='pending';}
   }},{rootMargin:'160px'});
   for(const item of items)observer.observe(item.target);
  }
  function reset(){generation++;observer?.disconnect();for(const item of items){item.target.replaceChildren(make('span','pdf-preview-status','Memuat halaman…'));item.target.dataset.preview='pending';}return stop();}
  return {
   button(source,index,rotation=0){const target=make('button','pdf-paper-id pdf-real-preview');target.type='button';target.title='Perbesar halaman';target.setAttribute('aria-label',`Perbesar halaman ${index+1} dokumen ${source.label}`);target.draggable=false;target.append(make('span','pdf-preview-status','Memuat halaman…'));target.onclick=()=>open(source,index,rotation,target);return {target,source,index,rotation,visible:false,paintToken:0};},
   update(value){reset();items=value;observe();},
   activate(){active=true;observe();},
   deactivate(){active=false;reset();if(dialog.open)dialog.close();clearPicture();},
   clear(){const stopped=reset();items=[];cache.clear();return stopped;},
   dispose(){active=false;reset();items=[];dialog.remove();cache.clear();}
  };
 }
 function createContinuous(screen,options){
  const cards=create(screen),make=(tag,cls,value)=>{const el=document.createElement(tag);el.className=cls;if(value)el.textContent=value;return el;};
  const dialog=make('dialog','merge-reader'),frame=make('div','merge-reader-frame'),stage=make('div','merge-reader-stage'),extent=make('div','merge-reader-extent');
  const title=make('h2','merge-reader-title'),close=make('button','merge-reader-close','×'),bar=make('div','merge-reader-controls'),prev=make('button','','←'),next=make('button','','→'),page=document.createElement('input'),of=make('span'),minus=make('button','','−'),plus=make('button','','＋'),zoom=make('span'),fit=make('button','','Pas layar'),rotate=make('button','','↻'),rotateLeft=make('button','','↶');
  title.id='merge-reader-title-'+(++dialogId);dialog.setAttribute('aria-labelledby',title.id);close.setAttribute('aria-label','Tutup pratinjau');prev.setAttribute('aria-label','Halaman sebelumnya');next.setAttribute('aria-label','Halaman berikutnya');minus.setAttribute('aria-label','Perkecil');plus.setAttribute('aria-label','Perbesar');rotate.setAttribute('aria-label','Putar tampilan ke kanan');rotateLeft.setAttribute('aria-label','Putar tampilan ke kiri');rotate.dataset.tip='Putar arah baca 90° ke kanan. Hasil PDF tidak berubah';rotateLeft.dataset.tip='Putar arah baca 90° ke kiri. Hasil PDF tidak berubah';page.type='number';page.min='1';page.setAttribute('aria-label','Nomor halaman pratinjau');stage.tabIndex=0;stage.setAttribute('aria-label','Halaman PDF bergulir');
  for(const button of [close,prev,next,minus,plus,fit,rotateLeft,rotate])button.type='button';bar.append(prev,page,of,next,minus,zoom,plus,fit,rotateLeft,rotate);stage.append(extent);frame.append(title,close,stage,bar);dialog.append(frame);screen.append(dialog);
  let current=null,focus=null,active=true,disposed=false,scale=1,width=0,height=0,base=0,token=0,raf=0,currentIndex=0,viewRotation=0,anchorScroll=null,pages=new Map(),pointers=new Map(),pinch=null,layouts=[];
  function invalidate(closeDocument=true){token++;if(raf)cancelAnimationFrame(raf);raf=0;for(const value of pages.values())value.element.remove();pages.clear();extent.replaceChildren();cancelStale();if(closeDocument)void stop();cache.clear();}
  function controls(){if(document.activeElement!==page)page.value=currentIndex+1;of.textContent='/ '+current.count;zoom.textContent=Math.round(scale*100)+'%';prev.disabled=currentIndex===0;next.disabled=currentIndex===current.count-1;page.max=current.count;if(current.pages){const item=current.pages[currentIndex];title.textContent=`Susunan halaman · Sumber ${item.source.label} · Halaman ${item.index+1}`;}}
  function paint(){
   raf=0;if(!current||!dialog.open)return;if(anchorScroll===null||Math.abs(stage.scrollTop-anchorScroll)>1){anchorScroll=null;currentIndex=stage.scrollTop+stage.clientHeight>=extent.scrollHeight-2?current.count-1:Math.min(current.count-1,Math.max(0,Math.max(0,layouts.findIndex(p=>p.top+p.height>stage.scrollTop+Math.min(stage.clientHeight*.35,120)))));}controls();
   const wanted=new Set([currentIndex,currentIndex-1,currentIndex+1].filter(i=>i>=0&&i<current.count));
   for(const [index,value]of pages)if(!wanted.has(index)){value.element.remove();pages.delete(index);}
   const captured=token,source=current;
   for(const index of wanted){
    if(!pages.has(index)){
     const element=make('div','merge-reader-paper'),label=make('span','merge-reader-loading','Memuat halaman '+(index+1)+'…');element.style.top=layouts[index].top+'px';element.style.width=width+'px';element.style.height=layouts[index].height+'px';element.append(label);extent.append(element);pages.set(index,{element,label,request:null});
    }
    const item=pages.get(index),{element,label}=item;
    const region=readerRegion(width,layouts[index].height,stage.scrollLeft,stage.scrollTop-layouts[index].top,stage.clientWidth,stage.clientHeight,window.devicePixelRatio||1);
    const request=region?rasterVariant(region):null;if(request===item.request)continue;item.request=request;const revision=item.revision=(item.revision||0)+1;
    if(!region){element.replaceChildren(label);continue;}
    const valid=()=>active&&!disposed&&dialog.open&&captured===token&&pages.get(index)===item&&item.revision===revision;
    void raster(layouts[index].source,layouts[index].index,layouts[index].rotation,region,valid).then(value=>{if(!valid())return;const img=document.createElement('img');img.alt='Halaman '+(index+1)+' · '+source.name;img.style.position='absolute';img.style.left=region.x+'px';img.style.top=region.y+'px';img.style.width=region.width+'px';img.style.height=region.height+'px';img.style.objectFit='fill';img.src=value.url;element.replaceChildren(img);}).catch(()=>{if(valid()){label.textContent='Pratinjau halaman '+(index+1)+' belum dapat ditampilkan.';element.replaceChildren(label);}});
   }
   cancelStale();
   dialog.dataset.renderedPages=pages.size;
  }
  function schedule(){if(!raf)raf=requestAnimationFrame(paint);}
  function layout(anchor=currentIndex){
   if(!current)return;invalidate(false);base=Math.max(140,Math.min(innerWidth-40,(innerHeight-128)*.707,760));width=Math.round(base*scale);layouts=readerLayout(current,width,viewRotation);height=layouts[anchor]?.height||width*1.414;
   const outer=Math.min(innerWidth-24,width+14);frame.style.width=outer+'px';extent.style.width=width+'px';extent.style.height=(layouts.at(-1).top+layouts.at(-1).height)+'px';currentIndex=anchor;stage.scrollTop=layouts[anchor].top;anchorScroll=stage.scrollTop;schedule();
  }
  function jump(index){if(!current)return;currentIndex=Math.max(0,Math.min(current.count-1,index));page.value=currentIndex+1;stage.scrollTop=layouts[currentIndex].top;anchorScroll=stage.scrollTop;schedule();}
  function setZoom(value,clientX,clientY){if(!current)return;const r=stage.getBoundingClientRect(),x=clientX==null?r.width/2:clientX-r.left,y=clientY==null?r.height/2:clientY-r.top,old=scale,vertical=(stage.scrollTop+y)/width,horizontal=(stage.scrollLeft+x)/width;scale=Math.max(.5,Math.min(3,value));if(scale===old)return;layout(currentIndex);stage.scrollTop=vertical*width-y;stage.scrollLeft=horizontal*width-x;anchorScroll=stage.scrollTop;schedule();}
  async function open(source,target,index=0){if(!active||disposed)return;cards.deactivate();await stop();if(!active||disposed||!target.isConnected)return;current=source;focus=target;currentIndex=index;viewRotation=0;dialog.dataset.viewRotation='0';scale=1;title.textContent=source.name;dialog.showModal();layout(index);stage.focus();}
  close.onclick=()=>dialog.close();dialog.addEventListener('close',()=>{if(dialog.open)return;const sourceId=current?.id;invalidate();current=null;pointers.clear();pinch=null;dialog.dataset.renderedPages='0';if(active&&!disposed)cards.activate();const target=focus?.isConnected?focus:screen.querySelector(`[data-source="${sourceId}"] .pdf-real-preview`);target?.focus({preventScroll:true});focus=null;});
  prev.onclick=()=>jump(currentIndex-1);next.onclick=()=>jump(currentIndex+1);page.onchange=()=>{const n=Number(page.value);if(Number.isInteger(n))jump(n-1);};page.onkeydown=e=>{if(e.key==='Enter'){e.preventDefault();page.onchange();}};minus.onclick=()=>setZoom(scale/1.2);plus.onclick=()=>setZoom(scale*1.2);fit.onclick=()=>setZoom(1);const turn=degrees=>{if(current){const anchor=currentIndex;viewRotation=(viewRotation+degrees+360)%360;dialog.dataset.viewRotation=String(viewRotation);layout(anchor);}};rotate.onclick=()=>turn(90);rotateLeft.onclick=()=>turn(-90);
  stage.addEventListener('scroll',schedule,{passive:true});stage.addEventListener('keydown',e=>{if(e.key==='PageDown'||e.key==='PageUp'){e.preventDefault();jump(currentIndex+(e.key==='PageDown'?1:-1));}if(e.key==='Home'){e.preventDefault();jump(0);}if(e.key==='End'){e.preventDefault();jump(current.count-1);}});
  stage.addEventListener('wheel',e=>{if(e.ctrlKey){e.preventDefault();setZoom(scale*Math.exp(-e.deltaY*.002),e.clientX,e.clientY);}},{passive:false});
  stage.addEventListener('pointerdown',e=>{if(e.pointerType==='mouse')return;pointers.set(e.pointerId,{x:e.clientX,y:e.clientY});stage.setPointerCapture(e.pointerId);if(pointers.size===2){const [a,b]=[...pointers.values()];pinch={distance:Math.hypot(a.x-b.x,a.y-b.y),scale};}});
  stage.addEventListener('pointermove',e=>{if(!pointers.has(e.pointerId))return;const before=pointers.get(e.pointerId);pointers.set(e.pointerId,{x:e.clientX,y:e.clientY});if(pointers.size===2&&pinch){const[a,b]=[...pointers.values()];setZoom(pinch.scale*Math.hypot(a.x-b.x,a.y-b.y)/pinch.distance,(a.x+b.x)/2,(a.y+b.y)/2);}else if(pointers.size===1){stage.scrollTop+=before.y-e.clientY;stage.scrollLeft+=before.x-e.clientX;}});
  const release=e=>{pointers.delete(e.pointerId);pinch=null;};stage.addEventListener('pointerup',release);stage.addEventListener('pointercancel',release);
  const resize=()=>{if(current)layout(currentIndex);};window.addEventListener('resize',resize);
  let dpiQuery;const watchDpi=()=>{dpiQuery?.removeEventListener('change',dpiChanged);dpiQuery=window.matchMedia?.(`(resolution: ${window.devicePixelRatio||1}dppx)`);dpiQuery?.addEventListener('change',dpiChanged);};
  const dpiChanged=()=>{watchDpi();resize();};watchDpi();
  return {open,button(source,index,rotation=0,reader=null){const item=cards.button(source,index,rotation);item.target.onclick=()=>void open(reader?.source||source,item.target,reader?.index??index);return item;},update(items){cards.update(items);if(dialog.open)cards.deactivate();},activate(){active=true;cards.activate();},deactivate(){active=false;if(dialog.open)dialog.close();invalidate();current=null;cards.deactivate();},clear(){if(dialog.open)dialog.close();invalidate();current=null;return cards.clear();},dispose(){disposed=true;active=false;if(dialog.open)dialog.close();invalidate();current=null;window.removeEventListener('resize',resize);dpiQuery?.removeEventListener('change',dpiChanged);dialog.remove();cards.dispose();}};
 }
 root.OtoPdfPreview={create};
})(typeof window!=='undefined'?window:globalThis);
