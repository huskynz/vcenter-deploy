using System.ComponentModel;
using System.Text.RegularExpressions;

namespace VCenterDeployGUI
{
    public partial class ConfigurationPanel : UserControl
    {
        private readonly EnvFileManager envManager;
        private readonly Dictionary<string, Control> fieldControls = new();
        private readonly Dictionary<string, Label> errorLabels = new();
        private TabControl? tabControl;

        public ConfigurationPanel(EnvFileManager envManager)
        {
            this.envManager = envManager;
            InitializeComponent();
            CreateConfigurationInterface();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            
            Name = "ConfigurationPanel";
            Size = new Size(1200, 650);
            BackColor = Color.FromArgb(248, 249, 250);
            Font = new Font("Segoe UI", 9F);
            
            ResumeLayout(false);
        }

        private void CreateConfigurationInterface()
        {
            Controls.Clear();
            
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                ItemSize = new Size(120, 32),
                Padding = new Point(8, 6),
                Appearance = TabAppearance.Normal
            };

            var configFields = envManager.GetConfigurationFields();
            
            // Group fields into logical tabs with icons
            var tabGroups = new Dictionary<string, List<string>>
            {
                ["⚙️ General"] = new() { "VM_NAME", "VCSA_CLI_PATH", "VCSA_HOST" },
                ["🔐 Credentials"] = new() { "VC_PASSWORD", "VCSA_ROOT_PASSWORD", "ESXI_HOST", "ESXI_USER", "ESXI_PASSWORD" },
                ["🌐 Network"] = new() { "IP_ADDRESS", "DNS_SERVERS", "NETWORK_PREFIX", "GATEWAY", "DEPLOYMENT_NETWORK" },
                ["🚀 Deployment"] = new() { "DATASTORE", "THIN_DISK_MODE", "DEPLOYMENT_OPTION", "NTP_SERVERS" },
                ["🔧 Advanced"] = new() { "SSO_DOMAIN", "CEIP_SETTINGS" }
            };

            foreach (var tabGroup in tabGroups)
            {
                var tabPage = new TabPage(tabGroup.Key)
                {
                    UseVisualStyleBackColor = true,
                    Padding = new Padding(20),
                    BackColor = Color.FromArgb(252, 253, 254),
                    Font = new Font("Segoe UI", 9F)
                };

                var scrollPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    BackColor = Color.Transparent
                };

                var headerLabel = new Label
                {
                    Text = tabGroup.Key.Replace("🔧", "").Replace("🚀", "").Replace("🌐", "").Replace("🔐", "").Replace("⚙️", "").Trim() + " Configuration",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(33, 37, 41),
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 20),
                    Dock = DockStyle.Top
                };
                
                var innerPanel = new TableLayoutPanel
                {
                    ColumnCount = 2,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Dock = DockStyle.Top,
                    BackColor = Color.Transparent,
                    Padding = new Padding(0, 40, 0, 20)
                };

                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                innerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                var row = 0;
                foreach (var fieldKey in tabGroup.Value)
                {
                    if (configFields.ContainsKey(fieldKey))
                    {
                        CreateFieldControl(innerPanel, fieldKey, configFields[fieldKey], row);
                        row++;
                    }
                }

