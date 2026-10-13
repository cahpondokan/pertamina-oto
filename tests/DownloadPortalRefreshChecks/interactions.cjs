// Runs the production interaction script against deterministic browser API fakes.
const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
let checks = 0;
function check(value, message) { assert.ok(value, message); console.log('PASS ' + message); checks++; }
function load(stored, dark, blocked = false) {
 const handlers = {}, linkHandlers = [], timerCallbacks = [];
 const control = {hidden:true};
 const picker = {value:'', closest:()=>control, addEventListener:(name, fn)=>handlers[name]=fn};
 const status = {textContent:''}, button = {addEventListener:(name,fn)=>handlers.close=fn};
 const toast = {hidden:true,querySelector:s=>s==='button'?button:status};
 const media = {matches:dark,addEventListener:(name,fn)=>handlers.system=fn};
 const root = {dataset:{}};
 let saved;
 const context = {document:{documentElement:root,querySelector:s=>s==='#theme'?picker:toast,querySelectorAll:()=>[{addEventListener:(name,fn)=>linkHandlers.push(fn)}]},matchMedia:()=>media,localStorage:{getItem:()=>{if(blocked)throw Error();return stored;},setItem:(k,v)=>{if(blocked)throw Error();saved=v;}},setTimeout:fn=>{timerCallbacks.push(fn);return timerCallbacks.length;},clearTimeout:()=>{}};
 vm.runInNewContext(fs.readFileSync('src/Hosts/Pertamina.Oto.Downloads/wwwroot/site.js','utf8'),context);
 return {handlers,picker,control,toast,status,root,media,click:linkHandlers[0],timerCallbacks,saved:()=>saved};
}
let app = load('dark',false);
check(app.root.dataset.theme==='dark' && !app.control.hidden,'stored dark preference applied');
app.picker.value='light';app.handlers.change();check(app.saved()==='light' && app.root.dataset.theme==='light','explicit light persisted');
app.picker.value='system';app.handlers.change();app.media.matches=true;app.handlers.system();check(app.root.dataset.theme==='dark','system preference responds to OS change');
app = load('invalid',false);check(app.picker.value==='system','invalid storage safely uses system');
app = load(null,true,true);check(app.root.dataset.theme==='dark','blocked storage does not break theme');
app.click({ctrlKey:true});check(app.toast.hidden,'modified click leaves browser behavior intact');
const event = {defaultPrevented:false,preventDefault:()=>{throw Error('must keep real link');}};
app.click(event);check(!app.toast.hidden && app.status.textContent==='Permintaan unduh dikirim. Periksa unduhan browser.','download notice is honest and does not prevent navigation');
app.handlers.close();check(app.toast.hidden,'notice dismisses via close button');
app.click(event);app.timerCallbacks.at(-1)();check(app.toast.hidden,'notice auto dismisses');
console.log(`${checks} interaction checks PASS`);
