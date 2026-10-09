using System.Text.Json;
using Pertamina.Oto.Desktop.App;

var output = args.Length > 0 ? args[0] : Path.Combine(Path.GetTempPath(), "oto-window-size-checks");
Directory.CreateDirectory(output);
var checks = new List<string>();
void Check(bool ok, string name) => checks.Add((ok ? "PASS " : "FAIL ") + name);
// Independent expected physical outer dimensions, with explicitly supplied chrome.
foreach (var (scale, frameWidth, frameHeight, width, height) in new[]
{
    (1d,16,39,656,799), (1.25,20,49,820,999), (1.5,24,59,984,1199),
    (1.75,28,69,1148,1399), (2d,32,78,1312,1598)
})
{
    var result = WindowSizeBounds.Calculate(scale, frameWidth, frameHeight, 4000, 2500);
    Check(result == new WindowMinimum(width,height,false), "client DIP plus supplied restored chrome " + scale);
}
Check(WindowSizeBounds.Calculate(1,16,39,1366,720) == new WindowMinimum(656,720,true), "height-only emergency cap preserves width");
Check(WindowSizeBounds.Calculate(2,32,78,960,1032) == new WindowMinimum(960,1032,true), "both dimensions emergency cap");
Check(WindowSizeBounds.Calculate(1,16,39,656,799) == new WindowMinimum(656,799,false), "exact work area is supported");
Check(WindowSizeBounds.Calculate(1.001,16,39,4000,2500) == new WindowMinimum(657,800,false), "fractional physical pixel rounds up");
foreach (var scale in new[] { 0d,-1,double.NaN,double.PositiveInfinity })
{
    var rejected = false; try { WindowSizeBounds.Calculate(scale,16,39,1920,1032); } catch (ArgumentOutOfRangeException) { rejected = true; }
    Check(rejected,"reject invalid scale " + scale);
}
foreach (var input in new[] { (-1,39,1920,1032), (16,-1,1920,1032), (16,39,0,1032), (16,39,1920,0) })
{
    var rejected = false; try { WindowSizeBounds.Calculate(1,input.Item1,input.Item2,input.Item3,input.Item4); } catch (ArgumentOutOfRangeException) { rejected = true; }
    Check(rejected,"reject invalid frame/work area " + input);
}
var matrix = new List<object>();
foreach (var (width,height) in new[] { (1366,768),(1600,900),(1920,1080),(1920,1200),(2560,1440),(2560,1600),(3840,2160) })
foreach (var scale in new[] { 1d,1.25,1.5,1.75,2 })
{
    var chromeWidth = (int)Math.Ceiling(16 * scale); var chromeHeight = (int)Math.Ceiling(39 * scale);
    var workHeight = height - (int)Math.Ceiling(48 * scale);
    var result = WindowSizeBounds.Calculate(scale,chromeWidth,chromeHeight,width,workHeight);
    var halfWidth = width / 2;
    matrix.Add(new { Mode="SIMULATED_GEOMETRY_NOT_NATIVE_DPI_PROOF", DisplayWidthPx=width,DisplayHeightPx=height,Scale=scale,
        AssumedTaskbarPx=height-workHeight,AssumedChromePx=new { Width=chromeWidth,Height=chromeHeight },Minimum=result,
        FullWorkAreaSupportsNominalMinimum=!result.WorkAreaFallback,
        HalfScreenSupportsNominalMinimum=halfWidth >= (int)Math.Ceiling(640*scale)+chromeWidth && workHeight >= (int)Math.Ceiling(760*scale)+chromeHeight });
    Check(result.Width <= width && result.Height <= workHeight,"effective minimum fits modeled work area " + width + "x" + height + "@" + scale);
}
File.WriteAllText(Path.Combine(output,"matrix.json"),JsonSerializer.Serialize(matrix,new JsonSerializerOptions { WriteIndented=true }));
File.WriteAllLines(Path.Combine(output,"audit.txt"),checks);
foreach (var check in checks) Console.WriteLine(check);
return checks.Any(c => c.StartsWith("FAIL ",StringComparison.Ordinal)) ? 1 : 0;
