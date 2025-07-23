# ===============================
# HuskyNZ VCSA Deployment Script
# ===============================
#
# Purpose: Reproducible VMware vCenter Server Appliance Deployment
# Author:  HuskyNZ
# Version: 4.0
#
# This script automates the deployment of vCenter Server Appliance
# using the VMware CLI installer with configuration from .env file.
# ===============================


$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = "Stop"


# Show help if -Help or --help is passed
if ($args -contains '-Help' -or $args -contains '--help' -or $args -contains '-?' -or $args -contains '--?') {
    Import-Module .\modules\help\help.psm1
    Show-Help
    exit 0
}

# Import modules
Import-Module .\modules\logging\logging.psm1
Import-Module .\modules\powercli\powercli.psm1
Import-Module .\modules\environment\environment.psm1
Import-Module .\modules\config\config.psm1
Import-Module .\modules\vsphere\vsphere.psm1
Import-Module .\modules\deployment\deployment.psm1
Import-Module .\modules\metadata\metadata.psm1
Import-Module .\modules\envcheck\envcheck.psm1
Import-Module .\modules\genaratejson\genaratejson.psm1

$env:PROJECT_ROOT = $ScriptRoot


# Load environment variables
$config = Get-EnvironmentVariables

# Validate required environment variables
Import-EnvCheck


$meta = Get-Metadata
# Show banner
Show-Banner -ScriptVersion $meta.ScriptVersion -ScriptLastUpdatedOn $meta.ScriptLastUpdatedOn -VCSAName $config.VCSAName -ESXiHost $config.ESXiHost -DeploymentOption $config.DeploymentOption -IPAddress $config.IPAddress

Write-Json


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