package id.ayani.oto.oto_scan;

import java.io.*;
import java.nio.charset.StandardCharsets;

/** Bounded copy from a possibly non-seekable provider; never uses its filename as a path. */
final class PdfInput {
  static final long MAX_BYTES=100L*1024*1024;
  static long copy(InputStream input,OutputStream output,long limit) throws IOException {
    if(input==null)throw new IOException("PDF tidak dapat dibaca. Buka lagi dari aplikasi asal.");
    byte[] buffer=new byte[65536];ByteArrayOutputStream header=new ByteArrayOutputStream(1024);
    long total=0;int count;
    while((count=input.read(buffer))!=-1){
      if(count==0)continue;
      if(count>limit-total)throw new IOException("PDF terlalu besar. Batas pembaca adalah 100 MB.");
      int headerCount=Math.min(count,1024-header.size());
      if(headerCount>0)header.write(buffer,0,headerCount);
      output.write(buffer,0,count);total+=count;
    }
    if(!new String(header.toByteArray(),StandardCharsets.ISO_8859_1).contains("%PDF-"))
      throw new IOException("File ini bukan PDF yang dapat dibaca.");
    return total;
  }
  static String name(String value){
    if(value==null)return "Dokumen.pdf";
    String clean=value.replaceAll("[\\p{Cntrl}\\p{Cf}/\\\\]"," ").trim();
    if(clean.isEmpty())return "Dokumen.pdf";
    return clean.length()>160?clean.substring(0,160):clean;
  }
}
