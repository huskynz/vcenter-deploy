using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using VCenterDeployGUI.Models;
using VCenterDeployGUI.Services;

namespace VCenterDeployGUI
{
    public partial class MainForm : Form
    {
        private readonly VCenterConfiguration _configuration;
        private readonly ConfigurationService _configService;
        private readonly PowerShellService _powerShellService;
        private CancellationTokenSource? _cancellationTokenSource;

        private TabControl _tabControl;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private MenuStrip _menuStrip;
        private ProgressBar _progressBar;
        private RichTextBox _logTextBox;

        // Control collections for each tab
        private readonly Dictionary<string, Control> _generalControls = new();
        private readonly Dictionary<string, Control> _credentialsControls = new();
        private readonly Dictionary<string, Control> _esxiControls = new();
        private readonly Dictionary<string, Control> _networkingControls = new();
        private readonly Dictionary<string, Control> _storageControls = new();
        private readonly Dictionary<string, Control> _deploymentControls = new();

        public MainForm()
        {
            _configuration = new VCenterConfiguration();
            _configService = new ConfigurationService();
            _powerShellService = new PowerShellService();

            InitializeComponent();
            SetupEventHandlers();
            LoadDefaultConfiguration();
        }

        private void InitializeComponent()
        {
            Text = "vCenter Deployment GUI";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(600, 400);

            CreateMenuStrip();
            CreateTabControl();
            CreateStatusStrip();
            CreateProgressAndLogging();

            // Apply professional styling
            BackColor = SystemColors.Control;
            Font = new Font("Segoe UI", 9F);
        }

