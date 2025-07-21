<div align="center">
  <a href="https://github.com/huskynz/vcenter-deploy">
    <img src="https://serv.hnz.li/logo/default.png" alt="Logo" width="80" height="80">
  </a>

  <center><h3>HuskyNZ vCenter Deploy</h3></center>
  <p><strong>Automated, Reliable VMware vCenter Server Appliance Deployment</strong></p>
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
- Windows with PowerShell 5.1+ or PowerShell 7+
- VMware vCenter ISO mounted
- ESXi host with available resources
- ESXi admin credentials

### Setup

1. **Clone the repository and prepare your workspace:**
   - Open PowerShell and run:
     ```powershell
     git clone https://github.com/huskynz/vcenter-deploy.git
     cd vcenter-deploy
     ```
   - This creates a working directory for your configuration and scripts.

2. **Ensure prerequisites are met:**
   - Make sure you have:
     - Windows with PowerShell 5.1+ or PowerShell 7+
     - The VMware vCenter ISO mounted (so you can reference `vcsa-deploy.exe`)
     - Network access to your ESXi host
     - ESXi admin credentials

3a. **Create your environment file interactively (recommended):**
   - Run the guided setup:
     ```powershell
     .\PrepareEnvironment.ps1
     ```
   - This script will:
     - Prompt you for each required setting (with descriptions and validation)
     - Mask sensitive input (like passwords)
     - Group related settings for clarity
     - Show a summary before saving
     - Write a complete `.env` file ready for deployment
   - _Tip: You can abort at any prompt by typing `:q`._

3b. **(Alternative) Edit `.env` manually:**
   - Copy the example environment file:
     ```powershell
     Copy-Item env.example .env
     ```
   - Open `.env` in your preferred text editor (e.g., VS Code, Notepad++)
   - Fill in all required fields. Refer to the Configuration Options section below for descriptions and defaults.
   - Double-check paths (especially `VCSA_CLI_PATH`) and credentials for accuracy.

4. **Deploy vCenter:**
   - Start the deployment process:
     ```powershell
     .\setup.ps1
     ```
   - The script will:
     - Validate your `.env` file and required variables
     - Automatically check and install VMware PowerCLI if needed
     - Connect to your ESXi host and check for existing VMs
     - Generate a JSON config for the vCenter CLI installer
     - Run the deployment and show progress with color-coded logs
     - Disconnect from ESXi and report success or errors


## Configuration Options

All configuration is managed via the `.env` file. You can generate this interactively or copy and edit `env.example`.

| Setting              | Description                                         | Default in .env.example |
|----------------------|-----------------------------------------------------|-------------------------|
| `VM_NAME`            | vCenter VM name                                     |                         |
| `VCSA_CLI_PATH`      | Path to vcsa-deploy.exe (from vCenter ISO)          | `[driveletter]:\vcsa-cli-installer\win32\vcsa-deploy.exe` |
| `VCSA_HOST`          | vCenter FQDN                                       |                         |
| `VC_PASSWORD`        | vCenter SSO password                               |                         |
| `VCSA_ROOT_PASSWORD` | vCenter root password                              |                         |
| `ESXI_HOST`          | ESXi host IP/FQDN                                  |                         |
| `ESXI_USER`          | ESXi username                                      |                         |
| `ESXI_PASSWORD`      | ESXi password                                      |                         |
| `NTP_SERVERS`        | NTP servers (comma-separated)                      | `pool.ntp.org`          |
| `DEPLOYMENT_NETWORK` | ESXi port group                                    | `VM Network`            |
| `DATASTORE`          | ESXi datastore name                                |                         |
| `THIN_DISK_MODE`     | Thin provisioning: `true`/`false`                  | `false`                 |
| `DEPLOYMENT_OPTION`  | vCenter size: `tiny`, `small`, `medium`, `large`, `xlarge` | `small`        |
| `IP_ADDRESS`         | vCenter IP address                                 |                         |
| `DNS_SERVERS`        | DNS servers (comma-separated)                      |                         |
| `NETWORK_PREFIX`     | Subnet mask (e.g., 24)                             |                         |
| `GATEWAY`            | Default gateway                                    |                         |
| `SSO_DOMAIN`         | SSO domain                                         | `vsphere.local`         |
| `CEIP_SETTINGS`      | Customer Experience Program: `true`/`false`        | `false`                 |

## Features

- **Modular PowerShell codebase** for maintainability and extensibility
- **Interactive .env setup** with validation and descriptions (`PrepareEnvironment.ps1`)
- **Automated PowerCLI installation and configuration**
- **ESXi connectivity validation** and VM existence checks
- **Version-controlled, environment-based configuration**
- **Progress tracking and color-coded logging**
- **Automatic JSON config generation for VCSA CLI**
- **Graceful error handling and clear exit codes**
- **Reproducible, idempotent deployments**

## Usage

- Run `.\setup.ps1` to deploy vCenter using your `.env` configuration.
- Use `.\PrepareEnvironment.ps1` to interactively create or update your `.env` file.
- For help, run:
  ```powershell
  .\setup.ps1 -Help
  ```

## Troubleshooting

- **PowerCLI issues:** Run as Administrator:
  ```powershell
  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
  ```
- **Connection issues:** Verify ESXi host accessibility and credentials
- **Path issues:** Ensure vCenter ISO is mounted and `vcsa-deploy.exe`