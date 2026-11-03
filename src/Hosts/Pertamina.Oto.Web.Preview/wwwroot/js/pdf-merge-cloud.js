/* Optional selected-file import. No document upload, token persistence, or OTO
 * processing endpoint. Provider execution requires separately admitted config. */
(function(root){
 'use strict';
 const fail=c=>{throw Error(c);};
 const aborted=signal=>{if(signal?.aborted)fail('CANCELLED');};
 function selected(provider,items,remainingBytes,remainingFiles){
  if(!Array.isArray(items)||!items.length)fail('CANCELLED');if(items.length>remainingFiles)fail('FILE_COUNT');
  let total=0;
  return items.map(item=>{
   const name=String(item.name||'');const size=Number(item.size??item.bytes);
   if(!name.toLowerCase().endsWith('.pdf')||!Number.isSafeInteger(size)||size<8)fail('TYPE');
   total+=size;if(total>remainingBytes)fail('TOTAL_BYTES');
   if(provider==='google'){if(!/^[a-zA-Z0-9_-]+$/.test(item.id||''))fail('CLOUD_RESPONSE');return {name,size,url:'https://www.googleapis.com/drive/v3/files/'+item.id+'?alt=media'};}
   const url=new URL(item.link);if(url.protocol!=='https:'||url.username||url.password||!(url.hostname==='dl.dropboxusercontent.com'||url.hostname.endsWith('.dl.dropboxusercontent.com')))fail('CLOUD_RESPONSE');
   return {name,size,url:url.href};
  });
 }
 async function download(items,{fetcher=fetch,createSink,signal,progress=()=>{},token='',maxBytes}){
  let aggregate=0;const files=[];
  for(let i=0;i<items.length;i++){
   aborted(signal);const item=items[i];let reader,sink,completed=false;
   try{
    const response=await fetcher(item.url,{method:'GET',headers:token?{Authorization:'Bearer '+token}:{},credentials:'omit',referrerPolicy:'no-referrer',cache:'no-store',redirect:'error',signal});
    if(!response.ok||!response.body)fail('CLOUD_DOWNLOAD');
    const declared=response.headers.get('content-length');if(declared&&Number(declared)!==item.size)fail('CLOUD_CHANGED');
    sink=await createSink(i);reader=response.body.getReader();let size=0,head=[];
    while(true){aborted(signal);const {done,value}=await reader.read();if(done)break;size+=value.length;aggregate+=value.length;if(size>item.size||aggregate>maxBytes)fail('TOTAL_BYTES');
     if(head.length<5){head.push(...value.subarray(0,5-head.length));if(head.length===5&&String.fromCharCode(...head)!=='%PDF-')fail('INVALID');}
     await sink.write(value);progress({stage:'download',done:aggregate,total:items.reduce((n,f)=>n+f.size,0)});
    }
    if(size!==item.size||head.length<5)fail('CLOUD_CHANGED');await sink.close();completed=true;files.push(await sink.file(item.name));
   }finally{try{await reader?.cancel();}catch{}if(!completed)try{await sink?.abort();}catch{}}
  }return files;
 }
 function script(src,id,attributes={}){return new Promise((resolve,reject)=>{const old=document.getElementById(id);if(old?.dataset.loaded==='true')return resolve();old?.remove();const e=document.createElement('script');e.src=src;e.id=id;Object.entries(attributes).forEach(([k,v])=>e.setAttribute(k,v));const timer=setTimeout(()=>{e.remove();reject(Error('CLOUD_LOAD'));},20000);e.onload=()=>{clearTimeout(timer);e.dataset.loaded='true';resolve();};e.onerror=()=>{clearTimeout(timer);e.remove();reject(Error('CLOUD_LOAD'));};document.head.append(e);});}
 async function choose(provider,config,signal){
  if(config.admitted!=='true'||!config.key||provider==='google'&&(!config.client||!config.project))fail('CLOUD_CONFIG');
  aborted(signal);
  if(provider==='dropbox'){
   await script('https://www.dropbox.com/static/api/2/dropins.js','dropboxjs',{'data-app-key':config.key});aborted(signal);
   return new Promise((resolve,reject)=>{let settled=false;const finish=(error,items)=>{if(settled)return;settled=true;signal.removeEventListener('abort',cancel);error?reject(Error(error)):resolve({items,token:'',dispose:()=>{}});};const cancel=()=>finish('CANCELLED');signal.addEventListener('abort',cancel,{once:true});
    Dropbox.choose({linkType:'direct',multiselect:true,extensions:['.pdf'],folderselect:false,sizeLimit:4000000000,success:items=>finish(null,items),cancel});
   });
  }
  await Promise.all([script('https://apis.google.com/js/api.js','oto-google-picker'),script('https://accounts.google.com/gsi/client','oto-google-identity')]);aborted(signal);
  await new Promise((resolve,reject)=>gapi.load('picker',{callback:resolve,onerror:()=>reject(Error('CLOUD_LOAD')),timeout:20000,ontimeout:()=>reject(Error('CLOUD_LOAD'))}));aborted(signal);
  return new Promise((resolve,reject)=>{
   let token='',picker,client,settled=false;
   const dispose=()=>{picker?.dispose();picker=null;if(client)client.callback=()=>{};client=null;token='';signal.removeEventListener('abort',cancel);};
   const cancel=()=>{if(!settled){settled=true;dispose();reject(Error('CANCELLED'));}};signal.addEventListener('abort',cancel,{once:true});
   client=google.accounts.oauth2.initTokenClient({client_id:config.client,scope:'https://www.googleapis.com/auth/drive.file',include_granted_scopes:false,error_callback:()=>{if(!settled){settled=true;dispose();reject(Error('CLOUD_AUTH'));}},callback:response=>{
    if(settled)return;if(response.error||!response.access_token){settled=true;dispose();reject(Error('CLOUD_AUTH'));return;}token=response.access_token;
    picker=new google.picker.PickerBuilder().addView(new google.picker.DocsView().setMimeTypes('application/pdf').setIncludeFolders(false)).enableFeature(google.picker.Feature.MULTISELECT_ENABLED).setOAuthToken(token).setDeveloperKey(config.key).setAppId(config.project).setOrigin(location.origin).setCallback(data=>{
     if(data.action===google.picker.Action.CANCEL){cancel();return;}
     if(data.action===google.picker.Action.PICKED&&!settled){settled=true;picker.setVisible(false);resolve({items:data.docs.map(d=>({id:d.id,name:d.name,size:Number(d.sizeBytes)})),token,dispose});}
    }).build();picker.setVisible(true);
   }});client.requestAccessToken({prompt:'consent'});
  });
 }
 const api={selected,download,choose};if(typeof module!=='undefined')module.exports=api;else root.OtoMergeCloud=api;
})(typeof window!=='undefined'?window:globalThis);
