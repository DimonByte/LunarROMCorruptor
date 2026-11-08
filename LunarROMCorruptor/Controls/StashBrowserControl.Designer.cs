namespace LunarROMCorruptor.Controls
{
    partial class StashBrowserControl
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
                Modules.StashLibrary.Changed -= OnLibraryChanged;
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
            FilterTxt = new TextBox();
            OptionsPanel = new Panel();
            SortCombo = new ComboBox();
            OnlyLoadedFileChk = new CheckBox();
            BodySplit = new SplitContainer();
            StashTree = new TreeView();
            StashMenu = new ContextMenuStrip(components);
            CorruptMenuItem = new ToolStripMenuItem();
            QuickLoadMenuItem = new ToolStripMenuItem();
            ApplyToLoadedMenuItem = new ToolStripMenuItem();
            MenuSeparator1 = new ToolStripSeparator();
            DetailsMenuItem = new ToolStripMenuItem();
            EditMenuItem = new ToolStripMenuItem();
            LoadMenuItem = new ToolStripMenuItem();
            LoadSettingsMenuItem = new ToolStripMenuItem();
            LoadStashsFileOnlyBtn = new ToolStripMenuItem();
            ScreenshotMenuItem = new ToolStripMenuItem();
            ScreenshotEmulatorMenuItem = new ToolStripMenuItem();
            ScreenshotClipboardMenuItem = new ToolStripMenuItem();
            ScreenshotFileMenuItem = new ToolStripMenuItem();
            ScreenshotByteViewMenuItem = new ToolStripMenuItem();
            RatingMenuItem = new ToolStripMenuItem();
            MenuSeparator2 = new ToolStripSeparator();
            OrganizeMenuItem = new ToolStripMenuItem();
            RenameMenuItem = new ToolStripMenuItem();
            MoveToMenuItem = new ToolStripMenuItem();
            NewGroupMenuItem = new ToolStripMenuItem();
            DeleteMenuItem = new ToolStripMenuItem();
            OrganizeSeparator = new ToolStripSeparator();
            ExportMenuItem = new ToolStripMenuItem();
            ImportMenuItem = new ToolStripMenuItem();
            OpenFolderMenuItem = new ToolStripMenuItem();
            MenuSeparator3 = new ToolStripSeparator();
            ShowPreviewMenuItem = new ToolStripMenuItem();
            RefreshMenuItem = new ToolStripMenuItem();
            NodeImages = new ImageList(components);
            PreviewPanel = new BufferedPanel();
            PreviewTip = new ToolTip(components);
            HintTip = new ToolTip(components);
            FilterTimer = new System.Windows.Forms.Timer(components);
            OptionsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)BodySplit).BeginInit();
            BodySplit.Panel1.SuspendLayout();
            BodySplit.Panel2.SuspendLayout();
            BodySplit.SuspendLayout();
            StashMenu.SuspendLayout();
            SuspendLayout();
            //
            // FilterTxt
            //
            FilterTxt.BackColor = Color.FromArgb(45, 45, 64);
            FilterTxt.BorderStyle = BorderStyle.FixedSingle;
            FilterTxt.Dock = DockStyle.Top;
            FilterTxt.ForeColor = Color.FromArgb(213, 216, 216);
            FilterTxt.Location = new Point(0, 0);
            FilterTxt.Name = "FilterTxt";
            FilterTxt.PlaceholderText = "🔎 Filter (name, file, engine, tag...)";
            FilterTxt.Size = new Size(254, 23);
            FilterTxt.TabIndex = 0;
            FilterTxt.TextChanged += FilterTxt_TextChanged;
            //
            // OptionsPanel
            //
            OptionsPanel.BackColor = Color.FromArgb(32, 35, 68);
            OptionsPanel.Controls.Add(SortCombo);
            OptionsPanel.Controls.Add(OnlyLoadedFileChk);
            OptionsPanel.Dock = DockStyle.Top;
            OptionsPanel.Location = new Point(0, 23);
            OptionsPanel.Name = "OptionsPanel";
            OptionsPanel.Size = new Size(254, 26);
            OptionsPanel.TabIndex = 1;
            //
            // SortCombo
            //
            SortCombo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SortCombo.BackColor = Color.FromArgb(45, 45, 64);
            SortCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            SortCombo.FlatStyle = FlatStyle.Flat;
            SortCombo.ForeColor = Color.FromArgb(213, 216, 216);
            SortCombo.FormattingEnabled = true;
            SortCombo.Items.AddRange(new object[] { "Sort: Name (A-Z)", "Sort: Newest first", "Sort: Oldest first", "Sort: Rating", "Sort: File", "Group by file" });
            SortCombo.Location = new Point(130, 2);
            SortCombo.Name = "SortCombo";
            SortCombo.Size = new Size(122, 23);
            SortCombo.TabIndex = 1;
            HintTip.SetToolTip(SortCombo, "How the stashes are sorted. \"Group by file\" lists every stash under the file it was made for (ignoring groups).");
            SortCombo.SelectedIndexChanged += SortCombo_SelectedIndexChanged;
            //
            // OnlyLoadedFileChk
            //
            OnlyLoadedFileChk.AutoSize = true;
            OnlyLoadedFileChk.ForeColor = Color.FromArgb(213, 216, 216);
            OnlyLoadedFileChk.Location = new Point(4, 4);
            OnlyLoadedFileChk.Name = "OnlyLoadedFileChk";
            OnlyLoadedFileChk.Size = new Size(113, 19);
            OnlyLoadedFileChk.TabIndex = 0;
            OnlyLoadedFileChk.Text = "Loaded file only";
            HintTip.SetToolTip(OnlyLoadedFileChk, "Only show the stashes that were made for the file that is loaded right now.");
            OnlyLoadedFileChk.UseVisualStyleBackColor = true;
            OnlyLoadedFileChk.CheckedChanged += OnlyLoadedFileChk_CheckedChanged;
            //
            // BodySplit
            //
            BodySplit.BackColor = Color.FromArgb(60, 66, 110);
            BodySplit.Dock = DockStyle.Fill;
            BodySplit.FixedPanel = FixedPanel.Panel2;
            BodySplit.Location = new Point(0, 49);
            BodySplit.Name = "BodySplit";
            BodySplit.Orientation = Orientation.Horizontal;
            //
            // BodySplit.Panel1
            //
            BodySplit.Panel1.Controls.Add(StashTree);
            BodySplit.Panel1MinSize = 60;
            //
            // BodySplit.Panel2
            //
            BodySplit.Panel2.Controls.Add(PreviewPanel);
            BodySplit.Panel2MinSize = 40;
            BodySplit.Size = new Size(254, 283);
            BodySplit.SplitterDistance = 159;
            BodySplit.SplitterWidth = 4;
            BodySplit.TabIndex = 2;
            BodySplit.SplitterMoved += BodySplit_SplitterMoved;
            //
            // StashTree
            //
            StashTree.AllowDrop = true;
            StashTree.BackColor = Color.FromArgb(39, 43, 83);
            StashTree.BorderStyle = BorderStyle.None;
            StashTree.ContextMenuStrip = StashMenu;
            StashTree.Dock = DockStyle.Fill;
            StashTree.DrawMode = TreeViewDrawMode.OwnerDrawText;
            StashTree.ForeColor = Color.FromArgb(213, 216, 216);
            StashTree.FullRowSelect = true;
            StashTree.HideSelection = false;
            StashTree.ImageIndex = 0;
            StashTree.ImageList = NodeImages;
            StashTree.ItemHeight = 20;
            StashTree.LabelEdit = true;
            StashTree.LineColor = Color.FromArgb(120, 120, 150);
            StashTree.Location = new Point(0, 0);
            StashTree.Name = "StashTree";
            StashTree.SelectedImageIndex = 0;
            StashTree.Size = new Size(254, 159);
            StashTree.TabIndex = 1;
            StashTree.BeforeLabelEdit += StashTree_BeforeLabelEdit;
            StashTree.AfterLabelEdit += StashTree_AfterLabelEdit;
            StashTree.AfterCollapse += StashTree_AfterExpandCollapse;
            StashTree.AfterExpand += StashTree_AfterExpandCollapse;
            StashTree.DrawNode += StashTree_DrawNode;
            StashTree.ItemDrag += StashTree_ItemDrag;
            StashTree.NodeMouseHover += StashTree_NodeMouseHover;
            StashTree.AfterSelect += StashTree_AfterSelect;
            StashTree.NodeMouseClick += StashTree_NodeMouseClick;
            StashTree.NodeMouseDoubleClick += StashTree_NodeMouseDoubleClick;
            StashTree.DragDrop += StashTree_DragDrop;
            StashTree.DragOver += StashTree_DragOver;
            StashTree.DragLeave += StashTree_DragLeave;
            StashTree.KeyDown += StashTree_KeyDown;
            StashTree.MouseLeave += StashTree_MouseLeave;
            //
            // StashMenu
            //
            StashMenu.Items.AddRange(new ToolStripItem[] { CorruptMenuItem, MenuSeparator1, DetailsMenuItem, EditMenuItem, LoadMenuItem, ScreenshotMenuItem, RatingMenuItem, MenuSeparator2, OrganizeMenuItem, MenuSeparator3, ShowPreviewMenuItem, RefreshMenuItem });
            StashMenu.Name = "StashMenu";
            StashMenu.Size = new Size(240, 220);
            StashMenu.Opening += StashMenu_Opening;
            //
            // CorruptMenuItem
            //
            CorruptMenuItem.DropDownItems.AddRange(new ToolStripItem[] { QuickLoadMenuItem, ApplyToLoadedMenuItem });
            CorruptMenuItem.Name = "CorruptMenuItem";
            CorruptMenuItem.Size = new Size(239, 22);
            CorruptMenuItem.Text = "Corrupt";
            //
            // QuickLoadMenuItem
            //
            QuickLoadMenuItem.Name = "QuickLoadMenuItem";
            QuickLoadMenuItem.Size = new Size(220, 22);
            QuickLoadMenuItem.Text = "Replay on original file";
            QuickLoadMenuItem.ToolTipText = "Loads the file this stash was made for, sets the save location it used and applies the stash: exactly the same corruption.";
            QuickLoadMenuItem.Click += QuickLoadMenuItem_Click;
            //
            // ApplyToLoadedMenuItem
            //
            ApplyToLoadedMenuItem.Name = "ApplyToLoadedMenuItem";
            ApplyToLoadedMenuItem.Size = new Size(220, 22);
            ApplyToLoadedMenuItem.Text = "Apply to current file";
            ApplyToLoadedMenuItem.ToolTipText = "Applies this stash to the file that is loaded right now and saves it to the current save location.";
            ApplyToLoadedMenuItem.Click += ApplyToLoadedMenuItem_Click;
            //
            // MenuSeparator1
            //
            MenuSeparator1.Name = "MenuSeparator1";
            MenuSeparator1.Size = new Size(236, 6);
            //
            // DetailsMenuItem
            //
            DetailsMenuItem.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            DetailsMenuItem.Name = "DetailsMenuItem";
            DetailsMenuItem.ShortcutKeyDisplayString = "Enter";
            DetailsMenuItem.Size = new Size(239, 22);
            DetailsMenuItem.Text = "Details, notes and screenshots...";
            DetailsMenuItem.Click += DetailsMenuItem_Click;
            //
            // EditMenuItem
            //
            EditMenuItem.Name = "EditMenuItem";
            EditMenuItem.Size = new Size(239, 22);
            EditMenuItem.Text = "Open in Stash Editor";
            EditMenuItem.Click += EditMenuItem_Click;
            //
            // LoadMenuItem
            //
            LoadMenuItem.DropDownItems.AddRange(new ToolStripItem[] { LoadSettingsMenuItem, LoadStashsFileOnlyBtn });
            LoadMenuItem.Name = "LoadMenuItem";
            LoadMenuItem.Size = new Size(239, 22);
            LoadMenuItem.Text = "Load";
            //
            // LoadSettingsMenuItem
            //
            LoadSettingsMenuItem.Name = "LoadSettingsMenuItem";
            LoadSettingsMenuItem.Size = new Size(240, 22);
            LoadSettingsMenuItem.Text = "Settings only";
            LoadSettingsMenuItem.ToolTipText = "Loads the engine settings, byte range and intensity this stash was made with, so you can make new corruptions like it.";
            LoadSettingsMenuItem.Click += LoadSettingsMenuItem_Click;
            //
            // LoadStashsFileOnlyBtn
            //
            LoadStashsFileOnlyBtn.Name = "LoadStashsFileOnlyBtn";
            LoadStashsFileOnlyBtn.Size = new Size(240, 22);
            LoadStashsFileOnlyBtn.Text = "File and settings (no corruption)";
            LoadStashsFileOnlyBtn.ToolTipText = "Loads the stash's file, engine settings, byte range and intensity this stash was made with, without corruption.";
            LoadStashsFileOnlyBtn.Click += LoadStashsFileOnlyBtn_Click;
            //
            // ScreenshotMenuItem
            //
            ScreenshotMenuItem.DropDownItems.AddRange(new ToolStripItem[] { ScreenshotEmulatorMenuItem, ScreenshotClipboardMenuItem, ScreenshotFileMenuItem, ScreenshotByteViewMenuItem });
            ScreenshotMenuItem.Name = "ScreenshotMenuItem";
            ScreenshotMenuItem.Size = new Size(239, 22);
            ScreenshotMenuItem.Text = "Add screenshot";
            //
            // ScreenshotEmulatorMenuItem
            //
            ScreenshotEmulatorMenuItem.Name = "ScreenshotEmulatorMenuItem";
            ScreenshotEmulatorMenuItem.Size = new Size(233, 22);
            ScreenshotEmulatorMenuItem.Text = "📷 From the running emulator";
            ScreenshotEmulatorMenuItem.Click += ScreenshotEmulatorMenuItem_Click;
            //
            // ScreenshotClipboardMenuItem
            //
            ScreenshotClipboardMenuItem.Name = "ScreenshotClipboardMenuItem";
            ScreenshotClipboardMenuItem.Size = new Size(233, 22);
            ScreenshotClipboardMenuItem.Text = "Paste from the clipboard";
            ScreenshotClipboardMenuItem.Click += ScreenshotClipboardMenuItem_Click;
            //
            // ScreenshotFileMenuItem
            //
            ScreenshotFileMenuItem.Name = "ScreenshotFileMenuItem";
            ScreenshotFileMenuItem.Size = new Size(233, 22);
            ScreenshotFileMenuItem.Text = "From an image file...";
            ScreenshotFileMenuItem.Click += ScreenshotFileMenuItem_Click;
            //
            // ScreenshotByteViewMenuItem
            //
            ScreenshotByteViewMenuItem.Name = "ScreenshotByteViewMenuItem";
            ScreenshotByteViewMenuItem.Size = new Size(233, 22);
            ScreenshotByteViewMenuItem.Text = "The current ByteView image";
            ScreenshotByteViewMenuItem.Click += ScreenshotByteViewMenuItem_Click;
            //
            // RatingMenuItem
            //
            RatingMenuItem.Name = "RatingMenuItem";
            RatingMenuItem.Size = new Size(239, 22);
            RatingMenuItem.Text = "Rating";
            //
            // MenuSeparator2
            //
            MenuSeparator2.Name = "MenuSeparator2";
            MenuSeparator2.Size = new Size(236, 6);
            //
            // OrganizeMenuItem
            //
            OrganizeMenuItem.DropDownItems.AddRange(new ToolStripItem[] { RenameMenuItem, MoveToMenuItem, NewGroupMenuItem, DeleteMenuItem, OrganizeSeparator, ExportMenuItem, ImportMenuItem, OpenFolderMenuItem });
            OrganizeMenuItem.Name = "OrganizeMenuItem";
            OrganizeMenuItem.Size = new Size(239, 22);
            OrganizeMenuItem.Text = "Organize";
            //
            // RenameMenuItem
            //
            RenameMenuItem.Name = "RenameMenuItem";
            RenameMenuItem.ShortcutKeyDisplayString = "F2";
            RenameMenuItem.Size = new Size(230, 22);
            RenameMenuItem.Text = "Rename";
            RenameMenuItem.Click += RenameMenuItem_Click;
            //
            // MoveToMenuItem
            //
            MoveToMenuItem.Name = "MoveToMenuItem";
            MoveToMenuItem.Size = new Size(230, 22);
            MoveToMenuItem.Text = "Move to group";
            //
            // NewGroupMenuItem
            //
            NewGroupMenuItem.Name = "NewGroupMenuItem";
            NewGroupMenuItem.Size = new Size(230, 22);
            NewGroupMenuItem.Text = "📁 New group...";
            NewGroupMenuItem.Click += NewGroupMenuItem_Click;
            //
            // DeleteMenuItem
            //
            DeleteMenuItem.Name = "DeleteMenuItem";
            DeleteMenuItem.ShortcutKeyDisplayString = "Del";
            DeleteMenuItem.Size = new Size(230, 22);
            DeleteMenuItem.Text = "Delete";
            DeleteMenuItem.Click += DeleteMenuItem_Click;
            //
            // OrganizeSeparator
            //
            OrganizeSeparator.Name = "OrganizeSeparator";
            OrganizeSeparator.Size = new Size(227, 6);
            //
            // ExportMenuItem
            //
            ExportMenuItem.Name = "ExportMenuItem";
            ExportMenuItem.Size = new Size(230, 22);
            ExportMenuItem.Text = "Export to share (.lrcstash)...";
            ExportMenuItem.Click += ExportMenuItem_Click;
            //
            // ImportMenuItem
            //
            ImportMenuItem.Name = "ImportMenuItem";
            ImportMenuItem.Size = new Size(230, 22);
            ImportMenuItem.Text = "Import (.lrcstash / .stash)...";
            ImportMenuItem.Click += ImportMenuItem_Click;
            //
            // OpenFolderMenuItem
            //
            OpenFolderMenuItem.Name = "OpenFolderMenuItem";
            OpenFolderMenuItem.Size = new Size(230, 22);
            OpenFolderMenuItem.Text = "Show in Explorer";
            OpenFolderMenuItem.Click += OpenFolderMenuItem_Click;
            //
            // MenuSeparator3
            //
            MenuSeparator3.Name = "MenuSeparator3";
            MenuSeparator3.Size = new Size(236, 6);
            //
            // ShowPreviewMenuItem
            //
            ShowPreviewMenuItem.CheckOnClick = true;
            ShowPreviewMenuItem.Name = "ShowPreviewMenuItem";
            ShowPreviewMenuItem.Size = new Size(239, 22);
            ShowPreviewMenuItem.Text = "Show preview panel";
            ShowPreviewMenuItem.Click += ShowPreviewMenuItem_Click;
            //
            // RefreshMenuItem
            //
            RefreshMenuItem.Name = "RefreshMenuItem";
            RefreshMenuItem.Size = new Size(239, 22);
            RefreshMenuItem.Text = "Refresh";
            RefreshMenuItem.Click += RefreshMenuItem_Click;
            //
            // NodeImages
            //
            NodeImages.ColorDepth = ColorDepth.Depth32Bit;
            NodeImages.ImageSize = new Size(16, 16);
            NodeImages.TransparentColor = Color.Transparent;
            //
            // PreviewPanel
            //
            PreviewPanel.BackColor = Color.FromArgb(28, 27, 55);
            PreviewPanel.Dock = DockStyle.Fill;
            PreviewPanel.Location = new Point(0, 0);
            PreviewPanel.Name = "PreviewPanel";
            PreviewPanel.Size = new Size(254, 120);
            PreviewPanel.TabIndex = 0;
            PreviewPanel.Paint += PreviewPanel_Paint;
            PreviewPanel.DoubleClick += PreviewPanel_DoubleClick;
            //
            // PreviewTip
            //
            PreviewTip.AutoPopDelay = 20000;
            PreviewTip.InitialDelay = 400;
            PreviewTip.OwnerDraw = true;
            PreviewTip.ReshowDelay = 100;
            PreviewTip.Draw += PreviewTip_Draw;
            PreviewTip.Popup += PreviewTip_Popup;
            //
            // FilterTimer
            //
            FilterTimer.Interval = 250;
            FilterTimer.Tick += FilterTimer_Tick;
            //
            // StashBrowserControl
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(39, 43, 83);
            Controls.Add(BodySplit);
            Controls.Add(OptionsPanel);
            Controls.Add(FilterTxt);
            Name = "StashBrowserControl";
            Size = new Size(254, 332);
            OptionsPanel.ResumeLayout(false);
            OptionsPanel.PerformLayout();
            BodySplit.Panel1.ResumeLayout(false);
            BodySplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)BodySplit).EndInit();
            BodySplit.ResumeLayout(false);
            StashMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox FilterTxt;
        private Panel OptionsPanel;
        private ComboBox SortCombo;
        private CheckBox OnlyLoadedFileChk;
        private SplitContainer BodySplit;
        private TreeView StashTree;
        private ContextMenuStrip StashMenu;
        private ToolStripMenuItem CorruptMenuItem;
        private ToolStripMenuItem QuickLoadMenuItem;
        private ToolStripMenuItem ApplyToLoadedMenuItem;
        private ToolStripSeparator MenuSeparator1;
        private ToolStripMenuItem DetailsMenuItem;
        private ToolStripMenuItem EditMenuItem;
        private ToolStripMenuItem LoadMenuItem;
        private ToolStripMenuItem LoadSettingsMenuItem;
        private ToolStripMenuItem LoadStashsFileOnlyBtn;
        private ToolStripMenuItem ScreenshotMenuItem;
        private ToolStripMenuItem ScreenshotEmulatorMenuItem;
        private ToolStripMenuItem ScreenshotClipboardMenuItem;
        private ToolStripMenuItem ScreenshotFileMenuItem;
        private ToolStripMenuItem ScreenshotByteViewMenuItem;
        private ToolStripMenuItem RatingMenuItem;
        private ToolStripSeparator MenuSeparator2;
        private ToolStripMenuItem OrganizeMenuItem;
        private ToolStripMenuItem RenameMenuItem;
        private ToolStripMenuItem MoveToMenuItem;
        private ToolStripMenuItem NewGroupMenuItem;
        private ToolStripMenuItem DeleteMenuItem;
        private ToolStripSeparator OrganizeSeparator;
        private ToolStripMenuItem ExportMenuItem;
        private ToolStripMenuItem ImportMenuItem;
        private ToolStripMenuItem OpenFolderMenuItem;
        private ToolStripSeparator MenuSeparator3;
        private ToolStripMenuItem ShowPreviewMenuItem;
        private ToolStripMenuItem RefreshMenuItem;
        private ImageList NodeImages;
        private BufferedPanel PreviewPanel;
        private ToolTip PreviewTip;
        private ToolTip HintTip;
        private System.Windows.Forms.Timer FilterTimer;
    }
}
