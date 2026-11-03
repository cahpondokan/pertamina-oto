/* Bounded PDF 1.7 structure preservation. No rendering, network or persistence.
 * Unknown relationships fail closed; this is not a PDF/UA conformance validator. */
(function(root){
  'use strict';
  const Annotations=typeof module!=='undefined'?require('./pdf-annotations.js'):root.OtoPdfAnnotations;
  const standard=new Set('Document Part Art Sect Div BlockQuote Caption TOC TOCI Index NonStruct Private P H H1 H2 H3 H4 H5 H6 L LI Lbl LBody Table TR TH TD THead TBody TFoot Span Quote Note Reference BibEntry Code Link Annot Ruby RB RT RP Warichu WT WP Figure Formula Form'.split(' '));
  const grouping=new Set(['Document','Part','Art','Sect','Div','NonStruct']);
  const fail=(code='TAGGED_STRUCTURE')=>{throw Error(code);};
  function helpers(P,doc){
    const key=s=>P.PDFName.of(s);
    function resolve(v){const seen=new Set();while(v instanceof P.PDFRef){if(seen.has(v)||seen.size>=32)fail();seen.add(v);v=doc.context.lookup(v);if(v===undefined)fail();}return v;}
    const get=(d,k)=>resolve(d.get(key(k)));
    const name=v=>{v=resolve(v);return v instanceof P.PDFName?v.decodeText():undefined;};
    const integer=v=>{v=resolve(v);if(!(v instanceof P.PDFNumber)||!Number.isSafeInteger(v.asNumber())||v.asNumber()<0||v.asNumber()>50000)fail();return v.asNumber();};
    const bytes=v=>{v=resolve(v);if(!(v instanceof P.PDFString||v instanceof P.PDFHexString))fail();const b=v.asBytes();if(b.length>2048)fail('LIMIT');return Array.from(b,n=>n.toString(16).padStart(2,'0')).join('');};
    function keys(d,allowed){if(!(d instanceof P.PDFDict))fail();for(const [k] of d.entries())if(!allowed.includes(k.decodeText()))fail('UNSUPPORTED_TAGGED');}
    function list(v){const resolved=resolve(v);return resolved===undefined||resolved===P.PDFNull?[]:resolved instanceof P.PDFArray?resolved.asArray():[v];}
    return {key,resolve,get,name,integer,bytes,keys,list};
  }
  // Over-approximate literal MCID usage before pruning stale reverse-index
  // entries. Do not strip strings, comments or inline-image bytes: a false
  // positive must refuse recovery, never hide a possible content reference.
  function possibleMCIDs(P,doc,streams,budget){
    const ids=new Set(),texts=[];if(streams.length>128)fail('LIMIT');
    for(const raw of streams){const stream=doc.context.lookup(raw);if(!(stream instanceof P.PDFRawStream))fail();
      let data;try{data=P.decodePDFRawStream(stream).decode();}catch(e){if(e.message==='OTO_LIMIT')fail('LIMIT');fail();}
      budget.bytes+=data.length;if(data.length>4*1024*1024||budget.bytes>16*1024*1024)fail('LIMIT');
      let text='';for(let i=0;i<data.length;i+=8192)text+=String.fromCharCode(...data.subarray(i,i+8192));
      texts.push(text);
    }
    // Contents arrays are concatenated streams. Inspect both each member and
    // their concatenation so neither split names nor adjacent tokens can hide
    // a possible MCID. Extra matches only make recovery more conservative.
    if(texts.length>1)texts.push(texts.join(''));
    for(const rawText of texts){
      // Decode after concatenation as an escape itself can cross streams.
      const text=rawText.replace(/#([0-9a-f]{2})/gi,(_,hex)=>String.fromCharCode(parseInt(hex,16)));
      const names=/\/MCID(?=[\x00\t\n\f\r ()<>\[\]{}\/%]|$)/g;let match;
      while((match=names.exec(text))){if(++budget.references>50000)fail('LIMIT');
        const tail=text.slice(names.lastIndex),value=/^(?:[\x00\t\n\f\r ]|%[^\r\n]*(?:\r\n?|\n|$))*([+-]?(?:\d+\.?\d*|\.\d+))(?=[\x00\t\n\f\r ()<>\[\]{}\/%]|$)/.exec(tail);
        if(!value)fail();const n=Number(value[1]);if(!Number.isSafeInteger(n)||n<0||n>50000)fail();
        // A suspicious indirect numeric operand cannot establish absence.
        if(/^(?:[\x00\t\n\f\r ]|%[^\r\n]*(?:\r\n?|\n|$))*[+-]?(?:\d|\.\d)/.test(tail.slice(value[0].length)))fail();
        ids.add(n);
      }
    }return ids;
  }
  function read(P,doc){
    const h=helpers(P,doc),{key,resolve,get,name,integer,bytes,keys,list}=h;
    const tree=get(doc.catalog,'StructTreeRoot'),pages=doc.getPages(),pageIndex=new Map(pages.map((p,i)=>[p.node,i]));
    const annotations=Annotations.read(P,doc),objectOwners=new Map();
    // MarkInfo and its Marked flag are optional (ISO 32000-1 tables 28/321).
    // Absence means false, not an invalid structure tree. Keep all tree checks
    // and report partial coverage; never label such a source fully tagged.
    const rawMark=get(doc.catalog,'MarkInfo'),mark=rawMark===undefined?doc.context.obj({}):rawMark;
    if(tree===undefined){
      if(mark instanceof P.PDFDict&&get(mark,'Marked')===P.PDFBool.True)fail();
      for(const [,o] of doc.context.enumerateIndirectObjects()){const d=o instanceof P.PDFRawStream?o.dict:o;if(d instanceof P.PDFDict&&(d.has(key('StructParents'))||d.has(key('StructParent'))))fail();}
      return null;
    }
    if(!(tree instanceof P.PDFDict))fail();
    keys(tree,['Type','K','ParentTree','ParentTreeNextKey','IDTree','RoleMap','ClassMap']);
    if(name(get(tree,'Type'))!=='StructTreeRoot')fail();
    if(!(mark instanceof P.PDFDict)||(mark.has(key('Marked'))&&!(get(mark,'Marked') instanceof P.PDFBool)))fail();
    keys(mark,['Type','Marked','Suspects','UserProperties']);
    // Some producers include a harmless dictionary discriminator. It is a name,
    // not a boolean flag; accept only this exact value, never arbitrary metadata.
    if(mark.has(key('Type'))&&name(get(mark,'Type'))!=='MarkInfo')fail();
    for(const [k,v] of mark.entries())if(k.decodeText()!=='Type'&&!(resolve(v) instanceof P.PDFBool))fail();
    const lang=get(doc.catalog,'Lang');if(lang!==undefined)bytes(lang);
    const roleMap=get(tree,'RoleMap'),classMap=get(tree,'ClassMap');
    if(roleMap!==undefined&&!(roleMap instanceof P.PDFDict))fail();
    if(classMap!==undefined&&!(classMap instanceof P.PDFDict))fail();
    function role(s){const seen=new Set();while(!standard.has(s)){if(!s||seen.has(s)||seen.size>32||!roleMap)fail('UNSUPPORTED_TAGGED');seen.add(s);s=name(get(roleMap,s));}return s;}
    const roles=new Map();
    if(roleMap)for(const [k,v] of roleMap.entries()){const n=k.decodeText();if(standard.has(n))fail('UNSUPPORTED_TAGGED');roles.set(n,role(name(v)));}
    const nodes=new Map(),ids=new Map(),owners=pages.map(()=>new Map());let steps=0,recovered=false;
    function page(v){const p=pageIndex.get(resolve(v));if(p===undefined)fail();return p;}
    function walk(value,parent,inherited,depth,detached=false){
      if(depth>128||++steps>10000)fail('LIMIT');
      const d=resolve(value);
      if(!(value instanceof P.PDFRef)||!(d instanceof P.PDFDict)||nodes.has(d))fail();
      keys(d,['Type','S','P','ID','Pg','K','A','C','R','T','Lang','Alt','E','ActualText','Ref','Info','Metadata']);
      // Type is optional for StructElem; an explicit discriminator must match.
      if(d.has(key('Type'))&&name(get(d,'Type'))!=='StructElem')fail();
      if(resolve(d.get(key('P')))!==parent){
        // Scanner-generated heading leaf: its unique forward edge, page and
        // ParentTree owner together determine the omitted back pointer. Do not
        // repair conflicting, dangling, null or arbitrary missing parents.
        const k=get(d,'K');
        if(d.keys().includes(key('P'))||name(get(d,'S'))!=='H1'||!(d.get(key('Pg')) instanceof P.PDFRef)||!(k instanceof P.PDFArray)||k.size()!==1||!(resolve(k.get(0)) instanceof P.PDFNumber))fail();
        recovered ||= !detached;
      }
      const s=name(get(d,'S'));
      // Certain spreadsheet exporters leave an empty-name, content-free leaf
      // under their explicitly role-mapped Artifact container. Keep that leaf
      // byte-for-byte in semantic fields, without inventing a role or dropping
      // the entire tag tree. This is preservation, not PDF/UA conformance repair.
      const inert=s===''&&name(get(parent,'S'))==='Artifact'&&roles.get('Artifact')==='Sect'&&
        d.entries().every(([k])=>['Type','S','P','K'].includes(k.decodeText()))&&
        get(d,'K') instanceof P.PDFArray&&get(d,'K').size()===0;
      const resolvedRole=inert?null:role(s);
      const pg=d.has(key('Pg'))?page(d.get(key('Pg'))):inherited;
      const n={d,ref:value,s,role:resolvedRole,inert,orphan:detached,pg,kids:[],pages:new Set(),id:undefined};nodes.set(d,n);
      for(const k of ['ID','T','Lang','Alt','E','ActualText'])if(d.has(key(k)))bytes(d.get(key(k)));
      if(d.has(key('ID'))){n.id=bytes(d.get(key('ID')));if(ids.has(n.id))fail();ids.set(n.id,n);}
      if(d.has(key('R')))integer(d.get(key('R')));
      for(const item of list(d.get(key('K')))){
        const child=resolve(item);
        if(child instanceof P.PDFNumber || (child instanceof P.PDFDict&&name(get(child,'Type'))==='MCR')){
          let p=pg,mcid;
          if(child instanceof P.PDFNumber)mcid=integer(child);
          else {keys(child,['Type','Pg','MCID']);if(child.has(key('Pg')))p=page(child.get(key('Pg')));mcid=integer(child.get(key('MCID')));}
          if(p===undefined||owners[p].has(mcid))fail();
          owners[p].set(mcid,n);n.kids.push({mcid,page:p});n.pages.add(p);
        }else if(child instanceof P.PDFDict&&name(get(child,'Type'))==='OBJR'){
          keys(child,['Type','Pg','Obj']);const ref=child.get(key('Obj')),object=resolve(ref),record=annotations.records.get(object);
          const p=child.has(key('Pg'))?page(child.get(key('Pg'))):pg;
          if(!(ref instanceof P.PDFRef)||!record||p!==record.page||objectOwners.has(object))fail();
          objectOwners.set(object,n);n.kids.push({obj:object,page:p});n.pages.add(p);
        }else{
          if(child instanceof P.PDFDict&&['OBJR','MCR'].includes(name(get(child,'Type'))))fail('UNSUPPORTED_TAGGED');
          const c=walk(item,d,pg,depth+1);n.kids.push(c);for(const p of c.pages)n.pages.add(p);
        }
      }
      if(!n.kids.length&&pg!==undefined)n.pages.add(pg);
      return n;
    }
    const top=list(tree.get(key('K'))).map(v=>walk(v,tree,undefined,0));
    if(!top.length)fail();
    const parent=numberTree(tree.get(key('ParentTree')),'ParentTree'),orphans=[];
    // Preserve, but do not invent a logical parent/reading order for, the exact
    // detached scanner H1 leaf indexed at MCID 0. It remains ParentTree-only in
    // the result, with Marked=false and a user-visible partial-structure notice.
    // All metadata/security checks below also apply to these retained leaves.
    for(let i=0;i<pages.length;i++){
      const index=pages[i].node.get(key('StructParents'));if(index===undefined)continue;
      const a=resolve(parent.get(integer(index)));if(!(a instanceof P.PDFArray))fail();
      const raw=a.get(0),d=resolve(raw);if(!d||d===P.PDFNull||nodes.has(d))continue;
      if(!(raw instanceof P.PDFRef)||!(d instanceof P.PDFDict)||owners[i].has(0)||d.keys().some(k=>!['Type','S','T','E','Lang','Alt','ActualText','K','Pg'].includes(k.decodeText()))||name(get(d,'S'))!=='H1'||!(d.get(key('Pg')) instanceof P.PDFRef)||page(d.get(key('Pg')))!==i)fail();
      const kids=get(d,'K');if(!(kids instanceof P.PDFArray)||kids.size()!==1||integer(kids.get(0))!==0)fail();
      orphans.push(walk(raw,tree,undefined,0,true));
    }
    // Metadata may contain nested plain values, never hidden page/tree references.
    function metadata(value,active=new Set(),depth=0){
      if(depth>64||++steps>50000)fail('LIMIT');
      const v=resolve(value);
      if(v===undefined)fail();
      if(v instanceof P.PDFDict||v instanceof P.PDFArray){
        if(active.has(v)||nodes.has(v)||pageIndex.has(v)||v===tree)fail();active.add(v);
        if(v instanceof P.PDFDict)for(const [k,x] of v.entries()){
          if(k.decodeText()==='Headers'){const a=resolve(x);if(!(a instanceof P.PDFArray))fail();for(const id of a.asArray())if(!ids.has(bytes(id)))fail();}
          metadata(x,active,depth+1);
        }else for(const x of v.asArray())metadata(x,active,depth+1);
        active.delete(v);
      }else if(!(v instanceof P.PDFName||v instanceof P.PDFString||v instanceof P.PDFHexString||v instanceof P.PDFNumber||v instanceof P.PDFBool||v===P.PDFNull))fail('UNSUPPORTED_TAGGED');
    }
    if(classMap)metadata(classMap);
    const metadataStreams=new Set();let metadataBytes=0;
    for(const n of nodes.values()){
      // Combined scans can retain each part's producer info and XMP. Preserve
      // bounded inert data verbatim; never parse XML, resolve URLs or execute it.
      if(n.d.has(key('Info'))){const info=get(n.d,'Info');keys(info,['Title','Author','Subject','Keywords','Creator','Producer','CreationDate','ModDate']);for(const [,v] of info.entries())bytes(v);}
      if(n.d.has(key('Metadata'))){const stream=get(n.d,'Metadata');
        if(!(stream instanceof P.PDFRawStream))fail('UNSUPPORTED_TAGGED');
        keys(stream.dict,['Type','Subtype','Length']);
        if(name(get(stream.dict,'Type'))!=='Metadata'||name(get(stream.dict,'Subtype'))!=='XML')fail('UNSUPPORTED_TAGGED');
        const size=stream.getContentsSize();if(size>1024*1024)fail('LIMIT');
        if(!metadataStreams.has(stream)){metadataStreams.add(stream);metadataBytes+=size;if(metadataBytes>16*1024*1024)fail('LIMIT');}
      }
      if(n.d.has(key('A')))metadata(n.d.get(key('A')));
      if(n.d.has(key('C')))for(const c of list(n.d.get(key('C')))){
        const v=resolve(c);if(v instanceof P.PDFNumber){integer(v);continue;}
        if(!(v instanceof P.PDFName)||!classMap?.has(v))fail();
      }
      for(const ref of list(n.d.get(key('Ref'))))if(!nodes.has(resolve(ref)))fail();
    }
    function numberTree(value,expectedType){const result=new Map(),seen=new Set();let last=-1;
      function visit(v,depth){v=resolve(v);if(depth>32||!(v instanceof P.PDFDict)||seen.has(v)||seen.size>10000)fail();seen.add(v);keys(v,['Type','Kids','Nums','Limits']);
        if(v.has(key('Type'))&&(!expectedType||name(get(v,'Type'))!==expectedType))fail();
        const nums=get(v,'Nums'),kids=get(v,'Kids');if((nums===undefined)===(kids===undefined))fail();
        if(nums!==undefined){if(!(nums instanceof P.PDFArray)||nums.size()%2)fail();for(let i=0;i<nums.size();i+=2){const k=integer(nums.get(i));if(k<=last)fail();last=k;result.set(k,nums.get(i+1));}}
        else {if(!(kids instanceof P.PDFArray)||!kids.size())fail();for(const child of kids.asArray())visit(child,depth+1);}
      }visit(value,0);return result;
    }
    const used=new Set(),stale=[];
    for(let i=0;i<pages.length;i++){
      const v=pages[i].node.get(key('StructParents'));
      if(v===undefined){if(owners[i].size)fail();continue;}
      const k=integer(v),a=resolve(parent.get(k));if(used.has(k)||!(a instanceof P.PDFArray))fail();used.add(k);
      for(let m=0;m<a.size();m++){const val=resolve(a.get(m)),owner=owners[i].get(m);
        if(owner){if(val!==owner.d)fail();}
        else if(val!==P.PDFNull){const n=nodes.get(val);
          if(!n||n.role!=='Figure'||!n.pages.has(i)||!n.kids.some(k=>k.page===i&&k.mcid!==undefined))fail();
          stale.push({page:i,mcid:m});
        }
      }
      for(const m of owners[i].keys())if(m>=a.size())fail();
    }
    for(const [object,owner] of objectOwners){const k=integer(object.get(key('StructParent')));if(used.has(k)||resolve(parent.get(k))!==owner.d)fail();used.add(k);}
    if(used.size!==parent.size)fail('UNSUPPORTED_TAGGED');
    if(stale.length){
      const budget={bytes:0,references:0},pageIds=new Map();
      // Named BDC property lists can conceal MCIDs outside content streams.
      // Recovery is not admitted for documents containing any /Properties
      // resource dictionary. Traverse direct children too, with cycle/budget
      // bounds; normal preservation of valid trees remains unaffected.
      const seen=new Set();let visited=0;
      function noProperties(raw,depth=0){const v=resolve(raw);if(++visited>100000||depth>128)fail('LIMIT');
        if(!v||seen.has(v))return;seen.add(v);
        if(v instanceof P.PDFRawStream)noProperties(v.dict,depth+1);
        else if(v instanceof P.PDFDict){if(v.keys().includes(key('Properties')))fail();for(const [,child] of v.entries())noProperties(child,depth+1);}
        else if(v instanceof P.PDFArray)for(const child of v.asArray())noProperties(child,depth+1);
      }
      noProperties(doc.catalog);for(const [,o] of doc.context.enumerateIndirectObjects())noProperties(o);
      const forms=doc.context.enumerateIndirectObjects().map(([,v])=>v).filter(v=>v instanceof P.PDFRawStream&&name(get(v.dict,'Subtype'))==='Form');
      const formIds=possibleMCIDs(P,doc,forms,budget);
      for(const entry of stale){if(!pageIds.has(entry.page))pageIds.set(entry.page,possibleMCIDs(P,doc,list(pages[entry.page].node.get(key('Contents'))),budget));
        if(pageIds.get(entry.page).has(entry.mcid)||formIds.has(entry.mcid))fail();
      }
      recovered=true;
    }
    for(const [,o] of doc.context.enumerateIndirectObjects()){
      const d=o instanceof P.PDFRawStream?o.dict:o;
      if(d instanceof P.PDFDict&&((d.has(key('StructParent'))&&!objectOwners.has(d))||(d.has(key('StructParents'))&&!pageIndex.has(d))))fail('UNSUPPORTED_TAGGED');
    }
    for(const object of annotations.records.keys())if(object.has(key('StructParent'))&&!objectOwners.has(object))fail();
    if(tree.has(key('IDTree'))){
      const seen=new Set(),indexed=new Map();
      function visit(v,depth=0){v=resolve(v);if(depth>32||!(v instanceof P.PDFDict)||seen.has(v)||seen.size>10000)fail();seen.add(v);keys(v,['Kids','Names','Limits']);
        const names=get(v,'Names'),kids=get(v,'Kids');if((names===undefined)===(kids===undefined))fail();
        if(names!==undefined){if(!(names instanceof P.PDFArray)||names.size()%2)fail();for(let i=0;i<names.size();i+=2){const id=bytes(names.get(i)),node=nodes.get(resolve(names.get(i+1)));if(indexed.has(id)||!node||node.id!==id)fail();indexed.set(id,node);}}
        else {if(!(kids instanceof P.PDFArray))fail();for(const child of kids.asArray())visit(child,depth+1);}
      }visit(tree.get(key('IDTree')));if(indexed.size!==ids.size)fail();
    }
    let labels=null;
    if(doc.catalog.has(key('PageLabels'))){labels=numberTree(doc.catalog.get(key('PageLabels')));for(const [i,v] of labels){if(i>=pages.length)fail();const d=resolve(v);keys(d,['Type','S','P','St']);if(d.has(key('S'))&&!['D','R','r','A','a'].includes(name(get(d,'S'))))fail();if(d.has(key('P')))bytes(d.get(key('P')));if(d.has(key('St'))&&integer(d.get(key('St')))===0)fail();}}
    const partial=get(mark,'Marked')!==P.PDFBool.True||orphans.length>0||pages.some((_,i)=>owners[i].size===0&&![...objectOwners.keys()].some(o=>annotations.records.get(o).page===i));
    return {P,doc,...h,tree,pages,mark,lang,top,orphans,partial,nodes,ids,owners,roles,classMap,labels,annotations,objectOwners,recovered};
  }
  async function copy(P,docs,models,items){
    const sources=[...new Set(items.map(x=>x.source))];
    const output=await P.PDFDocument.create(),c=output.context,key=s=>P.PDFName.of(s),tree=c.obj({Type:'StructTreeRoot'}),treeRef=c.register(tree);
    const wrapper=c.obj({Type:'StructElem',S:'Document',P:treeRef}),wrapperRef=c.register(wrapper);
    tree.set(key('K'),c.obj([wrapperRef]));const rootKids=[],parentNums=[],idEntries=[],roleMap=c.obj({}),classes=c.obj({}),labels=[];
    let parentKey=0,suspects=false,userProperties=false,typedMark=false,typedParent=false,partial=sources.some(s=>!models[s]||models[s].partial);
    const destPages=items.map(()=>c.nextRef()),finishedPages=new Array(items.length),multiple=sources.length>1;
    for(const source of sources){
      const m=models[source],selected=new Map();
      for(let i=0;i<items.length;i++)if(items[i].source===source){if(selected.has(items[i].index))fail('TAGGED_OPERATION');selected.set(items[i].index,i);}
      const positions=[...selected.values()];if(positions.at(-1)-positions[0]+1!==positions.length)fail('TAGGED_OPERATION');
      if(!m){
        // Untagged pages remain untagged: copy bytes/resources and annotation
        // edges, never fabricate semantic tags or flatten the tagged sources.
        const a=Annotations.read(P,docs[source]),copier=P.PDFObjectCopier.for(docs[source].context,c),original=copier.copy;
        copier.copy=v=>{const o=a.resolve(v);if(a.pageIndex.has(o)&&!selected.has(a.pageIndex.get(o)))fail('LINK_TARGET');return original(v);};
        for(const [index,pos] of selected)copier.traversedObjects.set(a.pages[index].ref,destPages[pos]);
        const annotations=Annotations.prepare(P,docs[source],a,output,copier,selected,destPages);
        for(const [index,pos] of selected){const node=copier.copy(a.pages[index].node);c.assign(destPages[pos],node);const pg=P.PDFPage.of(node,destPages[pos],output);pg.setRotation(P.degrees((pg.getRotation().angle+items[pos].rotation)%360));finishedPages[pos]=pg;}
        annotations.finish();continue;
      }
      const {resolve,get,bytes}=m;
      const unchanged=selected.size===m.pages.length&&[...selected.keys()].every((p,i)=>p===i);
      const kept=new Map();
      function plan(n){
        const present=[...n.pages].filter(p=>selected.has(p));
        if(n.pages.size&&!present.length)return null;
        if(!grouping.has(n.role)&&n.pages.size>1){
          const sorted=[...n.pages].sort((a,b)=>a-b),out=sorted.map(p=>selected.get(p));
          if(present.length!==n.pages.size||out.some((p,i)=>i>0&&p!==out[i-1]+1))fail('TAGGED_OPERATION');
        }
        let kids=n.kids.map(x=>x.mcid!==undefined||x.obj?(selected.has(x.page)?x:null):plan(x)).filter(Boolean);
        const indices=present.map(p=>selected.get(p)),first=indices.length?Math.min(...indices):Infinity,last=indices.length?Math.max(...indices):-1;
        if(!unchanged){
          if(!n.pages.size&&!grouping.has(n.role)&&!n.inert)fail('TAGGED_OPERATION');
          kids=kids.map((x,i)=>({x,i,first:x.mcid!==undefined||x.obj?selected.get(x.page):x.first,last:x.mcid!==undefined||x.obj?selected.get(x.page):x.last})).sort((a,b)=>a.first-b.first||a.i-b.i);
          for(let i=1;i<kids.length;i++)if(kids[i-1].last>kids[i].first)fail('TAGGED_OPERATION');
          kids=kids.map(x=>x.x);
        }
        const result={n,kids,first,last,ref:c.nextRef()};kept.set(n.d,result);return result;
      }
      let tops=m.top.map(plan).filter(Boolean);if(!unchanged){tops.sort((a,b)=>a.first-b.first);for(let i=1;i<tops.length;i++)if(tops[i-1].last>tops[i].first)fail('TAGGED_OPERATION');}
      const orphanPlans=m.orphans.map(plan).filter(Boolean);
      if(!tops.length&&!orphanPlans.length&&!m.partial)fail('TAGGED_OPERATION');
      const docNode=c.obj({Type:'StructElem',S:'Document',P:wrapperRef}),docRef=c.register(docNode);rootKids.push(docRef);
      if(m.lang)docNode.set(key('Lang'),m.lang.clone());
      const title=docs[source].getTitle();if(title)docNode.set(key('T'),P.PDFHexString.fromText(title));
      suspects ||= m.recovered||[...m.nodes.values()].some(n=>n.inert)||get(m.mark,'Suspects')===P.PDFBool.True;userProperties ||= get(m.mark,'UserProperties')===P.PDFBool.True;
      typedMark ||= m.mark.has(key('Type'));
      typedParent ||= get(m.tree,'ParentTree').has(key('Type'));
      for(const [name,role] of m.roles){const old=roleMap.get(key(name));if(old&&old.decodeText()!==role)fail('TAGGED_CONFLICT');roleMap.set(key(name),key(role));}
      const prefix=multiple?'oto'+(source+1)+'-':'',prefixHex=Array.from(prefix,ch=>ch.charCodeAt(0).toString(16).padStart(2,'0')).join('');
      const mappedId=id=>{const n=m.ids.get(id);if(!n||!kept.has(n.d))fail('TAGGED_OPERATION');return P.PDFHexString.of(prefixHex+id);};
      const copier=P.PDFObjectCopier.for(docs[source].context,c),originalCopy=copier.copy;
      // A reference into an omitted page or tag must never copy a hidden orphan.
      copier.copy=obj=>{const resolved=resolve(obj);if(m.pages.some(p=>p.node===resolved)&&!selected.has(m.pages.findIndex(p=>p.node===resolved)))fail('TAGGED_OPERATION');if(m.nodes.has(resolved)&&!kept.has(resolved))fail('TAGGED_OPERATION');return originalCopy(obj);};
      for(const [index,pos] of selected)copier.traversedObjects.set(m.pages[index].ref,destPages[pos]);
      const annotationCopy=Annotations.prepare(P,docs[source],m.annotations,output,copier,selected,destPages);
      for(const [d,p] of kept){copier.traversedObjects.set(p.n.ref,p.ref);copier.traversedObjects.set(d,c.obj({}));}
      const oldRoot=docs[source].catalog.get(key('StructTreeRoot'));copier.traversedObjects.set(oldRoot,docRef);copier.traversedObjects.set(m.tree,docNode);
      function meta(v,header=false){v=resolve(v);if(header)return mappedId(bytes(v));
        if(v instanceof P.PDFArray)return c.obj(v.asArray().map(x=>meta(x)));
        if(v instanceof P.PDFDict){const d=c.obj({});for(const [k,x] of v.entries()){if(k.decodeText()==='Headers')d.set(k,c.obj(resolve(x).asArray().map(id=>meta(id,true))));else d.set(k,meta(x));}return d;}
        return v.clone();
      }
      if(m.classMap)for(const [k,v] of m.classMap.entries())classes.set(key(prefix+k.decodeText()),meta(v));
      for(const [index,pos] of selected){
        const node=copier.copy(m.pages[index].node);c.assign(destPages[pos],node);
        const pg=P.PDFPage.of(node,destPages[pos],output);pg.setRotation(P.degrees((pg.getRotation().angle+items[pos].rotation)%360));finishedPages[pos]=pg;
        if(m.labels){let chosen,base=0;for(const [i,v] of m.labels)if(i<=index){chosen=v;base=i;}
          const d=chosen?meta(chosen):c.obj({S:'D',St:1});if(d.has(key('S')))d.set(key('St'),P.PDFNumber.of((d.lookup(key('St'),P.PDFNumber)?.asNumber()||1)+index-base));labels.push([pos,d]);}
      }
      const arrays=new Map();
      annotationCopy.finish();
      for(const [index,pos] of selected){const a=c.obj([]);arrays.set(index,a);finishedPages[pos].node.set(key('StructParents'),P.PDFNumber.of(parentKey));parentNums.push(parentKey++,a);}
      function emit(p,parent){const n=p.n,d=c.obj({Type:'StructElem',S:key(n.s)});if(parent)d.set(key('P'),parent);c.assign(p.ref,d);
        for(const k of ['R','T','Lang','Alt','E','ActualText','Info'])if(n.d.has(key(k)))d.set(key(k),meta(n.d.get(key(k))));
        if(n.d.has(key('Metadata')))d.set(key('Metadata'),copier.copy(n.d.get(key('Metadata'))));
        if(n.id!==undefined){const id=mappedId(n.id);d.set(key('ID'),id);idEntries.push([id,p.ref]);}
        if(n.d.has(key('A')))d.set(key('A'),meta(n.d.get(key('A'))));
        if(n.d.has(key('C'))){const names=m.list(n.d.get(key('C'))).map(v=>{v=resolve(v);return v instanceof P.PDFName?key(prefix+v.decodeText()):v.clone();});d.set(key('C'),c.obj(names));}
        if(n.d.has(key('Ref')))d.set(key('Ref'),c.obj(m.list(n.d.get(key('Ref'))).map(v=>{const dest=kept.get(resolve(v));if(!dest)fail('TAGGED_OPERATION');return dest.ref;})));
        if(!n.inert&&n.pg!==undefined&&selected.has(n.pg))d.set(key('Pg'),destPages[selected.get(n.pg)]);
        const children=p.kids.map(child=>{
          if(child.obj){const ref=annotationCopy.refs.get(child.obj);if(!ref)fail();annotationCopy.dicts.get(child.obj).set(key('StructParent'),P.PDFNumber.of(parentKey));parentNums.push(parentKey++,p.ref);return c.obj({Type:'OBJR',Pg:destPages[selected.get(child.page)],Obj:ref});}
          if(child.mcid===undefined){emit(child,p.ref);return child.ref;}
          const a=arrays.get(child.page);while(a.size()<=child.mcid)a.push(P.PDFNull);a.set(child.mcid,p.ref);
          return n.orphan?P.PDFNumber.of(child.mcid):c.obj({Type:'MCR',Pg:destPages[selected.get(child.page)],MCID:child.mcid});
        });d.set(key('K'),c.obj(children));
      }
      for(const p of tops)emit(p,docRef);docNode.set(key('K'),c.obj(tops.map(p=>p.ref)));
      for(const p of orphanPlans)emit(p,null);
    }
    for(const pg of finishedPages)output.addPage(pg);
    wrapper.set(key('K'),c.obj(rootKids));tree.set(key('ParentTree'),c.obj({Nums:parentNums}));tree.set(key('ParentTreeNextKey'),P.PDFNumber.of(parentKey));
    if(typedParent)tree.lookup(key('ParentTree'),P.PDFDict).set(key('Type'),key('ParentTree'));
    if(roleMap.entries().length)tree.set(key('RoleMap'),roleMap);if(classes.entries().length)tree.set(key('ClassMap'),classes);
    if(idEntries.length){idEntries.sort((a,b)=>a[0].asString().localeCompare(b[0].asString()));tree.set(key('IDTree'),c.obj({Names:idEntries.flat()}));}
    output.catalog.set(key('StructTreeRoot'),treeRef);output.catalog.set(key('MarkInfo'),c.obj({Marked:!partial,Suspects:suspects,UserProperties:userProperties}));
    if(typedMark)output.catalog.lookup(key('MarkInfo'),P.PDFDict).set(key('Type'),key('MarkInfo'));
    const langs=sources.map(s=>models[s]?.lang);if(langs.every(l=>l&&l.toString()===langs[0].toString()))output.catalog.set(key('Lang'),langs[0].clone());
    if(labels.length){for(let i=0;i<items.length;i++)if(!labels.some(([n])=>n===i))labels.push([i,c.obj({S:'D',St:i+1})]);labels.sort((a,b)=>a[0]-b[0]);output.catalog.set(key('PageLabels'),c.obj({Nums:labels.flat()}));}
    output.setTitle(sources.length===1?(docs[sources[0]].getTitle()||'Dokumen PDF'):'Dokumen gabungan');output.catalog.set(key('ViewerPreferences'),c.obj({DisplayDocTitle:true}));
    read(P,output);return output;
  }
  function decoration(P,doc,page,tool,text){
    const key=s=>P.PDFName.of(s),c=doc.context;
    if(tool==='numbers'){
      page.pushOperators(P.PDFOperator.of('BDC',[key('Artifact'),c.obj({Type:'Pagination'})]));
      // Expose the new visible number through page labels as well, not as real
      // paragraph content in the reading order. copy() emits one label per page.
      let labels=doc.catalog.lookupMaybe(key('PageLabels'),P.PDFDict);
      if(!labels){labels=c.obj({Nums:doc.getPages().flatMap((_,i)=>[i,c.obj({S:'D',St:i+1})])});doc.catalog.set(key('PageLabels'),labels);}
      const nums=labels.lookup(key('Nums'),P.PDFArray),index=doc.getPages().indexOf(page);let changed=false;
      for(let i=0;i<nums.size();i+=2)if(nums.lookup(i,P.PDFNumber).asNumber()===index){nums.set(i+1,c.obj({P:P.PDFHexString.fromText(text)}));changed=true;break;}
      if(!changed)fail();
    }
    else {
      // A meaningful watermark is real content, not silently hidden as an artifact.
      const tree=doc.catalog.lookup(key('StructTreeRoot'),P.PDFDict),parent=tree.lookup(key('ParentTree'),P.PDFDict).lookup(key('Nums'),P.PDFArray);
      const pageKey=page.node.lookup(key('StructParents'),P.PDFNumber).asNumber();let array;
      for(let i=0;i<parent.size();i+=2)if(parent.lookup(i,P.PDFNumber).asNumber()===pageKey)array=parent.lookup(i+1,P.PDFArray);
      if(!array)fail();const mcid=array.size(),wrapperRef=tree.lookup(key('K'),P.PDFArray).get(0),wrapper=c.lookup(wrapperRef,P.PDFDict);
      const note=c.obj({Type:'StructElem',S:'Note',P:wrapperRef,Pg:page.ref,K:mcid,Alt:P.PDFHexString.fromText('Tanda air: '+text)}),ref=c.register(note);
      wrapper.lookup(key('K'),P.PDFArray).push(ref);array.push(ref);
      page.pushOperators(P.PDFOperator.of('BDC',[key('Note'),c.obj({MCID:mcid})]));
    }
    return ()=>page.pushOperators(P.PDFOperator.of('EMC'));
  }
  const api={read,copy,decoration};if(typeof module!=='undefined')module.exports=api;else root.OtoPdfTags=api;
})(typeof self!=='undefined'?self:globalThis);
