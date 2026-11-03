/* Shared UI objects. No PDF mutation, network, persistence or execution admission. */
(function(root,factory){
 'use strict';
 const api=factory();
 if(typeof module==='object'&&module.exports)module.exports=api;
 else root.OtoPdfUi=api;
})(typeof globalThis!=='undefined'?globalThis:this,function(){
 'use strict';
 class ToolProfile {
  constructor(value){
   if(!value||!['pdf','images'].includes(value.inputKind)||['multiple','reorder','transferSources'].some(key=>typeof value[key]!=='boolean'))throw Error('TOOL_PROFILE');
   this.inputKind=value.inputKind;this.multiple=value.multiple;this.reorder=value.reorder;this.transferSources=value.transferSources;
   Object.freeze(this);
  }
  get images(){return this.inputKind==='images';}
  accepts(file){return (this.images?/\.(jpe?g|png)$/i:/\.pdf$/i).test(file.name||'');}
  static from(element){return new ToolProfile(JSON.parse(element.dataset.toolProfile));}
 }
 class WorkspaceController {
  #screen;#ports;#cleanups=[];#disposed=false;#active=true;
  constructor(screen,{tool,profile,...ports}){
   this.#screen=screen;this.#ports=ports;this.tool=tool;this.profile=profile;
   // Navigation may pass this method as a callback; retain instance ownership.
   this.resources=()=>this.#ports.resources?.()||{sources:[],outputBytes:0};
   this.transferSources=()=>this.#ports.transferSources?.()||this.resources().sources;
  }
  get disposed(){return this.#disposed;}
  get active(){return !this.#disposed&&this.#active&&this.#screen.isConnected;}
  busy(){return !!this.#ports.busy?.();}
  hasWork(){const value=this.resources();return this.#ports.hasWork?.()??!!(this.busy()||value.sources.length||value.outputBytes);}
  beforeLeave(){return this.#ports.beforeLeave?.()??true;}
  use(cleanup){if(this.#disposed)cleanup();else this.#cleanups.push(cleanup);return cleanup;}
  listen(target,type,handler){
   const guard=event=>{if(this.active)handler(event);};
   target.addEventListener(type,guard);this.use(()=>target.removeEventListener(type,guard));
  }
  activate(){if(this.#disposed)return;this.#active=true;this.#ports.activate?.();}
  deactivate(){if(this.#disposed||!this.#active)return;this.#active=false;this.#ports.deactivate?.();}
  dispose(){
   if(this.#disposed)return;this.#disposed=true;this.#active=false;
   const errors=[];
   for(const cleanup of this.#cleanups.splice(0).reverse()){try{cleanup();}catch(error){errors.push(error);}}
   try{this.#ports.dispose?.();}catch(error){errors.push(error);}
   if(errors.length)throw new AggregateError(errors,'Workspace cleanup failed');
  }
 }
 class FileInputController {
  #workspace;#options;#document;
  constructor(workspace,surface,options){
   this.#workspace=workspace;this.#options=options;this.#document=surface.ownerDocument;
   const doc=this.#document,host=options.host;
   workspace.listen(doc,'keydown',event=>{
    if(!this.#dialog()&&(event.ctrlKey||event.metaKey)&&event.key.toLowerCase()==='o'){event.preventDefault();this.pick();}
   });
   workspace.use(options.clipboard.mount(surface,{
    addFiles:files=>this.receive(files),busy:()=>workspace.busy(),hasResult:options.hasResult,
    notify:options.notify,resultMessage:options.resultMessage,isActive:()=>workspace.active,
    // Keep the previous image-text behavior while sharing safety and lifecycle.
    ignoreTextTargets:workspace.profile.images
   }));
   workspace.listen(host,'dragover',event=>{
    if(Array.from(event.dataTransfer?.types||[]).includes('Files')){
     event.preventDefault();if(!this.#blocked(false))surface.classList.add('is-dragging');
    }
   });
   workspace.listen(host,'dragleave',event=>{if(!event.relatedTarget)surface.classList.remove('is-dragging');});
   workspace.listen(host,'drop',event=>{
    surface.classList.remove('is-dragging');
    if(event.dataTransfer?.files.length){event.preventDefault();void this.receive(event.dataTransfer.files);}
   });
   workspace.listen(options.input,'change',event=>{
    const files=Array.from(event.target.files||[]);event.target.value='';void this.receive(files);
   });
   workspace.use(()=>surface.classList.remove('is-dragging'));
  }
  #dialog(){return !!this.#document.querySelector('dialog[open]');}
  #blocked(notify=true){
   if(!this.#workspace.active||this.#dialog())return true;
   const message=this.#workspace.busy()?'Proses masih berjalan. Tunggu atau batalkan proses, lalu coba kembali.':this.#options.hasResult()?this.#options.resultMessage:null;
   if(message&&notify)this.#options.notify(message);return !!message;
  }
  pick(){if(!this.#blocked())this.#options.pick();}
  receive(files){if(this.#blocked())return;return this.#options.addFiles(Array.from(files));}
 }
 return {ToolProfile,WorkspaceController,FileInputController};
});
