<#
.SYNOPSIS
    Registers PsdzRpcServer.exe as a Windows Service using NSSM.
    Required execution policy:
    Set-ExecutionPolicy -ExecutionPolicy RemoteSigned
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$UserName,

    [Parameter(Mandatory = $false)]
    [string]$Password,

    [Parameter(Mandatory = $false)]
    [string]$ServerExe = "$($env:ProgramFiles)\PsdzRpcServer\PsdzRpcServer.exe",

    [Parameter(Mandatory = $false)]
    [string]$NssmExe = "nssm.exe",

    [Parameter(Mandatory = $false)]
    [switch]$KeepRunning,

    [Parameter(Mandatory = $false)]
    [string]$IisAppPool = "Coding",

    [Parameter(Mandatory = $false)]
    [switch]$Uninstall
)

$ServiceName = "PsdzRpcServer"
$DisplayName = "Psdz RPC pipe server"
$Description = "Psdz JSON-RPC named pipe server"

# --- Admin check ---
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

function Remove-PsdzService {
    $existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $existing) {
        Write-Host "Service '$ServiceName' is not installed."
        return $true
    }

    Write-Host "Removing service '$ServiceName'..."
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
        return $false
    }

    Write-Host "Service '$ServiceName' removed."
    return $true
}

function Grant-ServiceStartPermission {
    param(
        [string]$ServiceName,
        [string]$AppPoolName = "Coding"
    )

    Write-Host "Granting service start permission to IIS APPPOOL\$AppPoolName..."

    try
    {
        $appPoolSid = (New-Object System.Security.Principal.NTAccount("IIS APPPOOL\$AppPoolName")).Translate(
            [System.Security.Principal.SecurityIdentifier]).Value
    }
    catch
    {
        Write-Warning "Could not resolve SID for 'IIS APPPOOL\$AppPoolName': $_"
        return
    }

    $sdResult = sc.exe sdshow $ServiceName 2>&1
    $sdLine = $sdResult | Where-Object { $_ -match "D:" } | Select-Object -First 1
    $sd = if ($sdLine) { $sdLine.Trim() } else { $null }

    if (-not $sd) {
        $sdAll = ($sdResult -join "")
        if ($sdAll -match "(D:.+)") { $sd = $Matches[1].Trim() }
    }

    if (-not $sd) {
        Write-Warning "Could not parse security descriptor."
        return
    }

    if ($sd.Contains($appPoolSid)) {
        Write-Host "  Permission already granted."
        return
    }

    $newAce = "(A;;RPWPLC;;;$appPoolSid)"
    $newSd = $sd -replace "(D:[^(]*)", "`$1$newAce"

    $result = sc.exe sdset $ServiceName $newSd
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Permission granted."
    } else {
        Write-Warning "Could not set service security descriptor: $result"
    }
}

# --- Validate NSSM ---
$resolvedNssm = Get-Command $NssmExe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
if (-not $resolvedNssm) {
    Write-Error "NSSM not found. Install with: winget install NSSM.NSSM"
    exit 1
}
$NssmExe = $resolvedNssm
Write-Host "Using NSSM: $NssmExe"

# --- Uninstall only ---
if ($Uninstall) {
    if (-not (Remove-PsdzService)) { exit 1 }
    Write-Host "Service '$ServiceName' uninstalled successfully." -ForegroundColor Green
    exit 0
}

# --- Validate required parameters for install ---
if ([string]::IsNullOrEmpty($UserName)) {
    Write-Error "Parameter -UserName is required for installation."
    exit 1
}
if ([string]::IsNullOrEmpty($Password)) {
    Write-Error "Parameter -Password is required for installation."
    exit 1
}

# --- Validate exe ---
if (-not (Test-Path $ServerExe)) {
    Write-Error "Server executable not found: $ServerExe"
    exit 1
}

# --- Normalize UserName ---
if ($UserName -notmatch '\\') {
    $UserName = ".\$UserName"
}
Write-Host "Using UserName: $UserName"

