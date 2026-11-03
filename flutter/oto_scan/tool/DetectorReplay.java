package id.ayani.oto.oto_scan;
import javax.imageio.ImageIO;
import java.io.*;
import java.awt.image.BufferedImage;
import java.util.Arrays;
public class DetectorReplay {
  public static void main(String[] args)throws Exception{
    BufferedImage b=ImageIO.read(new File(args[0]));
    int w=b.getWidth(),h=b.getHeight();long start=System.nanoTime();
    double[] q=DocumentDetector.detect(b.getRGB(0,0,w,h,null,0,w),w,h);
    System.out.println("dimensions="+w+"x"+h+" corners="+Arrays.toString(q)+" ms="+(System.nanoTime()-start)/1000000);
  }
}
