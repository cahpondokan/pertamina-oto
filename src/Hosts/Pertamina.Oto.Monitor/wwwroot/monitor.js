(() => {
  'use strict';
  const byId = id => document.getElementById(id);
  const features = {merge:'Gabungkan PDF',organize:'Atur halaman',split:'Pisahkan PDF','images-pdf':'Gambar ke PDF',numbers:'Nomor halaman',watermark:'Tanda air',remove:'Hapus halaman',extract:'Ambil halaman',rotate:'Putar halaman',crop:'Potong halaman',shell:'Aplikasi'};
  const stages = {input:'Input',process:'Proses',save:'Simpan',runtime:'Aplikasi',cleanup:'Pembersihan',preview:'Pratinjau',cloud:'Cloud'};
  const outcomes = {started:'Dimulai',succeeded:'Berhasil',failed:'Gagal',cancelled:'Dibatalkan',download_requested:'Unduh diminta',save_succeeded:'Tersimpan',save_cancelled:'Simpan dibatalkan',save_failed:'Simpan gagal',runtime_error:'Gangguan aplikasi'};
  const number = n => Number(n).toLocaleString('id-ID');
  const time = value => value ? new Date(value).toLocaleString('id-ID',{dateStyle:'medium',timeStyle:'medium'}) : 'Belum ada';
  const text = (id,value) => { byId(id).textContent=value; };
  let page = 1, pages = 1, timer, busy = false, revision = 0;
  let filters = {days:'1',feature:'',outcome:'',process:''};
  const td = value => { const node=document.createElement('td'); node.textContent=String(value); return node; };
  function table(id,rows,build) {
    const body=byId(id); body.replaceChildren();
    if(!rows.length) { const tr=document.createElement('tr'), cell=td('Tidak ada kejadian yang sesuai filter.');cell.colSpan=6;cell.className='empty';tr.append(cell);body.append(tr);return; }
    for(const row of rows) { const tr=document.createElement('tr');build(tr,row);body.append(tr); }
  }
  function render(data) {
    text('events',number(data.events));text('success',number(data.successfulProcesses));text('errors',number(data.errorEvents));
    text('pending',filters.outcome ? '—' : number(data.startedWithoutTerminal));
    text('updated','Diperbarui '+time(data.generatedAtUtc));
    const notice=byId('connection');notice.className='notice'+(data.stale||data.capacityWarning||data.rejectedRows?' warning':'');
    notice.textContent=data.state==='waiting'?'Menunggu catatan pertama. Collector belum membuat log.':data.stale?'Data berhasil dibaca. Belum ada catatan baru dalam 15 menit; ini tidak selalu berarti aplikasi bermasalah.':'Data diagnostik berhasil dibaca. Pembaruan otomatis aktif.';
    text('storage-value',(data.storageBytes/1048576).toFixed(2)+' / 16 MB');
    byId('storage').value=Math.max(data.storageBytes/data.capacityBytes,data.recordCount/data.recordLimit)*100;
    text('storage-detail',number(data.recordCount)+' / '+number(data.recordLimit)+' catatan · '+number(data.rejectedRows)+' baris ditolak · '+number(data.duplicateRows)+' duplikat');
    text('last-event','Terakhir diterima: '+time(data.lastReceivedAtUtc));
    text('storage-warning',data.capacityWarning?'Kapasitas hampir penuh. Arsipkan log melalui administrator VPS; tidak ada penghapusan otomatis.':'Log dibatasi kapasitasnya dan tidak dihapus otomatis.');
    byId('storage-warning').className=data.capacityWarning?'danger':'muted';
    table('error-rows',data.errors,(tr,row)=>{for(const value of [features[row.feature]||row.feature,stages[row.stage]||row.stage,row.code,row.release,row.browser,number(row.count)])tr.append(td(value));});
    text('error-count',data.errors.length+' kelompok');
    table('recent-rows',data.recent,(tr,row)=>{
      const e=row.data;tr.append(td(time(row.receivedAtUtc)),td((features[e.feature]||e.feature)+' / '+(stages[e.stage]||e.stage)));
      const cell=td(''),badge=document.createElement('span');badge.className='result'+(['failed','save_failed','runtime_error'].includes(e.event)?' bad':['succeeded','save_succeeded'].includes(e.event)?' ok':'');badge.textContent=outcomes[e.event]||e.event;cell.append(badge);tr.append(cell,td(e.code),td(number(e.elapsedMs)+' ms'));
      const process=td(''),button=document.createElement('button');button.type='button';button.className='process-link';button.textContent=e.processId;button.title='Telusuri proses '+e.processId;button.addEventListener('click',()=>{byId('process').value=e.processId;byId('outcome').value='';applyFilters();});process.append(button);tr.append(process);
    });
    page=data.page;pages=data.pages;text('page','Halaman '+page+' / '+pages);text('history-count',number(data.events)+' kejadian');
    byId('prev').disabled=page<=1;byId('next').disabled=page>=pages;
    const ns='http://www.w3.org/2000/svg',svg=document.createElementNS(ns,'svg');svg.setAttribute('viewBox','0 0 720 140');svg.setAttribute('preserveAspectRatio','none');
    const max=Math.max(1,...data.trend.map(b=>b.events));
    data.trend.forEach((b,i)=>{for(const [value,klass] of [[b.events,'bar'],[b.errors,'bar-error']]){const rect=document.createElementNS(ns,'rect'),height=value/max*125;rect.setAttribute('x',String(i*30+4));rect.setAttribute('y',String(140-height));rect.setAttribute('width','20');rect.setAttribute('height',String(height));rect.setAttribute('rx','2');rect.setAttribute('class',klass);const title=document.createElementNS(ns,'title');title.textContent=time(b.start)+': '+b.events+' kejadian, '+b.errors+' kesalahan';rect.append(title);svg.append(rect);}});
    byId('trend').replaceChildren(svg);byId('trend').setAttribute('aria-label','24 jam: '+data.trend.reduce((n,b)=>n+b.events,0)+' kejadian, '+data.trend.reduce((n,b)=>n+b.errors,0)+' kesalahan');
  }
  async function refresh() {
    if(busy||document.hidden)return;clearTimeout(timer);busy=true;byId('refresh').disabled=true;text('refresh-state','Memperbarui…');
    const requestRevision=revision,controller=new AbortController(),timeout=setTimeout(()=>controller.abort(),8000);
    try {
      const response=await fetch('/api/overview?'+new URLSearchParams({...filters,page:String(page)}),{cache:'no-store',credentials:'same-origin',redirect:'error',signal:controller.signal});
      if(response.status===401){location.replace('/login');return;}
      if(!response.ok)throw new Error('UNAVAILABLE');
      const data=await response.json();if(requestRevision===revision){render(data);text('refresh-state','● Otomatis setiap 15 detik');}
    } catch {
      text('refresh-state','Pembaruan terputus');byId('connection').className='notice danger';
      text('connection','Data belum berhasil diperbarui. Periksa koneksi SSH. Angka yang masih terlihat berasal dari pembaruan sebelumnya; sistem akan mencoba lagi.');
    } finally {clearTimeout(timeout);busy=false;byId('refresh').disabled=false;timer=setTimeout(refresh,requestRevision===revision?15000:0);}
  }
  function applyFilters() {
    if(!byId('filters').reportValidity())return;
    filters=Object.fromEntries(['days','feature','outcome','process'].map(id=>[id,byId(id).value.trim()]));page=1;revision++;
    if(busy){text('refresh-state','Filter diterapkan pada pembaruan berikutnya');return;}void refresh();
  }
  byId('filters').addEventListener('submit',event=>{event.preventDefault();applyFilters();});
  byId('reset').addEventListener('click',()=>{byId('filters').reset();applyFilters();});
  byId('refresh').addEventListener('click',()=>void refresh());
  byId('prev').addEventListener('click',()=>{if(!busy&&page>1){page--;void refresh();}});
  byId('next').addEventListener('click',()=>{if(!busy&&page<pages){page++;void refresh();}});
  document.addEventListener('visibilitychange',()=>{clearTimeout(timer);if(document.hidden)text('refresh-state','Dijeda saat tab tidak aktif');else void refresh();});
  void refresh();
})();
