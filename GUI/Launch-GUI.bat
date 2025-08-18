@echo off
REM ===============================
REM Launch vCenter Deployment GUI
REM ===============================

echo.
echo vCenter Deployment GUI Launcher
echo ===============================
echo.

REM Check if we're in the right directory
if not exist "Build-GUI.ps1" (
    echo Error: Build-GUI.ps1 not found in current directory.
    echo Please run this script from the GUI directory.
    echo.
    pause
    exit /b 1
)

REM Check for PowerShell
where powershell >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: PowerShell not found in PATH.
    echo Please ensure PowerShell is installed and accessible.
    echo.
    pause
    exit /b 1
)

REM Try to build and run the GUI
echo Building and launching vCenter Deployment GUI...
echo.

powershell -ExecutionPolicy Bypass -File "Build-GUI.ps1" -Run

if %errorlevel% neq 0 (
    echo.
    echo Build failed. Please check the output above for errors.
    echo.
    echo Common solutions:
    echo - Install .NET 8.0 SDK from https://dotnet.microsoft.com/download
    echo - Ensure you're running on Windows
    echo - Check PowerShell execution policy
    echo.
    pause
    exit /b 1
)

echo.
echo GUI launched successfully!
pause