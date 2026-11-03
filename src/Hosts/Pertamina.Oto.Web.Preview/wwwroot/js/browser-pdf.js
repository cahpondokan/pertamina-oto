window.OtoBrowserMount=function(screen,seed,session) {
  'use strict';
  seed ||= [];session ||= {};
  const root=screen.querySelector('[data-browser-tool]');
  if(!root) return;
  if(root.dataset.browserTool==='merge')return window.OtoMergeMount(screen);
  const profile=OtoPdfUi.ToolProfile.from(root),tool=root.dataset.browserTool,images=profile.images,merge=tool==='merge',reorder=profile.reorder,multi=profile.multiple,$=id=>screen.querySelector('#'+id);
  const L=OtoPdf.Capacity.input(tool), M=OtoPdfWorkspace, initialOutputName=$('pdf-output-name').value;
  const fileBacked=Object.hasOwn(L,'outputBytes'),byteSize=bytes=>bytes instanceof Blob?bytes.size:bytes.byteLength;
  let outputDirectory=null,pendingCleanup=Promise.resolve();
  async function removeDirectory(name){if(!name)return;try{await OtoMergeJob.remove(name);}catch(e){window.OtoDiagnostics?.begin(tool,'cleanup').fail(e);if(!controller.disposed)feedback('Pembersihan file sementara belum berhasil. Akan dicoba kembali saat aplikasi web dibuka.',true);}}
  const modern=root.dataset.workspaceLayout==='shared', preview=images&&!modern?null:OtoPdfPreview.create(screen,{continuous:modern});
  let output=null, outputs=[], lastAudit=null, resultPreviewItems=[];
  let sources=[], pages=[], busy=false, activeJob=null, sequence=0, sourceId=0, pageId=0, resultUrls=[], resultBytes=0, dragId=null, pending=null, showResult=false;
  let splitMode='custom',splitPageMode='all',splitRanges=[{start:1,end:1}],splitSelection=new Set(),splitSync=false,splitHover=-1;
  const controller=new OtoPdfUi.WorkspaceController(screen,{tool,profile,resources:()=>({sources,outputBytes:resultBytes,fileBacked}),transferSources:()=>tool==='split'&&showResult?outputs.map((item,index)=>({name:item.name,bytes:item.file,count:item.count,sourcePages:item.sourcePages||[],id:'result-'+index})):sources,busy:()=>busy,activate:()=>preview?.activate(),deactivate:()=>preview?.deactivate(),dispose:()=>{sequence++;compat?.cancel();activeJob?.cancel();resultView?.dispose();releaseResults();releaseSources();preview?.dispose();pages=[];}});
  function listen(target,event,handler){controller.listen(target,event,handler);}
  const documentMessages=OtoPdfMessages.browser(images,tool),messages=documentMessages.errors;
  const compat=images?null:window.OtoPdfCompatibility?.create({notify:feedback,active:()=>!controller.disposed});controller.use(()=>compat?.cancel());
  const flow=modern?new OtoPdfFlow.ProgressController(root,'pdf',feedback):null;
  const resultView=modern?new OtoPdfFlow.ResultController(root,'pdf',{audit:()=>lastAudit,compactResults:tool==='split',openResult:(trigger,index)=>{const item=outputs[index]||output;if(item)void preview.open(outputSource(outputs.indexOf(item)),trigger);},previewThumbnail:(item,index)=>preview?.button(outputSource(index),0),resultPreviews:items=>{resultPreviewItems=items;},renderNextTools:renderNextTools,sourceName:()=>sources[0]?.name||'',formatSourcePages:formatSourcePages,result:()=>output,results:()=>outputs,select:index=>output=outputs[index],rename:name=>{if(output.name!==name)output.saved=false;output.name=name;},renameResult:(index,name)=>{const item=outputs[index];if(item){item.name=name;item.saved=false;if(item===output)$('pdf-result-name').value=name;void resultView.clearArchive();}},cleanName:OtoPdf.cleanName,messages:documentMessages,busy:()=>busy,setBusy,preview,notify:feedback,render,archiveProgress:p=>flow?.update(p),saved:()=>{output.saved=true;if(tool==='split'){output=outputs.find(item=>!item.saved)||output;resultView.present(output);feedback(outputs.every(item=>item.saved)?'Semua file sudah disimpan. Hasil tetap tersedia sampai Anda memilih Proses PDF lain.':'File tersimpan. PDF berikutnya sudah dipilih.');}else{resetWorkspace();feedback('Semua file berhasil ditulis. Pekerjaan selesai dan sumber daya dilepas.');}}}):null;
  const resultCount=()=>modern?outputs.length:resultUrls.length;
  if(modern){new OtoPdfFlow.SettingsController(root,'pdf',$('pdf-ranges')||$('pdf-output-name'));const startNew=$('pdf-new');if(startNew)startNew.onclick=()=>tool==='split'?pickNewSplit():closeWorkspace();const previewOutput=$('pdf-preview-output');if(previewOutput)previewOutput.onclick=()=>{if(output)void preview.open(outputSource(),previewOutput);};}
  function outputSource(index=outputs.indexOf(output)){const item=outputs[index];return item?{id:'output-'+index,label:'Hasil '+(index+1),name:item.name,bytes:item.file,count:item.count,geometry:item.geometry,rotation:0}:null;}
  function formatSourcePages(values){const pages=[...new Set(values)],ranges=[];for(let i=0;i<pages.length;){let j=i;while(j+1<pages.length&&pages[j+1]===pages[j]+1)j++;ranges.push(j===i?String(pages[i]):`${pages[i]}–${pages[j]}`);i=j+1;}return ranges.join(', ');}
  function renderNextTools(results){
    if(tool!=='split')return;const multiple=results.length>1,host=$('pdf-next-tools');if(!host)return;
    const choices=multiple?[
      ['merge','Gabungkan PDF','Satukan file hasil','028-gabungkan-pdf.png',false],
      ['organize','Atur halaman','Susun dan rapikan','031-atur-ulang-halaman-pdf.png',true],
      ['numbers','Tambahkan nomor','Beri nomor halaman','023-tambahkan-nomor-halaman.png',true]
    ]:[
      ['organize','Atur halaman','Susun dan rapikan','031-atur-ulang-halaman-pdf.png',true],
      ['numbers','Tambahkan nomor','Beri nomor halaman','023-tambahkan-nomor-halaman.png',true],
      ['watermark','Tanda air','Beri identitas dokumen','022-tambah-tanda-air.png',true]
    ];
    $('pdf-next-note').textContent=multiple?'Pilih alat, lalu tentukan file hasil yang akan digunakan.':'Gunakan PDF ini langsung pada alat berikut.';
    host.replaceChildren(...choices.map(([id,title,description,icon,single])=>{const link=document.createElement('a');link.className='split-next-tool';link.href='/alat/'+id;link.dataset.localSingleSource=String(single);link.setAttribute('aria-label',title+' · '+description);const image=document.createElement('img');image.src='/assets/'+icon;image.alt='';image.width=48;image.height=48;const copy=text('span','','split-next-copy');copy.append(text('strong',title),text('small',description));link.append(image,copy,text('span','→','split-next-arrow'));return link;}));
  }
  const text=(tag,value,className)=>{const e=document.createElement(tag);e.textContent=value;if(className)e.className=className;return e;};
  function splitPagesFromText(value,count){try{return OtoPdf.range(value.replace(/[–—−]/g,'-'),count);}catch{return null;}}
  function splitPlan(){
    if(tool!=='split'||!sources.length)return {valid:false,groups:[],pages:[]};
    const count=sources[0].count;let groups=[],chosen=[];
    if(splitMode==='custom'){
      const rows=Array.from($('pdf-split-range-rows')?.querySelectorAll('.split-range-row')||[]);
      for(const row of rows){const start=Number(row.querySelector('[data-start]')?.value),end=Number(row.querySelector('[data-end]')?.value);if(!Number.isInteger(start)||!Number.isInteger(end)||start<1||end<start||end>count)return {valid:false,groups:[],pages:[],error:'Periksa awal dan akhir setiap rentang (1–'+count+').'};const g=Array.from({length:end-start+1},(_,i)=>start+i-1);groups.push(g);}
      const flat=groups.flat();if(!flat.length||new Set(flat).size!==flat.length)return {valid:false,groups:[],pages:[],error:'Rentang kosong atau halaman yang sama digunakan lebih dari sekali.'};
      chosen=flat;
      if($('pdf-split-combine-ranges')?.checked)groups=[flat];
    }else if(splitMode==='fixed'){
      const size=Number($('pdf-split-pages-per-file')?.value);if(!Number.isInteger(size)||size<1||size>count)return {valid:false,groups:[],pages:[],error:'Masukkan halaman per file dari 1 sampai '+count+'.'};
      for(let i=0;i<count;i+=size)groups.push(Array.from({length:Math.min(size,count-i)},(_,n)=>i+n));chosen=Array.from({length:count},(_,i)=>i);
    }else if(splitMode==='all'){groups=Array.from({length:count},(_,i)=>[i]);chosen=Array.from({length:count},(_,i)=>i);}
    else {chosen=splitPageMode==='all'?Array.from({length:count},(_,i)=>i):splitPagesFromText($('pdf-split-selected-pages')?.value||'',count);if(!chosen?.length)return {valid:false,groups:[],pages:[],error:'Pilih satu atau lebih halaman dengan nomor yang valid.'};groups=$('pdf-split-combine-selected')?.checked?[chosen]:chosen.map(i=>[i]);}
    return {valid:groups.length<=L.outputs,groups,pages:chosen,error:groups.length>L.outputs?'Jumlah file melebihi batas '+L.outputs+'.':''};
  }
  function splitSummary(){
    const plan=splitPlan(),summary=$('pdf-split-summary');if(!summary)return plan;
    const count=plan.groups.reduce((n,g)=>n+g.length,0);summary.textContent=plan.valid?`${plan.groups.length} PDF akan dibuat · ${count} halaman`:plan.error||'Atur halaman untuk melihat ringkasan.';summary.dataset.valid=String(plan.valid);
    const ready=$('pdf-readiness');if(ready&&tool==='split'&&sources.length){const title=ready.querySelector('strong'),detail=ready.querySelector('small');if(title)title.textContent=plan.valid?`${plan.groups.length} PDF · ${count} halaman siap`:plan.error||'Atur halaman yang akan dipisahkan';if(detail)detail.textContent=plan.valid?'Periksa pratinjau, lalu jalankan pemisahan.':'Koreksi pilihan halaman untuk melanjutkan.';}
    const button=$('pdf-run');if(button&&!busy)button.disabled=!plan.valid;
    const fixed=$('pdf-split-fixed-preview');if(fixed)fixed.textContent=plan.groups.length?'Pembagian otomatis · '+plan.groups.map(g=>`${g[0]+1}–${g.at(-1)+1}`).join(' · '):'';
    const all=$('pdf-split-all-help');if(all&&sources.length)all.textContent=`Semua ${sources[0].count} halaman dipilih. Satu halaman menjadi satu file PDF.`;
    return plan;
  }
  function syncSplitCards(){
    const plan=splitPlan();if(tool!=='split')return;const selected=new Set(plan.pages);for(const card of $('pdf-page-list')?.children||[]){const idx=Number(card.dataset.pageIndex);card.classList.toggle('split-selected',selected.has(idx));card.classList.toggle('split-hover',splitHover>=0&&Number(card.dataset.groupIndex)===splitHover);card.setAttribute('aria-checked',String(selected.has(idx)));let mark=card.querySelector('.split-check');if(selected.has(idx)){if(!mark){mark=text('span','✓','split-check');card.append(mark);}mark.hidden=false;}else if(mark)mark.hidden=true;}
  }
  const compatibilityText=warnings=>documentMessages.compatibility(warnings);
  function feedback(message,error=false){$('pdf-feedback').textContent=message;$('pdf-feedback').classList.toggle('is-error',error);if(!modern&&root.hidden){resultFeedback(message);$('pdf-result-feedback').classList.toggle('is-error',error);}}
  function resultFeedback(message){if(modern)return;$('pdf-result-feedback').textContent=message;$('pdf-result-feedback').hidden=!message;$('pdf-result-feedback').classList.remove('is-error');}
  function syncView() {
    const state=M.state({busy,showResult,resultCount:resultCount(),sourceCount:sources.length});
    root.dataset.state=state;
    if(modern){root.hidden=false;$('pdf-editor').hidden=state==='empty';$('pdf-documents').hidden=state==='result';$('pdf-documents').inert=busy;$('pdf-output-preview').hidden=tool==='split'||state!=='result';$('pdf-result').hidden=state!=='result';$('pdf-progress').hidden=!busy;}else{root.hidden=state==='result';$('pdf-results').hidden=state!=='result';}
    for(const id of ['pdf-loaded','pdf-loaded-header','pdf-editor-toolbar','pdf-output-panel','pdf-sources-panel'])$(id).hidden=!sources.length;
    $('pdf-empty').hidden=!!sources.length||(modern&&busy);
    $('pdf-show-result').hidden=modern?!(tool==='split'&&resultCount()&&!showResult):!resultUrls.length;
    if(modern){$('pdf-output-panel').hidden=busy||state==='result'||!sources.length;$('pdf-mobile-summary').textContent=pages.length+' halaman · '+(tool==='split'?'atur bagian':sources.length+(images?' gambar':' PDF'));}
    $('pdf-add').hidden=!multi&&!!sources.length;
  }
  function releaseResults(){void resultView?.clearArchive();const old=outputDirectory;outputDirectory=null;if(old)pendingCleanup=Promise.all([pendingCleanup,removeDirectory(old)]);resultUrls.forEach(URL.revokeObjectURL);resultUrls=[];resultBytes=0;output=null;outputs=[];resultPreviewItems=[];showResult=false;resultView?.reset();if(!modern){$('pdf-result-files').replaceChildren();$('pdf-result-compatibility').hidden=true;}resultFeedback('');syncView();}
  function releaseSources(){preview?.clear();sources.forEach(s=>{if(s.thumbnail)URL.revokeObjectURL(s.thumbnail);});sources=[];}
  function setBusy(value) {
    busy=value;if(value)flow?.reset();root.setAttribute('aria-busy',String(value));
    root.querySelectorAll('button,input,select').forEach(el=>el.disabled=value||el.dataset.unavailable==='true');
    $('pdf-cancel').hidden=!value;$('pdf-cancel').disabled=false;
    if(!value){$('pdf-run').disabled=!M.readiness(tool,sources,pages).ready||(tool==='split'&&!splitPlan().valid);$('pdf-close').disabled=!sources.length;}
    for(const id of ['pdf-result-back','pdf-result-new'])if($(id))$(id).disabled=value;
    syncView();
  }
  async function job(request) {
    if(!fileBacked){
      const handle=OtoPdfJob.start(request,Worker,L.timeout,p=>flow?.update(p));
      activeJob=handle;return handle.promise.finally(()=>{if(activeJob===handle)activeJob=null;});
    }
    const token=sequence;let directory=null,handle;
    try{
      if(request.action==='run'){await pendingCleanup;if(token!==sequence)throw Error('CANCELLED');directory=await OtoMergeJob.create();}
      if(token!==sequence)throw Error('CANCELLED');
      handle=(tool==='split'?OtoMergeJob.split:OtoMergeJob.file)({...request,directory},p=>flow?.update(p),Worker,L.timeout);activeJob=handle;
      const result=await handle.promise;if(token!==sequence)throw Error('CANCELLED');
      if(request.action==='run'){
        const files=await Promise.all(result.results.map((_,index)=>OtoMergeJob.result(directory,index)));
        if(token!==sequence)throw Error('CANCELLED');
        result.results=result.results.map((item,index)=>({...item,file:files[index]}));
        outputDirectory=directory;directory=null;
      }
      return result;
    }catch(e){if(e.name==='QuotaExceededError')throw Error('STORAGE');throw e;}
    finally{if(activeJob===handle)activeJob=null;if(directory)await removeDirectory(directory);}
  }
  function makePages(){
    return sources.flatMap(s=>Array.from({length:s.count},(_,index)=>({id:++pageId,sourceId:s.id,index,rotation:0})));
  }
  function focusedItem() {
    const active=document.activeElement,card=active?.closest('.pdf-page-item'),source=active?.closest('.pdf-source-row');
    if(card)return {kind:'card',id:card.dataset.cardId,position:Number(card.dataset.position),action:active.dataset.action};
    if(source)return {kind:'source',id:source.dataset.sourceId,position:Number(source.dataset.position)};
    return null;
  }
  function restoreFocus(item) {
    if(!item||root.hidden)return;
    const selector=item.kind==='card'?'.pdf-page-item':'.pdf-source-row';
    const items=Array.from(root.querySelectorAll(selector));
    const found=items.find(el=>(item.kind==='card'?el.dataset.cardId:el.dataset.sourceId)===item.id)||items[Math.min(item.position,items.length-1)];
    const action=found?.querySelector('[data-action="'+item.action+'"]');
    const target=action&&!action.disabled?action:item.kind==='source'?found?.querySelector('button'):found;
    (target||$('pdf-empty-pick')).focus({preventScroll:true});
  }
  function mutate(action) {
    if(busy)return;
    const focused=focusedItem();releaseResults();action();render();restoreFocus(focused);
    feedback('Susunan diperbarui. Buat PDF baru untuk mendapatkan hasil terbaru.');
  }
  function move(id,delta){mutate(()=>{if(merge)sources=M.move(sources,id,delta);else pages=M.move(pages,id,delta);});}
  function removeSource(id){const source=sources.find(s=>s.id===id);if(source?.thumbnail)URL.revokeObjectURL(source.thumbnail);sources=sources.filter(s=>s.id!==id);pages=pages.filter(p=>p.sourceId!==id);}
  function showResults() {
    if(!resultCount()||busy)return;
    showResult=true;syncView();if(modern){render();$(tool==='split'?'pdf-save-all':'pdf-result-name').focus({preventScroll:true});}else{$('pdf-results').focus();$('pdf-results').scrollIntoView({behavior:'smooth',block:'start'});}
  }
  function render() {
    if(controller.disposed)return;
    const focused=focusedItem();
    $('pdf-compatibility').textContent=compatibilityText(OtoPdfSession.warnings(sources));$('pdf-compatibility').hidden=!$('pdf-compatibility').textContent;
    $('pdf-summary').textContent=sources.length?sources.length+' '+(images?'gambar':'dokumen')+' · '+pages.length+' halaman':'Belum ada dokumen';
    $('pdf-source-count').textContent=String(sources.length);
    $('pdf-reset-order').hidden=!reorder||!sources.length;
    const readiness=M.readiness(tool,sources,pages),ready=$('pdf-readiness');
    if(tool==='split'&&readiness.ready&&$('pdf-split-mode')?.value==='pages')readiness.detail=pages.length+' file PDF akan dibuat, masing-masing satu halaman.';
    ready.dataset.ready=String(readiness.ready);ready.replaceChildren(text('strong',readiness.title),text('small',readiness.detail));
    const list=$('pdf-source-list');list.replaceChildren();
    sources.forEach((s,position)=>{
      const row=text('div','','pdf-source-row');row.dataset.sourceId=String(s.id);row.dataset.position=String(position);row.dataset.sourceTone=String((s.id-1)%6);
      row.append(text('span',s.label,'pdf-source-badge'));
      const size=byteSize(s.bytes)<1048576?(byteSize(s.bytes)/1024).toFixed(1)+' KB':(byteSize(s.bytes)/1048576).toFixed(2)+' MB';
      const info=text('div',''),name=text('strong',s.name);name.title=s.name;info.append(name,text('small',s.count+' halaman · '+size));row.append(info);
      const remove=text('button','×','button');remove.type='button';remove.title='Keluarkan file';remove.setAttribute('aria-label','Keluarkan file '+s.label);
      remove.onclick=()=>mutate(()=>removeSource(s.id));
      row.append(remove);list.append(row);
    });
    if(!sources.length)list.append(text('p','Belum ada file.','pdf-hint'));
    const grid=$('pdf-page-list');grid.replaceChildren();const previews=[];
    const cards=M.cards(tool,sources,pages);
    const plan=tool==='split'?splitPlan():null;
    const arranged=modern?{name:tool==='split'?'Halaman sumber':'Susunan halaman',count:cards.length,pages:cards.map(({source,page})=>({source,index:page.index,rotation:page.rotation}))}:null;
    cards.forEach(({id,source:s,page:p,position,identity})=>{
      const card=text('article','','pdf-page-item');card.dataset.cardId=String(id);card.dataset.pageId=String(id);card.dataset.sourceId=String(s.id);card.dataset.position=String(position);card.dataset.sourceTone=String((s.id-1)%6);card.tabIndex=0;
      if(tool==='split'){card.dataset.pageIndex=String(p.index);card.dataset.groupIndex=String(plan?.groups.findIndex(g=>g.includes(p.index))??-1);card.setAttribute('role','checkbox');}
      card.setAttribute('aria-label',(merge?'Dokumen ':'Halaman ')+identity+(!reorder?', nomor sumber ':', posisi hasil ')+(position+1)+(p?.rotation?', diputar '+p.rotation+' derajat':''));
      card.append(text('span',merge?'Sumber '+identity:identity,'pdf-card-identity'));
      if(tool==='split'){const group=Number(card.dataset.groupIndex);if(group>=0)card.append(text('span',`${splitMode==='fixed'?'File':'Bagian'} ${group+1} · halaman sumber ${p.index+1}`,'split-group-label'));}
      let paper=text('div','','pdf-paper-id');
      if(images&&s.thumbnail){const img=document.createElement('img');img.src=s.thumbnail;img.alt='Pratinjau gambar '+s.label;paper.classList.add('pdf-image-preview');paper.append(img);}
      else {const item=preview.button(s,merge?0:p.index,p?.rotation||0,modern?{source:arranged,index:position}:null);previews.push(item);paper=item.target;}
      card.append(paper);
      if(merge||images){const name=text('strong',s.name,'pdf-card-name');name.title=s.name;card.append(name);}
      else card.append(text('span','Sumber '+s.label+' · Halaman '+(p.index+1),'pdf-card-meta'));
      card.append(text('p',merge?`Halaman 1 dari ${s.count} · Urutan ${position+1}`:(!reorder?'Nomor asli ':'Urutan hasil ')+(position+1)+(p?.rotation?' · Putar +'+p.rotation+'°':''),'pdf-card-position'));
      if(reorder){
        card.draggable=!busy;
        card.addEventListener('dragstart',e=>{if(busy||document.querySelector('dialog[open]')){e.preventDefault();return;}dragId=id;e.dataTransfer.setData('text/x-oto-page',String(id));e.dataTransfer.effectAllowed='move';});
        card.addEventListener('dragover',e=>{if(dragId!==null&&!busy)e.preventDefault();});
        card.addEventListener('drop',e=>{if(dragId===null||busy)return;e.preventDefault();e.stopPropagation();const from=dragId;dragId=null;mutate(()=>{if(merge)sources=M.moveTo(sources,from,position);else pages=M.moveTo(pages,from,position);});});
        card.addEventListener('dragend',()=>dragId=null);
        const actions=text('div','','pdf-page-actions');
        const commands=[
          ['left','Geser ke kiri','←',()=>move(id,-1),position===0],
          ['right','Geser ke kanan','→',()=>move(id,1),position===cards.length-1]
        ];
        if(tool==='organize')commands.push(
          ['rotate','Putar 90 derajat','↻',()=>mutate(()=>p.rotation=(p.rotation+90)%360),false],
          ['duplicate',s.duplicable===false?'Duplikasi tidak didukung untuk sumber bertag atau beranotasi':'Duplikat','⧉',()=>{if(s.duplicable===false)return;if(pages.length>=L.pages){feedback(messages.LIMIT,true);return;}mutate(()=>pages.splice(position+1,0,{...p,id:++pageId}));},pages.length>=L.pages||s.duplicable===false],
          ['remove','Keluarkan halaman','×',()=>mutate(()=>pages=pages.filter(v=>v.id!==id)),pages.length===1]
        );
        if(merge)commands.push(['remove','Keluarkan dokumen','×',()=>mutate(()=>removeSource(s.id)),false]);
        for(const [action,label,icon,handler,disabled] of commands){
          const b=text('button',icon,'button');b.type='button';b.title=label;b.dataset.tip=label;b.setAttribute('aria-label',label+' '+identity);b.dataset.action=action;b.dataset.unavailable=String(disabled);b.disabled=disabled||busy;b.onclick=handler;actions.append(b);
        }
        card.append(actions);
      }
      if(tool==='split'){
        const zoom=text('button','⌕','split-zoom');zoom.type='button';zoom.title='Perbesar halaman '+(p.index+1);zoom.setAttribute('aria-label','Perbesar halaman '+(p.index+1));zoom.dataset.tip='Perbesar halaman '+(p.index+1);zoom.onclick=e=>{e.stopPropagation();paper.click();};card.append(zoom);
        card.addEventListener('click',e=>{if(splitMode!=='select'||splitPageMode!=='select'||e.target.closest('.split-zoom,.pdf-real-preview'))return;const index=p.index;if(splitSelection.has(index))splitSelection.delete(index);else splitSelection.add(index);$('pdf-split-selected-pages').value=[...splitSelection].sort((a,b)=>a-b).map(i=>i+1).join(', ');splitSync=true;splitSummary();splitSync=false;syncSplitCards();});
        card.addEventListener('keydown',e=>{if((e.key==='Enter'||e.key===' ')&&splitMode==='select'&&splitPageMode==='select'){e.preventDefault();card.click();}});
        card.addEventListener('mouseenter',()=>{splitHover=Number(card.dataset.groupIndex);syncSplitCards();});card.addEventListener('mouseleave',()=>{splitHover=-1;syncSplitCards();});
      }
      grid.append(card);
    });
    if(modern){$('pdf-output-cover').replaceChildren();if(output&&tool!=='split'){const item=preview.button(outputSource(),0);$('pdf-output-cover').append(item.target);previews.push(item);}if(tool==='split'&&showResult)previews.push(...resultPreviewItems);}
    preview?.update(previews);
    if(tool==='split'){splitSummary();syncSplitCards();}
    $('pdf-card-hint').textContent=images?'Seret gambar untuk mengubah urutan. Pilihan kartu juga tersedia saat difokuskan.':merge?'Seret kartu untuk mengatur urutan dokumen. Klik halaman untuk memperbesar dan melihat halaman lainnya.':tool==='organize'?'Seret kartu untuk mengatur urutan. Klik halaman untuk memperbesar. Nomor sumber tetap meskipun posisi berubah.':'Pratinjau halaman sumber. Klik untuk memperbesar; perubahan alat terlihat pada PDF hasil unduhan.';
    setBusy(busy);restoreFocus(focused);
  }
  async function addFiles(files) {
    if(busy||document.querySelector('dialog[open]'))return;
    files=Array.from(files);if(!files.length){feedback(messages.TYPE,true);return;}
    const audit=window.OtoDiagnostics?.begin(tool,'input');
    if(files.some(f=>!profile.accepts(f))){audit?.fail(Error('TYPE'));feedback(messages.TYPE,true);return;}
    if(!multi && (files.length!==1 || sources.length)){audit?.fail(Error('SINGLE'));feedback(messages.SINGLE,true);return;}
    const bytes=files.reduce((n,f)=>n+f.size,0)+sources.reduce((n,s)=>n+byteSize(s.bytes),0);
    if(files.some(f=>f.size>L.fileBytes)||files.length+sources.length>L.files||bytes>L.totalBytes){audit?.fail(Error('LIMIT'));feedback(messages.LIMIT,true);return;}
    if(!fileBacked&&session.canAdd&&!session.canAdd(files.reduce((n,f)=>n+f.size,0),files.length)){audit?.fail(Error('SESSION_LIMIT'));feedback(messages.SESSION_LIMIT,true);return;}
    const token=++sequence;let acceptedSource=null;showResult=false;setBusy(true);feedback('Memeriksa '+(images?'gambar':'PDF')+' di perangkat ini…');
    try{
      let buffers=fileBacked?files:await Promise.all(files.map(f=>f.arrayBuffer()));
      if(token!==sequence)throw Error('CANCELLED');
      let result;
      if(fileBacked&&compat){const prepared=await compat.prepare(files,incoming=>job({action:'inspect',tool,files:incoming}));files=prepared.files;buffers=files;result=prepared.result;}
      else result=await job(fileBacked?{action:'inspect',tool,files:images?[...sources.map(s=>s.bytes),...files]:files}:OtoPdfSession.inspection(tool,sources,buffers));
      if(token!==sequence)throw Error('CANCELLED');
      const counts=OtoPdfSession.additions(tool,sources,pages,result,L),offset=images?sources.length:0;
      if(counts.length!==files.length||(!images&&result.sources?.length!==files.length))throw Error('VERIFY');
      releaseResults();
      const added=files.map((file,i)=>({id:++sourceId,label:M.sourceLabel(sourceId),name:file.name,bytes:buffers[i],count:counts[i],tagged:result.sources?.[i]?.tagged===true,duplicable:result.sources?.[i]?.duplicable,geometry:result.sources?.[i]?.geometry,warnings:result.sources?.[i]?.warnings||[],thumbnail:images?URL.createObjectURL(new Blob([result.thumbnails[offset+i]],{type:'image/jpeg'})):null}));
      sources.push(...added);
      pages.push(...added.flatMap(s=>Array.from({length:s.count},(_,index)=>({id:++pageId,sourceId:s.id,index,rotation:0}))));
      if(tool==='split'){splitRanges=[{start:1,end:added[0].count}];splitSelection=new Set(Array.from({length:added[0].count},(_,i)=>i));renderRangeRows();$('pdf-split-selected-pages').value=Array.from(splitSelection,i=>i+1).join(', ');}
      acceptedSource=added[0].id;
      if(!$('pdf-output-name').dataset.edited)$('pdf-output-name').value=OtoPdf.cleanName(sources[0].name.replace(/\.(pdf|jpg|jpeg|png)$/i,'')+'_'+tool);
      if(['split','remove','extract','rotate'].includes(tool)&&$('pdf-ranges')&&!$('pdf-ranges').value && sources[0].count){
        const count=sources[0].count;
        $('pdf-ranges').value=tool==='split'?(count>1?'1; 2'+(count>2?'-'+count:''):''):tool==='remove'?'1':count>1?'1-'+count:'1';
      }
      feedback((images?'Gambar':'PDF')+' siap. Atur hasil di panel pengaturan, lalu jalankan alat.');
    audit?.success();
    }catch(e){audit?.fail(e);feedback(messages[e.message]||messages.INVALID,true);}
    finally{
      if(controller.disposed)return;
      setBusy(false);render();
      if(!document.querySelector('dialog[open]')){
        const card=acceptedSource&&Array.from($('pdf-page-list').children).find(el=>el.dataset.sourceId===String(acceptedSource));
        (card||$('pdf-feedback')).focus({preventScroll:true});
      }
    }
  }
  function pickNewSplit(){if(tool==='split'&&!busy)$('pdf-new-files')?.click();}
  async function replaceSplitFile(files){
    if(tool!=='split'||busy)return;files=Array.from(files||[]);if(!files.length)return;
    let file=files[0];const audit=window.OtoDiagnostics?.begin(tool,'input');
    if(files.length!==1||!profile.accepts(file)){audit?.fail(Error('TYPE'));feedback(messages.TYPE,true);return;}
    if(file.size>L.fileBytes||file.size>L.totalBytes){audit?.fail(Error('LIMIT'));feedback(messages.LIMIT,true);return;}
    if(!fileBacked&&session.canReplace&&!session.canReplace(controller,file.size,1)){audit?.fail(Error('SESSION_LIMIT'));feedback(messages.SESSION_LIMIT,true);return;}
    const token=++sequence;let accepted=false;setBusy(true);feedback('Memeriksa PDF baru di perangkat ini. Hasil sebelumnya tetap tersedia sampai pemeriksaan selesai.');
    try{
      let bytes=fileBacked?file:await file.arrayBuffer();if(token!==sequence)throw Error('CANCELLED');
      let inspected;if(fileBacked&&compat){const prepared=await compat.prepare([file],incoming=>job({action:'inspect',tool,files:incoming}));file=prepared.files[0];bytes=file;inspected=prepared.result;}else inspected=await job(fileBacked?{action:'inspect',tool,files:[file]}:OtoPdfSession.inspection(tool,[],[bytes]));if(token!==sequence)throw Error('CANCELLED');
      const counts=OtoPdfSession.additions(tool,[],[],inspected,L);if(counts.length!==1||inspected.sources?.length!==1)throw Error('VERIFY');
      const replacement={id:1,label:M.sourceLabel(1),name:file.name,bytes,count:counts[0],tagged:inspected.sources[0]?.tagged===true,duplicable:inspected.sources[0]?.duplicable,geometry:inspected.sources[0]?.geometry,warnings:inspected.sources[0]?.warnings||[],thumbnail:null};
      releaseResults();releaseSources();sources=[replacement];sourceId=1;pages=Array.from({length:replacement.count},(_,index)=>({id:++pageId,sourceId:1,index,rotation:0}));
      splitRanges=[{start:1,end:replacement.count}];splitSelection=new Set(Array.from({length:replacement.count},(_,index)=>index));$('pdf-split-selected-pages').value=Array.from(splitSelection,index=>index+1).join(', ');if(splitMode==='fixed')$('pdf-split-pages-per-file').value=String(Math.min(2,replacement.count));renderRangeRows();updateSplitTabs();$('pdf-output-name').value=OtoPdf.cleanName(file.name.replace(/\.pdf$/i,'')+'_split');delete $('pdf-output-name').dataset.edited;showResult=false;accepted=true;audit?.success();feedback('PDF baru siap. Hasil sebelumnya sudah dilepas setelah PDF baru lolos pemeriksaan.');
    }catch(error){audit?.fail(error);feedback(messages[error.message]||messages.INVALID,true);}
    finally{if(!controller.disposed){setBusy(false);render();if(accepted){const card=Array.from($('pdf-page-list').children).find(el=>el.dataset.sourceId==='1');(card||$('pdf-feedback')).focus({preventScroll:true});}}}
  }
  function pickFiles(){if(busy)return;if(!multi&&sources.length){feedback(messages.SINGLE,true);return;}$('pdf-files').click();}
  $('pdf-add').onclick=()=>input.pick();
  $('pdf-empty-pick').onclick=()=>input.pick();
  $('pdf-cancel').onclick=()=>{resultView?.cancel();sequence++;compat?.cancel();activeJob?.cancel();feedback(messages.CANCELLED);};
  $('pdf-reset-order').onclick=()=>mutate(()=>{if(merge)sources=sources.slice().sort((a,b)=>a.id-b.id);pages=makePages();});
  $('pdf-zoom').onchange=e=>{root.dataset.cardSize=['compact','normal','large'].includes(e.target.value)?e.target.value:'normal';};
  $('pdf-show-result').onclick=showResults;
  if($('pdf-result-back'))$('pdf-result-back').onclick=()=>{showResult=false;syncView();($('pdf-page-list').firstElementChild||$('pdf-empty-pick')).focus();};
  $('pdf-output-name').oninput=()=>{$('pdf-output-name').dataset.edited='true';releaseResults();};
  function renderRangeRows(){
    const host=$('pdf-split-range-rows');if(!host)return;host.replaceChildren();
    splitRanges.forEach((range,index)=>{const row=text('div','','split-range-row');row.dataset.rangeRow=String(index);const makeField=(caption,key,value)=>{const label=text('label',caption),input=document.createElement('input');input.type='number';input.min='1';input.inputMode='numeric';input.dataset[key]='';input.id=`pdf-split-${key}-${index}`;input.value=String(value);label.append(input);input.addEventListener('input',()=>{releaseResults();splitSummary();syncSplitCards();});return label;};row.append(makeField('Dari halaman','start',range.start),makeField('Sampai halaman','end',range.end));const remove=text('button','×','split-remove-range');remove.type='button';remove.setAttribute('aria-label','Hapus rentang '+(index+1));remove.disabled=splitRanges.length===1;remove.onclick=()=>{captureSplitRanges();splitRanges.splice(index,1);renderRangeRows();releaseResults();splitSummary();render();};row.addEventListener('mouseenter',()=>{splitHover=index;syncSplitCards();});row.addEventListener('mouseleave',()=>{splitHover=-1;syncSplitCards();});row.append(remove);host.append(row);});
    $('pdf-split-add-range').disabled=splitRanges.length>=L.outputs;
  }
  function captureSplitRanges(){const rows=Array.from($('pdf-split-range-rows')?.querySelectorAll('.split-range-row')||[]);if(rows.length)splitRanges=rows.map(row=>({start:Number(row.querySelector('[data-start]').value),end:Number(row.querySelector('[data-end]').value)}));}
  function updateSplitTabs(){
    const ranges=splitMode==='custom'||splitMode==='fixed';$('pdf-split-range-panel').hidden=!ranges;$('pdf-split-page-panel').hidden=ranges;
    $('pdf-split-tab-ranges').setAttribute('aria-selected',String(ranges));$('pdf-split-tab-pages').setAttribute('aria-selected',String(!ranges));
    $('pdf-split-custom-panel').hidden=splitMode!=='custom';$('pdf-split-fixed-panel').hidden=splitMode!=='fixed';
    $('pdf-split-custom').setAttribute('aria-pressed',String(splitMode==='custom'));$('pdf-split-fixed').setAttribute('aria-pressed',String(splitMode==='fixed'));
    $('pdf-split-all-help').hidden=splitPageMode!=='all';$('pdf-split-select-panel').hidden=splitPageMode!=='select';
    $('pdf-split-extract-all').setAttribute('aria-pressed',String(splitPageMode==='all'));$('pdf-split-select-pages').setAttribute('aria-pressed',String(splitPageMode==='select'));
    $('pdf-run').textContent=(ranges?'Pisahkan PDF':'Ekstrak halaman')+' →';
  }
  function chooseSplitMode(mode){splitMode=mode;releaseResults();updateSplitTabs();splitSummary();render();}
  if(tool==='split'&&$('pdf-split-tab-ranges')){
    splitRanges=[{start:1,end:1}];renderRangeRows();updateSplitTabs();
    $('pdf-split-tab-ranges').onclick=()=>chooseSplitMode('custom');$('pdf-split-tab-pages').onclick=()=>chooseSplitMode('all');
    $('pdf-split-custom').onclick=()=>chooseSplitMode('custom');$('pdf-split-fixed').onclick=()=>chooseSplitMode('fixed');
    $('pdf-split-extract-all').onclick=()=>{splitMode='all';splitPageMode='all';releaseResults();updateSplitTabs();splitSummary();render();};
    $('pdf-split-select-pages').onclick=()=>{splitMode='select';splitPageMode='select';if(!splitSelection.size&&sources[0])splitSelection=new Set(Array.from({length:sources[0].count},(_,i)=>i));$('pdf-split-selected-pages').value=[...splitSelection].sort((a,b)=>a-b).map(i=>i+1).join(', ');releaseResults();updateSplitTabs();splitSummary();render();};
    $('pdf-split-add-range').onclick=()=>{captureSplitRanges();const last=splitRanges.at(-1),next=last?.end+1||1;splitRanges.push({start:next,end:next});renderRangeRows();releaseResults();splitSummary();render();};
    $('pdf-split-pages-per-file').addEventListener('input',()=>{releaseResults();splitSummary();syncSplitCards();});
    $('pdf-split-less').onclick=()=>{const el=$('pdf-split-pages-per-file');el.value=String(Math.max(1,Number(el.value||1)-1));el.dispatchEvent(new Event('input'));};
    $('pdf-split-more').onclick=()=>{const el=$('pdf-split-pages-per-file');el.value=String(Math.min(sources[0]?.count||200,Number(el.value||1)+1));el.dispatchEvent(new Event('input'));};
    $('pdf-split-combine-ranges').onchange=()=>{releaseResults();splitSummary();render();};$('pdf-split-combine-selected').onchange=()=>{releaseResults();splitSummary();render();};
    $('pdf-split-selected-pages').addEventListener('input',()=>{if(splitSync)return;releaseResults();const chosen=sources[0]?splitPagesFromText($('pdf-split-selected-pages').value,sources[0].count):[];splitSelection=new Set(chosen||[]);splitSummary();syncSplitCards();});
    $('pdf-split-select-all').onclick=()=>{if(!sources[0])return;splitSelection=new Set(Array.from({length:sources[0].count},(_,i)=>i));$('pdf-split-selected-pages').value=Array.from(splitSelection,i=>i+1).join(', ');releaseResults();splitSummary();syncSplitCards();};
    $('pdf-split-clear-selection').onclick=()=>{splitSelection.clear();$('pdf-split-selected-pages').value='';releaseResults();splitSummary();syncSplitCards();};
  }
  $('pdf-ranges')?.addEventListener('input',releaseResults);
  $('pdf-angle')?.addEventListener('change',releaseResults);
  root.querySelectorAll('[data-option]').forEach(el=>el.addEventListener('input',releaseResults));
  $('pdf-run').onclick=async()=>{
    const currentSplitPlan=tool==='split'?splitPlan():null;
    if(busy||!M.readiness(tool,sources,pages).ready||(tool==='split'&&!currentSplitPlan.valid)){if(tool==='split'&&!currentSplitPlan.valid)feedback(currentSplitPlan.error||'Periksa pilihan halaman.',true);return;}
    const audit=lastAudit=window.OtoDiagnostics?.begin(tool,'process');
    const token=++sequence;let completed=false;releaseResults();setBusy(true);feedback('Memproses di browser… Jangan tutup halaman ini.');$('pdf-feedback').focus({preventScroll:true});
    try{
      const options={};
      for(const el of root.querySelectorAll('[data-option]'))options[el.dataset.option]=(el.type==='number'||el.dataset.numeric==='true')?(el.value===''?NaN:Number(el.value)):el.value;
      const ordered=M.input(tool,sources,pages),input=fileBacked?{files:ordered.buffers,pages:ordered.pages}:ordered;
      const splitRequest=tool==='split'?{splitMode,ranges:splitMode==='custom'?Array.from($('pdf-split-range-rows').children,row=>`${row.querySelector('[data-start]').value}-${row.querySelector('[data-end]').value}`).join(';'):undefined,pagesPerFile:Number($('pdf-split-pages-per-file').value),combineRanges:$('pdf-split-combine-ranges').checked,selectedPages:currentSplitPlan.pages,combineSelected:$('pdf-split-combine-selected').checked}:{};
      const result=await job({action:'run',tool,...input,ranges:$('pdf-ranges')?.value,...splitRequest,angle:Number($('pdf-angle')?.value),options,name:$('pdf-output-name').value});
      if(token!==sequence)throw Error('CANCELLED');
      const size=result.results.reduce((n,item)=>n+(fileBacked?item.file.size:item.bytes.byteLength),0);
      if(!fileBacked&&session.canResult&&!session.canResult(size))throw Error('SESSION_LIMIT');
      resultBytes=size;
      result.warnings=[...new Set([...(result.warnings||[]),...sources.flatMap(s=>s.warnings||[]).filter(w=>w==='COPY_STATIC'||w==='COPY_RASTER')])];
      if(modern){if(!result.results.length||(tool!=='split'&&result.results.length!==1))throw Error('VERIFY');const sourceGroups=tool==='split'?currentSplitPlan.groups.map(group=>group.map(index=>index+1)):[];outputs=result.results.map((item,index)=>({name:item.name,count:item.count,geometry:item.geometry,file:fileBacked?item.file:new Blob([item.bytes],{type:'application/pdf'}),sourcePages:sourceGroups[index]||[],warnings:result.warnings||[],saved:false}));output=outputs[0];resultView.present(output);}
      else {
      for(const item of result.results){
        const file=fileBacked?item.file:new Blob([item.bytes],{type:'application/pdf'}),url=URL.createObjectURL(file);resultUrls.push(url);
        const row=text('div','','pdf-result-row'),info=text('div','');
        info.append(text('strong',item.name),text('small',item.count+' halaman · '+(file.size/1024).toFixed(1)+' KB'));
        const link=text('a','↓ Unduh PDF','button primary');link.href=url;link.download=item.name;
        link.addEventListener('click',()=>resultFeedback('Unduhan dimulai. Periksa daftar unduhan browser untuk memastikan file sudah tersimpan.'));
        row.append(info,link);$('pdf-result-files').append(row);
      }
      $('pdf-result-summary').textContent=result.results.length+' file PDF · '+result.results.reduce((n,item)=>n+item.count,0)+' halaman';
      $('pdf-result-compatibility').textContent=compatibilityText(result.warnings);$('pdf-result-compatibility').hidden=!$('pdf-result-compatibility').textContent;
      resultFeedback('Hasil siap diunduh. Belum disimpan ke perangkat.');
      }
      audit?.success();completed=true;showResult=true;feedback(result.results.length+' PDF baru dibuat.');
    }catch(e){audit?.fail(e);releaseResults();feedback(messages[e.message]||messages.INVALID,true);}
    finally{if(!controller.disposed){setBusy(false);render();if(completed)showResults();else if(!document.querySelector('dialog[open]'))$('pdf-feedback').focus({preventScroll:true});}}
  };
  function confirm(action){if(document.querySelector('dialog[open]'))return;pending=action;$('pdf-confirm-dialog').showModal();$('pdf-stay').focus();}
  $('pdf-stay').onclick=()=>{$('pdf-confirm-dialog').close();pending=null;};
  $('pdf-confirm').onclick=()=>{$('pdf-confirm-dialog').close();const action=pending;pending=null;action?.();};
  $('pdf-confirm-dialog').addEventListener('cancel',()=>{pending=null;});
  function resetWorkspace(){sequence++;compat?.cancel();activeJob?.cancel();releaseSources();pages=[];sourceId=0;pageId=0;releaseResults();$('pdf-compatibility').textContent='';$('pdf-compatibility').hidden=true;if($('pdf-ranges'))$('pdf-ranges').value='';if(tool==='split'){$('pdf-split-pages-per-file').value='2';$('pdf-split-combine-ranges').checked=false;$('pdf-split-combine-selected').checked=false;$('pdf-split-selected-pages').value='';splitMode='custom';splitPageMode='all';splitRanges=[{start:1,end:1}];splitSelection.clear();renderRangeRows();updateSplitTabs();}$('pdf-output-name').value=initialOutputName;delete $('pdf-output-name').dataset.edited;render();feedback('Ruang kerja dikosongkan. File asli tetap utuh.');$('pdf-empty-pick').focus();}
  function closeWorkspace(){if(!busy)confirm(resetWorkspace);}
  if(tool==='split'&&$('pdf-edit-split'))$('pdf-edit-split').onclick=()=>{if(busy)return;showResult=false;render();feedback('Ubah pembagian lalu jalankan kembali. Hasil sebelumnya tersedia sampai pengaturan diubah.');$(splitMode==='custom'||splitMode==='fixed'?'pdf-split-tab-ranges':'pdf-split-tab-pages').focus();};
  $('pdf-close').onclick=closeWorkspace;if($('pdf-result-new'))$('pdf-result-new').onclick=tool==='split'?pickNewSplit:closeWorkspace;
  if(tool==='split'&&$('pdf-new-files'))$('pdf-new-files').addEventListener('change',event=>{const files=Array.from(event.target.files||[]);event.target.value='';void replaceSplitFile(files);});
  const input=new OtoPdfUi.FileInputController(controller,root,{host:window,input:$('pdf-files'),clipboard:OtoPdfPaste,pick:pickFiles,addFiles,hasResult:()=>resultCount()>0,notify:feedback,resultMessage:modern?'Hasil sebelumnya tetap tersedia. Simpan hasil, lalu pilih Mulai baru sebelum menambahkan PDF berikutnya.':'Hasil sebelumnya tetap tersedia. Unduh hasil, lalu pilih Tutup dokumen sebelum menambahkan file berikutnya.'});
  listen(document,'click',e=>{
    const a=e.target.closest('a[href]');
    if(e.defaultPrevented||!a||(!sources.length&&!busy)||a.download||a.target==='_blank'||a.getAttribute('href').startsWith('#')||e.button!==0||e.ctrlKey||e.metaKey||e.shiftKey||e.altKey)return;
    e.preventDefault();confirm(()=>{sequence++;compat?.cancel();activeJob?.cancel();releaseSources();pages=[];releaseResults();setBusy(false);location.assign(a.href);});
  });
  if(seed.length&&!images&&(fileBacked||seed.some(s=>s.bytes instanceof Blob))){
    // Revalidate transfers across the file-backed/bounded boundary before reading.
    void addFiles(seed.map(s=>new File([s.bytes],s.name,{type:'application/pdf'})));
  }else if(seed.length&&!images){
    sources=seed.map((s,i)=>({...s,id:i+1,label:M.sourceLabel(i+1),thumbnail:null}));sourceId=sources.length;pages=makePages();
    $('pdf-output-name').value=OtoPdf.cleanName(sources[0].name.replace(/\.pdf$/i,'')+'_'+tool);
    const count=sources[0].count;
    if(tool==='split'){splitRanges=[{start:1,end:count}];splitSelection=new Set(Array.from({length:count},(_,i)=>i));$('pdf-split-selected-pages').value=Array.from(splitSelection,i=>i+1).join(', ');renderRangeRows();}
    if($('pdf-ranges'))$('pdf-ranges').value=tool==='remove'?'1':count>1?'1-'+count:'1';
    feedback('Dokumen sumber tersedia di alat ini. Susunan dan hasil alat sebelumnya tetap ada; perubahan belum diterapkan ke file sumber.');
  }
  render();
  return controller;
};

