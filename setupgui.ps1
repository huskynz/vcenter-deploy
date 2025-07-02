$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# Form size
[int]$formWidth = 700
[int]$formHeight = 680

$form = New-Object System.Windows.Forms.Form
$form.Text = "HuskyNZ VCSA Deployment Tool"
$form.Size = [System.Drawing.Size]::new($formWidth, $formHeight)
$form.StartPosition = "CenterScreen"

# Scrollable panel
$panel = New-Object System.Windows.Forms.Panel
$panel.Location = [System.Drawing.Point]::new(10, 10)
$panel.Size = [System.Drawing.Size]::new($formWidth - 40, 480)
$panel.AutoScroll = $true
$form.Controls.Add($panel)

# Fields to edit
$envVars = @(
    "ORG_NAME", "VM_NAME", "VCSA_CLI_PATH", "VCSA_HOST",
    "VC_PASSWORD", "VCSA_ROOT_PASSWORD", "ESXI_HOST",
    "ESXI_USER", "ESXI_PASSWORD", "NTP_SERVERS", "DEPLOYMENT_NETWORK",
    "DATASTORE", "THIN_DISK_MODE", "DEPLOYMENT_OPTION",
    "IP_ADDRESS", "DNS_SERVERS", "NETWORK_PREFIX", "GATEWAY",
    "SSO_DOMAIN", "CEIP_SETTINGS"
)

# TextBoxes dictionary
$textBoxes = @{}
[int]$yPos = 10

foreach ($var in $envVars) {
    $label = New-Object System.Windows.Forms.Label
    $label.Text = $var
    $label.Size = [System.Drawing.Size]::new(180, 20)
    $label.Location = [System.Drawing.Point]::new(10, $yPos + 3)

    $textBox = New-Object System.Windows.Forms.TextBox
    $textBox.Size = [System.Drawing.Size]::new(420, 20)
    $textBox.Location = [System.Drawing.Point]::new(200, $yPos)
    $textBoxes[$var] = $textBox

    $panel.Controls.Add($label)
    $panel.Controls.Add($textBox)
    $yPos += 30
}

# Output textbox
$txtOutput = New-Object System.Windows.Forms.TextBox
$txtOutput.Multiline = $true
$txtOutput.ScrollBars = "Vertical"
$txtOutput.ReadOnly = $true
$txtOutput.Location = [System.Drawing.Point]::new(10, 500)
$txtOutput.Size = [System.Drawing.Size]::new($formWidth - 40, 120)
$form.Controls.Add($txtOutput)

# Status label
$lblStatus = New-Object System.Windows.Forms.Label
$lblStatus.Text = "Status: Waiting"
$lblStatus.AutoSize = $true
$lblStatus.Location = [System.Drawing.Point]::new(10, 630)
$form.Controls.Add($lblStatus)

# Deploy button
$btnDeploy = New-Object System.Windows.Forms.Button
$btnDeploy.Text = "Start Deployment"
$btnDeploy.Size = [System.Drawing.Size]::new(160, 30)
$btnDeploy.Location = [System.Drawing.Point]::new($formWidth - 190, 625)
$form.Controls.Add($btnDeploy)

# Load .env file
function Load-EnvFile {
    if (-not (Test-Path ".env")) { return }
    Get-Content ".env" | ForEach-Object {
        if ($_ -match "^\s*#|^\s*$") { return }
        $parts = $_ -split "=", 2
        if ($parts.Length -eq 2) {
            $key = $parts[0].Trim()
            $value = $parts[1].Trim().Trim('"')
            if ($textBoxes.ContainsKey($key)) {
                $textBoxes[$key].Text = $value
            }
        }
    }
}

# Save form fields back to .env
function Save-EnvFile {
    $lines = @()
    foreach ($key in $envVars) {
        $value = $textBoxes[$key].Text.Replace('"', '')
        $lines += "$key=""$value"""
    }
    Set-Content -Path ".env" -Value $lines
}

# Button action
$btnDeploy.Add_Click({
    $btnDeploy.Enabled = $false
    $lblStatus.Text = "Status: Running..."
    $txtOutput.Clear()

    Save-EnvFile

    $scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $PSCommandPath }
    $script = Join-Path -Path $scriptDir -ChildPath "setup.ps1" 
    if (-not (Test-Path $script)) {
        [System.Windows.Forms.MessageBox]::Show("Missing setup.ps1","Error")
        $btnDeploy.Enabled = $true
        return
    }

    $setupScriptPath = Join-Path $ScriptRoot "setup.ps1"
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "powershell.exe"
    $psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -File $script"
    $psi.WorkingDirectory = $ScriptRoot  
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true

    $proc = New-Object System.Diagnostics.Process
    $proc.StartInfo = $psi
    $proc.Start() | Out-Null

    $stdout = $proc.StandardOutput
    $stderr = $proc.StandardError

    while (-not $proc.HasExited) {
        $line = $stdout.ReadLine()
        if ($line) { $txtOutput.AppendText($line + "`r`n") }
        Start-Sleep -Milliseconds 100
    }

    while (-not $stdout.EndOfStream) {
        $txtOutput.AppendText($stdout.ReadLine() + "`r`n")
    }
    while (-not $stderr.EndOfStream) {
        $txtOutput.AppendText("[ERROR] " + $stderr.ReadLine() + "`r`n")
    }

    if ($proc.ExitCode -eq 0) {
        $lblStatus.Text = "Status: Completed successfully."
    } else {
        $lblStatus.Text = "Status: Failed. Exit code $($proc.ExitCode)"
    }

    $btnDeploy.Enabled = $true
})

# Load values into form
Load-EnvFile
$form.ShowDialog()
