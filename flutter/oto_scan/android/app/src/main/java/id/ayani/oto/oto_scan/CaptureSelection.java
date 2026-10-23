package id.ayani.oto.oto_scan;

/** Immutable selection for one shutter request, independent of the AF/AE wait. */
public final class CaptureSelection {
  private final DetectionTarget target;
  private final int viewWidth,viewHeight;

  public CaptureSelection(DetectionTarget requested,long frame,long pressedAt,
      int width,int height,boolean matchingGeometry) {
    viewWidth=width;viewHeight=height;
    target=requested==null?null:!matchingGeometry||width<1||height<1?
      DetectionTarget.manual():requested.atShutter(frame,pressedAt);
  }

  public DetectionTarget forPhoto(int width,int height,boolean matchingGeometry) {
    if(target==null)return null;
    if(!matchingGeometry||width!=viewWidth||height!=viewHeight)return DetectionTarget.manual();
    // Flutter pauses live detection while capture is busy. Do not expire the
    // accepted guide again after focus waiting (up to 1400 ms). It does not
    // authorize a crop on its own: ScanStore still detects/verifies the photo.
    return target;
  }
}
