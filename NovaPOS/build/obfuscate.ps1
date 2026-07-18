param(
    [Parameter(Mandatory = $true)]
    [string]$InputDir,

    [string]$ConfuserCli = "$PSScriptRoot\tools\ConfuserEx\Confuser.CLI.exe",
    [string]$ProjectFile = "$PSScriptRoot\crpack.crproj"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ConfuserCli)) {
    Write-Host "ConfuserEx not found at $ConfuserCli — skipping obfuscation."
    exit 0
}

$projectXml = Get-Content $ProjectFile -Raw
$projectXml = $projectXml -replace 'baseDir="[^"]*"', "baseDir=`"$InputDir`""
$projectXml = $projectXml -replace 'outputDir="[^"]*"', "outputDir=`"$InputDir`""
$tempProject = Join-Path $env:TEMP "aivorapos-crpack.crproj"
Set-Content -Path $tempProject -Value $projectXml -Encoding UTF8

& $ConfuserCli $tempProject
if ($LASTEXITCODE -ne 0) {
    throw "ConfuserEx failed with exit code $LASTEXITCODE"
}

Write-Host "Obfuscation complete for $InputDir"
