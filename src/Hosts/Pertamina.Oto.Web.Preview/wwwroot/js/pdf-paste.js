/* Read only the clipboard supplied by the user's paste event. No permission,
 * focus trap, keydown interception, clipboard polling or retained file queue. */
(function(root,factory){
 'use strict';
 const api=factory();
 if(typeof module==='object'&&module.exports)module.exports=api;
 else root.OtoPdfPaste=api;
})(typeof globalThis!=='undefined'?globalThis:this,function(){
 'use strict';
 function filesFrom(data){
  if(!data)return [];
  const files=Array.from(data.files||[]);
  // Some clipboard implementations expose files only as DataTransferItems.
  return files.length?files:Array.from(data.items||[]).filter(item=>item.kind==='file').map(item=>item.getAsFile()).filter(Boolean);
 }
 function editsText(target){
  const element=target?.nodeType===3?target.parentElement:target;
  if(element?.isContentEditable)return true;
  const field=element?.closest?.('input,textarea');
  return !!field&&(field.tagName==='TEXTAREA'||!['file','button','submit','reset','checkbox','radio','range','color','image','hidden'].includes(field.type));
 }
 function mount(screen,{addFiles,busy,hasResult,notify,resultMessage,isActive=()=>screen.isConnected,ignoreTextTargets=false}){
  const doc=screen.ownerDocument;
  const onPaste=event=>{
   if(!isActive()||event.defaultPrevented||doc.querySelector('dialog[open]'))return;
   const data=event.clipboardData,files=filesFrom(data);
   if(!files.length)return; // Native text/HTML paste remains untouched.
   const target=event.composedPath?.()[0]||event.target;
   if(editsText(target)&&(ignoreTextTargets||data.getData('text/plain')||data.getData('text/html')))return;
   event.preventDefault();
   if(busy()){notify('Proses masih berjalan. Tunggu atau batalkan proses, lalu tempel PDF kembali.');return;}
   if(hasResult()){notify(resultMessage);return;}
   // The existing input path enforces type, count, size and PDF validation.
   void addFiles(files);
  };
  doc.addEventListener('paste',onPaste);
  return ()=>doc.removeEventListener('paste',onPaste);
 }
 return {filesFrom,editsText,mount};
});
