<#
.SYNOPSIS
    Registers PsdzRpcServer.exe as a Windows Service (start on demand).

.PARAMETER UserName
    The user account to run the service (e.g. ".\serviceUser" or "DOMAIN\user").

.PARAMETER Password
    The password for the user account.

.PARAMETER ServerExe
    Full path to PsdzRpcServer.exe.

.EXAMPLE
    .\RegisterPsdzRpcService.ps1 -UserName ".\serviceUser" -Password "secret" -ServerExe "C:\Program Files\PsdzRpcServer\PsdzRpcServer.exe"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$UserName,

    [Parameter(Mandatory = $true)]
    [string]$Password,

    [Parameter(Mandatory = $false)]
    [string]$ServerExe = "C:\Program Files\PsdzRpcServer\PsdzRpcServer.exe"
)

$ServiceName    = "PsdzRpcServer"
$DisplayName    = "PsdzRpc Server"
$Description    = "PsdzRpc JSON-RPC Named Pipe Server"

# --- Admin check ---
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be run as Administrator."
    exit 1
}

# --- Validate exe ---
if (-not (Test-Path $ServerExe)) {
    Write-Error "Server executable not found: $ServerExe"
    exit 1
}

# --- Unregister if already exists ---
$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "Service '$ServiceName' already registered. Removing..."

    if ($existing.Status -eq 'Running') {
        Write-Host "Stopping service..."
        Stop-Service -Name $ServiceName -Force
        $existing.WaitForStatus('Stopped', [TimeSpan]::FromSeconds(30))
    }

    sc.exe delete $ServiceName | Out-Null
    # Wait until service is fully removed
    $retries = 0
    while ((Get-Service -Name $ServiceName -ErrorAction SilentlyContinue) -and $retries -lt 10) {
        Start-Sleep -Milliseconds 500
        $retries++
    }

    Write-Host "Service '$ServiceName' removed."
}

# --- Register service ---
Write-Host "Registering service '$ServiceName'..."
Write-Host "  Exe  : $ServerExe"
Write-Host "  User : $UserName"
Write-Host "  Start: Demand (manual)"

$result = sc.exe create $ServiceName `
    binPath= `"$ServerExe`" `
    obj= $UserName `
    password= $Password `
    start= demand `
    DisplayName= $DisplayName

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create service: $result"
    exit 1
}

# --- Set description ---
sc.exe description $ServiceName $Description | Out-Null

# --- Grant "Log on as a service" right ---
Write-Host "Granting 'Log on as a service' right to '$UserName'..."
$tempFile = [System.IO.Path]::GetTempFileName()
secedit /export /cfg $tempFile /quiet
$content = Get-Content $tempFile
$seServiceLogon = $content | Where-Object { $_ -match "SeServiceLogonRight" }
if ($seServiceLogon) {
    if ($seServiceLogon -notmatch [regex]::Escape($UserName)) {
        $content = $content -replace "SeServiceLogonRight\s*=\s*(.*)", "SeServiceLogonRight = `$1,$UserName"
        Set-Content $tempFile $content
        secedit /import /cfg $tempFile /db secedit.sdb /quiet
        secedit /configure /db secedit.sdb /quiet
        Write-Host "  Right granted."
    } else {
        Write-Host "  Right already granted."
    }
} else {
    Write-Warning "Could not find SeServiceLogonRight in security policy."
}
Remove-Item $tempFile -ErrorAction SilentlyContinue
Remove-Item "secedit.sdb" -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Service '$ServiceName' registered successfully." -ForegroundColor Green
Write-Host "Start with: Start-Service -Name $ServiceName"
Write-Host "Or via IIS after first client connects (StartServerIfNeeded)."
