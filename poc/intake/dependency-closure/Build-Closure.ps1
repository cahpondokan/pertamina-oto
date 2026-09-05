# Offline generator/validator: reads only frozen JSON metadata; no network or package execution.
param([switch]$Verify)
$ErrorActionPreference='Stop'
$selection=@(
 @('PDFsharp','6.2.4','net10.0'),
 @('Microsoft.Extensions.Logging.Abstractions','8.0.3','net8.0'),
 @('System.Security.Cryptography.Pkcs','8.0.1','net8.0'),
 @('Microsoft.Extensions.DependencyInjection.Abstractions','8.0.2','net8.0')
)
$components=@(foreach($s in $selection){
 $id=$s[0]; $v=$s[1]; $tfm=$s[2]; $lower=$id.ToLowerInvariant()
 $snapshot=Get-Content -LiteralPath "$PSScriptRoot/metadata/$lower.$v.json" -Raw | ConvertFrom-Json
 $c=$snapshot.catalog
 if($c.id -cne $id -or $c.version -ne $v -or $c.licenseExpression -ne 'MIT'){throw 'Identity/version/license mismatch'}
 if(([uri]$snapshot.registrationUrl).Host -ne 'api.nuget.org' -or ([uri]$snapshot.catalogUrl).Host -ne 'api.nuget.org'){throw 'Nonofficial metadata source'}
 $groups=@($c.dependencyGroups | Where-Object targetFramework -CEQ $tfm)
 if($groups.Count -ne 1){throw 'Missing/ambiguous framework group'}
 $deps=@(foreach($d in $groups[0].dependencies){
  if($null -eq $d){continue}
  $target=@($selection | Where-Object {$_[0] -eq $d.id})
  if($target.Count -ne 1){throw 'Closure missing dependency'}
  if($d.range -ne ('['+$target[0][1]+', )')){throw 'Unexpected dependency range; review proposal'}
  [ordered]@{id=$d.id;declaredRange=$d.range;proposedVersion=$target[0][1]}
 })
 [ordered]@{id=$id;version=$v;selectedDependencyGroup=$tfm;license='MIT';licenseUrl=$c.licenseUrl;costStatus='NO_LICENSE_FEE_PROPOSED_REVIEW_REQUIRED';checkedUtc=$snapshot.checkedUtc;metadataSource=$snapshot.registrationUrl;catalogSource=$snapshot.catalogUrl;gallerySource="https://www.nuget.org/packages/$id/$v";proposedAcquisitionUrl="https://api.nuget.org/v3-flatcontainer/$lower/$v/$lower.$v.nupkg";acquisitionStatus=$(if($id -eq 'PDFsharp'){'ALREADY_ACQUIRED_WAVE_0A_NO_REDOWNLOAD'}else{'PROPOSED_NOT_AUTHORIZED'});binarySha256=$null;dependencies=$deps}
})
$result=[ordered]@{schemaVersion=1;taskId='P2-W0C-A2-DEPENDENCY-METADATA';baseline='099af9d40810f54277f82baeee80573370427208';targetFramework='net10.0';targetPlatform='Windows x64 proposed';resolutionStatus='METADATA_ONLY_PROPOSED_NOT_RESTORED';versionPolicy='Exact lower-bound baseline; not latest/security-approved; no other top-level dependencies assumed';processingAuthorized=$false;restartAuthorized=$false;components=$components;newPackageAcquisitionCount=3;unresolved=@('Fresh security/vulnerability review','Artifact hashes/signatures and notices for three unacquired packages','RID/native/build/analyzer asset verification','SDK/runtime packs and exact guest runtime identity outside package closure','Actual restore graph, asset selection, framework pruning and conflict validation','Human acquisition, license, security and execution approvals')}
$json=($result | ConvertTo-Json -Depth 15).Replace("`r`n","`n")+"`n"
$path="$PSScriptRoot/closure.json"
if($Verify){
 if((Get-Content -LiteralPath $path -Raw) -cne $json){throw 'Manifest differs from frozen metadata'}
 if($components.Count -ne 4 -or @($components | ForEach-Object {$_.dependencies}).Count -ne 3){throw 'Unexpected node/edge count'}
 $bad=@(Get-ChildItem -LiteralPath $PSScriptRoot -Recurse -File | Where-Object Extension -NotIn '.md','.json','.ps1')
 if($bad.Count){throw 'Unexpected non-text artifact'}
 'PASS: metadata identities, MIT expressions, selected groups, 4 nodes/3 edges, exact URL/version allowlist, deterministic manifest, text-only scope'
}else{[IO.File]::WriteAllText($path,$json,[Text.UTF8Encoding]::new($false))}
