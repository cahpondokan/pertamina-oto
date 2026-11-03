package id.ayani.oto.oto_scan;
import java.io.*;
import java.nio.file.*;
import java.util.*;
public final class ReaderCacheTest {
  static int checks;
  static void check(boolean ok,String name){if(!ok)throw new AssertionError(name);checks++;}
  public static void main(String[] args)throws Exception{
    Path root=Files.createTempDirectory("oto-reader-cache-");
    try{
      ReaderDiskCache cache=new ReaderDiskCache(root.resolve("cache").toFile(),100,60000);
      byte[] a=new byte[60];Arrays.fill(a,(byte)23);cache.put("docA-1",a);
      check(Arrays.equals(cache.get("docA-1"),a),"cache reuses exact encoded bytes");
      cache.file("docA-1").setLastModified(System.currentTimeMillis()-10000);
      cache.put("docB-1",new byte[60]);
      check(cache.get("docA-1")==null&&cache.get("docB-1")!=null,"oldest cache evicted at byte budget");
      cache.file("docB-1").setLastModified(System.currentTimeMillis()-61000);
      check(cache.get("docB-1")==null,"stale cache expires");
      cache.put("docA-1",new byte[20]);cache.put("docB-1",new byte[20]);cache.remove("docA-");
      check(cache.get("docA-1")==null&&cache.get("docB-1")!=null,"deletion invalidates only selected document");
      Files.write(root.resolve("cache/interrupted.part"),new byte[8]);cache.trim();
      check(!Files.exists(root.resolve("cache/interrupted.part")),"partial write is never reused");
      try{cache.get("../private");throw new AssertionError("traversal");}catch(IOException expected){checks++;}
      Files.write(root.resolve("source.pdf"),a);cache.trim();cache.remove("docB-");
      check(Arrays.equals(Files.readAllBytes(root.resolve("source.pdf")),a),"source PDF untouched by cleanup");
      for(int[] shape:new int[][]{{595,842},{842,595},{100,2000},{2000,100}}){
        int[] screen=ReaderPolicy.dimensions(shape[0],shape[1],1080,false);
        int[] detail=ReaderPolicy.dimensions(shape[0],shape[1],3800,true);
        check((long)screen[0]*screen[1]<=2_000_000,"screen pixel bound");
        check((long)detail[0]*detail[1]<=8_000_000,"detail pixel bound");
        check(Math.max(detail[0],detail[1])<=3800,"detail long side bound");
        check(Math.abs(screen[0]/(double)screen[1]-shape[0]/(double)shape[1])<.02,"aspect ratio preserved");
      }
      System.out.println("PASS "+checks+" reader cache and raster budget checks");
    }finally{try(var paths=Files.walk(root)){paths.sorted(Comparator.reverseOrder()).forEach(p->{try{Files.delete(p);}catch(IOException e){throw new UncheckedIOException(e);}});}}
  }
}
