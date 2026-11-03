namespace LunarROMCorruptor.Controls
{
    partial class PresetBarControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            PresetRow = new FlowLayoutPanel();
            PresetLbl = new Label();
            PresetComboBox = new ComboBox();
            SavePresetBtn = new Button();
            DeletePresetBtn = new Button();
            ImportPresetBtn = new Button();
            OpenPresetFolderBtn = new Button();
            PresetDescriptionLbl = new Label();
            ToolTipHelper = new ToolTip(components);
            PresetRow.SuspendLayout();
            SuspendLayout();
            // 
            // PresetRow
            // 
            PresetRow.BackColor = Color.Transparent;
            PresetRow.Controls.Add(PresetLbl);
            PresetRow.Controls.Add(PresetComboBox);
            PresetRow.Controls.Add(SavePresetBtn);
            PresetRow.Controls.Add(DeletePresetBtn);
            PresetRow.Controls.Add(ImportPresetBtn);
            PresetRow.Controls.Add(OpenPresetFolderBtn);
            PresetRow.Dock = DockStyle.Top;
            PresetRow.Location = new Point(0, 0);
            PresetRow.Name = "PresetRow";
            PresetRow.Padding = new Padding(2, 2, 2, 0);
            PresetRow.Size = new Size(750, 30);
            PresetRow.TabIndex = 0;
            PresetRow.WrapContents = false;
            // 
            // PresetLbl
            // 
            PresetLbl.AutoSize = true;
            PresetLbl.ForeColor = Color.FromArgb(192, 255, 255);
            PresetLbl.Location = new Point(2, 9);
            PresetLbl.Margin = new Padding(0, 7, 2, 0);
            PresetLbl.Name = "PresetLbl";
            PresetLbl.Size = new Size(42, 15);
            PresetLbl.TabIndex = 0;
            PresetLbl.Text = "Preset:";
            // 
            // PresetComboBox
            // 
            PresetComboBox.BackColor = Color.FromArgb(45, 45, 64);
            PresetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            PresetComboBox.FlatStyle = FlatStyle.Flat;
            PresetComboBox.ForeColor = Color.White;
            PresetComboBox.FormattingEnabled = true;
            PresetComboBox.Location = new Point(46, 5);
            PresetComboBox.Margin = new Padding(0, 3, 0, 0);
            PresetComboBox.MaxDropDownItems = 20;
            PresetComboBox.Name = "PresetComboBox";
            PresetComboBox.Size = new Size(256, 23);
            PresetComboBox.TabIndex = 1;
            ToolTipHelper.SetToolTip(PresetComboBox, "Pick a preset to load its settings into the engine. ★ = built-in, 👤 = your presets (UserCorruptionEnginePresets folder).");
            PresetComboBox.SelectedIndexChanged += PresetComboBox_SelectedIndexChanged;
            PresetComboBox.SelectionChangeCommitted += PresetComboBox_SelectionChangeCommitted;
            // 
            // SavePresetBtn
            // 
            SavePresetBtn.AutoSize = true;
            SavePresetBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SavePresetBtn.BackColor = Color.FromArgb(52, 60, 111);
            SavePresetBtn.FlatAppearance.BorderSize = 0;
            SavePresetBtn.FlatStyle = FlatStyle.Flat;
            SavePresetBtn.ForeColor = Color.FromArgb(213, 216, 216);
            SavePresetBtn.Location = new Point(305, 4);
            SavePresetBtn.Margin = new Padding(3, 2, 0, 0);
            SavePresetBtn.MinimumSize = new Size(0, 25);
            SavePresetBtn.Name = "SavePresetBtn";
            SavePresetBtn.Padding = new Padding(4, 0, 4, 0);
            SavePresetBtn.Size = new Size(74, 25);
            SavePresetBtn.TabIndex = 2;
            SavePresetBtn.Text = "Save As...";
            ToolTipHelper.SetToolTip(SavePresetBtn, "Save the current engine settings as a new user preset.");
            SavePresetBtn.UseVisualStyleBackColor = false;
            SavePresetBtn.Click += SavePresetBtn_Click;
            // 
            // DeletePresetBtn
            // 
            DeletePresetBtn.AutoSize = true;
            DeletePresetBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            DeletePresetBtn.BackColor = Color.FromArgb(52, 60, 111);
            DeletePresetBtn.FlatAppearance.BorderSize = 0;
            DeletePresetBtn.FlatStyle = FlatStyle.Flat;
            DeletePresetBtn.ForeColor = Color.FromArgb(213, 216, 216);
            DeletePresetBtn.Location = new Point(382, 4);
            DeletePresetBtn.Margin = new Padding(3, 2, 0, 0);
            DeletePresetBtn.MinimumSize = new Size(0, 25);
            DeletePresetBtn.Name = "DeletePresetBtn";
            DeletePresetBtn.Padding = new Padding(4, 0, 4, 0);
            DeletePresetBtn.Size = new Size(58, 25);
            DeletePresetBtn.TabIndex = 3;
            DeletePresetBtn.Text = "Delete";
            ToolTipHelper.SetToolTip(DeletePresetBtn, "Delete the selected user preset.");
            DeletePresetBtn.UseVisualStyleBackColor = false;
            DeletePresetBtn.Click += DeletePresetBtn_Click;
            // 
            // ImportPresetBtn
            // 
            ImportPresetBtn.AutoSize = true;
            ImportPresetBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ImportPresetBtn.BackColor = Color.FromArgb(52, 60, 111);
            ImportPresetBtn.FlatAppearance.BorderSize = 0;
            ImportPresetBtn.FlatStyle = FlatStyle.Flat;
            ImportPresetBtn.ForeColor = Color.FromArgb(213, 216, 216);
            ImportPresetBtn.Location = new Point(443, 4);
            ImportPresetBtn.Margin = new Padding(3, 2, 0, 0);
            ImportPresetBtn.MinimumSize = new Size(0, 25);
            ImportPresetBtn.Name = "ImportPresetBtn";
            ImportPresetBtn.Padding = new Padding(4, 0, 4, 0);
            ImportPresetBtn.Size = new Size(70, 25);
            ImportPresetBtn.TabIndex = 4;
            ImportPresetBtn.Text = "Import...";
            ToolTipHelper.SetToolTip(ImportPresetBtn, "Import a .lrcpreset file someone shared (you can also drag it onto LRC).");
            ImportPresetBtn.UseVisualStyleBackColor = false;
            ImportPresetBtn.Click += ImportPresetBtn_Click;
            // 
            // OpenPresetFolderBtn
            // 
            OpenPresetFolderBtn.AutoSize = true;
            OpenPresetFolderBtn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            OpenPresetFolderBtn.BackColor = Color.FromArgb(52, 60, 111);
            OpenPresetFolderBtn.FlatAppearance.BorderSize = 0;
            OpenPresetFolderBtn.FlatStyle = FlatStyle.Flat;
            OpenPresetFolderBtn.ForeColor = Color.FromArgb(213, 216, 216);
            OpenPresetFolderBtn.Location = new Point(516, 4);
            OpenPresetFolderBtn.Margin = new Padding(3, 2, 0, 0);
            OpenPresetFolderBtn.MinimumSize = new Size(0, 25);
            OpenPresetFolderBtn.Name = "OpenPresetFolderBtn";
            OpenPresetFolderBtn.Padding = new Padding(4, 0, 4, 0);
            OpenPresetFolderBtn.Size = new Size(90, 25);
            OpenPresetFolderBtn.TabIndex = 5;
            OpenPresetFolderBtn.Text = "Open Folder";
            ToolTipHelper.SetToolTip(OpenPresetFolderBtn, "Open the UserCorruptionEnginePresets folder (share presets by sending the .lrcpreset files).");
            OpenPresetFolderBtn.UseVisualStyleBackColor = false;
            OpenPresetFolderBtn.Click += OpenPresetFolderBtn_Click;
            // 
            // PresetDescriptionLbl
            // 
            PresetDescriptionLbl.AutoEllipsis = true;
            PresetDescriptionLbl.Dock = DockStyle.Top;
            PresetDescriptionLbl.ForeColor = Color.Silver;
            PresetDescriptionLbl.Location = new Point(0, 30);
            PresetDescriptionLbl.Name = "PresetDescriptionLbl";
            PresetDescriptionLbl.Padding = new Padding(6, 0, 4, 0);
            PresetDescriptionLbl.Size = new Size(750, 18);
            PresetDescriptionLbl.TabIndex = 1;
            // 
            // PresetBarControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            Controls.Add(PresetDescriptionLbl);
            Controls.Add(PresetRow);
            Name = "PresetBarControl";
            Size = new Size(750, 48);
            PresetRow.ResumeLayout(false);
            PresetRow.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel PresetRow;
        private Label PresetLbl;
        private ComboBox PresetComboBox;
        private Button SavePresetBtn;
        private Button DeletePresetBtn;
        private Button ImportPresetBtn;
        private Button OpenPresetFolderBtn;
        private Label PresetDescriptionLbl;
        private ToolTip ToolTipHelper;
    }
}
