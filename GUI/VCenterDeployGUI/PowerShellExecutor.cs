using System.Diagnostics;
using System.Text;

namespace VCenterDeployGUI
{
    public class PowerShellExecutor
    {
        public event EventHandler<string>? OutputReceived;
        public event EventHandler<string>? ErrorReceived;
        public event EventHandler<bool>? ExecutionCompleted;

        private Process? currentProcess;
        private readonly StringBuilder outputBuffer = new();
        private readonly StringBuilder errorBuffer = new();

        public bool IsRunning => currentProcess != null && !currentProcess.HasExited;

        public async Task<bool> RunPrepareEnvironmentAsync(string workingDirectory)
        {
            var scriptPath = Path.Combine(workingDirectory, "PrepareEnvironment.ps1");
            
            if (!File.Exists(scriptPath))
            {
                ErrorReceived?.Invoke(this, $"PrepareEnvironment.ps1 not found at {scriptPath}");
                return false;
            }

            var arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -NonInteractive";
            return await RunPowerShellAsync(arguments, workingDirectory);
        }

        public async Task<bool> RunDeploymentAsync(string workingDirectory, string envFilePath)
        {
            var setupScriptPath = Path.Combine(workingDirectory, "setup.ps1");
            
            if (!File.Exists(setupScriptPath))
            {
                ErrorReceived?.Invoke(this, $"setup.ps1 not found at {setupScriptPath}");
                return false;
            }

            if (!File.Exists(envFilePath))
            {
                ErrorReceived?.Invoke(this, $".env file not found at {envFilePath}");
                return false;
            }

            // Copy the env file to the working directory as .env
            var targetEnvPath = Path.Combine(workingDirectory, ".env");
            try
            {
                File.Copy(envFilePath, targetEnvPath, true);
                OutputReceived?.Invoke(this, $"Configuration copied to {targetEnvPath}");
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Failed to copy configuration: {ex.Message}");
                return false;
            }

            var arguments = $"-ExecutionPolicy Bypass -File \"{setupScriptPath}\"";
            return await RunPowerShellAsync(arguments, workingDirectory);
        }

        public async Task<bool> ValidateEnvironmentAsync(string workingDirectory, string envFilePath)
        {
            try
            {
                // Basic validation - check if required files exist
                var requiredFiles = new[] { "setup.ps1", "PrepareEnvironment.ps1" };
                var missingFiles = requiredFiles.Where(f => !File.Exists(Path.Combine(workingDirectory, f))).ToList();
                
                if (missingFiles.Any())
                {
                    ErrorReceived?.Invoke(this, $"Missing required files: {string.Join(", ", missingFiles)}");
                    return false;
                }

                if (!File.Exists(envFilePath))
                {
                    ErrorReceived?.Invoke(this, "Configuration file (.env) not found");
                    return false;
                }

                OutputReceived?.Invoke(this, "Environment validation passed");
                return true;
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Environment validation failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> RunPowerShellAsync(string arguments, string workingDirectory)
        {
            if (IsRunning)
            {
                ErrorReceived?.Invoke(this, "Another PowerShell process is already running");
                return false;
            }

            try
            {
                currentProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "pwsh.exe", // Use PowerShell Core if available
                        Arguments = arguments,
                        WorkingDirectory = workingDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    },
                    EnableRaisingEvents = true
                };

                // Try PowerShell Core first, fallback to Windows PowerShell
                if (!IsPowerShellCoreAvailable())
                {
                    currentProcess.StartInfo.FileName = "powershell.exe";
                }

                outputBuffer.Clear();
                errorBuffer.Clear();

                currentProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuffer.AppendLine(e.Data);
                        OutputReceived?.Invoke(this, e.Data);
                    }
                };

                currentProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuffer.AppendLine(e.Data);
                        ErrorReceived?.Invoke(this, e.Data);
                    }
                };

                currentProcess.Exited += (sender, e) =>
                {
                    var success = currentProcess?.ExitCode == 0;
                    ExecutionCompleted?.Invoke(this, success);
                };

                OutputReceived?.Invoke(this, $"Starting PowerShell: {currentProcess.StartInfo.FileName} {arguments}");
                
                currentProcess.Start();
                currentProcess.BeginOutputReadLine();
                currentProcess.BeginErrorReadLine();

                await currentProcess.WaitForExitAsync();
                
                return currentProcess.ExitCode == 0;
            }
            catch (Exception ex)
            {
                ErrorReceived?.Invoke(this, $"Failed to run PowerShell: {ex.Message}");
                ExecutionCompleted?.Invoke(this, false);
                return false;
            }
            finally
            {
                currentProcess?.Dispose();
                currentProcess = null;
            }
        }

        private static bool IsPowerShellCoreAvailable()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "pwsh.exe",
                    Arguments = "-Version",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                });
                
                return process?.WaitForExit(2000) == true && process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public void StopExecution()
        {
            if (currentProcess != null && !currentProcess.HasExited)
            {
                try
                {
                    currentProcess.Kill(true); // Kill entire process tree
                    OutputReceived?.Invoke(this, "Execution stopped by user");
                }
                catch (Exception ex)
                {
                    ErrorReceived?.Invoke(this, $"Failed to stop execution: {ex.Message}");
                }
            }
        }

        public string GetFullOutput()
        {
            return outputBuffer.ToString();
        }

        public string GetFullErrorOutput()
        {
            return errorBuffer.ToString();
        }
    }
}