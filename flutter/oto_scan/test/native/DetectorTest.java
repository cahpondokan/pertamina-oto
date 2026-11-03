package id.ayani.oto.oto_scan;
import java.util.*;
public class DetectorTest {
  static int passed=0;
  static void check(boolean value,String label){if(!value)throw new AssertionError(label);passed++;System.out.println("PASS "+label);}
  static int[] scene(int w,int h,double[] quad,int bg,int paper,boolean lines){
    int[] a=new int[w*h];Arrays.fill(a,0xff000000|bg*0x010101);
    for(int y=0;y<h;y++)for(int x=0;x<w;x++){
      boolean in=true;double px=x/(double)(w-1),py=y/(double)(h-1);
      for(int i=0;i<4;i++){int j=(i+1)%4;if((quad[j*2]-quad[i*2])*(py-quad[i*2+1])-(quad[j*2+1]-quad[i*2+1])*(px-quad[i*2])<0)in=false;}
      if(in){int v=lines&&y%35<2&&x>w*.3&&x<w*.7?40:paper;a[y*w+x]=0xff000000|v*0x010101;}
    }return a;
  }
  public static void main(String[] args){
    int w=400,h=540;double[] q={.18,.1,.85,.16,.89,.88,.12,.93};
    double[] found=DocumentDetector.detect(scene(w,h,q,45,240,true),w,h);
    check(found!=null,"perspective printed document detected");
    double worst=0;for(int i=0;i<8;i++)worst=Math.max(worst,Math.abs(found[i]-q[i]));
    check(worst<.025,"four corners within 2.5 percent of oracle");
    check(DocumentDetector.valid(found),"detected quad convex");
    check(DocumentDetector.detect(scene(w,h,q,235,40,false),w,h)!=null,"dark rectangle on light background");
    check(DocumentDetector.detect(scene(w,h,q,90,150,true),w,h)!=null,"moderate contrast");
    check(DocumentDetector.detect(scene(w,h,q,150,160,false),w,h)==null,"ambiguous contrast falls back");
    check(DocumentDetector.detect(scene(w,h,new double[]{0,0,1,0,1,1,0,1},40,240,false),w,h)==null,"clipped paper rejected");
    check(DocumentDetector.detect(new int[w*h],w,h)==null,"blank rejected");
    check(!DocumentDetector.valid(new double[]{0,0,1,1,1,0,0,1}),"crossed quad rejected");
    check(!DocumentDetector.valid(new double[]{Double.NaN,0,1,0,1,1,0,1}),"nan rejected");
    check(!DocumentDetector.valid(new double[]{-.1,0,1,0,1,1,0,1}),"outside rejected");
    check(!DocumentDetector.valid(new double[]{.1,.1,.11,.1,.11,.11,.1,.11}),"tiny rejected");
    Random random=new Random(83);int[] noise=new int[w*h];for(int i=0;i<noise.length;i++){int v=random.nextInt(256);noise[i]=0xff000000|v*0x010101;}
    check(DocumentDetector.detect(noise,w,h)==null,"noise rejected");
    double[] rotated={.48,.08,.92,.48,.53,.94,.1,.53};
    check(DocumentDetector.detect(scene(w,h,rotated,35,245,false),w,h)!=null,"rotated rectangular object");
    int[] chroma=scene(w,h,q,45,240,false);
    for(int i=0;i<chroma.length;i++)chroma[i]=(chroma[i]&255)==240?0xffb8c4d1:0xffd9c5a8;
    check(DocumentDetector.detect(chroma,w,h)!=null,"cool darker paper on warm light desk");
    int[] equalLuma=scene(w,h,q,45,240,false);
    for(int i=0;i<equalLuma.length;i++)equalLuma[i]=(equalLuma[i]&255)==240?0xffa7bacf:0xffc8b59f;
    check(DocumentDetector.detect(equalLuma,w,h)!=null,"paper detected by colour when luminance overlaps");
    int[] large=scene(1200,1600,q,55,230,true);double[] fine=DocumentDetector.detect(large,1200,1600);
    check(fine!=null,"camera resolution refinement succeeds");
    double cornerError=0;for(int i=0;i<4;i++)cornerError=Math.max(cornerError,Math.hypot((fine[2*i]-q[2*i])*1199,(fine[2*i+1]-q[2*i+1])*1599));
    check(cornerError<14,"refined corners within fourteen source pixels of oracle: "+cornerError);
    int[] softEdges=scene(w,h,q,45,240,true);
    for(int y=0;y<h;y++)for(int x=0;x<w;x++){
      int at=y*w+x,v=softEdges[at]&255;
      if(v==240)softEdges[at]=0xffb1b3ba;
      else if(v==45){int shift=(int)(x/(double)w*10);softEdges[at]=0xff000000|((185-shift)<<16)|((177-shift)<<8)|(160+shift);}
    }
    check(DocumentDetector.detect(softEdges,w,h)!=null,"uneven soft colour edges retain all four sides");
    int[] faint=scene(w,h,q,170,179,true);
    check(DocumentDetector.detect(faint,w,h)==null,"printed content cannot rescue uniformly ambiguous paper edges");
    System.out.println(passed+" checks passed");
  }
}
