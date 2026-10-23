package id.ayani.oto.oto_scan;

/** Pure pixel operations; preserves dimensions and never synthesizes text. */
public final class ScanQuality {
  private ScanQuality() {}
  public static long pixelBudget(long heapBytes){
    return heapBytes>=256L*1024*1024?12_000_000:heapBytes>=192L*1024*1024?8_000_000:6_000_000;
  }
  public static int sampleSize(int w,int h,long budget){
    int sample=1;while((long)((w+sample-1)/sample)*((h+sample-1)/sample)>budget*1.03)sample*=2;
    return sample;
  }
  public static final class Tone {
    final int black;final double gain;
    Tone(int black,double gain){this.black=black;this.gain=gain;}
  }
  static int luminance(int p){return (299*((p>>16)&255)+587*((p>>8)&255)+114*(p&255))/1000;}
  public static Tone tone(int[] sample){
    if(sample.length==0)return new Tone(0,1);
    int[] hist=new int[256];for(int p:sample)hist[luminance(p)]++;
    int low=0,high=255,count=0;
    for(int v=0;v<256;v++){count+=hist[v];if(count>=Math.max(1,sample.length/100)){low=v;break;}}
    count=0;for(int v=0;v<256;v++){count+=hist[v];if(count>=Math.max(1,sample.length*92/100)){high=v;break;}}
    // A mostly blank page has no reliable black reference. Avoid amplifying
    // paper texture/noise or turning coloured seals into pure black/white.
    int black=low<140?Math.min(40,low):0;
    double gain=Math.min(1.6,255.0/Math.max(180,high-black));
    return new Tone(black,gain);
  }
  static int channel(int value,Tone tone,int detail){return Math.max(0,Math.min(255,(int)Math.round((value-tone.black)*tone.gain)+detail));}
  public static void row(int[] above,int[] center,int[] below,int[] output,Tone tone){
    int w=center.length;
    for(int x=0;x<w;x++){
      int left=Math.max(0,x-1),right=Math.min(w-1,x+1),p=center[x];
      int blur=(luminance(above[left])+2*luminance(above[x])+luminance(above[right])+
        2*luminance(center[left])+4*luminance(p)+2*luminance(center[right])+
        luminance(below[left])+2*luminance(below[x])+luminance(below[right])+8)/16;
      int difference=luminance(p)-blur;
      int detail=Math.abs(difference)<=2?0:Math.max(-16,Math.min(16,(int)Math.round(difference*.45)));
      output[x]=0xff000000|(channel((p>>16)&255,tone,detail)<<16)|(channel((p>>8)&255,tone,detail)<<8)|channel(p&255,tone,detail);
    }
  }
}
