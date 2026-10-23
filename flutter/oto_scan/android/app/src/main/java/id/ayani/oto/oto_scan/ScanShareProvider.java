package id.ayani.oto.oto_scan;
import android.content.*;
import android.database.*;
import android.net.Uri;
import android.os.ParcelFileDescriptor;
import android.provider.OpenableColumns;
import java.io.*;

public final class ScanShareProvider extends ContentProvider {
  @Override public boolean onCreate(){return true;}
  private File resolve(Uri uri) throws FileNotFoundException {
    String token=uri.getLastPathSegment();
    if(!"share".equals(uri.getPathSegments().isEmpty()?"":uri.getPathSegments().get(0))||uri.getPathSegments().size()!=2||token==null||!token.matches("[a-f0-9-]{36}\\.pdf"))throw new FileNotFoundException();
    File f=new File(new File(getContext().getCacheDir(),"share"),token);if(!f.isFile())throw new FileNotFoundException();return f;
  }
  @Override public ParcelFileDescriptor openFile(Uri uri,String mode) throws FileNotFoundException {
    if(!"r".equals(mode))throw new FileNotFoundException("Read only");return ParcelFileDescriptor.open(resolve(uri),ParcelFileDescriptor.MODE_READ_ONLY);
  }
  @Override public String getType(Uri uri){return "application/pdf";}
  @Override public Cursor query(Uri uri,String[] projection,String selection,String[] args,String sort){
    try{File f=resolve(uri);String[] columns=projection==null?new String[]{OpenableColumns.DISPLAY_NAME,OpenableColumns.SIZE}:projection;
      String display="OTO Scan.pdf";File name=new File(f.getParentFile(),f.getName()+".name");
      if(name.isFile()&&name.length()<1024){try{display=new String(java.nio.file.Files.readAllBytes(name.toPath()),java.nio.charset.StandardCharsets.UTF_8);}catch(IOException ignored){}}
      MatrixCursor c=new MatrixCursor(columns);Object[] row=new Object[columns.length];
      for(int i=0;i<columns.length;i++)row[i]=OpenableColumns.DISPLAY_NAME.equals(columns[i])?display:OpenableColumns.SIZE.equals(columns[i])?f.length():null;
      c.addRow(row);return c;
    }catch(FileNotFoundException e){return null;}
  }
  @Override public Uri insert(Uri u,ContentValues v){throw new UnsupportedOperationException();}
  @Override public int update(Uri u,ContentValues v,String s,String[] a){throw new UnsupportedOperationException();}
  @Override public int delete(Uri u,String s,String[] a){throw new UnsupportedOperationException();}
}
