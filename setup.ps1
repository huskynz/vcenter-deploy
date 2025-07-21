# ===============================
# HuskyNZ VCSA Deployment Script
# ===============================
#
# Purpose: Reproducible VMware vCenter Server Appliance Deployment
# Author:  HuskyNZ
# Version: 3.0
#
# This script automates the deployment of vCenter Server Appliance
# using the VMware CLI installer with configuration from .env file.
# ===============================

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = "Stop"

# Show help if -Help or --help is passed
if ($args -contains '-Help' -or $args -contains '--help' -or $args -contains '-?' -or $args -contains '--?') {
    Import-Module .\modules\help.psm1
    Show-Help
    exit 0
}

# Import modules
Import-Module .\modules\logging.psm1
Import-Module .\modules\powercli.psm1
Import-Module .\modules\environment.psm1
Import-Module .\modules\config.psm1
Import-Module .\modules\vsphere.psm1
Import-Module .\modules\deployment.psm1
Import-Module .\modules\metadata.psm1

if (-not (Get-Command Invoke-PcliCheck -ErrorAction SilentlyContinue)) {
    Write-Error "Invoke-PcliCheck function is not available after importing the module."
    exit 1
}
Invoke-PcliCheck


# Load environment variables
$config = Get-EnvironmentVariables

# Validate required environment variables
$requiredVariables = @(
    'VCSADeployCLI',
    'ESXiHost',
    'ESXiUser',
    'ESXiPassword',
    'VCSAName',
    'VCPassword',
    'VCSARootPass',
    'IPAddress',
    'Gateway',
    'DnsServers',
    'NetworkPrefix',
    'Datastore',
    'VmName'
)

foreach ($variable in $requiredVariables) {
    if (-not $config[$variable]) {
        Write-Log "[ERROR] Required environment variable '$variable' is not set in the .env file. Exiting." "Error"
        exit 1
    }
}

$meta = Get-Metadata

# Show banner
Show-Banner -ScriptVersion $($meta.ScriptVersion) -ScriptLastUpdatedOn $($meta.ScriptLastUpdatedOn) -VCSAName $config.VCSAName -ESXiHost $config.ESXiHost -DeploymentOption $config.DeploymentOption -IPAddress $config.IPAddress

# Generate JSON config
if (Test-Path ".\vcenter-deploy.json") {
    Remove-Item ".\vcenter-deploy.json" -Force
}
$tempJsonPath = New-VcenterJsonConfig -config $config
Write-Log "[+] Generated JSON config file at $tempJsonPath" "Success"

# Connect to ESXi and check for existing VM
Connect-ESXiHost -ESXiHost $config.ESXiHost -ESXiUser $config.ESXiUser -ESXiPassword $config.ESXiPassword

if (Test-VMExists -VmName $config.VmName) {
    Write-Log "[!] VM '$($config.VmName)' already exists. Exiting." "Warning"
    Disconnect-ESXiHost
    exit 0
}

Write-Log "[+] VM '$($config.VmName)' does not exist. Proceeding with deployment..." "Info"

# Start deployment
$exitCode = Start-VCSADeployment -VCSADeployCLI $config.VCSADeployCLI -tempJsonPath $tempJsonPath

# Handle result
if ($exitCode -eq 0) {
    Write-Log "[SUCCESS] VCSA deployment completed successfully. Exiting." "Success"
} else {
    Write-Log "[ERROR] VCSA deployment failed. Exiting." "Error"
}

Disconnect-ESXiHost
exit $exitCode