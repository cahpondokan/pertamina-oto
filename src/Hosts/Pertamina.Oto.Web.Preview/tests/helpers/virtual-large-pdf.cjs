'use strict';
function virtualPdf(size){
 const parts=[],offsets=[0];let offset=0;
 const add=s=>{const bytes=Buffer.from(s);parts.push({offset,bytes,length:bytes.length});offset+=bytes.length;};
 const object=(n,s)=>{offsets[n]=offset;add(n+' 0 obj\n'+s+'\nendobj\n');};
 add('%PDF-1.7\n');object(1,'<< /Type /Catalog /Pages 2 0 R >>');object(2,'<< /Type /Pages /Kids [3 0 R 4 0 R] /Count 2 >>');
 object(3,'<< /Type /Page /Parent 2 0 R /MediaBox [0 0 200 400] /Resources << >> /Contents 5 0 R >>');
 object(4,'<< /Type /Page /Parent 2 0 R /MediaBox [0 0 300 400] /Resources << >> >>');
 const payload=size-2*1048576;offsets[5]=offset;add('5 0 obj\n<< /Length '+payload+' >>\nstream\n');parts.push({offset,length:payload,fill:32});offset+=payload;add('\nendstream\nendobj\n');
 // Unreferenced stream makes exact input boundary independent of output metadata overhead.
 const slack=size-offset-1024;offsets[6]=offset;add('6 0 obj\n<< /Length '+slack+' >>\nstream\n');parts.push({offset,length:slack,fill:32});offset+=slack;add('\nendstream\nendobj\n');
 const xref=offset;add('xref\n0 7\n0000000000 65535 f \n'+offsets.slice(1).map(n=>String(n).padStart(10,'0')+' 00000 n \n').join('')+'trailer\n<< /Size 7 /Root 1 0 R >>\nstartxref\n'+xref+'\n%%EOF\n');
 parts.push({offset,length:size-offset,fill:32});return {size,parts};
}
function virtualRead(f,start,end){const b=Buffer.alloc(end-start);for(const p of f.parts){const a=Math.max(p.offset,start),z=Math.min(p.offset+p.length,end);if(z>a){if(p.bytes)p.bytes.copy(b,a-start,a-p.offset,z-p.offset);else b.fill(p.fill,a-start,z-start);}}return b;}
module.exports={virtualPdf,virtualRead};
