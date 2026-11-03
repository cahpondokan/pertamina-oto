package id.ayani.oto.oto_scan;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.Arrays;

public final class PdfInputTest {
  static int checks;
  static void check(boolean value,String label){if(!value)throw new AssertionError(label);checks++;}
  static void rejects(byte[] value,long limit) throws Exception {
    try{PdfInput.copy(new ByteArrayInputStream(value),new ByteArrayOutputStream(),limit);throw new AssertionError("accepted invalid input");}
    catch(IOException expected){checks++;}
  }
  public static void main(String[] args) throws Exception {
    byte[] pdf="%PDF-1.7\nsynthetic fixture only\n%%EOF".getBytes(StandardCharsets.US_ASCII);
    ByteArrayOutputStream out=new ByteArrayOutputStream();
    check(PdfInput.copy(new ByteArrayInputStream(pdf),out,pdf.length)==pdf.length,"exact bound");
    check(Arrays.equals(pdf,out.toByteArray()),"bytes preserved");
    rejects(pdf,pdf.length-1);rejects(new byte[0],100);rejects("not a PDF".getBytes(StandardCharsets.US_ASCII),100);
    byte[] padded=new byte[1024+pdf.length];System.arraycopy(pdf,0,padded,1024,pdf.length);rejects(padded,3000);
    ByteArrayInputStream shortReads=new ByteArrayInputStream(pdf){@Override public synchronized int read(byte[] b,int o,int n){return super.read(b,o,Math.min(n,2));}};
    out.reset();PdfInput.copy(shortReads,out,1000);check(Arrays.equals(pdf,out.toByteArray()),"short provider reads");
    check(PdfInput.name(null).equals("Dokumen.pdf"),"missing display name");
    check(PdfInput.name("  \u202e../secret\\file\n.pdf  ").equals(".. secret file .pdf"),"no path/control/bidi name");
    check(PdfInput.name("a".repeat(500)).length()==160,"bounded label");
    check(PdfInput.MAX_BYTES==104857600L,"100 MiB bound");
    try{PdfInput.copy(new InputStream(){public int read()throws IOException{throw new IOException("provider failure");}},out,100);throw new AssertionError("lost error");}
    catch(IOException expected){checks++;}
    System.out.println("PASS "+checks+" PDF input checks (synthetic streams only)");
  }
}
