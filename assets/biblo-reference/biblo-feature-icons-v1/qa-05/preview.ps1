Add-Type -AssemblyName System.Drawing
$root=Split-Path $PSScriptRoot -Parent
$s=Get-Content (Join-Path $root 'task-05-status.json') -Raw|ConvertFrom-Json
$files=@($s.items|Where-Object state -ne 'PENDING')
$sheet=New-Object System.Drawing.Bitmap 768,([int]([math]::Ceiling($files.Count/3)*176))
$g=[System.Drawing.Graphics]::FromImage($sheet)
$g.Clear([System.Drawing.Color]::White)
for($n=0;$n -lt $files.Count;$n++){
 $x=($n%3)*256; $y=[int][math]::Floor($n/3)*176
 $g.FillRectangle([System.Drawing.Brushes]::White,$x,$y,128,128)
 $g.FillRectangle([System.Drawing.Brushes]::MidnightBlue,($x+128),$y,128,128)
 $b=[System.Drawing.Bitmap]::FromFile((Join-Path $root $files[$n].filename))
 $g.DrawImage($b,$x,$y,128,128); $g.DrawImage($b,($x+128),$y,128,128)
 $g.DrawString($files[$n].id,(New-Object System.Drawing.Font 'Arial',12),[System.Drawing.Brushes]::Black,$x,($y+132))
 $b.Dispose()
}
$sheet.Save((Join-Path $PSScriptRoot 'small-light-dark-128.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$sheet.Dispose()
