using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VCenterDeployGUI.Services
{
    public class PowerShellService
    {
        public event EventHandler<string>? OutputReceived;
        public event EventHandler<string>? ErrorReceived;
        public event EventHandler<int>? ProcessExited;

        public async Task<bool> ExecuteSetupScriptAsync(string workingDirectory, CancellationToken cancellationToken = default)
        {
            try
            {
                var setupScriptPath = Path.Combine(workingDirectory, "setup.ps1");
                if (!File.Exists(setupScriptPath))
                {
                    ErrorReceived?.Invoke(this, "Setup script not found: " + setupScriptPath);
                    return false;
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{setupScriptPath}\"",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processStartInfo };
                
                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        OutputReceived?.Invoke(this, args.Data);
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        ErrorReceived?.Invoke(this, args.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);
                
                ProcessExited?.Invoke(this, process.ExitCode);
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Error executing PowerShell script: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ExecutePrepareEnvironmentScriptAsync(string workingDirectory, CancellationToken cancellationToken = default)
        {
            try
            {
                var prepareScriptPath = Path.Combine(workingDirectory, "PrepareEnvironment.ps1");
                if (!File.Exists(prepareScriptPath))
                {
                    ErrorReceived?.Invoke(this, "PrepareEnvironment script not found: " + prepareScriptPath);
                    return false;
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{prepareScriptPath}\"",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processStartInfo };
                
                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        OutputReceived?.Invoke(this, args.Data);
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                        ErrorReceived?.Invoke(this, args.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);
                
                ProcessExited?.Invoke(this, process.ExitCode);
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Error executing PowerShell script: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ValidateEnvironmentAsync(string workingDirectory, CancellationToken cancellationToken = default)
        {
            try
            {
                var envFile = Path.Combine(workingDirectory, ".env");
                if (!File.Exists(envFile))
                {
                    ErrorReceived?.Invoke(this, ".env file not found");
                    return false;
                }

                // Simple validation - just check if we can read the file
                var content = await File.ReadAllTextAsync(envFile, cancellationToken);
                OutputReceived?.Invoke(this, "Environment file validation completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Error validating environment: {ex.Message}");
                return false;
            }
        }
    }
}