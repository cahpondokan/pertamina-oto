package id.ayani.oto.oto_scan;
import java.io.*;
import java.nio.file.*;
import java.util.*;
import java.util.zip.InflaterInputStream;
import java.awt.image.BufferedImage;
import javax.imageio.*;
import javax.imageio.stream.MemoryCacheImageOutputStream;

public class CompactPdfTest {
  static int checks;
  static void check(boolean value,String label){if(!value)throw new AssertionError(label);checks++;System.out.println("PASS "+label);}
  interface Action{void run() throws Exception;}
  static void rejects(Action action,String label) throws Exception {boolean rejected=false;try{action.run();}catch(IOException|IllegalArgumentException e){rejected=true;}check(rejected,label);}
  static byte[] jpeg(BufferedImage image) throws Exception{
    ImageWriter writer=ImageIO.getImageWritersByFormatName("jpeg").next();ByteArrayOutputStream bytes=new ByteArrayOutputStream();
    try(MemoryCacheImageOutputStream out=new MemoryCacheImageOutputStream(bytes)){
      writer.setOutput(out);ImageWriteParam params=writer.getDefaultWriteParam();params.setCompressionMode(ImageWriteParam.MODE_EXPLICIT);params.setCompressionQuality(CompactPdf.JPEG_QUALITY/100f);
      writer.write(null,new IIOImage(image,null,null),params);out.flush();
    }finally{writer.dispose();}
    return bytes.toByteArray();
  }
  public static void main(String[] args) throws Exception{
    Path folder=Paths.get(args[0]);Files.createDirectories(folder);
    check(Arrays.equals(CompactPdf.dimensions(2480,3508),new int[]{2480,3508}),"A4 300dpi pixels kept");
    check(Arrays.equals(CompactPdf.dimensions(500,700),new int[]{500,700}),"small scans never upscaled");
    int[] size=CompactPdf.dimensions(4000,3000);
    check(size[0]==3508&&size[1]==2631,"large images bounded with aspect preserved");
    rejects(()->CompactPdf.dimensions(0,300),"invalid raster rejected");
    rejects(()->new CompactPdf(new ByteArrayOutputStream(),0),"empty PDF rejected");
    rejects(()->new CompactPdf(new ByteArrayOutputStream(),21),"page limit enforced");
    ByteArrayOutputStream stream=new ByteArrayOutputStream();CompactPdf pdf=new CompactPdf(stream,1);
    rejects(pdf::finish,"incomplete PDF cannot finish");
    rejects(()->pdf.page(100,100,new byte[]{1,2,3},false),"non JPEG rejected");
    rejects(()->pdf.page(Integer.MAX_VALUE,100,new byte[]{1,2},true),"oversized page rejected without overflow");
    int w=17,h=3;int[] pixels=new int[w*h];
    for(int i=0;i<pixels.length;i++)pixels[i]=i%3==0?0xff000000:0xffffffff;
    byte[] mono=CompactPdf.monochrome(w,h,(y,row)->System.arraycopy(pixels,y*w,row,0,w));
    byte[] raw=new InflaterInputStream(new ByteArrayInputStream(mono)).readAllBytes();
    check(raw.length==((w+7)/8)*h,"monochrome scanline padding exact");
    boolean same=true;for(int y=0;y<h;y++)for(int x=0;x<w;x++)same&=(((raw[y*3+x/8]>>(7-x%8))&1)==((pixels[y*w+x]&255)==255?1:0));
    check(same,"monochrome retains every source pixel losslessly");
    pixels[10]=0xfffefefe;
    check(CompactPdf.monochrome(w,h,(y,row)->System.arraycopy(pixels,y*w,row,0,w))==null,"gray antialiasing never silently binarized");
    pixels[10]=0xffff0000;
    check(CompactPdf.monochrome(w,h,(y,row)->System.arraycopy(pixels,y*w,row,0,w))==null,"coloured ink never removed");
    pdf.page(w,h,mono,true);pdf.finish();
    rejects(pdf::finish,"duplicate finish rejected");
    rejects(()->pdf.page(w,h,mono,true),"cannot append after finish");
    OutputStream failing=new OutputStream(){public void write(int b)throws IOException{throw new IOException("disk failure");}};
    rejects(()->new CompactPdf(failing,1),"output failure propagated");
    BufferedImage document=ImageIO.read(folder.resolve("source.png").toFile());
    byte[] colour=jpeg(document);Files.write(folder.resolve("encoded.jpg"),colour);
    int sw=document.getWidth(),sh=document.getHeight();
    // Same dimensions, dense original fixture, and independent encoder. Device encoder remains untested.
    try(OutputStream out=Files.newOutputStream(folder.resolve("compact-colour.pdf"))){CompactPdf p=new CompactPdf(out,1);p.page(sw,sh,colour,false);p.finish();}
    check(Files.size(folder.resolve("compact-colour.pdf"))<colour.length+2000,"PDF container adds less than 2KB");
    BufferedImage binary=ImageIO.read(folder.resolve("binary.png").toFile());
    byte[] bw=CompactPdf.monochrome(sw,sh,(y,row)->binary.getRGB(0,y,sw,1,row,0,sw));
    check(bw!=null,"binary fixture uses lossless one-bit compression");
    try(OutputStream out=Files.newOutputStream(folder.resolve("compact-mono.pdf"))){CompactPdf p=new CompactPdf(out,1);p.page(sw,sh,bw,true);p.finish();}
    BufferedImage orientation=ImageIO.read(folder.resolve("orientation.png").toFile());
    byte[] orientationJpeg=jpeg(orientation);
    try(OutputStream out=Files.newOutputStream(folder.resolve("three-pages.pdf"))){CompactPdf p=new CompactPdf(out,3);p.page(sw,sh,colour,false);p.page(sw,sh,bw,true);p.page(orientation.getWidth(),orientation.getHeight(),orientationJpeg,false);p.finish();}
    try(OutputStream out=Files.newOutputStream(folder.resolve("twenty-pages.pdf"))){CompactPdf p=new CompactPdf(out,20);for(int i=0;i<20;i++)p.page(orientation.getWidth(),orientation.getHeight(),orientationJpeg,false);p.finish();}
    check(Files.size(folder.resolve("twenty-pages.pdf"))<20L*orientationJpeg.length+20000,"multipage output remains streamed and compact");
    System.out.println(checks+" compact PDF checks passed");
  }
}
