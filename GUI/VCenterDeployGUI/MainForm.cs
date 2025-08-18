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
                Invoke(new Action<string>(SetStatus), message);
                return;
            }
            
            statusLabel.Text = message;
            statusStrip.Refresh();
        }

        private void LoadEnvMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Environment Files (*.env)|*.env|All Files (*.*)|*.*",
                Title = "Load Environment File",
                DefaultExt = "env"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var envData = envManager.LoadEnvFile(dialog.FileName);
                    configPanel?.LoadConfiguration(envData);
                    SetStatus($"Loaded configuration from {Path.GetFileName(dialog.FileName)}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetStatus("Error loading configuration");
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
                FileName = ".env"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var envData = configPanel?.GetConfiguration() ?? new Dictionary<string, string>();
                    envManager.SaveEnvFile(dialog.FileName, envData);
                    SetStatus($"Saved configuration to {Path.GetFileName(dialog.FileName)}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    SetStatus("Error saving configuration");
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
                    SetStatus("Loaded template from env.example");
                }
                else
                {
                    MessageBox.Show("env.example file not found. Please ensure it exists in the project directory.", 
                        "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading example file: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetStatus("Error loading example");
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
                    "A deployment is currently running. Are you sure you want to exit?",
                    "Deployment Running",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnFormClosing(e);
        }
    }
}