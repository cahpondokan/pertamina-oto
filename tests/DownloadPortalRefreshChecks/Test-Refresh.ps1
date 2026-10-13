param([string]$Publish,[string]$Fixture,[string]$Evidence,[int]$Port=5193,[switch]$KeepPreview)
$ErrorActionPreference='Stop'
$Publish=[IO.Path]::GetFullPath($Publish); $Evidence=[IO.Path]::GetFullPath($Evidence)
$feed=Join-Path $Evidence 'fixture-releases'; New-Item -ItemType Directory -Force $feed | Out-Null
$value=Get-Content -Raw -LiteralPath $Fixture | ConvertFrom-Json
$value.notes=@('Fixture sintetis — bukan rilis nyata.','Catatan <script>alert(1)</script> dienkode.')
$sizes=@(128,4096,2048); $index=0
foreach($artifact in @($value.onlineInstaller,$value.offlineInstaller,$value.updatePayload)) {
 $bytes=[byte[]]::new($sizes[$index++]); [Array]::Fill[byte]($bytes,65)
 $path=Join-Path $feed $artifact.fileName; [IO.File]::WriteAllBytes($path,$bytes)
 $artifact.size=$bytes.Length; $artifact.sha256=(Get-FileHash -LiteralPath $path).Hash
}
function Catalog { $value | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath "$feed/distribution-v2.json" }
Catalog
$valid=Get-Content -Raw "$feed/distribution-v2.json"
# A valid legacy catalog exists too; the new endpoint must never leak its schema.
$legacy=@{schema=1;version=$value.version;publishedAt=$value.publishedAt;channel=$value.channel;productVersion=$value.productVersion;fileName=$value.updatePayload.fileName;size=$value.updatePayload.size;sha256=$value.updatePayload.sha256;notes=$value.notes;installerFileName=$value.offlineInstaller.fileName;installerSize=$value.offlineInstaller.size;installerSha256=$value.offlineInstaller.sha256;runtimeBundled=$true;manualInstallerOnly=$true}
$legacy | ConvertTo-Json -Depth 8 | Set-Content "$feed/current.json"
$proc=Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList @(('"'+(Join-Path $Publish 'Pertamina.Oto.Downloads.dll')+'"'),'--urls',"http://127.0.0.1:$Port",('"--Distribution:Root='+$feed+'"'),'--Distribution:FixturePreview=true') -WorkingDirectory $Publish -WindowStyle Hidden -PassThru -RedirectStandardOutput "$Evidence/server-out.txt" -RedirectStandardError "$Evidence/server-error.txt"
$handler=[Net.Http.HttpClientHandler]::new();$handler.AllowAutoRedirect=$false
$client=[Net.Http.HttpClient]::new($handler);$client.BaseAddress=[Uri]"http://127.0.0.1:$Port"
$results=[Collections.Generic.List[string]]::new()
function Check([bool]$ok,[string]$label){if(!$ok){throw $label};$results.Add('PASS '+$label)}
function Request($path,$method='GET',$range=''){$req=[Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::new($method),$path);if($range){$req.Headers.Add('Range',$range)};try{return $client.SendAsync($req).GetAwaiter().GetResult()}finally{$req.Dispose()}}
function Status($path,$expected,$label){$r=Request $path;try{Check ([int]$r.StatusCode -eq $expected) $label}finally{$r.Dispose()}}
try{
 $ready=$false;for($i=0;$i -lt 50;$i++){try{$r=Request '/health';$ready=[int]$r.StatusCode -eq 204;$r.Dispose();if($ready){break}}catch{};Start-Sleep -Milliseconds 200};Check $ready 'preview health ready'
 $endpoint='/api/releases/windows-x64/baseline-1/latest'
 $r=Request $endpoint;$metadata=$r.Content.ReadAsStringAsync().Result | ConvertFrom-Json
 Check ([int]$r.StatusCode -eq 200 -and $metadata.schema -eq 2 -and $metadata.version -eq $value.version) 'baseline metadata identity matches fixture'
 Check $r.Headers.CacheControl.NoStore 'baseline metadata no-store';$r.Dispose()
 Status '/api/releases/windows-x64/latest' 204 'legacy manual guard remains fail-safe'
 $legacy.manualInstallerOnly=$false;$legacy | ConvertTo-Json -Depth 8 | Set-Content "$feed/current.json"
 Status '/api/releases/windows-x64/latest' 204 'presence of baseline prevents unguarded legacy offer'
 $r=Request '/api/releases/windows-x64/installer';$old=$r.Content.ReadAsStringAsync().Result | ConvertFrom-Json;Check ($old.schema -eq 1 -and $old.manualInstallerOnly -and $old.installerSha256 -eq $value.offlineInstaller.sha256) 'installer compatibility endpoint uses same baseline and manual guard';$r.Dispose()
 $r=Request '/';$html=$r.Content.ReadAsStringAsync().Result
 Check ($html.Contains('FIXTURE SINTETIS') -and $html.Contains('Installer online') -and $html.Contains('Installer offline lengkap')) 'preview distinguishes fixture and download choices'
 Check ($html.Contains($value.onlineInstaller.fileName) -and $html.Contains($value.offlineInstaller.fileName) -and $html.Contains('Total termasuk EXE awal')) 'one metadata drives both links and online total'
 Check ($html.Contains('&lt;script&gt;') -and !$html.Contains('<script>')) 'notes encoded'
 $csp=$r.Headers.GetValues('Content-Security-Policy') -join ''
 Check ($csp.Contains("script-src 'self'") -and !$csp.Contains('unsafe-')) 'CSP only allows same-origin scripts';$r.Dispose()
 foreach($artifact in @($value.onlineInstaller,$value.offlineInstaller,$value.updatePayload)){
  $path='/downloads/'+$artifact.fileName;$r=Request $path;$bytes=$r.Content.ReadAsByteArrayAsync().Result
  Check ([int]$r.StatusCode -eq 200 -and $bytes.Length -eq $artifact.size -and [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)) -eq $artifact.sha256) ('actual bytes/hash '+$artifact.fileName)
  Check ($r.Content.Headers.ContentDisposition.DispositionType -eq 'attachment') 'attachment disposition';$r.Dispose()
  $r=Request $path 'HEAD';Check ([int]$r.StatusCode -eq 200 -and $r.Content.Headers.ContentLength -eq $artifact.size) 'HEAD exact size';$r.Dispose()
  $r=Request $path 'GET' 'bytes=0-9';Check ([int]$r.StatusCode -eq 206 -and $r.Content.ReadAsByteArrayAsync().Result.Length -eq 10) 'range exact bytes';$r.Dispose()
 }
 Status $endpoint 200 'all three packages valid'
 $r=Request $endpoint 'POST';Check ([int]$r.StatusCode -eq 405) 'metadata endpoint read-only';$r.Dispose()
 Status '/downloads/unlisted.exe' 404 'unlisted filename rejected'
 $value.schema=3;Catalog;Status $endpoint 204 'unknown schema fails closed'
 $value=ConvertFrom-Json $valid;$value.onlineInstaller.fileName='../other.exe';Catalog;Status $endpoint 204 'traversal metadata rejected'
 $value=ConvertFrom-Json $valid;$value.minimumUpdaterBaseline=99;Catalog;Status $endpoint 204 'unsupported baseline rejected'
 $value=ConvertFrom-Json $valid;$value.offlineInstaller.sha256='f'*64;Catalog;Status $endpoint 204 'wrong hash blocks catalog'
 Status ('/downloads/'+$value.onlineInstaller.fileName) 404 'invalid artifact invalidates all download choices'
 $valid | Set-Content "$feed/distribution-v2.json"
 $missing=Join-Path $feed $value.offlineInstaller.fileName;Move-Item -LiteralPath $missing -Destination ($missing+'.held')
 Status $endpoint 204 'missing package blocks new metadata'
 $r=Request '/';$html=$r.Content.ReadAsStringAsync().Result;Check (!$html.Contains('data-download') -and $html.Contains('Segera tersedia di sini.')) 'bad v2 never falls back to a stale legacy offer';$r.Dispose()
 Move-Item -LiteralPath ($missing+'.held') -Destination $missing
 '{broken' | Set-Content "$feed/distribution-v2.json";Status $endpoint 204 'malformed JSON fails closed';Status '/' 200 'malformed catalog keeps portal usable'
 $valid | Set-Content "$feed/distribution-v2.json"
 $value=ConvertFrom-Json $valid;$value.onlineInstaller.size++;Catalog;Status $endpoint 204 'wrong length rejected'
 $valid | Set-Content "$feed/distribution-v2.json";Status $endpoint 200 'valid catalog recovers after rejected metadata'
}catch{$results.Add('FAIL '+$_.Exception.Message);throw}
finally{$results | Set-Content "$Evidence/test-results.txt";$client.Dispose();if(!$KeepPreview -or $results.Where({$_.StartsWith('FAIL')}).Count){if(!$proc.HasExited){Stop-Process -Id $proc.Id}}else{$proc.Id | Set-Content "$Evidence/preview-pid.txt"}}
Write-Output "$($results.Count) refresh checks PASS"


