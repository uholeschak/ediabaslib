param(
    [string]$OutputPath = "$PSScriptRoot\Certificates",
    [string]$Password,
    [int]$ValidYears    = 10
)

$ErrorActionPreference = "Stop"

if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be run as Administrator."
    exit 1
}

# --- ExecutionPolicy check ---
$effectivePolicy = Get-ExecutionPolicy
Write-Host "Effective ExecutionPolicy: $effectivePolicy"

if ($effectivePolicy -eq 'Restricted') {
    Write-Error "Script execution is disabled. Run the following command first:"
    Write-Error "Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser"
    exit 1
}

# --- Passwort prüfen ---
if ([string]::IsNullOrWhiteSpace($Password))
{
    Write-Error "Password must not be empty. Usage: .\CreateCertificates.ps1 -Password 'YourPassword'"
    exit 1
}

$secPwd = ConvertTo-SecureString -String $Password -Force -AsPlainText
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

# --- 1. CA-Zertifikat ---
$ca = New-SelfSignedCertificate `
    -Subject           "CN=PsdzRpc-CA, O=EdiabasLib, C=DE" `
    -KeyUsage          CertSign, CRLSign `
    -KeyUsageProperty  Sign `
    -KeyLength         4096 `
    -HashAlgorithm     SHA256 `
    -CertStoreLocation "Cert:\LocalMachine\Root" `
    -NotAfter          (Get-Date).AddYears($ValidYears) `
    -FriendlyName      "PsdzRpc CA"

Export-PfxCertificate  -Cert $ca -FilePath "$OutputPath\ca.pfx"          -Password $secPwd | Out-Null
Export-Certificate     -Cert $ca -FilePath "$OutputPath\ca.crt"           -Type CERT        | Out-Null
Write-Host "CA created:     $($ca.Thumbprint)"

# --- 2. Server-Zertifikat (signiert von CA) ---
$server = New-SelfSignedCertificate `
    -Subject           "CN=localhost, O=EdiabasLib, C=DE" `
    -DnsName           "localhost", "127.0.0.1" `
    -KeyUsage          DigitalSignature, KeyEncipherment `
    -KeyLength         2048 `
    -HashAlgorithm     SHA256 `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -NotAfter          (Get-Date).AddYears($ValidYears) `
    -FriendlyName      "PsdzRpc Server" `
    -Signer            $ca

Export-PfxCertificate -Cert $server -FilePath "$OutputPath\server.pfx" -Password $secPwd | Out-Null
Write-Host "Server created: $($server.Thumbprint)"

# --- 3. Client-Zertifikat (signiert von CA) ---
$client = New-SelfSignedCertificate `
    -Subject           "CN=PsdzRpcClient, O=EdiabasLib, C=DE" `
    -KeyUsage          DigitalSignature `
    -KeyLength         2048 `
    -HashAlgorithm     SHA256 `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -NotAfter          (Get-Date).AddYears($ValidYears) `
    -FriendlyName      "PsdzRpc Client" `
    -Signer            $ca

Export-PfxCertificate -Cert $client -FilePath "$OutputPath\client.pfx" -Password $secPwd | Out-Null
Write-Host "Client created: $($client.Thumbprint)"

Write-Host "`nAll certificates saved to: $OutputPath"
Write-Host "Password: $Password"
