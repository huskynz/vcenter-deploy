# ===============================
# THIS IS A HIGHLY EXPERIMENTAL GUI THAT MAY BREAK
# ===============================

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# Windows 10/11 Dark Mode color scheme
$colors = @{
    Primary = [System.Drawing.Color]::FromArgb(0, 120, 215)      # Windows Blue
    PrimaryLight = [System.Drawing.Color]::FromArgb(0, 99, 177)  # Darker Blue
    Secondary = [System.Drawing.Color]::FromArgb(107, 105, 214)  # Windows Purple
    Success = [System.Drawing.Color]::FromArgb(16, 185, 129)     # Modern Green
    SuccessLight = [System.Drawing.Color]::FromArgb(34, 197, 94) # Light Green
    Warning = [System.Drawing.Color]::FromArgb(245, 158, 11)     # Modern Orange
    Danger = [System.Drawing.Color]::FromArgb(239, 68, 68)       # Modern Red
    Background = [System.Drawing.Color]::FromArgb(17, 24, 39)    # Dark Background
    Surface = [System.Drawing.Color]::FromArgb(31, 41, 55)       # Dark Surface
    SurfaceLight = [System.Drawing.Color]::FromArgb(55, 65, 81)  # Lighter Surface
    TextPrimary = [System.Drawing.Color]::FromArgb(243, 244, 246) # Primary Text
    TextSecondary = [System.Drawing.Color]::FromArgb(156, 163, 175) # Secondary Text
    Border = [System.Drawing.Color]::FromArgb(75, 85, 99)        # Border Color
    Accent = [System.Drawing.Color]::FromArgb(59, 130, 246)      # Accent Color
}

# Form size and styling
[int]$formWidth = 1100
[int]$formHeight = 700

$form = New-Object System.Windows.Forms.Form
$form.Text = "HuskyNZ VCSA Deployment Tool"
$form.Size = [System.Drawing.Size]::new($formWidth, $formHeight)
$form.MinimumSize = [System.Drawing.Size]::new(900, 600)
$form.StartPosition = "CenterScreen"
$form.BackColor = $colors.Background
$form.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$form.FormBorderStyle = [System.Windows.Forms.FormBorderStyle]::Sizable

# Add a top panel for header and buttons
$topPanel = New-Object System.Windows.Forms.Panel
$topPanel.Dock = 'Top'
$topPanel.Height = 60
$topPanel.BackColor = $colors.Surface
$form.Controls.Add($topPanel)

# Header label (left)
$header = New-Object System.Windows.Forms.Label
$header.Text = "HuskyNZ VCSA Deployment Tool"
$header.Font = New-Object System.Drawing.Font("Segoe UI", 20, [System.Drawing.FontStyle]::Bold)
$header.ForeColor = $colors.TextPrimary
$header.AutoSize = $true
$header.Location = [System.Drawing.Point]::new(30, 12)
$header.TextAlign = 'MiddleLeft'
$topPanel.Controls.Add($header)

# Button panel (right)
$buttonPanel = New-Object System.Windows.Forms.Panel
$buttonPanel.Width = 360  # Increased width for 3 buttons
$buttonPanel.Height = 45
$buttonPanel.Anchor = 'Top,Right'
$buttonPanel.Location = [System.Drawing.Point]::new($form.Width - $buttonPanel.Width - 30, 5)
$buttonPanel.BackColor = [System.Drawing.Color]::Transparent
$topPanel.Controls.Add($buttonPanel)

$btnLoadEnv = New-Object System.Windows.Forms.Button
$btnLoadEnv.Text = "Load .env"
$btnLoadEnv.Size = [System.Drawing.Size]::new(110, 32)
$btnLoadEnv.Location = [System.Drawing.Point]::new(0, 6)
$btnLoadEnv.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
$btnLoadEnv.BackColor = $colors.PrimaryLight
$btnLoadEnv.ForeColor = $colors.TextPrimary
$btnLoadEnv.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
$btnLoadEnv.FlatAppearance.BorderSize = 0
$buttonPanel.Controls.Add($btnLoadEnv)

$btnSaveEnv = New-Object System.Windows.Forms.Button
$btnSaveEnv.Text = "Save .env"
$btnSaveEnv.Size = [System.Drawing.Size]::new(110, 32)
$btnSaveEnv.Location = [System.Drawing.Point]::new(120, 6)
$btnSaveEnv.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
$btnSaveEnv.BackColor = $colors.Primary
$btnSaveEnv.ForeColor = $colors.TextPrimary
$btnSaveEnv.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
$btnSaveEnv.FlatAppearance.BorderSize = 0
$buttonPanel.Controls.Add($btnSaveEnv)

