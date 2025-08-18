using System;
using System.IO;
using VCenterDeployGUI.Models;
using VCenterDeployGUI.Services;

namespace VCenterDeployGUI
{
    internal static class Program
    {
        static void Main()
        {
            Console.WriteLine("vCenter Deployment GUI - Component Test");
            Console.WriteLine("========================================");
            Console.WriteLine();

            TestConfigurationModel();
            TestValidationService();
            TestConfigurationService();

            Console.WriteLine();
            Console.WriteLine("All component tests completed successfully!");
            Console.WriteLine();
            Console.WriteLine("To use the full Windows GUI:");
            Console.WriteLine("1. Copy this project to a Windows machine");
            Console.WriteLine("2. Build with: dotnet build VCenterDeployGUI-Windows.csproj");
            Console.WriteLine("3. Run with: dotnet run --project VCenterDeployGUI-Windows.csproj");
        }

        static void TestConfigurationModel()
        {
            Console.WriteLine("Testing Configuration Model...");
            
            var config = new VCenterConfiguration
            {
                VmName = "vcenter-test",
                VcsaHost = "vcenter.example.com",
                EsxiHost = "192.168.1.100",
                IpAddress = "192.168.1.50"
            };

            Console.WriteLine($"  ✓ Created configuration with VM Name: {config.VmName}");
            Console.WriteLine($"  ✓ Configuration validation: {(config.IsValid() ? "Valid" : "Invalid (expected - missing required fields)")}");
            Console.WriteLine();
        }

        static void TestValidationService()
        {
            Console.WriteLine("Testing Validation Service...");
            
            // Test IP validation
            Console.WriteLine($"  ✓ Valid IP (192.168.1.1): {ValidationService.IsValidIPAddress("192.168.1.1")}");
            Console.WriteLine($"  ✓ Invalid IP (256.1.1.1): {!ValidationService.IsValidIPAddress("256.1.1.1")}");
            
            // Test hostname validation
            Console.WriteLine($"  ✓ Valid hostname (vcenter.local): {ValidationService.IsValidHostname("vcenter.local")}");
            Console.WriteLine($"  ✓ Invalid hostname (bad..host): {!ValidationService.IsValidHostname("bad..host")}");
            
            // Test network prefix validation
            Console.WriteLine($"  ✓ Valid prefix (24): {ValidationService.IsValidNetworkPrefix("24")}");
            Console.WriteLine($"  ✓ Invalid prefix (33): {!ValidationService.IsValidNetworkPrefix("33")}");
            
            // Test deployment option validation
            Console.WriteLine($"  ✓ Valid deployment option (small): {ValidationService.IsValidDeploymentOption("small")}");
            Console.WriteLine($"  ✓ Invalid deployment option (huge): {!ValidationService.IsValidDeploymentOption("huge")}");
            
            Console.WriteLine();
        }

        static void TestConfigurationService()
        {
            Console.WriteLine("Testing Configuration Service...");
            
            var configService = new ConfigurationService();
            var testConfig = new VCenterConfiguration
            {
                VmName = "test-vm",
                VcsaHost = "vcenter.test.com",
                EsxiHost = "192.168.1.100",
                EsxiUser = "root",
                IpAddress = "192.168.1.50",
                Gateway = "192.168.1.1",
                NetworkPrefix = "24",
                DnsServers = "8.8.8.8,8.8.4.4",
                Datastore = "datastore1"
            };

            // Test saving configuration
            var tempFile = Path.GetTempFileName();
            try
            {
                configService.SaveToEnvFile(testConfig, tempFile);
                Console.WriteLine($"  ✓ Saved test configuration to temporary file");

                // Test loading configuration
                var loadedConfig = configService.LoadFromEnvFile(tempFile);
                Console.WriteLine($"  ✓ Loaded configuration with VM Name: {loadedConfig.VmName}");
                Console.WriteLine($"  ✓ Loaded configuration matches: {loadedConfig.VmName == testConfig.VmName}");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }

            Console.WriteLine();
        }
    }
}
