namespace VCenterDeployGUI
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Form properties
            Text = "About vCenter Deployment Tool";
            Size = new Size(500, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(248, 249, 250);
            Font = new Font("Segoe UI", 9F);

            // Main panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(30),
                BackColor = Color.Transparent
            };

            // Logo/Title with icon
            var titleLabel = new Label
            {
                Text = "🖥️ vCenter Deployment Tool",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(13, 110, 253),
                Height = 50
            };

            // Version info with styling
            var versionLabel = new Label
            {
                Text = "Version 1.0.0\n🚀 GUI Wrapper for vCenter Automation",
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(52, 58, 64),
                Height = 60
            };

            // Description with better formatting
            var descriptionLabel = new Label
            {
                Text = "A modern Windows Forms GUI wrapper that provides an intuitive interface for VMware vCenter Server Appliance deployment automation.\n\n✨ Key Features:\n" +
                       "• 🎯 Interactive configuration management with real-time validation\n" +
                       "• 🔐 Secure credential handling and .env file management\n" +
                       "• ⚡ PowerShell integration with live output streaming\n" +
                       "• 📊 Professional progress tracking and status indicators\n" +
                       "• 🎨 Modern Windows interface with tabbed organization\n" +
                       "• 🛡️ Built-in error handling and recovery suggestions",
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.TopLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            // Author/Copyright with enhanced styling
            var copyrightLabel = new Label
            {
                Text = "🏗️ Created by HuskyNZ\n© 2024 - Built with Windows Forms and .NET 8.0\n💻 Open Source - Available on GitHub",
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(108, 117, 125),
                Height = 60
            };

            // Modern close button
            var closeButton = new Button
            {
                Text = "✓ Close",
                Size = new Size(100, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.OK,
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(13, 110, 253),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            closeButton.FlatAppearance.BorderSize = 0;

            var buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 10, 20, 10)
            };
            
            closeButton.Location = new Point(buttonPanel.Width - closeButton.Width - 30, 10);
            buttonPanel.Controls.Add(closeButton);

            // Set row styles for better spacing
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Title
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Version
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Description
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Copyright

            // Add controls with proper spacing
            mainPanel.Controls.Add(titleLabel, 0, 0);
            mainPanel.Controls.Add(versionLabel, 0, 1);
            mainPanel.Controls.Add(descriptionLabel, 0, 2);
            mainPanel.Controls.Add(copyrightLabel, 0, 3);

            Controls.Add(mainPanel);
            Controls.Add(buttonPanel);

            AcceptButton = closeButton;
            CancelButton = closeButton;

            ResumeLayout(false);
        }
    }
}