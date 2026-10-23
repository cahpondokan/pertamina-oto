package id.ayani.oto.oto_scan;

final class ReaderPolicy {
  static final long CACHE_BYTES=64L*1024*1024;
  static final long CACHE_AGE_MS=7L*24*60*60*1000;
  static int[] dimensions(int width,int height,int requested,boolean detail){
    if(width<=0||height<=0||requested<=0)throw new IllegalArgumentException();
    double scale=Math.min(requested/(double)width,Math.min((detail?3800.0:2200.0)/Math.max(width,height),Math.sqrt((detail?8_000_000.0:2_000_000.0)/((double)width*height))));
    return new int[]{Math.max(1,(int)(width*scale)),Math.max(1,(int)(height*scale))};
  }
  static int[] tile(int pageWidth,int pageHeight,int fullWidth,int x,int y,int width,int height){
    if(pageWidth<=0||pageHeight<=0||fullWidth<384||fullWidth>12000||x<0||y<0||width<1||height<1||width>513||height>513)
      throw new IllegalArgumentException("Invalid tile");
    double fullHeight=Math.ceil(fullWidth*(double)pageHeight/pageWidth);
    if((long)x+width>fullWidth||(long)y+height>fullHeight)throw new IllegalArgumentException("Tile outside page");
    return new int[]{width,height};
  }
}
