namespace LunarROMCorruptor.EngineControls
{
    partial class DatamoshEngineControl
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
            DatamoshEnginePanel = new Panel();
            HeaderLbl = new Label();
            MoshModeLbl = new Label();
            MoshModeComboBox = new ComboBox();
            MoshKeepLbl = new Label();
            MoshKeepNumericUpDown = new NumericUpDown();
            MoshRepeatLbl = new Label();
            MoshRepeatNumericUpDown = new NumericUpDown();
            MoshReplaceChkBox = new CheckBox();
            MoshHelpLbl = new Label();
            MoshPresetLbl = new Label();
            ((System.ComponentModel.ISupportInitialize)MoshKeepNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MoshRepeatNumericUpDown).BeginInit();
            DatamoshEnginePanel.SuspendLayout();
            SuspendLayout();
            // 
            // DatamoshEnginePanel
            // 
            DatamoshEnginePanel.BackColor = Color.FromArgb(40, 40, 60);
            DatamoshEnginePanel.Controls.Add(HeaderLbl);
            DatamoshEnginePanel.Controls.Add(MoshModeLbl);
            DatamoshEnginePanel.Controls.Add(MoshModeComboBox);
            DatamoshEnginePanel.Controls.Add(MoshKeepLbl);
            DatamoshEnginePanel.Controls.Add(MoshKeepNumericUpDown);
            DatamoshEnginePanel.Controls.Add(MoshRepeatLbl);
            DatamoshEnginePanel.Controls.Add(MoshRepeatNumericUpDown);
            DatamoshEnginePanel.Controls.Add(MoshReplaceChkBox);
            DatamoshEnginePanel.Controls.Add(MoshHelpLbl);
            DatamoshEnginePanel.Controls.Add(MoshPresetLbl);
            DatamoshEnginePanel.Dock = DockStyle.Fill;
            DatamoshEnginePanel.Location = new Point(0, 0);
            DatamoshEnginePanel.Name = "DatamoshEnginePanel";
            DatamoshEnginePanel.Size = new Size(642, 268);
            DatamoshEnginePanel.TabIndex = 0;
            DatamoshEnginePanel.Tag = "color:normal";
            // 
            // HeaderLbl
            // 
            HeaderLbl.AutoSize = true;
            HeaderLbl.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            HeaderLbl.ForeColor = Color.FromArgb(192, 255, 255);
            HeaderLbl.Location = new Point(3, 2);
            HeaderLbl.Name = "HeaderLbl";
            HeaderLbl.TabIndex = 0;
            HeaderLbl.Text = "-Datamosh Engine Settings-";
            // 
            // MoshModeLbl
            // 
            MoshModeLbl.AutoSize = true;
            MoshModeLbl.Font = new Font("Segoe UI", 9.75F);
            MoshModeLbl.ForeColor = Color.White;
            MoshModeLbl.Location = new Point(24, 30);
            MoshModeLbl.Text = "Mosh type:";
            MoshModeLbl.Name = "MoshModeLbl";
            MoshModeLbl.TabIndex = 1;
            // 
            // MoshModeComboBox
            // 
            MoshModeComboBox.BackColor = Color.FromArgb(45, 45, 64);
            MoshModeComboBox.FlatStyle = FlatStyle.Flat;
            MoshModeComboBox.ForeColor = Color.White;
            MoshModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            MoshModeComboBox.FormattingEnabled = true;
            MoshModeComboBox.Items.AddRange(new object[] { "REMOVE_KEYFRAMES", "DUPLICATE_FRAMES", "BOTH" });
            MoshModeComboBox.Location = new Point(190, 28);
            MoshModeComboBox.Size = new Size(191, 23);
            MoshModeComboBox.SelectedIndex = 0;
            MoshModeComboBox.Name = "MoshModeComboBox";
            MoshModeComboBox.TabIndex = 2;
            // 
            // MoshKeepLbl
            // 
            MoshKeepLbl.AutoSize = true;
            MoshKeepLbl.Font = new Font("Segoe UI", 9.75F);
            MoshKeepLbl.ForeColor = Color.White;
            MoshKeepLbl.Location = new Point(24, 58);
            MoshKeepLbl.Text = "Keep first keyframes:";
            MoshKeepLbl.Name = "MoshKeepLbl";
            MoshKeepLbl.TabIndex = 3;
            // 
            // MoshKeepNumericUpDown
            // 
            MoshKeepNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            MoshKeepNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            MoshKeepNumericUpDown.ForeColor = Color.White;
            MoshKeepNumericUpDown.Location = new Point(190, 56);
            MoshKeepNumericUpDown.DecimalPlaces = 0;
            MoshKeepNumericUpDown.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            MoshKeepNumericUpDown.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            MoshKeepNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            MoshKeepNumericUpDown.Size = new Size(191, 23);
            MoshKeepNumericUpDown.Name = "MoshKeepNumericUpDown";
            MoshKeepNumericUpDown.TabIndex = 4;
            // 
            // MoshRepeatLbl
            // 
            MoshRepeatLbl.AutoSize = true;
            MoshRepeatLbl.Font = new Font("Segoe UI", 9.75F);
            MoshRepeatLbl.ForeColor = Color.White;
            MoshRepeatLbl.Location = new Point(24, 86);
            MoshRepeatLbl.Text = "Repeat each frame (times):";
            MoshRepeatLbl.Name = "MoshRepeatLbl";
            MoshRepeatLbl.TabIndex = 5;
            // 
            // MoshRepeatNumericUpDown
            // 
            MoshRepeatNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            MoshRepeatNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            MoshRepeatNumericUpDown.ForeColor = Color.White;
            MoshRepeatNumericUpDown.Location = new Point(190, 84);
            MoshRepeatNumericUpDown.DecimalPlaces = 0;
            MoshRepeatNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            MoshRepeatNumericUpDown.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            MoshRepeatNumericUpDown.Value = new decimal(new int[] { 10, 0, 0, 0 });
            MoshRepeatNumericUpDown.Size = new Size(191, 23);
            MoshRepeatNumericUpDown.Name = "MoshRepeatNumericUpDown";
            MoshRepeatNumericUpDown.TabIndex = 6;
            // 
            // MoshReplaceChkBox
            // 
            MoshReplaceChkBox.AutoSize = true;
            MoshReplaceChkBox.ForeColor = Color.White;
            MoshReplaceChkBox.Location = new Point(24, 114);
            MoshReplaceChkBox.Text = "Replace removed keyframes with the previous frame (keeps audio in sync)";
            MoshReplaceChkBox.UseVisualStyleBackColor = true;
            MoshReplaceChkBox.Checked = true;
            MoshReplaceChkBox.Name = "MoshReplaceChkBox";
            MoshReplaceChkBox.TabIndex = 7;
            // 
            // MoshHelpLbl
            // 
            MoshHelpLbl.ForeColor = Color.Silver;
            MoshHelpLbl.Location = new Point(24, 140);
            MoshHelpLbl.Size = new Size(600, 48);
            MoshHelpLbl.Text = "Datamoshing for AVI videos (Xvid/DivX/MPEG-4 works best; H.264 AVIs work with DUPLICATE_FRAMES, but players may skip frames after removed keyframes). REMOVE_KEYFRAMES deletes keyframes so the motion of the next frames smears over the old picture (the classic melt). DUPLICATE_FRAMES repeats motion frames so the movement keeps pushing the pixels (bloom). Every Nth = every Nth keyframe/frame (Every Nth = 1 removes them all), Intensity = that many random ones.";
            MoshHelpLbl.Name = "MoshHelpLbl";
            MoshHelpLbl.TabIndex = 8;
            // 
            // MoshPresetLbl
            // 
            MoshPresetLbl.ForeColor = Color.Silver;
            MoshPresetLbl.Location = new Point(24, 192);
            MoshPresetLbl.Size = new Size(600, 70);
            MoshPresetLbl.Text = "Preset: convert your video first with ffmpeg -i input.mp4 -c:v mpeg4 -vtag XVID -q:v 2 -g 600 -bf 0 -c:a libmp3lame output.avi (few keyframes, no B-frames). Then use REMOVE_KEYFRAMES + Every Nth Byte = 1 + Keep first keyframes = 1. Play the result in VLC/mpv or bake it with ffmpeg -i moshed.avi -c:v libx264 -crf 18 final.mp4";
            MoshPresetLbl.Name = "MoshPresetLbl";
            MoshPresetLbl.TabIndex = 9;
            // 
            // DatamoshEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            Controls.Add(DatamoshEnginePanel);
            Name = "DatamoshEngineControl";
            Size = new Size(642, 268);
            ((System.ComponentModel.ISupportInitialize)MoshKeepNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)MoshRepeatNumericUpDown).EndInit();
            DatamoshEnginePanel.ResumeLayout(false);
            DatamoshEnginePanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public Panel DatamoshEnginePanel;
        private Label HeaderLbl;
        private Label MoshModeLbl;
        internal ComboBox MoshModeComboBox;
        private Label MoshKeepLbl;
        internal NumericUpDown MoshKeepNumericUpDown;
        private Label MoshRepeatLbl;
        internal NumericUpDown MoshRepeatNumericUpDown;
        internal CheckBox MoshReplaceChkBox;
        private Label MoshHelpLbl;
        private Label MoshPresetLbl;
    }
}

