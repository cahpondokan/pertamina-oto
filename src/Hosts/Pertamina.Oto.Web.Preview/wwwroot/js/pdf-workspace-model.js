/* Browser workspace presentation model. No DOM, network, persistence, or PDF mutation. */
(function(root) {
  'use strict';
  function sourceLabel(id) {
    let label='';
    while(id>0){id--;label=String.fromCharCode(65+id%26)+label;id=Math.floor(id/26);}
    return label;
  }
  function moveTo(items,id,position) {
    const next=items.slice(),from=next.findIndex(item=>item.id===id);
    if(from<0||!Number.isInteger(position)||position<0||position>=next.length||from===position)return next;
    const [item]=next.splice(from,1);next.splice(position,0,item);return next;
  }
  function move(items,id,delta) {
    return moveTo(items,id,items.findIndex(item=>item.id===id)+delta);
  }
  function cards(tool,sources,pages) {
    if(tool==='merge')return sources.map((source,position)=>({id:source.id,source,position,identity:source.label,count:source.count}));
    return pages.map((page,position)=>{
      const source=sources.find(item=>item.id===page.sourceId);
      if(!source)throw Error('INVALID');
      return {id:page.id,source,page,position,identity:source.label+' · '+(page.index+1),count:1};
    });
  }
  function input(tool,sources,pages) {
    const orderedPages=pages.map(page=>{
      const source=sources.findIndex(item=>item.id===page.sourceId);
      if(source<0)throw Error('INVALID');
      return {source,index:page.index,rotation:page.rotation};
    });
    // Merge consumes buffers in source order. Images consume buffers in card order.
    // Organizer consumes page references, keeping original page identity intact.
    const buffers=tool==='images-pdf'?orderedPages.map(page=>sources[page.source].bytes):sources.map(source=>source.bytes);
    return {buffers,pages:orderedPages};
  }
  function state({busy,showResult,resultCount,sourceCount}) {
    return busy?'processing':showResult&&resultCount>0?'result':sourceCount>0?'editing':'empty';
  }
  function readiness(tool,sources,pages) {
    if(!sources.length)return {ready:false,title:'Belum ada dokumen',detail:'Pilih file untuk memulai.'};
    if(!pages.length)return {ready:false,title:'Belum ada halaman hasil',detail:'Tambahkan dokumen atau reset susunan.'};
    if(tool==='merge'&&sources.length<2)return {ready:false,title:'Tambahkan satu PDF lagi',detail:'Minimal 2 PDF untuk digabungkan.'};
    if(tool==='split'&&sources[0].count<1)return {ready:false,title:'PDF tidak memiliki halaman',detail:'Pilih PDF yang memiliki halaman.'};
    if(tool==='merge')return {ready:true,title:sources.length+' PDF siap digabungkan',detail:'Urutan hasil mengikuti susunan dokumen.'};
    if(tool==='split')return {ready:true,title:pages.length+' halaman siap dipisahkan',detail:'Tentukan halaman atau rentang yang akan dibuat.'};
    if(tool==='organize')return {ready:true,title:pages.length+' halaman siap disusun',detail:'Identitas sumber tetap sama setelah dipindah.'};
    if(tool==='images-pdf')return {ready:true,title:pages.length+' gambar siap dibuat PDF',detail:'Satu gambar menjadi satu halaman.'};
    return {ready:true,title:pages.length+' halaman siap diproses',detail:'Periksa pengaturan, lalu buat PDF baru.'};
  }
  const api={sourceLabel,moveTo,move,cards,input,state,readiness};
  if(typeof module!=='undefined')module.exports=api;else root.OtoPdfWorkspace=api;
})(typeof self!=='undefined'?self:globalThis);
