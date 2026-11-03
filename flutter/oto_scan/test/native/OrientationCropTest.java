package id.ayani.oto.oto_scan;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.*;
import java.util.regex.*;

/** Synthetic desktop regressions for AOS-01..06. No Android runtime/device. */
public final class OrientationCropTest {
  static int checks;
  static void check(boolean pass,String name){if(!pass)throw new AssertionError(name);checks++;System.out.println("PASS "+name);}
  static double[] rect(double cx,double cy,double w,double h){return new double[]{cx-w/2,cy-h/2,cx+w/2,cy-h/2,cx+w/2,cy+h/2,cx-w/2,cy+h/2};}
  static double[] card(double area,int w,int h){double pw=Math.sqrt(area*w*h*1.586);return rect(.5,.5,pw/w,pw/1.586/h);}
  static int[] sensor(int[] portrait,int w,int h){int[] raw=new int[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++)raw[(w-1-x)*h+y]=portrait[y*w+x];return raw;}
  static int[] dimensions(double[] q,int w,int h){return PageGeometry.cropSize(Math.max(edge(q,0,1,w,h),edge(q,3,2,w,h)),Math.max(edge(q,0,3,w,h),edge(q,1,2,w,h)),"free");}
  static double edge(double[] q,int a,int b,int w,int h){return Math.hypot((q[2*a]-q[2*b])*(w-1),(q[2*a+1]-q[2*b+1])*(h-1));}
  static double[] angled(double degrees){double a=Math.toRadians(degrees),c=Math.cos(a),s=Math.sin(a);double[] q=new double[8];double[][] p={{-90,-55},{90,-55},{90,55},{-90,55}};for(int i=0;i<4;i++){q[2*i]=(200+c*p[i][0]-s*p[i][1])/399;q[2*i+1]=(200+s*p[i][0]+c*p[i][1])/399;}return q;}
  static double[] box(byte[] bytes){Matcher m=Pattern.compile("/MediaBox \\[0 0 ([0-9.]+) ([0-9.]+)\\]").matcher(new String(bytes,StandardCharsets.ISO_8859_1));if(!m.find())throw new AssertionError("No MediaBox");return new double[]{Double.parseDouble(m.group(1)),Double.parseDouble(m.group(2))};}
  static byte[] pdf(int w,int h,int mode)throws Exception{
    ByteArrayOutputStream out=new ByteArrayOutputStream();CompactPdf p=new CompactPdf(out,1);
    if(mode==0)p.page(w,h,CompactPdf.monochrome(w,h,(y,row)->Arrays.fill(row,0xffffffff)),true);
    else{
      byte[] rgb=DocumentCompression.lossless(w,h,(y,row)->Arrays.fill(row,0xffeeeeee));
      if(mode==1)p.documentPage(w,h,rgb,null,null);
      else {
        java.awt.image.BufferedImage image=new java.awt.image.BufferedImage(w,h,java.awt.image.BufferedImage.TYPE_INT_RGB);
        ByteArrayOutputStream jpg=new ByteArrayOutputStream();javax.imageio.ImageIO.write(image,"JPEG",jpg);
        if(mode==2)p.page(w,h,jpg.toByteArray(),false);
        else{
          DocumentCompression.Layers layer=DocumentCompression.split(w,h,(y,row)->Arrays.fill(row,y%9<2?0xff111111:0xffffffff),(y,row)->{});
          p.documentPage(w,h,layer.foreground,jpg.toByteArray(),layer.alpha);
        }
      }
    }
    p.finish();return out.toByteArray();
  }
  public static void main(String[] args)throws Exception{
    for(int sensor:new int[]{90,270})for(int turns=0;turns<4;turns++)check(ScanGeometry.jpegOrientation(sensor,turns)==(sensor+90*turns)%360,"rear JPEG orientation sensor="+sensor+" turns="+turns);
    check(ScanGeometry.deviceTurns(-1,-1)==-1&&ScanGeometry.deviceTurns(-1,2)==2,"unknown orientation retains last value without inventing first reading");
    check(ScanGeometry.deviceTurns(44,0)==0&&ScanGeometry.deviceTurns(50,0)==0&&ScanGeometry.deviceTurns(56,0)==1,"physical orientation hysteresis across 45 degrees");
    check(ScanGeometry.deviceTurns(359,0)==0&&ScanGeometry.deviceTurns(270,0)==3,"orientation wraps through 360 and reverse landscape");
    DetectionTarget prior=new DetectionTarget(new double[]{.23,.68},rect(.25,.65,.2,.24));
    for(int[] view:new int[][]{{720,1102},{1080,1920},{1000,1000},{1600,900}})for(int turns=0;turns<4;turns++){
      DetectionTarget normal=prior.toImage(3000,4000,view[0],view[1]);
      DetectionTarget oriented=prior.toOrientedImage(turns%2==0?3000:4000,turns%2==0?4000:3000,view[0],view[1],turns);
      double[] expected=ScanGeometry.rotatePoint(normal.point[0],normal.point[1],turns);
      check(Math.hypot(oriented.point[0]-expected[0],oriented.point[1]-expected[1])<1e-10&&DocumentDetector.valid(oriented.previous),"viewport to oriented JPEG preserves selected coordinates "+Arrays.toString(view)+" / "+turns);
      double[] roundTrip=ScanGeometry.rotateQuad(ScanGeometry.rotateQuad(normal.previous,turns),(4-turns)%4);
      double err=0;for(int i=0;i<8;i++)err=Math.max(err,Math.abs(normal.previous[i]-roundTrip[i]));check(err<1e-10,"four-corner rotation roundtrip "+Arrays.toString(view)+" / "+turns);
    }
    check(prior.atShutter(5000,6200)==prior&&prior.atShutter(5000,6201).confirmationRequired,"guide already stale at shutter requires manual confirmation");
    check(new DetectionTarget(null,prior.previous).atShutter(0,6200).confirmationRequired&&!ScanGeometry.fresh(7000,6200),"missing or future timestamps cannot authorize prior crop");

    for(double area:new double[]{.03,.05,.072,.074,.076,.078,.084,.12}){
      double[] oracle=card(area,480,640);
      int[] raw=sensor(DetectorTest.scene(480,640,oracle,45,230,false),480,640);
      PreviewPixels.Frame f=LivePipelineTest.frame(raw,640,480,90,720,1102);
      double[] preview=DocumentDetector.detectPreview(f.pixels,f.width,f.height);
      check(preview!=null,"small untapped preview card area="+area);
      DetectionTarget selected=new DetectionTarget(new double[]{.5,.5},preview).toImage(360,480,720,1102);
      double[] found=DocumentDetector.detectOutline(DetectorTest.scene(360,480,oracle,45,230,false),360,480,selected);
      check(found!=null&&TargetTest.error(found,oracle)<.03,"photo verifies same small preview card area="+area);
    }
    double[] small=card(.06,360,480),outer=rect(.5,.5,.84,.84);
    int[] nested=DetectorTest.scene(360,480,outer,35,150,false);
    for(int y=0;y<480;y++)for(int x=0;x<360;x++)if(DetectionTarget.contains(small,x/359.0,y/479.0))nested[y*360+x]=0xffeeeeee;
    DetectionTarget selected=new DetectionTarget(new double[]{.5,.5},small);
    double[] found=DocumentDetector.detectOutline(nested,360,480,selected);
    check(found!=null&&TargetTest.error(found,small)<.03,"selected card crops itself instead of containing large paper");
    check(DocumentDetector.detectOutline(DetectorTest.scene(360,480,outer,35,150,false),360,480,selected)==null,"missing selected card cannot silently become containing large sheet");
    check(DocumentDetector.detectOutline(new int[360*480],360,480,selected)==null,"prior proposal alone never causes automatic crop");
    DetectionTarget expired=selected.atShutter(100,1500).toOrientedImage(360,480,720,1102,0);
    check(expired.previous==null&&expired.point==null&&DocumentDetector.detectOutline(nested,360,480,expired)==null,"expired reference cannot switch to outer paper or reuse stale proposal");
    double[] tiny=card(.01,360,480);
    found=DocumentDetector.detectOutline(DetectorTest.scene(360,480,tiny,35,230,false),360,480,new DetectionTarget(new double[]{.5,.5},null));
    check(found!=null&&TargetTest.error(found,tiny)<.03,"tap ROI can find one-percent card with four genuine edges");
    check(DocumentDetector.valid(tiny)&&!DocumentDetector.valid(rect(.5,.5,.01,.01)),"manual geometry accepts small card but rejects degenerate tiny box");

    double[] a=DocumentDetector.detectOutline(DetectorTest.scene(400,400,angled(44),45,230,false),400,400);
    double[] b=DocumentDetector.detectOutline(DetectorTest.scene(400,400,angled(46),45,230,false),400,400,new DetectionTarget(null,a));
    int[] da=dimensions(a,400,400),db=dimensions(b,400,400);
    check(da[0]>da[1]&&db[0]>db[1]&&Math.abs(da[0]-db[0])<3,"44 to 46 degree crop keeps reference corner identity and landscape shape");
    for(int turns=0;turns<4;turns++){
      double[] rotated=ScanGeometry.rotateQuad(a,turns);int[] d=dimensions(rotated,400,400);
      check((turns%2==0)==(d[0]>d[1]),"intentional physical quarter-turn swaps crop axes "+turns);
    }
    check(Arrays.equals(PageGeometry.cropSize(200,126,"free"),new int[]{200,126}),"free mode preserves measured photo ratio");
    int[] corrected=PageGeometry.cropSize(120,126,"card");
    check(corrected[0]>corrected[1]&&Math.abs(corrected[0]/(double)corrected[1]-85.6/53.98)<.02&&corrected[0]<=120&&corrected[1]<=126,"explicit card landscape corrects foreshortened shape without upscaling");
    for(String mode:new String[]{"a4","a4-landscape","card","card-portrait"}){
      int[] d=PageGeometry.cropSize(1800,2400,mode);check(d[0]<=1800&&d[1]<=2400,"preset bounded by observed pixels: "+mode);
      for(int turns=0;turns<4;turns++){
        int[] c=PageGeometry.cropSize(1800,2400,mode,turns);
        double finalRatio=turns%2==0?c[0]/(double)c[1]:c[1]/(double)c[0];
        double expected=mode.equals("a4")?210.0/297:mode.equals("a4-landscape")?297.0/210:mode.equals("card")?85.6/53.98:53.98/85.6;
        check(Math.abs(finalRatio-expected)<.002&&c[0]<=1800&&c[1]<=2400,"preset describes final shape after manual rotation "+mode+" / "+turns);
      }
    }
    check(!PageGeometry.validMode("unknown"),"invalid aspect mode rejected");
    for(int mode=0;mode<4;mode++)for(int[] size:new int[][]{{90,120},{86,54},{100,100},{101,100},{21,29}}){
      double[] p=box(pdf(size[0],size[1],mode)),r=box(pdf(size[1],size[0],mode));
      check(Math.abs(p[0]-r[1])<.0002&&Math.abs(p[1]-r[0])<.0002,"PDF physical size invariant under rotation, encoding="+mode+" raster="+Arrays.toString(size));
    }
    System.out.println(checks+" orientation/crop/page regression checks passed (desktop only)");
  }
}
