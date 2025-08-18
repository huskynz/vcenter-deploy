using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using VCenterDeployGUI.Models;

namespace VCenterDeployGUI.Services
{
    public class ConfigurationService
    {
        public VCenterConfiguration LoadFromEnvFile(string filePath)
        {
            var config = new VCenterConfiguration();
            
            if (!File.Exists(filePath))
                return config;

            var lines = File.ReadAllLines(filePath);
            var envVars = new Dictionary<string, string>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                var parts = line.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim().Trim('"');
                    envVars[key] = value;
                }
            }

            // Map environment variables to configuration properties
            if (envVars.TryGetValue("VM_NAME", out var vmName))
                config.VmName = vmName;
            if (envVars.TryGetValue("VCSA_CLI_PATH", out var vcsaCliPath))
                config.VcsaCliPath = vcsaCliPath;
            if (envVars.TryGetValue("VCSA_HOST", out var vcsaHost))
                config.VcsaHost = vcsaHost;
            if (envVars.TryGetValue("VC_PASSWORD", out var vcPassword))
                config.VcPassword = vcPassword;
            if (envVars.TryGetValue("VCSA_ROOT_PASSWORD", out var vcsaRootPassword))
                config.VcsaRootPassword = vcsaRootPassword;
            if (envVars.TryGetValue("ESXI_HOST", out var esxiHost))
                config.EsxiHost = esxiHost;
            if (envVars.TryGetValue("ESXI_USER", out var esxiUser))
                config.EsxiUser = esxiUser;
            if (envVars.TryGetValue("ESXI_PASSWORD", out var esxiPassword))
                config.EsxiPassword = esxiPassword;
            if (envVars.TryGetValue("NTP_SERVERS", out var ntpServers))
                config.NtpServers = ntpServers;
            if (envVars.TryGetValue("DEPLOYMENT_NETWORK", out var deploymentNetwork))
                config.DeploymentNetwork = deploymentNetwork;
            if (envVars.TryGetValue("DATASTORE", out var datastore))
                config.Datastore = datastore;
            if (envVars.TryGetValue("THIN_DISK_MODE", out var thinDiskMode))
                config.ThinDiskMode = thinDiskMode.ToLowerInvariant() == "true";
            if (envVars.TryGetValue("DEPLOYMENT_OPTION", out var deploymentOption))
                config.DeploymentOption = deploymentOption;
            if (envVars.TryGetValue("IP_ADDRESS", out var ipAddress))
                config.IpAddress = ipAddress;
            if (envVars.TryGetValue("DNS_SERVERS", out var dnsServers))
                config.DnsServers = dnsServers;
            if (envVars.TryGetValue("NETWORK_PREFIX", out var networkPrefix))
                config.NetworkPrefix = networkPrefix;
            if (envVars.TryGetValue("GATEWAY", out var gateway))
                config.Gateway = gateway;
            if (envVars.TryGetValue("SSO_DOMAIN", out var ssoDomain))
                config.SsoDomain = ssoDomain;
            if (envVars.TryGetValue("CEIP_SETTINGS", out var ceipSettings))
                config.CeipSettings = ceipSettings.ToLowerInvariant() == "true";

            return config;
        }

        public void SaveToEnvFile(VCenterConfiguration config, string filePath)
        {
            var lines = new List<string>
            {
                "#Vcenter Applaince NAME",
                $"VM_NAME={config.VmName}",
                "",
                "# Required Paths (Mount the Vcenter DVD and past the path of \\vcsa-cli-installer\\win32\\vcsa-deploy.exe INCLUDE THE DEPLOY.exe ASWELL))",
                $"VCSA_CLI_PATH={config.VcsaCliPath}",
                $"VCSA_HOST={config.VcsaHost}",
                "",
                "# vCenter Credentials",
                $"VC_PASSWORD={config.VcPassword}",
                $"VCSA_ROOT_PASSWORD={config.VcsaRootPassword}",
                "",
                "# ESXi Host Credentials",
                $"ESXI_HOST={config.EsxiHost}",
                $"ESXI_USER={config.EsxiUser}",
                $"ESXI_PASSWORD={config.EsxiPassword}",
                "",
                "# NTP Servers (comma-separated)",
                $"NTP_SERVERS={config.NtpServers}",
                "",
                "# Networking & Deployment options",
                $"DEPLOYMENT_NETWORK=\"{config.DeploymentNetwork}\"",
                $"DATASTORE=\"{config.Datastore}\"",
                $"THIN_DISK_MODE={config.ThinDiskMode.ToString().ToLowerInvariant()}",
                $"DEPLOYMENT_OPTION={config.DeploymentOption}",
                "",
                "# Network config",
                $"IP_ADDRESS={config.IpAddress}",
                $"DNS_SERVERS={config.DnsServers}",
                $"NETWORK_PREFIX={config.NetworkPrefix}",
                $"GATEWAY={config.Gateway}",
                "",
                "# SSO config",
                $"SSO_DOMAIN={config.SsoDomain}",
                "",
                "# CEIP (Customer Experience Improvement Program) opt-in: true/false",
                $"CEIP_SETTINGS={config.CeipSettings.ToString().ToLowerInvariant()}"
            };

            File.WriteAllLines(filePath, lines);
        }

        public VCenterConfiguration LoadFromTemplate(string templatePath = "env.example")
        {
            // Load default values from env.example if it exists
            if (File.Exists(templatePath))
            {
                return LoadFromEnvFile(templatePath);
            }

            // Return a new configuration with defaults
            return new VCenterConfiguration();
        }
    }
}