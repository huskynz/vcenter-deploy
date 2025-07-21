function Import-DotEnv {
    [CmdletBinding()]
    param(
        [string]$Path = ".env"
    )
    if (-Not (Test-Path $Path)) {
        throw "Env file $Path not found"
    }
    $envVars = @{}
    Get-Content $Path | ForEach-Object {
        if ($_ -match '^\s*#') { return }
        if ($_ -match '^\s*$') { return }
        $parts = $_ -split '=', 2
        if ($parts.Length -eq 2) {
            $key = $parts[0].Trim()
            $value = $parts[1].Trim()
            $envVars[$key] = $value
        }
    }
    return $envVars
}

function Get-EnvironmentVariables {
    $loadedEnv = Import-DotEnv
    return @{
        VCSADeployCLI   = $loadedEnv.VCSA_CLI_PATH
        ESXiHost        = $loadedEnv.ESXI_HOST
        ESXiUser        = $loadedEnv.ESXI_USER
        ESXiPassword    = $loadedEnv.ESXI_PASSWORD
        VCSAName        = $loadedEnv.VCSA_HOST
        VCSARootPass    = $loadedEnv.VCSA_ROOT_PASSWORD
        VCPassword      = $loadedEnv.VC_PASSWORD
        NTPServers      = ($loadedEnv.NTP_SERVERS -split ',')
        DeploymentNetwork = $loadedEnv.DEPLOYMENT_NETWORK
        Datastore         = $loadedEnv.DATASTORE
        ThinDiskMode      = ($loadedEnv.THIN_DISK_MODE -match '^(1|true)$')
        DeploymentOption  = $loadedEnv.DEPLOYMENT_OPTION
        IPAddress        = $loadedEnv.IP_ADDRESS
        DnsServers       = ($loadedEnv.DNS_SERVERS -split ',')
        NetworkPrefix    = $loadedEnv.NETWORK_PREFIX
        Gateway          = $loadedEnv.GATEWAY
        SsoDomain        = $loadedEnv.SSO_DOMAIN
        CeipSettings     = ($loadedEnv.CEIP_SETTINGS -match '^(1|true)$')
        VmName           = $loadedEnv.VM_NAME
    }
}

Export-ModuleMember -Function Get-EnvironmentVariables
