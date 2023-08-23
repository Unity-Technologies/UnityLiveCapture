$packageName = $args[0]
$packageVersion = $args[1]

$fileExist = $true

try
{    
    $urlToCheck = "https://artifactory.prd.cds.internal.unity3d.com/artifactory/upm-candidates-master/$($packageName)/-/$($packageName)-$($packageVersion).tgz"
    Write-Host "Checking for package: "  $urlToCheck
    $artifactoryResponse = Invoke-restmethod -Uri $urlToCheck
    $urlToCheck | Out-File -FilePath .\FileAlreadyExists.txt
}
catch{

    ## Most likely, the file does not exist (404)
    Write-Host "StatusCode:" $_.Exception.Response.StatusCode.value__ 
    Write-Host "StatusDescription:" $_.Exception.Response.StatusDescription

    $fileExist = $false
}
