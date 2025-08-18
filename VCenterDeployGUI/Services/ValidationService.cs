using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace VCenterDeployGUI.Services
{
    public static class ValidationService
    {
        public static bool IsValidIPAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            return IPAddress.TryParse(ipAddress, out _);
        }

        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                // Check if the path format is valid
                return Path.IsPathFullyQualified(path) || Path.IsPathRooted(path);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidHostname(string hostname)
        {
            if (string.IsNullOrWhiteSpace(hostname))
                return false;

            // Check if it's a valid IP address
            if (IsValidIPAddress(hostname))
                return true;

            // Check if it's a valid hostname/FQDN
            var hostnameRegex = new Regex(@"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$");
            return hostnameRegex.IsMatch(hostname);
        }

        public static bool IsValidNetworkPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return false;

            if (int.TryParse(prefix, out var prefixValue))
            {
                return prefixValue >= 0 && prefixValue <= 32;
            }

            return false;
        }

        public static bool IsValidDeploymentOption(string option)
        {
            if (string.IsNullOrWhiteSpace(option))
                return false;

            var validOptions = new[] { "tiny", "small", "medium", "large", "xlarge" };
            return Array.Exists(validOptions, o => o.Equals(option, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsValidDnsServers(string dnsServers)
        {
            if (string.IsNullOrWhiteSpace(dnsServers))
                return false;

            var servers = dnsServers.Split(',');
            foreach (var server in servers)
            {
                if (!IsValidIPAddress(server.Trim()))
                    return false;
            }

            return true;
        }

        public static bool IsValidNtpServers(string ntpServers)
        {
            if (string.IsNullOrWhiteSpace(ntpServers))
                return false;

            var servers = ntpServers.Split(',');
            foreach (var server in servers)
            {
                var trimmed = server.Trim();
                if (!IsValidHostname(trimmed))
                    return false;
            }

            return true;
        }

        public static string GetValidationMessage(string fieldName, string value, Func<string, bool> validator)
        {
            if (string.IsNullOrWhiteSpace(value))
                return $"{fieldName} is required.";

            if (!validator(value))
                return $"{fieldName} has an invalid format.";

            return string.Empty;
        }
    }
}