/* Bounded PDF 1.7 annotation preservation. No URI is fetched or executed here. */
(function(root) {
  'use strict';
  const fail=(code='UNSUPPORTED_ANNOTATIONS')=>{throw Error(code);};
  const common='Type Subtype Rect Contents P NM M F AP AS Border C StructParent Lang'.split(' ');
  const markup='T Popup CA CreationDate IRT Subj RT'.split(' ');
  const extra={
    Link:'A Dest H QuadPoints BS', Text:'Open Name State StateModel', Popup:'Parent Open',
    FreeText:'DA Q CL IT RD LE BS', Line:'L BS LE IC LL LLE Cap IT LLO CP CO',
    Square:'BS IC BE RD', Circle:'BS IC BE RD', Polygon:'Vertices LE BS IC BE IT',
    PolyLine:'Vertices LE BS IC IT', Highlight:'QuadPoints', Underline:'QuadPoints',
    Squiggly:'QuadPoints', StrikeOut:'QuadPoints', Stamp:'Name', Caret:'RD Sy', Ink:'InkList BS'
  };
  function read(P,doc) {
    const key=s=>P.PDFName.of(s),pages=doc.getPages(),pageIndex=new Map(pages.map((p,i)=>[p.node,i]));
    function resolve(v){const seen=new Set();while(v instanceof P.PDFRef){if(seen.has(v)||seen.size>=32)fail('INVALID');seen.add(v);v=doc.context.lookup(v);if(v===undefined)fail('INVALID');}return v;}
    const get=(d,k)=>resolve(d.get(key(k)));
    const name=v=>{v=resolve(v);return v instanceof P.PDFName?v.decodeText():undefined;};
    function keys(d,allowed){if(!(d instanceof P.PDFDict))fail();for(const [k] of d.entries())if(!allowed.includes(k.decodeText()))fail();}
    function text(v,max=65536){v=resolve(v);if(!(v instanceof P.PDFString||v instanceof P.PDFHexString)||v.asBytes().length>max)fail();return v.decodeText();}
    function num(v,min=-1e7,max=1e7){v=resolve(v);if(!(v instanceof P.PDFNumber)||!Number.isFinite(v.asNumber())||v.asNumber()<min||v.asNumber()>max)fail();return v.asNumber();}
    function numbers(v,min,max,multiple=1){v=resolve(v);if(!(v instanceof P.PDFArray)||v.size()<min||v.size()>max||v.size()%multiple)fail();v.asArray().forEach(x=>num(x));return v;}
    function safeUri(v){
      const s=text(v,4096);
      // Preserve relative spreadsheet references as opaque data, never paths to
      // resolve or launch. No encoding, scheme, authority, macros or executable
      // suffix. Leading ../ is retained literally (not accessed by this app).
      if(!s.includes(':')&&/\.xlsx$/i.test(s)){
        if(!/^[\x20-\x7e]+$/.test(s)||/[\\<>"`|%?#:]/.test(s)||s.startsWith('/'))fail('UNSAFE_LINK');
        const parts=s.split('/');if(parts.length>24||parts.filter(p=>p==='..').length>8)fail('UNSAFE_LINK');
        let named=false;for(const p of parts){if(p==='..'&&!named)continue;named=true;
          if(!p||p==='.'||p==='..'||p.length>255||/[. ]$/.test(p)||/^(?:CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)/i.test(p))fail('UNSAFE_LINK');
        }
        return 'relative';
      }
      if(!/^[\x21-\x7e]+$/.test(s)||/[\\<>"`]/.test(s)||/%(?:0[0-9a-f]|1[0-9a-f]|7f)/i.test(s))fail('UNSAFE_LINK');
      if(/^mailto:[A-Za-z0-9.!#$&'*+\/=^_`{|}~-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$/i.test(s))return;
      if(!/^https?:\/\//i.test(s))fail('UNSAFE_LINK');
      let u;try{u=new URL(s);}catch{fail('UNSAFE_LINK');}
      if(!u.hostname||u.username||u.password||!['http:','https:'].includes(u.protocol))fail('UNSAFE_LINK');
    }
    // Resolve a named destination to an explicit target. Do not copy a catalog's
    // unrelated name trees or accidentally pull omitted pages into the output.
    let named;
    function namedDest(v){
      const id=v instanceof P.PDFName?v.decodeText():text(v,2048);
      if(!named){
        named=new Map();const legacy=get(doc.catalog,'Dests');
        if(legacy!==undefined){if(!(legacy instanceof P.PDFDict))fail();for(const [k,x] of legacy.entries())named.set(k.decodeText(),x);}
        const names=get(doc.catalog,'Names'),tree=names instanceof P.PDFDict?get(names,'Dests'):undefined;
        if(tree!==undefined){const seen=new Set();let entries=0;
          function visit(v,depth){v=resolve(v);if(depth>32||seen.has(v)||seen.size>=10000)fail();seen.add(v);keys(v,['Kids','Names','Limits']);
            const ns=get(v,'Names'),kids=get(v,'Kids');if((ns===undefined)===(kids===undefined))fail();
            if(ns!==undefined){if(!(ns instanceof P.PDFArray)||ns.size()%2)fail();for(let i=0;i<ns.size();i+=2){const s=text(ns.get(i),2048);if(named.has(s)||++entries>10000)fail();named.set(s,ns.get(i+1));}}
            else {if(!(kids instanceof P.PDFArray)||!kids.size())fail();kids.asArray().forEach(x=>visit(x,depth+1));}
          }visit(tree,0);
        }
      }
      if(!named.has(id))fail('LINK_TARGET');return resolve(named.get(id));
    }
    function destination(v){
      v=resolve(v);if(v instanceof P.PDFName||v instanceof P.PDFString||v instanceof P.PDFHexString)v=namedDest(v);
      if(v instanceof P.PDFDict){keys(v,['D']);v=get(v,'D');}
      if(!(v instanceof P.PDFArray)||v.size()<2)fail('LINK_TARGET');
      const page=pageIndex.get(resolve(v.get(0))),mode=name(v.get(1)),length={XYZ:5,Fit:2,FitH:3,FitV:3,FitR:6,FitB:2,FitBH:3,FitBV:3}[mode];
      if(page===undefined||!length||v.size()!==length)fail('LINK_TARGET');
      for(let i=2;i<v.size();i++){const x=resolve(v.get(i));if(x!==P.PDFNull||mode==='FitR')num(x);}
      return {page,tail:v.asArray().slice(1)};
    }
    const records=new Map(),byPage=pages.map(()=>[]),actions=new Set(),arrays=new Set();let relativeLinks=0;
    for(let i=0;i<pages.length;i++){
      const a=get(pages[i].node,'Annots');if(a===undefined||a===P.PDFNull)continue;
      if(!(a instanceof P.PDFArray))fail();arrays.add(a);
      for(const ref of a.asArray()){
        const d=resolve(ref);if(!(d instanceof P.PDFDict)||records.has(d))fail();
        if(records.size>=2000)fail('LIMIT');
        const type=name(get(d,'Subtype'));if(!Object.hasOwn(extra,type))fail();
        keys(d,[...common,...(!['Link','Popup'].includes(type)?markup:[]),...extra[type].split(' ')]);
        const required={FreeText:'DA',Line:'L',Polygon:'Vertices',PolyLine:'Vertices',Highlight:'QuadPoints',Underline:'QuadPoints',Squiggly:'QuadPoints',StrikeOut:'QuadPoints',Ink:'InkList'}[type];
        if(required&&!d.has(key(required)))fail();
        if(d.has(key('Type'))&&name(get(d,'Type'))!=='Annot')fail();
        const rect=numbers(get(d,'Rect'),4,4);if(num(rect.get(2))<num(rect.get(0))||num(rect.get(3))<num(rect.get(1)))fail();
        if(d.has(key('P'))&&resolve(d.get(key('P')))!==pages[i].node)fail();
        for(const s of ['Contents','NM','M','Lang','T','CreationDate','Subj','DA','State','StateModel'])if(d.has(key(s)))text(d.get(key(s)));
        for(const n of ['AS','Name','H','IT','CP','RT','Sy'])if(d.has(key(n))&&!name(d.get(key(n))))fail();
        for(const b of ['Open','Cap'])if(d.has(key(b))&&!(get(d,b) instanceof P.PDFBool))fail();
        for(const n of ['F','StructParent','Q'])if(d.has(key(n))&&!Number.isSafeInteger(num(get(d,n),0,2147483647)))fail();
        if(d.has(key('CA')))num(get(d,'CA'),0,1);
        for(const n of ['LL','LLE','LLO'])if(d.has(key(n)))num(get(d,n));
        for(const n of ['C','IC'])if(d.has(key(n))){const a=numbers(get(d,n),0,4);if(![0,1,3,4].includes(a.size()))fail();a.asArray().forEach(x=>num(x,0,1));}
        for(const n of ['Rect','RD','L'])if(d.has(key(n)))numbers(get(d,n),4,4);
        if(d.has(key('CO')))numbers(get(d,'CO'),2,2);
        if(d.has(key('CL'))){const a=numbers(get(d,'CL'),4,6,2);if(![4,6].includes(a.size()))fail();}
        if(d.has(key('QuadPoints')))numbers(get(d,'QuadPoints'),8,16000,8);
        if(d.has(key('Vertices')))numbers(get(d,'Vertices'),type==='Polygon'?6:4,16000,2);
        if(d.has(key('InkList'))){const a=get(d,'InkList');if(!(a instanceof P.PDFArray)||!a.size()||a.size()>1000)fail();a.asArray().forEach(x=>numbers(x,2,16000,2));}
        if(d.has(key('Border'))){const a=get(d,'Border');if(!(a instanceof P.PDFArray)||![3,4].includes(a.size()))fail();for(let j=0;j<3;j++)num(a.get(j),0);if(a.size()===4)numbers(a.get(3),0,100);}
        if(d.has(key('BS'))){const bs=get(d,'BS');keys(bs,['Type','W','S','D']);if(bs.has(key('Type'))&&name(get(bs,'Type'))!=='Border')fail();if(bs.has(key('W')))num(get(bs,'W'),0);if(bs.has(key('S'))&&!['S','D','B','I','U'].includes(name(get(bs,'S'))))fail();if(bs.has(key('D')))numbers(get(bs,'D'),0,100);}
        if(d.has(key('BE'))){const be=get(d,'BE');keys(be,['S','I']);if(be.has(key('S'))&&!['S','C'].includes(name(get(be,'S'))))fail();if(be.has(key('I')))num(get(be,'I'),0,2);}
        if(d.has(key('LE'))){const v=get(d,'LE'),names=v instanceof P.PDFArray?v.asArray():[v];if(v instanceof P.PDFArray&&v.size()!==2)fail();for(const x of names)if(!['Square','Circle','Diamond','OpenArrow','ClosedArrow','None','Butt','ROpenArrow','RClosedArrow','Slash'].includes(name(x)))fail();}
        if(d.has(key('AP'))){const ap=get(d,'AP');keys(ap,['N','R','D']);if(!ap.has(key('N')))fail();
          function stream(x){x=resolve(x);if(!(x instanceof P.PDFRawStream)||name(get(x.dict,'Subtype'))!=='Form')fail();if(x.dict.has(key('F')))fail('UNSUPPORTED_ACTION');numbers(get(x.dict,'BBox'),4,4);}
          for(const [,value] of ap.entries()){const v=resolve(value);if(v instanceof P.PDFRawStream)stream(v);else {if(!(v instanceof P.PDFDict)||!v.entries().length)fail();v.entries().forEach(([,x])=>stream(x));}}
        }
        const r={d,ref,page:i,type,dest:null,action:null};records.set(d,r);byPage[i].push(r);
        if(type==='Link'){
          if(d.has(key('A'))&&d.has(key('Dest')))fail();
          if(d.has(key('Dest')))r.dest=destination(d.get(key('Dest')));
          if(d.has(key('A'))){const a=get(d,'A'),kind=a instanceof P.PDFDict?name(get(a,'S')):undefined;
            if(!['URI','GoTo'].includes(kind))fail('UNSUPPORTED_ACTION');
            keys(a,kind==='URI'?['Type','S','URI','IsMap']:['Type','S','D']);
            if(a.has(key('Type'))&&name(get(a,'Type'))!=='Action')fail('UNSUPPORTED_ACTION');
            if(kind==='URI'){if(safeUri(get(a,'URI'))==='relative')relativeLinks++;if(a.has(key('IsMap'))&&get(a,'IsMap')!==P.PDFBool.False)fail('UNSAFE_LINK');}
            else r.dest=destination(a.get(key('D')));
            r.action=a;actions.add(a);
          }
        }
      }
    }
    // Popup/reply edges must remain on their owning page. Cyclic reply threads,
    // detached popups, or foreign objects are not copied speculatively.
    for(const r of records.values()){
      for(const k of ['Popup','Parent','IRT'])if(r.d.has(key(k))){const target=records.get(get(r.d,k));if(!target||target===r||target.page!==r.page)fail();
        if(k==='Popup'&&(target.type!=='Popup'||get(target.d,'Parent')!==r.d))fail();
        if(k==='Parent'&&(r.type!=='Popup'||get(target.d,'Popup')!==r.d))fail();
        if(k==='IRT'&&['Link','Popup'].includes(target.type))fail();
      }
      if(r.type==='Popup'&&!r.d.has(key('Parent')))fail();
      let current=r;const seen=new Set();while(current.d.has(key('IRT'))){if(seen.has(current))fail();seen.add(current);current=records.get(get(current.d,'IRT'));}
    }
    // Catalog opening views and bookmark GoTo actions are inert internal
    // navigation. Admit only owned, validated destinations; never execute them
    // or admit action chains, scripts, external actions or detached dictionaries.
    const navigationActions=new Set();let outlineCount=0;
    function goTo(raw){
      const a=resolve(raw);
      if(!(a instanceof P.PDFDict)||name(get(a,'S'))!=='GoTo')fail('UNSUPPORTED_ACTION');
      for(const [k] of a.entries())if(!['Type','S','D'].includes(k.decodeText()))fail('UNSUPPORTED_ACTION');
      if(a.has(key('Type'))&&name(get(a,'Type'))!=='Action')fail('UNSUPPORTED_ACTION');
      destination(a.get(key('D')));navigationActions.add(a);
    }
    const opening=get(doc.catalog,'OpenAction');
    if(opening!==undefined){if(opening instanceof P.PDFArray)destination(opening);else goTo(opening);}
    const outlines=get(doc.catalog,'Outlines');
    if(outlines!==undefined){
      const seen=new Set();
      if(!(outlines instanceof P.PDFDict))fail('UNSUPPORTED_ACTION');
      if(outlines.has(key('Type'))&&name(get(outlines,'Type'))!=='Outlines')fail('UNSUPPORTED_ACTION');
      function children(parent,depth){
        if(depth>32)fail('LIMIT');
        let current=get(parent,'First'),previous;
        const last=get(parent,'Last');
        if((current===undefined)!==(last===undefined))fail('UNSUPPORTED_ACTION');
        while(current!==undefined){
          if(!(current instanceof P.PDFDict)||seen.has(current)||get(current,'Parent')!==parent||get(current,'Prev')!==previous)fail('UNSUPPORTED_ACTION');
          seen.add(current);if(++outlineCount>10000)fail('LIMIT');
          text(get(current,'Title'));
          if(current.has(key('A'))&&current.has(key('Dest')))fail('UNSUPPORTED_ACTION');
          if(current.has(key('A')))goTo(current.get(key('A')));
          if(current.has(key('Dest')))destination(current.get(key('Dest')));
          children(current,depth+1);previous=current;current=get(current,'Next');
        }
        if(previous!==last)fail('UNSUPPORTED_ACTION');
      }
      children(outlines,0);
    }
    return {records,byPage,actions,arrays,resolve,get,pages,pageIndex,key,relativeLinks,navigationActions,sourceNavigation:opening!==undefined||outlineCount>0};
  }
  function prepare(P,source,m,output,copier,selected,destPages) {
    const c=output.context,key=m.key,refs=new Map(),dicts=new Map();
    for(const [index] of selected)for(const r of m.byPage[index]){
      if(r.dest&&!selected.has(r.dest.page))fail('LINK_TARGET');
      const d=c.obj({}),ref=c.register(d);refs.set(r.d,ref);dicts.set(r.d,d);
      copier.traversedObjects.set(r.d,d);if(r.ref instanceof P.PDFRef)copier.traversedObjects.set(r.ref,ref);
    }
    function dest(value){return c.obj([destPages[selected.get(value.page)],...value.tail.map(x=>copier.copy(x))]);}
    function finish(){for(const [d,ref] of refs){const r=m.records.get(d),target=dicts.get(d);
      for(const [k,v] of d.entries()){
        const name=k.decodeText();if(['P','A','Dest','Popup','Parent','IRT','StructParent'].includes(name))continue;
        target.set(k,copier.copy(v));
      }
      target.set(key('P'),destPages[selected.get(r.page)]);
      for(const name of ['Popup','Parent','IRT'])if(d.has(key(name)))target.set(key(name),refs.get(m.get(d,name)));
      if(d.has(key('Dest')))target.set(key('Dest'),dest(r.dest));
      if(r.action){const a=c.obj({});for(const [k,v] of r.action.entries())a.set(k,k.decodeText()==='D'?dest(r.dest):copier.copy(v));target.set(key('A'),a);}
    }}
    return {refs,dicts,finish};
  }
  async function copy(P,docs,models,items){
    const output=await P.PDFDocument.create(),c=output.context,destPages=items.map(()=>c.nextRef()),finished=new Array(items.length);
    for(const source of new Set(items.map(p=>p.source))){
      const m=models[source],selected=new Map();
      for(let pos=0;pos<items.length;pos++)if(items[pos].source===source){if(selected.has(items[pos].index))fail('ANNOTATION_OPERATION');selected.set(items[pos].index,pos);}
      const copier=P.PDFObjectCopier.for(docs[source].context,c),copy=copier.copy;
      copier.copy=v=>{const x=m.resolve(v);if(m.pageIndex.has(x)&&!selected.has(m.pageIndex.get(x)))fail('LINK_TARGET');return copy(v);};
      for(const [index,pos] of selected)copier.traversedObjects.set(m.pages[index].ref,destPages[pos]);
      const annotations=prepare(P,docs[source],m,output,copier,selected,destPages);
      for(const [index,pos] of selected){const node=copier.copy(m.pages[index].node);c.assign(destPages[pos],node);const page=P.PDFPage.of(node,destPages[pos],output);page.setRotation(P.degrees((page.getRotation().angle+items[pos].rotation)%360));finished[pos]=page;}
      annotations.finish();
    }
    for(const page of finished)output.addPage(page);return output;
  }
  const api={read,prepare,copy};if(typeof module!=='undefined')module.exports=api;else root.OtoPdfAnnotations=api;
})(typeof self!=='undefined'?self:globalThis);
