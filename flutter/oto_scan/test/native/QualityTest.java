package id.ayani.oto.oto_scan;
import java.util.*;
public class QualityTest {
  static int checks;
  static void check(boolean b,String name){if(!b)throw new AssertionError(name);checks++;System.out.println("PASS "+name);}
  static int gray(int v){return 0xff000000|v*0x010101;}
  public static void main(String[] args){
    check(ScanQuality.pixelBudget(256L*1024*1024)==12_000_000,"12MP retained on normal heap");
    check(ScanQuality.pixelBudget(128L*1024*1024)==6_000_000,"smaller heap bounds photo memory");
    check(ScanQuality.sampleSize(4000,3000,12_000_000)==1,"12MP camera detail is not reduced");
    int sample=ScanQuality.sampleSize(8000,6000,12_000_000);
    check(sample==2&&8000L/sample*(6000/sample)==12_000_000,"oversized capture is bounded without upscaling");
    int[] samplePixels=new int[1000];Arrays.fill(samplePixels,gray(205));Arrays.fill(samplePixels,0,80,gray(75));
    ScanQuality.Tone tone=ScanQuality.tone(samplePixels);
    int[] line=new int[41];Arrays.fill(line,gray(205));line[20]=gray(75);line[19]=line[21]=gray(170);
    int[] before=line.clone(),out=new int[41];ScanQuality.row(line,line,line,out,tone);
    check(Arrays.equals(line,before),"source row remains unchanged");
    check((out[20]&255)<(line[20]&255),"dark fine stroke gains contrast");
    check((out[10]&255)>(line[10]&255),"paper background brightens");
    check((out[10]&255)-(out[20]&255)>205-75,"text/background separation increases");
    check((out[20]&255)<(out[19]&255)&&(out[19]&255)<(out[18]&255),"one-pixel text stroke and antialiasing order preserved");
    check(Arrays.stream(out).allMatch(p->((p>>16)&255)==(p&255)&&((p>>8)&255)==(p&255)),"neutral ink stays neutral");
    int[] flat={gray(255),gray(255),gray(255)},white=new int[3];ScanQuality.row(flat,flat,flat,white,ScanQuality.tone(flat));
    check(Arrays.equals(flat,white),"blank white page unchanged");
    int[] edge={gray(0),gray(255),gray(255)},result=new int[3];ScanQuality.row(edge,edge,edge,result,ScanQuality.tone(flat));
    check((result[0]&255)==0&&(result[2]&255)==255,"sharp edge clipping never wraps pixel values");
    int[] tint={0xff456ea0,0xff456ea0,0xff456ea0};ScanQuality.row(tint,tint,tint,result,tone);
    check((result[1]&255)>((result[1]>>8)&255)&&((result[1]>>8)&255)>((result[1]>>16)&255),"colour seal channel ordering retained");
    check(result.length==tint.length,"enhancement does not resize the image");
    System.out.println(checks+" quality checks passed");
  }
}
