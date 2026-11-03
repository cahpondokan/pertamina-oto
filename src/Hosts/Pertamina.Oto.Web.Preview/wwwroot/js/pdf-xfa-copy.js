/* Bounded static XFA paint adapter. Uses PDF.js's parsed layout, never raw XML,
 * scripting, links or network images. Unsupported effects fail explicitly. */
(function(root){'use strict';
 const fail=()=>{throw Error('COPY_XFA');};
 async function render({page,pdfjs,canvas,check}){
  const html=await page.getXfa();check();if(!html)fail();
  const urls=[],host=document.createElement('div');host.className='oto-xfa-copy';host.setAttribute('aria-hidden','true');host.inert=true;
  let nodes=0;
  function safe(node){if(++nodes>20000)fail();if(!node)return node;const copy={...node};if(node.attributes){copy.attributes={...node.attributes};const a=copy.attributes;
    delete a.href;delete a.newWindow;
    if(a.src){if(typeof a.src!=='string'||!/^data:image\/(png|jpeg);base64,/i.test(a.src))fail();const [head,data]=a.src.split(',');if(data.length>24*1024*1024)fail();const decoded=atob(data),bytes=Uint8Array.from(decoded,c=>c.charCodeAt(0));const url=URL.createObjectURL(new Blob([bytes],{type:head.includes('png')?'image/png':'image/jpeg'}));urls.push(url);a.src=url;}
    if(a.style){a.style={...a.style};for(const v of Object.values(a.style))if(/url\s*\(|expression\s*\(/i.test(String(v)))fail();}
   }if(node.children)copy.children=node.children.map(safe);return copy;}
  try{
   const safeHtml=safe(html),layer=document.createElement('div');host.append(layer);host.style.width=html.attributes?.style?.width||page.getViewport({scale:1}).width+'px';host.style.height=html.attributes?.style?.height||page.getViewport({scale:1}).height+'px';document.body.append(host);
   pdfjs.XfaLayer.render({div:layer,xfaHtml:safeHtml,intent:'print',linkService:{addLinkAttributes(){}}});
   await Promise.all(Array.from(host.querySelectorAll('img'),image=>image.decode()));check();
   const bounds=layer.firstElementChild.getBoundingClientRect();if(!bounds.width||!bounds.height)fail();
   const ctx=canvas.getContext('2d'),sx=canvas.width/bounds.width,sy=canvas.height/bounds.height;ctx.save();ctx.scale(sx,sy);
   let characters=0;
   const rect=e=>{const r=e.getBoundingClientRect();return {x:r.x-bounds.x,y:r.y-bounds.y,width:r.width,height:r.height};};
   function font(style){return `${style.fontStyle} ${style.fontWeight} ${style.fontSize} ${style.fontFamily}`;}
   function textNode(node){if(!node.textContent.trim())return;const parent=node.parentElement,style=getComputedStyle(parent);if(style.visibility==='hidden'||style.display==='none')return;
    ctx.font=font(style);ctx.fillStyle=style.color;ctx.textBaseline='alphabetic';const range=document.createRange();const chars=Array.from(node.textContent);let offset=0;
    for(const char of chars){if(++characters>100000)fail();range.setStart(node,offset);offset+=char.length;range.setEnd(node,offset);const boxes=range.getClientRects();if(!boxes.length||!char.trim())continue;const r=boxes[0],metric=ctx.measureText(char),ascent=metric.fontBoundingBoxAscent??parseFloat(style.fontSize)*.8,descent=metric.fontBoundingBoxDescent??parseFloat(style.fontSize)*.2;ctx.fillText(char,r.x-bounds.x,r.y-bounds.y+(r.height-ascent-descent)/2+ascent);}
   }
   async function draw(e){check();if(e.nodeType===Node.TEXT_NODE){textNode(e);return;}if(e.nodeType!==Node.ELEMENT_NODE)return;
    const style=getComputedStyle(e);if(style.display==='none'||style.visibility==='hidden')return;const r=rect(e);
    if(style.filter!=='none'||style.transform!=='none'&&!/^matrix\(1, 0, 0, 1,/.test(style.transform))fail();
    ctx.save();ctx.globalAlpha*=Number(style.opacity)||0;
    if(style.backgroundColor!=='rgba(0, 0, 0, 0)'&&style.backgroundColor!=='transparent'){ctx.fillStyle=style.backgroundColor;ctx.fillRect(r.x,r.y,r.width,r.height);}
    for(const [side,x1,y1,x2,y2]of [['Top',r.x,r.y,r.x+r.width,r.y],['Right',r.x+r.width,r.y,r.x+r.width,r.y+r.height],['Bottom',r.x,r.y+r.height,r.x+r.width,r.y+r.height],['Left',r.x,r.y,r.x,r.y+r.height]]){
     const width=parseFloat(style['border'+side+'Width']);if(width&&style['border'+side+'Style']!=='none'){ctx.strokeStyle=style['border'+side+'Color'];ctx.lineWidth=width;ctx.beginPath();ctx.moveTo(x1,y1);ctx.lineTo(x2,y2);ctx.stroke();}
    }
    if(e.tagName==='IMG')ctx.drawImage(e,r.x,r.y,r.width,r.height);
    else if(e.tagName.toLowerCase()==='svg'){
     const source=new XMLSerializer().serializeToString(e);if(/(?:href|src)=/i.test(source))fail();const url=URL.createObjectURL(new Blob([source],{type:'image/svg+xml'}));urls.push(url);const image=new Image();image.src=url;await image.decode();ctx.drawImage(image,r.x,r.y,r.width,r.height);
    }else if(['INPUT','TEXTAREA','SELECT'].includes(e.tagName)){
     if(['checkbox','radio'].includes(e.type)){const s=Math.min(r.width,r.height,14);ctx.strokeStyle=style.color;ctx.lineWidth=1;ctx.strokeRect(r.x,r.y,s,s);if(e.checked){ctx.beginPath();ctx.moveTo(r.x+2,r.y+s/2);ctx.lineTo(r.x+s/2,r.y+s-2);ctx.lineTo(r.x+s-2,r.y+2);ctx.stroke();}}
     else{ctx.font=font(style);ctx.fillStyle=style.color;ctx.textBaseline='top';const value=e.tagName==='SELECT'?e.selectedOptions[0]?.textContent||'':e.value;ctx.save();ctx.beginPath();ctx.rect(r.x,r.y,r.width,r.height);ctx.clip();const x=r.x+(parseFloat(style.paddingLeft)||0),y=r.y+(parseFloat(style.paddingTop)||0),line=parseFloat(style.lineHeight)||parseFloat(style.fontSize)*1.2;String(value).split('\n').forEach((v,i)=>ctx.fillText(v,x,y+i*line));ctx.restore();}
    }else for(const child of e.childNodes)await draw(child);
    ctx.restore();
   }
   await draw(layer.firstElementChild);ctx.restore();check();
  }catch(e){if(e.message==='COPY_CANCELLED')throw e;fail();}
  finally{host.remove();urls.forEach(url=>URL.revokeObjectURL(url));}
 }
 root.OtoXfaCopy={render};
})(window);
