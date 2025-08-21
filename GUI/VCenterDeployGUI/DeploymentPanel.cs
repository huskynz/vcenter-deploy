using System.Text;

namespace VCenterDeployGUI
{
    public partial class DeploymentPanel : UserControl
    {
        private readonly PowerShellExecutor psExecutor;
        private TextBox? outputTextBox;
        private ProgressBar? progressBar;
        private Button? deployButton;
        private Button? validateButton;
        private Button? stopButton;
        private Label? statusLabel;
        private CheckBox? useCurrentDirCheckBox;
        private TextBox? workingDirTextBox;
        private Button? browseWorkingDirButton;

        public bool IsDeploymentRunning => psExecutor.IsRunning;

        public DeploymentPanel(PowerShellExecutor psExecutor)
        {
            this.psExecutor = psExecutor;
            InitializeComponent();
            CreateDeploymentInterface();
            SetupEventHandlers();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            
            Name = "DeploymentPanel";
            Size = new Size(1200, 650);
            BackColor = Color.FromArgb(248, 249, 250);
            Font = new Font("Segoe UI", 9F);
            
            ResumeLayout(false);
        }

        private void CreateDeploymentInterface()
        {
            Controls.Clear();

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1,
                Padding = new Padding(10)
            };

            // Row heights
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Controls
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Working directory
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Status
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Output
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Progress

            // Control panel
            var controlPanel = CreateControlPanel();
            mainPanel.Controls.Add(controlPanel, 0, 0);

            // Working directory panel
            var workingDirPanel = CreateWorkingDirectoryPanel();
            mainPanel.Controls.Add(workingDirPanel, 0, 1);

            // Status panel
            var statusPanel = CreateStatusPanel();
            mainPanel.Controls.Add(statusPanel, 0, 2);

            // Output panel
            var outputPanel = CreateOutputPanel();
            mainPanel.Controls.Add(outputPanel, 0, 3);

            // Progress panel
            var progressPanel = CreateProgressPanel();
            mainPanel.Controls.Add(progressPanel, 0, 4);

            Controls.Add(mainPanel);
        }

