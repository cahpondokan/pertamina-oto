package id.ayani.oto.oto_scan;
import java.io.*;
import java.nio.file.*;
import java.util.*;

/** Only disposable reader rasters live in this directory, never source PDFs. */
final class ReaderDiskCache {
  final File directory;
  final long limit,age;
  ReaderDiskCache(File directory){this(directory,ReaderPolicy.CACHE_BYTES,ReaderPolicy.CACHE_AGE_MS);}
  ReaderDiskCache(File directory,long limit,long age){this.directory=directory;this.limit=limit;this.age=age;directory.mkdirs();}
  File file(String key)throws IOException{
    if(key==null||!key.matches("[a-zA-Z0-9_-]{1,180}"))throw new IOException("Invalid cache key");
    return new File(directory,key+".jpg");
  }
  byte[] get(String key)throws IOException{
    File file=file(key);
    if(!file.isFile()||file.length()==0||file.length()>limit||System.currentTimeMillis()-file.lastModified()>age){file.delete();return null;}
    byte[] bytes=Files.readAllBytes(file.toPath());file.setLastModified(System.currentTimeMillis());return bytes;
  }
  void put(String key,byte[] bytes)throws IOException{
    if(bytes.length>limit)return;
    File target=file(key),temp=new File(directory,key+".part");
    try{Files.write(temp.toPath(),bytes);Files.move(temp.toPath(),target.toPath(),StandardCopyOption.REPLACE_EXISTING);}
    finally{temp.delete();}
    trim();
  }
  void remove(String prefix){File[] files=directory.listFiles();if(files!=null)for(File f:files)if(f.isFile()&&f.getName().startsWith(prefix))f.delete();}
  void trim(){
    File[] files=directory.listFiles();if(files==null)return;
    Arrays.sort(files,Comparator.comparingLong(File::lastModified).reversed());
    long used=0,now=System.currentTimeMillis();
    for(File file:files){if(!file.isFile())continue;long size=file.length();
      if(file.getName().endsWith(".part")||now-file.lastModified()>age||used+size>limit)file.delete();else used+=size;
    }
  }
}
