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
            StashTree = new TreeView();
            StashMenu = new ContextMenuStrip(components);
            QuickLoadMenuItem = new ToolStripMenuItem();
            ApplyToLoadedMenuItem = new ToolStripMenuItem();
            MenuSeparator1 = new ToolStripSeparator();
            DetailsMenuItem = new ToolStripMenuItem();
            EditMenuItem = new ToolStripMenuItem();
            LoadSettingsMenuItem = new ToolStripMenuItem();
            LoadStashsFileOnlyBtn = new ToolStripMenuItem();
            ScreenshotMenuItem = new ToolStripMenuItem();
            ScreenshotEmulatorMenuItem = new ToolStripMenuItem();
            ScreenshotClipboardMenuItem = new ToolStripMenuItem();
            ScreenshotFileMenuItem = new ToolStripMenuItem();
            ScreenshotByteViewMenuItem = new ToolStripMenuItem();
            RatingMenuItem = new ToolStripMenuItem();
            MenuSeparator2 = new ToolStripSeparator();
            RenameMenuItem = new ToolStripMenuItem();
            NewGroupMenuItem = new ToolStripMenuItem();
            MoveToMenuItem = new ToolStripMenuItem();
            DeleteMenuItem = new ToolStripMenuItem();
            MenuSeparator3 = new ToolStripSeparator();
            ExportMenuItem = new ToolStripMenuItem();
            ImportMenuItem = new ToolStripMenuItem();
            OpenFolderMenuItem = new ToolStripMenuItem();
            MenuSeparator4 = new ToolStripSeparator();
            SortNewestMenuItem = new ToolStripMenuItem();
            AutoScreenshotMenuItem = new ToolStripMenuItem();
            RefreshMenuItem = new ToolStripMenuItem();
            NodeImages = new ImageList(components);
            PreviewTip = new ToolTip(components);
            FilterTimer = new System.Windows.Forms.Timer(components);
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
            FilterTxt.Size = new Size(212, 23);
            FilterTxt.TabIndex = 0;
            FilterTxt.TextChanged += FilterTxt_TextChanged;
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
            StashTree.Location = new Point(0, 23);
            StashTree.Name = "StashTree";
            StashTree.SelectedImageIndex = 0;
            StashTree.Size = new Size(212, 271);
            StashTree.TabIndex = 1;
            StashTree.BeforeLabelEdit += StashTree_BeforeLabelEdit;
            StashTree.AfterLabelEdit += StashTree_AfterLabelEdit;
            StashTree.AfterCollapse += StashTree_AfterExpandCollapse;
            StashTree.AfterExpand += StashTree_AfterExpandCollapse;
            StashTree.DrawNode += StashTree_DrawNode;
            StashTree.ItemDrag += StashTree_ItemDrag;
            StashTree.NodeMouseHover += StashTree_NodeMouseHover;
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
            StashMenu.Items.AddRange(new ToolStripItem[] { QuickLoadMenuItem, ApplyToLoadedMenuItem, MenuSeparator1, DetailsMenuItem, EditMenuItem, LoadSettingsMenuItem, LoadStashsFileOnlyBtn, ScreenshotMenuItem, RatingMenuItem, MenuSeparator2, RenameMenuItem, NewGroupMenuItem, MoveToMenuItem, DeleteMenuItem, MenuSeparator3, ExportMenuItem, ImportMenuItem, OpenFolderMenuItem, MenuSeparator4, SortNewestMenuItem, AutoScreenshotMenuItem, RefreshMenuItem });
            StashMenu.Name = "StashMenu";
            StashMenu.Size = new Size(322, 424);
            StashMenu.Opening += StashMenu_Opening;
            // 
            // QuickLoadMenuItem
            // 
            QuickLoadMenuItem.Name = "QuickLoadMenuItem";
            QuickLoadMenuItem.Size = new Size(321, 22);
            QuickLoadMenuItem.Text = "Corrupt stash's file using this stash";
            QuickLoadMenuItem.Click += QuickLoadMenuItem_Click;
            // 
            // ApplyToLoadedMenuItem
            // 
            ApplyToLoadedMenuItem.Name = "ApplyToLoadedMenuItem";
            ApplyToLoadedMenuItem.Size = new Size(321, 22);
            ApplyToLoadedMenuItem.Text = "Corrupt loaded file using this stash";
            ApplyToLoadedMenuItem.Click += ApplyToLoadedMenuItem_Click;
            // 
            // MenuSeparator1
            // 
            MenuSeparator1.Name = "MenuSeparator1";
            MenuSeparator1.Size = new Size(318, 6);
            // 
            // DetailsMenuItem
            // 
            DetailsMenuItem.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            DetailsMenuItem.Name = "DetailsMenuItem";
            DetailsMenuItem.ShortcutKeyDisplayString = "Enter";
            DetailsMenuItem.Size = new Size(321, 22);
            DetailsMenuItem.Text = "Details, notes and screenshots...";
            DetailsMenuItem.Click += DetailsMenuItem_Click;
            // 
            // EditMenuItem
            // 
            EditMenuItem.Name = "EditMenuItem";
            EditMenuItem.Size = new Size(321, 22);
            EditMenuItem.Text = "Open in Stash Editor";
            EditMenuItem.Click += EditMenuItem_Click;
            // 
            // LoadSettingsMenuItem
            // 
            LoadSettingsMenuItem.Name = "LoadSettingsMenuItem";
            LoadSettingsMenuItem.Size = new Size(321, 22);
            LoadSettingsMenuItem.Text = "Load its engine, range and intensity";
            LoadSettingsMenuItem.ToolTipText = "Loads the engine settings, byte range and intensity this stash was made with, so you can make new corruptions like it.";
            LoadSettingsMenuItem.Click += LoadSettingsMenuItem_Click;
            // 
            // LoadStashsFileOnlyBtn
            // 
            LoadStashsFileOnlyBtn.Name = "LoadStashsFileOnlyBtn";
            LoadStashsFileOnlyBtn.Size = new Size(321, 22);
            LoadStashsFileOnlyBtn.Text = "Load stash's file, its engine, range and intensity";
            LoadStashsFileOnlyBtn.ToolTipText = "Loads the stash's file, engine settings, byte range and intensity this stash was made with, without corruption.";
            LoadStashsFileOnlyBtn.Click += LoadStashsFileOnlyBtn_Click;
            // 
            // ScreenshotMenuItem
            // 
            ScreenshotMenuItem.DropDownItems.AddRange(new ToolStripItem[] { ScreenshotEmulatorMenuItem, ScreenshotClipboardMenuItem, ScreenshotFileMenuItem, ScreenshotByteViewMenuItem });
            ScreenshotMenuItem.Name = "ScreenshotMenuItem";
            ScreenshotMenuItem.Size = new Size(321, 22);
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
            RatingMenuItem.Size = new Size(321, 22);
            RatingMenuItem.Text = "Rating";
            // 
            // MenuSeparator2
            // 
            MenuSeparator2.Name = "MenuSeparator2";
            MenuSeparator2.Size = new Size(318, 6);
            // 
            // RenameMenuItem
            // 
            RenameMenuItem.Name = "RenameMenuItem";
            RenameMenuItem.ShortcutKeyDisplayString = "F2";
            RenameMenuItem.Size = new Size(321, 22);
            RenameMenuItem.Text = "Rename";
            RenameMenuItem.Click += RenameMenuItem_Click;
            // 
            // NewGroupMenuItem
            // 
            NewGroupMenuItem.Name = "NewGroupMenuItem";
            NewGroupMenuItem.Size = new Size(321, 22);
            NewGroupMenuItem.Text = "📁 New group...";
            NewGroupMenuItem.Click += NewGroupMenuItem_Click;
            // 
            // MoveToMenuItem
            // 
            MoveToMenuItem.Name = "MoveToMenuItem";
            MoveToMenuItem.Size = new Size(321, 22);
            MoveToMenuItem.Text = "Move to group";
            // 
            // DeleteMenuItem
            // 
            DeleteMenuItem.Name = "DeleteMenuItem";
            DeleteMenuItem.ShortcutKeyDisplayString = "Del";
            DeleteMenuItem.Size = new Size(321, 22);
            DeleteMenuItem.Text = "Delete";
            DeleteMenuItem.Click += DeleteMenuItem_Click;
            // 
            // MenuSeparator3
            // 
            MenuSeparator3.Name = "MenuSeparator3";
            MenuSeparator3.Size = new Size(318, 6);
            // 
            // ExportMenuItem
            // 
            ExportMenuItem.Name = "ExportMenuItem";
            ExportMenuItem.Size = new Size(321, 22);
            ExportMenuItem.Text = "Export to share (.lrcstash)...";
            ExportMenuItem.Click += ExportMenuItem_Click;
            // 
            // ImportMenuItem
            // 
            ImportMenuItem.Name = "ImportMenuItem";
            ImportMenuItem.Size = new Size(321, 22);
            ImportMenuItem.Text = "Import (.lrcstash / .stash)...";
            ImportMenuItem.Click += ImportMenuItem_Click;
            // 
            // OpenFolderMenuItem
            // 
            OpenFolderMenuItem.Name = "OpenFolderMenuItem";
            OpenFolderMenuItem.Size = new Size(321, 22);
            OpenFolderMenuItem.Text = "Show in Explorer";
            OpenFolderMenuItem.Click += OpenFolderMenuItem_Click;
            // 
            // MenuSeparator4
            // 
            MenuSeparator4.Name = "MenuSeparator4";
            MenuSeparator4.Size = new Size(318, 6);
            // 
            // SortNewestMenuItem
            // 
            SortNewestMenuItem.CheckOnClick = true;
            SortNewestMenuItem.Name = "SortNewestMenuItem";
            SortNewestMenuItem.Size = new Size(321, 22);
            SortNewestMenuItem.Text = "Newest first (otherwise A-Z)";
            SortNewestMenuItem.Click += SortNewestMenuItem_Click;
            // 
            // AutoScreenshotMenuItem
            // 
            AutoScreenshotMenuItem.CheckOnClick = true;
            AutoScreenshotMenuItem.Name = "AutoScreenshotMenuItem";
            AutoScreenshotMenuItem.Size = new Size(321, 22);
            AutoScreenshotMenuItem.Text = "Screenshot the emulator when saving a stash";
            AutoScreenshotMenuItem.Click += AutoScreenshotMenuItem_Click;
            // 
            // RefreshMenuItem
            // 
            RefreshMenuItem.Name = "RefreshMenuItem";
            RefreshMenuItem.ShortcutKeyDisplayString = "F5";
            RefreshMenuItem.Size = new Size(321, 22);
            RefreshMenuItem.Text = "Refresh";
            RefreshMenuItem.Click += RefreshMenuItem_Click;
            // 
            // NodeImages
            // 
            NodeImages.ColorDepth = ColorDepth.Depth32Bit;
            NodeImages.ImageSize = new Size(16, 16);
            NodeImages.TransparentColor = Color.Transparent;
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
            Controls.Add(StashTree);
            Controls.Add(FilterTxt);
            Name = "StashBrowserControl";
            Size = new Size(212, 294);
            StashMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox FilterTxt;
        private TreeView StashTree;
        private ContextMenuStrip StashMenu;
        private ToolStripMenuItem QuickLoadMenuItem;
        private ToolStripMenuItem ApplyToLoadedMenuItem;
        private ToolStripSeparator MenuSeparator1;
        private ToolStripMenuItem DetailsMenuItem;
        private ToolStripMenuItem EditMenuItem;
        private ToolStripMenuItem LoadSettingsMenuItem;
        private ToolStripMenuItem ScreenshotMenuItem;
        private ToolStripMenuItem ScreenshotEmulatorMenuItem;
        private ToolStripMenuItem ScreenshotClipboardMenuItem;
        private ToolStripMenuItem ScreenshotFileMenuItem;
        private ToolStripMenuItem ScreenshotByteViewMenuItem;
        private ToolStripMenuItem RatingMenuItem;
        private ToolStripSeparator MenuSeparator2;
        private ToolStripMenuItem RenameMenuItem;
        private ToolStripMenuItem NewGroupMenuItem;
        private ToolStripMenuItem MoveToMenuItem;
        private ToolStripMenuItem DeleteMenuItem;
        private ToolStripSeparator MenuSeparator3;
        private ToolStripMenuItem ExportMenuItem;
        private ToolStripMenuItem ImportMenuItem;
        private ToolStripMenuItem OpenFolderMenuItem;
        private ToolStripSeparator MenuSeparator4;
        private ToolStripMenuItem SortNewestMenuItem;
        private ToolStripMenuItem AutoScreenshotMenuItem;
        private ToolStripMenuItem RefreshMenuItem;
        private ImageList NodeImages;
        private ToolTip PreviewTip;
        private System.Windows.Forms.Timer FilterTimer;
        private ToolStripMenuItem LoadStashsFileOnlyBtn;
    }
}
