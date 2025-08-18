@echo off
echo Building vCenter Deployment GUI for Windows...
echo.

if not exist "%ProgramFiles%\dotnet\dotnet.exe" (
    echo ERROR: .NET 8 SDK not found. Please install .NET 8 SDK first.
    echo Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo Building the application...
dotnet build VCenterDeployGUI-Windows.csproj

if %ERRORLEVEL% neq 0 (
    echo.
    echo BUILD FAILED! Please check the error messages above.
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo.
echo Starting vCenter Deployment GUI...
echo.

dotnet run --project VCenterDeployGUI-Windows.csproj

if %ERRORLEVEL% neq 0 (
    echo.
    echo Application exited with an error.
    pause
    exit /b 1
)

echo.
echo Application closed normally.
pause