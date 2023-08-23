$packageRelativePath=$args[0]
$newBuild=$args[1]

$packageManifest = Get-Content $packageRelativePath -raw | ConvertFrom-Json


$metadataVersionIndex = $packageManifest.Version.LastIndexOf(".")
$metadataStart = $packageManifest.Version.LastIndexOf("-")

if($metadataStart -eq -1 -or $metadataVersionIndex -lt  $metadataStart)
{
    Write-Error 'No metadata version found to increment. Suffix your semver -preview.1 or -exp.1 to get started.'
}
else 
{
    $packageManifest.Version = $packageManifest.Version.Substring(0, $packageManifest.Version.LastIndexOf(".")) + "." + $newBuild
    $packageManifest | ConvertTo-Json -depth 32|% { [System.Text.RegularExpressions.Regex]::Unescape($_) }| set-content $packageRelativePath -Encoding Default    
}

return $packageManifest.Version
