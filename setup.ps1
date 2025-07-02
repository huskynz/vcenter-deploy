# ================================
# HuskyNZ Vcenter Deployment Script
# Version 1.1
# Modular, robust, and cleaner version of your deployment script
# ================================

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

function Write-Log {
    param(
        [string]$Message,
        [ValidateSet('INFO','WARN','ERROR','SUCCESS')]
        [string]$Level = 'INFO'
    )
    switch ($Level) {
        'INFO'    { $color = 'Cyan' }
        'WARN'    { $color = 'Yellow' }
        'ERROR'   { $color = 'Red' }
        'SUCCESS' { $color = 'Green' }
    }
    Write-Host "[$Level] $Message" -ForegroundColor $color
}

function Load-Config {
    Import-DotEnv

    $config = [PSCustomObject]@{
        VCSADeployCLI    = $env:VCSA_CLI_PATH
        ESXiHost         = $env:ESXI_HOST
        ESXiUser         = $env:ESXI_USER
        ESXiPassword     = $env:ESXI_PASSWORD
        VCSAName         = $env:VCSA_HOST
        VCSARootPass     = $env:VCSA_ROOT_PASSWORD
        VCPassword       = $env:VC_PASSWORD
        NTPServers       = ($env:NTP_SERVERS -split ',').Trim()
        DeploymentNetwork= $env:DEPLOYMENT_NETWORK
        Datastore        = $env:DATASTORE
        ThinDiskMode     = [bool]::Parse($env:THIN_DISK_MODE)
        DeploymentOption = $env:DEPLOYMENT_OPTION
        IPAddress        = $env:IP_ADDRESS
        DnsServers       = ($env:DNS_SERVERS -split ',').Trim()
        NetworkPrefix    = $env:NETWORK_PREFIX
        Gateway          = $env:GATEWAY
        SsoDomain        = $env:SSO_DOMAIN
        CeipSettings     = [bool]::Parse($env:CEIP_SETTINGS)
        OrgName          = $env:ORG_NAME
        VmName           = $env:VM_NAME
    }

    # Validate critical variables
    foreach ($key in @('VCSADeployCLI','ESXiHost','ESXiUser','ESXiPassword','VCSAName','VCSARootPass','VCPassword','VmName')) {
        if (-not $config.$key) {
            throw "Configuration error: '$key' is not set."
        }
    }

    return $config
}

function Show-Header {
    param($OrgName, $VmName, $VCSAName, $ESXiHost, $DeploymentOption, $IPAddress)
    $width = 60
    $title = "$OrgName Vcenter Deployment Script"
    $sideChar = "|"
    $bottomLine = "-" * $width

    function Write-CenteredLine($text) {
        $textLength = $text.Length
        $spaceEachSide = [int][math]::Floor(($width - $textLength) / 2)
        $extraSpace = ($width - $textLength) % 2
        $line = ('=' * $spaceEachSide) + $text + ('=' * ($spaceEachSide + $extraSpace))
        Write-Host $line
    }

    function Write-Line($text) {
        $padding = $width - 2 - $text.Length
        if ($padding -lt 0) { $padding = 0 }
        Write-Host "$sideChar$text" -NoNewline
        Write-Host (" " * $padding) -NoNewline
        Write-Host "$sideChar"
    }

    $now = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

    Write-CenteredLine $title
    Write-Line ""
    Write-Line " VM Name             : $VmName"
    Write-Line " VCSA Hostname       : $VCSAName"
    Write-Line " ESXi Host           : $ESXiHost"
    Write-Line " Deployment Option   : $DeploymentOption"
    Write-Line " Network IP          : $IPAddress"
    Write-Line " Deployment Time     : $now"
    Write-Line ""
    Write-CenteredLine
    Write-Host ""
}

function Generate-JsonConfig {
    param(
        [PSCustomObject]$config,
        [string]$Path
    )

    $jsonObject = @{
        __version = "2.13"
        new_vcsa = @{
            esxi = @{
                hostname = $config.ESXiHost
                username = $config.ESXiUser
                password = $config.ESXiPassword
                deployment_network = $config.DeploymentNetwork
                datastore = $config.Datastore
            }
            appliance = @{
                thin_disk_mode = $config.ThinDiskMode
                deployment_option = $config.DeploymentOption
                name = $config.VmName
            }
            network = @{
                ip_family = "ipv4"
                mode = "static"
                ip = $config.IPAddress
                dns_servers = $config.DnsServers
                prefix = $config.NetworkPrefix
                gateway = $config.Gateway
                system_name = $config.VCSAName
            }
            os = @{
                password = $config.VCSARootPass
                ssh_enable = $true
                ntp_servers = $config.NTPServers
            }
            sso = @{
                password = $config.VCPassword
                domain_name = $config.SsoDomain
            }
        }
        ceip = @{
            settings = @{
                ceip_enabled = $config.CeipSettings
            }
        }
    }

    $jsonObject | ConvertTo-Json -Depth 4 | Set-Content -Path $Path -Encoding UTF8
    Write-Log "JSON config file generated at $Path" -Level SUCCESS
}

