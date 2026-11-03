package id.ayani.oto.oto_scan;

/** Small luminance sample for advisory movement gating, independent of AF hardware. */
public final class CaptureReadiness {
  int[] previous;int stable;long observed;
  public void observe(int[] pixels,int w,int h,long now){
    int[] sample=new int[100];double mean=0;
    for(int y=0;y<10;y++)for(int x=0;x<10;x++){
      int xx=Math.min(w-1,(int)((.1+.8*(x+.5)/10)*w)),yy=Math.min(h-1,(int)((.1+.8*(y+.5)/10)*h));
      sample[y*10+x]=ScanQuality.luminance(pixels[yy*w+xx]);
      if(previous!=null)mean+=sample[y*10+x]-previous[y*10+x];
    }
    if(previous!=null){
      mean/=sample.length;double movement=0;
      for(int i=0;i<sample.length;i++)movement+=Math.abs(sample[i]-previous[i]-mean);
      stable=movement/sample.length<3.5?stable+1:0;
    }
    previous=sample;observed=now;
  }
  public boolean steady(long now){return previous!=null&&now-observed<650&&stable>=2;}
  public boolean ready(long now,long started,boolean focused,boolean exposureReady,boolean analysisAvailable){
    return now-started>=120&&focused&&exposureReady&&(!analysisAvailable||steady(now));
  }
}
