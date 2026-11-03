'use strict';
const test=require('node:test'),assert=require('node:assert/strict');
const P=require('../wwwroot/js/pdf-paste.js');
const pdf=new File(['%PDF-test'],'sample.pdf',{type:'application/pdf'});
const field=(type='text',tagName='INPUT')=>({tagName,type,closest:()=>({tagName,type})});
function clipboard(files=[pdf],plain='',html='',items=[]){return {files,items,getData:type=>type==='text/plain'?plain:type==='text/html'?html:''};}
function environment(){
 const doc=new EventTarget();let modal=false;
 doc.querySelector=()=>modal?{}:null;
 const screen={ownerDocument:doc,isConnected:true},calls=[],messages=[];
 let processing=false,output=null;
 const dispose=P.mount(screen,{addFiles:files=>calls.push(files),busy:()=>processing,hasResult:()=>!!output,notify:m=>messages.push(m),resultMessage:'Simpan hasil sebelum menambah PDF.'});
 function paste(data=clipboard(),target={}){
  const e=new Event('paste',{cancelable:true});
  Object.defineProperties(e,{clipboardData:{value:data},target:{value:target},composedPath:{value:()=>[target,doc]}});
  doc.dispatchEvent(e);return e;
 }
 return {doc,screen,calls,messages,dispose,paste,setBusy:v=>processing=v,setOutput:v=>output=v,output:()=>output,setModal:v=>modal=v};
}
test('PDF paste on the document works without selecting an upload target',()=>{
 const e=environment();const event=e.paste();assert.equal(event.defaultPrevented,true);assert.deepEqual(e.calls,[[pdf]]);e.dispose();
});
for(const type of ['file','checkbox','radio','button','range','color'])test('PDF paste works with focus on input '+type,()=>{
 const e=environment();e.paste(clipboard(),field(type));assert.equal(e.calls.length,1);e.dispose();
});
for(const [label,target] of [['text',field()],['search',field('search')],['number',field('number')],['textarea',field('text','TEXTAREA')],['contenteditable',{isContentEditable:true}],['editable child',{isContentEditable:true}],['text node',{nodeType:3,parentElement:{isContentEditable:true}}]]){
 test('native plain text paste stays intact in '+label,()=>{
  const e=environment();assert.equal(e.paste(clipboard([],'teks asli'),target).defaultPrevented,false);assert.equal(e.calls.length,0);e.dispose();
 });
 test('mixed file/text clipboard gives native text precedence in '+label,()=>{
  const e=environment();assert.equal(e.paste(clipboard([pdf],'teks asli'),target).defaultPrevented,false);assert.equal(e.calls.length,0);e.dispose();
 });
}
test('file-only clipboard adds PDF even when focus was left in a text field',()=>{
 const e=environment();e.paste(clipboard(),field());assert.deepEqual(e.calls,[[pdf]]);e.dispose();
});
test('rich HTML paste in editable content is not intercepted',()=>{
 const e=environment();assert.equal(e.paste(clipboard([pdf],'','<b>teks</b>'),{isContentEditable:true}).defaultPrevented,false);assert.equal(e.calls.length,0);e.dispose();
});
test('no clipboard access or native-text suppression for missing/empty/files-as-text data',()=>{
 const e=environment();for(const c of [null,clipboard([]),clipboard([],'C:\\example.pdf'),clipboard([],'https://example.invalid/x.pdf')])assert.equal(e.paste(c).defaultPrevented,false);assert.equal(e.calls.length,0);e.dispose();
});
test('items-only file data works and null/string items are ignored',()=>{
 const e=environment();const items=[{kind:'string',getAsFile:()=>{throw Error('not a file');}},{kind:'file',getAsFile:()=>null},{kind:'file',getAsFile:()=>pdf}];
 e.paste(clipboard([],'','',items));assert.deepEqual(e.calls,[[pdf]]);e.dispose();
});
test('files and items expose the same payload only once, preserving multi-file order',()=>{
 const b=new File(['b'],'B.PDF'),e=environment();e.paste(clipboard([pdf,b],'','',[{kind:'file',getAsFile:()=>pdf}]));assert.deepEqual(e.calls,[[pdf,b]]);e.dispose();
});
test('file list is captured synchronously before browser clipboard event expires',()=>{
 const e=environment(),files=[pdf];e.paste(clipboard(files));files.length=0;assert.deepEqual(e.calls,[[pdf]]);e.dispose();
});
test('types and sizes go through the existing input validator without filtering or partial acceptance',()=>{
 const bad=new File(['bad'],'bad.txt'),e=environment();e.paste(clipboard([pdf,bad]));assert.deepEqual(e.calls,[[pdf,bad]]);e.dispose();
});
test('processing never queues or adds a PDF and provides retry guidance',()=>{
 const e=environment();e.setBusy(true);assert.equal(e.paste().defaultPrevented,true);assert.equal(e.calls.length,0);assert.match(e.messages[0],/tempel PDF kembali/);e.setBusy(false);assert.equal(e.calls.length,0);e.paste();assert.equal(e.calls.length,1);e.dispose();
});
test('unsaved result remains identical and no input mutation is invoked',()=>{
 const e=environment(),output={url:'blob:retained-result',bytes:new Uint8Array([3,2,1]),name:'result.pdf'};e.setOutput(output);
 for(let i=0;i<3;i++)assert.equal(e.paste().defaultPrevented,true);
 assert.equal(e.output(),output);assert.deepEqual(e.output().bytes,new Uint8Array([3,2,1]));assert.equal(e.calls.length,0);assert.equal(e.messages.length,3);
 e.setOutput(null);e.paste();assert.equal(e.calls.length,1);e.dispose();
});
test('an open dialog retains exclusive control of paste',()=>{
 const e=environment();e.setModal(true);assert.equal(e.paste().defaultPrevented,false);assert.equal(e.calls.length,0);e.setModal(false);e.paste();assert.equal(e.calls.length,1);e.dispose();
});
test('previously handled events are never handled a second time',()=>{
 const e=environment();let secondCalls=0;const remove=P.mount(e.screen,{addFiles:()=>secondCalls++,busy:()=>false,hasResult:()=>false,notify:()=>{}});
 e.paste();assert.equal(e.calls.length,1);assert.equal(secondCalls,0);remove();e.dispose();
});
test('only the active one of nine cached PDF screens receives each paste',()=>{
 const e=environment(),calls=Array(9).fill(0),screens=Array.from({length:9},()=>({ownerDocument:e.doc,isConnected:false}));e.dispose();
 const cleanups=screens.map((screen,i)=>P.mount(screen,{addFiles:()=>calls[i]++,busy:()=>false,hasResult:()=>false,notify:()=>{}}));
 for(let cycle=0;cycle<4;cycle++)for(let i=0;i<9;i++){screens.forEach((s,j)=>s.isConnected=i===j);e.paste();}
 assert.deepEqual(calls,Array(9).fill(4));cleanups.forEach(fn=>fn());e.paste();assert.deepEqual(calls,Array(9).fill(4));
});
test('shadow DOM target is used for editable-text precedence',()=>{
 const e=environment(),event=new Event('paste',{cancelable:true});
 Object.defineProperties(event,{clipboardData:{value:clipboard([pdf],'teks')},composedPath:{value:()=>[field()]}});e.doc.dispatchEvent(event);assert.equal(event.defaultPrevented,false);assert.equal(e.calls.length,0);e.dispose();
});
