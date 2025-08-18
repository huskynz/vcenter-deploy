using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VCenterDeployGUI.Models;
using VCenterDeployGUI.Services;

namespace VCenterDeployGUI
{
    public partial class MainForm
    {
        private void OnNewConfiguration(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Create a new configuration? All unsaved changes will be lost.", 
                               "New Configuration", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var newConfig = new VCenterConfiguration();
                LoadConfigurationIntoControls(newConfig);
                UpdateStatus("New configuration created");
                LogMessage("New configuration created", System.Drawing.Color.Cyan);
            }
        }

        private void OnLoadConfiguration(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Environment Files (*.env)|*.env|All Files (*.*)|*.*",
                Title = "Load Configuration"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var config = _configService.LoadFromEnvFile(openFileDialog.FileName);
                    LoadConfigurationIntoControls(config);
                    UpdateStatus($"Configuration loaded from {Path.GetFileName(openFileDialog.FileName)}");
                    LogMessage($"Configuration loaded from {openFileDialog.FileName}", System.Drawing.Color.Green);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading configuration: {ex.Message}", 
                                   "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogMessage($"Error loading configuration: {ex.Message}", System.Drawing.Color.Red);
                }
            }
        }

        private void OnSaveConfiguration(object? sender, EventArgs e)
        {
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "Environment Files (*.env)|*.env|All Files (*.*)|*.*",
                DefaultExt = "env",
                FileName = ".env",
                Title = "Save Configuration"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _configService.SaveToEnvFile(_configuration, saveFileDialog.FileName);
                    UpdateStatus($"Configuration saved to {Path.GetFileName(saveFileDialog.FileName)}");
                    LogMessage($"Configuration saved to {saveFileDialog.FileName}", System.Drawing.Color.Green);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving configuration: {ex.Message}", 
                                   "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogMessage($"Error saving configuration: {ex.Message}", System.Drawing.Color.Red);
                }
            }
        }

        private void OnLoadTemplate(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Template Files (env.example)|env.example|Environment Files (*.env)|*.env|All Files (*.*)|*.*",
                Title = "Load Template"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var config = _configService.LoadFromTemplate(openFileDialog.FileName);
                    LoadConfigurationIntoControls(config);
                    UpdateStatus($"Template loaded from {Path.GetFileName(openFileDialog.FileName)}");
                    LogMessage($"Template loaded from {openFileDialog.FileName}", System.Drawing.Color.Green);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading template: {ex.Message}", 
                                   "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogMessage($"Error loading template: {ex.Message}", System.Drawing.Color.Red);
                }
            }
        }

        private void OnExit(object? sender, EventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                if (MessageBox.Show("A deployment is in progress. Are you sure you want to exit?", 
                                   "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return;
                }
                
                _cancellationTokenSource.Cancel();
            }

            Application.Exit();
        }

        private void OnValidateConfiguration(object? sender, EventArgs e)
        {
            try
            {
                if (_configuration.IsValid())
                {
                    MessageBox.Show("Configuration is valid!", "Validation", 
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LogMessage("Configuration validation passed", System.Drawing.Color.Green);
                    UpdateStatus("Configuration is valid");
                }
                else
                {
                    MessageBox.Show("Configuration has missing required fields. Please check all required fields marked with *.", 
                                   "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogMessage("Configuration validation failed - missing required fields", System.Drawing.Color.Red);
                    UpdateStatus("Configuration validation failed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error validating configuration: {ex.Message}", 
                               "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"Error validating configuration: {ex.Message}", System.Drawing.Color.Red);
            }
        }

        private async void OnStartDeployment(object? sender, EventArgs e)
        {
            if (!_configuration.IsValid())
            {
                MessageBox.Show("Please fix all configuration errors before starting deployment.", 
                               "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Start vCenter deployment? This may take a significant amount of time.", 
                                        "Start Deployment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result != DialogResult.Yes)
                return;

            try
            {
                // First save the current configuration to .env file
                var scriptDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                var parentDirectory = Directory.GetParent(scriptDirectory)?.Parent?.Parent?.Parent?.FullName;
                
                if (parentDirectory == null)
                {
                    MessageBox.Show("Could not locate script directory.", "Error", 
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var envPath = Path.Combine(parentDirectory, ".env");
                _configService.SaveToEnvFile(_configuration, envPath);
                LogMessage($"Configuration saved to {envPath}", System.Drawing.Color.Cyan);

                // Disable deployment button and enable stop button
                var deployButton = FindControl("Start Deployment");
                var stopButton = FindControl("Stop Deployment");
                
                if (deployButton != null) deployButton.Enabled = false;
                if (stopButton != null) stopButton.Enabled = true;

                ShowProgress(true);
                UpdateStatus("Starting deployment...");
                LogMessage("Starting vCenter deployment...", System.Drawing.Color.Cyan);

                // Switch to logs tab
                _tabControl.SelectedIndex = _tabControl.TabPages.Count - 1;

                _cancellationTokenSource = new CancellationTokenSource();

                var success = await _powerShellService.ExecuteSetupScriptAsync(
                    parentDirectory, _cancellationTokenSource.Token);

                if (success)
                {
                    LogMessage("Deployment completed successfully!", System.Drawing.Color.Green);
                    UpdateStatus("Deployment completed successfully");
                    MessageBox.Show("vCenter deployment completed successfully!", "Success", 
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    LogMessage("Deployment failed. Check the logs for details.", System.Drawing.Color.Red);
                    UpdateStatus("Deployment failed");
                }
            }
            catch (OperationCanceledException)
            {
                LogMessage("Deployment was cancelled by user.", System.Drawing.Color.Yellow);
                UpdateStatus("Deployment cancelled");
            }
            catch (Exception ex)
            {
                LogMessage($"Error during deployment: {ex.Message}", System.Drawing.Color.Red);
                UpdateStatus("Deployment error");
                MessageBox.Show($"Error during deployment: {ex.Message}", "Error", 
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ShowProgress(false);
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                var deployButton = FindControl("Start Deployment");
                var stopButton = FindControl("Stop Deployment");
                
                if (deployButton != null) deployButton.Enabled = true;
                if (stopButton != null) stopButton.Enabled = false;
            }
        }

        private void OnStopDeployment(object? sender, EventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                var result = MessageBox.Show("Are you sure you want to stop the deployment?", 
                                            "Stop Deployment", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    _cancellationTokenSource.Cancel();
                    LogMessage("Deployment stop requested by user...", System.Drawing.Color.Yellow);
                    UpdateStatus("Stopping deployment...");
                }
            }
        }

        private void OnSaveLogs(object? sender, EventArgs e)
        {
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"vcenter-deploy-logs-{DateTime.Now:yyyy-MM-dd-HHmmss}.txt",
                Title = "Save Logs"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, _logTextBox.Text);
                    MessageBox.Show($"Logs saved to {saveFileDialog.FileName}", "Success", 
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving logs: {ex.Message}", "Error", 
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnAbout(object? sender, EventArgs e)
        {
            var aboutText = $@"vCenter Deployment GUI
Version 1.0

A Windows Forms wrapper for the vCenter deployment automation scripts.

Features:
• Tabbed interface for configuration management
• Real-time field validation
• .env file import/export
• PowerShell script integration
• Progress tracking and logging

Built with .NET 8 and Windows Forms
© 2024";

            MessageBox.Show(aboutText, "About vCenter Deployment GUI", 
                           MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                var result = MessageBox.Show("A deployment is in progress. Are you sure you want to close?", 
                                            "Close Application", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                
                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                
                _cancellationTokenSource.Cancel();
            }

            base.OnFormClosing(e);
        }
    }
}