        private void CreateMenuStrip()
        {
            _menuStrip = new MenuStrip();

            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add("&New Configuration", null, OnNewConfiguration);
            fileMenu.DropDownItems.Add("&Load Configuration...", null, OnLoadConfiguration);
            fileMenu.DropDownItems.Add("&Save Configuration...", null, OnSaveConfiguration);
            fileMenu.DropDownItems.Add("Load from &Template...", null, OnLoadTemplate);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("E&xit", null, OnExit);

            var deployMenu = new ToolStripMenuItem("&Deploy");
            deployMenu.DropDownItems.Add("&Validate Configuration", null, OnValidateConfiguration);
            deployMenu.DropDownItems.Add("&Start Deployment", null, OnStartDeployment);
            deployMenu.DropDownItems.Add("&Stop Deployment", null, OnStopDeployment);

            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add("&About", null, OnAbout);

            _menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, deployMenu, helpMenu });
            Controls.Add(_menuStrip);
            MainMenuStrip = _menuStrip;
        }

        private void CreateTabControl()
        {
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(0, _menuStrip.Height),
                Size = new Size(ClientSize.Width, ClientSize.Height - _menuStrip.Height - 100)
            };

            CreateGeneralTab();
            CreateCredentialsTab();
            CreateEsxiTab();
            CreateNetworkingTab();
            CreateStorageTab();
            CreateDeploymentTab();
            CreateLoggingTab();

            Controls.Add(_tabControl);
        }

        private void CreateGeneralTab()
        {
            var tabPage = new TabPage("General");
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10)
            };

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            AddConfigField(panel, "VM Name:", "VmName", _generalControls, isRequired: true);
            AddConfigField(panel, "VCSA CLI Path:", "VcsaCliPath", _generalControls, isRequired: true, isPath: true);
            AddConfigField(panel, "VCSA Host:", "VcsaHost", _generalControls, isRequired: true);
            AddCheckBoxField(panel, "CEIP Settings:", "CeipSettings", _generalControls);

            tabPage.Controls.Add(panel);
            _tabControl.TabPages.Add(tabPage);
        }

        private void CreateCredentialsTab()
        {
            var tabPage = new TabPage("Credentials");
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10)
            };

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            AddConfigField(panel, "VC Password:", "VcPassword", _credentialsControls, isRequired: true, isPassword: true);
            AddConfigField(panel, "VCSA Root Password:", "VcsaRootPassword", _credentialsControls, isRequired: true, isPassword: true);

            tabPage.Controls.Add(panel);
            _tabControl.TabPages.Add(tabPage);
        }

        private void CreateEsxiTab()
        {
            var tabPage = new TabPage("ESXi");
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10)
            };

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            AddConfigField(panel, "ESXi Host:", "EsxiHost", _esxiControls, isRequired: true);
            AddConfigField(panel, "ESXi User:", "EsxiUser", _esxiControls, isRequired: true);
            AddConfigField(panel, "ESXi Password:", "EsxiPassword", _esxiControls, isRequired: true, isPassword: true);

            tabPage.Controls.Add(panel);
            _tabControl.TabPages.Add(tabPage);
        }

        private void CreateNetworkingTab()
        {
            var tabPage = new TabPage("Networking");
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10)
            };

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            AddConfigField(panel, "IP Address:", "IpAddress", _networkingControls, isRequired: true);
            AddConfigField(panel, "DNS Servers:", "DnsServers", _networkingControls, isRequired: true);
            AddConfigField(panel, "Network Prefix:", "NetworkPrefix", _networkingControls, isRequired: true);
            AddConfigField(panel, "Gateway:", "Gateway", _networkingControls, isRequired: true);
            AddConfigField(panel, "NTP Servers:", "NtpServers", _networkingControls);
            AddConfigField(panel, "Deployment Network:", "DeploymentNetwork", _networkingControls, isRequired: true);
            AddConfigField(panel, "SSO Domain:", "SsoDomain", _networkingControls);

            tabPage.Controls.Add(panel);
            _tabControl.TabPages.Add(tabPage);
        }

        private void CreateStorageTab()
        {
            var tabPage = new TabPage("Storage");
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10)
            };

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            AddConfigField(panel, "Datastore:", "Datastore", _storageControls, isRequired: true);
            AddCheckBoxField(panel, "Thin Disk Mode:", "ThinDiskMode", _storageControls);

            tabPage.Controls.Add(panel);
            _tabControl.TabPages.Add(tabPage);
        }

        private void CreateDeploymentTab()
        {
            var tabPage = new TabPage("Deployment");
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(10)
            };

            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            AddDeploymentOptionField(panel, "Deployment Option:", "DeploymentOption", _deploymentControls);

            // Add deployment buttons
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.LeftToRight,
                Height = 40,
                Padding = new Padding(10)
            };

            var validateButton = new Button
            {
                Text = "Validate Configuration",
                Size = new Size(150, 30),
                UseVisualStyleBackColor = true
            };
            validateButton.Click += OnValidateConfiguration;

            var deployButton = new Button
            {
                Text = "Start Deployment",
                Size = new Size(120, 30),
                UseVisualStyleBackColor = true,
                BackColor = Color.Green,
                ForeColor = Color.White
            };
            deployButton.Click += OnStartDeployment;

            var stopButton = new Button
            {
                Text = "Stop Deployment",
                Size = new Size(120, 30),
                UseVisualStyleBackColor = true,
                BackColor = Color.Red,
                ForeColor = Color.White,
                Enabled = false
            };
            stopButton.Click += OnStopDeployment;

            buttonPanel.Controls.AddRange(new Control[] { validateButton, deployButton, stopButton });
            tabPage.Controls.Add(buttonPanel);
            tabPage.Controls.Add(panel);
            _tabControl.TabPages.Add(tabPage);
        }

        private void CreateLoggingTab()
        {
            var tabPage = new TabPage("Logs");
            
            _logTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9F),
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.LeftToRight,
                Height = 40,
                Padding = new Padding(10)
            };

            var clearButton = new Button
            {
                Text = "Clear Logs",
                Size = new Size(100, 30),
                UseVisualStyleBackColor = true
            };
            clearButton.Click += (s, e) => _logTextBox.Clear();

            var saveLogsButton = new Button
            {
                Text = "Save Logs",
                Size = new Size(100, 30),
                UseVisualStyleBackColor = true
            };
            saveLogsButton.Click += OnSaveLogs;

            buttonPanel.Controls.AddRange(new Control[] { clearButton, saveLogsButton });
            tabPage.Controls.Add(buttonPanel);
            tabPage.Controls.Add(_logTextBox);
            _tabControl.TabPages.Add(tabPage);
        }

        private void CreateProgressAndLogging()
        {
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10)
            };

            _progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 20,
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };

            bottomPanel.Controls.Add(_progressBar);
            Controls.Add(bottomPanel);
        }

        private void CreateStatusStrip()
        {
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Ready")
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _statusStrip.Items.Add(_statusLabel);
            Controls.Add(_statusStrip);
        }

        // This is a partial implementation - the remaining methods will be implemented in the next part
        // due to character limits...
    }
}