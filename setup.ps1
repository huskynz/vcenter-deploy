# ===============================
# Full VCSA Deployment + Post-Config Script with Licensing
# ===============================

$ErrorActionPreference = "Stop"

function Import-DotEnv {
    param([string]$Path = ".env")
    if (-Not (Test-Path $Path)) {
        throw "Env file $Path not found"
    }
    Get-Content $Path | ForEach-Object {
        if ($_ -match '^\s*#') { return }   # skip comments
        if ($_ -match '^\s*$') { return }   # skip empty lines
        $parts = $_ -split '=', 2
        if ($parts.Length -eq 2) {
            $key = $parts[0].Trim()
            $value = $parts[1].Trim('"')  # Remove quotes if present
            [System.Environment]::SetEnvironmentVariable($key, $value)
        }
    }
}

# Load environment variables
Import-DotEnv

# Read env vars
$VCSADeployCLI   = $env:VCSA_CLI_PATH
$VCSAHost        = $env:VCSA_HOST

$ESXiHost        = $env:ESXI_HOST
$ESXiUser        = $env:ESXI_USER
$ESXiPassword    = $env:ESXI_PASSWORD

$VCSAName        = $env:VCSA_HOST
$VCSARootPass    = $env:VCSA_ROOT_PASSWORD

$VCPassword      = $env:VC_PASSWORD

$DatacenterName  = $env:DATACENTER_NAME
$ClusterName     = $env:CLUSTER_NAME

$NTPServers      = $env:NTP_SERVERS -split ','

$DeploymentNetwork = $env:DEPLOYMENT_NETWORK
$Datastore         = $env:DATASTORE
$ThinDiskMode      = [bool]::Parse($env:THIN_DISK_MODE)
$DeploymentOption  = $env:DEPLOYMENT_OPTION

$IPAddress        = $env:IP_ADDRESS
$DnsServers       = $env:DNS_SERVERS -split ','
$NetworkPrefix    = $env:NETWORK_PREFIX
$Gateway          = $env:GATEWAY

$SsoDomain        = $env:SSO_DOMAIN
$SsoSite          = $env:SSO_SITE

$CeipSettings     = [bool]::Parse($env:CEIP_SETTINGS)

$BannerMsg        = $env:BANNER_MSG
$LegalNotice      = $env:LEGAL_NOTICE
$InstanceName     = $env:INSTANCE_NAME
$OrgName          = $env:ORG_NAME

$VCLicenseKey     = $env:VC_LICENSE_KEY
$ESXiLicenseKey   = $env:ESXI_LICENSE_KEY

if (-not $VCSARootPass) {
    Write-Host "[ERROR] VCSA root password ([1mVCSA_ROOT_PASSWORD[0m) is not set in the environment or .env file. Exiting." -ForegroundColor Red
    exit 1
}

# Generate JSON config object
$deployConfig = [PSCustomObject]@{
  __version = "2.13"
  new_vcsa = @{
    esxi = @{
      hostname = $ESXiHost
      username = $ESXiUser
      password = $ESXiPassword
      deployment_network = $DeploymentNetwork
      datastore = $Datastore
    }
    appliance = @{
      thin_disk_mode = $ThinDiskMode
      deployment_option = $DeploymentOption
      name = $VCSAName
    }
    network = @{
      ip_family = "ipv4"
      mode = "static"
      ip = $IPAddress
      dns_servers = $DnsServers
      prefix = [int]$NetworkPrefix
      gateway = $Gateway
      system_name = $VCSAName
    }
    os = @{
      password = $VCSARootPass
      ssh_enable = $true
      time_tools_sync = $true
      ntp_servers = $NTPServers
    }
    sso = @{
      password = $VCPassword
      domain_name = $SsoDomain
      site_name = $SsoSite
    }
  }
  ceip = @{
    settings = $CeipSettings
  }
}

# Save JSON to file
$tempJsonPath = ".\vcenter-deploy.json"
$deployConfig | ConvertTo-Json -Depth 10 | Set-Content $tempJsonPath
Write-Host "[+] Generated JSON config file at $tempJsonPath" -ForegroundColor Green

# Run the VCSA deploy CLI
Write-Host "[+] Starting VCSA deployment..." -ForegroundColor Cyan
& $VCSADeployCLI install $tempJsonPath --accept-eula --acknowledge-ceip --no-ssl-certificate-verification

