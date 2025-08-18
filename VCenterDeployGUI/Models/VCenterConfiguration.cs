using System.ComponentModel;

namespace VCenterDeployGUI.Models
{
    public class VCenterConfiguration : INotifyPropertyChanged
    {
        private string _vmName = string.Empty;
        private string _vcsaCliPath = "[driveletter]:\\vcsa-cli-installer\\win32\\vcsa-deploy.exe";
        private string _vcsaHost = string.Empty;
        private string _vcPassword = string.Empty;
        private string _vcsaRootPassword = string.Empty;
        private string _esxiHost = string.Empty;
        private string _esxiUser = string.Empty;
        private string _esxiPassword = string.Empty;
        private string _ntpServers = "pool.ntp.org";
        private string _deploymentNetwork = "VM Network";
        private string _datastore = string.Empty;
        private bool _thinDiskMode = false;
        private string _deploymentOption = "small";
        private string _ipAddress = string.Empty;
        private string _dnsServers = string.Empty;
        private string _networkPrefix = string.Empty;
        private string _gateway = string.Empty;
        private string _ssoDomain = "vsphere.local";
        private bool _ceipSettings = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        [Description("vCenter VM name")]
        [Category("General")]
        [DisplayName("VM Name")]
        public string VmName
        {
            get => _vmName;
            set { _vmName = value; OnPropertyChanged(nameof(VmName)); }
        }

        [Description("Path to vcsa-deploy.exe (from vCenter ISO)")]
        [Category("General")]
        [DisplayName("VCSA CLI Path")]
        public string VcsaCliPath
        {
            get => _vcsaCliPath;
            set { _vcsaCliPath = value; OnPropertyChanged(nameof(VcsaCliPath)); }
        }

        [Description("vCenter FQDN")]
        [Category("General")]
        [DisplayName("VCSA Host")]
        public string VcsaHost
        {
            get => _vcsaHost;
            set { _vcsaHost = value; OnPropertyChanged(nameof(VcsaHost)); }
        }

        [Description("vCenter SSO password")]
        [Category("Credentials")]
        [DisplayName("VC Password")]
        public string VcPassword
        {
            get => _vcPassword;
            set { _vcPassword = value; OnPropertyChanged(nameof(VcPassword)); }
        }

        [Description("vCenter root password")]
        [Category("Credentials")]
        [DisplayName("VCSA Root Password")]
        public string VcsaRootPassword
        {
            get => _vcsaRootPassword;
            set { _vcsaRootPassword = value; OnPropertyChanged(nameof(VcsaRootPassword)); }
        }

        [Description("ESXi host IP/FQDN")]
        [Category("ESXi")]
        [DisplayName("ESXi Host")]
        public string EsxiHost
        {
            get => _esxiHost;
            set { _esxiHost = value; OnPropertyChanged(nameof(EsxiHost)); }
        }

        [Description("ESXi username")]
        [Category("ESXi")]
        [DisplayName("ESXi User")]
        public string EsxiUser
        {
            get => _esxiUser;
            set { _esxiUser = value; OnPropertyChanged(nameof(EsxiUser)); }
        }

        [Description("ESXi password")]
        [Category("ESXi")]
        [DisplayName("ESXi Password")]
        public string EsxiPassword
        {
            get => _esxiPassword;
            set { _esxiPassword = value; OnPropertyChanged(nameof(EsxiPassword)); }
        }

        [Description("NTP servers (comma-separated)")]
        [Category("Networking")]
        [DisplayName("NTP Servers")]
        public string NtpServers
        {
            get => _ntpServers;
            set { _ntpServers = value; OnPropertyChanged(nameof(NtpServers)); }
        }

        [Description("ESXi port group")]
        [Category("Networking")]
        [DisplayName("Deployment Network")]
        public string DeploymentNetwork
        {
            get => _deploymentNetwork;
            set { _deploymentNetwork = value; OnPropertyChanged(nameof(DeploymentNetwork)); }
        }

        [Description("ESXi datastore name")]
        [Category("Storage")]
        [DisplayName("Datastore")]
        public string Datastore
        {
            get => _datastore;
            set { _datastore = value; OnPropertyChanged(nameof(Datastore)); }
        }

        [Description("Thin provisioning: true/false")]
        [Category("Storage")]
        [DisplayName("Thin Disk Mode")]
        public bool ThinDiskMode
        {
            get => _thinDiskMode;
            set { _thinDiskMode = value; OnPropertyChanged(nameof(ThinDiskMode)); }
        }

        [Description("vCenter size: tiny, small, medium, large, xlarge")]
        [Category("Deployment")]
        [DisplayName("Deployment Option")]
        public string DeploymentOption
        {
            get => _deploymentOption;
            set { _deploymentOption = value; OnPropertyChanged(nameof(DeploymentOption)); }
        }

        [Description("vCenter IP address")]
        [Category("Network Config")]
        [DisplayName("IP Address")]
        public string IpAddress
        {
            get => _ipAddress;
            set { _ipAddress = value; OnPropertyChanged(nameof(IpAddress)); }
        }

        [Description("DNS servers (comma-separated)")]
        [Category("Network Config")]
        [DisplayName("DNS Servers")]
        public string DnsServers
        {
            get => _dnsServers;
            set { _dnsServers = value; OnPropertyChanged(nameof(DnsServers)); }
        }

        [Description("Subnet mask (e.g., 24)")]
        [Category("Network Config")]
        [DisplayName("Network Prefix")]
        public string NetworkPrefix
        {
            get => _networkPrefix;
            set { _networkPrefix = value; OnPropertyChanged(nameof(NetworkPrefix)); }
        }

        [Description("Default gateway")]
        [Category("Network Config")]
        [DisplayName("Gateway")]
        public string Gateway
        {
            get => _gateway;
            set { _gateway = value; OnPropertyChanged(nameof(Gateway)); }
        }

        [Description("SSO domain")]
        [Category("SSO")]
        [DisplayName("SSO Domain")]
        public string SsoDomain
        {
            get => _ssoDomain;
            set { _ssoDomain = value; OnPropertyChanged(nameof(SsoDomain)); }
        }

        [Description("Customer Experience Program: true/false")]
        [Category("General")]
        [DisplayName("CEIP Settings")]
        public bool CeipSettings
        {
            get => _ceipSettings;
            set { _ceipSettings = value; OnPropertyChanged(nameof(CeipSettings)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(VmName) &&
                   !string.IsNullOrWhiteSpace(VcsaCliPath) &&
                   !string.IsNullOrWhiteSpace(VcsaHost) &&
                   !string.IsNullOrWhiteSpace(VcPassword) &&
                   !string.IsNullOrWhiteSpace(VcsaRootPassword) &&
                   !string.IsNullOrWhiteSpace(EsxiHost) &&
                   !string.IsNullOrWhiteSpace(EsxiUser) &&
                   !string.IsNullOrWhiteSpace(EsxiPassword) &&
                   !string.IsNullOrWhiteSpace(Datastore) &&
                   !string.IsNullOrWhiteSpace(IpAddress) &&
                   !string.IsNullOrWhiteSpace(DnsServers) &&
                   !string.IsNullOrWhiteSpace(NetworkPrefix) &&
                   !string.IsNullOrWhiteSpace(Gateway);
        }
    }
}