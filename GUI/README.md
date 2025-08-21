# vCenter Deployment GUI

This Windows Forms application provides a user-friendly graphical interface for the vCenter deployment automation tools.

## Prerequisites

- **Windows Operating System** (Windows 10/11 or Windows Server 2016+)
- **.NET 8.0 Runtime** (Windows Desktop Runtime)
- **PowerShell 5.1 or PowerShell Core 7+**
- **VMware vCenter ISO** mounted or extracted
- **Network access** to target ESXi host

## Installation

### Option 1: Build from Source

1. **Install .NET 8.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Choose "Desktop Runtime" for Windows

2. **Clone and Build**
   ```cmd
   git clone https://github.com/huskynz/vcenter-deploy.git
   cd vcenter-deploy\GUI\VCenterDeployGUI
   dotnet restore
   dotnet build -c Release
   ```

3. **Run the Application**
   ```cmd
   dotnet run
   ```
   or navigate to `bin\Release\net8.0-windows\` and run `VCenterDeployGUI.exe`

### Option 2: Publish Self-Contained Executable

To create a standalone executable that doesn't require .NET to be installed:

```cmd
cd GUI\VCenterDeployGUI
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be created in `bin\Release\net8.0-windows\win-x64\publish\VCenterDeployGUI.exe`

## Features

### 1. Configuration Management
- **Tabbed Interface**: Organized into logical groups (General, Credentials, Network, Deployment, Advanced)
- **Load/Save .env Files**: Import existing configurations or save current settings
- **Template Loading**: Load default values from `env.example`
- **Real-time Validation**: Immediate feedback for invalid inputs
- **Field Descriptions**: Tooltips and help text for each configuration option

### 2. Deployment Integration
- **Environment Validation**: Check prerequisites before deployment
- **PowerShell Integration**: Execute `setup.ps1` and `PrepareEnvironment.ps1` scripts
- **Real-time Output**: Live display of script execution progress
- **Progress Tracking**: Visual indicators for long-running operations
- **Error Handling**: Clear error messages and recovery options

### 3. User Experience
- **Professional Styling**: Consistent with Windows design guidelines
- **Intuitive Navigation**: Logical flow from configuration to deployment
- **Confirmation Dialogs**: Safety prompts for destructive operations
- **Status Feedback**: Always-visible status bar with current operation
- **Responsive UI**: Non-blocking interface during script execution

## Usage

### Step 1: Configure vCenter Deployment

1. **Launch the application**
2. **Go to Configuration tab**
3. **Fill in required fields**:
   - VM Name for the vCenter appliance
   - Path to `vcsa-deploy.exe` (from mounted vCenter ISO)
   - vCenter hostname and credentials
   - ESXi host details and credentials
   - Network configuration (IP, DNS, Gateway)
   - Deployment options (size, datastore, etc.)

### Step 2: Save Configuration

1. **Use File menu** → **Save .env File** to save your configuration
2. **Or use File menu** → **Load .env File** to load existing configuration
3. **Or use File menu** → **Load from Example** to start with template

### Step 3: Deploy vCenter

1. **Go to Deployment tab**
2. **Set working directory** (where PowerShell scripts are located)
3. **Click "Validate Environment"** to check prerequisites
4. **Click "Start Deployment"** to begin the vCenter deployment process
5. **Monitor progress** in the output window

## Configuration Fields

### General Tab
- **VM Name**: Name for the vCenter virtual machine
- **VCSA CLI Path**: Full path to `vcsa-deploy.exe` from vCenter ISO
- **vCenter FQDN**: Fully qualified domain name for vCenter

### Credentials Tab
- **vCenter SSO Password**: Password for vCenter administrator
- **vCenter Root Password**: Root password for the vCenter appliance
- **ESXi Host**: IP address or hostname of target ESXi host
- **ESXi Username**: ESXi administrator username (typically 'root')
- **ESXi Password**: ESXi administrator password

### Network Tab
- **IP Address**: Static IP address for vCenter appliance
- **DNS Servers**: Comma-separated list of DNS server IPs
- **Network Prefix**: Subnet mask in CIDR notation (e.g., 24)
- **Gateway**: Default gateway IP address
- **Deployment Network**: ESXi port group for deployment

### Deployment Tab
- **Datastore**: ESXi datastore name for vCenter storage
- **Thin Disk Mode**: Enable thin provisioning for virtual disks
- **Deployment Size**: vCenter appliance size (tiny/small/medium/large/xlarge)
- **NTP Servers**: Time synchronization servers

### Advanced Tab
- **SSO Domain**: Single Sign-On domain (default: vsphere.local)
- **CEIP Settings**: Customer Experience Improvement Program participation

## Validation Rules

The GUI performs real-time validation on all input fields:

- **Required Fields**: Must be filled before deployment
- **IP Addresses**: Must be valid IPv4 format
- **Hostnames**: Must be valid FQDN or hostname format
- **Paths**: File paths are validated for existence
- **Network Prefix**: Must be valid CIDR notation (1-32)
- **VM Name**: Only alphanumeric, hyphens, and underscores allowed

## Troubleshooting

### Common Issues

1. **"PowerShell not found"**
   - Ensure PowerShell is installed and in PATH
   - Try both `powershell.exe` (Windows PowerShell) and `pwsh.exe` (PowerShell Core)

2. **"Scripts not found"**
   - Verify working directory contains `setup.ps1` and `PrepareEnvironment.ps1`
   - Check that you're in the correct project directory

3. **"Access denied" errors**
   - Run the application as Administrator
   - Check PowerShell execution policy: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`

4. **Validation failures**
   - Verify network connectivity to ESXi host
   - Check ESXi credentials and permissions
   - Ensure vCenter ISO is mounted and path is correct

### Debug Mode

To see detailed PowerShell output, monitor the output window in the Deployment tab. All script output, including errors, is displayed in real-time.

### Log Files

PowerShell scripts may generate log files in the working directory. Check for:
- `*.log` files for detailed execution logs
- `vcenter-deploy.json` for generated configuration files

## Integration with Existing Scripts

The GUI integrates seamlessly with the existing PowerShell automation:

- **Reads from `env.example`**: Uses the template file for field definitions
- **Generates `.env` files**: Creates properly formatted environment files
- **Executes `setup.ps1`**: Runs the main deployment script
- **Uses `PrepareEnvironment.ps1`**: Leverages existing validation logic

## Security Considerations

- **Password Protection**: Sensitive fields are masked in the UI
- **No Password Storage**: Passwords are only kept in memory during use
- **File Permissions**: Generated .env files inherit system permissions
- **Process Isolation**: PowerShell scripts run in separate processes

## Support

For issues with the GUI wrapper:
1. Check the troubleshooting section above
2. Review PowerShell script output in the Deployment tab
3. Verify that the underlying PowerShell scripts work independently

For issues with the vCenter deployment process:
1. Refer to the main project README.md
2. Check VMware documentation for vCenter deployment
3. Verify network connectivity and credentials