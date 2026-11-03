/* One capacity contract for worker admission, input UI and messages.
 * The static envelope is generated at build time from pdf-capacity.json.
 * No document, network or storage access. Unsupported tools fail closed. */
(function(root) {
  'use strict';
  const data=typeof module==='object'&&module.exports?require('./pdf-capacity.json'):root.OtoPdfCapacityData;
  if(!data||data.version!==1)throw Error('CAPACITY_CONFIG');
  function freeze(value){if(value&&typeof value==='object'){Object.values(value).forEach(freeze);Object.freeze(value);}return value;}
  freeze(data);
  function input(tool) {
    if(!Object.hasOwn(data.tools,tool))throw Error('TOOL');
    const profile=data.profiles[data.tools[tool]],budget=data[profile.budget];
    return Object.freeze({...budget,files:profile.multiple?budget.files:1,totalBytes:profile.multiple?budget.totalBytes:budget.fileBytes,
      minimum:profile.minimum,kind:profile.kind,multiple:profile.multiple});
  }
  const number=value=>value.toLocaleString('id-ID');
  function bytes(value){return value>=1000000000&&value%1000000000===0?`${value/1000000000} GB`:
    value%1048576===0?`${value/1048576} MiB`:`${number(value)} byte`;}
  const api=Object.freeze({bounded:data.bounded,merge:data.merge,split:data.split,local:data.local,images:data.images,tools:Object.freeze(Object.keys(data.tools)),
    splitMinimumParts:data.splitMinimumParts,input,bytes,number});
  if(typeof module==='object'&&module.exports)module.exports=api;else root.OtoPdfCapacity=api;
})(typeof globalThis!=='undefined'?globalThis:this);
