param([string]$RepositoryRoot = ([IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))))
$ErrorActionPreference = 'Stop'
try {
    $allowed = '^src/(BuildingBlocks/Pertamina\.Oto\.(Contracts|Security|Observability|Storage\.Abstractions|Processing\.Abstractions)|Modules/Pdf/Pertamina\.Oto\.Modules\.Pdf\.(Contracts|Domain|Application)|Hosts/Pertamina\.Oto\.Desktop)/'
    foreach ($file in Get-ChildItem $RepositoryRoot -Recurse -File -Force) {
        $p = [IO.Path]::GetRelativePath($RepositoryRoot,$file.FullName).Replace('\','/')
        if ($p -match '(^|/)(\.git|bin|obj|artifacts)(/|$)') { continue }
        if ($p -match '(^|/)(deploy|deployment|production)(/|[.])|(^|/)appsettings\.Production\.' -or ($p.StartsWith('src/') -and $p -notmatch $allowed)) { throw 'scope' }
        if ($p.StartsWith('src/') -and $file.Extension -in @('.cs','.csproj','.props','.targets')) {
            $body = Get-Content -Raw -LiteralPath $file.FullName
            if ($body -match 'Microsoft\.NET\.Sdk\.(Web|Razor)|Microsoft\.AspNetCore|UseMaui|net\d+\.\d+-(android|ios)|\b(HttpClient|WebApplication|HubConnection|MapPost|MapGet)\b') { throw 'platform' }
            if ($file.Extension -eq '.csproj') {
                [xml]$xml = $body
                if ($xml.SelectNodes("//*[local-name()='PackageReference' or local-name()='Reference' or local-name()='COMReference']").Count -gt 0) { throw 'provider' }
            }
        }
    }
    'PHASE1_SCOPE_RESULT=PASS'; exit 0
} catch { 'PHASE1_SCOPE_RESULT=FAIL'; exit 1 }
