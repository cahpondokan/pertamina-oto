param([string]$Publish,[string]$Release,[string]$Evidence,[int]$Port=5195)
$ErrorActionPreference='Stop';$Publish=[IO.Path]::GetFullPath($Publish);$Release=[IO.Path]::GetFullPath($Release);$Evidence=[IO.Path]::GetFullPath($Evidence)
New-Item -ItemType Directory -Force $Evidence | Out-Null
$value=Get-Content -Raw -LiteralPath (Join-Path $Release current.json) | ConvertFrom-Json
$proc=Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList @(('"'+(Join-Path $Publish 'Pertamina.Oto.Downloads.dll')+'"'),'--urls',"http://127.0.0.1:$Port",('"--Distribution:Root='+$Release+'"')) -WorkingDirectory $Publish -WindowStyle Hidden -PassThru -RedirectStandardOutput "$Evidence/server-out.txt" -RedirectStandardError "$Evidence/server-error.txt"
$client=[Net.Http.HttpClient]::new();$client.BaseAddress=[Uri]"http://127.0.0.1:$Port";$client.Timeout=[TimeSpan]::FromSeconds(120)
$results=[Collections.Generic.List[string]]::new()
function Check([bool]$ok,[string]$label){if(!$ok){throw $label};$results.Add('PASS '+$label)}
function Request($path,$method='GET',$range=''){$req=[Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::new($method),$path);if($range){$req.Headers.Add('Range',$range)};try{return $client.SendAsync($req).GetAwaiter().GetResult()}finally{$req.Dispose()}}
try{
 $ready=$false;for($i=0;$i -lt 50;$i++){try{$r=Request '/health';$ready=[int]$r.StatusCode -eq 204;$r.Dispose();if($ready){break}}catch{};Start-Sleep -Milliseconds 200};Check $ready 'existing real package host ready'
 $r=Request '/';$html=$r.Content.ReadAsStringAsync().Result;Check ($html.Contains($value.version) -and $html.Contains('Installer online belum tersedia') -and !$html.Contains('OnlineSetup')) 'real 0.2.4 full installer correctly remains fallback';$r.Dispose()
 $r=Request '/api/releases/windows-x64/latest';Check ([int]$r.StatusCode -eq 204) 'real manual transition legacy update closed';$r.Dispose()
 $r=Request '/api/releases/windows-x64/baseline-1/latest';Check ([int]$r.StatusCode -eq 204) 'no invented new baseline for legacy release';$r.Dispose()
 foreach($artifact in @(@{fileName=$value.installerFileName;size=$value.installerSize;sha256=$value.installerSha256},@{fileName=$value.fileName;size=$value.size;sha256=$value.sha256})){
  $path='/downloads/'+$artifact.fileName;$r=Request $path;$stream=$r.Content.ReadAsStreamAsync().Result;$hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream));$stream.Dispose()
  Check ([int]$r.StatusCode -eq 200 -and $r.Content.Headers.ContentLength -eq $artifact.size -and $hash -eq $artifact.sha256) ('full real HTTP bytes/SHA256 '+$artifact.fileName);$r.Dispose()
  $r=Request $path 'HEAD';Check ([int]$r.StatusCode -eq 200 -and $r.Content.Headers.ContentLength -eq $artifact.size) 'real HEAD exact size';$r.Dispose()
  $r=Request $path 'GET' 'bytes=0-9';Check ([int]$r.StatusCode -eq 206 -and $r.Content.ReadAsByteArrayAsync().Result.Length -eq 10) 'real range supported';$r.Dispose()
 }
}catch{$results.Add('FAIL '+$_.Exception.Message);throw}finally{$results | Set-Content "$Evidence/test-results.txt";$client.Dispose();if(!$proc.HasExited){Stop-Process -Id $proc.Id}}
Write-Output "$($results.Count) real package checks PASS"