        private Panel CreateControlPanel()
        {
            var panel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 10)
            };

            // Header label
            var headerLabel = new Label
            {
                Text = "🚀 Deployment Actions",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            validateButton = new Button
            {
                Text = "🔍 Validate Environment",
                Size = new Size(160, 36),
                Location = new Point(0, 25),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            validateButton.FlatAppearance.BorderSize = 0;

            deployButton = new Button
            {
                Text = "🚀 Start Deployment",
                Size = new Size(150, 36),
                Location = new Point(170, 25),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Enabled = false
            };
            deployButton.FlatAppearance.BorderSize = 0;

            stopButton = new Button
            {
                Text = "⏹️ Stop",
                Size = new Size(80, 36),
                Location = new Point(330, 25),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false,
                Enabled = false
            };
            stopButton.FlatAppearance.BorderSize = 0;

            var clearButton = new Button
            {
                Text = "🗑️ Clear Output",
                Size = new Size(120, 36),
                Location = new Point(420, 25),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            clearButton.FlatAppearance.BorderSize = 0;

            clearButton.Click += (s, e) => outputTextBox?.Clear();

            panel.Controls.AddRange(new Control[] { headerLabel, validateButton, deployButton, stopButton, clearButton });
            return panel;
        }

        private Panel CreateWorkingDirectoryPanel()
        {
            var panel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 8)
            };

            var label = new Label
            {
                Text = "📁 Working Directory:",
                Size = new Size(140, 22),
                Location = new Point(0, 12),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64)
            };

            useCurrentDirCheckBox = new CheckBox
            {
                Text = "Use current directory",
                Size = new Size(160, 22),
                Location = new Point(145, 12),
                Checked = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(52, 58, 64),
                BackColor = Color.Transparent
            };

            workingDirTextBox = new TextBox
            {
                Size = new Size(320, 26),
                Location = new Point(310, 10),
                Text = GetDefaultWorkingDirectory(),
                Enabled = false,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(233, 236, 239),
                BorderStyle = BorderStyle.FixedSingle
            };

            browseWorkingDirButton = new Button
            {
                Text = "Browse...",
                Size = new Size(80, 26),
                Location = new Point(640, 10),
                Enabled = false,
                Font = new Font("Segoe UI", 8.5F),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            browseWorkingDirButton.FlatAppearance.BorderSize = 0;

            useCurrentDirCheckBox.CheckedChanged += (s, e) =>
            {
                var useCurrentDir = useCurrentDirCheckBox.Checked;
                workingDirTextBox.Enabled = !useCurrentDir;
                browseWorkingDirButton.Enabled = !useCurrentDir;
                
                if (useCurrentDir)
                {
                    workingDirTextBox.Text = GetDefaultWorkingDirectory();
                }
                else
                {
                    workingDirTextBox.BackColor = Color.White;
                }
            };

            browseWorkingDirButton.Click += (s, e) =>
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "Select working directory containing PowerShell scripts",
                    UseDescriptionForTitle = true
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    workingDirTextBox.Text = dialog.SelectedPath;
                }
            };

            panel.Controls.AddRange(new Control[] { label, useCurrentDirCheckBox, workingDirTextBox, browseWorkingDirButton });
            return panel;
        }

        private Panel CreateStatusPanel()
        {
            var panel = new Panel
            {
                Height = 35,
                Dock = DockStyle.Top,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 5, 0, 5)
            };

            statusLabel = new Label
            {
                Text = "💡 Ready to validate or deploy",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 167, 69),
                BackColor = Color.FromArgb(212, 237, 218),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10, 8, 10, 8)
            };

            panel.Controls.Add(statusLabel);
            return panel;
        }

        private Panel CreateOutputPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            var groupBox = new GroupBox
            {
                Text = "📄 Output Console",
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                BackColor = Color.Transparent
            };

            outputTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.5F),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(204, 204, 204),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Text = "Ready to execute PowerShell scripts...\r\n"
            };

            groupBox.Controls.Add(outputTextBox);
            panel.Controls.Add(groupBox);
            return panel;
        }

        private Panel CreateProgressPanel()
        {
            var panel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Bottom,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 8)
            };

            progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Continuous,
                Height = 24,
                BackColor = Color.FromArgb(233, 236, 239),
                ForeColor = Color.FromArgb(0, 123, 255)
            };

            panel.Controls.Add(progressBar);
            return panel;
        }

        private void SetupEventHandlers()
        {
            psExecutor.OutputReceived += OnOutputReceived;
            psExecutor.ErrorReceived += OnErrorReceived;
            psExecutor.ExecutionCompleted += OnExecutionCompleted;

            if (validateButton != null)
                validateButton.Click += OnValidateClick;

            if (deployButton != null)
                deployButton.Click += OnDeployClick;

            if (stopButton != null)
                stopButton.Click += OnStopClick;
        }

        private async void OnValidateClick(object? sender, EventArgs e)
        {
            var workingDir = GetWorkingDirectory();
            if (string.IsNullOrWhiteSpace(workingDir) || !Directory.Exists(workingDir))
            {
                MessageBox.Show("Please select a valid working directory.", "Invalid Directory", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetControlsEnabled(false);
            SetStatus("Validating environment...", Color.Blue);
            progressBar?.SetProgressBarStepped();

            try
            {
                var envFilePath = Path.Combine(workingDir, ".env");
                var isValid = await psExecutor.ValidateEnvironmentAsync(workingDir, envFilePath);

                if (isValid)
                {
                    SetStatus("Environment validation successful", Color.Green);
                    deployButton!.Enabled = true;
                }
                else
                {
                    SetStatus("Environment validation failed", Color.Red);
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"Validation error: {ex.Message}", Color.Red);
                SetStatus("Validation error", Color.Red);
            }
            finally
            {
                progressBar?.SetProgressBarContinuous();
                SetControlsEnabled(true);
            }
        }

        private async void OnDeployClick(object? sender, EventArgs e)
        {
            var workingDir = GetWorkingDirectory();
            var envFilePath = Path.Combine(workingDir, ".env");

            if (!File.Exists(envFilePath))
            {
                var result = MessageBox.Show(
                    "No .env file found in the working directory. Would you like to save the current configuration first?",
                    "Missing Configuration",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Get the main form and access the configuration
                    var mainForm = FindForm() as MainForm;
                    // This would need to be implemented to get the configuration from the other tab
                    MessageBox.Show("Please go to the Configuration tab and save your configuration first.", 
                        "Save Configuration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                {
                    return;
                }
            }

            var confirmResult = MessageBox.Show(
                "This will start the vCenter deployment process. This operation cannot be undone and may take a significant amount of time. Are you sure you want to continue?",
                "Confirm Deployment",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmResult != DialogResult.Yes)
                return;

            SetControlsEnabled(false);
            SetStatus("Starting deployment...", Color.Blue);
            progressBar?.SetProgressBarStepped();

            try
            {
                var success = await psExecutor.RunDeploymentAsync(workingDir, envFilePath);
                
                if (success)
                {
                    SetStatus("Deployment completed successfully", Color.Green);
                }
                else
                {
                    SetStatus("Deployment failed", Color.Red);
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"Deployment error: {ex.Message}", Color.Red);
                SetStatus("Deployment error", Color.Red);
            }
            finally
            {
                progressBar?.SetProgressBarContinuous();
                SetControlsEnabled(true);
            }
        }

        private void OnStopClick(object? sender, EventArgs e)
        {
            psExecutor.StopExecution();
            SetStatus("Stopping execution...", Color.Orange);
        }

        private void OnOutputReceived(object? sender, string message)
        {
            AppendOutput(message, Color.LimeGreen);
        }

        private void OnErrorReceived(object? sender, string message)
        {
            AppendOutput($"ERROR: {message}", Color.Red);
        }

        private void OnExecutionCompleted(object? sender, bool success)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object?, bool>(OnExecutionCompleted), sender, success);
                return;
            }

            progressBar?.SetProgressBarContinuous();
            SetControlsEnabled(true);

            var statusText = success ? "Execution completed successfully" : "Execution completed with errors";
            var statusColor = success ? Color.Green : Color.Red;
            SetStatus(statusText, statusColor);
        }

        private void AppendOutput(string message, Color color)
        {
            if (outputTextBox?.IsDisposed == true) return;

            if (InvokeRequired)
            {
                Invoke(new Action<string, Color>(AppendOutput), message, color);
                return;
            }

            if (outputTextBox != null)
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                var formattedMessage = $"[{timestamp}] {message}";
                
                outputTextBox.AppendText(formattedMessage + Environment.NewLine);
                outputTextBox.SelectionStart = outputTextBox.Text.Length;
                outputTextBox.ScrollToCaret();
            }
        }

        private void SetStatus(string message, Color color)
        {
            if (statusLabel?.IsDisposed == true) return;

            if (InvokeRequired)
            {
                Invoke(new Action<string, Color>(SetStatus), message, color);
                return;
            }

            if (statusLabel != null)
            {
                statusLabel.Text = message;
                statusLabel.ForeColor = color;
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<bool>(SetControlsEnabled), enabled);
                return;
            }

            if (validateButton != null) validateButton.Enabled = enabled;
            if (deployButton != null) deployButton.Enabled = enabled;
            if (stopButton != null) stopButton.Enabled = !enabled && psExecutor.IsRunning;
        }

        private string GetWorkingDirectory()
        {
            if (useCurrentDirCheckBox?.Checked == true)
            {
                return GetDefaultWorkingDirectory();
            }

            return workingDirTextBox?.Text ?? GetDefaultWorkingDirectory();
        }

        private static string GetDefaultWorkingDirectory()
        {
            // Try to find the project directory by looking for setup.ps1
            var currentDir = Directory.GetCurrentDirectory();
            var parentDir = Directory.GetParent(currentDir);
            
            // Look in current directory first
            if (File.Exists(Path.Combine(currentDir, "setup.ps1")))
                return currentDir;
            
            // Look in parent directories
            while (parentDir != null)
            {
                if (File.Exists(Path.Combine(parentDir.FullName, "setup.ps1")))
                    return parentDir.FullName;
                
                parentDir = parentDir.Parent;
            }

            return currentDir;
        }
    }

    public static class ProgressBarExtensions
    {
        public static void SetProgressBarStepped(this ProgressBar progressBar)
        {
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.MarqueeAnimationSpeed = 30;
        }

        public static void SetProgressBarContinuous(this ProgressBar progressBar)
        {
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.MarqueeAnimationSpeed = 0;
            progressBar.Value = 0;
        }
    }
}