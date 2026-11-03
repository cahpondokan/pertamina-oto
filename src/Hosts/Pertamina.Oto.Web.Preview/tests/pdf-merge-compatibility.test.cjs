const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js'),E=require('../wwwroot/js/pdf-engine.js'),M=require('../wwwroot/js/pdf-merge-engine.js');
const A=require('../wwwroot/js/pdf-annotations.js'),T=require('../wwwroot/js/pdf-tags.js');
const {annotated}=require('./annotation-fixtures.cjs');
const read=(f,a,b)=>f.bytes.slice(a,b),file=bytes=>({bytes,size:bytes.length,name:'synthetic.pdf'});
for(const tagged of [false,true])test('large adapter preserves '+(tagged?'tagged':'plain')+' annotation relationships',async()=>{
 const source=file(await annotated({tags:tagged})),chunks=[];
 const r=await M.run(P,E,{files:[source,source],items:[{rotation:90,range:''},{rotation:0,range:''}],options:{bookmarks:true,blank:'none'}},{read,sink:{write:async b=>chunks.push(b),file:async()=>file(Buffer.concat(chunks))}});
 const doc=await P.PDFDocument.load(Buffer.concat(chunks));assert.equal(r.count,4);assert.equal(A.read(P,doc).records.size,32);assert.equal(!!T.read(P,doc),tagged);assert.equal(doc.getPage(0).getRotation().angle,90);
});
const dir=process.env.OTO_PDF_TEST_DIR;
if(dir)for(const name of fs.readdirSync(dir).filter(n=>n.endsWith('.pdf')))test('random-access compatibility '+name,async()=>{
 const bytes=new Uint8Array(fs.readFileSync(path.join(dir,name)));let old;
 try{old={result:await E.run(P,{action:'inspect',buffers:[bytes]})};}catch(e){old={error:e.message};}
 if(bytes.length>E.LIMITS.fileBytes)return;
 try{const r=await M.load(P,E,file(bytes),read);if(old.error&&old.error!=='LIMIT')assert.fail('old guard rejected '+old.error);if(old.result)assert.equal(r.count,old.result.counts[0]);}
 catch(e){assert.equal(e.message,old.error);}
});
