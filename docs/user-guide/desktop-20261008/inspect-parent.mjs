import fs from 'node:fs/promises';
import {FileBlob,PresentationFile} from 'file:///C:/Users/cahpo/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs';
const p=await PresentationFile.importPptx(await FileBlob.load('D:/Download/PCP P-OTO v1.pptx'));
console.log('Slides',p.slides.items.length);
for (const [name,slide] of [['parent-first',p.slides.items[0]],['parent-last',p.slides.items.at(-1)]]){
const b=await p.export({slide,format:'png',scale:0.8});await fs.writeFile('artifacts/user-guide-20261008/ppt-build/'+name+'.png',new Uint8Array(await b.arrayBuffer()));}
