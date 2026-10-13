import fs from 'node:fs/promises';
import path from 'node:path';
import {Presentation,PresentationFile} from 'file:///C:/Users/cahpo/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs';
import {finalizePresentation} from 'file:///C:/Users/cahpo/.codex/plugins/cache/openai-primary-runtime/presentations/26.1007.11041/skills/presentations/container_tools/artifact_tool_utils.mjs';
const workspaceDir=path.resolve('artifacts/user-guide-20261008');
const skill='C:/Users/cahpo/.codex/plugins/cache/openai-primary-runtime/presentations/26.1007.11041/skills/presentations';
const build=path.join(workspaceDir,'ppt-build');
const output=path.join(workspaceDir,'ppt');
await fs.mkdir(build,{recursive:true}); await fs.mkdir(output,{recursive:true});
const captures=JSON.parse(await fs.readFile(path.join(workspaceDir,'captures.json'),'utf8'));
const p=Presentation.create({slideSize:{width:2800,height:1575}});
function text(slide,name,copy,x,y,w,h,size=26,bold=false){const s=slide.shapes.add({geometry:'textbox',name,position:{left:x,top:y,width:w,height:h},fill:'none',line:{fill:'none',width:0}});s.text=copy;s.text.style={fontFamily:'Aptos',fontSize:size,color:'#102B50',bold};return s;}
for(let start=0;start<captures.length;start+=16){
 const slide=p.slides.add();slide.background.fill='#EDF3FA';
 text(slide,'title','PERTAMINA OTO Windows',64,25,2100,64,52,true);
 text(slide,'status','0.2.3  ·  '+captures.length+' tampilan halaman  ·  16 tampilan per slide  ·  halaman '+(Math.floor(start/16)+1),64,100,2600,46,28);
 const batch=captures.slice(start,start+16);
 for(let j=0;j<batch.length;j++){
  const c=batch[j];const row=Math.floor(j/4);const cols=4;const col=j%4;const w=650,h=310,gap=28;const x=(2800-(cols*w+(cols-1)*gap))/2+col*(w+gap),y=180+row*330;
  slide.shapes.add({geometry:'rect',name:'frame-'+c.id,position:{left:x,top:y,width:w,height:h},fill:'#FFFFFF',line:{fill:'#CCD8E5',width:1}});
  text(slide,'caption-'+c.id,String(start+j+1).padStart(2,'0')+'  '+c.title,x+14,y+9,w-28,42,24,true);
  const box={left:x+14,top:y+57,width:w-28,height:195};const ratio=Math.min(box.width/c.width,box.height/c.height);const iw=c.width*ratio,ih=c.height*ratio;
  const bytes=await fs.readFile(path.join(workspaceDir,c.path));
  slide.images.add({name:c.id,blob:bytes.buffer.slice(bytes.byteOffset,bytes.byteOffset+bytes.byteLength),contentType:c.path.endsWith('.png')?'image/png':'image/jpeg',alt:c.title,position:{left:box.left+(box.width-iw)/2,top:box.top+(box.height-ih)/2,width:iw,height:ih},fit:'contain'});
  text(slide,'instruction-'+c.id,c.mode==='audit-client-render'?(c.privacyCrop?'Crop privasi: bagian atas Pengaturan dan update.':c.description.length>85?c.description.slice(0,82)+'…':c.description):c.mode==='maximized'?'Buka PDF, pilih halaman, gulir atau cari teks.':c.mode==='snap-right'?'Baca dokumen pada Snap kanan Windows.':'Cari teks dan pindah kemunculan pada ukuran minimum.',x+14,y+260,w-28,46,20);
 }
 text(slide,'footer','Screenshot asli terpisah. Render client audit1920×930, bukan native maximize. Tiga reader native. Pengaturan: crop privasi.',64,1520,2600,35,25);
 slide.speakerNotes.text=batch.map(c=>`${c.title}\n${c.description}\nSource: ${path.join(workspaceDir,c.path)}\nBuild: ${c.build}\nDLL: ${c.dllSha256}\nImage SHA256: ${c.imageSha256}`).join('\n\n');
 const png=await p.export({slide,format:'png',scale:0.6});await fs.writeFile(path.join(build,'slide-'+p.slides.items.length+'.png'),new Uint8Array(await png.arrayBuffer()));
}
const candidatePath=path.join(build,'candidate.pptx');await(await PresentationFile.exportPptx(p)).save(candidatePath);
const finalPath=path.join(output,'OTO-Dokumentasi-16-Tampilan-Final.pptx');
const result=await finalizePresentation({workspaceDir,candidatePath,finalPath,pythonExecutable:'C:/Users/cahpo/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe',integrityValidatorPath:path.join(skill,'container_tools/inspect_presentation_package_integrity.py'),layoutValidatorPath:path.join(skill,'container_tools/inspect_presentation_layout_geometry.py'),layoutArgs:['--expected-slide-size-emu','26670000,15001875'],fontPolicy:{basis:'design',families:['Aptos']},verifyArtifactToolImport:true,receiptPath:path.join(build,'validation-16-r2.json')});
console.log(JSON.stringify(result));



