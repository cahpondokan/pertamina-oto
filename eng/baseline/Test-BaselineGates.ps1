[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$safeTempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$fixtureRoot = [System.IO.Path]::GetFullPath((Join-Path $safeTempRoot ("oto-baseline-gates-{0}" -f [Guid]::NewGuid().ToString('N'))))
if (-not $fixtureRoot.StartsWith($safeTempRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    Write-Error 'BASELINE_GATE_TEST_ERROR=unsafe_temp_path'
    exit 130
}

$powerShellHost = Join-Path $PSHOME 'pwsh.exe'
$dependencyGate = Join-Path $PSScriptRoot 'Assert-ApprovedDependencies.ps1'
$scopeGate = Join-Path $PSScriptRoot 'Assert-Phase0Scope.ps1'
$policy = Join-Path $PSScriptRoot 'phase0-dependency-policy.json'

New-Item -ItemType Directory -Path (Join-Path $fixtureRoot 'src\BuildingBlocks\Pertamina.Oto.Contracts') -Force | Out-Null
try {
    @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  <packageSourceMapping>
    <clear />
    <packageSource key="nuget.org">
      <package pattern="Microsoft.*" />
      <package pattern="MSTest.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
'@ | Set-Content -LiteralPath (Join-Path $fixtureRoot 'NuGet.Config') -Encoding utf8NoBOM

    '<Project><ItemGroup /></Project>' | Set-Content -LiteralPath (Join-Path $fixtureRoot 'Directory.Packages.props') -Encoding utf8NoBOM
    '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>' |
        Set-Content -LiteralPath (Join-Path $fixtureRoot 'src\BuildingBlocks\Pertamina.Oto.Contracts\Pertamina.Oto.Contracts.csproj') -Encoding utf8NoBOM

    & $powerShellHost -NoProfile -File $dependencyGate -RepositoryRoot $fixtureRoot -PolicyPath $policy
    $dependencyPositiveExit = $LASTEXITCODE
    Write-Output "BASELINE_DEPENDENCY_POSITIVE_EXIT=$dependencyPositiveExit"
    if ($dependencyPositiveExit -ne 0) {
        Write-Error 'BASELINE_GATE_TEST_ERROR=approved_fixture_rejected'
        exit 131
    }

    & $powerShellHost -NoProfile -File $scopeGate -RepositoryRoot $fixtureRoot
    $scopePositiveExit = $LASTEXITCODE
    Write-Output "BASELINE_SCOPE_POSITIVE_EXIT=$scopePositiveExit"
    if ($scopePositiveExit -ne 0) {
        Write-Error 'BASELINE_GATE_TEST_ERROR=allowed_scope_rejected'
        exit 132
    }

    '<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><PackageReference Include="Unapproved.Package" Version="1.2.3" /></ItemGroup></Project>' |
        Set-Content -LiteralPath (Join-Path $fixtureRoot 'src\BuildingBlocks\Pertamina.Oto.Contracts\Pertamina.Oto.Contracts.csproj') -Encoding utf8NoBOM
    & $powerShellHost -NoProfile -File $dependencyGate -RepositoryRoot $fixtureRoot -PolicyPath $policy *> $null
    $dependencyNegativeExit = $LASTEXITCODE
    Write-Output "BASELINE_DEPENDENCY_NEGATIVE_EXIT=$dependencyNegativeExit"
    if ($dependencyNegativeExit -eq 0) {
        Write-Error 'BASELINE_GATE_TEST_ERROR=unapproved_dependency_not_blocked'
        exit 133
    }

    New-Item -ItemType Directory -Path (Join-Path $fixtureRoot 'src\Features\Forbidden') -Force | Out-Null
    'namespace Forbidden;' | Set-Content -LiteralPath (Join-Path $fixtureRoot 'src\Features\Forbidden\Feature.cs') -Encoding utf8NoBOM
    & $powerShellHost -NoProfile -File $scopeGate -RepositoryRoot $fixtureRoot *> $null
    $scopeNegativeExit = $LASTEXITCODE
    Write-Output "BASELINE_SCOPE_NEGATIVE_EXIT=$scopeNegativeExit"
    if ($scopeNegativeExit -eq 0) {
        Write-Error 'BASELINE_GATE_TEST_ERROR=forbidden_scope_not_blocked'
        exit 134
    }
}
finally {
    if (Test-Path -LiteralPath $fixtureRoot) {
        Remove-Item -LiteralPath $fixtureRoot -Recurse -Force
    }
}

Write-Output 'BASELINE_GATE_TEST_RESULT=PASS'
exit 0
