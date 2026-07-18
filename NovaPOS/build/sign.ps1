param(
    [Parameter(Mandatory = $true)]
    [string[]]$Files,

    [string]$CertificatePath = $env:CODESIGN_CERT_PATH,
    [string]$CertificatePassword = $env:CODESIGN_CERT_PASSWORD,
    [string]$TimestampUrl = "http://timestamp.digicert.com"
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($CertificatePath) -or -not (Test-Path $CertificatePath)) {
    Write-Host "Code signing certificate not configured — skipping signing step."
    exit 0
}

$securePassword = ConvertTo-SecureString $CertificatePassword -AsPlainText -Force
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($CertificatePath, $securePassword, "Exportable")

foreach ($file in $Files) {
    if (-not (Test-Path $file)) {
        Write-Warning "File not found for signing: $file"
        continue
    }

    Write-Host "Signing $file"
    & signtool.exe sign /fd SHA256 /tr $TimestampUrl /td SHA256 /f $CertificatePath /p $CertificatePassword $file
    if ($LASTEXITCODE -ne 0) {
        throw "signtool failed for $file"
    }
}

Write-Host "Code signing complete."
