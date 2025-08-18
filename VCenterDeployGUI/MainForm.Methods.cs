using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VCenterDeployGUI.Models;
using VCenterDeployGUI.Services;

namespace VCenterDeployGUI
{
    public partial class MainForm
    {
        private void AddConfigField(TableLayoutPanel panel, string labelText, string propertyName, 
            Dictionary<string, Control> controlCollection, bool isRequired = false, bool isPassword = false, bool isPath = false)
        {
            var rowIndex = panel.RowCount++;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = labelText + (isRequired ? " *" : ""),
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                ForeColor = isRequired ? Color.Red : SystemColors.ControlText
            };

            var textBox = new TextBox
            {
                Dock = DockStyle.Fill,
                UseSystemPasswordChar = isPassword,
                Tag = propertyName
            };

            // Add validation on text change
            textBox.TextChanged += (s, e) => ValidateField(textBox, propertyName, isRequired);
            textBox.Leave += (s, e) => UpdateConfigurationProperty(propertyName, textBox.Text);

            // Add browse button for path fields
            if (isPath)
            {
                var pathPanel = new Panel { Dock = DockStyle.Fill, Height = textBox.Height };
                textBox.Dock = DockStyle.Fill;
                textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

                var browseButton = new Button
                {
                    Text = "...",
                    Width = 30,
                    Dock = DockStyle.Right,
                    UseVisualStyleBackColor = true
                };
                browseButton.Click += (s, e) => BrowseForFile(textBox);

                pathPanel.Controls.Add(browseButton);
                pathPanel.Controls.Add(textBox);
                panel.Controls.Add(pathPanel, 1, rowIndex);
            }
            else
            {
                panel.Controls.Add(textBox, 1, rowIndex);
            }

            panel.Controls.Add(label, 0, rowIndex);
            controlCollection[propertyName] = textBox;

            // Add tooltip
            var tooltip = new ToolTip();
            var property = typeof(VCenterConfiguration).GetProperty(propertyName);
            if (property != null)
            {
                var descriptionAttr = property.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
                    .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
                if (descriptionAttr != null)
                {
                    tooltip.SetToolTip(textBox, descriptionAttr.Description);
                    tooltip.SetToolTip(label, descriptionAttr.Description);
                }
            }
        }

        private void AddCheckBoxField(TableLayoutPanel panel, string labelText, string propertyName, 
            Dictionary<string, Control> controlCollection)
        {
            var rowIndex = panel.RowCount++;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            var checkBox = new CheckBox
            {
                AutoSize = true,
                Tag = propertyName
            };

            checkBox.CheckedChanged += (s, e) => UpdateConfigurationProperty(propertyName, checkBox.Checked);

            panel.Controls.Add(label, 0, rowIndex);
            panel.Controls.Add(checkBox, 1, rowIndex);
            controlCollection[propertyName] = checkBox;

            // Add tooltip
            var tooltip = new ToolTip();
            var property = typeof(VCenterConfiguration).GetProperty(propertyName);
            if (property != null)
            {
                var descriptionAttr = property.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
                    .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;
                if (descriptionAttr != null)
                {
                    tooltip.SetToolTip(checkBox, descriptionAttr.Description);
                    tooltip.SetToolTip(label, descriptionAttr.Description);
                }
            }
        }

        private void AddDeploymentOptionField(TableLayoutPanel panel, string labelText, string propertyName, 
            Dictionary<string, Control> controlCollection)
        {
            var rowIndex = panel.RowCount++;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };

