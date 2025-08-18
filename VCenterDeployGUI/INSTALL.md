# vCenter Deployment GUI - Installation Guide

## Overview

This guide will help you install and run the vCenter Deployment GUI on Windows. The GUI provides a user-friendly interface for the vCenter deployment automation scripts.

## Prerequisites

### Required Software
- **Windows 10/11** or **Windows Server 2019/2022**
- **.NET 8 Runtime or SDK** (download from https://dotnet.microsoft.com/download/dotnet/8.0)
- **PowerShell 5.1 or later** (included with Windows)

### Required Resources
- **VMware vCenter ISO** mounted or extracted
- **Network access** to your ESXi host
- **ESXi administrator credentials**
- **Static IP configuration** details for vCenter

## Quick Installation

### Option 1: Using the Batch File (Easiest)

1. **Copy the entire `VCenterDeployGUI` folder** to your Windows machine
2. **Double-click `run-gui.bat`** in the VCenterDeployGUI folder
3. The script will:
   - Check for .NET 8 installation
   - Build the application automatically
   - Launch the GUI

### Option 2: Using PowerShell

1. **Copy the entire `VCenterDeployGUI` folder** to your Windows machine
2. **Right-click PowerShell** and "Run as Administrator"
3. **Navigate** to the VCenterDeployGUI folder:
   ```powershell
   cd "C:\path\to\VCenterDeployGUI"
   ```
4. **Run the launcher script**:
   ```powershell
   .\run-gui.ps1
   ```

### Option 3: Manual Build and Run

1. **Copy the entire `VCenterDeployGUI` folder** to your Windows machine
2. **Open Command Prompt or PowerShell**
3. **Navigate** to the VCenterDeployGUI folder
4. **Build the application**:
   ```cmd
   dotnet build VCenterDeployGUI-Windows.csproj
   ```
5. **Run the application**:
   ```cmd
   dotnet run --project VCenterDeployGUI-Windows.csproj
   ```

## Detailed Setup Instructions

### Step 1: Install .NET 8

If you don't have .NET 8 installed:

1. **Download** from https://dotnet.microsoft.com/download/dotnet/8.0
2. **Choose** ".NET Desktop Runtime" for end users or ".NET SDK" for developers
3. **Run the installer** and follow the prompts
4. **Verify installation** by opening Command Prompt and running:
   ```cmd
   dotnet --version
   ```

### Step 2: Prepare Your Environment

1. **Mount or extract** your VMware vCenter ISO
2. **Locate** the `vcsa-deploy.exe` file (typically in `\vcsa-cli-installer\win32\`)
3. **Note the full path** - you'll need this in the GUI
4. **Gather** your ESXi host details:
   - IP address or hostname
   - Administrator username (usually `root`)
   - Administrator password
   - Datastore name

### Step 3: Copy and Run the GUI

1. **Copy the VCenterDeployGUI folder** to your Windows machine
2. **Navigate** to the folder in File Explorer
3. **Choose your preferred method**:
   - Double-click `run-gui.bat` for automatic setup
   - Right-click `run-gui.ps1` → "Run with PowerShell"
   - Use manual command-line build (see Option 3 above)

## Using the GUI

### First Time Setup

1. **Launch the application** using one of the methods above
2. **Go to File → Load from Template** to import default settings from `env.example`
3. **Configure each tab** with your specific details:
   - **General**: VM name, vCenter hostname, VCSA CLI path
   - **Credentials**: Set your vCenter passwords
   - **ESXi**: Configure ESXi host connection
   - **Networking**: Set IP, DNS, gateway configuration
   - **Storage**: Select datastore and disk options
   - **Deployment**: Choose vCenter size

### Validation and Deployment

1. **Click "Validate Configuration"** to check all settings
2. **Fix any validation errors** highlighted in red
3. **Click "Start Deployment"** when ready
4. **Monitor progress** in the Logs tab
5. **Wait for completion** - this can take 30-60 minutes

### Configuration Management

- **Save configurations**: File → Save Configuration
- **Load existing configs**: File → Load Configuration
- **Start fresh**: File → New Configuration

## Troubleshooting

### Build Errors

**Error**: "MSBuild error" or "SDK not found"
- **Solution**: Install .NET 8 SDK (not just runtime)
- **Download**: https://dotnet.microsoft.com/download/dotnet/8.0

**Error**: "Project file not found"
- **Solution**: Ensure you're in the VCenterDeployGUI folder
- **Check**: The folder should contain `VCenterDeployGUI-Windows.csproj`

### Runtime Errors

**Error**: "PowerShell execution policy"
- **Solution**: Run PowerShell as administrator and execute:
  ```powershell
  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
  ```

**Error**: "setup.ps1 not found"
- **Solution**: Ensure the VCenterDeployGUI folder is in the same directory as `setup.ps1` and `PrepareEnvironment.ps1`

**Error**: "Access denied" or permission errors
- **Solution**: Run as administrator or check file permissions

### Application Errors

**Error**: "vcsa-deploy.exe not found"
- **Solution**: 
  - Mount the vCenter ISO
  - Use the browse button to locate `vcsa-deploy.exe`
  - Ensure the full path includes the filename

**Error**: "Cannot connect to ESXi host"
- **Solution**:
  - Verify ESXi host IP/hostname
  - Check network connectivity (ping the host)
  - Verify ESXi credentials
  - Ensure ESXi host is accessible from your network

## Directory Structure

After copying to Windows, your folder should look like this:

```
VCenterDeployGUI/
├── Models/
│   └── VCenterConfiguration.cs
├── Services/
│   ├── ConfigurationService.cs
│   ├── ValidationService.cs
│   └── PowerShellService.cs
├── MainForm.cs
├── MainForm.Methods.cs
├── MainForm.Events.cs
├── WindowsProgram.cs
├── VCenterDeployGUI-Windows.csproj
├── app.manifest
├── run-gui.bat
├── run-gui.ps1
└── README.md
```

And in the parent directory:
```
├── setup.ps1
├── PrepareEnvironment.ps1
├── env.example
└── VCenterDeployGUI/
```

## Getting Help

If you encounter issues:

1. **Check the Logs tab** in the GUI for detailed error messages
2. **Verify all prerequisites** are installed correctly
3. **Ensure network connectivity** to your ESXi host
4. **Check file paths** especially for vcsa-deploy.exe
5. **Try the command-line scripts first** to isolate GUI vs. script issues

## Advanced Options

### Building a Standalone Executable

To create a standalone .exe file:

```cmd
dotnet publish VCenterDeployGUI-Windows.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The executable will be in `bin\Release\net8.0-windows\win-x64\publish\`

### Integration with Existing Scripts

The GUI is designed to work alongside the existing PowerShell scripts:
- It reads and writes the same `.env` format
- It calls the same `setup.ps1` script for deployment
- All existing functionality is preserved
- You can switch between GUI and command-line at any time