$btnSettings = New-Object System.Windows.Forms.Button
$btnSettings.Text = "Settings"
$btnSettings.Size = [System.Drawing.Size]::new(110, 32)
$btnSettings.Location = [System.Drawing.Point]::new(240, 6)
$btnSettings.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
$btnSettings.BackColor = $colors.SurfaceLight
$btnSettings.ForeColor = $colors.TextPrimary
$btnSettings.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
$btnSettings.FlatAppearance.BorderSize = 0
$buttonPanel.Controls.Add($btnSettings)

# Button hover effects for Load/Save
$btnLoadEnv.Add_MouseEnter({ $btnLoadEnv.BackColor = $colors.Primary })
$btnLoadEnv.Add_MouseLeave({ $btnLoadEnv.BackColor = $colors.PrimaryLight })
$btnSaveEnv.Add_MouseEnter({ $btnSaveEnv.BackColor = $colors.PrimaryLight })
$btnSaveEnv.Add_MouseLeave({ $btnSaveEnv.BackColor = $colors.Primary })

# Main TableLayoutPanel (3 rows)
$mainTable = New-Object System.Windows.Forms.TableLayoutPanel
$mainTable.Dock = 'Fill'
$mainTable.BackColor = $colors.Background
$mainTable.ColumnCount = 1
$mainTable.RowCount = 3
$mainTable.RowStyles.Add((New-Object System.Windows.Forms.RowStyle([System.Windows.Forms.SizeType]::Percent, 60))) # config
$mainTable.RowStyles.Add((New-Object System.Windows.Forms.RowStyle([System.Windows.Forms.SizeType]::Absolute, 200))) # output
$mainTable.RowStyles.Add((New-Object System.Windows.Forms.RowStyle([System.Windows.Forms.SizeType]::Absolute, 60)))  # status
$form.Controls.Add($mainTable)
$form.Controls.SetChildIndex($mainTable, 0)

# --- Config Section (scrollable panel with TableLayoutPanel inside) ---
$configPanel = New-Object System.Windows.Forms.Panel
$configPanel.Dock = 'Fill'
$configPanel.BackColor = $colors.Surface
$configPanel.Padding = [System.Windows.Forms.Padding]::new(24, 18, 24, 18)
$configPanel.AutoScroll = $true
$configPanel.BorderStyle = 'FixedSingle'

# Add a title/description above config
$configTitle = New-Object System.Windows.Forms.Label
$configTitle.Text = "Deployment Configuration"
$configTitle.Font = New-Object System.Drawing.Font("Segoe UI", 14, [System.Drawing.FontStyle]::Bold)
$configTitle.ForeColor = $colors.TextPrimary
$configTitle.Dock = 'Top'
$configTitle.Height = 36
$configPanel.Controls.Add($configTitle)

$configTable = New-Object System.Windows.Forms.TableLayoutPanel
$configTable.ColumnCount = 2
$configTable.Dock = 'Top'
$configTable.AutoSize = $true
$configTable.AutoSizeMode = 'GrowAndShrink'
$configTable.BackColor = $colors.Surface
$configTable.ColumnStyles.Add((New-Object System.Windows.Forms.ColumnStyle([System.Windows.Forms.SizeType]::Percent, 38)))
$configTable.ColumnStyles.Add((New-Object System.Windows.Forms.ColumnStyle([System.Windows.Forms.SizeType]::Percent, 62)))

# Fields
$envVars = @(
    "VM_NAME", "VCSA_CLI_PATH", "VCSA_HOST", "VC_PASSWORD", "VCSA_ROOT_PASSWORD", "ESXI_HOST", "ESXI_USER", "ESXI_PASSWORD", "NTP_SERVERS", "DEPLOYMENT_NETWORK", "DATASTORE", "THIN_DISK_MODE", "DEPLOYMENT_OPTION", "IP_ADDRESS", "DNS_SERVERS", "NETWORK_PREFIX", "GATEWAY", "SSO_DOMAIN", "CEIP_SETTINGS"
)
$textBoxes = @{}

