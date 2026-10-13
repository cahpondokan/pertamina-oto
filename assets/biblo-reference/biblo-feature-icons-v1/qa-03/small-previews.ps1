Add-Type -AssemblyName System.Drawing
$assetRoot = Split-Path $PSScriptRoot -Parent
$manifest = Get-Content -Raw (Join-Path $assetRoot 'manifest.json') | ConvertFrom-Json
$laneItems = @($manifest.items | Where-Object lane -eq 3)
for ($pageIndex=0; $pageIndex -lt 4; $pageIndex++) {
 $board = New-Object System.Drawing.Bitmap 640,960
 $g = [System.Drawing.Graphics]::FromImage($board)
 $g.Clear([System.Drawing.Color]::White)
 $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
 $dark = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(24,32,48))
 $font = New-Object System.Drawing.Font 'Arial',12
 for ($rowIndex=0; $rowIndex -lt 6; $rowIndex++) {
  $itemIndex=$pageIndex*6+$rowIndex
  if ($itemIndex -ge $laneItems.Count) { break }
  $item=$laneItems[$itemIndex]
  $imagePath=Join-Path $assetRoot $item.filename
  if (-not (Test-Path -LiteralPath $imagePath)) { continue }
  $im=[System.Drawing.Image]::FromFile($imagePath)
  $y=$rowIndex*160
  $g.FillRectangle($dark,320,$y,320,160)
  $g.DrawString($item.id,$font,[System.Drawing.Brushes]::Black,2,$y)
  $g.DrawString($item.id,$font,[System.Drawing.Brushes]::White,322,$y)
  $g.DrawImage($im,40,($y+45),64,64)
  $g.DrawImage($im,150,($y+20),128,128)
  $g.DrawImage($im,360,($y+45),64,64)
  $g.DrawImage($im,470,($y+20),128,128)
  $im.Dispose()
 }
 $board.Save((Join-Path $PSScriptRoot ('small-preview-'+$pageIndex+'.png')),[System.Drawing.Imaging.ImageFormat]::Png)
 $font.Dispose(); $dark.Dispose(); $g.Dispose(); $board.Dispose()
}
