[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls, [Net.SecurityProtocolType]::Tls11, [Net.SecurityProtocolType]::Tls12, [Net.SecurityProtocolType]::Ssl3
[Net.ServicePointManager]::SecurityProtocol = "Tls, Tls11, Tls12, Ssl3"
$incrementId=$args[0]
$newBuild = Invoke-restmethod -Uri "https://increment.build/$incrementId/get"
if($newBuild -is [int])
{
    return $newBuild
}
else
{
    throw "The build version received from increment.io is not an integer"
}
