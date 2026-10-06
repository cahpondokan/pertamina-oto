$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationCore,PresentationFramework,WindowsBase
$root = Split-Path $PSScriptRoot -Parent
$path = Join-Path $root 'src/Hosts/Pertamina.Oto.Desktop.App/Assets/oto.ico'
$bytes = [IO.File]::ReadAllBytes($path)
$expected = @(16,24,32,48,64,128,256)
if ([BitConverter]::ToUInt16($bytes,2) -ne 1 -or [BitConverter]::ToUInt16($bytes,4) -ne 7) { throw 'Invalid ICO directory' }
$visual = [Windows.Media.DrawingVisual]::new(); $drawing = $visual.RenderOpen()
foreach ($row in @(0,1)) {
 $color = if ($row -eq 0) { '#F4F4F4' } else { '#071D3D' }
 $drawing.DrawRectangle([Windows.Media.BrushConverter]::new().ConvertFromString($color),$null,[Windows.Rect]::new(0,$row*300,700,300))
}
$x=16
for ($i=0;$i -lt 7;$i++) {
 $entry=6+16*$i; $size=$expected[$i]
 $dimension=[int]$bytes[$entry]; if ($dimension -eq 0) { $dimension=256 }
 if ($dimension -ne $size -or [BitConverter]::ToUInt16($bytes,$entry+6) -ne 32) { throw 'Invalid frame size/depth' }
 $length=[BitConverter]::ToUInt32($bytes,$entry+8); $offset=[BitConverter]::ToUInt32($bytes,$entry+12)
 $memory=[IO.MemoryStream]::new($bytes,$offset,$length)
 $decoder=[Windows.Media.Imaging.PngBitmapDecoder]::new($memory,[Windows.Media.Imaging.BitmapCreateOptions]::PreservePixelFormat,[Windows.Media.Imaging.BitmapCacheOption]::OnLoad)
 $frame=$decoder.Frames[0]; $memory.Dispose()
 if ($frame.PixelWidth -ne $size -or $frame.PixelHeight -ne $size) { throw 'Invalid PNG dimensions' }
 $bgra=[Windows.Media.Imaging.FormatConvertedBitmap]::new($frame,[Windows.Media.PixelFormats]::Bgra32,$null,0)
 $pixels=[byte[]]::new($size*$size*4); $bgra.CopyPixels($pixels,$size*4,0)
 $transparent=0; $opaque=0; $partial=0
 for ($j=3;$j -lt $pixels.Length;$j+=4) { if ($pixels[$j] -eq 0) { $transparent++ } elseif ($pixels[$j] -eq 255) { $opaque++ } else { $partial++ } }
 if ($pixels[3] -ne 0 -or $pixels[$pixels.Length-1] -ne 0 -or $transparent -eq 0 -or $opaque -eq 0 -or $partial -eq 0) { throw 'Invalid alpha coverage' }
 foreach ($row in @(0,1)) { $drawing.DrawImage($frame,[Windows.Rect]::new($x,$row*300+16,$size,$size)) }
 Write-Output "PASS ${size}px: transparent=$transparent opaque=$opaque antialiased=$partial"
 $x += $size+16
}
$drawing.Close()
$bitmap=[Windows.Media.Imaging.RenderTargetBitmap]::new(700,600,96,96,[Windows.Media.PixelFormats]::Pbgra32); $bitmap.Render($visual)
$encoder=[Windows.Media.Imaging.PngBitmapEncoder]::new(); $encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
$evidence=Join-Path $root 'artifacts/icon-audit'; [IO.Directory]::CreateDirectory($evidence) | Out-Null
$stream=[IO.File]::Create((Join-Path $evidence 'light-dark-native-sizes.png')); try { $encoder.Save($stream) } finally { $stream.Dispose() }