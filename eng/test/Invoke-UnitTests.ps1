[CmdletBinding()]
param(
    [string]$Project,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$ResultsDirectory,
    [switch]$NoRestore,
    [switch]$NoBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
if ([string]::IsNullOrWhiteSpace($Project)) {
    $Project = Join-Path $repositoryRoot 'tests\Unit\Pertamina.Oto.Foundation.UnitTests\Pertamina.Oto.Foundation.UnitTests.csproj'
}
elseif (-not [System.IO.Path]::IsPathRooted($Project)) {
    $Project = Join-Path $repositoryRoot $Project
}

if ([string]::IsNullOrWhiteSpace($ResultsDirectory)) {
    $runId = [DateTimeOffset]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
    $ResultsDirectory = Join-Path $repositoryRoot "artifacts\test-results\unit-$runId"
}
elseif (-not [System.IO.Path]::IsPathRooted($ResultsDirectory)) {
    $ResultsDirectory = Join-Path $repositoryRoot $ResultsDirectory
}

if (-not (Test-Path -LiteralPath $Project -PathType Leaf)) {
    Write-Error 'UNIT_TEST_ERROR=project_missing'
    exit 30
}

New-Item -ItemType Directory -Path $ResultsDirectory -Force | Out-Null

$projectDirectory = Split-Path -Parent $Project
$projectName = [System.IO.Path]::GetFileNameWithoutExtension($Project)

if (-not $NoBuild) {
    $buildArguments = @(
        'build'
        $Project
        '--configuration'
        $Configuration
        '--nologo'
        '--verbosity'
        'minimal'
    )
    if ($NoRestore) {
        $buildArguments += '--no-restore'
    }

    & dotnet @buildArguments
    $buildExitCode = $LASTEXITCODE
    Write-Output "UNIT_TEST_BUILD_EXIT=$buildExitCode"
    if ($buildExitCode -ne 0) {
        Write-Error 'UNIT_TEST_BUILD_RESULT=FAIL'
        exit $buildExitCode
    }
}

$testApplications = @(Get-ChildItem -LiteralPath (Join-Path $projectDirectory "bin\$Configuration") -Filter "$projectName.exe" -File -Recurse)
if ($testApplications.Count -ne 1) {
    Write-Error "UNIT_TEST_ERROR=test_application_count_$($testApplications.Count)"
    exit 31
}

$testArguments = @(
    '--report-trx'
    '--report-trx-filename'
    'unit-tests.trx'
    '--results-directory'
    $ResultsDirectory
    '--minimum-expected-tests'
    '1'
    '--zero-tests-policy'
    'strict'
    '--output'
    'Minimal'
    '--show-stdout'
    'None'
    '--show-stderr'
    'Failed'
    '--no-ansi'
    '--progress'
    'off'
)

& $testApplications[0].FullName @testArguments
$testExitCode = $LASTEXITCODE
Write-Output "UNIT_TEST_COMMAND_EXIT=$testExitCode"

if ($testExitCode -ne 0) {
    Write-Error 'UNIT_TEST_RESULT=FAIL'
    exit $testExitCode
}

& (Join-Path $PSScriptRoot 'Assert-NonZeroTestResults.ps1') -ResultsDirectory $ResultsDirectory
$guardExitCode = $LASTEXITCODE
Write-Output "ZERO_TEST_GUARD_EXIT=$guardExitCode"

if ($guardExitCode -ne 0) {
    exit $guardExitCode
}

Write-Output 'UNIT_TEST_RESULT=PASS'
exit 0
