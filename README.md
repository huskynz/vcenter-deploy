# HuskyNZ VCenter Deploy

<div align="center">
  <a href="https://github.com/huskynz/vcenter-deploy">
    <img src="https://serv.hnz.li/logo/default.png" alt="Logo" width="80" height="80">
  </a>

  <h3>HuskyNZ VCenter Deploy</h3>
  <p><strong>Reproducible VMware vCenter Server Appliance Deployment</strong></p>
</div>

<div align="center">

![Contributors](https://img.shields.io/github/contributors/HuskyNZ/vcenter-deploy?color=dark-green)
![Issues](https://img.shields.io/github/issues/HuskyNZ/vcenter-deploy)
![License](https://img.shields.io/github/license/HuskyNZ/vcenter-deploy)

</div>

## Overview

Reproducible VMware vCenter Server Appliance (VCSA) deployment automation. Deploy vCenter consistently across environments with version-controlled configuration. Perfect for labs, testing, and production where you need reliable, repeatable deployments.

## Quick Start

### Prerequisites
- Windows with PowerShell 5.1+
- VMware vCenter ISO mounted
- ESXi host with available resources
- ESXi admin credentials

### Setup
1. **Clone and configure:**
   ```powershell
   git clone https://github.com/huskynz/vcenter-deploy.git
   cd vcenter-deploy
   Copy-Item env.example .env
   ```

2. **Edit `.env` with your settings:**

    See the Configuration Options below

3. **Deploy (CLI recommended):**
   ```powershell
   # Recommended: Command line
   .\setup.ps1
   
   # Experimental: GUI (VERY BUGGY)
   .\setupgui.ps1
   ```

## Configuration Options

### Required Settings
| Setting | Description | Default in .env.example |
|---------|-------------|-------------------------|
| `VCSA_CLI_PATH` | Path to vcsa-deploy | `[driveletter]:\vcsa-cli-installer\win32\vcsa-deploy.exe` |
| `ESXI_HOST` | ESXi host IP/FQDN | None |
| `ESXI_USER` | ESXi username | None |
| `ESXI_PASSWORD` | ESXi password | None | 
| `VCSA_HOST` | vCenter FQDN | None |
| `VC_PASSWORD` | vCenter SSO password | None |
| `VCSA_ROOT_PASSWORD` | vCenter root password | None |
| `IP_ADDRESS` | vCenter IP address | None |
| `GATEWAY` | Default gateway | None |
| `DNS_SERVERS` | DNS servers (comma-separated) | None |
| `NETWORK_PREFIX` | Subnet mask (e.g., 24) | None |
| `DEPLOYMENT_NETWORK` | ESXi port group | `VM Network` |
| `DATASTORE` | ESXi datastore name | None |
| `DEPLOYMENT_OPTION` | vCenter size: `tiny`, `small`, `medium`, `large`, `xlarge` | `small` |
| `THIN_DISK_MODE` | Thin provisioning: `true`/`false` | `false` |
| `NTP_SERVERS` | NTP servers (comma-separated) | `pool.ntp.org` |
| `SSO_DOMAIN` | SSO domain | `vsphere.local` |
| `CEIP_SETTINGS` | Customer Experience Program: `true`/`false` | `false` |

## Features

- **CLI Interface** (recommended) - Stable, reliable deployment
- **GUI Interface** (experimental) - Use at your own risk
- Environment-based configuration
- Automatic PowerCLI setup
- ESXi connectivity validation
- Progress tracking and error handling
- Reproducible deployments

## Troubleshooting

**PowerCLI issues:** Run as Administrator:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

**Connection issues:** Verify ESXi host accessibility and credentials

**Path issues:** Ensure vCenter ISO is mounted and `vcsa-deploy.exe` path is correct

**VM exists:** Script automatically checks for existing VMs and exits if found

## Built With

- PowerShell
- VMware PowerCLI
- JSON configuration
- Git

## License

MIT License - see [LICENSE](LICENSE) file

## Author

[HuskyNZ](https://www.husky.nz)