# Required fields (for demo, mark all except NTP_SERVERS, DNS_SERVERS, CEIP_SETTINGS as required)
$requiredFields = @(
    "VM_NAME", "VCSA_CLI_PATH", "VCSA_HOST", "VC_PASSWORD", "VCSA_ROOT_PASSWORD", "ESXI_HOST", "ESXI_USER", "ESXI_PASSWORD", "DEPLOYMENT_NETWORK", "DATASTORE", "THIN_DISK_MODE", "DEPLOYMENT_OPTION", "IP_ADDRESS", "NETWORK_PREFIX", "GATEWAY", "SSO_DOMAIN"
)

# Friendly label mapping
$friendlyLabels = @{
    VM_NAME = 'VM Name*'
    VCSA_CLI_PATH = 'VCSA CLI Path*'
    VCSA_HOST = 'VCSA Host*'
    VC_PASSWORD = 'vCenter Password*'
    VCSA_ROOT_PASSWORD = 'VCSA Root Password*'
    ESXI_HOST = 'ESXi Host*'
    ESXI_USER = 'ESXi User*'
    ESXI_PASSWORD = 'ESXi Password*'
    NTP_SERVERS = 'NTP Servers'
    DEPLOYMENT_NETWORK = 'Deployment Network*'
    DATASTORE = 'Datastore*'
    THIN_DISK_MODE = 'Thin Disk Mode*'
    DEPLOYMENT_OPTION = 'Deployment Option*'
    IP_ADDRESS = 'IP Address*'
    DNS_SERVERS = 'DNS Servers'
    NETWORK_PREFIX = 'Network Prefix*'
    GATEWAY = 'Gateway*'
    SSO_DOMAIN = 'SSO Domain*'
    CEIP_SETTINGS = 'CEIP Settings'
}

# Store ORG_NAME in a variable
$global:ORG_NAME = "HuskyNZ"

# Function to update window title and header
function Update-OrgNameTitle {
    $form.Text = "$global:ORG_NAME VCSA Deployment Tool"
    $header.Text = "$global:ORG_NAME VCSA Deployment Tool"
}
Update-OrgNameTitle

# On startup, only load ORG_NAME from .env (if present)
function Load-OrgName-Only {
    if (-not (Test-Path ".env")) { return }
    Get-Content ".env" | ForEach-Object {
        if ($_ -match "^\s*#|^\s*$") { return }
        $parts = $_ -split "=", 2
        if ($parts.Length -eq 2) {
            $key = $parts[0].Trim()
            $value = $parts[1].Trim().Trim('"')
            if ($key -eq 'ORG_NAME') {
                $global:ORG_NAME = $value
                Update-OrgNameTitle
            }
        }
    }
}
Load-OrgName-Only

# Alternating row backgrounds
$rowColor1 = $colors.Surface
$rowColor2 = $colors.SurfaceLight
$rowIndex = 0

foreach ($var in $envVars) {
    $label = New-Object System.Windows.Forms.Label
    $label.Text = if ($friendlyLabels.ContainsKey($var)) { $friendlyLabels[$var] } else { $var }
    $label.ForeColor = $colors.TextPrimary
    $label.Dock = 'Fill'
    $label.TextAlign = 'MiddleLeft'
    $label.Margin = [System.Windows.Forms.Padding]::new(6, 6, 12, 6)
    $label.AutoSize = $true
    if ($requiredFields -contains $var) {
        $label.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
    }
    $rowPanel = New-Object System.Windows.Forms.Panel
    $rowPanel.Dock = 'Top'
    $rowPanel.Height = 36
    $rowPanel.BackColor = if ($rowIndex % 2 -eq 0) { $rowColor1 } else { $rowColor2 }
    $rowPanel.Padding = [System.Windows.Forms.Padding]::new(0, 0, 0, 0)
    $rowPanel.Margin = [System.Windows.Forms.Padding]::new(0, 0, 0, 0)
    if ($var -eq 'THIN_DISK_MODE' -or $var -eq 'CEIP_SETTINGS') {
        $combo = New-Object System.Windows.Forms.ComboBox
        $combo.Items.AddRange(@('true','false'))
        $combo.DropDownStyle = 'DropDownList'
        $combo.Dock = 'Fill'
        $combo.Margin = [System.Windows.Forms.Padding]::new(2, 2, 2, 2)
        $combo.BackColor = $colors.Surface
        $combo.ForeColor = $colors.TextPrimary
        $combo.FlatStyle = 'Flat'
        $combo.Height = 28
        $textBoxes[$var] = $combo
        $configTable.Controls.Add($label)
        $configTable.Controls.Add($combo)
    } else {
        $textBox = New-Object System.Windows.Forms.TextBox
        $textBox.Dock = 'Fill'
        $textBox.Margin = [System.Windows.Forms.Padding]::new(2, 2, 2, 2)
        $textBox.BackColor = $colors.Surface
        $textBox.ForeColor = $colors.TextPrimary
        $textBox.BorderStyle = [System.Windows.Forms.BorderStyle]::FixedSingle
        $textBox.Height = 28
        if ($requiredFields -contains $var) {
            $textBox.Tag = 'required'
        }
        $textBoxes[$var] = $textBox
        $configTable.Controls.Add($label)
        $configTable.Controls.Add($textBox)
    }
    $rowIndex++
}
$configPanel.Controls.Add($configTable)
$mainTable.Controls.Add($configPanel, 0, 0)

