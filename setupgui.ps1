Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# --- Helper: Show wait dialog with a message ---
function Show-WaitBox {
    param([string]$InitialMessage)

    $dlg = New-Object System.Windows.Forms.Form
    $dlg.Text = "Please Wait"
    $dlg.Size = New-Object System.Drawing.Size(400, 120)
    $dlg.StartPosition = 'CenterScreen'
    $dlg.FormBorderStyle = 'FixedDialog'
    $dlg.ControlBox = $false
    $dlg.TopMost = $true

    $label = New-Object System.Windows.Forms.Label
    $label.Text = $InitialMessage
    $label.AutoSize = $true
    $label.Location = New-Object System.Drawing.Point(20, 25)
    $dlg.Controls.Add($label)

    return @{ Form = $dlg; Label = $label }
}

# --- Helper: Create Label + TextBox at a given Y position ---
function Create-LabelAndTextBox {
    param(
        [string]$labelText,
        [int]$yPos,
        [int]$width = 280
    )
    $lbl = New-Object System.Windows.Forms.Label
    $lbl.Text = $labelText
    $lbl.Location = New-Object System.Drawing.Point(20, $yPos)
    $lbl.Size = New-Object System.Drawing.Size(150, 22)
    $lbl.TextAlign = 'MiddleRight'

    $txt = New-Object System.Windows.Forms.TextBox
    $txt.Location = New-Object System.Drawing.Point(180, $yPos - 3)
    $txt.Size = New-Object System.Drawing.Size($width, 25)

    return @{ Label = $lbl; TextBox = $txt }
}

