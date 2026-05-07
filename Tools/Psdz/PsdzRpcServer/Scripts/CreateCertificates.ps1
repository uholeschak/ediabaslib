param(
    [string]$OutputPath = "$PSScriptRoot\..\..\Certificates",
    [string]$Password = "BmwCoding12!!",
    [int]$ValidYears    = 100
)

$ErrorActionPreference = "Stop"

function Remove-OldCertificates
{
    $subjectsToClean = @(
        @{ Store = "Cert:\CurrentUser\My"; Subject = "CN=PsdzRpc-CA" },
        @{ Store = "Cert:\CurrentUser\My"; Subject = "CN=PsdzRpcServer" },
        @{ Store = "Cert:\CurrentUser\My"; Subject = "CN=PsdzRpcClient" }
    )

    foreach ($entry in $subjectsToClean)
    {
        $existing = Get-ChildItem -Path $entry.Store |
            Where-Object { $_.Subject -like "*$($entry.Subject)*" }

        foreach ($cert in $existing)
        {
            Remove-Item -Path "$($entry.Store)\$($cert.Thumbprint)" -Force
            Write-Host "Removed old certificate: $($cert.Subject) [$($cert.Thumbprint)]"
        }
    }
}

function New-CaCertificate
{
    param(
        [int]$ValidYears
    )

    $ca = New-SelfSignedCertificate `
        -Subject           "CN=PsdzRpc-CA, O=EdiabasLib, C=DE" `
        -KeyUsage          CertSign, CRLSign `
        -KeyUsageProperty  Sign `
        -KeyLength         4096 `
        -HashAlgorithm     SHA256 `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -NotAfter          (Get-Date).AddYears($ValidYears) `
        -FriendlyName      "PsdzRpc CA"

    Write-Host "CA created: $($ca.Thumbprint)"
    return $ca
}

function New-ServerCertificate
{
    param(
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$CaCert,
        [string]$OutputPath,
        [securestring]$SecurePassword,
        [int]$ValidYears
    )

    $server = New-SelfSignedCertificate `
        -Subject           "CN=PsdzRpcServer, O=EdiabasLib, C=DE" `
        -KeyUsage          DigitalSignature, KeyEncipherment `
        -KeyLength         2048 `
        -HashAlgorithm     SHA256 `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -NotAfter          (Get-Date).AddYears($ValidYears) `
        -FriendlyName      "PsdzRpc Server" `
        -Signer            $CaCert

    Export-PfxCertificate -Cert $server -FilePath "$OutputPath\server.pfx" -Password $SecurePassword | Out-Null
    Write-Host "Server created: $($server.Thumbprint)"
}

function New-ClientCertificate
{
    param(
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$CaCert,
        [string]$OutputPath,
        [securestring]$SecurePassword,
        [int]$ValidYears
    )

    $client = New-SelfSignedCertificate `
        -Subject           "CN=PsdzRpcClient, O=EdiabasLib, C=DE" `
        -KeyUsage          DigitalSignature `
        -KeyLength         2048 `
        -HashAlgorithm     SHA256 `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -NotAfter          (Get-Date).AddYears($ValidYears) `
        -FriendlyName      "PsdzRpc Client" `
        -Signer            $CaCert

    Export-PfxCertificate -Cert $client -FilePath "$OutputPath\client.pfx" -Password $SecurePassword | Out-Null
    Write-Host "Client created: $($client.Thumbprint)"
}

function Export-CaCertificate
{
    param(
        [System.Security.Cryptography.X509Certificates.X509Certificate2]$CaCert,
        [string]$OutputPath,
        [securestring]$SecurePassword
    )

    Export-PfxCertificate -Cert $CaCert -FilePath "$OutputPath\ca.pfx" -Password $SecurePassword | Out-Null
    Export-Certificate    -Cert $CaCert -FilePath "$OutputPath\ca.crt" -Type CERT                | Out-Null

    Write-Host "CA exported: $($CaCert.Thumbprint)"
}

function Remove-PfxPassword
{
    param(
        [string]$PfxPath,
        [string]$Password
    )

    $cert = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new(
        $PfxPath,
        $Password,
        [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)

    try
    {
        $bytes = $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, "")
        [System.IO.File]::WriteAllBytes($PfxPath, $bytes)
        Write-Host "Password removed from: $PfxPath"
    }
    finally
    {
        $cert.Dispose()
    }
}

function Get-ExistingCaCertificate
{
    param(
        [string]$OutputPath
    )

    $caCrtPath = "$OutputPath\ca.crt"
    $caPfxPath = "$OutputPath\ca.pfx"

    if (-not (Test-Path $caCrtPath) -or -not (Test-Path $caPfxPath))
    {
        return $null
    }

    # CA aus .pfx laden (enthält privaten Schlüssel zum Signieren)
    $ca = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new(
        $caPfxPath,
        "",
        [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)

    # Temporär in \My importieren damit -Signer funktioniert
    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("My", "CurrentUser")
    $store.Open("ReadWrite")
    $store.Add($ca)
    $store.Close()

    Write-Host "CA reused: $($ca.Thumbprint)"
    return $ca
}

# --- Passwort prüfen ---
if ([string]::IsNullOrWhiteSpace($Password))
{
    Write-Error "Password must not be empty. Usage: .\CreateCertificates.ps1 -Password 'YourPassword'"
    exit 1
}

$secPwd = ConvertTo-SecureString -String $Password -Force -AsPlainText
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

# Bestehende CA wiederverwenden oder neue erstellen
$ca = Get-ExistingCaCertificate -OutputPath $OutputPath
if ($ca -eq $null)
{
    Remove-OldCertificates
    $ca = New-CaCertificate  -ValidYears $ValidYears
    Export-CaCertificate     -CaCert $ca -OutputPath $OutputPath -SecurePassword $secPwd
    Remove-PfxPassword       -PfxPath "$OutputPath\ca.pfx"      -Password $Password
}
else
{
    # Nur Server/Client-Zertifikate bereinigen
    $subjectsToClean = @(
        @{ Store = "Cert:\CurrentUser\My"; Subject = "CN=PsdzRpcServer" },
        @{ Store = "Cert:\CurrentUser\My"; Subject = "CN=PsdzRpcClient" }
    )
    foreach ($entry in $subjectsToClean)
    {
        Get-ChildItem -Path $entry.Store |
            Where-Object { $_.Subject -like "*$($entry.Subject)*" } |
            ForEach-Object {
                Remove-Item -Path "$($entry.Store)\$($_.Thumbprint)" -Force
                Write-Host "Removed old certificate: $($_.Subject) [$($_.Thumbprint)]"
            }
    }
}

New-ServerCertificate -CaCert $ca -OutputPath $OutputPath -SecurePassword $secPwd -ValidYears $ValidYears
New-ClientCertificate -CaCert $ca -OutputPath $OutputPath -SecurePassword $secPwd -ValidYears $ValidYears

foreach ($pfx in @("server.pfx", "client.pfx"))
{
    Remove-PfxPassword -PfxPath "$OutputPath\$pfx" -Password $Password
}

# CA aus \My entfernen (wurde nur temporär für -Signer benötigt)
Remove-Item -Path "Cert:\CurrentUser\My\$($ca.Thumbprint)" -Force -ErrorAction SilentlyContinue

Write-Host "`nAll certificates saved to: $OutputPath"
Write-Host "Password removed from all .pfx files"