# --- Output Section ---
$outputGroup = New-Object System.Windows.Forms.GroupBox
$outputGroup.Text = "Deployment Output"
$outputGroup.Font = New-Object System.Drawing.Font("Segoe UI", 11, [System.Drawing.FontStyle]::Bold)
$outputGroup.ForeColor = $colors.TextPrimary
$outputGroup.BackColor = $colors.Surface
$outputGroup.Dock = 'Fill'
$outputGroup.Padding = [System.Windows.Forms.Padding]::new(10)
$outputGroup.FlatStyle = 'Flat'
$outputGroup.Height = 200
$outputGroup.Margin = [System.Windows.Forms.Padding]::new(0, 10, 0, 0)

$txtOutput = New-Object System.Windows.Forms.TextBox
$txtOutput.Multiline = $true
$txtOutput.ScrollBars = "Vertical"
$txtOutput.ReadOnly = $true
$txtOutput.Dock = 'Fill'
$txtOutput.Font = New-Object System.Drawing.Font("Consolas", 11)
$txtOutput.BackColor = $colors.SurfaceLight
$txtOutput.ForeColor = $colors.TextPrimary
$txtOutput.BorderStyle = [System.Windows.Forms.BorderStyle]::FixedSingle
$outputGroup.Controls.Add($txtOutput)
$mainTable.Controls.Add($outputGroup, 0, 1)

# --- Status Section ---
$statusPanel = New-Object System.Windows.Forms.Panel
$statusPanel.Dock = 'Fill'
$statusPanel.BackColor = $colors.Surface
$statusPanel.Height = 60
$statusPanel.Padding = [System.Windows.Forms.Padding]::new(0, 10, 0, 10)
$statusPanel.BorderStyle = 'FixedSingle'

$lblStatus = New-Object System.Windows.Forms.Label
$lblStatus.Text = "Status: Ready to deploy"
$lblStatus.AutoSize = $true
$lblStatus.Location = [System.Drawing.Point]::new(10, 20)
$lblStatus.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
$lblStatus.ForeColor = $colors.Success
$statusPanel.Controls.Add($lblStatus)

$progressBar = New-Object System.Windows.Forms.ProgressBar
$progressBar.Location = [System.Drawing.Point]::new(250, 20)
$progressBar.Size = [System.Drawing.Size]::new(250, 20)
$progressBar.Style = [System.Windows.Forms.ProgressBarStyle]::Marquee
$progressBar.MarqueeAnimationSpeed = 30
$progressBar.Visible = $false
$statusPanel.Controls.Add($progressBar)

$btnDeploy = New-Object System.Windows.Forms.Button
$btnDeploy.Text = "Start Deployment"
$btnDeploy.Size = [System.Drawing.Size]::new(170, 38)
$btnDeploy.Location = [System.Drawing.Point]::new(700, 15)
$btnDeploy.Font = New-Object System.Drawing.Font("Segoe UI", 11, [System.Drawing.FontStyle]::Bold)
$btnDeploy.BackColor = $colors.Primary
$btnDeploy.ForeColor = $colors.TextPrimary
$btnDeploy.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
$btnDeploy.FlatAppearance.BorderSize = 0
$btnDeploy.Cursor = [System.Windows.Forms.Cursors]::Hand
$statusPanel.Controls.Add($btnDeploy)
$mainTable.Controls.Add($statusPanel, 0, 2)

# Button hover effects
$btnDeploy.Add_MouseEnter({
    $btnDeploy.BackColor = $colors.PrimaryLight
})
$btnDeploy.Add_MouseLeave({
    $btnDeploy.BackColor = $colors.Primary
})

