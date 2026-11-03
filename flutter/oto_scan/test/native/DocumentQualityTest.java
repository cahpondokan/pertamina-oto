package id.ayani.oto.oto_scan;

import java.awt.*;
import java.awt.image.BufferedImage;
import java.io.*;
import java.nio.file.*;
import java.util.*;
import javax.imageio.*;
import javax.imageio.stream.MemoryCacheImageOutputStream;

public final class DocumentQualityTest {
  static int checks;
  static void check(boolean ok,String message){if(!ok)throw new AssertionError(message);checks++;System.out.println("PASS "+message);}
  static AdaptiveDocument.Rows rows(BufferedImage b){return (y,row)->b.getRGB(0,y,b.getWidth(),1,row,0,b.getWidth());}
  static AdaptiveDocument.Output output(BufferedImage b){return (y,row)->b.setRGB(0,y,b.getWidth(),1,row,0,b.getWidth());}
  static byte[] jpeg(BufferedImage b)throws Exception{
    ImageWriter writer=ImageIO.getImageWritersByFormatName("jpeg").next();ByteArrayOutputStream bytes=new ByteArrayOutputStream();
    try(MemoryCacheImageOutputStream out=new MemoryCacheImageOutputStream(bytes)){
      writer.setOutput(out);ImageWriteParam param=writer.getDefaultWriteParam();param.setCompressionMode(ImageWriteParam.MODE_EXPLICIT);param.setCompressionQuality(.90f);
      writer.write(null,new IIOImage(b,null,null),param);out.flush();
    }finally{writer.dispose();}return bytes.toByteArray();
  }
  static BufferedImage copy(BufferedImage b){BufferedImage c=new BufferedImage(b.getWidth(),b.getHeight(),BufferedImage.TYPE_INT_RGB);Graphics2D g=c.createGraphics();g.drawImage(b,0,0,null);g.dispose();return c;}
  static void legacy(BufferedImage b){
    int w=b.getWidth(),h=b.getHeight(),sw=Math.max(1,(int)(w*160.0/Math.max(w,h))),sh=Math.max(1,(int)(h*160.0/Math.max(w,h)));
    BufferedImage small=new BufferedImage(sw,sh,BufferedImage.TYPE_INT_RGB);Graphics2D g=small.createGraphics();g.drawImage(b,0,0,sw,sh,null);g.dispose();
    ScanQuality.Tone tone=ScanQuality.tone(small.getRGB(0,0,sw,sh,null,0,sw));
    int[] a=new int[w],c=new int[w],next=new int[w],out=new int[w];b.getRGB(0,0,w,1,c,0,w);System.arraycopy(c,0,a,0,w);
    for(int y=0;y<h;y++){if(y+1<h)b.getRGB(0,y+1,w,1,next,0,w);else System.arraycopy(c,0,next,0,w);ScanQuality.row(a,c,next,out,tone);b.setRGB(0,y,w,1,out,0,w);int[] reuse=a;a=c;c=next;next=reuse;}
  }
  static void plain(Path path,BufferedImage b,byte[] data,boolean mono)throws Exception{
    try(OutputStream out=Files.newOutputStream(path)){CompactPdf p=new CompactPdf(out,1);p.page(b.getWidth(),b.getHeight(),data,mono);p.finish();}
  }
  static void encoded(Path path,int w,int h,byte[] data,byte[] bg,byte[] alpha)throws Exception{
    try(OutputStream out=Files.newOutputStream(path)){CompactPdf p=new CompactPdf(out,1);p.documentPage(w,h,data,bg,alpha);p.finish();}
  }
  public static void main(String[] args)throws Exception{
    Path folder=Paths.get(args[0]);Files.createDirectories(folder);
    check(ScanQuality.sampleSize(4032,3024,12_000_000)==1,"12.2MP camera is not accidentally quartered");
    int w=73,h=69;BufferedImage input=new BufferedImage(w,h,BufferedImage.TYPE_INT_RGB);Random random=new Random(1313);
    for(int y=0;y<h;y++)for(int x=0;x<w;x++){int v=random.nextInt(256);input.setRGB(x,y,v*0x010101);}
    BufferedImage binary=copy(input);AdaptiveDocument.binarize(w,h,rows(binary),output(binary));
    int radius=8;boolean exact=true;
    for(int y=0;y<h;y++)for(int x=0;x<w;x++){
      long sum=0,sq=0;int n=0;
      for(int yy=Math.max(0,y-radius);yy<=Math.min(h-1,y+radius);yy++)for(int xx=Math.max(0,x-radius);xx<=Math.min(w-1,x+radius);xx++){int v=input.getRGB(xx,yy)&255;sum+=v;sq+=(long)v*v;n++;}
      double mean=sum/(double)n,std=Math.sqrt(Math.max(0,sq/(double)n-mean*mean));boolean ink=(input.getRGB(x,y)&255)<mean*(1+.14*(std/128-1));
      exact&=((binary.getRGB(x,y)&255)==0)==ink;
    }
    check(exact,"streaming Sauvola equals independent brute-force windows including all borders");
    BufferedImage shadow=new BufferedImage(400,300,BufferedImage.TYPE_INT_RGB);
    for(int y=0;y<300;y++)for(int x=0;x<400;x++){int v=160+x*85/399;if(y%40>=15&&y%40<=18&&x>20&&x<380)v-=80;shadow.setRGB(x,y,v*0x010101);}
    AdaptiveDocument.normalize(400,300,rows(shadow),output(shadow));
    check(Math.abs((shadow.getRGB(80,8)&255)-(shadow.getRGB(320,8)&255))<15,"uneven paper illumination is flattened");
    AdaptiveDocument.binarize(400,300,rows(shadow),output(shadow));
    boolean strokes=true;for(int y=16;y<280;y+=40)for(int x=40;x<360;x++)strokes&=(shadow.getRGB(x,y)&255)==0;
    check(strokes,"thin strokes survive across light and shadow");
    BufferedImage flat=new BufferedImage(80,80,BufferedImage.TYPE_INT_RGB);Graphics2D fg=flat.createGraphics();fg.setColor(Color.WHITE);fg.fillRect(0,0,80,80);fg.dispose();
    check(!AdaptiveDocument.blurred(80,80,rows(flat)),"blank paper is not reported as a blurred document");
    CaptureReadiness gate=new CaptureReadiness();int[] frame=new int[10000];for(int i=0;i<frame.length;i++)frame[i]=0xff000000|random.nextInt(256)*0x010101;
    gate.observe(frame,100,100,0);gate.observe(frame,100,100,200);gate.observe(frame,100,100,400);
    check(gate.ready(450,200,true,true,true),"focused stationary frames allow capture");
    check(!gate.ready(450,200,false,true,true),"failed focus cannot pass readiness");
    check(!gate.ready(450,200,true,false,true),"exposure convergence is considered");
    check(!gate.ready(1100,200,true,true,true),"stale analysis cannot claim steady capture");
    check(gate.ready(1500,1200,true,true,false),"fixed/no-analysis hardware remains supported");
    check(Arrays.equals(ReaderPolicy.tile(595,842,6656,4096,5120,513,513),new int[]{513,513}),"high zoom tile beyond old whole-page cap");
    try{ReaderPolicy.tile(595,842,6656,6650,0,513,513);throw new AssertionError();}catch(IllegalArgumentException expected){checks++;}
    StringBuilder stats=new StringBuilder("[\n");int count=0;
    for(String name:new String[]{"text","shadow","colour","photo"}){
      long start=System.nanoTime();BufferedImage source=ImageIO.read(folder.resolve(name+".png").toFile());w=source.getWidth();h=source.getHeight();
      BufferedImage old=copy(source);legacy(old);byte[] oldJpeg=jpeg(old);plain(folder.resolve(name+"-old.pdf"),old,oldJpeg,false);
      BufferedImage improved=copy(source);AdaptiveDocument.enhance(w,h,rows(improved),output(improved));ImageIO.write(improved,"png",folder.resolve(name+"-processed.png").toFile());
      byte[] fallback=jpeg(improved);String mode="jpeg";byte[] chosen=fallback,bg=null,alpha=null;
      if(DocumentCompression.paperLike(w,h,rows(improved))){
        byte[] lossless=DocumentCompression.lossless(w,h,rows(improved));
        if(lossless.length<=fallback.length){mode="lossless-rgb";chosen=lossless;}
        else{
          BufferedImage background=new BufferedImage(w,h,BufferedImage.TYPE_INT_RGB);DocumentCompression.Layers layers=DocumentCompression.split(w,h,rows(improved),output(background));
          byte[] base=jpeg(background);
          if((long)layers.foreground.length+layers.alpha.length+base.length+600<fallback.length){mode="layers";chosen=layers.foreground;bg=base;alpha=layers.alpha;}
        }
      }
      Path result=folder.resolve(name+"-new.pdf");
      if(mode.equals("jpeg"))plain(result,improved,chosen,false);else encoded(result,w,h,chosen,bg,alpha);
      check(Files.size(result)<=fallback.length+2000,name+" export remains within same-image JPEG90 size budget");
      if(name.equals("text")){
        BufferedImage background=new BufferedImage(w,h,BufferedImage.TYPE_INT_RGB);DocumentCompression.Layers layers=DocumentCompression.split(w,h,rows(improved),output(background));
        encoded(folder.resolve("forced-layers.pdf"),w,h,layers.foreground,jpeg(background),layers.alpha);
        encoded(folder.resolve("forced-lossless.pdf"),w,h,DocumentCompression.lossless(w,h,rows(improved)),null,null);
        AdaptiveDocument.binarize(w,h,rows(improved),output(improved));ImageIO.write(improved,"png",folder.resolve("adaptive-bw.png").toFile());
        final BufferedImage bw=improved;byte[] mono=CompactPdf.monochrome(w,h,(y,row)->bw.getRGB(0,y,bw.getWidth(),1,row,0,bw.getWidth()));
        plain(folder.resolve("adaptive-bw.pdf"),bw,mono,true);
      }
      if(count++>0)stats.append(",\n");
      stats.append(String.format(Locale.ROOT,"{\"fixture\":\"%s\",\"mode\":\"%s\",\"baseline_bytes\":%d,\"new_bytes\":%d,\"same_image_jpeg_bytes\":%d,\"desktop_ms\":%.2f}",name,mode,Files.size(folder.resolve(name+"-old.pdf")),Files.size(result),fallback.length,(System.nanoTime()-start)/1e6));
    }
    stats.append("\n]");Files.writeString(folder.resolve("compression.json"),stats.toString());
    System.out.println("PASS "+checks+" document quality checks");System.out.println(stats);
  }
}
