'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js'),E=require('../wwwroot/js/pdf-engine.js'),S=require('../wwwroot/js/pdf-split-engine.js');
const T=require('../wwwroot/js/pdf-tags.js'),A=require('../wwwroot/js/pdf-annotations.js');
const {annotated}=require('./annotation-fixtures.cjs'),{tagged,key}=require('./tagged-fixtures.cjs');
const wrap=bytes=>({size:bytes.length,bytes,name:'source.pdf'}),read=(f,a,b)=>f.bytes.slice(a,b),hash=b=>crypto.createHash('sha256').update(b).digest('hex');
async function fixture(count=4,edit=()=>{}){const d=await P.PDFDocument.create();for(let i=0;i<count;i++)d.addPage([200+i,400]).drawText('PAGE '+i);edit(d);return d.save({useObjectStreams:false});}
async function split(bytes,ranges='1-2;3-4',extra={}){const files=[],progress=[];const result=await S.run(P,E,{action:'run',tool:'split',files:[wrap(bytes)],ranges,name:'parts.pdf',...extra},{read,progress:p=>progress.push(p),output:async i=>{const chunks=[];return {write:async b=>chunks.push(b),file:async()=>files[i]=wrap(Buffer.concat(chunks))};}});return {result,files,progress};}
test('file-backed split preserves source, page order and exact content bytes',async()=>{const b=await fixture(),before=hash(b),{result,files}=await split(b,'4,1;2-3'),source=await P.PDFDocument.load(b);assert.deepEqual(result.results.map(r=>r.count),[2,2]);for(const [part,indices]of [[0,[3,0]],[1,[1,2]]]){const out=await P.PDFDocument.load(files[part].bytes);indices.forEach((index,i)=>{assert.equal(out.getPage(i).getWidth(),source.getPage(index).getWidth());const contents=(d,n)=>d.getPage(n).node.Contents().asArray().map(r=>hash(d.context.lookup(r).getContents()));assert.deepEqual(contents(out,i),contents(source,index));});}assert.equal(hash(b),before);});
for(const tags of [false,true])test('file-backed split keeps all supported annotations and links, tagged='+tags,async()=>{const b=await annotated({tags,count:4}),{files}=await split(b);for(const f of files){const d=await P.PDFDocument.load(f.bytes);assert.equal(A.read(P,d).records.size,16);if(tags){const model=T.read(P,d);assert.ok(model);assert.equal(model.objectOwners.size,14);}assert.deepEqual((await S.run(P,E,{tool:'split',action:'inspect',files:[f]},{read})).counts,[2]);}});
test('tag metadata and table constraints remain owned by the existing copier',async()=>{const b=await tagged({count:4}),{files}=await split(b);for(const f of files){const m=T.read(P,await P.PDFDocument.load(f.bytes));assert.ok(m);assert.equal(m.pages.length,2);}});
for(const [label,edit,code]of [
 ['signature',d=>d.catalog.set(key('Perms'),d.context.obj({})),'UNSUPPORTED_SIGNATURE'],
 ['script',d=>d.catalog.set(key('OpenAction'),d.context.obj({S:'JavaScript',JS:P.PDFString.of('private sentinel')})),'UNSUPPORTED_ACTION'],
 ['form',d=>d.catalog.set(key('AcroForm'),d.context.obj({Fields:[{FT:'Tx'}]})),'UNSUPPORTED_FORM'],
 ['attachment',d=>d.catalog.set(key('AF'),d.context.obj([])),'UNSUPPORTED_ATTACHMENT']
])test('file-backed split rejects '+label+' before creating any output',async()=>{const b=await fixture(4,edit);let opened=0;for(const action of ['inspect','run'])await assert.rejects(S.run(P,E,{tool:'split',action,files:[wrap(b)],ranges:'1-2;3-4'},{read,output:async()=>{opened++;}}),new RegExp('^Error: '+code+'$'));assert.equal(opened,0);});
test('cross-part link targets fail closed without removing links',async()=>{await assert.rejects(split(await annotated({count:2,cross:true}),'1;2'),/^Error: LINK_TARGET$/);});
test('unsafe URI remains rejected',async()=>{const b=await annotated({edit:(d,s)=>s.records[5].dict.lookup(key('A'),P.PDFDict).set(key('URI'),P.PDFString.of('javascript:alert(1)'))});await assert.rejects(split(b,'1;2'),/^Error: UNSAFE_LINK$/);});
test('range validation rejects overlap, empty and invalid ranges before writing',async()=>{const b=await fixture();for(const ranges of ['1-3;3-4','1-2;','1-5','0;2-4']){let count=0;await assert.rejects(S.run(P,E,{tool:'split',action:'run',files:[wrap(b)],ranges},{read,output:async()=>{count++;}}),/^Error: (PARTITION|SPLIT|RANGE)$/);assert.equal(count,0);}});
test('input sizes, page count and supported operation are checked independently',async()=>{assert.equal(S.admission([{size:4000000000}]),4000000000);assert.throws(()=>S.admission([{size:4000000001}]),/^Error: TOTAL_BYTES$/);assert.throws(()=>S.admission([{size:8},{size:8}]),/^Error: SINGLE$/);const f=wrap(await fixture(201));await assert.rejects(S.run(P,E,{tool:'split',action:'inspect',files:[f]},{read}),/^Error: LIMIT$/);await assert.rejects(S.run(P,E,{tool:'organize',action:'inspect',files:[f]},{read}),/^Error: TOOL$/);});
test('write failures and damaged output never return a partial successful result',async()=>{const f=wrap(await fixture());for(const stage of ['write','verify']){let part=0;await assert.rejects(S.run(P,E,{tool:'split',action:'run',files:[f],ranges:'1;2-4'},{read,output:async()=>{const index=part++,chunks=[];return {write:async b=>{if(stage==='write'&&index===1)throw Error('STORAGE');chunks.push(b);},file:async()=>wrap(stage==='verify'&&index===1?Buffer.from('%PDF-corrupt'):Buffer.concat(chunks))};}}),/^Error: (STORAGE|INVALID)$/);assert.equal(part,2);}});
test('input is revalidated on execution, never trusted from earlier inspection',async()=>{const bytes=await fixture(),f=wrap(bytes);await S.run(P,E,{tool:'split',action:'inspect',files:[f]},{read});bytes[0]=0;await assert.rejects(S.run(P,E,{tool:'split',action:'run',files:[f],ranges:'1-2;3-4',validated:true},{read}),/^Error: INVALID$/);});