                scrollPanel.Controls.Add(innerPanel);
                scrollPanel.Controls.Add(headerLabel);
                tabPage.Controls.Add(scrollPanel);
                tabControl.TabPages.Add(tabPage);
            }

            Controls.Add(tabControl);
        }

        private void CreateFieldControl(TableLayoutPanel parent, string fieldKey, ConfigField field, int row)
        {
            parent.RowCount = Math.Max(parent.RowCount, row + 2);
            parent.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Label with improved styling
            var label = new Label
            {
                Text = field.DisplayName + (field.IsRequired ? " *" : ""),
                Font = new Font("Segoe UI", 9.5F, field.IsRequired ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = field.IsRequired ? Color.FromArgb(220, 53, 69) : Color.FromArgb(52, 58, 64),
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 12, 15, 6),
                TextAlign = ContentAlignment.MiddleLeft
            };

            parent.Controls.Add(label, 0, row);

            // Input control with modern styling
            Control inputControl;

            if (field.IsBoolean)
            {
                var checkBox = new CheckBox
                {
                    Text = field.Description,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                    Margin = new Padding(0, 8, 0, 8),
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(52, 58, 64),
                    BackColor = Color.Transparent
                };
                
                if (bool.TryParse(field.DefaultValue, out bool defaultBool))
                    checkBox.Checked = defaultBool;

                checkBox.CheckedChanged += (s, e) => ValidateField(fieldKey);
                inputControl = checkBox;
            }
            else if (field.IsDropdown && field.DropdownOptions != null)
            {
                var comboBox = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                    Margin = new Padding(0, 8, 0, 8),
                    Font = new Font("Segoe UI", 9F),
                    Height = 28,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(52, 58, 64)
                };

                comboBox.Items.AddRange(field.DropdownOptions);
                
                if (!string.IsNullOrEmpty(field.DefaultValue))
                    comboBox.Text = field.DefaultValue;

                comboBox.SelectedIndexChanged += (s, e) => ValidateField(fieldKey);
                inputControl = comboBox;
            }
            else
            {
                var textBox = new TextBox
                {
                    UseSystemPasswordChar = field.IsPassword,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                    Margin = new Padding(0, 8, 0, 8),
                    Text = field.DefaultValue,
                    Font = new Font("Segoe UI", 9F),
                    Height = 26,
                    BackColor = Color.White,
                    ForeColor = Color.FromArgb(52, 58, 64),
                    BorderStyle = BorderStyle.FixedSingle
                };

                if (field.IsFilePath)
                {
                    var panel = new Panel
                    {
                        Height = 30,
                        Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                        Margin = new Padding(0, 8, 0, 8),
                        BackColor = Color.Transparent
                    };

                    textBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                    textBox.Width = panel.Width - 85;

                    var browseButton = new Button
                    {
                        Text = "Browse...",
                        Width = 80,
                        Height = 26,
                        Anchor = AnchorStyles.Top | AnchorStyles.Right,
                        Font = new Font("Segoe UI", 8.5F),
                        BackColor = Color.FromArgb(0, 123, 255),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        UseVisualStyleBackColor = false
                    };
                    browseButton.FlatAppearance.BorderSize = 0;
                    browseButton.Left = panel.Width - browseButton.Width;

                    browseButton.Click += (s, e) => BrowseForFile(textBox, field);

                    panel.Controls.Add(textBox);
                    panel.Controls.Add(browseButton);
                    
                    panel.Resize += (s, e) =>
                    {
                        textBox.Width = panel.Width - 85;
                        browseButton.Left = panel.Width - browseButton.Width;
                    };

                    inputControl = panel;
                }
                else
                {
                    inputControl = textBox;
                }

                textBox.TextChanged += (s, e) => ValidateField(fieldKey);
                textBox.Leave += (s, e) => ValidateField(fieldKey);
            }

            // Add tooltip with better styling
            var toolTip = new ToolTip();
            toolTip.SetToolTip(inputControl, field.Description);

            parent.Controls.Add(inputControl, 1, row);
            fieldControls[fieldKey] = inputControl;

            // Error label with improved styling
            var errorLabel = new Label
            {
                ForeColor = Color.FromArgb(220, 53, 69),
                AutoSize = true,
                Visible = false,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Margin = new Padding(0, 2, 0, 12),
                BackColor = Color.Transparent
            };

            parent.Controls.Add(errorLabel, 1, row + 1);
            errorLabels[fieldKey] = errorLabel;
        }

        private void BrowseForFile(TextBox textBox, ConfigField field)
        {
            using var dialog = new OpenFileDialog
            {
                Title = $"Select {field.DisplayName}",
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = dialog.FileName;
            }
        }

        private void ValidateField(string fieldKey)
        {
            var configFields = envManager.GetConfigurationFields();
            if (!configFields.ContainsKey(fieldKey) || !fieldControls.ContainsKey(fieldKey))
                return;

            var field = configFields[fieldKey];
            var control = fieldControls[fieldKey];
            var errorLabel = errorLabels[fieldKey];
            
            var value = GetControlValue(control);
            var isValid = true;
            var errorMessage = "";

            // Required field validation
            if (field.IsRequired && string.IsNullOrWhiteSpace(value))
            {
                isValid = false;
                errorMessage = "This field is required.";
            }
            // Pattern validation
            else if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(field.ValidationPattern))
            {
                if (!Regex.IsMatch(value, field.ValidationPattern))
                {
                    isValid = false;
                    errorMessage = GetValidationErrorMessage(fieldKey);
                }
            }

            // Visual feedback with modern styling
            if (control is TextBox textBox)
            {
                textBox.BackColor = isValid ? Color.White : Color.FromArgb(255, 235, 238);
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is Panel panel && panel.Controls[0] is TextBox panelTextBox)
            {
                panelTextBox.BackColor = isValid ? Color.White : Color.FromArgb(255, 235, 238);
                panelTextBox.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.BackColor = isValid ? Color.White : Color.FromArgb(255, 235, 238);
            }

            errorLabel.Text = errorMessage;
            errorLabel.Visible = !isValid;
            
            // Add success indicator for valid required fields
            if (isValid && field.IsRequired && !string.IsNullOrWhiteSpace(value))
            {
                if (control is TextBox successTextBox)
                {
                    successTextBox.BackColor = Color.FromArgb(240, 253, 244);
                }
                else if (control is Panel successPanel && successPanel.Controls[0] is TextBox successPanelTextBox)
                {
                    successPanelTextBox.BackColor = Color.FromArgb(240, 253, 244);
                }
                else if (control is ComboBox successComboBox)
                {
                    successComboBox.BackColor = Color.FromArgb(240, 253, 244);
                }
            }
        }

        private string GetValidationErrorMessage(string fieldKey)
        {
            return fieldKey switch
            {
                "IP_ADDRESS" => "Please enter a valid IP address (e.g., 192.168.1.100)",
                "GATEWAY" => "Please enter a valid gateway IP address",
                "NETWORK_PREFIX" => "Please enter a valid network prefix (1-32)",
                "VM_NAME" => "VM name can only contain letters, numbers, hyphens, and underscores",
                "VCSA_HOST" => "Please enter a valid hostname or FQDN",
                "ESXI_HOST" => "Please enter a valid hostname or IP address",
                _ => "Invalid format"
            };
        }

        private string GetControlValue(Control control)
        {
            return control switch
            {
                TextBox textBox => textBox.Text,
                CheckBox checkBox => checkBox.Checked.ToString().ToLower(),
                ComboBox comboBox => comboBox.Text,
                Panel panel when panel.Controls[0] is TextBox panelTextBox => panelTextBox.Text,
                _ => ""
            };
        }

        private void SetControlValue(Control control, string value)
        {
            switch (control)
            {
                case TextBox textBox:
                    textBox.Text = value;
                    break;
                case CheckBox checkBox:
                    checkBox.Checked = bool.TryParse(value, out bool boolValue) && boolValue;
                    break;
                case ComboBox comboBox:
                    comboBox.Text = value;
                    break;
                case Panel panel when panel.Controls[0] is TextBox panelTextBox:
                    panelTextBox.Text = value;
                    break;
            }
        }

        public void LoadConfiguration(Dictionary<string, string> envData)
        {
            foreach (var kvp in envData)
            {
                if (fieldControls.ContainsKey(kvp.Key))
                {
                    SetControlValue(fieldControls[kvp.Key], kvp.Value);
                    ValidateField(kvp.Key);
                }
            }
        }

        public Dictionary<string, string> GetConfiguration()
        {
            var config = new Dictionary<string, string>();
            
            foreach (var kvp in fieldControls)
            {
                var value = GetControlValue(kvp.Value);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    config[kvp.Key] = value;
                }
            }

            return config;
        }

        public bool ValidateAllFields()
        {
            var isAllValid = true;
            
            foreach (var fieldKey in fieldControls.Keys)
            {
                ValidateField(fieldKey);
                if (errorLabels[fieldKey].Visible)
                {
                    isAllValid = false;
                }
            }

            return isAllValid;
        }
    }
}