            var comboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Tag = propertyName
            };

            comboBox.Items.AddRange(new[] { "tiny", "small", "medium", "large", "xlarge" });
            comboBox.SelectedItem = "small";
            comboBox.SelectedValueChanged += (s, e) => UpdateConfigurationProperty(propertyName, comboBox.SelectedItem?.ToString() ?? "small");

            panel.Controls.Add(label, 0, rowIndex);
            panel.Controls.Add(comboBox, 1, rowIndex);
            controlCollection[propertyName] = comboBox;

            // Add tooltip
            var tooltip = new ToolTip();
            tooltip.SetToolTip(comboBox, "vCenter size: tiny, small, medium, large, xlarge");
            tooltip.SetToolTip(label, "vCenter size: tiny, small, medium, large, xlarge");
        }

        private void ValidateField(Control control, string propertyName, bool isRequired)
        {
            var value = control.Text;
            var isValid = true;
            var errorMessage = string.Empty;

            if (isRequired && string.IsNullOrWhiteSpace(value))
            {
                isValid = false;
                errorMessage = "This field is required.";
            }
            else if (!string.IsNullOrWhiteSpace(value))
            {
                switch (propertyName)
                {
                    case "IpAddress":
                    case "Gateway":
                        isValid = ValidationService.IsValidIPAddress(value);
                        if (!isValid) errorMessage = "Invalid IP address format.";
                        break;
                    case "EsxiHost":
                    case "VcsaHost":
                        isValid = ValidationService.IsValidHostname(value);
                        if (!isValid) errorMessage = "Invalid hostname or IP address format.";
                        break;
                    case "VcsaCliPath":
                        isValid = ValidationService.IsValidPath(value);
                        if (!isValid) errorMessage = "Invalid path format.";
                        break;
                    case "NetworkPrefix":
                        isValid = ValidationService.IsValidNetworkPrefix(value);
                        if (!isValid) errorMessage = "Network prefix must be between 0 and 32.";
                        break;
                    case "DnsServers":
                        isValid = ValidationService.IsValidDnsServers(value);
                        if (!isValid) errorMessage = "Invalid DNS servers format. Use comma-separated IP addresses.";
                        break;
                    case "NtpServers":
                        isValid = ValidationService.IsValidNtpServers(value);
                        if (!isValid) errorMessage = "Invalid NTP servers format.";
                        break;
                }
            }

            // Update visual feedback
            control.BackColor = isValid ? SystemColors.Window : Color.LightPink;
            
            if (!isValid && !string.IsNullOrEmpty(errorMessage))
            {
                var tooltip = new ToolTip();
                tooltip.SetToolTip(control, errorMessage);
            }
        }

        private void UpdateConfigurationProperty(string propertyName, object value)
        {
            var property = typeof(VCenterConfiguration).GetProperty(propertyName);
            property?.SetValue(_configuration, value);
        }

        private void BrowseForFile(TextBox textBox)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
                Title = "Select vcsa-deploy.exe"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = openFileDialog.FileName;
            }
        }

        private void SetupEventHandlers()
        {
            _powerShellService.OutputReceived += (s, output) => LogMessage(output, Color.White);
            _powerShellService.ErrorReceived += (s, error) => LogMessage(error, Color.Red);
            _powerShellService.ProcessExited += (s, exitCode) => 
            {
                if (InvokeRequired)
                {
                    Invoke(() => OnProcessExited(exitCode));
                }
                else
                {
                    OnProcessExited(exitCode);
                }
            };
        }

        private void LoadDefaultConfiguration()
        {
            try
            {
                var scriptDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                var parentDirectory = Directory.GetParent(scriptDirectory)?.Parent?.Parent?.Parent?.FullName;
                
                if (parentDirectory != null && File.Exists(Path.Combine(parentDirectory, "env.example")))
                {
                    var templateConfig = _configService.LoadFromTemplate(Path.Combine(parentDirectory, "env.example"));
                    LoadConfigurationIntoControls(templateConfig);
                }
                else
                {
                    LoadConfigurationIntoControls(_configuration);
                }
                
                UpdateStatus("Configuration loaded from template");
            }
            catch (Exception ex)
            {
                LogMessage($"Error loading default configuration: {ex.Message}", Color.Yellow);
                UpdateStatus("Ready");
            }
        }

        private void LoadConfigurationIntoControls(VCenterConfiguration config)
        {
            var allControls = new[] { _generalControls, _credentialsControls, _esxiControls, 
                                    _networkingControls, _storageControls, _deploymentControls }
                .SelectMany(dict => dict)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var property in typeof(VCenterConfiguration).GetProperties())
            {
                if (allControls.TryGetValue(property.Name, out var control))
                {
                    var value = property.GetValue(config);
                    
                    if (control is TextBox textBox)
                    {
                        textBox.Text = value?.ToString() ?? string.Empty;
                    }
                    else if (control is CheckBox checkBox)
                    {
                        checkBox.Checked = (bool)(value ?? false);
                    }
                    else if (control is ComboBox comboBox)
                    {
                        comboBox.SelectedItem = value?.ToString();
                    }
                }
            }
        }

        private void LogMessage(string message, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(() => LogMessage(message, color));
                return;
            }

            _logTextBox.SelectionStart = _logTextBox.TextLength;
            _logTextBox.SelectionLength = 0;
            _logTextBox.SelectionColor = color;
            _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            _logTextBox.ScrollToCaret();
        }

        private void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateStatus(message));
                return;
            }

            _statusLabel.Text = message;
        }

        private void ShowProgress(bool show)
        {
            if (InvokeRequired)
            {
                Invoke(() => ShowProgress(show));
                return;
            }

            _progressBar.Visible = show;
            if (show)
            {
                _progressBar.Style = ProgressBarStyle.Marquee;
            }
        }

        private void OnProcessExited(int exitCode)
        {
            ShowProgress(false);
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            var deployButton = FindControl("Start Deployment");
            var stopButton = FindControl("Stop Deployment");
            
            if (deployButton != null) deployButton.Enabled = true;
            if (stopButton != null) stopButton.Enabled = false;

            if (exitCode == 0)
            {
                LogMessage("Process completed successfully!", Color.Green);
                UpdateStatus("Deployment completed successfully");
            }
            else
            {
                LogMessage($"Process failed with exit code: {exitCode}", Color.Red);
                UpdateStatus("Deployment failed");
            }
        }

        private Control? FindControl(string text)
        {
            return FindControlRecursive(this, c => c is Button && c.Text == text);
        }

        private Control? FindControlRecursive(Control parent, Func<Control, bool> predicate)
        {
            foreach (Control control in parent.Controls)
            {
                if (predicate(control))
                    return control;

                var found = FindControlRecursive(control, predicate);
                if (found != null)
                    return found;
            }
            return null;
        }

        // Event handlers continue in the next part...
    }
}