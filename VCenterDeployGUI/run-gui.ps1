# vCenter Deployment GUI - PowerShell Launcher
# Run this script to build and launch the GUI on Windows

Write-Host "vCenter Deployment GUI - PowerShell Launcher" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET 8 is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "Found .NET version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "ERROR: .NET 8 SDK not found. Please install .NET 8 SDK first." -ForegroundColor Red
    Write-Host "Download from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Build the Windows project
Write-Host ""
Write-Host "Building the Windows Forms application..." -ForegroundColor Yellow

try {
    dotnet build VCenterDeployGUI-Windows.csproj
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    Write-Host "Build completed successfully!" -ForegroundColor Green
} catch {
    Write-Host "BUILD FAILED! Please check the error messages above." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Run the application
Write-Host ""
Write-Host "Starting vCenter Deployment GUI..." -ForegroundColor Yellow
Write-Host ""

try {
    dotnet run --project VCenterDeployGUI-Windows.csproj
    Write-Host ""
    Write-Host "Application closed normally." -ForegroundColor Green
} catch {
    Write-Host ""
    Write-Host "Application exited with an error." -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
}

Write-Host ""
Read-Host "Press Enter to exit"