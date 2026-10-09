[CmdletBinding()]
param(
    [string]$RepositoryRoot,
    [string]$PolicyPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
}
else {
    $RepositoryRoot = [System.IO.Path]::GetFullPath($RepositoryRoot)
}

if ([string]::IsNullOrWhiteSpace($PolicyPath)) {
    $PolicyPath = Join-Path $PSScriptRoot 'phase0-dependency-policy.json'
}

$requiredFiles = @(
    $PolicyPath,
    (Join-Path $RepositoryRoot 'NuGet.Config')
)
foreach ($requiredFile in $requiredFiles) {
    if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
        Write-Error 'DEPENDENCY_POLICY_ERROR=required_file_missing'
        exit 80
    }
}

try {
    $policy = Get-Content -LiteralPath $PolicyPath -Raw | ConvertFrom-Json -Depth 20
}
catch {
    Write-Error 'DEPENDENCY_POLICY_ERROR=invalid_policy_json'
    exit 81
}

if ($policy.schemaVersion -ne 1) {
    Write-Error 'DEPENDENCY_POLICY_ERROR=unsupported_schema'
    exit 82
}

if (@($policy.packageSources).Count -ne 1) {
    Write-Error 'DEPENDENCY_POLICY_ERROR=source_policy_count'
    exit 83
}

$approvedSource = @($policy.packageSources)[0]
$approvedSdkVersions = @{}
foreach ($entry in @($policy.versionedProjectSdks)) {
    $approvedSdkVersions[[string]$entry.id] = [string]$entry.version
}
$approvedPackageVersions = @{}
foreach ($entry in @($policy.packageReferences)) {
    $approvedPackageVersions[[string]$entry.id] = [string]$entry.version
}

try {
    [xml]$nuget = Get-Content -LiteralPath (Join-Path $RepositoryRoot 'NuGet.Config') -Raw
}
catch {
    Write-Error 'DEPENDENCY_POLICY_ERROR=invalid_nuget_config'
    exit 84
}

$sourceNodes = @($nuget.configuration.packageSources.add)
if ($sourceNodes.Count -ne 1 -or
    [string]$sourceNodes[0].key -cne [string]$approvedSource.key -or
    [string]$sourceNodes[0].value -cne [string]$approvedSource.url) {
    Write-Error 'DEPENDENCY_POLICY_RESULT=FAIL;REASON=unapproved_package_source'
    exit 85
}

$credentialNodes = @($nuget.SelectNodes("/configuration/packageSourceCredentials/*"))
if ($credentialNodes.Count -gt 0) {
    Write-Error 'DEPENDENCY_POLICY_RESULT=FAIL;REASON=source_credentials_forbidden'
    exit 86
}

$mappingNode = @($nuget.configuration.packageSourceMapping.packageSource | Where-Object {
    [string]$_.key -ceq [string]$approvedSource.key
})
if ($mappingNode.Count -ne 1) {
    Write-Error 'DEPENDENCY_POLICY_RESULT=FAIL;REASON=source_mapping_missing'
    exit 87
}

$actualPatterns = @($mappingNode[0].package | ForEach-Object { [string]$_.pattern } | Sort-Object)
$approvedPatterns = @($approvedSource.patterns | ForEach-Object { [string]$_ } | Sort-Object)
if (($actualPatterns -join "`n") -cne ($approvedPatterns -join "`n")) {
    Write-Error 'DEPENDENCY_POLICY_RESULT=FAIL;REASON=source_mapping_mismatch'
    exit 88
}

$excludedSegments = @('.git', 'bin', 'obj', 'TestResults', 'artifacts', 'packages')
$projectFiles = @(Get-ChildItem -LiteralPath $RepositoryRoot -Recurse -Filter '*.csproj' -File | Where-Object {
    $relative = [System.IO.Path]::GetRelativePath($RepositoryRoot, $_.FullName)
    $segments = $relative -split '[\\/]'
    -not ($segments | Where-Object { $excludedSegments -contains $_ })
})
if ($projectFiles.Count -eq 0) {
    Write-Error 'DEPENDENCY_POLICY_RESULT=FAIL;REASON=no_projects'
    exit 89
}

