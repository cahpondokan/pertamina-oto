/* A deliberately small DOM port for controller integration tests, not a browser
 * emulator. Layout, native clipboard and rendering require separate browser QA. */
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const base=path.resolve(__dirname,'../..'),js=path.join(base,'wwwroot/js');
class Element extends EventTarget{
 constructor(tag,doc){super();this.tagName=tag.toUpperCase();this.ownerDocument=doc;this.children=[];this.dataset={};this.attributes={};this.type=tag==='input'?'text':'';this.value='';this.checked=false;this.className='';this.hidden=false;this.isConnected=true;this.text='';
  this.classList={add:(...v)=>{this.className=[...new Set([...this.className.split(' '),...v])].join(' ');},remove:(...v)=>{this.className=this.className.split(' ').filter(x=>!v.includes(x)).join(' ');},toggle:(v,on)=>{on??=!this.className.split(' ').includes(v);this.classList[on?'add':'remove'](v);return on;}};
 }
 set textContent(v){this.text=String(v);this.children=[];} get textContent(){return this.text+this.children.map(c=>c.textContent).join('');}
 append(...children){for(const c of children){c.parentElement=this;this.children.push(c);}}
 replaceChildren(...children){this.children=[];this.append(...children);}
 get firstElementChild(){return this.children[0];}
 setAttribute(k,v){this.attributes[k]=String(v);} removeAttribute(k){delete this.attributes[k];}
 matches(s){if(s==='dialog[open]')return this.tagName==='DIALOG'&&this.open;
  if(s[0]==='#')return this.id===s.slice(1);if(s[0]==='.')return this.className.split(' ').includes(s.slice(1));
  if(s.startsWith('[data-'))return s.slice(6,-1).replace(/-([a-z])/g,(_,c)=>c.toUpperCase()) in this.dataset;
  return this.tagName===s.toUpperCase();}
 querySelectorAll(s){const out=[];for(const c of this.children){if(s.split(',').some(v=>c.matches(v)))out.push(c);out.push(...c.querySelectorAll(s));}return out;}
 querySelector(s){return this.querySelectorAll(s)[0]||null;}
 closest(s){return s.split(',').some(v=>this.matches(v))?this:this.parentElement?.closest(s)||null;}
 focus(){this.ownerDocument.activeElement=this;} scrollIntoView(){} showModal(){this.open=true;} close(){this.open=false;}
 click(){return this.onclick?.({target:this});}
}
async function mount(tool,{seed=[],session={},configure}={}){
 const doc=new EventTarget(),screen=new Element('div',doc),root=new Element('div',doc);screen.append(root);root.dataset.browserTool=tool;if(tool==='merge')root.className='merge-workspace';
 root.dataset.toolProfile=JSON.stringify({inputKind:tool==='images-pdf'?'images':'pdf',multiple:['merge','organize','images-pdf'].includes(tool),reorder:['merge','organize','images-pdf'].includes(tool),transferSources:!['merge','images-pdf'].includes(tool)});
 doc.activeElement=null;doc.querySelector=s=>screen.querySelector(s);doc.createElement=tag=>new Element(tag,doc);
 const readView=name=>fs.readFileSync(path.join(base,'Views/Shared',name+'.cshtml'),'utf8');
 let markup=readView(tool==='merge'?'_MergeWorkspace':'_PageWorkspace');
 {
  const prefix=tool==='merge'?'merge':'pdf';
  for(const name of ['_PdfProcessPanel','_PdfResultPanel'])markup+='\n'+readView(name).replaceAll('@(Model.Prefix)',prefix);
  markup+='\n'+readView('_PdfDropZone').replaceAll('@Model.Id',prefix==='merge'?'merge-empty':'pdf-empty').replaceAll('@Model.PickId',prefix==='merge'?'merge-pick':'pdf-empty-pick');
 }
 if(tool!=='merge'){root.dataset.workspaceLayout='shared';markup+='\n'+readView('_PdfToolSettings');}
 const fields=new Map();
 for(const match of markup.matchAll(/<([a-z][a-z0-9]*)\b([^>]*\bid="([\w-]+)"[^>]*)>/g)){
  const [,tag,attrs,id]=match;if(fields.has(id))continue;const el=new Element(tag,doc);el.id=id;
  for(const [key,name] of [['type','type'],['value','value'],['class','className']]){const m=attrs.match(new RegExp('\\b'+key+'="([^"@]*)"'));if(m)el[name]=m[1];}
  fields.set(id,el);root.append(el);
 }
 for(const name of ['merge-documents','merge-settings-content']){const el=new Element('section',doc);el.className=name;root.append(el);}
 const $=id=>fields.get(id);
 const options={numbers:{position:'bottom-center',start:1,size:12,format:'number',margin:12},watermark:{text:'DRAF',size:48,opacity:18,angle:45,color:'blue'},crop:{top:5,right:5,bottom:5,left:5}}[tool]||{};
 for(const [key,value]of Object.entries(options)){const el=new Element('input',doc);el.dataset.option=key;el.type=typeof value==='number'?'number':'text';el.value=String(value);root.append(el);}
 if($('pdf-ranges'))$('pdf-ranges').value='';if($('pdf-angle'))$('pdf-angle').value='90';if($('pdf-output-name'))$('pdf-output-name').value='result.pdf';
 if($('merge-blank'))$('merge-blank').value='none';if($('merge-name'))$('merge-name').value='result.pdf';
 const P=require(path.join(base,'wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js')),E=require(path.join(js,'pdf-engine.js')),M=require(path.join(js,'pdf-merge-engine.js'));
 const Split=require(path.join(js,'pdf-file-engine.js'));
 const output=new Map(),urls=new Map(),fileBytes=new WeakMap(),presented=new Map(),previewOps={opens:[]};let serial=0,jobs=0;
 // The production worker uses FileReaderSync. Tiny fixtures use an in-memory port.
 const read=(f,a,b)=>fileBytes.get(f).slice(a,b);
 const ctx=new EventTarget();Object.assign(ctx,{document:doc,console,File,Blob,Uint8Array,TextEncoder,ReadableStream,WritableStream,setTimeout,AbortController,DOMException,URL:{createObjectURL:blob=>{const url='blob:test-'+(++serial);urls.set(url,blob);return url;},revokeObjectURL:url=>urls.delete(url)},Worker:class{},OtoPdf:E,OtoMergeEngine:M,OtoPdfSession:require(path.join(js,'pdf-session.js')),OtoPdfWorkspace:require(path.join(js,'pdf-workspace-model.js')),OtoPdfPaste:require(path.join(js,'pdf-paste.js')),
  OtoPdfPreview:{create:()=>({button:()=>({target:new Element('button',doc)}),open(source,target){previewOps.opens.push({source,target});},update(){},clear:async()=>{},activate(){},deactivate(){},dispose(){}})},
  OtoPdfJob:{start:request=>{jobs++;return {promise:E.run(P,request),cancel(){}};}},
  OtoMergeJob:{create:async()=>{const id='test-'+(++serial);output.set(id,null);return id;},remove:async id=>output.delete(id),result:async (id,index)=>index===undefined?output.get(id):output.get(id)[index],split:request=>{jobs++;const parts=[];return {promise:Promise.all(request.files.map(async f=>fileBytes.set(f,new Uint8Array(await f.arrayBuffer())))).then(()=>Split.run(P,E,request,{read,output:async index=>{const chunks=[];return {write:async b=>chunks.push(b),file:async()=>{const f=new File(chunks,'part-'+index+'.pdf');fileBytes.set(f,new Uint8Array(await f.arrayBuffer()));parts[index]=f;return f;}};}})).then(result=>{if(request.directory)output.set(request.directory,parts);return result;}),cancel(){}};},start:request=>{jobs++;const chunks=[];const sink={write:async b=>chunks.push(b),file:async()=>{const f=new File(chunks,'result.pdf');fileBytes.set(f,new Uint8Array(await f.arrayBuffer()));return f;}};return {promise:Promise.all(request.files.map(async f=>fileBytes.set(f,new Uint8Array(await f.arrayBuffer())))).then(()=>M.run(P,E,request,{read,sink})).then(async result=>{if(request.directory)output.set(request.directory,await sink.file());return result;}),cancel(){}};}}
 });ctx.OtoMergeJob.file=ctx.OtoMergeJob.split;configure?.(ctx);ctx.window=ctx;vm.createContext(ctx);
 for(const file of ['pdf-capacity-data.js','pdf-capacity.js','pdf-workspace-ui.js','pdf-document-messages.js','pdf-result-archive.js','pdf-workspace-flow.js','pdf-merge-ui.js','browser-pdf.js'])vm.runInContext(fs.readFileSync(path.join(js,file),'utf8'),ctx,{filename:file});
 const Result=ctx.OtoPdfFlow.ResultController;
 ctx.OtoPdfFlow.ResultController=class extends Result{present(result){super.present(result);presented.set(this,this.ports.results?.().map(item=>item.file)||[result.file]);}reset(){super.reset();presented.delete(this);}};
 const controller=ctx.OtoBrowserMount(screen,seed,session);
 const data=await P.PDFDocument.create();for(let i=0;i<4;i++)data.addPage().drawText('PASTE PAGE '+(i+1));const file=new File([await data.save({useObjectStreams:false})],'Paste-test.pdf',{type:'application/pdf'});
 function paste(files=[file],target=screen,text=''){
  const event=new Event('paste',{cancelable:true});Object.defineProperties(event,{clipboardData:{value:{files,getData:type=>type==='text/plain'?text:''}},target:{value:target},composedPath:{value:()=>[target,doc]}});doc.dispatchEvent(event);return event;
 }
 async function idle(){const deadline=Date.now()+10000;do{await new Promise(resolve=>setImmediate(resolve));if(Date.now()>deadline)throw Error('controller timed out');}while(controller.busy());}
 const buffers=()=>tool==='merge'?[...output.values()].filter(Boolean):[...presented.values()].flat();
 return {tool,screen,root,$,file,paste,idle,controller,buffers,context:ctx,jobs:()=>jobs,P,previewOps,feedback:()=>$(tool==='merge'?'merge-status':'pdf-feedback').textContent};
}
module.exports={mount};
