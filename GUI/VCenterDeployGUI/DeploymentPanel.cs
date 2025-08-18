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
            Size = new Size(800, 600);
            BackColor = SystemColors.Control;
            
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
                Height = 40,
                Dock = DockStyle.Top
            };

            validateButton = new Button
            {
                Text = "Validate Environment",
                Size = new Size(140, 30),
                Location = new Point(0, 5),
                UseVisualStyleBackColor = true
            };

            deployButton = new Button
            {
                Text = "Start Deployment",
                Size = new Size(120, 30),
                Location = new Point(150, 5),
                UseVisualStyleBackColor = true,
                Enabled = false
            };

            stopButton = new Button
            {
                Text = "Stop",
                Size = new Size(60, 30),
                Location = new Point(280, 5),
                UseVisualStyleBackColor = true,
                Enabled = false
            };

            var clearButton = new Button
            {
                Text = "Clear Output",
                Size = new Size(90, 30),
                Location = new Point(350, 5),
                UseVisualStyleBackColor = true
            };

            clearButton.Click += (s, e) => outputTextBox?.Clear();

            panel.Controls.AddRange(new Control[] { validateButton, deployButton, stopButton, clearButton });
            return panel;
        }

        private Panel CreateWorkingDirectoryPanel()
        {
            var panel = new Panel
            {
                Height = 35,
                Dock = DockStyle.Top
            };

            var label = new Label
            {
                Text = "Working Directory:",
                Size = new Size(120, 20),
                Location = new Point(0, 8),
                TextAlign = ContentAlignment.MiddleLeft
            };

            useCurrentDirCheckBox = new CheckBox
            {
                Text = "Use current directory",
                Size = new Size(150, 20),
                Location = new Point(125, 8),
                Checked = true
            };

            workingDirTextBox = new TextBox
            {
                Size = new Size(300, 23),
                Location = new Point(280, 6),
                Text = GetDefaultWorkingDirectory(),
                Enabled = false
            };

            browseWorkingDirButton = new Button
            {
                Text = "Browse...",
                Size = new Size(70, 23),
                Location = new Point(590, 6),
                Enabled = false
            };

            useCurrentDirCheckBox.CheckedChanged += (s, e) =>
            {
                var useCurrentDir = useCurrentDirCheckBox.Checked;
                workingDirTextBox.Enabled = !useCurrentDir;
                browseWorkingDirButton.Enabled = !useCurrentDir;
                
                if (useCurrentDir)
                {
                    workingDirTextBox.Text = GetDefaultWorkingDirectory();
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
                Height = 25,
                Dock = DockStyle.Top
            };

            statusLabel = new Label
            {
                Text = "Ready to validate or deploy",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold)
            };

            panel.Controls.Add(statusLabel);
            return panel;
        }

        private Panel CreateOutputPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill
            };

            var groupBox = new GroupBox
            {
                Text = "Output",
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            outputTextBox = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
                ReadOnly = true
            };

            groupBox.Controls.Add(outputTextBox);
            panel.Controls.Add(groupBox);
            return panel;
        }

        private Panel CreateProgressPanel()
        {
            var panel = new Panel
            {
                Height = 30,
                Dock = DockStyle.Bottom
            };

            progressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Continuous,
                Margin = new Padding(0, 5, 0, 0)
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