$findings = [System.Collections.Generic.List[string]]::new()
$directReferenceCount = 0
$versionedSdkCount = 0

foreach ($projectFile in $projectFiles) {
    try {
        [xml]$project = Get-Content -LiteralPath $projectFile.FullName -Raw
    }
    catch {
        $findings.Add('invalid_project_xml')
        continue
    }

    $sdkValues = [System.Collections.Generic.List[string]]::new()
    $projectElement = $project.DocumentElement
    $sdkAttribute = $projectElement.GetAttribute('Sdk')
    if (-not [string]::IsNullOrWhiteSpace($sdkAttribute)) {
        $sdkValues.Add($sdkAttribute)
    }
    foreach ($sdkNode in @($projectElement.SelectNodes("./*[local-name()='Sdk']"))) {
        $sdkName = $sdkNode.GetAttribute('Name')
        $sdkVersion = $sdkNode.GetAttribute('Version')
        if (-not [string]::IsNullOrWhiteSpace($sdkName) -and
            -not [string]::IsNullOrWhiteSpace($sdkVersion)) {
            $sdkValues.Add(('{0}/{1}' -f $sdkName, $sdkVersion))
        }
    }

    foreach ($sdkValue in @($sdkValues | Select-Object -Unique)) {
        if ($sdkValue -notmatch '/') {
            continue
        }
        $parts = $sdkValue.Split('/', 2)
        $versionedSdkCount++
        if (-not $approvedSdkVersions.ContainsKey($parts[0]) -or
            $approvedSdkVersions[$parts[0]] -cne $parts[1]) {
            $findings.Add('unapproved_versioned_project_sdk')
        }
    }

    foreach ($reference in @($project.SelectNodes("//*[local-name()='PackageReference']"))) {
        $id = [string]$reference.Include
        if ([string]::IsNullOrWhiteSpace($id)) {
            $id = [string]$reference.Update
        }
        $version = [string]$reference.Version
        if ([string]::IsNullOrWhiteSpace($version)) {
            $versionNode = $reference.SelectSingleNode("*[local-name()='Version']")
            if ($null -ne $versionNode) {
                $version = [string]$versionNode.InnerText
            }
        }

        $directReferenceCount++
        if (-not $approvedPackageVersions.ContainsKey($id) -or
            $approvedPackageVersions[$id] -cne $version) {
            $findings.Add('unapproved_package_reference')
        }
    }

    $overrideNodes = @($project.SelectNodes("//*[local-name()='RestoreSources' or local-name()='RestoreAdditionalProjectSources']"))
    if ($overrideNodes.Count -gt 0) {
        $findings.Add('project_source_override_forbidden')
    }
}

$centralFile = Join-Path $RepositoryRoot 'Directory.Packages.props'
if (Test-Path -LiteralPath $centralFile -PathType Leaf) {
    [xml]$central = Get-Content -LiteralPath $centralFile -Raw
    foreach ($entry in @($central.SelectNodes("//*[local-name()='PackageVersion']"))) {
        $id = [string]$entry.Include
        $version = [string]$entry.Version
        if (-not $approvedPackageVersions.ContainsKey($id) -or
            $approvedPackageVersions[$id] -cne $version) {
            $findings.Add('unapproved_central_package_version')
        }
    }
}

Write-Output "DEPENDENCY_POLICY_PROJECTS=$($projectFiles.Count)"
Write-Output "DEPENDENCY_POLICY_VERSIONED_SDKS=$versionedSdkCount"
Write-Output "DEPENDENCY_POLICY_DIRECT_REFERENCES=$directReferenceCount"
Write-Output "DEPENDENCY_POLICY_FINDINGS=$($findings.Count)"

if ($findings.Count -gt 0) {
    foreach ($finding in @($findings | Sort-Object -Unique)) {
        Write-Output "DEPENDENCY_POLICY_FINDING=$finding"
    }
    Write-Error 'DEPENDENCY_POLICY_RESULT=FAIL'
    exit 90
}

Write-Output 'DEPENDENCY_POLICY_RESULT=PASS'
exit 0
