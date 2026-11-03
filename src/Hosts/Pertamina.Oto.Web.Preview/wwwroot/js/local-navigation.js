/* Same-document navigation from server-authored inert templates. No fetch,
 * document persistence, HTML from files, inline script evaluation or uploads. */
(function(){
 'use strict';
 if(document.body.dataset.catalogOnly!=='true')return;
 const templates=new Map([...document.querySelectorAll('template[data-local-page]')].map(t=>[t.dataset.localPage,t]));
 const entries=new Map(),L=OtoPdf.LIMITS;
 let current=location.pathname,lastPdf=null,navigating=false;
 const notice=message=>{const e=document.getElementById('toast');e.textContent=message;e.hidden=false;clearTimeout(notice.timer);notice.timer=setTimeout(()=>e.hidden=true,5000);};
 const syncSwitch=()=>{const select=document.querySelector('[data-tool-switch]');if(select)select.value=current;};
 const resources=()=>OtoPdfSession.usage([...entries.values()].filter(e=>e.controller).map(e=>e.controller.resources));
 const session={canAdd:(bytes,count)=>{const used=resources();return used.files+count<=L.files&&used.inputBytes+bytes<=L.totalBytes;},canReplace:(owner,bytes,count)=>{const used=OtoPdfSession.usage([...entries.values()].filter(e=>e.controller&&e.controller!==owner).map(e=>e.controller.resources));return used.files+count<=L.files&&used.inputBytes+bytes<=L.totalBytes;},canResult:bytes=>resources().outputBytes+bytes<=L.outputBytes};
 function mount(screen,seed){return window.OtoBrowserMount(screen,seed,session);}
 const initial=document.getElementById('oto-screen');
 entries.set(current,{screen:initial,controller:mount(initial,[]),title:document.title,scroll:0});
 function sourceChoice(sources){
  const dialog=document.getElementById('local-source-dialog'),select=document.getElementById('local-source-choice');
  select.replaceChildren(...sources.map((source,i)=>{const option=document.createElement('option');option.value=String(i);option.textContent=source.name;return option;}));
  return new Promise(resolve=>{
   const use=document.getElementById('local-source-use'),cancel=document.getElementById('local-source-cancel');
   const finish=value=>{use.removeEventListener('click',yes);cancel.removeEventListener('click',no);dialog.removeEventListener('cancel',no);dialog.close();resolve(value);};
   const yes=()=>finish([sources[Number(select.value)]]),no=()=>finish(null);
   use.addEventListener('click',yes);cancel.addEventListener('click',no);dialog.addEventListener('cancel',no);dialog.showModal();select.focus();
  });
 }
 async function go(url,pop=false,forceSingleSource=false){
  if(url.origin!==location.origin||url.search||!templates.has(url.pathname))return false;
  const previous=entries.get(current),control=previous.controller;
  if(navigating||control?.busy()||document.querySelector('dialog[open]')){
   if(pop)history.replaceState(null,'',current);
   syncSwitch();notice('Selesaikan atau batalkan proses/dialog yang sedang terbuka dahulu.');return true;
  }
  if(url.pathname===current){if(url.hash)document.getElementById(url.hash.slice(1))?.scrollIntoView();return true;}
  navigating=true;
  try{
   if(control?.beforeLeave&&!await control.beforeLeave()){if(pop)history.replaceState(null,'',current);return true;}
   let entry=entries.get(url.pathname),remount=false;
   if(entry&&control?.tool==='split'&&control.transferSources?.().some(source=>String(source.id).startsWith('result-'))){
    const template=templates.get(url.pathname),screen=document.createElement('div');screen.id='oto-screen';screen.append(template.content.cloneNode(true));
    const workspace=screen.querySelector('[data-browser-tool]'),profile=workspace?OtoPdfUi.ToolProfile.from(workspace):null;let seed=profile?.transferSources?(control.transferSources?.()||[]):[];
    if(seed.length>1&&(forceSingleSource||!profile.multiple)){seed=await sourceChoice(seed);if(seed===null){if(pop)history.replaceState(null,'',current);return true;}}
    if(entry.controller?.hasWork?.()&&!window.confirm('Alat tujuan sudah memiliki pekerjaan lokal. Ganti dengan PDF hasil yang dipilih? Pekerjaan di alat tujuan saat ini akan dilepas.'))return true;
    entry.controller?.dispose();entry={screen,controller:null,title:template.dataset.pageTitle+' · PERTAMINA OTO',scroll:0,seed};entries.set(url.pathname,entry);remount=true;
   }
   if(!entry){
    const template=templates.get(url.pathname),screen=document.createElement('div');screen.id='oto-screen';screen.append(template.content.cloneNode(true));
    const workspace=screen.querySelector('[data-browser-tool]'),profile=workspace?OtoPdfUi.ToolProfile.from(workspace):null;
    if(control?.profile?.transferSources&&(control.transferSources?.()||control.resources().sources).length)lastPdf=control;
    let seed=profile?.transferSources?(lastPdf?.transferSources?.()||lastPdf?.resources().sources||[]):[];
    if(seed.length>1&&(forceSingleSource||!profile.multiple)){seed=await sourceChoice(seed);if(seed===null){if(pop)history.replaceState(null,'',current);return true;}}
    entry={screen,controller:null,title:template.dataset.pageTitle+' · PERTAMINA OTO',scroll:0,seed};
   }
   previous.scroll=window.scrollY;control?.deactivate?.();previous.screen.replaceWith(entry.screen);
   if(!entries.has(url.pathname)||remount){entries.set(url.pathname,entry);entry.controller=mount(entry.screen,entry.seed);delete entry.seed;}
   entry.controller?.activate?.();
   if(control?.profile?.transferSources&&(control.transferSources?.()||control.resources().sources).length)lastPdf=control;
   current=url.pathname;document.title=entry.title;
   if(!pop)history.pushState(null,'',url.pathname+url.hash);
   document.querySelectorAll('.primary-nav-link').forEach(a=>{if(a.pathname===(current==='/favorit'?'/favorit':'/'))a.setAttribute('aria-current','page');else a.removeAttribute('aria-current');});
   window.OtoSiteRefresh?.();
   const heading=entry.screen.querySelector('h1');if(heading){heading.tabIndex=-1;heading.focus({preventScroll:true});}
   window.scrollTo({top:entry.scroll,behavior:'instant'});
   if(url.hash)document.getElementById(url.hash.slice(1))?.scrollIntoView();
   return true;
  }finally{navigating=false;syncSwitch();}
 }
 document.addEventListener('click',e=>{
  const a=e.target.closest('a[href]');
  if(!a||e.defaultPrevented||e.button!==0||e.metaKey||e.ctrlKey||e.shiftKey||e.altKey||a.download||a.target||a.getAttribute('href').startsWith('#'))return;
  const url=new URL(a.href,location.href);
  if(url.origin!==location.origin||url.search||!templates.has(url.pathname))return;
  if(a.id==='info-download'&&a.closest('#info-dialog'))a.closest('#info-dialog').close();
  e.preventDefault();e.stopPropagation();void go(url,false,a.dataset.localSingleSource==='true').catch(()=>notice('Pindah alat belum berhasil. Ruang kerja tidak dihapus; coba kembali.'));
 },true);
 document.addEventListener('change',e=>{if(e.target.matches('[data-tool-switch]'))void go(new URL(e.target.value,location.href)).catch(()=>notice('Pindah alat belum berhasil. Coba kembali.'));});
 window.addEventListener('popstate',()=>void go(new URL(location.href),true).catch(()=>notice('Navigasi belum berhasil. Ruang kerja tetap tersedia.')));
 window.addEventListener('beforeunload',e=>{if(resources().files||[...entries.values()].some(v=>v.controller?.busy()||v.controller?.hasWork?.())){e.preventDefault();e.returnValue='';}});
 window.addEventListener('pagehide',()=>{for(const entry of entries.values())entry.controller?.dispose();entries.clear();lastPdf=null;});
 window.addEventListener('pageshow',e=>{if(e.persisted)location.reload();});
})();
