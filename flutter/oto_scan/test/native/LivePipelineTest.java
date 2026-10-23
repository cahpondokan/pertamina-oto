package id.ayani.oto.oto_scan;
import java.nio.*;
import java.util.*;

public class LivePipelineTest {
  static int checks;
  static void check(boolean value,String label){if(!value)throw new AssertionError(label);checks++;System.out.println("PASS "+label);}
  static PreviewPixels.Plane plane(int[] values,int w,int h,int stride,int pixel,int base){
    byte[] bytes=new byte[base+(h-1)*stride+(w-1)*pixel+1];
    for(int y=0;y<h;y++)for(int x=0;x<w;x++)bytes[base+y*stride+x*pixel]=(byte)values[y*w+x];
    ByteBuffer buffer=ByteBuffer.wrap(bytes);buffer.position(base);
    return new PreviewPixels.Plane(buffer,stride,pixel);
  }
  static PreviewPixels.Frame frame(int[] rgb,int w,int h,int rotation,int vw,int vh){
    int[] yy=new int[w*h],u=new int[(w+1)/2*((h+1)/2)],v=u.clone();
    for(int i=0;i<rgb.length;i++){
      int r=(rgb[i]>>16)&255,g=(rgb[i]>>8)&255,b=rgb[i]&255;
      yy[i]=((66*r+129*g+25*b+128)>>8)+16;
    }
    int cw=(w+1)/2,ch=(h+1)/2;
    for(int y=0;y<ch;y++)for(int x=0;x<cw;x++){
      int p=rgb[Math.min(h-1,y*2)*w+Math.min(w-1,x*2)],r=(p>>16)&255,g=(p>>8)&255,b=p&255;
      u[y*cw+x]=((-38*r-74*g+112*b+128)>>8)+128;
      v[y*cw+x]=((112*r-94*g-18*b+128)>>8)+128;
    }
    return PreviewPixels.convert(plane(yy,w,h,w+13,1,5),plane(u,cw,ch,cw*2+7,2,3),plane(v,cw,ch,cw*2+9,2,4),0,0,w,h,rotation,vw,vh);
  }
  static boolean near(int actual,int expected){for(int shift=0;shift<=16;shift+=8)if(Math.abs(((actual>>shift)&255)-((expected>>shift)&255))>4)return false;return true;}
  public static void main(String[] args){
    int[] colors={0xffff0000,0xff00ff00,0xff0000ff,0xffffffff},pixels=new int[8*4];
    for(int y=0;y<4;y++)for(int x=0;x<8;x++)pixels[y*8+x]=colors[(y/2)*2+x/4];
    int[][] expected={{0,1,2,3},{2,0,3,1},{3,2,1,0},{1,3,0,2}};
    for(int rotation=0;rotation<360;rotation+=90){
      int w=rotation%180==0?8:4,h=rotation%180==0?4:8;
      PreviewPixels.Frame f=frame(pixels,8,4,rotation,w,h);boolean correct=true;
      for(int j=0;j<4;j++)correct&=near(f.pixels[(j/2* h/2+h/4)*w+j%2*w/2+w/4],colors[expected[rotation/90][j]]);
      check(correct,"YUV colours, padded/interleaved strides and rotation "+rotation);
    }
    PreviewPixels.Frame bounded=frame(pixels,8,4,0,720,1102);
    check(bounded.height==320&&bounded.width<=320,"analysis stays bounded in displayed aspect ratio");
    int[] y=new int[8*4];for(int j=0;j<4;j++)for(int x=0;x<8;x++)y[j*8+x]=16+x*25;
    int[] neutral=new int[8];Arrays.fill(neutral,128);
    PreviewPixels.Frame crop=PreviewPixels.convert(plane(y,8,4,10,1,2),plane(neutral,4,2,4,1,0),plane(neutral,4,2,4,1,0),2,0,4,4,0,4,4);
    check((crop.pixels[0]&255)>50&&(crop.pixels[3]&255)<155,"nonzero image crop region respected");
    double[] q={.18,.16,.84,.16,.84,.84,.18,.84};
    int[] document=DetectorTest.scene(640,480,q,75,225,true);
    PreviewPixels.Frame f=frame(document,640,480,90,720,1102);
    double[] detected=DocumentDetector.detectPreview(f.pixels,f.width,f.height);
    check(detected!=null,"direct camera-plane document reaches live detector");
    double visibleWidth=640*720.0/1102,left=(480-visibleWidth)/2;
    double[] oracle={(480*.16-left)/visibleWidth,.18,(480*.84-left)/visibleWidth,.18,(480*.84-left)/visibleWidth,.84,(480*.16-left)/visibleWidth,.84};
    double error=0;for(int i=0;i<8;i++)error=Math.max(error,Math.abs(detected[i]-oracle[i]));
    check(error<.035,"live corners match center-cropped portrait preview");
    // Background and paper brightness ranges overlap. No single global
    // intensity/chroma threshold separates this page from its background.
    int w=320,h=420;double[] paper={.17,.12,.84,.16,.88,.89,.12,.92};
    int[] mixed=DetectorTest.scene(w,h,paper,30,220,false);
    for(int py=0;py<h;py++)for(int x=0;x<w;x++){
      int index=py*w+x;boolean inside=(mixed[index]&255)==220;
      double wave=Math.sin(x*.019+py*.012);
      int value=(int)((inside?184:132)+(inside?30:80)*wave);
      if(inside&&py%39<2&&x>w*.3&&x<w*.7)value=35;
      value=Math.max(0,Math.min(255,value));mixed[index]=0xff000000|value*0x010101;
    }
    double[] edge=EdgeDetector.detect(mixed,w,h);
    check(edge!=null,"line fallback finds paper across mixed exposure");
    double worst=0;for(int i=0;i<8;i++)worst=Math.max(worst,Math.abs(edge[i]-paper[i]));
    check(worst<.035,"mixed-exposure corners stay on the paper perimeter");
    check(DocumentDetector.detect(mixed,w,h)!=null,"still-photo path uses edge fallback");
    check(DocumentDetector.detectPreview(mixed,w,h)!=null,"live path also uses edge fallback");
    int[] table=new int[240*320];Arrays.fill(table,0xffffffff);
    for(int row=70;row<=250;row++)for(int x=40;x<=200;x++)
      if(x==40||x==200||row%30==10)table[row*240+x]=0xff202020;
    check(EdgeDetector.detect(table,240,320)==null,"printed table lines alone cannot become paper edges");
    System.out.println(checks+" live pipeline checks passed");
  }
}
