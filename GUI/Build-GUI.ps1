# ===============================
# Build-GUI.ps1
# ===============================
# Script to build and optionally run the vCenter Deployment GUI

param(
    [switch]$Run,
    [switch]$Publish,
    [switch]$Help
)

$ErrorActionPreference = "Stop"

function Show-Help {
    Write-Host @"
vCenter Deployment GUI Build Script

Usage: .\Build-GUI.ps1 [-Run] [-Publish] [-Help]

Parameters:
  -Run      Build and run the application
  -Publish  Create a self-contained executable
  -Help     Show this help message

Examples:
  .\Build-GUI.ps1                # Build only
  .\Build-GUI.ps1 -Run           # Build and run
  .\Build-GUI.ps1 -Publish       # Create standalone executable

Requirements:
  - Windows operating system
  - .NET 8.0 SDK or Runtime
  - PowerShell 5.1 or PowerShell Core 7+

"@ -ForegroundColor Cyan
}

function Test-DotNetInstalled {
    try {
        $dotnetVersion = & dotnet --version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[✓] .NET SDK found: $dotnetVersion" -ForegroundColor Green
            return $true
        }
    } catch {
        # dotnet command not found
    }
    
    Write-Host "[✗] .NET SDK not found" -ForegroundColor Red
    Write-Host "Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    return $false
}

function Test-WindowsPlatform {
    if ($PSVersionTable.Platform -eq "Win32NT" -or [System.Environment]::OSVersion.Platform -eq "Win32NT" -or $IsWindows) {
        Write-Host "[✓] Windows platform detected" -ForegroundColor Green
        return $true
    }
    
    Write-Host "[✗] This GUI application requires Windows" -ForegroundColor Red
    return $false
}

function Build-Application {
    Write-Host "Building vCenter Deployment GUI..." -ForegroundColor Cyan
    
    $projectPath = Join-Path $PSScriptRoot "VCenterDeployGUI"
    
    if (-not (Test-Path $projectPath)) {
        throw "Project directory not found: $projectPath"
    }
    
    Push-Location $projectPath
    try {
        Write-Host "Restoring dependencies..." -ForegroundColor Yellow
        & dotnet restore | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Failed to restore dependencies" }
        
        Write-Host "Building application..." -ForegroundColor Yellow
        & dotnet build -c Release | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Build failed" }
        
        Write-Host "[✓] Build completed successfully" -ForegroundColor Green
        
        $outputPath = Join-Path $projectPath "bin\Release\net8.0-windows"
        return $outputPath
    } finally {
        Pop-Location
    }
}

function Publish-Application {
    Write-Host "Publishing self-contained application..." -ForegroundColor Cyan
    
    $projectPath = Join-Path $PSScriptRoot "VCenterDeployGUI"
    
    Push-Location $projectPath
    try {
        Write-Host "Creating self-contained executable..." -ForegroundColor Yellow
        & dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "Publish failed" }
        
        $publishPath = Join-Path $projectPath "bin\Release\net8.0-windows\win-x64\publish"
        $exePath = Join-Path $publishPath "VCenterDeployGUI.exe"
        
        if (Test-Path $exePath) {
            Write-Host "[✓] Self-contained executable created: $exePath" -ForegroundColor Green
            Write-Host "File size: $([math]::Round((Get-Item $exePath).Length / 1MB, 2)) MB" -ForegroundColor Gray
            return $exePath
        } else {
            throw "Executable not found after publish"
        }
    } finally {
        Pop-Location
    }
}

function Start-Application {
    param([string]$OutputPath)
    
    $exePath = Join-Path $OutputPath "VCenterDeployGUI.exe"
    
    if (Test-Path $exePath) {
        Write-Host "Starting application..." -ForegroundColor Cyan
        & $exePath
    } else {
        Write-Host "Executable not found: $exePath" -ForegroundColor Red
        Write-Host "You can run the application with: dotnet run" -ForegroundColor Yellow
    }
}

# Main script execution
try {
    if ($Help) {
        Show-Help
        exit 0
    }
    
    Write-Host "vCenter Deployment GUI Build Script" -ForegroundColor Magenta
    Write-Host "====================================" -ForegroundColor Magenta
    Write-Host ""
    
    # Pre-flight checks
    if (-not (Test-WindowsPlatform)) {
        exit 1
    }
    
    if (-not (Test-DotNetInstalled)) {
        exit 1
    }
    
    # Build application
    $outputPath = Build-Application
    
    # Publish if requested
    if ($Publish) {
        $publishedExe = Publish-Application
        Write-Host ""
        Write-Host "Self-contained executable available at:" -ForegroundColor Green
        Write-Host $publishedExe -ForegroundColor White
        
        if ($Run) {
            Write-Host ""
            Start-Process -FilePath $publishedExe
        }
    }
    # Run if requested (and not publishing)
    elseif ($Run) {
        Write-Host ""
        Start-Application -OutputPath $outputPath
    }
    
    Write-Host ""
    Write-Host "Build completed successfully!" -ForegroundColor Green
    
    if (-not $Run -and -not $Publish) {
        Write-Host ""
        Write-Host "To run the application:" -ForegroundColor Yellow
        Write-Host "  .\Build-GUI.ps1 -Run" -ForegroundColor White
        Write-Host ""
        Write-Host "To create a standalone executable:" -ForegroundColor Yellow
        Write-Host "  .\Build-GUI.ps1 -Publish" -ForegroundColor White
    }
    
} catch {
    Write-Host ""
    Write-Host "[✗] Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Build failed. Please check the output above for errors." -ForegroundColor Yellow
    exit 1
}