# Wait for vCenter API to become responsive
Write-Host "[+] Waiting for vCenter to become reachable..."
do {
    Start-Sleep -Seconds 10
    try {
        $resp = Invoke-WebRequest -Uri "https://$VCSAHost" -UseBasicParsing -TimeoutSec 5
    } catch { $resp = $null }
} while (-not $resp)
Write-Host "[✓] vCenter is up." -ForegroundColor Green

# Load PowerCLI
if (-not (Get-Module -ListAvailable -Name VMware.PowerCLI)) {
    Install-Module -Name VMware.PowerCLI -Scope CurrentUser -Force
}
Import-Module VMware.PowerCLI
Set-PowerCLIConfiguration -InvalidCertificateAction Ignore -Confirm:$false | Out-Null

# Connect to vCenter
Write-Host "[+] Connecting to vCenter..."
Connect-VIServer -Server $VCSAHost -User "administrator@vsphere.local" -Password $VCPassword | Out-Null

# Create datacenter
Write-Host "[+] Creating datacenter $DatacenterName..."
$dc = New-Datacenter -Name $DatacenterName

# Create cluster with DRS and HA enabled
Write-Host "[+] Creating cluster $ClusterName..."
$cluster = New-Cluster -Name $ClusterName -Location $dc -DrsEnabled:$true -HAEnabled:$true

# Add ESXi host to cluster
Write-Host "[+] Adding ESXi host $ESXiHost..."
Add-VMHost -Name $ESXiHost -Location $cluster -User $ESXiUser -Password $ESXiPassword | Out-Null

# Configure NTP servers on ESXi host
Write-Host "[+] Configuring NTP servers on ESXi host..."
$vmhost = Get-VMHost -Name $ESXiHost
foreach ($ntp in $NTPServers) {
    Add-VMHostNtpServer -VMHost $vmhost -NtpServer $ntp -Confirm:$false
}
Start-VMHostService -HostService (Get-VMHostService -VMHost $vmhost | Where-Object {$_.Key -eq "ntpd"})

# Apply branding and login banner settings
Write-Host "[+] Applying branding and login banner..."
Get-AdvancedSetting -Entity $global:DefaultVIServer -Name "Security.LoginBanner" | Set-AdvancedSetting -Value $BannerMsg -Confirm:$false
Get-AdvancedSetting -Entity $global:DefaultVIServer -Name "Security.LoginBannerEnable" | Set-AdvancedSetting -Value "true" -Confirm:$false
Get-AdvancedSetting -Entity $global:DefaultVIServer -Name "vpxd.banner" | Set-AdvancedSetting -Value $LegalNotice -Confirm:$false
Set-AdvancedSetting -Entity $global:DefaultVIServer -Name "VirtualCenter.InstanceName" -Value $InstanceName -Confirm:$false
Set-AdvancedSetting -Entity $global:DefaultVIServer -Name "VirtualCenter.OrganizationName" -Value $OrgName -Confirm:$false

# Assign license key to vCenter
if ($VCLicenseKey) {
    Write-Host "[+] Assigning license key to vCenter..."
    $licenseManager = Get-View LicenseManager
    $licenseManager.AddLicense($VCLicenseKey)
    $licenseManager.UpdateLicenseLabel($VCLicenseKey, "vCenter License")
    $licenseManager.SetLicenseEdition($VCLicenseKey, "enterprise-plus") # adjust edition as needed
}

# Assign license key to ESXi host
if ($ESXiLicenseKey) {
    Write-Host "[+] Assigning license key to ESXi host..."
    $vmhostView = Get-View -Id $vmhost.Id
    $licenseAssignmentManager = Get-View $licenseManager.ExtensionData.LicenseAssignmentManager
    $entity = New-Object VMware.Vim.ManagedObjectReference
    $entity.Type = "HostSystem"
    $entity.Value = $vmhostView.MoRef.Value
    $licenseAssignmentManager.AssignLicense($ESXiLicenseKey, $entity)
}

# Disconnect from vCenter
Disconnect-VIServer * -Confirm:$false | Out-Null
Write-Host "[✓] Deployment, licensing and post-configuration complete." -ForegroundColor Green