# --- Load .env file if exists ---
function Import-DotEnv {
    param([string]$Path = ".env")
    if (-Not (Test-Path $Path)) {
        return $false
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
    return $true
}

# --- Ensure PowerCLI installed and configured ---
function Ensure-PowerCLI {
    try {
        if (-not (Get-Command Connect-VIServer -ErrorAction SilentlyContinue)) {
            throw "PowerCLI not installed"
        }

        Get-PowerCLIConfiguration -ErrorAction Stop | Out-Null
        Set-PowerCLIConfiguration -InvalidCertificateAction Ignore -Confirm:$false | Out-Null
        Set-PowerCLIConfiguration -Scope User -ParticipateInCEIP $false -Confirm:$false | Out-Null

        return $true
    } catch {
        # Install PowerCLI
        try {
            Write-Host "[INFO] Installing VMware.PowerCLI module..." -ForegroundColor Yellow
            # Uninstall conflicting VMware modules aggressively
            Get-Module -ListAvailable | Where-Object { $_.Name -like "VMware.*" } | Uninstall-Module -Force -ErrorAction SilentlyContinue

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

            return $true
        } catch {
            [System.Windows.Forms.MessageBox]::Show("Failed to install PowerCLI:`n$($_.Exception.Message)", "Error", 'OK', 'Error')
            return $false
        }
    }
}

# --- Main form setup ---
$form = New-Object System.Windows.Forms.Form
$form.Text = "VCSA Deployment GUI"
$form.Size = New-Object System.Drawing.Size(500, 560)
$form.StartPosition = 'CenterScreen'
$form.FormBorderStyle = 'FixedDialog'
$form.MaximizeBox = $false

# Define fields
$fields = @{
    "VCSA_HOST"      = "VCSA Hostname"
    "VM_NAME"        = "VM Name"
    "ESXI_HOST"      = "ESXi Host"
    "ESXI_USER"      = "ESXi Username"
    "ESXI_PASSWORD"  = "ESXi Password"
    "DEPLOYMENT_OPTION" = "Deployment Option"
    "IP_ADDRESS"     = "Network IP"
    "DEPLOYMENT_NETWORK" = "Deployment Network"
    "DATASTORE"      = "Datastore"
    "VCSA_ROOT_PASSWORD" = "VCSA Root Password"
    "VC_PASSWORD"    = "VC SSO Password"
    "NTP_SERVERS"    = "NTP Servers (comma-separated)"
    "DNS_SERVERS"    = "DNS Servers (comma-separated)"
    "NETWORK_PREFIX" = "Network Prefix (e.g. 24)"
    "GATEWAY"        = "Gateway"
    "SSO_DOMAIN"     = "SSO Domain"
    "CEIP_SETTINGS"  = "CEIP Enabled (true/false)"
    "ORG_NAME"       = "Organization Name"
}

# Try loading .env if exists, and set default values from environment vars
Import-DotEnv | Out-Null

# Controls dictionary for textboxes
$controls = @{}
[int]$y = 20

foreach ($field in $fields.GetEnumerator()) {
    $ctrls = Create-LabelAndTextBox -labelText $field.Value -yPos $y
    $form.Controls.Add($ctrls.Label)
    $form.Controls.Add($ctrls.TextBox)

    # Fill default from env if present
    $envVal = [System.Environment]::GetEnvironmentVariable($field.Key)
    if ($envVal) {
        $ctrls.TextBox.Text = $envVal
    }

    $controls[$field.Key] = $ctrls.TextBox
    $y += 35
}

# Deploy button
$btnDeploy = New-Object System.Windows.Forms.Button
$btnDeploy.Text = "Deploy VCSA"
$btnDeploy.Size = New-Object System.Drawing.Size(120, 35)
$btnDeploy.Location = New-Object System.Drawing.Point(180, $y + 10)
$form.Controls.Add($btnDeploy)

# Disable button while busy
$btnDeploy.Add_Click({
    $btnDeploy.Enabled = $false

    # Update environment variables based on form input
    foreach ($key in $controls.Keys) {
        [System.Environment]::SetEnvironmentVariable($key, $controls[$key].Text)
    }

    # Show wait dialog for PowerCLI check/install
    $wait = Show-WaitBox -InitialMessage "Checking PowerCLI installation, please wait..."
    $wait.Form.Show()

    $powercliInstalled = $false
    try {
        $powercliInstalled = Ensure-PowerCLI
    } finally {
        $wait.Form.Close()
    }

    if (-not $powercliInstalled) {
        [System.Windows.Forms.MessageBox]::Show("PowerCLI installation failed. Please fix and try again.", "Error", 'OK', 'Error')
        $btnDeploy.Enabled = $true
        return
    }

    # Show wait dialog for VM existence check
    $wait = Show-WaitBox -InitialMessage "Checking if VM exists, please wait..."
    $wait.Form.Show()

    try {
        # Connect to ESXi host and check VM
        Connect-VIServer -Server $controls["ESXI_HOST"].Text -User $controls["ESXI_USER"].Text -Password $controls["ESXI_PASSWORD"].Text -ErrorAction Stop | Out-Null
        $vm = Get-VM -Name $controls["VM_NAME"].Text -ErrorAction SilentlyContinue

        if ($vm) {
            $wait.Form.Close()
            [System.Windows.Forms.MessageBox]::Show("VM '$($controls["VM_NAME"].Text)' already exists! Opening vCenter UI.", "Info", 'OK', 'Information')
            Start-Process "https://$($controls["VCSA_HOST"].Text)/ui/"
            Disconnect-VIServer * -Confirm:$false | Out-Null
            $btnDeploy.Enabled = $true
            return
        }
    } catch {
        $wait.Form.Close()
        [System.Windows.Forms.MessageBox]::Show("Failed to check VM existence:`n$($_.Exception.Message)", "Error", 'OK', 'Error')
        $btnDeploy.Enabled = $true
        return
    }
    $wait.Form.Close()

    # Proceed with deployment
    try {
        # Generate JSON content for deployment config
        $ThinDiskModeJson = "false"  # or pull from form if you add that option
        $CeipSettingsJson = $controls["CEIP_SETTINGS"].Text.ToLower()

        $DnsServersArray = (($controls["DNS_SERVERS"].Text -split ',') | ForEach-Object { '"' + $_.Trim() + '"' }) -join ","
        $NtpServersArray = (($controls["NTP_SERVERS"].Text -split ',') | ForEach-Object { '"' + $_.Trim() + '"' }) -join ","


        $jsonContent = @"
{
  "__version": "2.13",
  "new_vcsa": {
    "esxi": {
      "hostname": "$($controls["ESXI_HOST"].Text)",
      "username": "$($controls["ESXI_USER"].Text)",
      "password": "$($controls["ESXI_PASSWORD"].Text)",
      "deployment_network": "$($controls["DEPLOYMENT_NETWORK"].Text)",
      "datastore": "$($controls["DATASTORE"].Text)"
    },
    "appliance": {
      "thin_disk_mode": $ThinDiskModeJson,
      "deployment_option": "$($controls["DEPLOYMENT_OPTION"].Text)",
      "name": "$($controls["VM_NAME"].Text)"
    },
    "network": {
      "ip_family": "ipv4",
      "mode": "static",
      "ip": "$($controls["IP_ADDRESS"].Text)",
      "dns_servers": [$DnsServersArray],
      "prefix": "$($controls["NETWORK_PREFIX"].Text)",
      "gateway": "$($controls["GATEWAY"].Text)",
      "system_name": "$($controls["VCSA_HOST"].Text)"
    },
    "os": {
      "password": "$($controls["VCSA_ROOT_PASSWORD"].Text)",
      "ssh_enable": true,
      "ntp_servers": [$NtpServersArray]
    },
    "sso": {
      "password": "$($controls["VC_PASSWORD"].Text)",
      "domain_name": "$($controls["SSO_DOMAIN"].Text)"
    }
  },
  "ceip": {
    "settings": {
      "ceip_enabled": $CeipSettingsJson
    }
  }
}
"@

        $jsonFilePath = Join-Path -Path $PSScriptRoot -ChildPath "vcenter-deploy.json"
        $jsonContent | Set-Content -Path $jsonFilePath -Encoding UTF8

        # Run deployment script in separate powershell window asynchronously
        $deployScript = @"
`$ErrorActionPreference = 'Stop'
Write-Host '[+] Starting VCSA deployment...' -ForegroundColor Cyan
& `"$($controls["VCSA_CLI_PATH"].Text)`" install `"$jsonFilePath`" --accept-eula --no-ssl-certificate-verification
`$exitCode = `$LASTEXITCODE
if (`$exitCode -eq 0) {
    Write-Host '[SUCCESS] VCSA deployment completed successfully.' -ForegroundColor Green
} else {
    Write-Host '[ERROR] VCSA deployment failed.' -ForegroundColor Red
}
exit `$exitCode
"@

        $deployScriptPath = Join-Path -Path $env:TEMP -ChildPath "vcsa-deploy-temp.ps1"
        $deployScript | Set-Content -Path $deployScriptPath -Encoding UTF8

        [System.Windows.Forms.MessageBox]::Show("Starting VCSA deployment in a new PowerShell window. Please wait for it to finish.", "Info")

        Start-Process powershell.exe -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-File", "`"$deployScriptPath`""

        # Disconnect from vCenter
        Disconnect-VIServer * -Confirm:$false | Out-Null
    } catch {
        [System.Windows.Forms.MessageBox]::Show("Deployment failed:`n$($_.Exception.Message)", "Error", 'OK', 'Error')
    }

    $btnDeploy.Enabled = $true
})

[void] $form.ShowDialog()
