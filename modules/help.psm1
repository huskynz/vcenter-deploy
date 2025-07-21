Import-Module .\modules\metadata.psm1

$meta = Get-Metadata

function Show-Help {
    [CmdletBinding()]
    param()

    $helpText = @"
==============================
$($meta.ScriptCreatedBy) VCenter Deploy
==============================

What it does:

Reproducible VMware vCenter Server Appliance (VCSA) deployment automation. Deploy vCenter consistently across environments with version-controlled configuration. Perfect for labs, testing, and production where you need reliable, repeatable deployments.

USAGE:
    .\setup.ps1

REQUIRED SETUP:
    1. Copy env.example to .env and edit with your environment details.
    2. Ensure the vCenter ISO is mounted and vcsa-deploy.exe path is correct.

REQUIRED .env SETTINGS:

    Setting              Description                                 Default
    -------------------  -----------------------------------------  -----------------------------
    VCSA_CLI_PATH        Path to vcsa-deploy                        [drive]:\vcsa-cli-installer\win32\vcsa-deploy.exe
    ESXI_HOST            ESXi host IP/FQDN                          None
    ESXI_USER            ESXi username                              None
    ESXI_PASSWORD        ESXi password                              None
    VCSA_HOST            vCenter FQDN                               None
    VC_PASSWORD          vCenter SSO password                       None
    VCSA_ROOT_PASSWORD   vCenter root password                      None
    IP_ADDRESS           vCenter IP address                         None
    GATEWAY              Default gateway                            None
    DNS_SERVERS          DNS servers (comma-separated)              None
    NETWORK_PREFIX       Subnet mask (e.g., 24)                     None
    DEPLOYMENT_NETWORK   ESXi port group                            VM Network
    DATASTORE            ESXi datastore name                        None
    DEPLOYMENT_OPTION    vCenter size: tiny/small/medium/large/xl   small
    THIN_DISK_MODE       Thin provisioning: true/false              false
    NTP_SERVERS          NTP servers (comma-separated)              pool.ntp.org
    SSO_DOMAIN           SSO domain                                 vsphere.local
    CEIP_SETTINGS        Customer Experience Program: true/false    false

EXAMPLE:
    .\setup.ps1

OPTIONS:
    -Help, --help      Show this help message and exit
"@
    Write-Host $helpText -ForegroundColor Cyan
}

Export-ModuleMember -Function Show-Help 