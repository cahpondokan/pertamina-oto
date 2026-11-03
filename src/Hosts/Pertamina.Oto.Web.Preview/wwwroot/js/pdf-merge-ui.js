window.OtoMergeMount=function(screen){
 'use strict';
 const root=screen.querySelector('.merge-workspace'),$=id=>root.querySelector('#merge-'+id),M=OtoMergeEngine,J=OtoMergeJob;
 let sources=[],result=null,resultFile=null,directory=null,job=null,busy=false,generation=0,nextId=0,drag=null,view='cards',disposed=false,confirmation=null,failure=null,resultSourceCount=0;
 const profile=OtoPdfUi.ToolProfile.from(root),preview=OtoPdfPreview.create(screen,{merge:true});
 const controller=new OtoPdfUi.WorkspaceController(screen,{tool:'merge',profile,busy:()=>busy,hasWork:()=>!!(sources.length||resultFile||busy),resources:()=>({sources:[],outputBytes:0}),beforeLeave:async()=>!resultFile||await confirm('Hasil gabungan belum dipastikan tersimpan. Pindah alat akan membuang hasil sementara. Tetap lanjut?','Pindah alat?'),activate:()=>preview.activate(),deactivate:()=>{void clear();preview.deactivate();},dispose:()=>{disposed=true;resolveConfirm(false);void clear();preview.dispose();}});
 let lastAudit=null;

 const documentMessages=OtoPdfMessages.merge(),message=documentMessages.errors,error=e=>documentMessages.error(e);
 const size=OtoPdfFlow.size;
 const text=(tag,value,cls='')=>{const e=document.createElement(tag);e.className=cls;e.textContent=value;return e;};
 function status(value,bad=false){$('status').textContent=value;$('status').classList.toggle('error',bad);}
 const notices=warnings=>documentMessages.summary(warnings);
 const compat=window.OtoPdfCompatibility?.create({notify:status,active:()=>!disposed});controller.use(()=>compat?.cancel());
 const flow=new OtoPdfFlow.ProgressController(root,'merge',status);
 const resultView=new OtoPdfFlow.ResultController(root,'merge',{audit:()=>lastAudit,result:()=>resultFile?{...result,file:resultFile}:null,rename:name=>result.name=name,cleanName:OtoPdf.cleanName,messages:documentMessages,busy:()=>busy,setBusy,preview,notify:status,render,changed:telemetry,saved:async()=>{await clear();status('File berhasil ditulis. Pekerjaan selesai dan sumber daya dilepas.');$('pick').focus();}});
 controller.use(()=>resultView.dispose());
 function telemetry(){root.dataset.resources=JSON.stringify({sourceFiles:sources.length,inputBuffers:0,outputFiles:resultFile?1:0,objectUrls:resultView.objectUrls,jobWorkers:job?1:0,directories:directory?1:0});}
 function setBusy(value){busy=value;root.setAttribute('aria-busy',String(value));$('progress').hidden=!value;if(value)flow.reset();syncPanels();root.querySelectorAll('button,input,select').forEach(e=>{if(!['merge-cancel','merge-stay','merge-confirm-yes'].includes(e.id))e.disabled=value;});summary();telemetry();}
 function progress(p){if(!disposed)flow.update(p);}
 async function runJob(request){job=J.start(request,progress);telemetry();try{return await job.promise;}finally{job=null;telemetry();}}
 async function removeDirectory(name){try{await J.remove(name);}catch(e){window.OtoDiagnostics?.begin('merge','cleanup').fail(e);status('Pembersihan penyimpanan sementara belum berhasil. Akan dicoba kembali saat ruang kerja dibuka.',true);}}
 async function releaseResult(){const old=directory;directory=null;result=null;resultFile=null;resultView.reset();telemetry();await removeDirectory(old);}

 async function clear(){failure=null;$('result-notes').open=false;generation++;compat?.cancel();job?.cancel();job=null;sources=[];await preview.clear();await releaseResult();$('files').value='';$('name').value='dokumen_gabungan.pdf';$('bookmarks').checked=false;$('blank').value='none';$('sort').value='manual';setBusy(false);render();}
 function confirm(message,title='Selesaikan pekerjaan?'){
  if(confirmation)return Promise.resolve(false);
  $('confirm-title').textContent=title;$('confirm-message').textContent=message;$('confirm').showModal();$('stay').focus();return new Promise(resolve=>confirmation=resolve);
 }
 function resolveConfirm(value){const resolve=confirmation;confirmation=null,failure=null,resultSourceCount=0;$('confirm').close();resolve?.(value);}
 $('stay').onclick=()=>resolveConfirm(false);$('confirm-yes').onclick=()=>resolveConfirm(true);$('confirm').addEventListener('cancel',e=>{e.preventDefault();resolveConfirm(false);});
 function summary(){
  let pages=0,valid=true;for(const s of sources){try{const count=M.range(s.range,s.count).length;pages+=count;if($('blank').value==='always'||$('blank').value==='odd'&&count%2)pages++;}catch{valid=false;}}
  $('count').textContent=sources.length;$('summary').textContent=`${sources.length} PDF · ${valid?pages.toLocaleString('id-ID'):'—'} halaman hasil · ${size(sources.reduce((n,s)=>n+s.bytes.size,0))}`;
  $('mobile-summary').textContent=`${sources.length} PDF · ${valid?pages.toLocaleString('id-ID'):'—'} halaman`;$('run').disabled=busy||sources.length<2||!valid;
 }
 function reorder(id,delta){if(busy)return;const from=sources.findIndex(s=>s.id===id),to=from+delta;if(to<0||to>=sources.length)return;const [item]=sources.splice(from,1);sources.splice(to,0,item);$('sort').value='manual';render();root.querySelector(`[data-source="${id}"] button[data-direction="${delta}"]`)?.focus();status('Urutan dokumen diperbarui.');}
 function syncPanels(){
  const state=busy?'processing':result?'result':failure?'error':sources.length?'selected':'empty';root.dataset.state=state;
  $('empty').hidden=state!=='empty';$('editor').hidden=state==='empty';$('setup').hidden=busy||!!result||!!failure;$('progress').hidden=!busy;$('failure').hidden=!failure||busy;$('result').hidden=!result||busy;
  root.querySelector('.merge-documents').inert=busy||!!failure;root.querySelector('.merge-documents').hidden=!!result;$('output-preview').hidden=!result||busy;
  if(failure)$('failure-message').textContent=failure;
 }
 function render(){
  window.OtoTooltips?.hide();syncPanels();
  $('list').replaceChildren();$('list').classList.toggle('rows',view==='rows');const previews=[];
  sources.forEach((s,index)=>{
   const card=text('article','','merge-card');card.dataset.source=s.id;card.draggable=!busy;
   const top=text('div','','merge-card-top');top.append(text('span',String(index+1).padStart(2,'0')),text('span','✓ Siap'));
   const cover=text('div','','merge-cover'),item=preview.button(s,0,s.rotation);cover.append(item.target);previews.push(item);
   const copy=text('div','','merge-copy');copy.append(text('strong',s.name),text('small',`${s.count.toLocaleString('id-ID')} halaman · ${size(s.bytes.size)}${s.rotation?' · '+s.rotation+'°':''}`));
   const actions=text('div','','merge-card-tools');
   function button(label,title,fn){const b=text('button',label);b.type='button';b.dataset.tip=title;b.setAttribute('aria-label',title+' · '+s.name);b.disabled=busy;b.onclick=fn;actions.append(b);return b;}
   button('Pratinjau','Buka pratinjau',()=>item.target.click());
   const left=button('←','Pindah lebih awal',()=>reorder(s.id,-1));left.dataset.direction='-1';left.disabled=busy||index===0;
   const right=button('→','Pindah lebih akhir',()=>reorder(s.id,1));right.dataset.direction='1';right.disabled=busy||index===sources.length-1;
   button('↻','Putar semua halaman 90°',()=>{s.rotation=(s.rotation+90)%360;render();});
   button('×','Hapus dari pilihan',()=>{sources=sources.filter(x=>x!==s);render();status('Dokumen dikeluarkan dari pilihan. File asli tetap utuh.');});
   const details=text('details','','merge-range'),label=text('label','Halaman yang digabung'),input=document.createElement('input');input.type='text';input.value=s.range;input.placeholder='Semua · contoh 1, 3-5';input.maxLength=1000;input.setAttribute('aria-label','Rentang halaman '+s.name);input.disabled=busy;
   input.oninput=()=>{s.range=input.value;try{M.range(s.range,s.count);input.setAttribute('aria-invalid','false');status('Rentang diperbarui.');}catch{input.setAttribute('aria-invalid','true');status(message.RANGE,true);}summary();};label.append(input);details.append(text('summary',s.range?'Halaman: '+s.range:'Semua halaman'),label);
   card.append(top,cover,copy,actions,details);
   card.addEventListener('dragstart',e=>{if(busy||e.target.closest('input,button,select')){e.preventDefault();return;}drag=s.id;card.classList.add('dragging');e.dataTransfer.setData('text/plain',String(s.id));});
   card.addEventListener('dragend',()=>{drag=null;card.classList.remove('dragging');});
   card.addEventListener('dragover',e=>{if(drag!==null)e.preventDefault();});
   card.addEventListener('drop',e=>{if(drag===null||busy)return;e.preventDefault();e.stopPropagation();const from=sources.findIndex(v=>v.id===drag),to=sources.indexOf(s);if(from>=0){const [moved]=sources.splice(from,1);sources.splice(to,0,moved);$('sort').value='manual';render();}drag=null;});
   $('list').append(card);
  });
  $('output-cover').replaceChildren();
  if(result&&resultFile){const source={id:'output',label:'hasil',name:result.name,bytes:resultFile,count:result.count,rotation:0};const item=preview.button(source,0);$('output-cover').append(item.target);previews.push(item);$('preview-output').onclick=()=>void preview.open(source,$('preview-output'));}
  preview.update(previews);summary();telemetry();
 }
 async function add(files){
  if(busy||result||disposed)return;failure=null;
  let incoming=Array.from(files);if(!incoming.length)return;
  const token=++generation,audit=window.OtoDiagnostics?.begin('merge','input');
  try{
   if(incoming.some(f=>!profile.accepts(f)))throw Error('TYPE');
   M.admission([...sources.map(s=>s.bytes),...incoming]);setBusy(true);await preview.clear();
   let inspected;if(compat){const prepared=await compat.prepare(incoming,files=>runJob({action:'inspect',files}));incoming=prepared.files;inspected=prepared.result;}else inspected=await runJob({action:'inspect',files:incoming});if(token!==generation)throw Error('CANCELLED');
   M.admission([...sources.map(s=>s.bytes),...incoming]);
   incoming.forEach((f,i)=>sources.push({id:++nextId,label:String(nextId),name:f.name,bytes:f,count:inspected.sources[i].count,warnings:inspected.sources[i].warnings,tagged:inspected.sources[i].tagged,range:'',rotation:0}));
   status(`${incoming.length} PDF siap. `+notices(sources.flatMap(s=>s.warnings)));audit?.success();
  }catch(e){audit?.fail(e);if(token===generation)status(error(e),true);}
  finally{if(token===generation){setBusy(false);render();}}
 }
 const pick=()=>{if(!busy&&!result&&!disposed)$('files').click();};$('pick').onclick=$('add').onclick=()=>input.pick();
 $('cards').onclick=()=>{view='cards';$('cards').setAttribute('aria-pressed','true');$('rows').setAttribute('aria-pressed','false');render();};
 $('rows').onclick=()=>{view='rows';$('cards').setAttribute('aria-pressed','false');$('rows').setAttribute('aria-pressed','true');render();};
 $('sort').onchange=()=>{if($('sort').value==='original')sources.sort((a,b)=>a.id-b.id);else if($('sort').value!=='manual')sources.sort((a,b)=>a.name.localeCompare(b.name,'id',{numeric:true})*($('sort').value==='desc'?-1:1));render();};
 $('blank').onchange=summary;$('clear').onclick=async()=>{if(await confirm('Kosongkan susunan Merge? File asli tetap utuh.','Kosongkan pilihan?')){await clear();status('Ruang kerja dikosongkan.');$('pick').focus();}};
 $('cancel').onclick=()=>{resultView.cancel();compat?.cancel();job?.cancel();};
 $('run').onclick=async()=>{
  if(busy||sources.length<2)return;failure=null;resultSourceCount=sources.length;const token=++generation,audit=lastAudit=window.OtoDiagnostics?.begin('merge','process');let ownDirectory=null;
  setBusy(true);await preview.clear();status('Menyiapkan penyimpanan hasil lokal…');
  try{
   ownDirectory=await J.create();directory=ownDirectory;telemetry();
   const value=await runJob({files:sources.map(s=>s.bytes),items:sources.map(s=>({range:s.range,rotation:s.rotation})),options:{blank:$('blank').value,bookmarks:$('bookmarks').checked},name:$('name').value,directory});
   if(token!==generation)throw Error('CANCELLED');
   resultFile=await J.result(directory);if(token!==generation)throw Error('CANCELLED');
   value.warnings=[...new Set([...(value.warnings||[]),...sources.flatMap(s=>s.warnings||[]).filter(w=>w==='COPY_STATIC'||w==='COPY_RASTER')])];
   result=value;sources=[];await preview.clear();$('list').replaceChildren();
   resultView.present({...result,file:resultFile});status('');audit?.success();
  }catch(e){audit?.fail(e);if(token===generation){await releaseResult();if(e.message!=='CANCELLED'&&e.name!=='AbortError')failure=error(e);status(error(e),true);}}
  finally{if(token===generation){setBusy(false);render();}else await removeDirectory(ownDirectory);}
 };
 $('retry').onclick=()=>$('run').click();$('failure-back').onclick=()=>{failure=null;render();$('run').focus();};
 $('new').onclick=async()=>{if(await confirm('Mulai baru akan membuang hasil sementara. Pastikan PDF sudah tersimpan.','Mulai pekerjaan baru?')){await clear();status('Pilih PDF untuk pekerjaan baru.');$('pick').focus();}};
 new OtoPdfFlow.SettingsController(root,'merge',$('name'));
 const input=new OtoPdfUi.FileInputController(controller,root,{host:window,input:$('files'),clipboard:OtoPdfPaste,pick,addFiles:add,hasResult:()=>!!resultFile,notify:status,resultMessage:'Hasil sebelumnya tetap tersedia. Simpan hasil, lalu pilih Mulai baru sebelum menempel PDF berikutnya.'});
 render();
 return controller;
};