// A real PDF syntax layout whose whitespace stream is generated on demand.
// This exercises >32-bit file offsets/serialization without filling the user's disk.
const {virtualPdf,virtualRead}=require('./helpers/virtual-large-pdf.cjs');
test('exact 4 GB virtual PDF crosses 2 GB offsets, splits and verifies using bounded reads/writes',async()=>{
 const file=virtualPdf(4000000000),outputs=[];let maxRead=0,maxWrite=0,written=0;
 const spaces=Buffer.alloc(1048576,32);
 const read=(f,a,b)=>{maxRead=Math.max(maxRead,b-a);assert.ok(b-a<=1048576);return virtualRead(f,a,b);};
 const r=await S.run(P,E,{tool:'split',action:'run',files:[file],ranges:'1;2',name:'large'},{read,output:async index=>{
  const parts=[];let offset=0;
  return {write:async b=>{maxWrite=Math.max(maxWrite,b.length);assert.ok(b.length<=1048576);const bytes=Buffer.from(b.buffer,b.byteOffset,b.byteLength);const fill=b.length<=spaces.length&&bytes.equals(spaces.subarray(0,b.length));parts.push({offset,length:b.length,...(fill?{fill:32}:{bytes:Buffer.from(b)})});offset+=b.length;written+=b.length;},file:async()=>outputs[index]={parts,size:offset}};
 }});
 assert.deepEqual(r.results.map(p=>p.count),[1,1]);assert.ok(written>3990000000&&written<4000000000);assert.ok(outputs[0].size>2**31);assert.ok(maxRead<=1048576&&maxWrite<=1048576);
 for(const f of outputs)assert.deepEqual((await S.run(P,E,{tool:'split',action:'inspect',files:[f]},{read})).counts,[1]);
});

