function New-VcenterJsonConfig {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [hashtable]$config
    )

    $ThinDiskModeJson = $config.ThinDiskMode.ToString().ToLower()
    $CeipSettingsJson = $config.CeipSettings.ToString().ToLower()

    $jsonContent = @"
{
  "__version": "2.13",
  "new_vcsa": {
    "esxi": {
      "hostname": "$($config.ESXiHost)",
      "username": "$($config.ESXiUser)",
      "password": "$($config.ESXiPassword)",
      "deployment_network": "$($config.DeploymentNetwork)",
      "datastore": "$($config.Datastore)"
    },
    "appliance": {
      "thin_disk_mode": $ThinDiskModeJson,
      "deployment_option": "$($config.DeploymentOption)",
      "name": "$($config.VmName)"
    },
    "network": {
      "ip_family": "ipv4",
      "mode": "static",
      "ip": "$($config.IPAddress)",
      "dns_servers": [$(($config.DnsServers | ForEach-Object { "`"$_`"" }) -join ',')],
      "prefix": "$($config.NetworkPrefix)",
      "gateway": "$($config.Gateway)",
      "system_name": "$($config.VCSAName)"
    },
    "os": {
      "password": "$($config.VCSARootPass)",
      "ssh_enable": true,
      "ntp_servers": [$(($config.NTPServers | ForEach-Object { "`"$_`"" }) -join ',')]
    },
    "sso": {
      "password": "$($config.VCPassword)",
      "domain_name": "$($config.SsoDomain)"
    }
  },
  "ceip": {
    "settings": {
      "ceip_enabled": $CeipSettingsJson
    }
  }
}
"@
    $tempJsonPath = ".\vcenter-deploy.json"
    $jsonContent | Set-Content -Path $tempJsonPath
    return $tempJsonPath
}

Export-ModuleMember -Function New-VcenterJsonConfig