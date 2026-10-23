package id.ayani.oto.oto_scan;

/** Shutter-to-photo regressions, using the actual detector on synthetic pixels. */
public final class CaptureSelectionTest {
  static int checks;
  static void check(boolean ok,String message){if(!ok)throw new AssertionError(message);checks++;System.out.println("PASS "+message);}
  static int[] rotate(int[] pixels,int w,int h,int turns){
    int ow=turns%2==0?w:h,oh=turns%2==0?h:w;int[] out=new int[ow*oh];
    for(int y=0;y<h;y++)for(int x=0;x<w;x++){
      int xx=turns==1?h-1-y:turns==2?w-1-x:turns==3?y:x;
      int yy=turns==1?x:turns==2?h-1-y:turns==3?w-1-x:y;
      out[yy*ow+xx]=pixels[y*w+x];
    }
    return out;
  }
  public static void main(String[] args){
    int w=360,h=480,vw=720,vh=1102;
    long frame=10000,pressed=10250;
    double[][] objects={OrientationCropTest.rect(.5,.5,.64,.80),OrientationCropTest.card(.06,w,h)};
    for(int object=0;object<objects.length;object++){
      double[] oracle=objects[object];int[] photo=DetectorTest.scene(w,h,oracle,40,235,false);
      // Text sits inside the object; the generic full-frame text fixture would
      // cut the small card into separate rectangles by drawing across its edges.
      for(int y=(int)(h*(oracle[1]+.04));y<h*(oracle[5]-.04);y++)
        for(int x=(int)(w*(oracle[0]+.04));x<w*(oracle[2]-.04);x++)
          if(y%13<2)photo[y*w+x]=0xff333333;
      PreviewPixels.Frame preview=LivePipelineTest.frame(OrientationCropTest.sensor(photo,w,h),h,w,90,vw,vh);
      double[] guide=DocumentDetector.detectPreview(preview.pixels,preview.width,preview.height);
      check(guide!=null&&TargetTest.error(new DetectionTarget(null,guide).toImage(w,h,vw,vh).previous,oracle)<.03,"correct visible live guide object="+object);
      for(int delay:new int[]{120,700,1000,1400})for(int turns=0;turns<4;turns++){
        CaptureSelection selection=new CaptureSelection(new DetectionTarget(null,guide),frame,pressed,vw,vh,true);
        // AF/AE wait elapses after the accepted shutter request, without new
        // Dart detections. Freshness must not be evaluated again at this point.
        long exposure=pressed+delay;
        if(delay>=1000)check(!ScanGeometry.fresh(frame,exposure),"old expiry condition exercised delay="+delay+" turns="+turns+" object="+object);
        DetectionTarget selected=selection.forPhoto(vw,vh,true);
        int iw=turns%2==0?w:h,ih=turns%2==0?h:w;
        selected=selected.toOrientedImage(iw,ih,vw,vh,turns);
        double[] found=DocumentDetector.detectOutline(rotate(photo,w,h,turns),iw,ih,selected);
        double[] expected=ScanGeometry.rotateQuad(oracle,turns);
        check(found!=null&&TargetTest.error(ScanGeometry.align(found,expected),expected)<.03,
          "photo verifies crop after focus wait="+delay+" orientation="+turns+" object="+object);
      }
    }
    double[] card=objects[1];DetectionTarget guide=new DetectionTarget(null,card);
    for(long badFrame:new long[]{0,pressed+1,pressed-1201}){
      DetectionTarget bad=new CaptureSelection(guide,badFrame,pressed,vw,vh,true).forPhoto(vw,vh,true);
      check(bad.confirmationRequired&&DocumentDetector.detectOutline(DetectorTest.scene(w,h,card,40,235,false),w,h,bad)==null,
        "guide already invalid at shutter cannot auto-crop timestamp="+badFrame);
    }
    check(!new CaptureSelection(guide,pressed-1200,pressed,vw,vh,true).forPhoto(vw,vh,true).confirmationRequired,"valid shutter freshness boundary accepted");
    CaptureSelection selection=new CaptureSelection(guide,frame,pressed,vw,vh,true);
    check(selection.forPhoto(vw+1,vh,true).confirmationRequired,"changed viewport requires confirmation");
    check(selection.forPhoto(vw,vh,false).confirmationRequired,"changed stream geometry requires confirmation");
    check(new CaptureSelection(guide,frame,pressed,vw,vh,false).forPhoto(vw,vh,true).confirmationRequired,"invalid initial geometry stays manual");
    check(new CaptureSelection(guide,frame,pressed,0,vh,true).forPhoto(0,vh,true).confirmationRequired,"zero viewport cannot authorize guide");
    DetectionTarget selected=selection.forPhoto(vw,vh,true);
    check(DocumentDetector.detectOutline(new int[w*h],w,h,selected)==null,"frozen guide alone never crops a blank photo");
    double[] outer=OrientationCropTest.rect(.5,.5,.84,.84);
    check(DocumentDetector.detectOutline(DetectorTest.scene(w,h,outer,35,230,false),w,h,selected)==null,"missing card does not switch to larger containing paper");
    double[] moved=OrientationCropTest.rect(.84,.82,.16,.12);
    check(DocumentDetector.detectOutline(DetectorTest.scene(w,h,moved,35,230,false),w,h,selected)==null,"unrelated distant object is not automatically cropped");
    check(new CaptureSelection(null,0,pressed,vw,vh,true).forPhoto(vw,vh,true)==null,"capture without guide still permits independent photo detection");
    DetectionTarget tap=new DetectionTarget(new double[]{.5,.5},null);
    check(!new CaptureSelection(tap,0,pressed,vw,vh,true).forPhoto(vw,vh,true).confirmationRequired,"tap-only selection remains supported");
    check(new CaptureSelection(DetectionTarget.manual(),0,pressed,vw,vh,true).forPhoto(vw,vh,true).confirmationRequired,"manual requirement is never silently cleared");
    System.out.println(checks+" capture selection checks passed (desktop only)");
  }
}