# --- Usability: Required fields and Deploy button enable/disable ---
function Check-RequiredFields {
    $allFilled = $true
    foreach ($key in $requiredFields) {
        $ctrl = $textBoxes[$key]
        if ($ctrl -is [System.Windows.Forms.TextBox]) {
            $ctrl.BackColor = $colors.Surface
            $ctrl.BorderStyle = [System.Windows.Forms.BorderStyle]::FixedSingle
            if ([string]::IsNullOrWhiteSpace($ctrl.Text)) {
                $allFilled = $false
            }
        } elseif ($ctrl -is [System.Windows.Forms.ComboBox]) {
            if (-not $ctrl.SelectedItem) {
                $ctrl.BackColor = $colors.Surface
                $allFilled = $false
            } else {
                $ctrl.BackColor = $colors.Surface
            }
        }
    }
    $btnDeploy.Enabled = $allFilled
}
foreach ($key in $requiredFields) {
    $ctrl = $textBoxes[$key]
    if ($ctrl -is [System.Windows.Forms.TextBox]) {
        $ctrl.Add_TextChanged({ Check-RequiredFields })
    } elseif ($ctrl -is [System.Windows.Forms.ComboBox]) {
        $ctrl.Add_SelectedIndexChanged({ Check-RequiredFields })
    }
}
Check-RequiredFields

# --- Usability: Confirmation dialog on success ---
function Show-DeploymentSuccess {
    [System.Windows.Forms.MessageBox]::Show("Deployment completed successfully!", "Success", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
}

# Settings dialog for ORG_NAME
function Show-SettingsDialog {
    $dlg = New-Object System.Windows.Forms.Form
    $dlg.Text = "Settings"
    $dlg.Size = [System.Drawing.Size]::new(350, 150)
    $dlg.StartPosition = 'CenterParent'
    $dlg.FormBorderStyle = 'FixedDialog'
    $dlg.MaximizeBox = $false
    $dlg.MinimizeBox = $false
    $dlg.BackColor = $colors.Surface
    $dlg.Font = New-Object System.Drawing.Font("Segoe UI", 10)

    $lblOrg = New-Object System.Windows.Forms.Label
    $lblOrg.Text = "Organization Name:"
    $lblOrg.Location = [System.Drawing.Point]::new(20, 25)
    $lblOrg.Size = [System.Drawing.Size]::new(140, 24)
    $lblOrg.ForeColor = $colors.TextPrimary
    $dlg.Controls.Add($lblOrg)

    $txtOrg = New-Object System.Windows.Forms.TextBox
    $txtOrg.Location = [System.Drawing.Point]::new(170, 22)
    $txtOrg.Size = [System.Drawing.Size]::new(140, 28)
    $txtOrg.Text = $global:ORG_NAME
    $txtOrg.BackColor = $colors.SurfaceLight
    $txtOrg.ForeColor = $colors.TextPrimary
    $dlg.Controls.Add($txtOrg)

    $btnOK = New-Object System.Windows.Forms.Button
    $btnOK.Text = "OK"
    $btnOK.Size = [System.Drawing.Size]::new(80, 32)
    $btnOK.Location = [System.Drawing.Point]::new(120, 70)
    $btnOK.BackColor = $colors.Primary
    $btnOK.ForeColor = $colors.TextPrimary
    $btnOK.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $btnOK.FlatAppearance.BorderSize = 0
    $dlg.Controls.Add($btnOK)

    $btnOK.Add_Click({
        $global:ORG_NAME = $txtOrg.Text
        Update-OrgNameTitle
        # Update only ORG_NAME in .env, preserving all other values
        $envPath = ".env"
        $envTable = @{}
        if (Test-Path $envPath) {
            foreach ($line in Get-Content $envPath) {
                if ($line -match "^\s*#|^\s*$") { continue }
                $parts = $line -split '=', 2
                if ($parts.Length -eq 2) {
                    $key = $parts[0].Trim()
                    $value = $parts[1].Trim()
                    $envTable[$key] = $value
                }
            }
        }
        $envTable['ORG_NAME'] = '"' + $global:ORG_NAME + '"'
        $lines = @()
        foreach ($key in $envTable.Keys) {
            $lines += "$key=$($envTable[$key])"
        }
        Set-Content -Path $envPath -Value $lines
        $dlg.Close()
    })
    $dlg.ShowDialog()
}
$btnSettings.Add_Click({ Show-SettingsDialog })

# Update Load-EnvFile and Save-EnvFile to handle ORG_NAME
function Load-EnvFile {
    if (-not (Test-Path ".env")) { return }
    Get-Content ".env" | ForEach-Object {
        if ($_ -match "^\s*#|^\s*$") { return }
        $parts = $_ -split "=", 2
        if ($parts.Length -eq 2) {
            $key = $parts[0].Trim()
            $value = $parts[1].Trim().Trim('"')
            if ($key -eq 'ORG_NAME') {
                $global:ORG_NAME = $value
                Update-OrgNameTitle
            } elseif ($textBoxes.ContainsKey($key)) {
                if ($key -eq 'THIN_DISK_MODE' -or $key -eq 'CEIP_SETTINGS') {
                    $textBoxes[$key].SelectedItem = $value
                } else {
                    $textBoxes[$key].Text = $value
                }
            }
        }
    }
}

function Save-EnvFile {
    $lines = @()
    $lines += "ORG_NAME=`"$global:ORG_NAME`""
    foreach ($key in $envVars) {
        if ($key -eq 'THIN_DISK_MODE' -or $key -eq 'CEIP_SETTINGS') {
            $value = $textBoxes[$key].SelectedItem
        } else {
            $value = $textBoxes[$key].Text.Replace('"', '')
        }
        $lines += "$key=`"$value`""
    }
    Set-Content -Path ".env" -Value $lines
}

