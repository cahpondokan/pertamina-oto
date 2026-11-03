'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const E=require('../wwwroot/js/pdf-engine.js'),P=require('../wwwroot/vendor/pdf-lib-1.17.1/pdf-lib.js');
const dir=process.env.OTO_PDF_TEST_DIR,A=new Uint8Array(fs.readFileSync(path.join(dir,'A.pdf')));
const original=crypto.createHash('sha256').update(A).digest('hex');
const defaults={numbers:{start:1,size:12,margin:12,position:'bottom-center',format:'total'},watermark:{text:'DRAF OTO',size:48,opacity:18,angle:45,color:'blue'},crop:{top:5,right:10,bottom:15,left:20}};
const request=(tool,extra={})=>({action:'run',tool,buffers:[A],name:tool,options:defaults[tool],...extra});
for(const tool of Object.keys(defaults))test(tool+' creates ordinary PDF with unchanged page count and source',async()=>{
  const {results}=await E.run(P,request(tool));assert.equal(results[0].count,4);assert.equal((await P.PDFDocument.load(results[0].bytes)).getPageCount(),4);
  fs.writeFileSync(path.join(dir,tool+'.pdf'),results[0].bytes);assert.equal(crypto.createHash('sha256').update(A).digest('hex'),original);
});
for(const position of ['bottom-left','bottom-center','bottom-right','top-left','top-center','top-right'])test('numbers position '+position,async()=>{
  const r=await E.run(P,request('numbers',{options:{...defaults.numbers,position},ranges:'4,2'}));assert.equal(r.results[0].count,4);
});
test('rotated crop box with nonzero origin handled in visual coordinates',async()=>{
  const d=await P.PDFDocument.load(A);[0,90,180,270].forEach((angle,i)=>{d.getPage(i).setRotation(P.degrees(angle));d.getPage(i).setCropBox(10,20,260,360);});
  const bytes=await d.save();fs.writeFileSync(path.join(dir,'rotated-source.pdf'),bytes);
  for(const tool of Object.keys(defaults)){
    const r=await E.run(P,request(tool,{buffers:[bytes],name:tool+'-rotated'}));fs.writeFileSync(path.join(dir,tool+'-rotated.pdf'),r.results[0].bytes);
    const out=await P.PDFDocument.load(r.results[0].bytes);assert.deepEqual(out.getPages().map(p=>p.getRotation().angle),[0,90,180,270]);
    if(tool==='crop'){
      const unit=72/25.4;
      const expected=[[10+20*unit,20+15*unit,260-30*unit,360-20*unit],[10+5*unit,20+20*unit,260-20*unit,360-30*unit],[10+10*unit,20+5*unit,260-30*unit,360-20*unit],[10+15*unit,20+10*unit,260-20*unit,360-30*unit]];
      out.getPages().forEach((page,i)=>{const b=page.getCropBox();[b.x,b.y,b.width,b.height].forEach((v,j)=>assert.ok(Math.abs(v-expected[i][j])<.001));});
    }
  }
});
test('crop selection leaves other boxes intact',async()=>{
  const out=await E.run(P,request('crop',{ranges:'2'}));const d=await P.PDFDocument.load(out.results[0].bytes);
  assert.equal(d.getPage(0).getCropBox().width,300);assert.equal(d.getPage(2).getCropBox().height,420);assert.ok(d.getPage(1).getCropBox().width<300);
});
test('watermark long supported text fits or safely refuses',async()=>{
  const r=await E.run(P,request('watermark',{options:{...defaults.watermark,text:'W'.repeat(30)}}));assert.equal(r.results[0].count,4);
});
for(const [tool,options,code] of [
 ['numbers',{...defaults.numbers,start:NaN},'OPTIONS'],['numbers',{...defaults.numbers,start:1.5},'OPTIONS'],
 ['numbers',{...defaults.numbers,position:'injected'},'OPTIONS'],['numbers',{...defaults.numbers,size:400},'OPTIONS'],
 ['numbers',{...defaults.numbers,format:'bad'},'OPTIONS'],['numbers',{...defaults.numbers,margin:-2},'OPTIONS'],
 ['watermark',{...defaults.watermark,text:'😀'},'TEXT'],['watermark',{...defaults.watermark,text:''},'TEXT'],
 ['watermark',{...defaults.watermark,text:'line\nbreak'},'TEXT'],['watermark',{...defaults.watermark,text:'x'.repeat(81)},'TEXT'],
 ['watermark',{...defaults.watermark,opacity:100},'OPTIONS'],['watermark',{...defaults.watermark,angle:22},'OPTIONS'],
 ['watermark',{...defaults.watermark,color:'bad'},'OPTIONS'],['crop',{...defaults.crop,top:999},'OPTIONS'],
 ['crop',{...defaults.crop,left:100},'LAYOUT'],['crop',{...defaults.crop,bottom:NaN},'OPTIONS']
])test('rejects '+tool+' '+code+' '+JSON.stringify(options),async()=>assert.rejects(E.run(P,request(tool,{options})),new RegExp('^Error: '+code+'$')));
test('unsupported page scaling refuses physical-mm operations',async()=>{
 const d=await P.PDFDocument.load(A);d.getPage(0).node.set(P.PDFName.of('UserUnit'),P.PDFNumber.of(2));
 await assert.rejects(E.run(P,request('numbers',{buffers:[await d.save()]})),/LAYOUT/);
});
test('image headers accept bounded PNG and RGB JPEG',()=>{
 for(const file of ['portrait.jpg','transparent.png']){const info=E.imageHeader(fs.readFileSync(path.join(dir,file)));assert.equal(info.width,400);assert.equal(info.height,600);}
});
test('image pixel bomb rejected before any decoder',()=>{
 const bytes=fs.readFileSync(path.join(dir,'transparent.png'));bytes.writeUInt32BE(65535,16);assert.throws(()=>E.imageHeader(bytes),/IMAGE_LIMIT/);
});
test('disguised SVG rejected',()=>assert.throws(()=>E.imageHeader(Buffer.from('<svg xmlns="http://www.w3.org/2000/svg"></svg>')),/IMAGE_FORMAT/));
test('CMYK JPEG refused',()=>assert.throws(()=>E.imageHeader(fs.readFileSync(path.join(dir,'cmyk.jpg'))),/IMAGE_FORMAT/));
test('image processing fails honestly without browser decode APIs',async()=>assert.rejects(E.run(P,{action:'inspect',tool:'images-pdf',buffers:[fs.readFileSync(path.join(dir,'portrait.jpg'))]}),/IMAGE_BROWSER/));
test('image invalid bytes rejected',async()=>assert.rejects(E.run(P,{action:'inspect',tool:'images-pdf',buffers:[Buffer.from('not an image')]}),/IMAGE_FORMAT/));
