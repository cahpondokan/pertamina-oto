from pathlib import Path
root=Path(__file__).resolve().parents[1]
source=(root/'android/app/src/main/java/id/ayani/oto/oto_scan/DocumentDetector.java').read_text()
source=source.replace('if(offsets.size()<8)return q;', 'System.out.println("side="+i+" count="+offsets.size()+" offsets="+Arrays.deepToString(offsets.toArray()));if(offsets.size()<8)return q;')
source=source.replace('if(Math.hypot(x-corners[i][0],y-corners[i][1])>radius*1.8)return q;', 'System.out.println("corner shift="+Math.hypot(x-corners[i][0],y-corners[i][1]));if(Math.hypot(x-corners[i][0],y-corners[i][1])>radius*1.8)return q;')
folder=root/'evidence/trace';folder.mkdir(exist_ok=True)
(folder/'DocumentDetector.java').write_text(source)
