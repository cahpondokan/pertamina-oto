package id.ayani.oto.oto_scan;

/** Output geometry, independent of image enhancement and compression. */
public final class PageGeometry {
  public static double[] pdfSize(int width,int height){
    if(width<1||height<1)throw new IllegalArgumentException("Invalid page size");
    double scale=842.0/Math.max(width,height);
    return new double[]{width*scale,height*scale};
  }
  public static boolean validMode(String mode){return "free".equals(mode)||"a4".equals(mode)||"a4-landscape".equals(mode)||"card".equals(mode)||"card-portrait".equals(mode);}
  /** Presets constrain shape, never enlarge beyond either measured source edge. */
  public static int[] cropSize(double width,double height,String mode){
    return cropSize(width,height,mode,0);
  }
  public static int[] cropSize(double width,double height,String mode,int turns){
    if(turns<0||turns>3)throw new IllegalArgumentException("Invalid rotation");
    if(!validMode(mode)||!Double.isFinite(width)||!Double.isFinite(height)||width<1||height<1)throw new IllegalArgumentException("Invalid crop size");
    if(!"free".equals(mode)){
      double ratio="a4".equals(mode)?210.0/297:"a4-landscape".equals(mode)?297.0/210:"card".equals(mode)?85.6/53.98:53.98/85.6;
      // Mode describes the final image; the warp precedes the user's rotation.
      if(turns%2!=0)ratio=1/ratio;
      if(width/height>ratio)width=height*ratio;else height=width/ratio;
    }
    return new int[]{Math.max(1,(int)Math.round(width)),Math.max(1,(int)Math.round(height))};
  }
}
