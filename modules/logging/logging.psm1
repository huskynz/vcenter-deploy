function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "Info"  # Info, Success, Warning, Error
    )
    $isRedirected = [Console]::IsOutputRedirected
    switch ($Level) {
        "Success" { $color = "Green" }
        "Warning" { $color = "Yellow" }
        "Error"   { $color = "Red" }
        default   { $color = "Cyan" }
    }
    if (-not $isRedirected) {
        Write-Host $Message -ForegroundColor $color
    } else {
        if ($Level -eq "Error") {
            Write-Error $Message
        } else {
            Write-Output $Message
            [Console]::Out.Flush()
        }
    }
}

function Show-Banner {
    param(
        [string]$ScriptVersion,
        [string]$ScriptLastUpdatedOn,
        [string]$VCSAName,
        [string]$ESXiHost,
        [string]$DeploymentOption,
        [string]$IPAddress,
        [string]$ScriptCreatedBy = "HuskyNZ"
    )

    if (-not (Get-Command Get-Metadata -ErrorAction SilentlyContinue)) {
        try {
            Import-Module "..\metadata\metadata.psm1" -Force
        } catch {
            Write-Log "[ERROR] Could not import metadata.psm1. Banner will be incomplete." "Error"
            return
        }
    }
    if (-not (Get-Command Get-Metadata -ErrorAction SilentlyContinue)) {
        Write-Log "[ERROR] Get-Metadata function is not available. Banner will be incomplete." "Error"
        return
    }
    $currentTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $width = 60
    $title = "$ScriptCreatedBy Vcenter Deployment Script"
    $paddingLeft = [Math]::Floor(($width - 2 - $title.Length) / 2)
    $paddingRight = $width - 2 - $title.Length - $paddingLeft
    $centeredTitle = (' ' * $paddingLeft) + $title + (' ' * $paddingRight)
    $topLine    = "_" * $width
    $bottomLine = "-" * $width
    $sideChar   = "|"

    function Write-Line($text) {
        $padding = $width - 2 - $text.Length
        if ($padding -lt 0) { $padding = 0 }
        Write-Host "$sideChar$text" -NoNewline
        Write-Host (" " * $padding) -NoNewline
        Write-Host "$sideChar"
    }

    Write-Log $topLine "Info"
    Write-Line ($centeredTitle)
    Write-Line ""
    Write-Log (" Script Version      : $ScriptVersion") "Info"
    Write-Log (" Script Last Updated : $ScriptLastUpdatedOn") "Info"
    Write-Log (" VCSA Hostname       : $VCSAName") "Info"
    Write-Log (" ESXi Host           : $ESXiHost") "Info"
    Write-Log (" Deployment Option   : $DeploymentOption") "Info"
    Write-Log (" Network IP          : $IPAddress") "Info"
    Write-Log (" Deployment Time     : $currentTime") "Info"
    Write-Line ""
    Write-Log $bottomLine "Info"
    Write-Log "" "Info"
}

Export-ModuleMember -Function Write-Log, Show-Banner
