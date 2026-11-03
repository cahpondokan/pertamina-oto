const test=require('node:test'),assert=require('node:assert/strict');
const {ToolProfile,WorkspaceController,FileInputController}=require('../wwwroot/js/pdf-workspace-ui.js');
const clipboard=require('../wwwroot/js/pdf-paste.js'),Messages=require('../wwwroot/js/pdf-document-messages.js');
const profile=(extra={})=>new ToolProfile({inputKind:'pdf',multiple:false,reorder:false,transferSources:true,...extra});
function harness(extra={}){
 const doc=new EventTarget(),host=new EventTarget(),field=new EventTarget(),classes=new Set(),screen={isConnected:true,ownerDocument:doc,classList:{add:x=>classes.add(x),remove:x=>classes.delete(x)}};
 let busy=false,result=false,dialog=false,picks=0,accepted=[],notices=[],disposed=0;
 doc.querySelector=()=>dialog?{}:null;
 const workspace=new WorkspaceController(screen,{tool:'test',profile:profile(extra),busy:()=>busy,resources:()=>({sources:[],outputBytes:result?5:0}),dispose:()=>disposed++});
 const intake=new FileInputController(workspace,screen,{host,input:field,clipboard,pick:()=>picks++,addFiles:files=>accepted.push(files),hasResult:()=>result,notify:value=>notices.push(value),resultMessage:'RESULT_RETAINED'});
 function event(target,type,props={}){const e=new Event(type,{cancelable:true});for(const [key,value]of Object.entries(props))Object.defineProperty(e,key,{value});if(props.target)Object.defineProperty(e,'composedPath',{value:()=>[props.target,target]});target.dispatchEvent(e);return e;}
 return {workspace,intake,doc,host,field,screen,classes,event,accepted,notices,picks:()=>picks,disposed:()=>disposed,setBusy:v=>busy=v,setResult:v=>result=v,setDialog:v=>dialog=v};
}
test('profiles are immutable, validate metadata and select input types without a tool-id list',()=>{
 const p=profile();assert.ok(Object.isFrozen(p));assert.ok(p.accepts({name:'REPORT.PDF'}));assert.equal(p.accepts({name:'fake.pdf.exe'}),false);
 const image=profile({inputKind:'images'});assert.ok(image.accepts({name:'photo.JPEG'}));assert.equal(image.accepts({name:'report.pdf'}),false);
 assert.throws(()=>profile({multiple:'true'}),/TOOL_PROFILE/);assert.throws(()=>profile({inputKind:'anything'}),/TOOL_PROFILE/);
});
test('one intake object handles shortcut, selected files, drop and file paste',()=>{
 const h=harness(),file={name:'input.pdf'},data={files:[file],types:['Files'],getData:()=>''};
 assert.ok(h.event(h.doc,'keydown',{key:'o',ctrlKey:true}).defaultPrevented);assert.equal(h.picks(),1);
 h.event(h.field,'change',{target:{files:[file],value:'selection'}});h.event(h.host,'dragover',{dataTransfer:data});assert.ok(h.classes.has('is-dragging'));
 h.event(h.host,'drop',{dataTransfer:data});assert.equal(h.classes.has('is-dragging'),false);
 assert.ok(h.event(h.doc,'paste',{clipboardData:data,target:h.screen}).defaultPrevented);assert.equal(h.accepted.length,3);
 h.workspace.dispose();
});
for(const state of ['busy','result','dialog'])test(state+' blocks every intake channel without clearing existing output',()=>{
 const h=harness();({busy:h.setBusy,result:h.setResult,dialog:h.setDialog})[state](true);
 const file={name:'input.pdf'},data={files:[file],types:['Files'],getData:()=>''};
 h.intake.pick();h.event(h.field,'change',{target:{files:[file],value:'selection'}});h.event(h.host,'drop',{dataTransfer:data});h.event(h.doc,'paste',{clipboardData:data,target:h.screen});
 assert.equal(h.picks(),0);assert.equal(h.accepted.length,0);if(state==='result'){assert.equal(h.workspace.resources().outputBytes,5);assert.ok(h.notices.includes('RESULT_RETAINED'));}
 h.workspace.dispose();
});
test('text paste remains native, including mixed clipboard in a text field',()=>{
 const h=harness(),field={tagName:'INPUT',type:'text',closest:()=>field};
 const plain=h.event(h.doc,'paste',{target:field,clipboardData:{files:[],getData:()=> 'name'}});
 const mixed=h.event(h.doc,'paste',{target:field,clipboardData:{files:[{name:'input.pdf'}],getData:()=> 'name'}});
 assert.equal(plain.defaultPrevented,false);assert.equal(mixed.defaultPrevented,false);assert.equal(h.accepted.length,0);h.workspace.dispose();
});
test('inactive, detached and disposed workspaces do not intercept input; reactivation attaches no duplicates',()=>{
 const h=harness(),key=()=>h.event(h.doc,'keydown',{key:'o',ctrlKey:true});
 h.workspace.deactivate();assert.equal(key().defaultPrevented,false);h.workspace.activate();key();assert.equal(h.picks(),1);
 h.screen.isConnected=false;assert.equal(key().defaultPrevented,false);h.screen.isConnected=true;
 h.workspace.dispose();h.workspace.dispose();h.workspace.activate();assert.equal(key().defaultPrevented,false);assert.equal(h.disposed(),1);
 const pasted=h.event(h.doc,'paste',{target:h.screen,clipboardData:{files:[{name:'a.pdf'}],getData:()=>''}});assert.equal(pasted.defaultPrevented,false);
});
test('all cleanup runs even if one service fails, and disposal stays idempotent',()=>{
 const h=harness();let clean=0;h.workspace.use(()=>clean++);h.workspace.use(()=>{throw Error('failure');});
 assert.throws(()=>h.workspace.dispose(),AggregateError);assert.equal(clean,1);assert.equal(h.disposed(),1);h.workspace.dispose();
 assert.equal(h.event(h.doc,'keydown',{key:'o',ctrlKey:true}).defaultPrevented,false);
});
test('controllers keep independent state and navigation may detach resources callback safely',()=>{
 const a=harness(),b=harness();a.setResult(true);const read=a.workspace.resources;assert.equal(read().outputBytes,5);assert.equal(a.workspace.hasWork(),true);assert.equal(b.workspace.hasWork(),false);
 a.workspace.dispose();b.intake.pick();assert.equal(b.picks(),1);b.workspace.dispose();
});
test('shared document messages preserve operation-specific limits and safe unknown error fallback',()=>{
 const merge=Messages.merge(),browser=Messages.browser();assert.match(merge.errors.TIMEOUT,/30 menit/);assert.match(browser.errors.TIMEOUT,/30 menit/);
 assert.match(merge.error(Error('UNSUPPORTED_ACTION')),/skrip/);assert.match(browser.error(Error('UNSUPPORTED_ACTION')),/skrip/);
 assert.equal(merge.error(Error('PRIVATE_DOCUMENT_CONTENT')),merge.errors.INVALID);assert.equal(browser.error(new DOMException('secret','AbortError')),browser.errors.CANCELLED);
 assert.equal(merge.details(['SOURCE_NAVIGATION','SOURCE_NAVIGATION']).length,1);assert.equal(merge.compatibility(['PARTIAL_TAGS']),browser.compatibility(['PARTIAL_TAGS']));
});
