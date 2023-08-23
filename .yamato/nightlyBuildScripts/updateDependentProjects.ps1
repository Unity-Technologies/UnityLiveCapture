
$packagesFolder=$args[0]
$dependeePackagePath=$args[1]

$allManifests = Get-ChildItem -Path $packagesFolder -Depth 2 -File -Include ('package.json')
$packageManifest =  Get-Content $dependeePackagePath -raw | ConvertFrom-Json

Write-Host "Looking for all package manifest for packages in " $packagesFolder
Write-Host "Checking for dependency on package: " $packageManifest.name " and updating to version " $packageManifest.version 

foreach($currentManifest in $allManifests)
{
    $fileNeedsSaving = $false;
    Write-Host "current file:" $currentManifest   

    $currentManifestJson =  Get-Content $currentManifest -raw | ConvertFrom-Json
    if($currentManifestJson.dependencies.'com.unity.live-capture' -ne $null)
    {
        $currentManifestJson.dependencies.'com.unity.live-capture' = $packageManifest.version
        $currentManifestJson.version = $packageManifest.version
        $fileNeedsSaving = $true
        Write-Host "File dependency has been updated."
    }
    else
    {
        Write-Host "Current file does not contain a dependency"
    }

    if($fileNeedsSaving)
    {
        $currentManifestJson | ConvertTo-Json | Set-Content $currentManifest.FullName
    }    
}
