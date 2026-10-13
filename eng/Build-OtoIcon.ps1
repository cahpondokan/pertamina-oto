$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationCore,PresentationFramework,WindowsBase
$xaml = @'
<Canvas xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" Width="112" Height="112">
<Canvas Canvas.Top="17">
<Path Data="M8,4 L48,13 L56,23 L64,13 L104,4 L94,25 L64,25 L56,32 L48,25 L18,25 Z" Fill="#ED2939"/>
<Ellipse Canvas.Left="16" Canvas.Top="23" Width="32" Height="32" Stroke="White" StrokeThickness="8"/>
<Ellipse Canvas.Left="64" Canvas.Top="23" Width="32" Height="32" Stroke="White" StrokeThickness="8"/>
<Path Data="M48,26 L64,26 L64,34 L60,34 L60,57 L52,57 L52,34 L48,34 Z" Fill="White"/>
<Path Data="M47,55 L56,49 L65,55 L56,68 Z" Fill="#00A6E8"/>
<Path Data="M14,51 L48,66 L56,71 L64,66 L98,51 L87,67 L56,78 L25,67 Z" Fill="#64B32E"/>
</Canvas></Canvas>
'@
$canvas = [Windows.Markup.XamlReader]::Parse($xaml)
$canvas.Measure([Windows.Size]::new(112,112))
$canvas.Arrange([Windows.Rect]::new(0,0,112,112))
$frames = @()
foreach ($size in @(16,24,32,48,64,128,256)) {
 $canvas.LayoutTransform = [Windows.Media.ScaleTransform]::new($size / 112.0, $size / 112.0)
 $canvas.Measure([Windows.Size]::new($size,$size))
 $canvas.Arrange([Windows.Rect]::new(0,0,$size,$size))
 $canvas.UpdateLayout()
 $bitmap = [Windows.Media.Imaging.RenderTargetBitmap]::new($size,$size,96,96,[Windows.Media.PixelFormats]::Pbgra32)
 $bitmap.Render($canvas)
 $encoder = [Windows.Media.Imaging.PngBitmapEncoder]::new()
 $encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
 $memory = [IO.MemoryStream]::new(); $encoder.Save($memory)
 $frames += ,@{ Size=$size; Bytes=$memory.ToArray() }; $memory.Dispose()
}
$target = Join-Path $PSScriptRoot '../src/Hosts/Pertamina.Oto.Desktop.App/Assets/oto.ico'
[IO.Directory]::CreateDirectory((Split-Path $target)) | Out-Null
$stream = [IO.File]::Create($target); $writer = [IO.BinaryWriter]::new($stream)
try {
 $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$frames.Count)
 $offset = 6 + 16 * $frames.Count
 foreach ($frame in $frames) {
  $dimension = if ($frame.Size -eq 256) { 0 } else { $frame.Size }
  $writer.Write([byte]$dimension); $writer.Write([byte]$dimension); $writer.Write([byte]0); $writer.Write([byte]0)
  $writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([uint32]$frame.Bytes.Length); $writer.Write([uint32]$offset)
  $offset += $frame.Bytes.Length
 }
 foreach ($frame in $frames) { $writer.Write([byte[]]$frame.Bytes) }
} finally { $writer.Dispose() }

[IO.File]::WriteAllBytes((Join-Path (Split-Path $target) 'oto.png'), [byte[]]$frames[-1].Bytes)
