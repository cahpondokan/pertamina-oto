package id.ayani.oto.oto_scan;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.*;
import java.util.zip.DeflaterOutputStream;

/** Small, streamed PDF 1.4 image container; never decodes or rewrites JPEG data. */
public final class CompactPdf {
  public static final int JPEG_QUALITY=90,MAX_EDGE=3508;
  final OutputStream output;
  final int expected;
  final long[] offsets;
  long position;
  int pages;
  boolean finished;

  public static int[] dimensions(int w,int h){
    if(w<1||h<1)throw new IllegalArgumentException("Invalid raster dimensions");
    double scale=Math.min(1,Math.min(MAX_EDGE/(double)Math.max(w,h),Math.sqrt(12_000_000.0/((double)w*h))));
    return new int[]{Math.max(1,(int)Math.floor(w*scale)),Math.max(1,(int)Math.floor(h*scale))};
  }
  public interface RowReader {void read(int y,int[] row) throws IOException;}
  /** Exact black/white only. Never binarizes gray text or removes coloured ink. */
  public static byte[] monochrome(int w,int h,RowReader reader) throws IOException {
    if(w<1||h<1||(long)w*h>12_000_000)throw new IOException("Invalid raster dimensions");
    ByteArrayOutputStream bytes=new ByteArrayOutputStream();
    try(DeflaterOutputStream zip=new DeflaterOutputStream(bytes)){
      int[] row=new int[w];byte[] packed=new byte[(w+7)/8];
      for(int y=0;y<h;y++){
        reader.read(y,row);Arrays.fill(packed,(byte)0);
        for(int x=0;x<w;x++){
          int p=row[x]&0xffffff;
          if(p!=0&&p!=0xffffff)return null;
          if(p==0xffffff)packed[x/8]|=(byte)(0x80>>(x%8));
        }
        zip.write(packed);
      }
    }
    return bytes.toByteArray();
  }
  public CompactPdf(OutputStream output,int pageCount) throws IOException {
    if(pageCount<1||pageCount>20)throw new IOException("Invalid page count");
    this.output=Objects.requireNonNull(output);expected=pageCount;offsets=new long[3+5*pageCount];
    bytes(new byte[]{37,80,68,70,45,49,46,52,10,37,(byte)0xe2,(byte)0xe3,(byte)0xcf,(byte)0xd3,10});
    object(1,"<< /Type /Catalog /Pages 2 0 R >>");
    StringBuilder kids=new StringBuilder();
    for(int i=0;i<pageCount;i++)kids.append(3+i*5).append(" 0 R ");
    object(2,"<< /Type /Pages /Count "+pageCount+" /Kids ["+kids+"] >>");
  }
  void bytes(byte[] data) throws IOException {output.write(data);position+=data.length;}
  void text(String value) throws IOException {bytes(value.getBytes(StandardCharsets.US_ASCII));}
  void start(int id) throws IOException {offsets[id]=position;text(id+" 0 obj\n");}
  void object(int id,String body) throws IOException {start(id);text(body+"\nendobj\n");}
  public void page(int w,int h,byte[] encoded,boolean mono) throws IOException {
    if(finished||pages>=expected||w<1||h<1||(long)w*h>12_000_000||encoded==null||encoded.length<2||encoded.length>40_000_000)
      throw new IOException("Invalid PDF page");
    if(!mono&&(encoded[0]!=(byte)0xff||encoded[1]!=(byte)0xd8||encoded[encoded.length-2]!=(byte)0xff||encoded[encoded.length-1]!=(byte)0xd9))
      throw new IOException("Invalid JPEG");
    int id=3+pages*5;
    double[] pageSize=PageGeometry.pdfSize(w,h);double pw=pageSize[0],ph=pageSize[1];
    String width=String.format(Locale.ROOT,"%.4f",pw),height=String.format(Locale.ROOT,"%.4f",ph);
    object(id,"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 "+width+" "+height+"] /Resources << /XObject << /Im0 "+(id+2)+" 0 R >> >> /Contents "+(id+1)+" 0 R >>");
    byte[] content=("q\n"+width+" 0 0 "+height+" 0 0 cm\n/Im0 Do\nQ\n").getBytes(StandardCharsets.US_ASCII);
    start(id+1);text("<< /Length "+content.length+" >>\nstream\n");bytes(content);text("endstream\nendobj\n");
    start(id+2);text("<< /Type /XObject /Subtype /Image /Width "+w+" /Height "+h+
      " /ColorSpace /"+(mono?"DeviceGray":"DeviceRGB")+" /BitsPerComponent "+(mono?1:8)+
      " /Filter /"+(mono?"FlateDecode":"DCTDecode")+" /Length "+encoded.length+" >>\nstream\n");
    bytes(encoded);text("\nendstream\nendobj\n");object(id+3,"null");object(id+4,"null");pages++;
  }
  void image(int id,int w,int h,byte[] data,int colors,boolean jpeg,Integer mask)throws IOException {
    start(id);text("<< /Type /XObject /Subtype /Image /Width "+w+" /Height "+h+
      " /ColorSpace /"+(colors==1?"DeviceGray":"DeviceRGB")+" /BitsPerComponent 8 /Filter /"+(jpeg?"DCTDecode":"FlateDecode")+
      (jpeg?"":" /DecodeParms << /Predictor 15 /Colors "+colors+" /BitsPerComponent 8 /Columns "+w+" >>")+
      (mask==null?"":" /SMask "+mask+" 0 R")+(colors==1?" /Matte [1 1 1]":"")+" /Length "+data.length+" >>\nstream\n");
    bytes(data);text("\nendstream\nendobj\n");
  }
  public void documentPage(int w,int h,byte[] lossless,byte[] background,byte[] alpha)throws IOException {
    if(finished||pages>=expected||w<1||h<1||(long)w*h>12_000_000||lossless==null||lossless.length<2||(background==null)!=(alpha==null))throw new IOException("Invalid document page");
    int id=3+pages*5;double[] pageSize=PageGeometry.pdfSize(w,h);double pw=pageSize[0],ph=pageSize[1];
    String width=String.format(Locale.ROOT,"%.4f",pw),height=String.format(Locale.ROOT,"%.4f",ph);
    boolean layers=background!=null;
    object(id,"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 "+width+" "+height+"] /Resources << /XObject << /Im0 "+(id+2)+" 0 R"+(layers?" /Ink "+(id+3)+" 0 R":"")+" >> >> /Contents "+(id+1)+" 0 R >>");
    byte[] content=("q\n"+width+" 0 0 "+height+" 0 0 cm\n/Im0 Do\n"+(layers?"/Ink Do\n":"")+"Q\n").getBytes(StandardCharsets.US_ASCII);
    start(id+1);text("<< /Length "+content.length+" >>\nstream\n");bytes(content);text("endstream\nendobj\n");
    if(layers){image(id+2,w,h,background,3,true,null);image(id+3,w,h,lossless,3,false,id+4);image(id+4,w,h,alpha,1,false,null);}
    else{image(id+2,w,h,lossless,3,false,null);object(id+3,"null");object(id+4,"null");}
    pages++;
  }
  public void finish() throws IOException {
    if(finished||pages!=expected)throw new IOException("Incomplete PDF");
    long xref=position;text("xref\n0 "+offsets.length+"\n0000000000 65535 f \n");
    for(int i=1;i<offsets.length;i++)text(String.format(Locale.ROOT,"%010d 00000 n \n",offsets[i]));
    text("trailer\n<< /Size "+offsets.length+" /Root 1 0 R >>\nstartxref\n"+xref+"\n%%EOF\n");
    output.flush();finished=true;
  }
}
