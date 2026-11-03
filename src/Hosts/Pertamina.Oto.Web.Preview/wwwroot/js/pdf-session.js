/* UI admission bookkeeping only. Never trusted by the PDF execution worker.
 * Bytes and workspace state stay in this tab; no storage/network API. */
(function(root){
 'use strict';
 function usage(readers){
  const inputs=new Set();let outputBytes=0;
  for(const read of readers){const value=read();if(value.fileBacked)continue;for(const s of value.sources)inputs.add(s.bytes);outputBytes+=value.outputBytes;}
  return {files:inputs.size,inputBytes:[...inputs].reduce((n,b)=>n+b.byteLength,0),outputBytes};
 }
 function warnings(sources){
  const result=new Set(sources.flatMap(s=>s.warnings||[]));
  if(sources.some(s=>s.tagged)&&sources.some(s=>!s.tagged))result.add('PARTIAL_TAGS');
  return [...result];
 }
 function inspection(tool,sources,buffers){return {action:'inspect',tool,details:true,buffers:tool==='images-pdf'?[...sources.map(s=>s.bytes),...buffers]:buffers};}
 function additions(tool,sources,pages,result,limits){
  const counts=tool==='images-pdf'?result.counts.slice(sources.length):result.counts;
  if(!Array.isArray(counts)||counts.some(n=>!Number.isInteger(n)||n<1))throw Error('VERIFY');
  const extra=counts.reduce((n,v)=>n+v,0);
  if(sources.reduce((n,s)=>n+s.count,0)+extra>limits.pages||pages.length+extra>limits.pages)throw Error('LIMIT');
  return counts;
 }
 const api={usage,warnings,inspection,additions};
 if(typeof module!=='undefined')module.exports=api;else root.OtoPdfSession=api;
})(typeof self!=='undefined'?self:globalThis);
