param([string]$Publish, [int]$Port = 5157, [string]$Evidence)
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
if (!$Publish) { $Publish = Join-Path $project 'artifacts/download-portal/publish' }
$Publish = [IO.Path]::GetFullPath($Publish)
$evidence = if ($Evidence) { [IO.Path]::GetFullPath($Evidence) } else { Join-Path $project ('artifacts/portal-checks-' + [Guid]::NewGuid().ToString('N')) }
$feed = Join-Path $evidence 'releases'
$portableFeed = Join-Path $evidence 'portable-releases'
New-Item -ItemType Directory -Path $feed -Force | Out-Null
New-Item -ItemType Directory -Path $portableFeed -Force | Out-Null
$results = [Collections.Generic.List[string]]::new()
function Check([bool]$ok, [string]$label) { if (!$ok) { throw $label }; $results.Add('PASS ' + $label) }
function Catalog($value) { [IO.File]::WriteAllText((Join-Path $feed 'current.json'), ($value | ConvertTo-Json -Depth 4), [Text.UTF8Encoding]::new($false)) }
$hostProcess = Start-Process -FilePath (Get-Command dotnet).Source -ArgumentList @(('"' + (Join-Path $Publish 'Pertamina.Oto.Downloads.dll') + '"'), '--urls', "http://127.0.0.1:$Port", ('"--Distribution:Root=' + $feed + '"'), ('"--Distribution:PortableRoot=' + $portableFeed + '"')) -WorkingDirectory $Publish -WindowStyle Hidden -PassThru -RedirectStandardOutput "$evidence/server-out.txt" -RedirectStandardError "$evidence/server-error.txt"
$handler = [Net.Http.HttpClientHandler]::new(); $handler.AllowAutoRedirect = $false
$client = [Net.Http.HttpClient]::new($handler); $client.BaseAddress = [Uri]"http://127.0.0.1:$Port"; $client.Timeout = [TimeSpan]::FromSeconds(15)
function Request([string]$path, [string]$method = 'GET', [string]$range = '') {
    $request = [Net.Http.HttpRequestMessage]::new([Net.Http.HttpMethod]::new($method), $path)
    if ($range) { $request.Headers.Add('Range', $range) }
    try { return $client.SendAsync($request).GetAwaiter().GetResult() } finally { $request.Dispose() }
}
try {
    $ready = $false
    for ($i=0; $i -lt 50; $i++) { try { $response = Request '/health'; $ready = [int]$response.StatusCode -eq 204; $response.Dispose(); if ($ready) { break } } catch { Start-Sleep -Milliseconds 200 } }
    Check $ready 'loopback health responds 204'
    $response = Request '/api/releases/portable/windows-x64/latest'; Check ([int]$response.StatusCode -eq 404) 'deferred portable feed unavailable'; $response.Dispose()
    $response = Request '/portable-downloads/fixture.zip'; Check ([int]$response.StatusCode -eq 404) 'deferred portable download unavailable'; $response.Dispose()
    $response = Request '/'; $html = $response.Content.ReadAsStringAsync().Result
    Check (!$html.Contains('id="portable"') -and !$html.Contains('/portable-downloads/')) 'portable promotion absent'; $response.Dispose()
    $response = Request '/api/releases/windows-x64/latest'; Check ([int]$response.StatusCode -eq 204) 'empty catalog has no fabricated release'; $response.Dispose()
    $response = Request '/'; $html = $response.Content.ReadAsStringAsync().Result; Check ($html.Contains('Segera tersedia di sini.')) 'empty page explains missing release'; $response.Dispose()
    $version = '20260930-180000-aaaaaaaa'; $name = "PERTAMINA-OTO-Windows-x64-$version.zip"
    $zip = Join-Path $feed $name
    $archive = [IO.Compression.ZipFile]::Open($zip, [IO.Compression.ZipArchiveMode]::Create)
    $writer = [IO.StreamWriter]::new($archive.CreateEntry('fixture.txt').Open()); $writer.Write('Synthetic portal test'); $writer.Dispose(); $archive.Dispose()
    $value = @{schema=1;version=$version;publishedAt=[DateTimeOffset]::UtcNow.ToString('O');channel='development';fileName=$name;size=(Get-Item -LiteralPath $zip).Length;sha256=(Get-FileHash -LiteralPath $zip).Hash;notes=@('Perbaikan <script>alert(1)</script>')}
    Catalog $value
    $response = Request '/api/releases/windows-x64/latest'; Check ([int]$response.StatusCode -eq 200) 'verified release endpoint responds 200'; Check $response.Headers.CacheControl.NoStore 'manual update metadata avoids stale cache'; $metadata = $response.Content.ReadAsStringAsync().Result | ConvertFrom-Json; Check ($metadata.version -eq $version -and $metadata.sha256 -eq $value.sha256) 'metadata matches immutable package'; $response.Dispose()
    $response = Request '/'; $html = $response.Content.ReadAsStringAsync().Result
    Check ($html.Contains("/downloads/$name") -and $html.Contains('Setup.exe')) 'download page points to actual package and installation instructions'
    Check ($html.Contains('&lt;script&gt;') -and !$html.Contains('<script>')) 'release notes HTML encoded'
    Check ($response.Headers.GetValues('X-Content-Type-Options') -contains 'nosniff') 'nosniff header present'
    Check (($response.Headers.GetValues('Content-Security-Policy') -join '').Contains("script-src 'self'")) 'restrictive CSP present'
    Check (!$response.Headers.Contains('Server')) 'server version not exposed'; $response.Dispose()
    $response = Request '/site.css'; Check ([int]$response.StatusCode -eq 200 -and $response.Content.Headers.ContentType.MediaType -eq 'text/css') 'published styles served'; $response.Dispose()
    $response = Request "/downloads/$name"; $bytes = $response.Content.ReadAsByteArrayAsync().Result
    Check ([int]$response.StatusCode -eq 200 -and [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)) -eq $value.sha256) 'download bytes match advertised SHA-256'
    Check ($response.Content.Headers.ContentDisposition.DispositionType -eq 'attachment') 'ZIP uses attachment download'; $response.Dispose()
    $response = Request "/downloads/$name" 'HEAD'; Check ([int]$response.StatusCode -eq 200 -and $response.Content.Headers.ContentLength -eq $value.size) 'HEAD reports accurate size'; $response.Dispose()
    $response = Request "/downloads/$name" 'GET' 'bytes=0-9'; Check ([int]$response.StatusCode -eq 206 -and $response.Content.ReadAsByteArrayAsync().Result.Length -eq 10) 'range download returns requested bytes'; $response.Dispose()
    $exeName = "PERTAMINA-OTO-Setup-x64-$version.exe"; $exe = Join-Path $feed $exeName
    [IO.File]::WriteAllText($exe, 'Inert EXE download fixture, never executed.')
    $value.installerFileName = $exeName; $value.installerSize = (Get-Item -LiteralPath $exe).Length; $value.installerSha256 = (Get-FileHash -LiteralPath $exe).Hash
    Catalog $value
    $response = Request '/'; $html = $response.Content.ReadAsStringAsync().Result; Check ($html.Contains("/downloads/$exeName") -and $html.Contains('EXE') -and $html.Contains('Kebutuhan komputer diperiksa otomatis.')) 'EXE becomes primary download with wizard instructions'; $response.Dispose()
    $response = Request '/api/releases/windows-x64/latest'; $metadata = $response.Content.ReadAsStringAsync().Result | ConvertFrom-Json; Check ($metadata.fileName -eq $name -and $metadata.installerFileName -eq $exeName) 'new EXE metadata preserves real ZIP contract for old clients'; $response.Dispose()
    $response = Request "/downloads/$exeName"; $bytes = $response.Content.ReadAsByteArrayAsync().Result
    Check ([int]$response.StatusCode -eq 200 -and [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)) -eq $value.installerSha256) 'EXE bytes match advertised SHA-256'
    Check ($response.Content.Headers.ContentType.MediaType -eq 'application/octet-stream' -and $response.Content.Headers.ContentDisposition.DispositionType -eq 'attachment') 'EXE has binary attachment headers'; $response.Dispose()
    $response = Request "/downloads/$exeName" 'HEAD'; Check ([int]$response.StatusCode -eq 200 -and $response.Content.Headers.ContentLength -eq $value.installerSize) 'EXE HEAD returns actual size'; $response.Dispose()
    $response = Request "/downloads/$exeName" 'GET' 'bytes=0-9'; Check ([int]$response.StatusCode -eq 206 -and $response.Content.ReadAsByteArrayAsync().Result.Length -eq 10) 'EXE download can resume using ranges'; $response.Dispose()
    $response = Request "/downloads/$name"; Check ([int]$response.StatusCode -eq 200) 'legacy ZIP remains available'; $response.Dispose()
    $value.runtimeBundled = $true; $value.manualInstallerOnly = $true; $value.productVersion = '0.2.4'; Catalog $value
    $response = Request '/api/releases/windows-x64/latest'; Check ([int]$response.StatusCode -eq 204) 'manual transition never enters legacy in-app update feed'; Check $response.Headers.CacheControl.NoStore 'transition feed is not cached'; $response.Dispose()
    $response = Request '/api/releases/windows-x64/installer'; $metadata = $response.Content.ReadAsStringAsync().Result | ConvertFrom-Json
    Check ([int]$response.StatusCode -eq 200 -and $metadata.runtimeBundled -and $metadata.manualInstallerOnly) 'installer endpoint exposes verified transition policy'
    Check ($metadata.installerSha256 -eq $value.installerSha256 -and $metadata.sha256 -eq $value.sha256) 'transition metadata preserves exact EXE and ZIP'; $response.Dispose()
    $response = Request '/'; $html = $response.Content.ReadAsStringAsync().Result
    Check ($html.Contains('tidak perlu memasang .NET 10 terpisah') -and $html.Contains('Paket uji 0.2.4')) 'website explains bundled runtime and trial status'
    Check (!$html.Contains('installer mengunduhnya dari Microsoft') -and $html.Contains('tidak ditawarkan melalui updater lama')) 'website does not promise legacy runtime installation or update'; $response.Dispose()
    $response = Request '/panduan'; $html = $response.Content.ReadAsStringAsync().Result
    Check ($html.Contains('id="uji-laptop"') -and $html.Contains('Transisi ke 0.2.4')) 'guide has exact transition and laptop checklist'
    Check (!$html.Contains('Pasang .NET dan OTO') -and !$html.Contains('Alternatif feed lokal')) 'guide suppresses incompatible legacy steps'; $response.Dispose()
    $response = Request "/downloads/$exeName" 'HEAD'; Check ([int]$response.StatusCode -eq 200 -and $response.Content.Headers.ContentLength -eq $value.installerSize) 'manual installer remains downloadable'; $response.Dispose()
    $response = Request '/api/releases/windows-x64/installer' 'POST'; Check ([int]$response.StatusCode -eq 405) 'installer metadata has no write method'; $response.Dispose()
    $savedInstaller = @{installerFileName=$value.installerFileName;installerSize=$value.installerSize;installerSha256=$value.installerSha256}
    foreach ($key in $savedInstaller.Keys) { $value.Remove($key) }; Catalog $value
    $response = Request '/api/releases/windows-x64/installer'; Check ([int]$response.StatusCode -eq 204) 'manual release without full installer is rejected'; $response.Dispose()
    foreach ($key in $savedInstaller.Keys) { $value[$key]=$savedInstaller[$key] }
    $value.manualInstallerOnly='true'; Catalog $value
    $response = Request '/api/releases/windows-x64/installer'; Check ([int]$response.StatusCode -eq 204) 'invalid policy type fails closed'; $response.Dispose()
    $value.Remove('runtimeBundled'); $value.Remove('manualInstallerOnly'); $value.Remove('productVersion'); Catalog $value
    $response = Request '/api/releases/windows-x64/latest'; Check ([int]$response.StatusCode -eq 200) 'ordinary compatible releases retain legacy update behavior'; $response.Dispose()
    $value.installerSize = 0; Catalog $value; $response = Request '/api/releases/windows-x64/latest'; Check ([int]$response.StatusCode -eq 204) 'partial EXE metadata rejected'; $response.Dispose()
    $value.installerSize = (Get-Item -LiteralPath $exe).Length; $value.installerSha256 = 'b' * 64; Catalog $value
    $response = Request "/downloads/$exeName"; Check ([int]$response.StatusCode -eq 404) 'incorrect EXE hash blocks download'; $response.Dispose()
    $value.installerSha256 = (Get-FileHash -LiteralPath $exe).Hash; Catalog $value
    [IO.File]::AppendAllText($exe, 'tampered'); $response = Request '/api/releases/windows-x64/latest'; Check ([int]$response.StatusCode -eq 204) 'tampered EXE invalidates verified catalog'; $response.Dispose()
    $value.Remove('installerFileName'); $value.Remove('installerSize'); $value.Remove('installerSha256'); Catalog $value
    $response = Request '/downloads/other.zip'; Check ([int]$response.StatusCode -eq 404) 'unlisted download rejected'; $response.Dispose()
    $response = Request '/api/releases/windows-x64/latest' 'POST'; Check ([int]$response.StatusCode -eq 405) 'release endpoint has no public write method'; $response.Dispose()
    $value.fileName = '../private.zip'; Catalog $value; $response = Request '/api/releases/windows-x64/latest'; Check ([int]$response.StatusCode -eq 204) 'unsafe catalog path rejected without server error'; $response.Dispose()
    $value.fileName = $name; $value.schema = 2; Catalog $value; $response = Request '/api/releases/windows-x64/latest'; Check ([int]$response.StatusCode -eq 204) 'unknown schema rejected'; $response.Dispose()
    $value.schema = 1; $value.sha256 = 'a' * 64; Catalog $value; $response = Request "/downloads/$name"; Check ([int]$response.StatusCode -eq 404) 'damaged package is not served'; $response.Dispose()
    [IO.File]::WriteAllText((Join-Path $feed 'current.json'), '{broken'); $response = Request '/'; Check ([int]$response.StatusCode -eq 200) 'malformed metadata keeps landing page usable'; $response.Dispose()
    $results | Set-Content -LiteralPath "$evidence/test-results.txt"
    Write-Output "$($results.Count) portal checks PASS: $evidence"
} catch { $results.Add('FAIL ' + $_.Exception.Message); $results | Set-Content -LiteralPath "$evidence/test-results.txt"; throw }
finally { $client.Dispose(); if (!$hostProcess.HasExited) { Stop-Process -Id $hostProcess.Id } }

