# vCenter Deployment GUI

A professional Windows Forms GUI wrapper for the vCenter deployment automation scripts.

## Features

- **Clean, professional Windows Forms interface** with tabbed layout
- **Real-time field validation** for IP addresses, paths, and required fields
- **Configuration management** - load/save .env files and import from templates
- **PowerShell script integration** - execute deployment scripts with progress tracking
- **Color-coded status messages** and comprehensive logging
- **Professional styling** consistent with Windows standards

## Quick Start (Windows)

### Prerequisites

- Windows with .NET 8 Runtime
- PowerShell 5.1 or later
- VMware vCenter ISO mounted
- Network access to ESXi host

### Building and Running

1. **On Windows machines only:**
   ```cmd
   cd VCenterDeployGUI
   dotnet build VCenterDeployGUI-Windows.csproj
   dotnet run --project VCenterDeployGUI-Windows.csproj
   ```

2. **Or use the precompiled executable (if available)**

### Usage

1. **Launch the application** - the GUI will open with a tabbed interface
2. **Configure deployment settings** using the tabs:
   - **General**: VM name, VCSA CLI path, VCSA host, CEIP settings
   - **Credentials**: vCenter and root passwords (masked input)
   - **ESXi**: ESXi host connection details
   - **Networking**: IP configuration, DNS, NTP, network settings
   - **Storage**: Datastore and disk provisioning options
   - **Deployment**: Size options and deployment controls
   - **Logs**: Real-time deployment progress and output

3. **Load existing configuration**:
   - Use File → Load Configuration to import .env files
   - Use File → Load from Template to import env.example

4. **Validate configuration**:
   - Use Deploy → Validate Configuration or click "Validate Configuration"
   - Real-time validation provides immediate feedback

5. **Start deployment**:
   - Click "Start Deployment" on the Deployment tab
   - Monitor progress in the Logs tab
   - Use "Stop Deployment" to cancel if needed

6. **Save configuration**:
   - Use File → Save Configuration to export .env files

## Technical Details

### Architecture

- **Models**: `VCenterConfiguration` - strongly-typed configuration model
- **Services**: 
  - `ConfigurationService` - handles .env file I/O
  - `ValidationService` - provides field validation logic
  - `PowerShellService` - executes PowerShell scripts with progress tracking
- **UI**: Tabbed Windows Forms interface with real-time validation

### Integration

The GUI integrates seamlessly with existing PowerShell scripts:
- Reads and writes standard .env file format
- Executes `setup.ps1` for deployment
- Compatible with `PrepareEnvironment.ps1` workflow
- Maintains all existing script functionality

### Field Validation

- **IP Addresses**: Validates IPv4 format for IP addresses and gateways
- **Hostnames**: Validates FQDN format for ESXi and vCenter hosts
- **Paths**: Validates file path format for VCSA CLI path
- **Network Prefix**: Validates CIDR notation (0-32)
- **DNS/NTP Servers**: Validates comma-separated server lists
- **Required Fields**: Highlights missing required fields

### Progress Tracking

- Real-time PowerShell output capture
- Color-coded log messages (Info=Cyan, Success=Green, Error=Red, Warning=Yellow)
- Progress bar during deployment
- Process cancellation support
- Exit code handling

## File Structure

```
VCenterDeployGUI/
├── Models/
│   └── VCenterConfiguration.cs      # Configuration data model
├── Services/
│   ├── ConfigurationService.cs      # .env file operations
│   ├── ValidationService.cs         # Field validation logic
│   └── PowerShellService.cs         # Script execution
├── MainForm.cs                      # Main UI form (partial)
├── MainForm.Methods.cs              # UI helper methods (partial)
├── MainForm.Events.cs               # Event handlers (partial)
├── WindowsProgram.cs                # Windows entry point
├── VCenterDeployGUI-Windows.csproj  # Windows project file
└── app.manifest                     # Windows application manifest
```

## Configuration Options

All options from the original .env format are supported:

| Category | Fields | Validation |
|----------|--------|------------|
| **General** | VM_NAME, VCSA_CLI_PATH, VCSA_HOST, CEIP_SETTINGS | Required fields, path validation, hostname validation |
| **Credentials** | VC_PASSWORD, VCSA_ROOT_PASSWORD | Password masking, required fields |
| **ESXi** | ESXI_HOST, ESXI_USER, ESXI_PASSWORD | Hostname validation, required fields |
| **Networking** | IP_ADDRESS, DNS_SERVERS, NETWORK_PREFIX, GATEWAY, NTP_SERVERS, DEPLOYMENT_NETWORK | IP validation, CIDR validation |
| **Storage** | DATASTORE, THIN_DISK_MODE | Required datastore |
| **Deployment** | DEPLOYMENT_OPTION | Dropdown with valid options |
| **SSO** | SSO_DOMAIN | Default: vsphere.local |

## Error Handling

- Comprehensive input validation with visual feedback
- PowerShell execution error capture and display
- File I/O error handling with user-friendly messages
- Deployment cancellation support
- Graceful handling of missing files or invalid paths

## Professional Features

- **Visual Styling**: Professional Windows appearance with proper DPI awareness
- **Tooltips**: Contextual help for all configuration fields
- **Status Bar**: Current operation status display
- **Confirmation Dialogs**: Important action confirmations
- **Log Management**: Save logs to file, clear logs functionality
- **Keyboard Navigation**: Full keyboard accessibility
- **Window Management**: Proper minimize/maximize/close handling

## Troubleshooting

- **Build Issues**: Ensure you're building on Windows with .NET 8 SDK
- **PowerShell Errors**: Check that scripts are in the parent directory
- **Permission Issues**: Run as administrator if needed for PowerShell execution
- **Path Issues**: Use the browse button to select vcsa-deploy.exe correctly

## Development Notes

This GUI was designed to complement, not replace, the existing command-line workflow. It provides the same functionality in a user-friendly interface while maintaining full compatibility with the original automation scripts.