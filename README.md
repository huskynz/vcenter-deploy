<br/>
<p align="center">
  <a href="https://github.com/huskynz/vcenter-deploy">
    <img src="https://serv.hnz.li/logo/default.png" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">HuskyNZ Vcenter Deploy</h3>
  <p align="center">Reproducible Vcenter Deployments</p>
  <br>

</p>

![Contributors](https://img.shields.io/github/contributors/HuskyNZ/vcenter-deploy?color=dark-green) ![Issues](https://img.shields.io/github/issues/HuskyNZ/vcenter-deploy) ![License](https://img.shields.io/github/license/HuskyNZ/vcenter-deploy)

## About The Project

**HuskyNZ Vcenter Deploy** is a PowerShell script that automates VMware vCenter Server Appliance (VCSA) deployment. It's designed for users who want to make the deployment of vCenter reproducible. This is extremely useful for labs where you might be tearing down your environment a lot.

## Quick Start

### 1. Choose Your Deployment Method

#### **A. Command Line**

1. Copy the example environment file:
    ```powershell
    cp env.example .env
    ```
2. Edit `.env` with your deployment settings (use any text editor).
3. Run the deployment script:
    ```powershell
    .\setup.ps1
    ```

#### **B. GUI**

1. Start the GUI:
    ```powershell
    .\setupgui.ps1
    ```
2. Enter or load your configuration directly in the app.
3. Review your settings and click deploy.

**Requirements:** VMware vCenter ISO mounted (e.g., `E:\vcsa-cli-installer\win32\vcsa-deploy.exe`), accessible ESXi host, PowerShell 5.1+

## Built With

- Powershell
- Json
- Git
- Powercli

## Roadmap
N/A

## Contributing

N/A

## License

Distributed under the MIT License. See [LICENSE](https://github.com/huskynz/template/blob/master/LICENSE) for more information.

## Authors

- [HuskyNZ](https://www.husky.nz)

## Acknowledgements