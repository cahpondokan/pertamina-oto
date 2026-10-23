package id.ayani.oto.oto_scan;

import java.nio.ByteBuffer;

/** Stride-aware YUV420 conversion directly from camera planes, independent of GPU copies. */
public final class PreviewPixels {
  public static final class Plane {
    final ByteBuffer bytes;final int row,pixel,base;
    public Plane(ByteBuffer bytes,int rowStride,int pixelStride){
      if(rowStride<1||pixelStride<1)throw new IllegalArgumentException();
      this.bytes=bytes.duplicate();row=rowStride;pixel=pixelStride;base=bytes.position();
    }
    int at(int x,int y){return bytes.get(base+y*row+x*pixel)&255;}
  }
  public static final class Frame {
    public final int[] pixels;public final int width,height;
    Frame(int[] pixels,int width,int height){this.pixels=pixels;this.width=width;this.height=height;}
  }
  static int clamp(int v){return Math.max(0,Math.min(255,v));}
  public static Frame convert(Plane y,Plane u,Plane v,int cropLeft,int cropTop,int cropWidth,int cropHeight,int rotation,int viewWidth,int viewHeight){
    if(cropLeft<0||cropTop<0||cropWidth<2||cropHeight<2||viewWidth<1||viewHeight<1||rotation%90!=0)throw new IllegalArgumentException();
    rotation=((rotation%360)+360)%360;
    int rw=rotation%180==0?cropWidth:cropHeight,rh=rotation%180==0?cropHeight:cropWidth;
    double fit=Math.max(viewWidth/(double)rw,viewHeight/(double)rh);
    double visibleW=viewWidth/fit,visibleH=viewHeight/fit;
    double scale=Math.min(1,320.0/Math.max(viewWidth,viewHeight));
    int w=Math.max(2,(int)Math.round(viewWidth*scale)),h=Math.max(2,(int)Math.round(viewHeight*scale));
    int[] pixels=new int[w*h];
    for(int oy=0;oy<h;oy++)for(int ox=0;ox<w;ox++){
      int rx=Math.min(rw-1,(int)((rw-visibleW)/2+(ox+.5)*visibleW/w));
      int ry=Math.min(rh-1,(int)((rh-visibleH)/2+(oy+.5)*visibleH/h));
      int sx,sy;
      if(rotation==90){sx=ry;sy=cropHeight-1-rx;}
      else if(rotation==180){sx=cropWidth-1-rx;sy=cropHeight-1-ry;}
      else if(rotation==270){sx=cropWidth-1-ry;sy=rx;}
      else{sx=rx;sy=ry;}
      sx+=cropLeft;sy+=cropTop;
      int c=Math.max(0,y.at(sx,sy)-16),d=u.at(sx/2,sy/2)-128,e=v.at(sx/2,sy/2)-128;
      pixels[oy*w+ox]=0xff000000|(clamp((298*c+409*e+128)>>8)<<16)|(clamp((298*c-100*d-208*e+128)>>8)<<8)|clamp((298*c+516*d+128)>>8);
    }
    return new Frame(pixels,w,h);
  }
}
