package id.ayani.oto.oto_scan;
import java.util.*;
import java.awt.Polygon;

public class TargetTest {
  static int checks;
  static void check(boolean b,String name){if(!b)throw new AssertionError(name);checks++;System.out.println("PASS "+name);}
  static final double[] LEFT={.08,.18,.38,.20,.39,.81,.07,.79},RIGHT={.53,.06,.94,.08,.94,.94,.53,.92};
  static void paint(int[] p,int w,int h,double[] q,int color){
    Polygon polygon=new Polygon();for(int i=0;i<4;i++)polygon.addPoint((int)Math.round(q[2*i]*(w-1)),(int)Math.round(q[2*i+1]*(h-1)));
    for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(polygon.contains(x,y))p[y*w+x]=0xff000000|color*0x010101;
  }
  static int[] two(int w,int h,int shade){int[] p=new int[w*h];Arrays.fill(p,0xff454545);paint(p,w,h,LEFT,shade);paint(p,w,h,RIGHT,245);return p;}
  static double error(double[] found,double[] oracle){
    if(found==null)return 1;double best=1;
    for(int shift=0;shift<4;shift++){double max=0;for(int i=0;i<4;i++){int j=(i+shift)%4;max=Math.max(max,Math.hypot(found[2*i]-oracle[2*j],found[2*i+1]-oracle[2*j+1]));}best=Math.min(best,max);}return best;
  }
  public static void main(String[] args){
    int w=400,h=400;int[] p=two(w,h,180);
    check(error(DocumentDetector.detectPreview(p,w,h),RIGHT)<.03,"unselected baseline favors larger sheet");
    DetectionTarget target=new DetectionTarget(new double[]{.22,.5},null);
    check(error(DocumentDetector.detectPreview(p,w,h,target),LEFT)<.03,"tap selects smaller paper beside larger distractor");
    check(error(DocumentDetector.detectOutline(p,w,h,target),LEFT)<.03,"still photo uses the same selected object");
    check(error(EdgeDetector.detect(p,w,h,target),LEFT)<.03,"line fallback respects selected object");
    check(DocumentDetector.detectPreview(p,w,h,new DetectionTarget(new double[]{.47,.5},null))==null,"tap on empty gap does not crop a neighboring page");
    check(error(DocumentDetector.detectPreview(p,w,h,new DetectionTarget(null,LEFT)),LEFT)<.03,"previous object outranks a larger new distractor");
    double max=0;long[] elapsed=new long[24];int successful=0;
    for(int i=0;i<24;i++){
      int[] scene=two(w,h,120+i*4);long start=System.nanoTime();
      double e=error(DocumentDetector.detectPreview(scene,w,h,target),LEFT);elapsed[i]=System.nanoTime()-start;max=Math.max(max,e);if(e<.03)successful++;
    }
    check(successful==24,"24 selected-paper contrast scenes retain target and corner accuracy");
    Arrays.sort(elapsed);System.out.println("selected_scenes="+successful+"/24 max_normalized_corner_error="+max+" desktop_p50_ms="+elapsed[12]/1e6+" desktop_p95_ms="+elapsed[22]/1e6);
    double[] center=ScanGeometry.viewToImage(.5,.5,3000,4000,720,1102);
    check(Math.abs(center[0]-.5)<1e-10&&Math.abs(center[1]-.5)<1e-10,"portrait viewport center remains image center");
    double[] edge=ScanGeometry.viewToImage(0,0,3000,4000,720,1102);
    check(edge[0]>.06&&edge[0]<.07&&Math.abs(edge[1])<1e-12,"photo target includes horizontal center-crop offset");
    DetectionTarget mapped=new DetectionTarget(new double[]{.22,.5},LEFT).toImage(3000,4000,720,1102);
    check(mapped.point[0]>.22&&mapped.point[0]<.27&&mapped.previous[0]>LEFT[0],"capture maps both point and prior corners out of cropped preview");
    int[][] centers={{1100,850},{1100,2350},{3100,2350},{3100,850}};
    for(int rotation=0;rotation<360;rotation+=90){
      int vw=rotation%180==0?4000:3000,vh=rotation%180==0?3000:4000;
      int[] r=ScanGeometry.metering(.25,.25,vw,vh,4000,3000,rotation,100,100,4000,3000);
      check(Math.abs((r[0]+r[2])/2-centers[rotation/90][0])<=1&&Math.abs((r[1]+r[3])/2-centers[rotation/90][1])<=1,"metering orientation and active-array offset "+rotation);
    }
    for(double x:new double[]{0,1})for(double y:new double[]{0,1}){
      int[] r=ScanGeometry.metering(x,y,720,1102,4000,3000,90,100,200,4000,3000);
      check(r[0]>=100&&r[1]>=200&&r[2]<=4100&&r[3]<=3200&&r[2]>r[0]&&r[3]>r[1],"metering rectangle clamped at corner "+x+","+y);
    }
    boolean rejected=false;try{new DetectionTarget(new double[]{Double.NaN,.5},null);}catch(IllegalArgumentException e){rejected=true;}
    check(rejected,"invalid target never reaches camera or detector");
    System.out.println(checks+" target checks passed");
  }
}
