package id.ayani.oto.oto_scan;
import java.util.*;
public class SensitivityTest {
  public static void main(String[] args){
    int w=300,h=400;double[] small={.3,.28,.66,.3,.68,.65,.28,.67};
    double[] q=DocumentDetector.detect(DetectorTest.scene(w,h,small,85,220,true),w,h);
    DetectorTest.check(q!=null,"smaller document below old 15 percent area limit detected");
    double worst=0;for(int i=0;i<8;i++)worst=Math.max(worst,Math.abs(q[i]-small[i]));
    DetectorTest.check(worst<.03,"small-page corners track external edge");
    double[] paper={.18,.1,.8,.1,.8,.9,.18,.9};
    int[] weak=DetectorTest.scene(w,h,paper,120,180,false);
    for(int y=0;y<h;y++)for(int x=(int)(w*.8);x<w;x++)weak[y*w+x]=0xffafafaf; // right edge contrast only 5
    DetectorTest.check(DocumentDetector.detect(weak,w,h)!=null,"one weak edge supported by other three paper sides");
    DetectorTest.check(DocumentDetector.detectPreview(weak,w,h)!=null,"preview also finds weak paper edge");
    int[] blank=new int[w*h];Arrays.fill(blank,0xffbababa);
    DetectorTest.check(DocumentDetector.detectPreview(blank,w,h)==null,"blank preview produces no invented box");
    int[] crop=DetectorTest.scene(w,h,new double[]{0,.1,.8,.1,.8,.9,0,.9},30,230,false);
    DetectorTest.check(DocumentDetector.detectPreview(crop,w,h)==null,"fast preview still rejects clipped sheet");
    Random random=new Random(481);for(int i=0;i<blank.length;i++)blank[i]=0xff000000|(175+random.nextInt(9))*0x010101;
    DetectorTest.check(DocumentDetector.detect(blank,w,h)==null,"subtle sensor noise remains rejected");
    System.out.println(DetectorTest.passed+" sensitivity checks passed");
  }
}
