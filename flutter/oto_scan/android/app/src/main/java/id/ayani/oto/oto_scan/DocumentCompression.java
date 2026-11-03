package id.ayani.oto.oto_scan;

import java.io.*;
import java.util.zip.DeflaterOutputStream;

/** Lossless pixel streams and a conservative foreground split, never OCR text. */
public final class DocumentCompression {
  public static final class Layers {
    public final byte[] foreground,alpha;
    public final double coverage;
    Layers(byte[] foreground,byte[] alpha,double coverage){this.foreground=foreground;this.alpha=alpha;this.coverage=coverage;}
  }
  /** PNG row prediction inside PDF FlateDecode. Row storage is O(width). */
  static final class Predictor implements AutoCloseable {
    final ByteArrayOutputStream bytes=new ByteArrayOutputStream();
    final DeflaterOutputStream zip=new DeflaterOutputStream(bytes);
    final int colors;byte[] previous,encoded;
    Predictor(int width,int colors){this.colors=colors;previous=new byte[width*colors];encoded=new byte[previous.length+1];}
    void row(byte[] raw)throws IOException{
      long sub=0,up=0;
      for(int i=0;i<raw.length;i+=colors){sub+=Math.abs((byte)(raw[i]-(i>=colors?raw[i-colors]:0)));up+=Math.abs((byte)(raw[i]-previous[i]));}
      boolean useUp=up<=sub;encoded[0]=(byte)(useUp?2:1);
      for(int i=0;i<raw.length;i++)encoded[i+1]=(byte)(raw[i]-(useUp?previous[i]:(i>=colors?raw[i-colors]:0)));
      zip.write(encoded);System.arraycopy(raw,0,previous,0,raw.length);
    }
    public void close()throws IOException{zip.close();}
    byte[] data(){return bytes.toByteArray();}
  }
  public static byte[] lossless(int w,int h,AdaptiveDocument.Rows input)throws IOException{
    Predictor encoder=new Predictor(w,3);int[] row=new int[w];byte[] raw=new byte[w*3];
    try(encoder){for(int y=0;y<h;y++){input.read(y,row);for(int x=0;x<w;x++){int p=row[x];raw[x*3]=(byte)(p>>16);raw[x*3+1]=(byte)(p>>8);raw[x*3+2]=(byte)p;}encoder.row(raw);}}
    return encoder.data();
  }
  static boolean foreground(int p){
    int r=(p>>16)&255,g=(p>>8)&255,b=p&255;
    return Math.min(r,Math.min(g,b))<248||Math.max(r,Math.max(g,b))-Math.min(r,Math.min(g,b))>3;
  }
  public static boolean paperLike(int w,int h,AdaptiveDocument.Rows input)throws IOException{
    int[] row=new int[w];long ink=0,total=0;
    for(int y=0;y<h;y+=8){input.read(y,row);for(int x=0;x<w;x+=8){total++;if(foreground(row[x]))ink++;}}
    return total>0&&ink>0&&ink/(double)total<.45;
  }
  public static Layers split(int w,int h,AdaptiveDocument.Rows input,AdaptiveDocument.Output background)throws IOException{
    Predictor rgb=new Predictor(w,3),mask=new Predictor(w,1);
    byte[] raw=new byte[w*3],alpha=new byte[w];int[] row=new int[w];long count=0;
    try(rgb;mask){
      for(int y=0;y<h;y++){
        input.read(y,row);
        for(int x=0;x<w;x++){
          int p=row[x];boolean ink=foreground(p);alpha[x]=(byte)(ink?255:0);
          // Every visible foreground RGB pixel is copied exactly, including
          // antialiasing, pale ink and coloured stamps/signatures.
          raw[x*3]=(byte)(ink?p>>16:255);raw[x*3+1]=(byte)(ink?p>>8:255);raw[x*3+2]=(byte)(ink?p:255);
          if(ink){count++;row[x]=0xffffffff;}
        }
        rgb.row(raw);mask.row(alpha);background.write(y,row);
      }
    }
    return new Layers(rgb.data(),mask.data(),count/(double)((long)w*h));
  }
}
