/* One delegated tooltip owner for every server-authored page in the shared shell. */
(() => {
 'use strict';
 if(window.OtoTooltips)return;
 const abort=new AbortController(),help=document.getElementById('oto-help'),theme=document.getElementById('theme-toggle');
 const tip=document.createElement('div');tip.id='oto-tooltip';tip.className='oto-tooltip';tip.setAttribute('role','tooltip');tip.setAttribute('popover','manual');tip.hidden=true;document.body.append(tip);
 let target=null,timer=null,helpMode=false,disposed=false;
 const texts={'merge-pick':'Pilih PDF langsung dari perangkat','merge-add':'Tambahkan PDF ke susunan ini','merge-sort':'Susun nama A–Z, Z–A, atau urutan awal','merge-cards':'Tampilkan sebagai kartu','merge-rows':'Tampilkan sebagai daftar','merge-clear':'Kosongkan pilihan. File asli tetap utuh','merge-run':'Gabungkan halaman sesuai urutan dan pengaturan','merge-name':'Nama file PDF yang akan dibuat','merge-bookmarks':'Buat penanda dari nama setiap PDF','merge-blank':'Sisipkan halaman kosong sesuai pilihan','merge-cancel':'Hentikan proses. Sumber asli tetap utuh','merge-preview-output':'Periksa isi PDF hasil sebelum menyimpan','merge-save':'Simpan PDF ke perangkat Anda','merge-download':'Unduh PDF langsung melalui browser','merge-done':'Konfirmasikan file tersimpan, lalu bersihkan sesi','merge-new':'Mulai pekerjaan baru setelah konfirmasi hasil','merge-continue':'Lihat pilihan alat lanjutan','merge-result-name':'Ubah nama file hasil sebelum menyimpan','merge-settings-open':'Atur nama, bookmark dan halaman kosong','merge-retry':'Coba lagi dengan susunan dan pengaturan yang sama','merge-failure-back':'Kembali ke pilihan dokumen','merge-more':'Pilihan tambahan untuk hasil'};
 const find=node=>node instanceof Element?node.closest('[data-tip],button,a,input,select,summary'):null;
 function message(el){return el.id==='theme-toggle'?el.getAttribute('aria-label'):el.dataset.tip||texts[el.id]||el.getAttribute('aria-label')||el.getAttribute('title')||(el.labels?.[0]?.textContent||el.textContent).trim().replace(/\s+/g,' ').slice(0,180);}
 function hide(){clearTimeout(timer);timer=null;if(target){const ids=(target.getAttribute('aria-describedby')||'').split(' ').filter(id=>id&&id!==tip.id);if(ids.length)target.setAttribute('aria-describedby',ids.join(' '));else target.removeAttribute('aria-describedby');}target=null;if(tip.matches(':popover-open'))tip.hidePopover();tip.hidden=true;tip.textContent='';}
 function show(el){hide();if(disposed||!el?.isConnected||el.closest('[inert]')||!el.getClientRects().length)return;const value=message(el);if(!value)return;target=el;tip.textContent=value;tip.hidden=false;tip.style.left='0px';tip.style.top='0px';tip.showPopover?.();const r=el.getBoundingClientRect(),t=tip.getBoundingClientRect();tip.style.left=Math.max(8,Math.min(innerWidth-t.width-8,r.left+r.width/2-t.width/2))+'px';tip.style.top=Math.max(8,r.top-t.height-8>8?r.top-t.height-8:Math.min(innerHeight-t.height-8,r.bottom+8))+'px';el.setAttribute('aria-describedby',[(el.getAttribute('aria-describedby')||''),tip.id].filter(Boolean).join(' '));}
 function setHelp(value){helpMode=value;help?.setAttribute('aria-pressed',String(value));help?.setAttribute('aria-label',value?'Tutup bantuan sentuh':'Mode bantuan');}
 function prepare(node){if(!(node instanceof Element))return;for(const el of [node,...node.querySelectorAll('[title]')])if(el.hasAttribute('title')){if(!el.dataset.tip)el.dataset.tip=el.title;el.removeAttribute('title');}}
 const on=(node,event,fn,options={})=>node.addEventListener(event,fn,{...options,signal:abort.signal});
 on(document,'pointerover',e=>{if(e.pointerType==='touch')return;const el=find(e.target);if(el&&!el.contains(e.relatedTarget)){clearTimeout(timer);timer=setTimeout(()=>show(el),220);}});
 on(document,'pointerout',e=>{const el=find(e.target);if(el&&!el.contains(e.relatedTarget))hide();});
 on(document,'focusin',e=>show(find(e.target)));on(document,'focusout',hide);on(document,'pointerdown',hide,{capture:true});
 on(document,'click',e=>{const el=find(e.target);if(helpMode&&el&&el!==help){e.preventDefault();e.stopImmediatePropagation();setHelp(false);show(el);}else hide();},{capture:true});
 on(document,'keydown',e=>{if(e.key==='Escape'&&(target||helpMode)){e.preventDefault();e.stopImmediatePropagation();hide();setHelp(false);}},{capture:true});
 on(document,'scroll',hide,{capture:true});on(window,'resize',hide);
 if(help)on(help,'click',()=>{setHelp(!helpMode);if(helpMode){help.dataset.tip='Ketuk satu kontrol untuk penjelasan. Ketuk lagi untuk menjalankan; Escape menutup bantuan.';show(help);}else hide();});
 const syncTheme=()=>theme?.setAttribute('aria-label',document.documentElement.dataset.theme==='dark'?'Gunakan tema terang':'Gunakan tema gelap');syncTheme();
 on(window,'storage',e=>{if(e.key==='oto.web.theme'&&['light','dark'].includes(e.newValue)){document.documentElement.dataset.theme=e.newValue;syncTheme();}});
 const observer=new MutationObserver(records=>{if(target&&!target.isConnected)hide();for(const r of records)for(const node of r.addedNodes)if(node!==tip)prepare(node);});prepare(document.body);observer.observe(document.body,{childList:true,subtree:true});
 function dispose(){if(disposed)return;hide();disposed=true;abort.abort();observer.disconnect();tip.remove();setHelp(false);}
 on(window,'pagehide',dispose);window.OtoTooltips={hide,dispose,resources:()=>({popover:tip.isConnected?1:0,pendingTimer:timer?1:0,retainedTarget:target?1:0,helpMode})};
})();
