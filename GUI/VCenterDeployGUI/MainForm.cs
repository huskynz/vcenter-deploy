using System.Diagnostics;

namespace VCenterDeployGUI
{
    public partial class MainForm : Form
    {
        private ConfigurationPanel? configPanel;
        private DeploymentPanel? deployPanel;
        private readonly EnvFileManager envManager;
        private readonly PowerShellExecutor psExecutor;

        public MainForm()
        {
            InitializeComponent();
            envManager = new EnvFileManager();
            psExecutor = new PowerShellExecutor();
            
            InitializePanels();
            SetStatus("Ready");
        }

        private void InitializePanels()
        {
            // Initialize Configuration Panel
            configPanel = new ConfigurationPanel(envManager);
            configPanel.Dock = DockStyle.Fill;
            configTab.Controls.Add(configPanel);

            // Initialize Deployment Panel
            deployPanel = new DeploymentPanel(psExecutor);
            deployPanel.Dock = DockStyle.Fill;
            deployTab.Controls.Add(deployPanel);
        }

        public void SetStatus(string message)
        {
            if (statusLabel.IsDisposed) return;
            
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action<string>(SetStatus), message);
                }
                catch (ObjectDisposedException)
                {
                    // Form is being disposed, ignore
                }
                catch (InvalidOperationException)
                {
                    // Control is not in a valid state, ignore
                }
                return;
            }
            
            statusLabel.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
            statusStrip.Refresh();
        }

        private void LoadEnvMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Environment Files (*.env)|*.env|All Files (*.*)|*.*",
                Title = "Load Environment File",
                DefaultExt = "env",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var envData = envManager.LoadEnvFile(dialog.FileName);
                    configPanel?.LoadConfiguration(envData);
                    SetStatus($"✅ Loaded configuration from {Path.GetFileName(dialog.FileName)}");
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access denied. Please check file permissions and try again.", "Access Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SetStatus("❌ Access denied loading configuration");
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("The selected file could not be found.", "File Not Found", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetStatus("❌ File not found");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetStatus("❌ Error loading configuration");
                }
            }
        }

        private void SaveEnvMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Environment Files (*.env)|*.env|All Files (*.*)|*.*",
                Title = "Save Environment File",
                DefaultExt = "env",
                FileName = ".env",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var envData = configPanel?.GetConfiguration() ?? new Dictionary<string, string>();
                    if (envData.Count == 0)
                    {
                        var result = MessageBox.Show("No configuration data to save. Continue anyway?", "Empty Configuration", 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.No) return;
                    }
                    
                    envManager.SaveEnvFile(dialog.FileName, envData);
                    SetStatus($"💾 Saved configuration to {Path.GetFileName(dialog.FileName)}");
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Access denied. Please check folder permissions and try again.", "Access Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    SetStatus("❌ Access denied saving configuration");
                }
                catch (DirectoryNotFoundException)
                {
                    MessageBox.Show("The directory path could not be found.", "Directory Not Found", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetStatus("❌ Directory not found");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetStatus("❌ Error saving configuration");
                }
            }
        }

        private void LoadExampleMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Look for env.example in parent directories
                var currentDir = Directory.GetCurrentDirectory();
                var parentDir = Directory.GetParent(currentDir)?.Parent?.FullName;
                var examplePath = Path.Combine(parentDir ?? currentDir, "env.example");
                
                if (File.Exists(examplePath))
                {
                    var envData = envManager.LoadEnvExample(examplePath);
                    configPanel?.LoadConfiguration(envData);
                    SetStatus("📋 Loaded template from env.example");
                    
                    MessageBox.Show("Template configuration loaded successfully!\n\nPlease review and update the fields with your specific values before deployment.", 
                        "Template Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    var result = MessageBox.Show("env.example file not found in the expected location.\n\nWould you like to browse for the file manually?", 
                        "File Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        using var dialog = new OpenFileDialog
                        {
                            Filter = "Example Files (*.example)|*.example|Environment Files (*.env)|*.env|All Files (*.*)|*.*",
                            Title = "Select env.example File",
                            FileName = "env.example"
                        };
                        
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            var envData = envManager.LoadEnvExample(dialog.FileName);
                            configPanel?.LoadConfiguration(envData);
                            SetStatus($"📋 Loaded template from {Path.GetFileName(dialog.FileName)}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading example file: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("❌ Error loading example");
            }
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            using var aboutForm = new AboutForm();
            aboutForm.ShowDialog(this);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (deployPanel?.IsDeploymentRunning == true)
            {
                var result = MessageBox.Show(
                    "A deployment is currently running. Closing the application may interrupt the process.\n\nAre you sure you want to exit?",
                    "⚠️ Deployment Running",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            
            try
            {
                // Clean up resources
                psExecutor?.Dispose();
            }
            catch
            {
                // Ignore cleanup errors during shutdown
            }

            base.OnFormClosing(e);
        }
    }
}