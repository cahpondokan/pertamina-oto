/* OTO browser PDF operations. No network, persistence, DOM, or path access. */
(function(root) {
  'use strict';
  const Tags=typeof module!=='undefined'?require('./pdf-tags.js'):root.OtoPdfTags;
  const Annotations=typeof module!=='undefined'?require('./pdf-annotations.js'):root.OtoPdfAnnotations;
  const Capacity=typeof module!=='undefined'?require('./pdf-capacity.js'):root.OtoPdfCapacity;
  const LIMITS = Capacity.bounded;
  const TOOLS = Capacity.tools;
  const IMAGE_LIMITS = Capacity.images;
  function fail(code) { throw new Error(code); }
  function number(value,min,max,integer=false) {
    if(typeof value!=='number'||!Number.isFinite(value)||value<min||value>max||(integer&&!Number.isInteger(value)))fail('OPTIONS');
    return value;
  }
  function choice(value,allowed) { if(!allowed.includes(value))fail('OPTIONS');return value; }
  const mm=value=>value*72/25.4;
  // Header limits precede native decoding. Never pass arbitrary PNG data to pdf-lib's inflater.
  function imageHeader(buffer) {
    const b=new Uint8Array(buffer),v=new DataView(b.buffer,b.byteOffset,b.byteLength);
    let width,height,type;
    if(b.length>=33&&[137,80,78,71,13,10,26,10].every((n,i)=>b[i]===n)) {
      if(v.getUint32(8)!==13||String.fromCharCode(...b.subarray(12,16))!=='IHDR'||b[24]!==8||![0,2,3,4,6].includes(b[25]))fail('IMAGE_FORMAT');
      width=v.getUint32(16);height=v.getUint32(20);type='image/png';
      let i=8,ended=false;
      while(i+12<=b.length) {
        const length=v.getUint32(i),kind=String.fromCharCode(...b.subarray(i+4,i+8));
        if(length>b.length-i-12)fail('IMAGE_FORMAT');
        if(kind==='acTL')fail('IMAGE_FORMAT');
        i+=length+12;
        if(kind==='IEND'){ended=true;break;}
      }
      if(!ended)fail('IMAGE_FORMAT');
    } else if(b.length>12&&b[0]===255&&b[1]===216) {
      type='image/jpeg';let i=2;
      while(i+4<=b.length) {
        if(b[i++]!==255)fail('IMAGE_FORMAT');
        while(b[i]===255)i++;
        const marker=b[i++];
        if(marker===218||marker===217)break;
        if(marker===1||(marker>=208&&marker<=215))continue;
        if(i+2>b.length)fail('IMAGE_FORMAT');
        const length=v.getUint16(i);
        if(length<2||i+length>b.length)fail('IMAGE_FORMAT');
        if([192,193,194].includes(marker)) {
          if(length<8||b[i+2]!==8||![1,3].includes(b[i+7]))fail('IMAGE_FORMAT');
          height=v.getUint16(i+3);width=v.getUint16(i+5);break;
        }
        i+=length;
      }
    } else fail('IMAGE_FORMAT');
    if(!width||!height)fail('IMAGE_FORMAT');
    if(width>IMAGE_LIMITS.dimension||height>IMAGE_LIMITS.dimension||width*height>IMAGE_LIMITS.pixels)fail('IMAGE_LIMIT');
    return {width,height,type};
  }
  async function imageInput(buffers,visit) {
    if(!Array.isArray(buffers)||!buffers.length||buffers.length>LIMITS.files||buffers.some(b=>!b||b.byteLength>LIMITS.fileBytes)||buffers.reduce((n,b)=>n+b.byteLength,0)>LIMITS.totalBytes)fail('LIMIT');
    const headers=buffers.map(imageHeader);
    if(headers.reduce((n,h)=>n+h.width*h.height,0)>IMAGE_LIMITS.totalPixels)fail('IMAGE_LIMIT');
    if(typeof createImageBitmap!=='function'||typeof OffscreenCanvas!=='function')fail('IMAGE_BROWSER');
    const sizes=[];
    for(let i=0;i<buffers.length;i++) {
      let bitmap;
      try {
        bitmap=await createImageBitmap(new Blob([buffers[i]],{type:headers[i].type}));
        if(!bitmap.width||!bitmap.height||bitmap.width*bitmap.height>IMAGE_LIMITS.pixels||bitmap.width>IMAGE_LIMITS.dimension||bitmap.height>IMAGE_LIMITS.dimension)fail('IMAGE_LIMIT');
        sizes.push({width:bitmap.width,height:bitmap.height});
        if(visit)await visit(bitmap,i);
      } catch(e) { if(['IMAGE_LIMIT','IMAGE_BROWSER','LIMIT','OPTIONS','VERIFY'].includes(e.message))throw e;fail('IMAGE_FORMAT'); }
      finally {bitmap?.close();}
    }
    return sizes;
  }
  async function imagesPdf(lib,request) {
    if(request.action==='inspect') {
      const thumbnails=[];
      const sizes=await imageInput(request.buffers,async bitmap=>{
        const scale=Math.min(1,240/Math.max(bitmap.width,bitmap.height));
        const canvas=new OffscreenCanvas(Math.max(1,Math.round(bitmap.width*scale)),Math.max(1,Math.round(bitmap.height*scale)));
        try {
          const ctx=canvas.getContext('2d',{alpha:false});if(!ctx)fail('IMAGE_BROWSER');
          ctx.fillStyle='#fff';ctx.fillRect(0,0,canvas.width,canvas.height);ctx.drawImage(bitmap,0,0,canvas.width,canvas.height);
          thumbnails.push(new Uint8Array(await (await canvas.convertToBlob({type:'image/jpeg',quality:.8})).arrayBuffer()));
        } finally {canvas.width=1;canvas.height=1;}
      });
      return {counts:sizes.map(()=>1),sizes,thumbnails};
    }
    const options=request.options||{},layout=choice(options.layout,['auto','portrait','landscape']),margin=mm(number(options.margin,0,30));
    const output=await lib.PDFDocument.create();output.setProducer('PERTAMINA OTO Web · pdf-lib');
    let encoded=0;
    await imageInput(request.buffers,async bitmap=>{
      const canvas=new OffscreenCanvas(bitmap.width,bitmap.height);
      try {
        const ctx=canvas.getContext('2d',{alpha:false});if(!ctx)fail('IMAGE_BROWSER');
        ctx.fillStyle='#ffffff';ctx.fillRect(0,0,bitmap.width,bitmap.height);ctx.drawImage(bitmap,0,0);
        const blob=await canvas.convertToBlob({type:'image/jpeg',quality:0.92});
        if(blob.type!=='image/jpeg')fail('IMAGE_BROWSER');
        encoded+=blob.size;if(encoded>LIMITS.outputBytes)fail('LIMIT');
        const image=await output.embedJpg(await blob.arrayBuffer());
        const landscape=layout==='landscape'||(layout==='auto'&&bitmap.width>bitmap.height);
        const [width,height]=landscape?[mm(297),mm(210)]:[mm(210),mm(297)];
        const scale=Math.min((width-2*margin)/image.width,(height-2*margin)/image.height);
        const page=output.addPage([width,height]);
        page.drawImage(image,{x:(width-image.width*scale)/2,y:(height-image.height*scale)/2,width:image.width*scale,height:image.height*scale});
      } finally {canvas.width=1;canvas.height=1;}
    });
    const bytes=await output.save({useObjectStreams:false,addDefaultPage:false});
    if(bytes.length>LIMITS.outputBytes)fail('LIMIT');
    if((await lib.PDFDocument.load(bytes)).getPageCount()!==request.buffers.length)fail('VERIFY');
    return {results:[{name:cleanName(request.name),count:request.buffers.length,bytes}]};
  }
  function geometry(lib,page) {
    const m=page.getMediaBox(),c=page.getCropBox(),rotation=((page.getRotation().angle%360)+360)%360;
    const unit=page.node.lookup(lib.PDFName.of('UserUnit'));
    if(unit&&(!(unit instanceof lib.PDFNumber)||unit.asNumber()!==1))fail('LAYOUT');
    const x=Math.max(m.x,c.x),y=Math.max(m.y,c.y),w=Math.min(m.x+m.width,c.x+c.width)-x,h=Math.min(m.y+m.height,c.y+c.height)-y;
    if(![0,90,180,270].includes(rotation)||![x,y,w,h].every(Number.isFinite)||w<28||h<28||w>14400||h>14400)fail('LAYOUT');
    const point=(u,v)=>rotation===0?{x:x+u,y:y+v}:rotation===90?{x:x+w-v,y:y+u}:rotation===180?{x:x+w-u,y:y+h-v}:{x:x+v,y:y+h-u};
    return {width:rotation%180?h:w,height:rotation%180?w:h,rotation,point};
  }
  async function decorate(lib,output,request) {
    const tool=request.tool,options=request.options||{},pages=output.getPages();
    const selected=request.ranges?.trim()?range(request.ranges,pages.length):pages.map((_,i)=>i);
    const font=tool==='crop'?null:await output.embedFont(lib.StandardFonts.Helvetica);
    for(let n=0;n<selected.length;n++) {
      const page=pages[selected[n]],g=geometry(lib,page);
      if(tool==='crop') {
        const [top,right,bottom,left]=['top','right','bottom','left'].map(k=>mm(number(options[k],0,500)));
        if(g.width-left-right<mm(10)||g.height-top-bottom<mm(10))fail('LAYOUT');
        const points=[g.point(left,bottom),g.point(g.width-right,bottom),g.point(left,g.height-top),g.point(g.width-right,g.height-top)];
        const xs=points.map(p=>p.x),ys=points.map(p=>p.y);
        page.setCropBox(Math.min(...xs),Math.min(...ys),Math.max(...xs)-Math.min(...xs),Math.max(...ys)-Math.min(...ys));
      } else if(tool==='numbers') {
        const start=number(options.start,1,99999,true),size=number(options.size,8,32),margin=mm(number(options.margin,4,30));
        const position=choice(options.position,['bottom-left','bottom-center','bottom-right','top-left','top-center','top-right']);
        const format=choice(options.format,['number','total']);
        const text=String(start+n)+(format==='total'?' / '+(start+selected.length-1):''),width=font.widthOfTextAtSize(text,size);
        if(width+margin*2>g.width||margin*2+size>g.height)fail('LAYOUT');
        const x=position.endsWith('left')?margin:position.endsWith('right')?g.width-margin-width:(g.width-width)/2;
        const y=position.startsWith('top')?g.height-margin-size:margin;
        const finish=output.catalog.has(lib.PDFName.of('StructTreeRoot'))?Tags.decoration(lib,output,page,'numbers',text):()=>{};
        page.drawText(text,{...g.point(x,y),font,size,color:lib.rgb(.08,.16,.28),rotate:lib.degrees(g.rotation)});finish();
      } else {
        const text=typeof options.text==='string'?options.text.trim():'';
        if(!text||text.length>80||/[\u0000-\u001f\u007f]/.test(text))fail('TEXT');
        try {font.encodeText(text);}catch{fail('TEXT');}
        let size=number(options.size,12,100);
        const angle=choice(options.angle,[0,45]),opacity=number(options.opacity,5,70)/100;
        const color=choice(options.color,['blue','gray','red']);
        const rad=angle*Math.PI/180,cos=Math.cos(rad),sin=Math.sin(rad),unitWidth=font.widthOfTextAtSize(text,1);
        size=Math.min(size,(g.width-24)/(unitWidth*cos+sin),(g.height-24)/(unitWidth*sin+cos));
        if(size<8)fail('LAYOUT');
        const width=unitWidth*size,x=g.width/2-width*cos/2+size*sin/2,y=g.height/2-width*sin/2-size*cos/2;
        const rgb=color==='blue'?[.03,.29,.65]:color==='red'?[.7,.07,.12]:[.3,.3,.3];
        const finish=output.catalog.has(lib.PDFName.of('StructTreeRoot'))?Tags.decoration(lib,output,page,'watermark',text):()=>{};
        page.drawText(text,{...g.point(x,y),font,size,opacity,color:lib.rgb(...rgb),rotate:lib.degrees(g.rotation+angle)});finish();
      }
    }
  }
  function range(text, count) {
    if (typeof text !== 'string' || text.length > 1000 || !text.trim()) fail('RANGE');
    const found = [];
    for (const token of text.split(',')) {
      const match = /^\s*(\d{1,4})(?:\s*-\s*(\d{1,4}))?\s*$/.exec(token);
      if (!match) fail('RANGE');
      const start = Number(match[1]), end = Number(match[2] || match[1]);
      if (start < 1 || end < start || end > count) fail('RANGE');
      for (let i=start; i<=end; i++) { if (found.includes(i-1)) fail('RANGE'); found.push(i-1); }
    }
    return found;
  }
  function cleanName(value, suffix='hasil') {
    const name = String(value || suffix).replace(/\.pdf$/i,'').replace(/[<>:"/\\|?*\u0000-\u001f]/g,'_').trim().replace(/[. ]+$/,'').slice(0,100);
    return ((!name || /^(con|prn|aux|nul|com\d|lpt\d)$/i.test(name)) ? suffix : name) + '.pdf';
  }
  // Dereference only within the parsed document. Missing/cyclic references are not
  // equivalent to empty structures. A small bound also prevents reference-chain abuse.
  function resolveStructure(lib,doc,value) {
    const seen=new Set();
    while(value instanceof lib.PDFRef) {
      const key=value.toString();
      if(seen.has(key)||seen.size>=32)fail('INVALID');
      seen.add(key);value=doc.context.lookup(value);
      if(value===undefined)fail('INVALID');
    }
    return value;
  }
  function emptyForm(lib,doc,value) {
    const resolved=resolveStructure(lib,doc,value);
    if(resolved===lib.PDFNull)return true;
    if(!(resolved instanceof lib.PDFDict))return false;
    // No fields means no interactive state. Admit only a bounded, static default
    // appearance and standard-font resources; never use getForm() (it normalizes
    // form structures). XFA, flags, calculation order and real fields stay denied.
    const entries=resolved.entries();
    if(entries.length===0)return true;
    const key=s=>lib.PDFName.of(s),get=(d,k)=>resolveStructure(lib,doc,d.get(key(k)));
    const keys=(d,allowed)=>d instanceof lib.PDFDict&&d.entries().every(([k])=>allowed.includes(k.decodeText()));
    const name=v=>v instanceof lib.PDFName?v.decodeText():undefined;
    if(!keys(resolved,['Fields','DA','DR']))return false;
    const fields=get(resolved,'Fields');if(!(fields instanceof lib.PDFArray)||fields.size()!==0)return false;
    function encoding(v){
      if(v instanceof lib.PDFName)return ['WinAnsiEncoding','MacRomanEncoding','MacExpertEncoding'].includes(name(v));
      if(!keys(v,['Type','BaseEncoding','Differences']))return false;
      if(v.has(key('Type'))&&name(get(v,'Type'))!=='Encoding')return false;
      if(v.has(key('BaseEncoding'))&&!['WinAnsiEncoding','MacRomanEncoding','MacExpertEncoding'].includes(name(get(v,'BaseEncoding'))))return false;
      if(v.has(key('Differences'))){const a=get(v,'Differences');if(!(a instanceof lib.PDFArray)||a.size()>512)return false;let index=-1;
        for(const item of a.asArray()){const x=resolveStructure(lib,doc,item);if(x instanceof lib.PDFNumber){index=x.asNumber();if(!Number.isInteger(index)||index<0||index>255)return false;}
          else if(!(x instanceof lib.PDFName)||index<0||index++>255||x.decodeText().length>128)return false;}
      }return true;
    }
    let fonts;
    if(resolved.has(key('DR'))){const dr=get(resolved,'DR');if(!keys(dr,['Font','Encoding']))return false;
      for(const category of ['Font','Encoding'])if(dr.has(key(category))){const dict=get(dr,category);if(!(dict instanceof lib.PDFDict)||dict.entries().length>64)return false;
        if(category==='Font')fonts=dict;
        for(const [,raw] of dict.entries()){const v=resolveStructure(lib,doc,raw);
          if(category==='Encoding'){if(!encoding(v))return false;continue;}
          if(!keys(v,['Type','Subtype','BaseFont','Encoding','Name'])||name(get(v,'Type'))!=='Font'||name(get(v,'Subtype'))!=='Type1'||!Object.values(lib.StandardFonts).includes(name(get(v,'BaseFont'))))return false;
          if(v.has(key('Name'))&&!(get(v,'Name') instanceof lib.PDFName))return false;
          if(v.has(key('Encoding'))&&!encoding(get(v,'Encoding')))return false;
        }
      }
    }
    if(resolved.has(key('DA'))){const da=get(resolved,'DA');if(!(da instanceof lib.PDFString||da instanceof lib.PDFHexString)||da.asBytes().length>2048)return false;
      const tokens=da.decodeText().trim().split(/\s+/),stack=[];if(tokens.length===1&&tokens[0]==='')return true;
      for(const token of tokens){if(/^\/[A-Za-z0-9_.+-]{1,64}$/.test(token)){stack.push(token);continue;}
        if(/^[+-]?(?:\d+\.?\d*|\.\d+)$/.test(token)){const n=Number(token);if(!Number.isFinite(n)||Math.abs(n)>10000)return false;stack.push(n);continue;}
        const arity={Tf:2,g:1,rg:3,k:4}[token];if(!arity||stack.length!==arity)return false;
        if(token==='Tf'){if(typeof stack[0]!=='string'||typeof stack[1]!=='number'||stack[1]<0||!fonts?.has(key(stack[0].slice(1))))return false;}
        else if(stack.some(n=>typeof n!=='number'||n<0||n>1))return false;
        stack.length=0;
      }if(stack.length)return false;
    }
    return true;
  }
  // Keep admitted empty form defaults in outputs. Conflicting defaults from
  // different sources are not silently overwritten or stripped during merging.
  function copyEmptyForms(lib,docs,items,output) {
    const key=s=>lib.PDFName.of(s);let selected=null,fingerprint=null,minimal=false;
    for(const source of new Set(items.map(p=>p.source))){const doc=docs[source],raw=doc.catalog.get(key('AcroForm'));
      if(raw===undefined)continue;const form=resolveStructure(lib,doc,raw);
      if(form===lib.PDFNull)continue;
      if(!emptyForm(lib,doc,form))fail('UNSUPPORTED_FORM');minimal=true;
      if(!form.has(key('DA'))&&!form.has(key('DR')))continue;
      let steps=0;
      function canonical(value,active=new Set(),depth=0){if(++steps>5000||depth>32)fail('LIMIT');const v=resolveStructure(lib,doc,value);
        if(v instanceof lib.PDFDict||v instanceof lib.PDFArray){if(active.has(v))fail('INVALID');active.add(v);
          const result=v instanceof lib.PDFArray?v.asArray().map(x=>canonical(x,active,depth+1)):v.entries().sort((a,b)=>a[0].toString().localeCompare(b[0].toString())).map(([k,x])=>[k.toString(),canonical(x,active,depth+1)]);active.delete(v);return result;}
        return v.toString();
      }
      const signature=JSON.stringify(canonical(form));if(fingerprint!==null&&signature!==fingerprint)fail('UNSUPPORTED_FORM');fingerprint=signature;selected={doc,form};
    }
    if(selected)output.catalog.set(key('AcroForm'),lib.PDFObjectCopier.for(selected.doc.context,output.context).copy(selected.form));
    else if(minimal)output.catalog.set(key('AcroForm'),output.context.obj({Fields:[]}));
  }
  function inspectStructures(lib,doc,visitLimit=80000) {
    // A merged output contains references plus rebuilt tag/parent dictionaries.
    // Only the merge verifier opts into the larger, still bounded work budget.
    if(!Number.isSafeInteger(visitLimit)||visitLimit<1||visitLimit>160000)fail('OPTIONS');
    const rejected=new Map([
      ...['AA','JavaScript','JS'].map(k=>[k,'UNSUPPORTED_ACTION']),
      ...['EmbeddedFiles','Filespec','EF','AF'].map(k=>[k,'UNSUPPORTED_ATTACHMENT']),
      ...['Perms','Sig'].map(k=>[k,'UNSUPPORTED_SIGNATURE']),
      ['XFA','UNSUPPORTED_FORM'], ['OPI','UNSUPPORTED_ACTION']
    ]);
    const activeActions=new Set(['JavaScript','Launch','GoToR','GoToE','SubmitForm','ImportData','ResetForm','Hide','Named','SetOCGState','Rendition','Sound','Movie','Thread','Trans','GoTo3DView']);
    // Preserve inspection of every indirect object, even if it is not reachable
    // from the catalog. Admitting an empty container must not hide active payloads.
    const queue=[doc.catalog,...doc.context.enumerateIndirectObjects().map(pair=>pair[1])],seen=new Set();
    let inspected=0;
    while(queue.length) {
      let obj=queue.pop();
      if(!obj || seen.has(obj))continue;
      seen.add(obj);if(++inspected>visitLimit)fail('LIMIT');
      if(obj instanceof lib.PDFRef){queue.push(resolveStructure(lib,doc,obj));continue;}
      if(obj instanceof lib.PDFDict) {
        for(const [key,value] of obj.entries()) {
          const name=key.decodeText();
          if(name==='OpenAction'&&obj!==doc.catalog)fail('UNSUPPORTED_ACTION');
          if(name==='AcroForm'&&!emptyForm(lib,doc,value))fail('UNSUPPORTED_FORM');
          if(rejected.has(name))fail(rejected.get(name));
          if(['S','Type','FT'].includes(name)) {
            const resolved=resolveStructure(lib,doc,value);
            if(resolved instanceof lib.PDFName) {
              const kind=resolved.decodeText();
              if(name==='S'&&activeActions.has(kind))fail('UNSUPPORTED_ACTION');
              if(['Type','FT'].includes(name)&&kind==='Sig')fail('UNSUPPORTED_SIGNATURE');
              if(name==='Type'&&['Filespec','EmbeddedFile'].includes(kind))fail('UNSUPPORTED_ATTACHMENT');
            }
          }
          queue.push(value);
        }
      } else if(obj instanceof lib.PDFArray)queue.push(...obj.asArray());
      else if(obj instanceof lib.PDFRawStream){if(obj.dict.has(lib.PDFName.of('F'))||obj.dict.has(lib.PDFName.of('Ref'))||resolveStructure(lib,doc,obj.dict.get(lib.PDFName.of('Subtype')))===lib.PDFName.of('PS'))fail('UNSUPPORTED_ACTION');queue.push(obj.dict);}
    }
    const annotations=Annotations.read(lib,doc),key=s=>lib.PDFName.of(s);
    // URI/GoTo actions need validated ownership: Link annotations or strictly
    // internal catalog/bookmark navigation. Detached/external actions still fail.
    for(const obj of seen)if(obj instanceof lib.PDFDict){
      const type=resolveStructure(lib,doc,obj.get(key('Type'))),s=resolveStructure(lib,doc,obj.get(key('S')));
      if((type instanceof lib.PDFName&&type.decodeText()==='Action'||s instanceof lib.PDFName&&['URI','GoTo'].includes(s.decodeText()))&&!annotations.actions.has(obj)&&!annotations.navigationActions.has(obj))fail('UNSUPPORTED_ACTION');
      if(obj.has(key('URI'))&&!annotations.actions.has(obj))fail('UNSUPPORTED_ACTION');
      if(type instanceof lib.PDFName&&type.decodeText()==='Annot'&&!annotations.records.has(obj))fail('UNSUPPORTED_ANNOTATIONS');
      if(obj.has(key('Annots'))&&!annotations.pageIndex.has(obj))fail('UNSUPPORTED_ANNOTATIONS');
    }
    return annotations;
  }
  async function loadCheckedSources(lib, buffers,progress=()=>{}) {
    if (!Array.isArray(buffers) || !buffers.length || buffers.length > LIMITS.files) fail('LIMIT');
    if (buffers.some(b=>!b || b.byteLength>LIMITS.fileBytes) || buffers.reduce((n,b)=>n+b.byteLength,0)>LIMITS.totalBytes) fail('LIMIT');
    let total=0, pages=0;
    const docs=[],tagged=[],annotations=[];
    progress({stage:"inspect",done:0,total:buffers.length});
    for (const buffer of buffers) {
      const bytes = new Uint8Array(buffer);
      total += bytes.byteLength;
      if (bytes.length < 8) fail('INVALID');
      if (bytes.length > LIMITS.fileBytes || total > LIMITS.totalBytes) fail('LIMIT');
      if (String.fromCharCode(...bytes.subarray(0,5)) !== '%PDF-') fail('INVALID');
      let doc;
      try { doc=await lib.PDFDocument.load(bytes,{ignoreEncryption:false,updateMetadata:false,throwOnInvalidObject:true}); }
      catch(e) { if(e?.message==='OTO_LIMIT') fail('LIMIT'); if(e instanceof lib.EncryptedPDFError || e?.message?.startsWith('Input document to `PDFDocument.load` is encrypted.')) fail('ENCRYPTED'); fail('INVALID'); }
      if (doc.isEncrypted) fail('ENCRYPTED');
      let count;
      try { count=doc.getPageCount(); } catch { fail('INVALID'); }
      pages+=count;
      if(!count || pages>LIMITS.pages) fail('LIMIT');
      annotations.push(inspectStructures(lib,doc));
      tagged.push(Tags.read(lib,doc));
      docs.push(doc);progress({stage:"inspect",done:docs.length,total:buffers.length});
    }
    return {docs,tagged,annotations};
  }
  async function loadSources(lib,buffers){return (await loadCheckedSources(lib,buffers)).docs;}
  function previewGeometry(doc){return doc.getPages().map(p=>({width:p.getCropBox().width,height:p.getCropBox().height,rotation:p.getRotation().angle}));}
  async function run(lib, request,progress=()=>{}) {
    // Single-document admission is enforced during inspection as well as export.
    if(TOOLS.includes(request.tool)&&!Capacity.input(request.tool).multiple&&Array.isArray(request.buffers)&&request.buffers.length>1)fail('SINGLE');
    if(request.tool==='images-pdf')return imagesPdf(lib,request);
    // Each execution still parses and validates every input. Reuse the models
    // from this exact validation pass, never an external or persistent receipt.
    const {docs,tagged,annotations}=await loadCheckedSources(lib,request.buffers,progress);
    const counts=docs.map(d=>d.getPageCount());
    function notices(m,a,source=false){return [...(m?.partial?['PARTIAL_TAGS']:[]),...(m?.orphans.length?['ORPHAN_TAG']:[]),...(a.relativeLinks?['RELATIVE_LINKS']:[]),...(source&&a.sourceNavigation?['SOURCE_NAVIGATION']:[])];}
    const inputWarnings=new Set(tagged.flatMap((m,i)=>notices(m,annotations[i],true)));
    if(tagged.some(Boolean)&&tagged.some(m=>!m))inputWarnings.add('PARTIAL_TAGS');
    if(request.action==='inspect') return {counts,...(inputWarnings.size?{warnings:[...inputWarnings]}:{}),...(request.details===true?{sources:tagged.map((m,i)=>({tagged:!!m,duplicable:!m&&!annotations[i].records.size,geometry:previewGeometry(docs[i]),warnings:notices(m,annotations[i],true)}))}:{})};
    const tool=request.tool;
    if(!TOOLS.includes(tool)) fail('TOOL');
    if(!Capacity.input(tool).multiple && docs.length!==1) fail('SINGLE');
    if(tool==='merge' && docs.length<Capacity.input('merge').minimum) fail('MERGE');
    let all=docs.flatMap((d,source)=>d.getPages().map((p,index)=>({source,index,rotation:0})));
    let groups;
    if(tool==='organize') {
      all=request.pages;
      if(!Array.isArray(all) || !all.length || all.length>LIMITS.pages) fail('EMPTY');
      for(const p of all) if(!Number.isInteger(p.source) || !Number.isInteger(p.index) || !docs[p.source] || p.index<0 || p.index>=counts[p.source] || ![0,90,180,270].includes(p.rotation)) fail('RANGE');
      groups=[all];
    } else if(tool==='split') {
      const mode=request.splitMode||'ranges';
      if(mode==='pages'||mode==='all'||mode==='extract-all')groups=all.map(page=>[page]);
      else if(mode==='ranges'||mode==='custom'){
        if(typeof request.ranges!=='string'||request.ranges.length>1000||!request.ranges.trim())fail('RANGE');
        const parts=request.ranges.split(';');if(parts.length>LIMITS.outputs)fail('SPLIT');
        const indices=parts.map(p=>range(p,counts[0])),flat=indices.flat();
        if(!flat.length||new Set(flat).size!==flat.length)fail('PARTITION');groups=indices.map(g=>g.map(i=>all[i]));
        if(request.combineRanges)groups=[groups.flat()];
      }else if(mode==='fixed'){
        const size=request.pagesPerFile;if(!Number.isInteger(size)||size<1||size>counts[0])fail('RANGE');groups=[];for(let i=0;i<all.length;i+=size)groups.push(all.slice(i,i+size));
      }else if(mode==='select'){
        const chosen=request.selectedPages;if(!Array.isArray(chosen)||!chosen.length||chosen.length>LIMITS.pages||chosen.some(i=>!Number.isInteger(i)||i<0||i>=counts[0])||new Set(chosen).size!==chosen.length)fail('RANGE');
        const selected=chosen.map(i=>all[i]);groups=request.combineSelected?[selected]:selected.map(page=>[page]);
      }else fail('SPLIT');
      if(!groups.length||groups.length>LIMITS.outputs)fail('SPLIT');
    } else if(['extract','remove','rotate'].includes(tool)) {
      const chosen=range(request.ranges,counts[0]);
      if(tool==='extract') all=chosen.map(i=>all[i]);
      if(tool==='remove') all=all.filter(p=>!chosen.includes(p.index));
      if(tool==='rotate') {
        if(![90,180,270].includes(request.angle)) fail('RANGE');
        all=all.map(p=>({...p,rotation:chosen.includes(p.index)?request.angle:0}));
      }
      if(!all.length) fail('EMPTY');
      groups=[all];
    } else groups=[all];
    const results=[],warnings=new Set(inputWarnings.has('SOURCE_NAVIGATION')?['SOURCE_NAVIGATION']:[]); let outputTotal=0;
    for(let part=0;part<groups.length;part++) {
      const preserveTags=groups[part].some(p=>tagged[p.source]);
      const preserveAnnotations=groups[part].some(p=>annotations[p.source].records.size);
      progress({stage:"copy",done:0,total:0});
      let copiedCount=0;
      const output=preserveTags?await Tags.copy(lib,docs,tagged,groups[part]):preserveAnnotations?await Annotations.copy(lib,docs,annotations,groups[part]):await lib.PDFDocument.create();
      output.setProducer('PERTAMINA OTO Web · pdf-lib');
      if(!preserveTags&&!preserveAnnotations)for(const item of groups[part]) {
        const [copied]=await output.copyPages(docs[item.source],[item.index]);
        copied.setRotation(lib.degrees((copied.getRotation().angle + item.rotation)%360));
        output.addPage(copied);progress({stage:"copy",done:++copiedCount,total:groups[part].length});
      }
      progress({stage:"copy",done:groups[part].length,total:groups[part].length});
      copyEmptyForms(lib,docs,groups[part],output);
      if(['numbers','watermark','crop'].includes(tool))await decorate(lib,output,request);
      progress({stage:"write",done:0,total:0});
      const bytes=await output.save({useObjectStreams:false,addDefaultPage:false});
      outputTotal+=bytes.length;
      if(outputTotal>LIMITS.outputBytes) fail('LIMIT');
      progress({stage:"verify",done:part,total:groups.length});
      const check=await lib.PDFDocument.load(bytes,{updateMetadata:false,throwOnInvalidObject:true});
      if(check.getPageCount()!==groups[part].length) fail('VERIFY');
      const a=inspectStructures(lib,check),m=Tags.read(lib,check);notices(m,a).forEach(w=>warnings.add(w));
      const base=cleanName(request.name).replace(/\.pdf$/i,'');
      results.push({name:cleanName(groups.length>1?base+'_'+String(part+1).padStart(2,'0'):base),count:groups[part].length,bytes,geometry:previewGeometry(check)});
      progress({stage:"verify",done:part+1,total:groups.length});
    }
    return {results,...(warnings.size?{warnings:[...warnings]}:{})};
  }
  const api={LIMITS,IMAGE_LIMITS,TOOLS,Capacity,range,cleanName,run,loadSources,imageHeader,inspectStructures,copyEmptyForms,decorate};
  if(typeof module!=='undefined') module.exports=api; else root.OtoPdf=api;
})(typeof self!=='undefined'?self:globalThis);

