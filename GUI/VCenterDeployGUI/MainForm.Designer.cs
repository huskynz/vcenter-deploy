namespace VCenterDeployGUI
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tabControl = new TabControl();
            configTab = new TabPage();
            deployTab = new TabPage();
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            menuStrip = new MenuStrip();
            fileMenu = new ToolStripMenuItem();
            loadEnvMenuItem = new ToolStripMenuItem();
            saveEnvMenuItem = new ToolStripMenuItem();
            loadExampleMenuItem = new ToolStripMenuItem();
            toolStripSeparator = new ToolStripSeparator();
            exitMenuItem = new ToolStripMenuItem();
            helpMenu = new ToolStripMenuItem();
            aboutMenuItem = new ToolStripMenuItem();
            
            SuspendLayout();
            
            // 
            // tabControl
            // 
            tabControl.Controls.Add(configTab);
            tabControl.Controls.Add(deployTab);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 24);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1200, 650);
            tabControl.TabIndex = 0;
            tabControl.Font = new Font("Segoe UI", 9F);
            tabControl.ItemSize = new Size(100, 30);
            tabControl.Padding = new Point(12, 6);
            
            // 
            // configTab
            // 
            configTab.Location = new Point(4, 34);
            configTab.Name = "configTab";
            configTab.Padding = new Padding(12);
            configTab.Size = new Size(1192, 612);
            configTab.TabIndex = 0;
            configTab.Text = "🔧 Configuration";
            configTab.UseVisualStyleBackColor = true;
            configTab.BackColor = Color.FromArgb(248, 249, 250);
            
            // 
            // deployTab
            // 
            deployTab.Location = new Point(4, 34);
            deployTab.Name = "deployTab";
            deployTab.Padding = new Padding(12);
            deployTab.Size = new Size(1192, 612);
            deployTab.TabIndex = 1;
            deployTab.Text = "🚀 Deployment";
            deployTab.UseVisualStyleBackColor = true;
            deployTab.BackColor = Color.FromArgb(248, 249, 250);
            
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusStrip.Location = new Point(0, 674);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1200, 26);
            statusStrip.TabIndex = 1;
            statusStrip.Text = "statusStrip1";
            statusStrip.BackColor = Color.FromArgb(240, 242, 245);
            
            // 
            // statusLabel
            // 
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(39, 21);
            statusLabel.Text = "Ready";
            statusLabel.Font = new Font("Segoe UI", 9F);
            statusLabel.ForeColor = Color.FromArgb(65, 74, 84);
            
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, helpMenu });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1200, 24);
            menuStrip.TabIndex = 2;
            menuStrip.Text = "menuStrip1";
            menuStrip.BackColor = Color.FromArgb(250, 251, 252);
            menuStrip.Font = new Font("Segoe UI", 9F);
            
            // 
            // fileMenu
            // 
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] {
                loadEnvMenuItem,
                saveEnvMenuItem,
                loadExampleMenuItem,
                toolStripSeparator,
                exitMenuItem
            });
            fileMenu.Name = "fileMenu";
            fileMenu.Size = new Size(37, 20);
            fileMenu.Text = "&File";
            
            // 
            // loadEnvMenuItem
            // 
            loadEnvMenuItem.Name = "loadEnvMenuItem";
            loadEnvMenuItem.Size = new Size(200, 22);
            loadEnvMenuItem.Text = "📁 &Load .env File...";
            loadEnvMenuItem.Click += LoadEnvMenuItem_Click;
            
            // 
            // saveEnvMenuItem
            // 
            saveEnvMenuItem.Name = "saveEnvMenuItem";
            saveEnvMenuItem.Size = new Size(200, 22);
            saveEnvMenuItem.Text = "💾 &Save .env File...";
            saveEnvMenuItem.Click += SaveEnvMenuItem_Click;
            
            // 
            // loadExampleMenuItem
            // 
            loadExampleMenuItem.Name = "loadExampleMenuItem";
            loadExampleMenuItem.Size = new Size(200, 22);
            loadExampleMenuItem.Text = "📋 Load from &Example";
            loadExampleMenuItem.Click += LoadExampleMenuItem_Click;
            
            // 
            // toolStripSeparator
            // 
            toolStripSeparator.Name = "toolStripSeparator";
            toolStripSeparator.Size = new Size(197, 6);
            
            // 
            // exitMenuItem
            // 
            exitMenuItem.Name = "exitMenuItem";
            exitMenuItem.Size = new Size(200, 22);
            exitMenuItem.Text = "🚪 E&xit";
            exitMenuItem.Click += ExitMenuItem_Click;
            
            // 
            // helpMenu
            // 
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] { aboutMenuItem });
            helpMenu.Name = "helpMenu";
            helpMenu.Size = new Size(44, 20);
            helpMenu.Text = "&Help";
            
            // 
            // aboutMenuItem
            // 
            aboutMenuItem.Name = "aboutMenuItem";
            aboutMenuItem.Size = new Size(107, 22);
            aboutMenuItem.Text = "ℹ️ &About";
            aboutMenuItem.Click += AboutMenuItem_Click;
            
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1200, 700);
            Controls.Add(tabControl);
            Controls.Add(statusStrip);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "vCenter Deployment Tool";
            MinimumSize = new Size(1000, 600);
            BackColor = Color.FromArgb(248, 249, 250);
            Font = new Font("Segoe UI", 9F);
            Icon = null;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TabControl tabControl;
        private TabPage configTab;
        private TabPage deployTab;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenu;
        private ToolStripMenuItem loadEnvMenuItem;
        private ToolStripMenuItem saveEnvMenuItem;
        private ToolStripMenuItem loadExampleMenuItem;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripMenuItem exitMenuItem;
        private ToolStripMenuItem helpMenu;
        private ToolStripMenuItem aboutMenuItem;
    }
}