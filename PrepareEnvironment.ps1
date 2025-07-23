# ===============================
# PrepareEnvironment.ps1
# ===============================
# Interactively sets up .env from env.example with user-friendly prompts, descriptions, masking, summary, section grouping, progress, and validation.

Import-Module .\modules\metadata\metadata.psm1

$meta = Get-Metadata
$envExample = if ($meta.envExample) { $meta.envExample } else { "env.example" }
$envFile = if ($meta.envFile) { $meta.envFile } else { ".env" }

function Show-Intro {
    Write-Host "[INFO] This script will help you create a .env file for your vCenter deployment." -ForegroundColor Cyan
    Write-Host "You will be prompted for each configuration variable. Press Enter to accept the default value in [brackets]." -ForegroundColor Cyan
    Write-Host "Descriptions are shown when available. Passwords are masked. Required fields must be filled. Type :q to abort at any prompt." -ForegroundColor Cyan
    Write-Host "---"
}

function Parse-EnvExample {
    $lines = Get-Content $envExample
    $sections = @()
    $currentSection = @{ Name = "General"; Vars = @() }
    $descBuffer = @()
    foreach ($line in $lines) {
        if ($line -match '^\s*#(.*)') {
            $comment = $Matches[1].Trim()
            if ($comment -eq "") { continue }
            # Section header if all caps and not a sentence
            if ($comment -cmatch '^[A-Z0-9 _&()]+$' -and $descBuffer.Count -eq 0) {
                if ($currentSection.Vars.Count -gt 0) { $sections += $currentSection }
                $currentSection = @{ Name = $comment; Vars = @() }
            } else {
                $descBuffer += $comment
            }
            continue
        }
        if ($line -match '^([A-Z0-9_]+)=(.*)$') {
            $key = $Matches[1].Trim()
            $default = $Matches[2].Trim()
            $desc = ($descBuffer -join ' ') -replace '\s+', ' '
            $descBuffer = @()
            $currentSection.Vars += [PSCustomObject]@{
                Key = $key
                Default = $default
                Description = $desc
            }
        }
    }
    if ($currentSection.Vars.Count -gt 0) { $sections += $currentSection }
    return $sections
}

function Is-Boolean {
    param($val)
    return $val -in @('true','false')
}

function Is-IPAddress {
    param($val)
    return [System.Net.IPAddress]::TryParse($val, [ref]([System.Net.IPAddress]::None))
}

function Prompt-For-Value {
    param(
        [string]$Key,
        [string]$Default,
        [string]$Description,
        [bool]$IsPassword,
        [bool]$IsRequired,
        [string]$Type
    )
    while ($true) {
        $reqStr = if ($IsRequired) { '[Required]' } else { '[Optional]' }
        $defStr = if ($Default) { "[default: $Default]" } else { '' }
        Write-Host "Variable: $Key $reqStr" -ForegroundColor Yellow
        if ($Description) { Write-Host "Description: $Description" -ForegroundColor Gray }
        if ($defStr) { Write-Host $defStr -ForegroundColor DarkGray }
        $prompt = "Enter value for $Key"
        if ($Default) { $prompt += " [$Default]" }
        if ($IsPassword) {
            $input = Read-Host "$prompt" -AsSecureString
            if ($input.Length -eq 0 -and $Default) { return $Default }
            if ($input.Length -eq 0 -and $IsRequired) { Write-Host "[ERROR] This field is required." -ForegroundColor Red; continue }
            if ($input.Length -eq 0) { return "" }
            $plain = [System.Net.NetworkCredential]::new("", $input).Password
            if ($plain -eq ':q') { throw 'Aborted by user.' }
            return $plain
        } else {
            $input = Read-Host $prompt
            if ($input -eq ':q') { throw 'Aborted by user.' }
            if (-not $input -and $Default) { $input = $Default }
            if (-not $input -and $IsRequired) { Write-Host "[ERROR] This field is required." -ForegroundColor Red; continue }
            if ($Type -eq 'bool' -and $input -and -not (Is-Boolean $input)) {
                Write-Host "[ERROR] Please enter 'true' or 'false'." -ForegroundColor Red; continue
            }
            if ($Type -eq 'ip' -and $input -and -not (Is-IPAddress $input)) {
                Write-Host "[ERROR] Please enter a valid IP address." -ForegroundColor Red; continue
            }
            return $input
        }
    }
}

function Show-Summary {
    param($envOut)
    Write-Host "Summary of your .env file:" -ForegroundColor Cyan
    Write-Host ("-" * 40)
    foreach ($item in $envOut) {
        $val = if ($item.IsPassword) { '********' } else { $item.Value }
        Write-Host ("{0,-22} = {1}" -f $item.Key, $val)
    }
    Write-Host ("-" * 40)
}

# Main script
try {
    if (-Not (Test-Path $envExample)) {
        Write-Host "[ERROR] $envExample not found in the current directory." -ForegroundColor Red
        exit 1
    }

    if (Test-Path $envFile) {
        Copy-Item $envFile "$envFile.bak" -Force
        $overwrite = Read-Host ".env already exists. Overwrite? (y/N)"
        if ($overwrite -notin @('y','Y')) {
            Write-Host "[INFO] Skipping .env setup." -ForegroundColor Yellow
            exit 0
        }
    }

    Show-Intro
    $sections = Parse-EnvExample
    $envOut = @()
    $totalVars = ($sections | ForEach-Object { $_.Vars.Count } | Measure-Object -Sum).Sum
    $step = 1
    foreach ($section in $sections) {
        Write-Host "\n=== $($section.Name) ===" -ForegroundColor Cyan
        foreach ($var in $section.Vars) {
            $isPassword = $var.Key -match 'PASSWORD'
            $isRequired = -not $var.Default
            $type = if ($var.Key -match 'THIN_DISK_MODE|CEIP_SETTINGS') { 'bool' } elseif ($var.Key -match 'IP_ADDRESS|GATEWAY|DNS_SERVERS') { 'ip' } else { '' }
            Write-Host "[Step $step of $totalVars]" -ForegroundColor Magenta
            $value = Prompt-For-Value -Key $var.Key -Default $var.Default -Description $var.Description -IsPassword $isPassword -IsRequired $isRequired -Type $type
            $envOut += [PSCustomObject]@{
                Key = $var.Key
                Value = $value
                IsPassword = $isPassword
            }
            $step++
        }
    }
    Show-Summary $envOut
    $confirm = Read-Host "Write these values to .env? (Y/n)"
    if ($confirm -notin @('y','Y','')) {
        Write-Host "[INFO] Aborted. No changes made." -ForegroundColor Yellow
        exit 0
    }
    $envOut | ForEach-Object { "{0}={1}" -f $_.Key, $_.Value } | Set-Content $envFile
    Write-Host "[SUCCESS] .env file created interactively." -ForegroundColor Green
} catch {
    Write-Host "[ERROR] $_" -ForegroundColor Red
    exit 1
} 