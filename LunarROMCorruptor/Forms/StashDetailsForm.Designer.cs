namespace LunarROMCorruptor.Forms
{
    partial class StashDetailsForm
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
                PreviewBox.Image?.Dispose();
                ClearScreenshotImages();
                components?.Dispose();
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
            components = new System.ComponentModel.Container();
            ScreenshotImages = new ImageList(components);
            ToolTipHelper = new ToolTip(components);
            EmulatorShotBtn = new Button();
            PasteShotBtn = new Button();
            FileShotBtn = new Button();
            ThumbnailBtn = new Button();
            RemoveShotBtn = new Button();
            LoadSettingsBtn = new Button();
            QuickLoadBtn = new Button();
            OpenFolderBtn = new Button();
            LocateFileBtn = new Button();
            PreviewBox = new PictureBox();
            ScreenshotList = new ListView();
            NameLbl = new Label();
            NameTxt = new TextBox();
            RatingLbl = new Label();
            RatingComboBox = new ComboBox();
            TagsLbl = new Label();
            TagsTxt = new TextBox();
            NotesLbl = new Label();
            NotesTxt = new TextBox();
            InfoTxt = new TextBox();
            SaveBtn = new Button();
            CloseBtn = new Button();
            ((System.ComponentModel.ISupportInitialize)PreviewBox).BeginInit();
            SuspendLayout();
            // 
            // ScreenshotImages
            // 
            ScreenshotImages.ColorDepth = ColorDepth.Depth32Bit;
            ScreenshotImages.ImageSize = new Size(96, 64);
            ScreenshotImages.TransparentColor = Color.Transparent;
            // 
            // EmulatorShotBtn
            // 
            EmulatorShotBtn.BackColor = Color.FromArgb(52, 60, 111);
            EmulatorShotBtn.FlatAppearance.BorderSize = 0;
            EmulatorShotBtn.FlatStyle = FlatStyle.Flat;
            EmulatorShotBtn.ForeColor = Color.FromArgb(213, 216, 216);
            EmulatorShotBtn.Location = new Point(8, 430);
            EmulatorShotBtn.Name = "EmulatorShotBtn";
            EmulatorShotBtn.Size = new Size(92, 27);
            EmulatorShotBtn.TabIndex = 2;
            EmulatorShotBtn.Text = "📷 Emulator";
            ToolTipHelper.SetToolTip(EmulatorShotBtn, "Take a screenshot of the running emulator.");
            EmulatorShotBtn.UseVisualStyleBackColor = false;
            EmulatorShotBtn.Click += EmulatorShotBtn_Click;
            // 
            // PasteShotBtn
            // 
            PasteShotBtn.BackColor = Color.FromArgb(52, 60, 111);
            PasteShotBtn.FlatAppearance.BorderSize = 0;
            PasteShotBtn.FlatStyle = FlatStyle.Flat;
            PasteShotBtn.ForeColor = Color.FromArgb(213, 216, 216);
            PasteShotBtn.Location = new Point(103, 430);
            PasteShotBtn.Name = "PasteShotBtn";
            PasteShotBtn.Size = new Size(62, 27);
            PasteShotBtn.TabIndex = 3;
            PasteShotBtn.Text = "Paste";
            ToolTipHelper.SetToolTip(PasteShotBtn, "Add the image on the clipboard.");
            PasteShotBtn.UseVisualStyleBackColor = false;
            PasteShotBtn.Click += PasteShotBtn_Click;
            // 
            // FileShotBtn
            // 
            FileShotBtn.BackColor = Color.FromArgb(52, 60, 111);
            FileShotBtn.FlatAppearance.BorderSize = 0;
            FileShotBtn.FlatStyle = FlatStyle.Flat;
            FileShotBtn.ForeColor = Color.FromArgb(213, 216, 216);
            FileShotBtn.Location = new Point(168, 430);
            FileShotBtn.Name = "FileShotBtn";
            FileShotBtn.Size = new Size(80, 27);
            FileShotBtn.TabIndex = 4;
            FileShotBtn.Text = "Add file...";
            ToolTipHelper.SetToolTip(FileShotBtn, "Add an image file.");
            FileShotBtn.UseVisualStyleBackColor = false;
            FileShotBtn.Click += FileShotBtn_Click;
            // 
            // ThumbnailBtn
            // 
            ThumbnailBtn.BackColor = Color.FromArgb(52, 60, 111);
            ThumbnailBtn.FlatAppearance.BorderSize = 0;
            ThumbnailBtn.FlatStyle = FlatStyle.Flat;
            ThumbnailBtn.ForeColor = Color.FromArgb(213, 216, 216);
            ThumbnailBtn.Location = new Point(251, 430);
            ThumbnailBtn.Name = "ThumbnailBtn";
            ThumbnailBtn.Size = new Size(112, 27);
            ThumbnailBtn.TabIndex = 5;
            ThumbnailBtn.Text = "Use as thumbnail";
            ToolTipHelper.SetToolTip(ThumbnailBtn, "Show the selected screenshot in the stash list.");
            ThumbnailBtn.UseVisualStyleBackColor = false;
            ThumbnailBtn.Click += ThumbnailBtn_Click;
            // 
            // RemoveShotBtn
            // 
            RemoveShotBtn.BackColor = Color.FromArgb(52, 60, 111);
            RemoveShotBtn.FlatAppearance.BorderSize = 0;
            RemoveShotBtn.FlatStyle = FlatStyle.Flat;
            RemoveShotBtn.ForeColor = Color.FromArgb(213, 216, 216);
            RemoveShotBtn.Location = new Point(366, 430);
            RemoveShotBtn.Name = "RemoveShotBtn";
            RemoveShotBtn.Size = new Size(74, 27);
            RemoveShotBtn.TabIndex = 6;
            RemoveShotBtn.Text = "Remove";
            ToolTipHelper.SetToolTip(RemoveShotBtn, "Delete the selected screenshot.");
            RemoveShotBtn.UseVisualStyleBackColor = false;
            RemoveShotBtn.Click += RemoveShotBtn_Click;
            // 
            // LoadSettingsBtn
            // 
            LoadSettingsBtn.BackColor = Color.FromArgb(52, 60, 111);
            LoadSettingsBtn.FlatAppearance.BorderSize = 0;
            LoadSettingsBtn.FlatStyle = FlatStyle.Flat;
            LoadSettingsBtn.ForeColor = Color.FromArgb(213, 216, 216);
            LoadSettingsBtn.Location = new Point(452, 430);
            LoadSettingsBtn.Name = "LoadSettingsBtn";
            LoadSettingsBtn.Size = new Size(130, 27);
            LoadSettingsBtn.TabIndex = 16;
            LoadSettingsBtn.Text = "Load engine + range";
            ToolTipHelper.SetToolTip(LoadSettingsBtn, "Load the engine settings, byte range and intensity this stash was made with.");
            LoadSettingsBtn.UseVisualStyleBackColor = false;
            LoadSettingsBtn.Click += LoadSettingsBtn_Click;
            // 
            // QuickLoadBtn
            // 
            QuickLoadBtn.BackColor = Color.FromArgb(52, 60, 111);
            QuickLoadBtn.FlatAppearance.BorderSize = 0;
            QuickLoadBtn.FlatStyle = FlatStyle.Flat;
            QuickLoadBtn.ForeColor = Color.FromArgb(213, 216, 216);
            QuickLoadBtn.Location = new Point(585, 430);
            QuickLoadBtn.Name = "QuickLoadBtn";
            QuickLoadBtn.Size = new Size(130, 27);
            QuickLoadBtn.TabIndex = 17;
            QuickLoadBtn.Text = "Corrupt stash’s file";
            ToolTipHelper.SetToolTip(QuickLoadBtn, "Load the file this stash was made for and apply the stash.");
            QuickLoadBtn.UseVisualStyleBackColor = false;
            QuickLoadBtn.Click += QuickLoadBtn_Click;
            // 
            // OpenFolderBtn
            // 
            OpenFolderBtn.BackColor = Color.FromArgb(52, 60, 111);
            OpenFolderBtn.FlatAppearance.BorderSize = 0;
            OpenFolderBtn.FlatStyle = FlatStyle.Flat;
            OpenFolderBtn.ForeColor = Color.FromArgb(213, 216, 216);
            OpenFolderBtn.Location = new Point(718, 430);
            OpenFolderBtn.Name = "OpenFolderBtn";
            OpenFolderBtn.Size = new Size(54, 27);
            OpenFolderBtn.TabIndex = 18;
            OpenFolderBtn.Text = "📂";
            ToolTipHelper.SetToolTip(OpenFolderBtn, "Show the stash folder in Explorer.");
            OpenFolderBtn.UseVisualStyleBackColor = false;
            OpenFolderBtn.Click += OpenFolderBtn_Click;
            // 
            // LocateFileBtn
            // 
            LocateFileBtn.BackColor = Color.FromArgb(52, 60, 111);
            LocateFileBtn.FlatAppearance.BorderSize = 0;
            LocateFileBtn.FlatStyle = FlatStyle.Flat;
            LocateFileBtn.ForeColor = Color.FromArgb(213, 216, 216);
            LocateFileBtn.Location = new Point(452, 474);
            LocateFileBtn.Name = "LocateFileBtn";
            LocateFileBtn.Size = new Size(110, 27);
            LocateFileBtn.TabIndex = 19;
            LocateFileBtn.Text = "Locate file...";
            ToolTipHelper.SetToolTip(LocateFileBtn, "The file this stash was made for moved? Pick its new location.");
            LocateFileBtn.UseVisualStyleBackColor = false;
            LocateFileBtn.Click += LocateFileBtn_Click;
            // 
            // PreviewBox
            // 
            PreviewBox.BackColor = Color.FromArgb(20, 20, 30);
            PreviewBox.Location = new Point(8, 8);
            PreviewBox.Name = "PreviewBox";
            PreviewBox.Size = new Size(432, 300);
            PreviewBox.SizeMode = PictureBoxSizeMode.Zoom;
            PreviewBox.TabIndex = 0;
            PreviewBox.TabStop = false;
            // 
            // ScreenshotList
            // 
            ScreenshotList.BackColor = Color.FromArgb(39, 43, 83);
            ScreenshotList.ForeColor = Color.FromArgb(213, 216, 216);
            ScreenshotList.LargeImageList = ScreenshotImages;
            ScreenshotList.Location = new Point(8, 314);
            ScreenshotList.MultiSelect = false;
            ScreenshotList.Name = "ScreenshotList";
            ScreenshotList.Size = new Size(432, 110);
            ScreenshotList.TabIndex = 1;
            ScreenshotList.UseCompatibleStateImageBehavior = false;
            ScreenshotList.SelectedIndexChanged += ScreenshotList_SelectedIndexChanged;
            // 
            // NameLbl
            // 
            NameLbl.AutoSize = true;
            NameLbl.ForeColor = Color.FromArgb(192, 255, 255);
            NameLbl.Location = new Point(452, 8);
            NameLbl.Name = "NameLbl";
            NameLbl.Size = new Size(42, 15);
            NameLbl.TabIndex = 7;
            NameLbl.Text = "Name:";
            // 
            // NameTxt
            // 
            NameTxt.BackColor = Color.FromArgb(45, 45, 64);
            NameTxt.BorderStyle = BorderStyle.FixedSingle;
            NameTxt.ForeColor = Color.FromArgb(213, 216, 216);
            NameTxt.Location = new Point(452, 26);
            NameTxt.Name = "NameTxt";
            NameTxt.Size = new Size(320, 23);
            NameTxt.TabIndex = 8;
            // 
            // RatingLbl
            // 
            RatingLbl.AutoSize = true;
            RatingLbl.ForeColor = Color.FromArgb(192, 255, 255);
            RatingLbl.Location = new Point(452, 57);
            RatingLbl.Name = "RatingLbl";
            RatingLbl.Size = new Size(44, 15);
            RatingLbl.TabIndex = 9;
            RatingLbl.Text = "Rating:";
            // 
            // RatingComboBox
            // 
            RatingComboBox.BackColor = Color.FromArgb(45, 45, 64);
            RatingComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            RatingComboBox.FlatStyle = FlatStyle.Flat;
            RatingComboBox.ForeColor = Color.Gold;
            RatingComboBox.Location = new Point(502, 54);
            RatingComboBox.Name = "RatingComboBox";
            RatingComboBox.Size = new Size(120, 23);
            RatingComboBox.TabIndex = 10;
            // 
            // TagsLbl
            // 
            TagsLbl.AutoSize = true;
            TagsLbl.ForeColor = Color.FromArgb(192, 255, 255);
            TagsLbl.Location = new Point(452, 84);
            TagsLbl.Name = "TagsLbl";
            TagsLbl.Size = new Size(140, 15);
            TagsLbl.TabIndex = 11;
            TagsLbl.Text = "Tags (comma separated):";
            // 
            // TagsTxt
            // 
            TagsTxt.BackColor = Color.FromArgb(45, 45, 64);
            TagsTxt.BorderStyle = BorderStyle.FixedSingle;
            TagsTxt.ForeColor = Color.FromArgb(213, 216, 216);
            TagsTxt.Location = new Point(452, 102);
            TagsTxt.Name = "TagsTxt";
            TagsTxt.Size = new Size(320, 23);
            TagsTxt.TabIndex = 12;
            // 
            // NotesLbl
            // 
            NotesLbl.AutoSize = true;
            NotesLbl.ForeColor = Color.FromArgb(192, 255, 255);
            NotesLbl.Location = new Point(452, 130);
            NotesLbl.Name = "NotesLbl";
            NotesLbl.Size = new Size(41, 15);
            NotesLbl.TabIndex = 13;
            NotesLbl.Text = "Notes:";
            // 
            // NotesTxt
            // 
            NotesTxt.AcceptsReturn = true;
            NotesTxt.BackColor = Color.FromArgb(45, 45, 64);
            NotesTxt.BorderStyle = BorderStyle.FixedSingle;
            NotesTxt.ForeColor = Color.FromArgb(213, 216, 216);
            NotesTxt.Location = new Point(452, 148);
            NotesTxt.Multiline = true;
            NotesTxt.Name = "NotesTxt";
            NotesTxt.ScrollBars = ScrollBars.Vertical;
            NotesTxt.Size = new Size(320, 108);
            NotesTxt.TabIndex = 14;
            // 
            // InfoTxt
            // 
            InfoTxt.AcceptsReturn = true;
            InfoTxt.BackColor = Color.FromArgb(32, 32, 50);
            InfoTxt.BorderStyle = BorderStyle.FixedSingle;
            InfoTxt.ForeColor = Color.FromArgb(213, 216, 216);
            InfoTxt.Location = new Point(452, 262);
            InfoTxt.Multiline = true;
            InfoTxt.Name = "InfoTxt";
            InfoTxt.ReadOnly = true;
            InfoTxt.ScrollBars = ScrollBars.Vertical;
            InfoTxt.Size = new Size(320, 162);
            InfoTxt.TabIndex = 15;
            // 
            // SaveBtn
            // 
            SaveBtn.BackColor = Color.FromArgb(52, 60, 111);
            SaveBtn.FlatAppearance.BorderSize = 0;
            SaveBtn.FlatStyle = FlatStyle.Flat;
            SaveBtn.ForeColor = Color.FromArgb(213, 216, 216);
            SaveBtn.Location = new Point(612, 474);
            SaveBtn.Name = "SaveBtn";
            SaveBtn.Size = new Size(78, 27);
            SaveBtn.TabIndex = 20;
            SaveBtn.Text = "Save";
            SaveBtn.UseVisualStyleBackColor = false;
            SaveBtn.Click += SaveBtn_Click;
            // 
            // CloseBtn
            // 
            CloseBtn.BackColor = Color.FromArgb(52, 60, 111);
            CloseBtn.FlatAppearance.BorderSize = 0;
            CloseBtn.FlatStyle = FlatStyle.Flat;
            CloseBtn.ForeColor = Color.FromArgb(213, 216, 216);
            CloseBtn.Location = new Point(694, 474);
            CloseBtn.Name = "CloseBtn";
            CloseBtn.Size = new Size(78, 27);
            CloseBtn.TabIndex = 21;
            CloseBtn.Text = "Close";
            CloseBtn.UseVisualStyleBackColor = false;
            CloseBtn.Click += CloseBtn_Click;
            // 
            // StashDetailsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            ClientSize = new Size(780, 510);
            Controls.Add(PreviewBox);
            Controls.Add(ScreenshotList);
            Controls.Add(EmulatorShotBtn);
            Controls.Add(PasteShotBtn);
            Controls.Add(FileShotBtn);
            Controls.Add(ThumbnailBtn);
            Controls.Add(RemoveShotBtn);
            Controls.Add(NameLbl);
            Controls.Add(NameTxt);
            Controls.Add(RatingLbl);
            Controls.Add(RatingComboBox);
            Controls.Add(TagsLbl);
            Controls.Add(TagsTxt);
            Controls.Add(NotesLbl);
            Controls.Add(NotesTxt);
            Controls.Add(InfoTxt);
            Controls.Add(LoadSettingsBtn);
            Controls.Add(QuickLoadBtn);
            Controls.Add(OpenFolderBtn);
            Controls.Add(LocateFileBtn);
            Controls.Add(SaveBtn);
            Controls.Add(CloseBtn);
            ForeColor = Color.FromArgb(213, 216, 216);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "StashDetailsForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Stash details";
            ((System.ComponentModel.ISupportInitialize)PreviewBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ImageList ScreenshotImages;
        private ToolTip ToolTipHelper;
        private PictureBox PreviewBox;
        private ListView ScreenshotList;
        private Button EmulatorShotBtn;
        private Button PasteShotBtn;
        private Button FileShotBtn;
        private Button ThumbnailBtn;
        private Button RemoveShotBtn;
        private Label NameLbl;
        private TextBox NameTxt;
        private Label RatingLbl;
        private ComboBox RatingComboBox;
        private Label TagsLbl;
        private TextBox TagsTxt;
        private Label NotesLbl;
        private TextBox NotesTxt;
        private TextBox InfoTxt;
        private Button LoadSettingsBtn;
        private Button QuickLoadBtn;
        private Button OpenFolderBtn;
        private Button LocateFileBtn;
        private Button SaveBtn;
        private Button CloseBtn;
    }
}