# Update status
function Update-Status {
    param(
        [string]$Message,
        [string]$Type = "Info"
    )
    
    $lblStatus.Text = "Status: $Message"
    
    switch ($Type) {
        "Success" { $lblStatus.ForeColor = $colors.Success }
        "Warning" { $lblStatus.ForeColor = $colors.Warning }
        "Error" { $lblStatus.ForeColor = $colors.Danger }
        "Running" { $lblStatus.ForeColor = $colors.Primary }
        default { $lblStatus.ForeColor = $colors.TextSecondary }
    }
}

# Deploy button click handler
$btnDeploy.Add_Click({
    $btnDeploy.Enabled = $false
    $btnDeploy.Text = "Deploying..."
    $btnDeploy.BackColor = $colors.Warning
    $progressBar.Visible = $true
    Update-Status "Running deployment..." "Running"
    $txtOutput.Clear()
    Save-EnvFile

    $scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $PSCommandPath }
    $script = Join-Path -Path $scriptDir -ChildPath "setup.ps1"
    if (-not (Test-Path $script)) {
        [System.Windows.Forms.MessageBox]::Show('Missing setup.ps1', "Error", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        $btnDeploy.Enabled = $true
        $btnDeploy.Text = "Start Deployment"
        $btnDeploy.BackColor = $colors.Primary
        $progressBar.Visible = $false
        Update-Status "Error: Missing setup.ps1" "Error"
        return
    }
    $setupScriptPath = Join-Path $ScriptRoot "setup.ps1"
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "powershell.exe"
    $psi.Arguments = "-NoProfile -ExecutionPolicy Bypass -Command & '$script'"
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
        if ($line) { $txtOutput.AppendText($line + "`r`n"); $txtOutput.ScrollToCaret() }
        [System.Windows.Forms.Application]::DoEvents()
    }
    while (-not $stdout.EndOfStream) {
        $txtOutput.AppendText($stdout.ReadLine() + "`r`n"); $txtOutput.ScrollToCaret()
        [System.Windows.Forms.Application]::DoEvents()
    }
    while (-not $stderr.EndOfStream) {
        $txtOutput.AppendText("[ERROR] " + $stderr.ReadLine() + "`r`n"); $txtOutput.ScrollToCaret()
        [System.Windows.Forms.Application]::DoEvents()
    }
    $progressBar.Visible = $false
    if ($proc.ExitCode -eq 0) {
        Update-Status "Deployment completed successfully!" "Success"
        $btnDeploy.BackColor = $colors.Success
        Show-DeploymentSuccess
    } else {
        Update-Status "Deployment failed. Exit code $($proc.ExitCode)" "Error"
        $btnDeploy.BackColor = $colors.Danger
    }
    $btnDeploy.Enabled = $true
    $btnDeploy.Text = "Start Deployment"
})

# Button actions
$btnLoadEnv.Add_Click({ Load-EnvFile })
$btnSaveEnv.Add_Click({ Save-EnvFile })

# Add tooltip to Settings button
$settingsTooltip = New-Object System.Windows.Forms.ToolTip
$settingsTooltip.SetToolTip($btnSettings, 'Settings')

$form.ShowDialog()
