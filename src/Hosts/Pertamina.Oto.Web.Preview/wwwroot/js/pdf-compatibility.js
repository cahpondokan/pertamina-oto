/* Local-only compatibility intake. A disclosed derivative is validated again
 * by the normal engine. No original file writes, password persistence or uploads. */
(function(root){'use strict';
 const BASE='/vendor/pdfjs-6.4.299/',MAX_BYTES=128*1024*1024,MAX_PAGES=200,MAX_PIXELS=16*1024*1024;
 const eligible=new Set(['ENCRYPTED','UNSUPPORTED_SIGNATURE','UNSUPPORTED_FORM','UNSUPPORTED_ATTACHMENT','UNSUPPORTED_ACTION','UNSUPPORTED_ANNOTATIONS','UNSUPPORTED_TAGGED','TAGGED_STRUCTURE','UNSAFE_LINK','LINK_TARGET']);
 // Signed PDFs and forms are accepted inputs. Prepare their visible pages
 // automatically; these engine signals describe preparation, not UI refusal.
 const automatic=new Set(['UNSUPPORTED_SIGNATURE','UNSUPPORTED_FORM']);
 const messages={COPY_LIMIT:'Salinan kompatibilitas dibatasi 128 MiB dan 200 halaman. Gunakan dokumen lebih kecil; file asli tetap utuh.',COPY_XFA:'Formulir XFA ini belum dapat dirender lengkap. Tidak ada salinan kosong yang dibuat.',COPY_RENDER:'Tampilan dokumen ini belum dapat disalin lengkap. File asli tetap utuh.',COPY_PASSWORD:'Kata sandi belum benar. Coba kembali.',COPY_FEATURE:'Browser ini belum mendukung penyalinan PDF khusus. Gunakan Chrome atau Edge terbaru.',COPY_CANCELLED:'Penyiapan salinan dibatalkan. File asli tetap utuh.'};
 function pixelSize(width,height){if(!Number.isFinite(width)||!Number.isFinite(height)||width<=0||height<=0||width>14400||height>14400)throw Error('COPY_LIMIT');const scale=Math.min(400/72,Math.sqrt(MAX_PIXELS/(width*height)),8192/Math.max(width,height));return {scale,width:Math.max(1,Math.floor(width*scale)),height:Math.max(1,Math.floor(height*scale))};}
 function copyName(name){return String(name||'dokumen.pdf').replace(/\.pdf$/i,'').slice(0,85)+'_salinan.pdf';}
 if(typeof module!=='undefined'){module.exports={eligible,messages,pixelSize,copyName,MAX_BYTES,MAX_PAGES,MAX_PIXELS};return;}
 let imports;
 function libraries(){return imports||=Promise.all([import(BASE+'build/pdf.min.mjs'),import(BASE+'binary-data.mjs')]);}
 function element(tag,text,cls){const e=document.createElement(tag);if(text)e.textContent=text;if(cls)e.className=cls;return e;}
 function create({notify=()=>{},active=()=>true}={}){
  let abort=null,dialog=null,resolveDialog=null,loading=null,native=null,worker=null,renderTask=null,copyWorker=null,timer=null;
  const check=()=>{if(abort?.signal.aborted||!active())throw Error('COPY_CANCELLED');};
  function cancellable(promise){check();const signal=abort.signal;return new Promise((resolve,reject)=>{const stop=()=>reject(Error('COPY_CANCELLED'));signal.addEventListener('abort',stop,{once:true});Promise.resolve(promise).then(value=>{signal.removeEventListener('abort',stop);resolve(value);},error=>{signal.removeEventListener('abort',stop);reject(error);});});}
  function closeDialog(){const d=dialog;dialog=null;d?.close();d?.remove();}
  function cancel(){abort?.abort();resolveDialog?.(null);resolveDialog=null;closeDialog();renderTask?.cancel();void loading?.destroy().catch(()=>{});native?.terminate();copyWorker?.terminate();}
  function ask({title,description,password=false,error=false,primary='Buat salinan & lanjutkan',initial=''}){
   check();closeDialog();const d=dialog=element('dialog',null,'pdf-compatible-dialog');d.setAttribute('aria-label',title);
   const form=element('form'),h=element('h2',title),p=element('p',description),controls=element('div',null,'pdf-compatible-actions');form.append(h,p);
   let input;if(password){const label=element('label','Kata sandi PDF');input=element('input');input.type='password';input.autocomplete='off';input.spellcheck=false;input.value=initial;label.append(input);form.append(label);}
   if(error){const note=element('p','Kata sandi belum benar. Silakan coba kembali.','pdf-compatible-error');note.setAttribute('role','alert');form.append(note);}
   const no=element('button','Batal','button'),yes=element('button',primary,'button primary');no.type='button';yes.type='submit';yes.id='pdf-compat-continue';no.id='pdf-compat-cancel';controls.append(no,yes);form.append(controls);d.append(form);document.body.append(d);
   return new Promise(resolve=>{let settled=false;const finish=value=>{if(settled)return;settled=true;resolveDialog=null;if(input)input.value='';closeDialog();resolve(value);};resolveDialog=finish;
    no.onclick=()=>finish(null);d.addEventListener('cancel',e=>{e.preventDefault();finish(null);});form.onsubmit=e=>{e.preventDefault();finish(input?input.value:true);};d.showModal();(input||yes).focus();
   });
  }
  function progress(text){check();notify(text);if(dialog?.dataset.progress){dialog.querySelector('p').textContent=text;return;}closeDialog();const d=dialog=element('dialog',null,'pdf-compatible-dialog');d.dataset.progress='true';d.setAttribute('aria-label','Menyiapkan salinan PDF');const p=element('p',text);p.setAttribute('role','status');const stop=element('button','Batalkan','button');stop.id='pdf-compat-cancel';stop.onclick=cancel;d.addEventListener('cancel',e=>{e.preventDefault();cancel();});d.append(element('h2','Menyiapkan salinan PDF'),p,stop);document.body.append(d);d.showModal();stop.focus();}
  async function task(request){check();return new Promise((resolve,reject)=>{
   const w=copyWorker=new Worker('/js/pdf-compatible-worker.js');let settled=false;
   const done=(e,v)=>{if(settled)return;settled=true;clearTimeout(t);abort.signal.removeEventListener('abort',stop);w.terminate();if(copyWorker===w)copyWorker=null;e?reject(e):resolve(v);};
   const stop=()=>done(Error('COPY_CANCELLED')),t=setTimeout(()=>done(Error('COPY_LIMIT')),120000);abort.signal.addEventListener('abort',stop,{once:true});
   w.onmessage=e=>e.data.ok?done(null,e.data.bytes):done(Error(e.data.code));w.onerror=e=>{e.preventDefault();done(Error('COPY_RENDER'));};w.postMessage(request);
  });}
  async function open(file){
   const [pdfjs,binary]=await cancellable(libraries());check();
   native=new Worker('/js/pdf-compatible-pdfjs-worker.mjs',{type:'module'});worker=new pdfjs.PDFWorker({port:native});
   const initial=new Uint8Array(await file.slice(0,65536).arrayBuffer()),range=new pdfjs.PDFDataRangeTransport(file.size,initial);
   let stopped=false;range.abort=()=>{stopped=true;};range.requestDataRange=(a,b)=>{if(stopped)return;void file.slice(a,b).arrayBuffer().then(data=>{if(!stopped&&!abort.signal.aborted)range.onDataRange(a,new Uint8Array(data));}).catch(cancel);};
   class LocalData{fetch({kind,filename}){return binary.read(kind,filename);}}
   loading=pdfjs.getDocument({range,length:file.size,rangeChunkSize:65536,worker,BinaryDataFactory:LocalData,useWorkerFetch:false,enableXfa:true,enableScripting:false,isEvalSupported:false,useWasm:false,disableFontFace:true,useSystemFonts:true,stopAtErrors:true,disableAutoFetch:true,disableStream:true,verbosity:0,maxImageSize:MAX_PIXELS*2,canvasMaxAreaInBytes:MAX_PIXELS*4,isImageDecoderSupported:false,isOffscreenCanvasSupported:false});
   loading.onPassword=(update,reason)=>{void ask({title:'Buka PDF dengan kata sandi',description:'Kata sandi hanya digunakan pada perangkat ini dan tidak disimpan. Hasil salinan tidak memakai enkripsi sumber.',password:true,error:reason===pdfjs.PasswordResponses.INCORRECT_PASSWORD,primary:'Buka PDF'}).then(password=>{if(password===null){cancel();return;}check();update(password);password='';progress('Membuka PDF di perangkat ini…');}).catch(cancel);};
   const doc=await cancellable(loading.promise);check();if(doc.numPages<1||doc.numPages>MAX_PAGES)throw Error('COPY_LIMIT');return {doc,pdfjs};
  }
  async function render(doc,pdfjs){
   const pages=[];let total=0;
   for(let i=1;i<=doc.numPages;i++){
    check();progress(`Membuat salinan tampilan · halaman ${i} dari ${doc.numPages}…`);
    const page=await doc.getPage(i),view=page.getViewport({scale:1}),size=pixelSize(view.width,view.height),canvas=document.createElement('canvas');canvas.width=size.width;canvas.height=size.height;
    try{
     const ctx=canvas.getContext('2d',{alpha:false});if(!ctx)throw Error('COPY_FEATURE');ctx.fillStyle='white';ctx.fillRect(0,0,canvas.width,canvas.height);
     if(doc.isPureXfa){await root.OtoXfaCopy.render({page,pdfjs,canvas,viewport:page.getViewport({scale:size.scale}),check});}
     else{renderTask=page.render({canvasContext:ctx,viewport:page.getViewport({scale:size.scale}),annotationMode:pdfjs.AnnotationMode.ENABLE,background:'rgb(255,255,255)'});await cancellable(renderTask.promise);renderTask=null;}
     check();const blob=await new Promise(resolve=>canvas.toBlob(resolve,'image/png'));if(!blob||(total+=blob.size)>MAX_BYTES)throw Error('COPY_LIMIT');
     pages.push({width:view.width,height:view.height,bytes:new Uint8Array(await blob.arrayBuffer())});
    }finally{renderTask=null;canvas.width=canvas.height=1;page.cleanup();}
   }
   progress('Memeriksa salinan tampilan…');return task({action:'raster',pages});
  }
  async function convert(file,reason){
   if(file.size>MAX_BYTES)throw Error('COPY_LIMIT');
   if(!automatic.has(reason)){
    const accepted=await ask({title:'Buat salinan yang dapat diproses',description:`${file.name}\n\nFile asli tetap utuh. Salinan baru tidak membawa sertifikat tanda tangan digital, password, isian interaktif, lampiran, tautan, atau multimedia. Teks dan gambar asli dipertahankan bila memungkinkan. Jika perlu salinan berupa gambar halaman, OTO akan memberi tahu sebelum melanjutkan.`});
    if(accepted===null)throw Error('COPY_CANCELLED');
   }
   progress(automatic.has(reason)?'PDF diterima. Menyiapkan halaman dan tampilan isian…':'Menyiapkan salinan lokal…');let bytes,mode='vector';
   try{if(reason==='ENCRYPTED')throw Error('COPY_RENDER');bytes=await task({action:'normalize',file});}
   catch(e){
    if(['COPY_LIMIT','COPY_CANCELLED'].includes(e.message))throw e;
    const {doc,pdfjs}=await open(file);let vectorFailed=doc.isPureXfa;
    if(!vectorFailed){
     try{
      // A new multi-document export does not inherit encryption/signature status
      // from one source. The temporary last page is removed by the copy worker.
      if(typeof Math.sumPrecise!=='function'||typeof Map.prototype.getOrInsertComputed!=='function')throw Error('COPY_FEATURE');
      const placeholder=await task({action:'blank'});check();const exported=await cancellable(doc.extractPages([{document:null},{document:placeholder}]));check();
      if(!(exported instanceof Uint8Array)||exported.length>MAX_BYTES)throw Error('COPY_RENDER');
      bytes=await task({action:'normalize',file:new Blob([exported],{type:'application/pdf'}),dropLast:1});
     }catch(error){if(['COPY_LIMIT','COPY_CANCELLED'].includes(error.message))throw error;vectorFailed=true;}
    }
    if(vectorFailed){
     const rasterAccepted=await ask({title:'Gunakan salinan tampilan statis?',description:'Struktur ini belum dapat disalin sebagai teks dan objek asli. Halaman akan menjadi gambar resolusi tinggi. Teks tidak dapat dipilih atau dicari; ukuran file dapat bertambah dan detail saat zoom mengikuti resolusi gambar. Konten interaktif yang tidak terlihat tidak ikut disalin. Periksa pratinjau hasil sebelum menyimpan.',primary:'Buat salinan tampilan'});
     if(rasterAccepted===null)throw Error('COPY_CANCELLED');mode='raster';bytes=await render(doc,pdfjs);
    }
   }
   check();return {file:new File([bytes],copyName(file.name),{type:'application/pdf'}),mode};
  }
  async function cleanup(){closeDialog();resolveDialog=null;renderTask?.cancel();renderTask=null;const old=loading;loading=null;if(old)await Promise.race([old.destroy().catch(()=>{}),new Promise(r=>setTimeout(r,200))]);worker?.destroy();worker=null;native?.terminate();native=null;copyWorker?.terminate();copyWorker=null;}
  async function prepare(files,inspect){
   abort=new AbortController();timer=setTimeout(cancel,10*60*1000);const converted=[];
   try{
    try{return {files,result:await inspect(files),converted};}catch(e){check();if(!eligible.has(e.message))throw e;}
    return await navigator.locks.request('oto-compatible-copy-v1',{ifAvailable:true},async lock=>{
    if(!lock)throw Error('BUSY');
    const prepared=[];
    for(const file of files){check();try{await inspect([file]);prepared.push(file);}catch(e){check();if(!eligible.has(e.message))throw e;
      const copy=await convert(file,e.message);prepared.push(copy.file);converted.push({index:prepared.length-1,mode:copy.mode});await cleanup();
    }}
    check();const result=await inspect(prepared);check();
    for(const c of converted){const source=result.sources?.[c.index];if(source)source.warnings=[...(source.warnings||[]),c.mode==='raster'?'COPY_RASTER':'COPY_STATIC'];}
    return {files:prepared,result,converted};
    });
   }catch(e){if(abort?.signal.aborted||e?.name==='AbortError')throw Error('COPY_CANCELLED');throw e;}
   finally{clearTimeout(timer);timer=null;await cleanup();abort=null;}
  }
  return {prepare,cancel};
 }
 root.OtoPdfCompatibility={create,messages,pixelSize,copyName};
})(typeof window!=='undefined'?window:globalThis);
