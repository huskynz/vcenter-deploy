# Ensure PowerCLI is installed
function Invoke-PcliCheck {
    [CmdletBinding()]
    param()

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

Export-ModuleMember -Function Invoke-PcliCheck
