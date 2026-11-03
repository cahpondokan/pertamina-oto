$ErrorActionPreference = 'Stop'
$evidence = 'D:/AFT Ahmad Yani Semarang/Teknik/Dokumentasi Pertamina OTO/artifacts/web-mvc-preview-20261016/browser-pdf'
$vendor = Join-Path $evidence 'upstream-pdf-lib-1.17.1'
New-Item -ItemType Directory -Force -Path $vendor | Out-Null
$archive = Join-Path $evidence 'pdf-lib-1.17.1.tgz'
Invoke-WebRequest 'https://registry.npmjs.org/pdf-lib/-/pdf-lib-1.17.1.tgz' -OutFile $archive
$hash = [Convert]::ToBase64String([System.Security.Cryptography.SHA512]::HashData([IO.File]::ReadAllBytes($archive)))
if ($hash -cne 'V/mpyJAoTsN4cnP31vc0wfNA1+p20evqqnap0KLoRUN0Yk/p3wN52DOEsL4oBFcLdb76hlpKPtzJIgo67j/XLw==') { throw 'Archive integrity mismatch' }
# Extract exact regular members only. No dependency install or package lifecycle scripts.
tar -xzf $archive -C $vendor --strip-components=2 package/dist/pdf-lib.js
if ($LASTEXITCODE) { throw 'Bundle extraction failed' }
tar -xzf $archive -C $vendor --strip-components=1 package/LICENSE.md package/package.json package/yarn.lock
if ($LASTEXITCODE) { throw 'Notice extraction failed' }
$notices = @{
 'standard-fonts-MIT.txt' = 'https://raw.githubusercontent.com/Hopding/standard-fonts/master/LICENSE.md'
 'upng-MIT.txt' = 'https://raw.githubusercontent.com/Hopding/upng/master/LICENSE'
 'pako-MIT.txt' = 'https://raw.githubusercontent.com/nodeca/pako/1.0.11/LICENSE'
 'tslib-Apache-2.0.txt' = 'https://raw.githubusercontent.com/microsoft/tslib/1.11.1/LICENSE.txt'
 'tslib-copyright.txt' = 'https://raw.githubusercontent.com/microsoft/tslib/1.11.1/CopyrightNotice.txt'
}
foreach ($entry in $notices.GetEnumerator()) { Invoke-WebRequest $entry.Value -OutFile (Join-Path $vendor $entry.Key) }
Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $vendor 'pdf-lib.js'),$archive | Select-Object Algorithm,Hash,Path
