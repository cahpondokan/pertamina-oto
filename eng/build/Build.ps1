[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$expectedSdk = '10.0.400'
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$solution = Join-Path $repositoryRoot 'Pertamina.Oto.sln'
$nugetConfig = Join-Path $repositoryRoot 'NuGet.Config'

Push-Location $repositoryRoot
try {
    $actualSdk = (& dotnet --version).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to query the installed .NET SDK."
    }

    if ($actualSdk -ne $expectedSdk) {
        throw "Expected .NET SDK $expectedSdk but found $actualSdk."
    }

    & dotnet restore $solution --configfile $nugetConfig --locked-mode
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE."
    }

    & dotnet build $solution --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE."
    }
}
finally {
    Pop-Location
}
