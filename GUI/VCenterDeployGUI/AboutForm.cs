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
            Size = new Size(400, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;

            // Main panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(20)
            };

            // Logo/Title
            var titleLabel = new Label
            {
                Text = "vCenter Deployment Tool",
                Font = new Font("Microsoft Sans Serif", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.DarkBlue
            };

            // Version info
            var versionLabel = new Label
            {
                Text = "Version 1.0.0\nGUI Wrapper for vCenter Automation",
                Font = new Font("Microsoft Sans Serif", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            // Description
            var descriptionLabel = new Label
            {
                Text = "A Windows Forms GUI wrapper that provides a user-friendly interface for the vCenter deployment automation. This tool simplifies the process of configuring and deploying VMware vCenter Server Appliance.\n\nFeatures:\n• Interactive configuration management\n• Real-time field validation\n• PowerShell integration\n• Progress tracking\n• Professional Windows interface",
                Font = new Font("Microsoft Sans Serif", 9),
                TextAlign = ContentAlignment.TopLeft,
                Dock = DockStyle.Fill
            };

            // Author/Copyright
            var copyrightLabel = new Label
            {
                Text = "Created by HuskyNZ\n© 2024 - Built with Windows Forms and .NET",
                Font = new Font("Microsoft Sans Serif", 8),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = Color.Gray
            };

            // Close button
            var closeButton = new Button
            {
                Text = "Close",
                Size = new Size(75, 23),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                DialogResult = DialogResult.OK
            };

            var buttonPanel = new Panel
            {
                Height = 35,
                Dock = DockStyle.Bottom
            };
            
            closeButton.Location = new Point(buttonPanel.Width - closeButton.Width - 20, 6);
            buttonPanel.Controls.Add(closeButton);

            // Set row styles
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Add controls
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