# ===============================
# HuskyNZ VCSA Deployment
# ===============================

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$ErrorActionPreference = "Stop"

function Import-DotEnv {
    param([string]$Path = ".env")
    if (-Not (Test-Path $Path)) {
        throw "Env file $Path not found"
    }
    Get-Content $Path | ForEach-Object {
        if ($_ -match '^\s*#') { return }
        if ($_ -match '^\s*$') { return }
        $parts = $_ -split '=', 2
        if ($parts.Length -eq 2) {
            $key = $parts[0].Trim()
            $value = $parts[1].Trim('"')
            [System.Environment]::SetEnvironmentVariable($key, $value)
        }
    }
}

# Load environment variables
Import-DotEnv

# Read env vars
$VCSADeployCLI   = $env:VCSA_CLI_PATH
$ESXiHost        = $env:ESXI_HOST
$ESXiUser        = $env:ESXI_USER
$ESXiPassword    = $env:ESXI_PASSWORD
$VCSAName        = $env:VCSA_HOST
$VCSARootPass    = $env:VCSA_ROOT_PASSWORD
$VCPassword      = $env:VC_PASSWORD
$NTPServers      = $env:NTP_SERVERS -split ','
$DeploymentNetwork = $env:DEPLOYMENT_NETWORK
$Datastore         = $env:DATASTORE
$ThinDiskMode      = ($env:THIN_DISK_MODE -match '^(1|true)$')
$DeploymentOption  = $env:DEPLOYMENT_OPTION
$IPAddress        = $env:IP_ADDRESS
$DnsServers       = $env:DNS_SERVERS -split ','
$NetworkPrefix    = $env:NETWORK_PREFIX
$Gateway          = $env:GATEWAY
$SsoDomain        = $env:SSO_DOMAIN
$CeipSettings     = ($env:CEIP_SETTINGS -match '^(1|true)$')
$OrgName          = $env:ORG_NAME
$VmName           = $env:VM_NAME

$currentTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$width = 60
$title = "$OrgName Vcenter Deployment Script"

# Center the title inside the width minus 2 (for frame sides)
$paddingLeft = [Math]::Floor(($width - 2 - $title.Length) / 2)
$paddingRight = $width - 2 - $title.Length - $paddingLeft
$centeredTitle = (' ' * $paddingLeft) + $title + (' ' * $paddingRight)

# Simple ASCII frame chars
$topLine    = "_" * $width
$bottomLine = "-" * $width
$sideChar   = "|"

# Helper to pad lines correctly
function Write-Line($text) {
    $padding = $width - 2 - $text.Length
    if ($padding -lt 0) { $padding = 0 }
    Write-Host "$sideChar$text" -NoNewline
    Write-Host (" " * $padding) -NoNewline
    Write-Host "$sideChar"
}

function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "Info"  # Info, Success, Warning, Error
    )
    $isRedirected = [Console]::IsOutputRedirected
    switch ($Level) {
        "Success" { $color = "Green" }
        "Warning" { $color = "Yellow" }
        "Error"   { $color = "Red" }
        default   { $color = "Cyan" }
    }
    if (-not $isRedirected) {
        Write-Host $Message -ForegroundColor $color
    } else {
        if ($Level -eq "Error") {
            Write-Error $Message
        } else {
            Write-Output $Message
            [Console]::Out.Flush()
        }
    }
}

Write-Log $topLine "Info"
Write-Line ($centeredTitle)
Write-Line ""  # empty line

Write-Log (" VCSA Hostname       : $VCSAName") "Info"
Write-Log (" ESXi Host           : $ESXiHost") "Info"
Write-Log (" Deployment Option   : $DeploymentOption") "Info"
Write-Log (" Network IP          : $IPAddress") "Info"
Write-Log (" Deployment Time     : $currentTime") "Info"
Write-Line ""  # empty line

Write-Log $bottomLine "Info"
Write-Log "" "Info"

if (-not $VCSARootPass) {
    Write-Log "[ERROR] VCSA root password (VCSA_ROOT_PASSWORD) is not set in the environment or .env file. Exiting." "Error"
    exit 1
}

if (Test-Path ".\vcenter-deploy.json") {
    Remove-Item ".\vcenter-deploy.json" -Force
}

# Convert booleans to lowercase strings for JSON
$ThinDiskModeJson = $ThinDiskMode.ToString().ToLower()
$CeipSettingsJson = $CeipSettings.ToString().ToLower()

