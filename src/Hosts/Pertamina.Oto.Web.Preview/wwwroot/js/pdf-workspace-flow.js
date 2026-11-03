/* Presentation-only ports shared by local PDF workspaces. The caller owns PDF bytes. */
(function(root,factory){
 'use strict';const api=factory();if(typeof module==='object'&&module.exports)module.exports=api;else root.OtoPdfFlow=api;
})(typeof globalThis!=='undefined'?globalThis:this,function(){
 'use strict';
 const size=n=>n>=1e9?(n/1e9).toLocaleString('id-ID',{maximumFractionDigits:2})+' GB':n>=1e6?(n/1e6).toLocaleString('id-ID',{maximumFractionDigits:2})+' MB':(n/1e3).toLocaleString('id-ID',{maximumFractionDigits:1})+' kB';
 class ProgressController {
  constructor(element,prefix,notify){this.element=element;this.$=id=>element.querySelector('#'+prefix+'-'+id);this.notify=notify;}
  reset(){this.$('meter').removeAttribute('value');this.$('percent').textContent='—';this.$('progress-count').textContent='';this.$('progress-title').textContent='Menyiapkan proses';this.$('progress-detail').textContent='Menyiapkan proses lokal…';}
  update(p){
   const labels={download:'Mengambil PDF pilihan dari cloud',inspect:'Memeriksa PDF',copy:'Menyusun halaman',write:'Menulis hasil lokal',verify:'Memeriksa hasil'};
   const detail=p.total?(['write','download'].includes(p.stage)?size(p.done)+' / '+size(p.total):p.done+' / '+p.total):'Sedang dikerjakan…';
   this.notify?.(`${labels[p.stage]||'Memproses'} · ${detail}`);this.$('progress-title').textContent=labels[p.stage]||'Memproses';this.$('progress-detail').textContent=detail;
   this.$('progress-count').textContent='Tahap '+(p.stage==='copy'?'susun':['write','verify'].includes(p.stage)?'hasil':'periksa');
   this.$('percent').textContent=p.total?Math.min(100,Math.floor(p.done/p.total*100))+'%':'—';
   if(p.total){this.$('meter').max=p.total;this.$('meter').value=p.done;}else this.$('meter').removeAttribute('value');
   const stage=['write','verify'].includes(p.stage)?'verify':p.stage==='copy'?'copy':'inspect';
   this.element.querySelectorAll('[data-stage]').forEach(el=>{el.classList.toggle('active',el.dataset.stage===stage);if(el.dataset.stage===stage)el.setAttribute('aria-current','step');else el.removeAttribute('aria-current');});
  }
 }
 class ResultController {
  constructor(element,prefix,ports){this.$=id=>element.querySelector('#'+prefix+'-'+id);this.document=element.ownerDocument;this.ports=ports;this.compact=ports.compactResults===true;this.url=null;this.abort=null;this.archiveMessage=null;
   this.$('save').onclick=()=>this.save();this.$('download').onclick=()=>this.download();this.$('result-name').onchange=()=>this.name();
   const select=this.$('result-select');if(select&&ports.results)select.onchange=()=>this.select(Number(select.value));
   if(this.$('result-prev'))this.$('result-prev').onclick=()=>this.select(ports.results().indexOf(ports.result())-1);
   if(this.$('result-next'))this.$('result-next').onclick=()=>this.select(ports.results().indexOf(ports.result())+1);
   if(this.$('save-all'))this.$('save-all').onclick=()=>this.saveAll();
  }
  select(index){const p=this.ports;if(p.busy())return false;if(!this.name()){this.choices();return false;}if(!Number.isInteger(index)||!p.results()[index])return false;p.select(index);this.present(p.result());p.render();return true;}
  get objectUrls(){return (this.url?1:0)+(this.archiveUrl?1:0);}
  choices(){const select=this.$('result-select');if(!select||!this.ports.results)return;const results=this.ports.results();select.replaceChildren(...results.map((item,index)=>{const option=this.document.createElement('option');option.value=String(index);option.textContent=`${index+1}. ${item.name} · ${item.count} halaman${item.saved?' · tersimpan':''}`;return option;}));const index=results.indexOf(this.ports.result());select.value=String(index);this.$('result-total').textContent=this.compact?`${results.length} file PDF siap diunduh`:`${results.length} file PDF · ${results.reduce((n,item)=>n+item.count,0)} halaman · ${results.filter(item=>item.saved).length} tersimpan`;
   if(this.compact)this.list(results);
   for(const [id,disabled] of [['result-prev',index<=0],['result-next',index>=results.length-1]]){const button=this.$(id);if(button){button.dataset.unavailable=String(disabled);button.disabled=disabled||this.ports.busy();}}
   this.ports.render?.();
  }
  list(results){
   const list=this.$('result-list');if(!list)return;
   const active=this.document.activeElement,focus=active?.dataset?.resultAction?{index:Number(active.dataset.resultIndex),action:active.dataset.resultAction}:null;
   const create=(tag,value,style)=>{const node=this.document.createElement(tag);node.textContent=value;if(style)node.className=style;return node;},previews=[];
   this.$('result-summary').textContent=`File hasil (${results.length})`;
   this.$('save-all').textContent=results.length===1?'↓ Unduh PDF':'↓ Unduh semua (ZIP)';
   this.$('save-all').setAttribute('aria-label',this.$('save-all').textContent.replace(/^↓ /,''));
   const count=results.reduce((n,item)=>n+item.count,0),total=results.reduce((n,item)=>n+item.file.size,0);
   this.$('result-total').textContent=`${results.length} file PDF · ${count} halaman · ${size(total)}`;
   const source=this.ports.sourceName?.();if(this.$('result-source'))this.$('result-source').textContent=source?`Hasil dari ${source}`:'';
   const requested=results.find(item=>item.downloadRequested);this.$('archive-note').textContent=this.archiveMessage||(requested?`Unduhan ${requested.name} dimulai. Periksa folder unduhan Anda.`:results.length===1?'PDF akan diunduh langsung ke perangkat Anda.':'Satu ZIP berisi semua PDF hasil. Buka atau ekstrak ZIP setelah diunduh.');
   list.replaceChildren(...results.map((item,index)=>{
    const row=create('article','','split-file-card'),thumb=create('div','','split-file-thumb'),info=create('div','','split-file-info'),actions=create('div','','split-file-actions'),menu=this.document.createElement('details'),menuButton=create('summary','⋯','split-file-menu-button'),menuContent=create('div','','split-file-menu');
    row.dataset.resultIndex=String(index);menu.className='split-file-menu-wrap';menuButton.setAttribute('aria-label',`Opsi ${item.name}`);menu.append(menuButton);
    const sourcePages=item.sourcePages||[],range=this.ports.formatSourcePages?.(sourcePages)||'';
    const preview=this.ports.previewThumbnail?.(item,index);if(preview){thumb.append(preview.target);previews.push(preview);}else thumb.append(create('span','PDF','split-file-icon'));
    info.append(create('strong',item.name),create('small',range?`Halaman sumber ${range}`:`${item.count} halaman`),create('small',`${item.count} halaman · ${size(item.file.size)}`));if(item.saved||item.downloadRequested)info.append(create('small',item.saved?'Tersimpan ke perangkat':'Unduhan dimulai…','split-file-download-status'));
    const previewButton=create('button','◉  Pratinjau','merge-text-action'),previewLink=create('button','Lihat PDF','split-file-zoom');previewButton.type=previewLink.type='button';previewButton.dataset.resultIndex=String(index);previewButton.dataset.resultAction='preview';previewLink.dataset.resultIndex=String(index);previewLink.dataset.resultAction='preview';previewButton.setAttribute('aria-label',`Pratinjau ${item.name}`);previewLink.setAttribute('aria-label',`Lihat PDF ${item.name}`);
    const openPreview=()=>{if(!this.select(index))return;this.ports.openResult?.(this.rowButton(index,'preview'),index);};previewButton.onclick=openPreview;if(preview)preview.target.onclick=openPreview;previewLink.onclick=openPreview;
    actions.append(previewButton);if(results.length>1){const download=create('button','↓  Unduh file','merge-text-action');download.type='button';download.dataset.resultIndex=String(index);download.dataset.resultAction='download';download.setAttribute('aria-label',`Unduh file ${item.name}`);download.onclick=()=>{if(this.select(index)){this.download();this.rowButton(index,'download')?.focus();}};actions.append(download);}
    const rename=create('button','Ubah nama','split-file-menu-action'),save=create('button','Simpan sebagai…','split-file-menu-action'),renameArea=create('div','','split-file-rename-area'),renameInput=this.document.createElement('input'),renameError=create('small','','split-file-rename-error'),renameApply=create('button','Terapkan nama','button');rename.type=save.type=renameApply.type='button';renameInput.type='text';renameInput.value=item.name;renameInput.maxLength=110;renameInput.autocomplete='off';renameInput.setAttribute('aria-label',`Nama file ${index+1}`);renameError.hidden=true;
    const commitName=()=>{const value=renameInput.value.trim(),invalid=!value||/[\\/:*?"<>|]/.test(value),clean=invalid?'':this.ports.cleanName(value),duplicate=clean&&results.some((other,otherIndex)=>otherIndex!==index&&other.name.toLowerCase()===clean.toLowerCase());if(invalid||duplicate){renameInput.setAttribute('aria-invalid','true');renameError.hidden=false;renameError.textContent=invalid?'Nama wajib diisi dan tidak boleh memuat \\ / : * ? " < > |.':'Gunakan nama berbeda untuk setiap file.';renameInput.focus();return;}item.name=clean;item.saved=false;renameInput.value=clean;renameInput.setAttribute('aria-invalid','false');renameError.hidden=true;this.ports.renameResult?.(index,clean);this.choices();this.ports.render?.();};
    rename.onclick=()=>{renameArea.hidden=!renameArea.hidden;if(!renameArea.hidden){renameInput.value=item.name;renameInput.focus();}};renameApply.onclick=commitName;renameInput.addEventListener('keydown',event=>{if(event.key==='Enter'){event.preventDefault();commitName();}});save.onclick=()=>{if(this.select(index))void this.save();};menuContent.append(rename,save);renameArea.hidden=true;renameArea.append(renameInput,renameError,renameApply);menuContent.append(renameArea);menu.append(menuContent);
    thumb.append(previewLink);row.append(thumb,info,menu,actions);return row;
   }));
   this.ports.resultPreviews?.(previews);
   this.ports.renderNextTools?.(results);
   if(focus)this.rowButton(focus.index,focus.action)?.focus();
  }
  rowButton(index,action){return Array.from(this.$('result-list')?.children[index]?.querySelectorAll('button')||[]).find(button=>button.dataset.resultAction===action);}
  revealName(){const index=this.ports.results?.().indexOf(this.ports.result())??-1,card=this.$('result-list')?.children[index],menu=card?.querySelector('.split-file-menu-wrap'),area=menu?.querySelector('.split-file-rename-area'),input=area?.querySelector('input');if(input){menu.open=true;area.hidden=false;input.focus();}else this.$('result-name').focus();}
  present(result){this.reset();this.choices();this.$('result-name').value=result.name;this.$('result-name').setAttribute('aria-invalid','false');this.$('result-name-error').hidden=true;
   this.$('result-info').textContent=`${result.count.toLocaleString('id-ID')} halaman · ${size(result.file.size)}`;
   const warnings=[...new Set(result.warnings||[])];this.$('result-notes').hidden=!warnings.length;this.$('result-notes').open=false;this.$('result-notes-summary').textContent=`${warnings.length} catatan dokumen`;
   this.$('result-warning').replaceChildren(...this.ports.messages.details(warnings).map(value=>{const li=this.document.createElement('li');li.textContent=value;return li;}));
   this.$('save').textContent=this.compact?'Simpan sebagai…':result.saved?'Simpan lagi':'Simpan PDF';this.$('download').hidden=true;this.$('save-note').textContent=result.saved?'Bagian ini sudah disimpan ke perangkat.':'Belum disimpan ke perangkat.';
  }
  name(){const result=this.ports.result(),value=this.$('result-name').value.trim(),invalid=!value||/[\\/:*?"<>|]/.test(value);this.$('result-name').setAttribute('aria-invalid',String(invalid));this.$('result-name-error').hidden=!invalid;
   if(invalid){this.$('result-name-error').textContent='Isi nama tanpa karakter \\ / : * ? " < > |.';this.revealName();return false;}const clean=this.ports.cleanName(value);if(this.ports.results?.().some(item=>item!==result&&item.name.toLowerCase()===clean.toLowerCase())){this.$('result-name').setAttribute('aria-invalid','true');this.$('result-name-error').hidden=false;this.$('result-name-error').textContent='Gunakan nama berbeda untuk setiap bagian.';this.revealName();return false;}if(result){const renamed=result.name!==clean;this.ports.rename(clean);if(renamed&&this.ports.results)this.$('save-note').textContent='Nama diperbarui. Simpan file dengan nama ini.';}this.choices();return true;
  }
  download(){const result=this.ports.result();if(!result||this.ports.busy()||!this.name())return;if(!this.url)this.url=URL.createObjectURL(result.file);
   const a=this.document.createElement('a');a.href=this.url;a.download=this.ports.result().name;a.click();this.ports.audit?.()?.record('download_requested','save');this.$('save-note').textContent='Unduhan dimulai. Periksa folder unduhan Anda.';this.$('save').textContent=this.compact?'Simpan sebagai…':'Simpan lagi';this.$('download').hidden=true;if(this.compact){result.downloadRequested=true;this.archiveMessage=`Unduhan ${result.name} dimulai. Periksa folder unduhan Anda.`;this.choices();}this.ports.notify('');this.ports.changed?.();
  }
  async save(){
   const result=this.ports.result();if(!result||this.ports.busy()||!this.name())return;
   if(typeof window.showSaveFilePicker!=='function'){this.download();return;}
   let writable,abortListener,written=false;const audit=this.ports.audit?.();const abort=this.abort=new AbortController(),signal=abort.signal;
   try{
    this.ports.setBusy(true);void this.ports.preview.clear();this.ports.notify('Pilih lokasi penyimpanan. Jika dialog tidak muncul, batalkan untuk memakai unduhan langsung.');
    const picker=window.showSaveFilePicker({suggestedName:this.ports.result().name,types:[{description:'Dokumen PDF',accept:{'application/pdf':['.pdf']}}]});
    const cancelled=new Promise((_,reject)=>{abortListener=()=>reject(new DOMException('Cancelled','AbortError'));signal.addEventListener('abort',abortListener,{once:true});});
    const handle=await Promise.race([picker,cancelled]);signal.removeEventListener('abort',abortListener);if(signal.aborted)throw new DOMException('Cancelled','AbortError');
    this.ports.notify('Menyimpan ke file pilihan Anda…');writable=await handle.createWritable();await result.file.stream().pipeTo(writable,{signal});writable=null;written=true;audit?.record('save_succeeded','save');
    await this.ports.saved();
   }catch(e){audit?.record(written?'failed':e.name==='AbortError'?'save_cancelled':'save_failed',written?'cleanup':'save',e);try{await writable?.abort();}catch{}if(!this.compact)this.$('download').hidden=false;this.$('save-note').textContent=e.name==='AbortError'?'Penyimpanan dibatalkan. Hasil tetap tersedia. Gunakan unduhan PDF langsung atau coba Simpan sebagai lagi.':'Belum tersimpan. Hasil tetap tersedia; coba lagi atau gunakan unduhan PDF langsung.';this.ports.notify(this.$('save-note').textContent);}
   finally{signal.removeEventListener('abort',abortListener);if(this.abort===abort)this.abort=null;this.ports.setBusy(false);this.ports.render();}
  }
  async saveAll(){
   const p=this.ports;if(p.busy()||!p.results?.().length||!this.name())return;
   if(this.compact&&p.results().length===1){this.download();return;}
   const results=p.results().map(item=>({...item})),name=results[0].name.replace(/(?:_\d+)?\.pdf$/i,'')+'_semua.zip';
   let lease;const abort=this.abort=new AbortController(),signal=abort.signal;
   try{
    const plan=OtoPdfArchive.plan(results);p.setBusy(true);void p.preview.clear();
    const options={signal,progress:(done,total)=>p.archiveProgress?.({stage:'write',done,total})};
    p.notify('Menyiapkan satu ZIP berisi semua bagian di perangkat ini...');
    await this.clearArchive();lease=await OtoPdfArchive.temporary(plan,options);
    if(signal.aborted)throw new DOMException('Cancelled','AbortError');
    this.archive=lease;lease=null;this.archiveUrl=URL.createObjectURL(this.archive.file);
    const a=this.document.createElement('a');a.href=this.archiveUrl;a.download=name;a.click();
    this.archiveMessage='Unduhan ZIP dimulai. Periksa folder unduhan, lalu buka ZIP untuk mengambil semua PDF.';this.$('archive-note').textContent=this.archiveMessage;p.audit?.()?.record('download_requested','save');
    p.notify('');
   }catch(e){this.archiveMessage=e.name==='AbortError'?'Pembuatan ZIP dibatalkan. Semua file tetap tersedia.':e.name==='QuotaExceededError'||e.message==='STORAGE'?'Ruang sementara tidak cukup untuk ZIP. Simpan setiap file sebagai PDF.':'ZIP belum tersedia. Coba unduh semua lagi atau simpan setiap file.';this.$('archive-note').textContent=this.archiveMessage;p.notify('');}
   finally{if(lease)await lease.release().catch(()=>{});if(this.abort===abort)this.abort=null;p.setBusy(false);this.choices();p.render();}
  }
  async clearArchive(){this.archiveMessage=null;if(this.$('archive-note'))this.$('archive-note').textContent=this.compact?'Satu ZIP berisi semua PDF hasil. Buka atau ekstrak ZIP setelah diunduh.':'Semua bagian dalam satu ZIP. Buka ZIP untuk mengambil file PDF-nya.';if(this.archiveUrl){URL.revokeObjectURL(this.archiveUrl);this.archiveUrl=null;}const old=this.archive;this.archive=null;if(old)try{await old.release();}catch{this.ports.notify('Pembersihan ZIP sementara akan dicoba kembali saat aplikasi dibuka.');}}
  cancel(){this.abort?.abort();}
  reset(){if(this.url){URL.revokeObjectURL(this.url);this.url=null;}this.$('result-warning').replaceChildren();this.$('result-list')?.replaceChildren();this.ports.resultPreviews?.([]);this.ports.changed?.();}
  dispose(){this.cancel();this.reset();void this.clearArchive();}
 }
 class SettingsController {
  constructor(element,prefix,input){const $=id=>element.querySelector('#'+prefix+'-'+id),content=element.querySelector('.merge-settings-content');
   $('settings-open').onclick=()=>{$('settings-body').append(content);$('settings').showModal();input.focus();};$('settings-close').onclick=()=>$('settings').close();
   $('settings').addEventListener('close',()=>{$('settings-home').append(content);$('settings-open').focus();});
  }
 }
 return {ProgressController,ResultController,SettingsController,size};
});
