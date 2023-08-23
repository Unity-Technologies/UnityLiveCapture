[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls, [Net.SecurityProtocolType]::Tls11, [Net.SecurityProtocolType]::Tls12, [Net.SecurityProtocolType]::Ssl3
[Net.ServicePointManager]::SecurityProtocol = "Tls, Tls11, Tls12, Ssl3"
$changelogPath=$args[0]
$newVersion=$args[1]
$newChangeLogEntry = $args[2]

$changelogHeader = "and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html)."

Invoke-RestMethod -Uri $newChangeLogEntry -OutFile "newChangeLogEntry.txt"

$content = [IO.File]::ReadAllText("newChangeLogEntry.txt")

(Get-Content $changelogPath) |
    Foreach-Object {
        $_ # send the current line to output

        if ($_ -eq $changelogHeader)
        {
            #Add Lines after the selected pattern

            "`n## [$($newVersion)] - $(Get-Date -Format yyyy-MM-dd)"
            "`n$($content )"
        }
    } | Set-Content $changelogPath
