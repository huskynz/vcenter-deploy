. (Join-Path $PSScriptRoot '..\logging\logging.psm1')

function Connect-ESXiHost {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [string]$ESXiHost,
        [Parameter(Mandatory=$true)]
        [string]$ESXiUser,
        [Parameter(Mandatory=$true)]
        [string]$ESXiPassword
    )
    Write-Log "[+] Connecting to ESXi host $ESXiHost for VM check..." "Info"
    Connect-VIServer -Server $ESXiHost -User $ESXiUser -Password $ESXiPassword -ErrorAction Stop | Out-Null
}

function Test-VMExists {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [string]$VmName
    )
    $vm = Get-VM -Name $VmName -ErrorAction SilentlyContinue
    return $vm -ne $null
}

function Disconnect-ESXiHost {
    [CmdletBinding()]
    param()
    Disconnect-VIServer * -Confirm:$false | Out-Null
}

Export-ModuleMember -Function Connect-ESXiHost, Test-VMExists, Disconnect-ESXiHost
