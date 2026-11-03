'use strict';
// Parser diagnostics can contain document fragments. Do not log them.
console.log = console.warn = console.error = function() {};
importScripts('/js/pdf-capacity-data.js','/js/pdf-capacity.js','/vendor/pdf-lib-1.17.1/pdf-lib.js', '/js/pdf-annotations.js', '/js/pdf-tags.js', '/js/pdf-engine.js');
self.onmessage = async function(event) {
  try {
    const result = await OtoPdf.run(PDFLib,event.data,progress=>self.postMessage({progress}));
    self.postMessage({ok:true,...result},(result.results||[]).map(r=>r.bytes.buffer));
  } catch(e) {
    const code=e?.message==='OTO_LIMIT'?'LIMIT':e?.message;
    self.postMessage({ok:false,code:['LIMIT','RANGE','INVALID','ENCRYPTED','UNSUPPORTED','UNSUPPORTED_ANNOTATIONS','UNSAFE_LINK','LINK_TARGET','ANNOTATION_OPERATION','UNSUPPORTED_FORM','UNSUPPORTED_ACTION','UNSUPPORTED_ATTACHMENT','UNSUPPORTED_SIGNATURE','UNSUPPORTED_TAGGED','TAGGED_STRUCTURE','TAGGED_OPERATION','TAGGED_MIXED','TAGGED_CONFLICT','TOOL','SINGLE','MERGE','EMPTY','SPLIT','PARTITION','VERIFY','OPTIONS','LAYOUT','TEXT','IMAGE_FORMAT','IMAGE_LIMIT','IMAGE_BROWSER'].includes(code)?code:'INVALID'});
  }
  self.close();
};