# --- Unregister if already exists ---
if (-not (Remove-PsdzService)) { exit 1 }

# --- Create log directory ---
$logDir = "$env:ProgramData\ISTA\logs"
Write-Host "Creating log directory: $logDir"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

$resolvedUser = $UserName -replace '^\.[\\\/]', "$env:COMPUTERNAME\"
Write-Host "  Setting permissions for: $resolvedUser"

try
{
    $acl = Get-Acl $logDir
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        $resolvedUser, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($rule)
    Set-Acl $logDir $acl
    Write-Host "  Log directory permissions set."
}
catch
{
    Write-Warning "Could not set permissions for '$resolvedUser': $_"
    Write-Warning "Please set permissions manually on: $logDir"
}

# --- Register with NSSM ---
Write-Host "Registering service '$ServiceName' with NSSM..."
Write-Host "  Exe        : $ServerExe"
Write-Host "  User       : $UserName"
Write-Host "  Start      : Demand (manual)"
Write-Host "  KeepRunning: $KeepRunning"

$appArgs = if ($KeepRunning) { "--keeprunning" } else { "" }
& $NssmExe install $ServiceName "$ServerExe" $appArgs
& $NssmExe set $ServiceName AppDirectory (Split-Path $ServerExe)
& $NssmExe set $ServiceName DisplayName $DisplayName
& $NssmExe set $ServiceName Description $Description
& $NssmExe set $ServiceName ObjectName $UserName $Password
& $NssmExe set $ServiceName Start SERVICE_DEMAND_START
& $NssmExe set $ServiceName AppExit Default Exit
& $NssmExe set $ServiceName AppStdout "$logDir\PsdzRpcServer.log"
& $NssmExe set $ServiceName AppStderr "$logDir\PsdzRpcServer-error.log"
& $NssmExe set $ServiceName AppRotateFiles 1
& $NssmExe set $ServiceName AppRotateOnline 1
& $NssmExe set $ServiceName AppRotateBytes 10485760   # 10 MB

# --- Grant IIS App Pool service start permission ---
if (-not [string]::IsNullOrEmpty($IisAppPool)) {
    Grant-ServiceStartPermission -ServiceName $ServiceName -AppPoolName $IisAppPool
}

# --- Grant "Log on as a service" right ---
Write-Host "Granting 'Log on as a service' right to '$resolvedUser'..."
$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null
$seceditCfg = Join-Path $tempDir "secedit.cfg"
$seceditSdb = Join-Path $tempDir "secedit.sdb"

#Write-Host "  Cfg: $seceditCfg"
#Write-Host "  Sdb: $seceditSdb"

secedit /export /cfg $seceditCfg /quiet
$content = Get-Content $seceditCfg
$seServiceLogon = $content | Where-Object { $_ -match "SeServiceLogonRight" }

if ($seServiceLogon) {
    $shortUser = $resolvedUser -replace '^.*[\\\/]', ''
    $seceditUser = "*$resolvedUser"
    $alreadyGranted = ($seServiceLogon -match [regex]::Escape($seceditUser)) -or
                      ($seServiceLogon -match "(?i)(^|,)\*?$([regex]::Escape($shortUser))(,|$)")
    if (-not $alreadyGranted) {
        $content = $content -replace "(SeServiceLogonRight\s*=\s*.*)", "`$1,$seceditUser"
        Set-Content $seceditCfg $content
        secedit /import /cfg $seceditCfg /db $seceditSdb /quiet
        secedit /configure /db $seceditSdb /quiet
        Write-Host "  Right granted."
    } else {
        Write-Host "  Right already granted."
    }
} else {
    Write-Warning "Could not find SeServiceLogonRight in security policy."
}

Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Service '$ServiceName' registered successfully." -ForegroundColor Green
Write-Host "Start with: Start-Service -Name $ServiceName"
Write-Host "Or via IIS after first client connects (StartServerIfNeeded)."
