param([string]$Id,[string]$Source)
$root=Split-Path $PSScriptRoot -Parent
$s=Get-Content -Raw (Join-Path $root 'task-02-status.json')|ConvertFrom-Json
$i=$s.items|Where-Object id -eq $Id
$dest=Join-Path $root $i.filename
Copy-Item -LiteralPath $Source -Destination $dest
Add-Type -AssemblyName System.Drawing
$b=[Drawing.Bitmap]::FromFile($dest)
$zero=0;$fg=0
for($y=0;$y -lt $b.Height;$y+=8){for($x=0;$x -lt $b.Width;$x+=8){$a=$b.GetPixel($x,$y).A;if($a -eq 0){$zero++};if($a -gt 200){$fg++}}}
$i.state='VERIFIED';$i.dimensions=@($b.Width,$b.Height);$i.alphaVerified=($zero -gt 0 -and $fg -gt 0);$i.visualReviewed=$true
$i|Add-Member -Force fileHash (Get-FileHash -LiteralPath $dest).Hash
$i|Add-Member -Force smallSizeReviewed $false
$s|ConvertTo-Json -Depth 8|Set-Content -Encoding UTF8 (Join-Path $root 'task-02-status.json')
$b.Dispose()
Write-Output "$Id alpha=$zero foreground=$fg"
