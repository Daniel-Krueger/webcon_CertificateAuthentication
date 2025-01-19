#$pfxPath = ".\WEBCON to SharePoint v7.pfx"
$pfxPath = ".\UpdatedCert.pfx"
$pfxBytes = [System.IO.File]::ReadAllBytes($pfxPath)
$pfxBase64 = [System.Convert]::ToBase64String($pfxBytes)

$pfxBase64 | clip.exe