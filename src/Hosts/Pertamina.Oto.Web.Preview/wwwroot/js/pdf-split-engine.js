/* Split compatibility API over the shared file-backed engine. */
(function(root){
 'use strict';const local=typeof module!=='undefined',C=local?require('./pdf-capacity.js'):root.OtoPdfCapacity,F=local?require('./pdf-file-engine.js'):root.OtoFileEngine;
 const api={LIMITS:C.split,admission:files=>F.admission(files,'split'),groups:F.splitGroups,run:(P,E,request,io)=>{if(request.tool!=='split')return Promise.reject(Error('TOOL'));return F.run(P,E,request,io);}};
 if(local)module.exports=api;else root.OtoSplitEngine=api;
})(typeof self!=='undefined'?self:globalThis);