function Ensure-PowerCLI {
    if (Get-Command -Name Connect-VIServer -ErrorAction SilentlyContinue) {
        try {
            Get-PowerCLIConfiguration -ErrorAction Stop | Out-Null
            Set-PowerCLIConfiguration -InvalidCertificateAction Ignore -Confirm:$false | Out-Null
            Set-PowerCLIConfiguration -Scope User -ParticipateInCEIP $false -Confirm:$false | Out-Null
            Write-Log "PowerCLI already configured." -Level INFO
            return
        } catch {
            Write-Log "PowerCLI configuration corrupted, reinstalling..." -Level WARN
        }
    }
    Write-Log "Installing/Updating PowerCLI..." -Level INFO
    # Remove conflicting VMware modules
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
    Import-Module VMware.Vim -ErrorAction Stop
    Set-PowerCLIConfiguration -InvalidCertificateAction Ignore -Confirm:$false | Out-Null
    Write-Log "PowerCLI setup completed." -Level SUCCESS
}

function Check-VMExists {
    param([string]$VmName)
    try {
        return Get-VM -Name $VmName -ErrorAction Stop
    } catch {
        return $null
    }
}

function Deploy-VCSA {
    param(
        [string]$DeployCLI,
        [string]$JsonPath
    )

    $deployScriptContent = @"
`$ErrorActionPreference = 'Stop'
Write-Host '[+] Starting VCSA deployment...' -ForegroundColor Cyan
& `"$DeployCLI`" install `"$JsonPath`" --accept-eula --no-ssl-certificate-verification
`$exitCode = `$LASTEXITCODE

if (`$exitCode -eq 0) {
    Write-Host '[SUCCESS] VCSA deployment completed successfully.' -ForegroundColor Green
} else {
    Write-Host '[ERROR] VCSA deployment failed.' -ForegroundColor Red
}
exit `$exitCode
"@

    $tempDeployScriptPath = Join-Path $PWD "appliance-deploy.ps1"
    $deployScriptContent | Set-Content -Path $tempDeployScriptPath -Encoding UTF8

    Write-Log "Starting VCSA deployment script..." -Level INFO
    $process = Start-Process powershell.exe -ArgumentList "-ExecutionPolicy Bypass -File `"$tempDeployScriptPath`"" -Wait -PassThru
    Remove-Item -Path $tempDeployScriptPath -Force
    return $process.ExitCode
}

# === Main execution ===

try {
    Write-Log "Starting vCenter Deployment Script..." -Level INFO

    $config = Load-Config

    Show-Header -OrgName $config.OrgName -VmName $config.VmName -VCSAName $config.VCSAName -ESXiHost $config.ESXiHost -DeploymentOption $config.DeploymentOption -IPAddress $config.IPAddress

    Write-Log "Preparing deployment files..." -Level INFO
    $jsonConfigPath = Join-Path $PWD "vcenter-deploy.json"
    if (Test-Path $jsonConfigPath) {
        Remove-Item $jsonConfigPath -Force
        Write-Log "Removed existing JSON config." -Level INFO
    }

    Generate-JsonConfig -config $config -Path $jsonConfigPath

    Ensure-PowerCLI

    Write-Log "Connecting to ESXi host $($config.ESXiHost)..." -Level INFO
    Connect-VIServer -Server $config.ESXiHost -User $config.ESXiUser -Password $config.ESXiPassword -ErrorAction Stop | Out-Null
    Write-Log "Connected to ESXi host." -Level SUCCESS

    $vm = Check-VMExists -VmName $config.VmName
    if ($vm) {
        Write-Log "VM '$($config.VmName)' already exists. Opening vCenter interface." -Level WARN
        Start-Process "https://$($config.VCSAName)/ui/"
    } else {
        Write-Log "VM '$($config.VmName)' does not exist. Proceeding with deployment." -Level INFO
        $exitCode = Deploy-VCSA -DeployCLI $config.VCSADeployCLI -JsonPath $jsonConfigPath
        if ($exitCode -eq 0) {
            Write-Log "Deployment completed successfully." -Level SUCCESS
            Start-Process "https://$($config.VCSAName)/ui/"
        } else {
            Write-Log "Deployment failed with exit code $exitCode." -Level ERROR
            exit $exitCode
        }
    }

    Disconnect-VIServer -Confirm:$false | Out-Null
    Write-Log "Disconnected from ESXi host. Script complete." -Level INFO
} catch {
    Write-Log "Fatal error: $_" -Level ERROR
    exit 1
}
