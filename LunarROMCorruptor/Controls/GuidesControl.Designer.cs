namespace LunarROMCorruptor.Controls
{
    partial class GuidesControl
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
            if (disposing)
            {
                _loadCts?.Cancel();
                _loadCts?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            GuidesSplit = new SplitContainer();
            GuideList = new ListBox();
            GuideButtonsPanel = new FlowLayoutPanel();
            BackBtn = new Button();
            ReloadBtn = new Button();
            OpenOnlineBtn = new Button();
            GuideStatusLbl = new Label();
            GuideBrowser = new WebBrowser();
            ToolTipHelper = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)GuidesSplit).BeginInit();
            GuidesSplit.Panel1.SuspendLayout();
            GuidesSplit.Panel2.SuspendLayout();
            GuidesSplit.SuspendLayout();
            GuideButtonsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // GuidesSplit
            // 
            GuidesSplit.Dock = DockStyle.Fill;
            GuidesSplit.FixedPanel = FixedPanel.Panel1;
            GuidesSplit.Location = new Point(0, 0);
            GuidesSplit.Name = "GuidesSplit";
            // 
            // GuidesSplit.Panel1
            // 
            GuidesSplit.Panel1.Controls.Add(GuideList);
            GuidesSplit.Panel1.Controls.Add(GuideStatusLbl);
            GuidesSplit.Panel1.Controls.Add(GuideButtonsPanel);
            // 
            // GuidesSplit.Panel2
            // 
            GuidesSplit.Panel2.Controls.Add(GuideBrowser);
            GuidesSplit.Size = new Size(756, 413);
            GuidesSplit.SplitterDistance = 170;
            GuidesSplit.SplitterWidth = 5;
            GuidesSplit.TabIndex = 0;
            // 
            // GuideList
            // 
            GuideList.BackColor = Color.FromArgb(39, 43, 83);
            GuideList.BorderStyle = BorderStyle.None;
            GuideList.Dock = DockStyle.Fill;
            GuideList.ForeColor = Color.FromArgb(213, 216, 216);
            GuideList.IntegralHeight = false;
            GuideList.ItemHeight = 15;
            GuideList.Location = new Point(0, 62);
            GuideList.Name = "GuideList";
            GuideList.Size = new Size(170, 311);
            GuideList.TabIndex = 1;
            GuideList.SelectedIndexChanged += GuideList_SelectedIndexChanged;
            // 
            // GuideButtonsPanel
            // 
            GuideButtonsPanel.Controls.Add(BackBtn);
            GuideButtonsPanel.Controls.Add(ReloadBtn);
            GuideButtonsPanel.Controls.Add(OpenOnlineBtn);
            GuideButtonsPanel.Dock = DockStyle.Top;
            GuideButtonsPanel.Location = new Point(0, 0);
            GuideButtonsPanel.Name = "GuideButtonsPanel";
            GuideButtonsPanel.Size = new Size(170, 62);
            GuideButtonsPanel.TabIndex = 0;
            // 
            // BackBtn
            // 
            BackBtn.BackColor = Color.FromArgb(52, 60, 111);
            BackBtn.FlatAppearance.BorderSize = 0;
            BackBtn.FlatStyle = FlatStyle.Flat;
            BackBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BackBtn.Location = new Point(3, 3);
            BackBtn.Name = "BackBtn";
            BackBtn.Size = new Size(78, 25);
            BackBtn.TabIndex = 0;
            BackBtn.Text = "◀ Back";
            ToolTipHelper.SetToolTip(BackBtn, "Go back to the previous guide/section.");
            BackBtn.UseVisualStyleBackColor = false;
            BackBtn.Click += BackBtn_Click;
            // 
            // ReloadBtn
            // 
            ReloadBtn.BackColor = Color.FromArgb(52, 60, 111);
            ReloadBtn.FlatAppearance.BorderSize = 0;
            ReloadBtn.FlatStyle = FlatStyle.Flat;
            ReloadBtn.ForeColor = Color.FromArgb(213, 216, 216);
            ReloadBtn.Location = new Point(87, 3);
            ReloadBtn.Name = "ReloadBtn";
            ReloadBtn.Size = new Size(78, 25);
            ReloadBtn.TabIndex = 1;
            ReloadBtn.Text = "⟳ Reload";
            ToolTipHelper.SetToolTip(ReloadBtn, "Download the latest version of this guide from GitHub.");
            ReloadBtn.UseVisualStyleBackColor = false;
            ReloadBtn.Click += ReloadBtn_Click;
            // 
            // OpenOnlineBtn
            // 
            OpenOnlineBtn.BackColor = Color.FromArgb(52, 60, 111);
            OpenOnlineBtn.FlatAppearance.BorderSize = 0;
            OpenOnlineBtn.FlatStyle = FlatStyle.Flat;
            OpenOnlineBtn.ForeColor = Color.FromArgb(213, 216, 216);
            OpenOnlineBtn.Location = new Point(3, 34);
            OpenOnlineBtn.Name = "OpenOnlineBtn";
            OpenOnlineBtn.Size = new Size(162, 25);
            OpenOnlineBtn.TabIndex = 2;
            OpenOnlineBtn.Text = "Open on GitHub ↗";
            ToolTipHelper.SetToolTip(OpenOnlineBtn, "Open this guide in your web browser.");
            OpenOnlineBtn.UseVisualStyleBackColor = false;
            OpenOnlineBtn.Click += OpenOnlineBtn_Click;
            // 
            // GuideStatusLbl
            // 
            GuideStatusLbl.Dock = DockStyle.Bottom;
            GuideStatusLbl.ForeColor = Color.Silver;
            GuideStatusLbl.Location = new Point(0, 373);
            GuideStatusLbl.Name = "GuideStatusLbl";
            GuideStatusLbl.Padding = new Padding(3, 0, 3, 0);
            GuideStatusLbl.Size = new Size(170, 40);
            GuideStatusLbl.TabIndex = 2;
            // 
            // GuideBrowser
            // 
            GuideBrowser.AllowWebBrowserDrop = false;
            GuideBrowser.Dock = DockStyle.Fill;
            GuideBrowser.IsWebBrowserContextMenuEnabled = false;
            GuideBrowser.Location = new Point(0, 0);
            GuideBrowser.MinimumSize = new Size(20, 20);
            GuideBrowser.Name = "GuideBrowser";
            GuideBrowser.ScriptErrorsSuppressed = true;
            GuideBrowser.Size = new Size(581, 413);
            GuideBrowser.TabIndex = 0;
            GuideBrowser.WebBrowserShortcutsEnabled = true;
            GuideBrowser.DocumentCompleted += GuideBrowser_DocumentCompleted;
            GuideBrowser.Navigating += GuideBrowser_Navigating;
            // 
            // GuidesControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            Controls.Add(GuidesSplit);
            Name = "GuidesControl";
            Size = new Size(756, 413);
            GuidesSplit.Panel1.ResumeLayout(false);
            GuidesSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)GuidesSplit).EndInit();
            GuidesSplit.ResumeLayout(false);
            GuideButtonsPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer GuidesSplit;
        private ListBox GuideList;
        private FlowLayoutPanel GuideButtonsPanel;
        private Button BackBtn;
        private Button ReloadBtn;
        private Button OpenOnlineBtn;
        private Label GuideStatusLbl;
        private WebBrowser GuideBrowser;
        private ToolTip ToolTipHelper;
    }
}
