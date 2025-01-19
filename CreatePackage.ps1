param($targetDir,$projectDir,$projectName,$copyDestinationFolder)
Write-Output "targetDir:'$targetDir'"
Write-Output "projectDir:'$projectDir'"
Write-Output "projectName:'$projectName'"


New-Item $projectDir\Publish -ItemType Directory -Force -ErrorAction SilentlyContinue
$files = Get-ChildItem -Path "$targetDir\$projectName.*" -Exclude "$projectName.deps.json*"
$bpspkgFiles = [array] (Get-ChildItem -Path "$projectdir\bin\*.bpspkg" -Recurse )
Compress-Archive -Path $files -DestinationPath "$projectDir\Publish\$projectName.zip" -Force -CompressionLevel:Optimal
if ($bpspkgFiles.Count -gt 0) {
    Compress-Archive -Path $bpspkgFiles -DestinationPath "$projectDir\Publish\$projectName.zip" -Update
}