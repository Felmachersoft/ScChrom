namespace ScChrom
    {
    partial class BrowserForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrowserForm));
            this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.ts_top = new System.Windows.Forms.ToolStrip();
            this.backButton = new System.Windows.Forms.ToolStripButton();
            this.forwardButton = new System.Windows.Forms.ToolStripButton();
            this.urlTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.goButton = new System.Windows.Forms.ToolStripButton();
            this.tSddb_menu = new System.Windows.Forms.ToolStripDropDownButton();
            this.tsmi_ShowDevTools = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_refresh = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_topmost = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.toolStripContainer.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer.SuspendLayout();
            this.ts_top.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer
            // 
            // 
            // toolStripContainer.ContentPanel
            // 
            this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(730, 465);
            this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer.LeftToolStripPanelVisible = false;
            this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer.Name = "toolStripContainer";
            this.toolStripContainer.RightToolStripPanelVisible = false;
            this.toolStripContainer.Size = new System.Drawing.Size(730, 490);
            this.toolStripContainer.TabIndex = 0;
            this.toolStripContainer.Text = "toolStripContainer1";
            // 
            // toolStripContainer.TopToolStripPanel
            // 
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.ts_top);
            // 
            // ts_top
            // 
            this.ts_top.Dock = System.Windows.Forms.DockStyle.None;
            this.ts_top.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ts_top.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backButton,
            this.forwardButton,
            this.urlTextBox,
            this.goButton,
            this.tSddb_menu});
            this.ts_top.Location = new System.Drawing.Point(0, 0);
            this.ts_top.Name = "ts_top";
            this.ts_top.Padding = new System.Windows.Forms.Padding(0);
            this.ts_top.Size = new System.Drawing.Size(730, 25);
            this.ts_top.Stretch = true;
            this.ts_top.TabIndex = 0;
            this.ts_top.Layout += new System.Windows.Forms.LayoutEventHandler(this.HandleToolStripLayout);
            // 
            // backButton
            // 
            this.backButton.Enabled = false;
            this.backButton.Image = global::ScChrom.Properties.Resources.nav_left_green;
            this.backButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(49, 22);
            this.backButton.Text = "Back";
            this.backButton.Click += new System.EventHandler(this.BackButtonClick);
            // 
            // forwardButton
            // 
            this.forwardButton.Enabled = false;
            this.forwardButton.Image = global::ScChrom.Properties.Resources.nav_right_green;
            this.forwardButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(67, 22);
            this.forwardButton.Text = "Forward";
            this.forwardButton.Click += new System.EventHandler(this.ForwardButtonClick);
            // 
            // urlTextBox
            // 
            this.urlTextBox.AutoSize = false;
            this.urlTextBox.Name = "urlTextBox";
            this.urlTextBox.Size = new System.Drawing.Size(500, 25);
            this.urlTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.UrlTextBoxKeyUp);
            // 
            // goButton
            // 
            this.goButton.Image = global::ScChrom.Properties.Resources.nav_plain_green;
            this.goButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(40, 22);
            this.goButton.Text = "Go";
            this.goButton.Click += new System.EventHandler(this.GoButtonClick);
            // 
            // tSddb_menu
            // 
            this.tSddb_menu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tSddb_menu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmi_ShowDevTools,
            this.tsmi_refresh,
            this.tsmi_topmost});
            this.tSddb_menu.Image = global::ScChrom.Properties.Resources.menu;
            this.tSddb_menu.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tSddb_menu.Name = "tSddb_menu";
            this.tSddb_menu.Size = new System.Drawing.Size(29, 22);
            this.tSddb_menu.Text = "Menu";
            this.tSddb_menu.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            this.tSddb_menu.ToolTipText = "Click hiere to open the menu";
            this.tSddb_menu.DropDownOpening += new System.EventHandler(this.tSddb_menu_DropDownOpening);
            // 
            // tsmi_ShowDevTools
            // 
            this.tsmi_ShowDevTools.Name = "tsmi_ShowDevTools";
            this.tsmi_ShowDevTools.Size = new System.Drawing.Size(152, 22);
            this.tsmi_ShowDevTools.Text = "Show dev tools";
            this.tsmi_ShowDevTools.Click += new System.EventHandler(this.tsmi_ShowDevTools_Click);
            // 
            // tsmi_refresh
            // 
            this.tsmi_refresh.Name = "tsmi_refresh";
            this.tsmi_refresh.Size = new System.Drawing.Size(152, 22);
            this.tsmi_refresh.Text = "Refresh";
            this.tsmi_refresh.Click += new System.EventHandler(this.tsmiRefresh_Click);
            // 
            // tsmi_topmost
            // 
            this.tsmi_topmost.Name = "tsmi_topmost";
            this.tsmi_topmost.Size = new System.Drawing.Size(152, 22);
            this.tsmi_topmost.Text = "Topmost";
            this.tsmi_topmost.Click += new System.EventHandler(this.tsmi_topmost_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "ScChrom";
            // 
            // BrowserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(730, 490);
            this.Controls.Add(this.toolStripContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BrowserForm";
            this.Text = "BrowserForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BrowserForm_FormClosing);
            this.Shown += new System.EventHandler(this.BrowserForm_Shown);
            this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.PerformLayout();
            this.toolStripContainer.ResumeLayout(false);
            this.toolStripContainer.PerformLayout();
            this.ts_top.ResumeLayout(false);
            this.ts_top.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer;
        private System.Windows.Forms.ToolStrip ts_top;
        private System.Windows.Forms.ToolStripButton backButton;
        private System.Windows.Forms.ToolStripButton forwardButton;
        private System.Windows.Forms.ToolStripTextBox urlTextBox;
        private System.Windows.Forms.ToolStripButton goButton;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ToolStripDropDownButton tSddb_menu;
        private System.Windows.Forms.ToolStripMenuItem tsmi_ShowDevTools;
        private System.Windows.Forms.ToolStripMenuItem tsmi_refresh;
        private System.Windows.Forms.ToolStripMenuItem tsmi_topmost;
    }
}