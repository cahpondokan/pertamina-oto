package id.ayani.oto.oto_scan;

import java.io.IOException;

/** Streaming document operations. No OCR, glyph substitution or generated detail. */
public final class AdaptiveDocument {
  public interface Rows { void read(int y,int[] row) throws IOException; }
  public interface Output { void write(int y,int[] row) throws IOException; }
  static int gray(int p){return ScanQuality.luminance(p);}
  static int clamp(int value){return Math.max(0,Math.min(255,value));}

  public static void normalize(int w,int h,Rows input,Output output)throws IOException{
    int cell=Math.max(64,(Math.max(w,h)+47)/48),cols=(w+cell-1)/cell,rows=(h+cell-1)/cell;
    int[][] hist=new int[cols*rows][256];int[] count=new int[hist.length],line=new int[w];
    for(int y=0;y<h;y+=4){input.read(y,line);for(int x=0;x<w;x+=4){int i=(y/cell)*cols+x/cell;hist[i][gray(line[x])]++;count[i]++;}}
    double[] paper=new double[hist.length];
    for(int i=0;i<paper.length;i++){
      int n=0,v=255;for(int j=0;j<256;j++){n+=hist[i][j];if(n>=Math.max(1,count[i]*9/10)){v=j;break;}}
      // A dark/photo cell is not evidence of shadowed white paper.
      paper[i]=v<145?250:Math.max(145,v);
    }
    for(int y=0;y<h;y++){
      input.read(y,line);double gy=Math.max(0,Math.min(rows-1,y/(double)cell-.5));int y0=(int)gy,y1=Math.min(rows-1,y0+1);double fy=gy-y0;
      for(int x=0;x<w;x++){
        double gx=Math.max(0,Math.min(cols-1,x/(double)cell-.5));int x0=(int)gx,x1=Math.min(cols-1,x0+1);double fx=gx-x0;
        double top=paper[y0*cols+x0]*(1-fx)+paper[y0*cols+x1]*fx;
        double bottom=paper[y1*cols+x0]*(1-fx)+paper[y1*cols+x1]*fx;
        double gain=Math.min(1.7,250/(top*(1-fy)+bottom*fy));
        int p=line[x];line[x]=0xff000000|(clamp((int)Math.round(((p>>16)&255)*gain))<<16)|(clamp((int)Math.round(((p>>8)&255)*gain))<<8)|clamp((int)Math.round((p&255)*gain));
      }
      output.write(y,line);
    }
  }

  public static void enhance(int w,int h,Rows input,Output output)throws IOException{
    normalize(w,h,input,output);
    int[] above=new int[w],center=new int[w],below=new int[w],result=new int[w];
    input.read(0,center);System.arraycopy(center,0,above,0,w);
    ScanQuality.Tone tone=new ScanQuality.Tone(0,1);
    for(int y=0;y<h;y++){
      if(y+1<h)input.read(y+1,below);else System.arraycopy(center,0,below,0,w);
      ScanQuality.row(above,center,below,result,tone);output.write(y,result);
      int[] reuse=above;above=center;center=below;below=reuse;
    }
  }

  /** Sauvola local threshold with bounded row storage, safe for in-place output. */
  public static void binarize(int w,int h,Rows input,Output output)throws IOException{
    int radius=Math.max(8,Math.min(32,Math.min(w,h)/80)),window=radius*2+1;
    int[][] ring=new int[window][w];long[] sum=new long[w],square=new long[w];int[] pixels=new int[w],result=new int[w];
    int loaded=-1,removed=-1;
    for(int y=0;y<h;y++){
      int first=Math.max(0,y-radius),last=Math.min(h-1,y+radius);
      while(removed<first-1){int[] row=ring[(++removed)%window];for(int x=0;x<w;x++){sum[x]-=row[x];square[x]-=(long)row[x]*row[x];}}
      while(loaded<last){int yy=++loaded;input.read(yy,pixels);int[] row=ring[yy%window];for(int x=0;x<w;x++){int v=gray(pixels[x]);row[x]=v;sum[x]+=v;square[x]+=(long)v*v;}}
      long s=0,ss=0;int right=-1,left=0;
      for(int x=0;x<w;x++){
        int nextRight=Math.min(w-1,x+radius),nextLeft=Math.max(0,x-radius);
        while(right<nextRight){right++;s+=sum[right];ss+=square[right];}
        while(left<nextLeft){s-=sum[left];ss-=square[left++];}
        int n=(last-first+1)*(right-left+1);double mean=s/(double)n,std=Math.sqrt(Math.max(0,ss/(double)n-mean*mean));
        double threshold=mean*(1+.14*(std/128-1));
        result[x]=ring[y%window][x]<threshold?0xff000000:0xffffffff;
      }
      output.write(y,result);
    }
  }

  /** Edge spread ratio: advisory only; blank pages are not rejected as blurred. */
  public static boolean blurred(int w,int h,Rows input)throws IOException{
    if(w<32||h<32)return false;
    int[] a=new int[w],b=new int[w],c=new int[w];double gradient=0,laplacian=0;int edges=0;
    for(int y=Math.max(2,h/20);y<h-h/20-1;y+=3){
      input.read(y-1,a);input.read(y,b);input.read(y+1,c);
      for(int x=Math.max(2,w/20);x<w-w/20-1;x+=2){
        int center=gray(b[x]);int gx=gray(b[x+1])-gray(b[x-1]),gy=gray(c[x])-gray(a[x]);
        double g=(double)gx*gx+(double)gy*gy;if(g<64)continue;
        int lap=4*center-gray(a[x])-gray(c[x])-gray(b[x-1])-gray(b[x+1]);
        gradient+=g;laplacian+=(double)lap*lap;edges++;
      }
    }
    return edges>Math.max(40,w*h/8000)&&gradient>0&&laplacian/gradient<.10;
  }
}
