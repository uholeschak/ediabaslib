<#
.SYNOPSIS
    Registers PsdzRpcServer.exe as a Windows Service using NSSM.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$UserName,

    [Parameter(Mandatory = $true)]
    [string]$Password,

    [Parameter(Mandatory = $false)]
    [string]$ServerExe = "C:\Program Files\PsdzRpcServer\PsdzRpcServer.exe",

    [Parameter(Mandatory = $false)]
    [string]$NssmExe = "nssm.exe"
)

$ServiceName = "PsdzRpcServer"
$DisplayName = "PsdzRpc Server"
$Description = "PsdzRpc JSON-RPC Named Pipe Server"

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

# --- Validate NSSM ---
$nssmPath = Get-Command $NssmExe -ErrorAction SilentlyContinue
if (-not $nssmPath) {
    $nssmPath = "C:\ProgramData\chocolatey\bin\nssm.exe"
    if (-not (Test-Path $nssmPath)) {
        Write-Error "NSSM not found. Install with: winget install NSSM.NSSM"
        exit 1
    }
    $NssmExe = $nssmPath
}

Write-Host "Using NSSM: $NssmExe"

# --- Unregister if already exists ---
$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "Service '$ServiceName' already registered. Removing..."

    if ($existing.Status -eq 'Running') {
        Write-Host "Stopping service..."
        & $NssmExe stop $ServiceName confirm | Out-Null
    }

    & $NssmExe remove $ServiceName confirm | Out-Null

    Write-Host "Waiting for service to be fully removed..." -NoNewline
    $retries = 0
    while ($retries -lt 30) {
        Start-Sleep -Milliseconds 1000
        if (-not (Get-Service -Name $ServiceName -ErrorAction SilentlyContinue)) { break }
        Write-Host "." -NoNewline
        $retries++
    }
    Write-Host ""

    if (Get-Service -Name $ServiceName -ErrorAction SilentlyContinue) {
        Write-Error "Service could not be fully removed. Close Services.msc and try again."
        exit 1
    }

    Write-Host "Service '$ServiceName' removed."
}

# --- Register with NSSM ---
Write-Host "Registering service '$ServiceName' with NSSM..."
Write-Host "  Exe  : $ServerExe"
Write-Host "  User : $UserName"
Write-Host "  Start: Demand (manual)"

# --- Create log directory ---
$logDir = "$env:ProgramData\ISTA\logs"
Write-Host "Creating log directory: $logDir"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

# Benutzer braucht Schreibrecht auf Log-Verzeichnis
$acl = Get-Acl $logDir
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    $UserName, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl $logDir $acl
Write-Host "  Log directory permissions set for '$UserName'."


& $NssmExe install $ServiceName $ServerExe "--keeprunning"
& $NssmExe set $ServiceName AppDirectory (Split-Path $ServerExe)
& $NssmExe set $ServiceName DisplayName $DisplayName
& $NssmExe set $ServiceName Description $Description
& $NssmExe set $ServiceName ObjectName $UserName $Password
& $NssmExe set $ServiceName Start SERVICE_DEMAND_START
& $NssmExe set $ServiceName AppStdout "$logDir\PsdzRpcServer.log"
& $NssmExe set $ServiceName AppStderr "$logDir\PsdzRpcServer-error.log"
& $NssmExe set $ServiceName AppRotateFiles 1
& $NssmExe set $ServiceName AppRotateBytes 10485760

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