# Generate JSON config
$jsonContent = @"
{
  "__version": "2.13",
  "new_vcsa": {
    "esxi": {
      "hostname": "$ESXiHost",
      "username": "$ESXiUser",
      "password": "$ESXiPassword",
      "deployment_network": "$DeploymentNetwork",
      "datastore": "$Datastore"
    },
    "appliance": {
      "thin_disk_mode": $ThinDiskModeJson,
      "deployment_option": "$DeploymentOption",
      "name": "$VmName"
    },
    "network": {
      "ip_family": "ipv4",
      "mode": "static",
      "ip": "$IPAddress",
      "dns_servers": [$(($DnsServers | ForEach-Object { "`"$_`"" }) -join ',')],
      "prefix": "$NetworkPrefix",
      "gateway": "$Gateway",
      "system_name": "$VCSAName"
    },
    "os": {
      "password": "$VCSARootPass",
      "ssh_enable": true,
      "ntp_servers": [$(($NTPServers | ForEach-Object { "`"$_`"" }) -join ',')]
    },
    "sso": {
      "password": "$VCPassword",
      "domain_name": "$SsoDomain"
    }
  },
  "ceip": {
    "settings": {
      "ceip_enabled": $CeipSettingsJson
    }
  }
}
"@

# Save JSON
$tempJsonPath = ".\vcenter-deploy.json"
$jsonContent | Set-Content $tempJsonPath
Write-Log "[+] Generated JSON config file at $tempJsonPath" "Success"

# PowerCLI install/setup logic (re-added for Get-VM functionality)
function Ensure-PowerCLI {
    if (Get-Command -Name Connect-VIServer -ErrorAction SilentlyContinue) {
        try {
            Get-PowerCLIConfiguration -ErrorAction Stop | Out-Null
            Set-PowerCLIConfiguration -InvalidCertificateAction Ignore -Confirm:$false | Out-Null
            Set-PowerCLIConfiguration -Scope User -ParticipateInCEIP $false -Confirm:$false | Out-Null
            return
        } catch {
            # fall through to reinstall
        }
    }

    Write-Log "[INFO] Installing or Reinstalling PowerCLI..." "Warning"
    # Aggressively uninstall all VMware modules to prevent assembly conflicts
    Get-Module -ListAvailable | Where-Object {$_.Name -like "VMware.*"} | Uninstall-Module -Force -ErrorAction SilentlyContinue

    if (-not (Get-PSRepository | Where-Object { $_.Name -eq 'PSGallery' })) {
        Register-PSRepository -Default
    }
    Set-PSRepository -Name PSGallery -InstallationPolicy Trusted

    if (-not (Get-PackageProvider -ListAvailable | Where-Object { $_.Name -eq 'NuGet' })) {
        Install-PackageProvider -Name NuGet -Force -Scope CurrentUser
    }

    Install-Module -Name VMware.PowerCLI -Scope CurrentUser -Force -ErrorAction Stop
    Import-Module VMware.PowerCLI -ErrorAction Stop
    # Explicitly import VMware.Vim as well
    Import-Module VMware.Vim -ErrorAction Stop
    Set-PowerCLIConfiguration -InvalidCertificateAction Ignore -Confirm:$false | Out-Null
}

Ensure-PowerCLI

# Connect to ESXi host for VM check
Write-Log "[+] Connecting to ESXi host $ESXiHost for VM check..." "Info"
Connect-VIServer -Server $ESXiHost -User $ESXiUser -Password $ESXiPassword -ErrorAction Stop | Out-Null

# Check for existing VM and proceed with deployment if not found
$vm = Get-VM -Name $VmName -ErrorAction SilentlyContinue

if ($vm) {
    Write-Log "[!] VM '$VmName' already exists. Exiting." "Warning"
    Disconnect-VIServer * -Confirm:$false | Out-Null
    exit 0
} else {
    Write-Log "[+] VM '$VmName' does not exist. Proceeding with deployment..." "Info"

    # Build deployment script content for the new window
    $deployScript = @"
`$ErrorActionPreference = 'Stop'
function Write-Log {
    param(
        [string]`$Message,
        [string]`$Level = 'Info'
    )
    `$isRedirected = [Console]::IsOutputRedirected
    switch (`$Level) {
        'Success' { `$color = 'Green' }
        'Warning' { `$color = 'Yellow' }
        'Error'   { `$color = 'Red' }
        default   { `$color = 'Cyan' }
    }
    if (-not `$isRedirected) {
        Write-Host `$Message -ForegroundColor `$color
    } else {
        if (`$Level -eq 'Error') {
            Write-Error `$Message
        } else {
            Write-Output `$Message
            [Console]::Out.Flush()
        }
    }
}
Write-Log '[+] Starting VCSA deployment...' 'Info'
& "$VCSADeployCLI" install "$tempJsonPath" --accept-eula --no-ssl-certificate-verification
`$exitCode = `$LASTEXITCODE
if (`$exitCode -eq 0) {
    Write-Log '[SUCCESS] VCSA deployment completed successfully.' 'Success'
} else {
    Write-Log '[ERROR] VCSA deployment failed.' 'Error'
}
exit `$exitCode
"@

    # Save script to temp .ps1
    $tempDeployScriptPath = "$env:TEMP\vcsa-deploy-temp.ps1"
    $deployScript | Set-Content -Path $tempDeployScriptPath -Encoding UTF8

    # Run the script in a hidden process and wait for it to finish
    $process = Start-Process powershell.exe -ArgumentList "-ExecutionPolicy Bypass -File `"$tempDeployScriptPath`"" -Wait -PassThru
    $exitCode = $process.ExitCode

    # Handle result based on exit code
    if ($exitCode -eq 0) {
        Write-Log "[SUCCESS] VCSA deployment completed successfully. Exiting." "Success"
        Disconnect-VIServer * -Confirm:$false | Out-Null
        exit 0
    } else {
        Write-Log "[ERROR] VCSA deployment failed. Exiting." "Error"
        Disconnect-VIServer * -Confirm:$false | Out-Null
        exit 1
    }
}