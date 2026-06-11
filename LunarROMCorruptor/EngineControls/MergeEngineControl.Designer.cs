namespace LunarROMCorruptor.EngineControls
{
    partial class MergeEngineControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MergeEngineControl));
            MergeEnginePanel = new Panel();
            CorrTypeMerge = new ComboBox();
            Mod256MergeEnginechkbox = new CheckBox();
            ReplaceByteWithSamePos = new CheckBox();
            Label15 = new Label();
            Label13 = new Label();
            MergeOpenFilebtn = new Button();
            MergeFileLocationTxt = new TextBox();
            Label9 = new Label();
            openFileDialog1 = new OpenFileDialog();
            HelpAboutEngineLbl = new Label();
            MergeEnginePanel.SuspendLayout();
            SuspendLayout();
            // 
            // MergeEnginePanel
            // 
            MergeEnginePanel.BackColor = Color.Teal;
            MergeEnginePanel.Controls.Add(HelpAboutEngineLbl);
            MergeEnginePanel.Controls.Add(CorrTypeMerge);
            MergeEnginePanel.Controls.Add(Mod256MergeEnginechkbox);
            MergeEnginePanel.Controls.Add(ReplaceByteWithSamePos);
            MergeEnginePanel.Controls.Add(Label15);
            MergeEnginePanel.Controls.Add(Label13);
            MergeEnginePanel.Controls.Add(MergeOpenFilebtn);
            MergeEnginePanel.Controls.Add(MergeFileLocationTxt);
            MergeEnginePanel.Controls.Add(Label9);
            MergeEnginePanel.Dock = DockStyle.Fill;
            MergeEnginePanel.Location = new Point(0, 0);
            MergeEnginePanel.Name = "MergeEnginePanel";
            MergeEnginePanel.Size = new Size(642, 268);
            MergeEnginePanel.TabIndex = 159;
            MergeEnginePanel.Tag = "color:normal";
            // 
            // CorrTypeMerge
            // 
            CorrTypeMerge.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            CorrTypeMerge.BackColor = Color.FromArgb(45, 45, 64);
            CorrTypeMerge.FlatStyle = FlatStyle.Flat;
            CorrTypeMerge.ForeColor = Color.White;
            CorrTypeMerge.FormattingEnabled = true;
            CorrTypeMerge.Items.AddRange(new object[] { "NONE", "RANGE" });
            CorrTypeMerge.Location = new Point(127, 112);
            CorrTypeMerge.Name = "CorrTypeMerge";
            CorrTypeMerge.Size = new Size(124, 23);
            CorrTypeMerge.TabIndex = 99;
            CorrTypeMerge.Text = "NONE";
            // 
            // Mod256MergeEnginechkbox
            // 
            Mod256MergeEnginechkbox.AutoSize = true;
            Mod256MergeEnginechkbox.BackColor = Color.Transparent;
            Mod256MergeEnginechkbox.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Mod256MergeEnginechkbox.ForeColor = Color.White;
            Mod256MergeEnginechkbox.Location = new Point(403, 69);
            Mod256MergeEnginechkbox.Name = "Mod256MergeEnginechkbox";
            Mod256MergeEnginechkbox.Size = new Size(71, 17);
            Mod256MergeEnginechkbox.TabIndex = 98;
            Mod256MergeEnginechkbox.Text = "Mod 256";
            Mod256MergeEnginechkbox.UseVisualStyleBackColor = false;
            // 
            // ReplaceByteWithSamePos
            // 
            ReplaceByteWithSamePos.AutoSize = true;
            ReplaceByteWithSamePos.BackColor = Color.Transparent;
            ReplaceByteWithSamePos.Checked = true;
            ReplaceByteWithSamePos.CheckState = CheckState.Checked;
            ReplaceByteWithSamePos.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ReplaceByteWithSamePos.ForeColor = Color.White;
            ReplaceByteWithSamePos.Location = new Point(127, 69);
            ReplaceByteWithSamePos.Name = "ReplaceByteWithSamePos";
            ReplaceByteWithSamePos.Size = new Size(270, 17);
            ReplaceByteWithSamePos.TabIndex = 98;
            ReplaceByteWithSamePos.Text = "Replace byte with the byte at the same position";
            ReplaceByteWithSamePos.UseVisualStyleBackColor = false;
            // 
            // Label15
            // 
            Label15.AutoSize = true;
            Label15.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Label15.ForeColor = Color.FromArgb(224, 224, 224);
            Label15.Location = new Point(123, 89);
            Label15.Name = "Label15";
            Label15.Size = new Size(124, 20);
            Label15.TabIndex = 97;
            Label15.Text = "Corruption Type:";
            // 
            // Label13
            // 
            Label13.AutoSize = true;
            Label13.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Label13.ForeColor = Color.FromArgb(224, 224, 224);
            Label13.Location = new Point(123, 19);
            Label13.Name = "Label13";
            Label13.Size = new Size(103, 20);
            Label13.TabIndex = 97;
            Label13.Text = "File Selection:";
            // 
            // MergeOpenFilebtn
            // 
            MergeOpenFilebtn.BackColor = Color.FromArgb(60, 60, 80);
            MergeOpenFilebtn.FlatAppearance.BorderSize = 0;
            MergeOpenFilebtn.FlatStyle = FlatStyle.Flat;
            MergeOpenFilebtn.ForeColor = Color.Aqua;
            MergeOpenFilebtn.Image = Properties.Resources.upload;
            MergeOpenFilebtn.ImageAlign = ContentAlignment.MiddleLeft;
            MergeOpenFilebtn.Location = new Point(5, 41);
            MergeOpenFilebtn.Name = "MergeOpenFilebtn";
            MergeOpenFilebtn.Size = new Size(116, 23);
            MergeOpenFilebtn.TabIndex = 96;
            MergeOpenFilebtn.Text = "Open File";
            MergeOpenFilebtn.UseVisualStyleBackColor = false;
            MergeOpenFilebtn.Click += MergeOpenFilebtn_Click;
            // 
            // MergeFileLocationTxt
            // 
            MergeFileLocationTxt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            MergeFileLocationTxt.BackColor = Color.FromArgb(67, 67, 81);
            MergeFileLocationTxt.BorderStyle = BorderStyle.FixedSingle;
            MergeFileLocationTxt.ForeColor = Color.White;
            MergeFileLocationTxt.Location = new Point(127, 42);
            MergeFileLocationTxt.Name = "MergeFileLocationTxt";
            MergeFileLocationTxt.ReadOnly = true;
            MergeFileLocationTxt.Size = new Size(463, 23);
            MergeFileLocationTxt.TabIndex = 94;
            // 
            // Label9
            // 
            Label9.AutoSize = true;
            Label9.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            Label9.ForeColor = Color.FromArgb(192, 255, 255);
            Label9.Location = new Point(3, 2);
            Label9.Name = "Label9";
            Label9.Size = new Size(157, 17);
            Label9.TabIndex = 93;
            Label9.Text = "-Merge Engine Settings-";
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // HelpAboutEngineLbl
            // 
            HelpAboutEngineLbl.ForeColor = Color.White;
            HelpAboutEngineLbl.Location = new Point(19, 149);
            HelpAboutEngineLbl.Name = "HelpAboutEngineLbl";
            HelpAboutEngineLbl.Size = new Size(571, 100);
            HelpAboutEngineLbl.TabIndex = 100;
            HelpAboutEngineLbl.Text = resources.GetString("HelpAboutEngineLbl.Text");
            // 
            // MergeEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(MergeEnginePanel);
            Name = "MergeEngineControl";
            Size = new Size(642, 268);
            MergeEnginePanel.ResumeLayout(false);
            MergeEnginePanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public Panel MergeEnginePanel;
        public ComboBox CorrTypeMerge;
        public CheckBox Mod256MergeEnginechkbox;
        public CheckBox ReplaceByteWithSamePos;
        internal Label Label15;
        internal Label Label13;
        internal Button MergeOpenFilebtn;
        public TextBox MergeFileLocationTxt;
        private Label Label9;
        private OpenFileDialog openFileDialog1;
        private Label HelpAboutEngineLbl;
    }
}
