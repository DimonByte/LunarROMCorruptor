namespace LunarROMCorruptor.EngineControls
{
    partial class CompressionEngineControl
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
            CompressionEnginePanel = new Panel();
            HeaderLbl = new Label();
            CompressionOptionLbl = new Label();
            CompressionOptionComboBox = new ComboBox();
            CompressionTiltLbl = new Label();
            CompressionTiltNumericUpDown = new NumericUpDown();
            CompressionScanBtn = new Button();
            CompressionScanResultLbl = new Label();
            CompressionFormatsLbl = new Label();
            CompressionYaz0ChkBox = new CheckBox();
            CompressionYay0ChkBox = new CheckBox();
            CompressionMio0ChkBox = new CheckBox();
            CompressionLz10ChkBox = new CheckBox();
            CompressionLz11ChkBox = new CheckBox();
            CompressionHelpLbl = new Label();
            ((System.ComponentModel.ISupportInitialize)CompressionTiltNumericUpDown).BeginInit();
            CompressionEnginePanel.SuspendLayout();
            SuspendLayout();
            // 
            // CompressionEnginePanel
            // 
            CompressionEnginePanel.BackColor = Color.FromArgb(10, 56, 71);
            CompressionEnginePanel.Controls.Add(HeaderLbl);
            CompressionEnginePanel.Controls.Add(CompressionOptionLbl);
            CompressionEnginePanel.Controls.Add(CompressionOptionComboBox);
            CompressionEnginePanel.Controls.Add(CompressionTiltLbl);
            CompressionEnginePanel.Controls.Add(CompressionTiltNumericUpDown);
            CompressionEnginePanel.Controls.Add(CompressionScanBtn);
            CompressionEnginePanel.Controls.Add(CompressionScanResultLbl);
            CompressionEnginePanel.Controls.Add(CompressionFormatsLbl);
            CompressionEnginePanel.Controls.Add(CompressionYaz0ChkBox);
            CompressionEnginePanel.Controls.Add(CompressionYay0ChkBox);
            CompressionEnginePanel.Controls.Add(CompressionMio0ChkBox);
            CompressionEnginePanel.Controls.Add(CompressionLz10ChkBox);
            CompressionEnginePanel.Controls.Add(CompressionLz11ChkBox);
            CompressionEnginePanel.Controls.Add(CompressionHelpLbl);
            CompressionEnginePanel.Dock = DockStyle.Fill;
            CompressionEnginePanel.Location = new Point(0, 0);
            CompressionEnginePanel.Name = "CompressionEnginePanel";
            CompressionEnginePanel.Size = new Size(642, 268);
            CompressionEnginePanel.TabIndex = 0;
            CompressionEnginePanel.Tag = "color:normal";
            // 
            // HeaderLbl
            // 
            HeaderLbl.AutoSize = true;
            HeaderLbl.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            HeaderLbl.ForeColor = Color.FromArgb(192, 255, 255);
            HeaderLbl.Location = new Point(3, 2);
            HeaderLbl.Text = "-Compression Engine Settings-";
            HeaderLbl.Name = "HeaderLbl";
            HeaderLbl.TabIndex = 1;
            // 
            // CompressionOptionLbl
            // 
            CompressionOptionLbl.AutoSize = true;
            CompressionOptionLbl.Font = new Font("Segoe UI", 9.75F);
            CompressionOptionLbl.ForeColor = Color.White;
            CompressionOptionLbl.Location = new Point(24, 30);
            CompressionOptionLbl.Text = "Corruption Type:";
            CompressionOptionLbl.Name = "CompressionOptionLbl";
            CompressionOptionLbl.TabIndex = 2;
            // 
            // CompressionOptionComboBox
            // 
            CompressionOptionComboBox.BackColor = Color.FromArgb(45, 45, 64);
            CompressionOptionComboBox.FlatStyle = FlatStyle.Flat;
            CompressionOptionComboBox.ForeColor = Color.White;
            CompressionOptionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            CompressionOptionComboBox.FormattingEnabled = true;
            CompressionOptionComboBox.Items.AddRange(new object[] { "RANDOM", "RANDOMTILT", "TILT" });
            CompressionOptionComboBox.Location = new Point(190, 28);
            CompressionOptionComboBox.Size = new Size(191, 23);
            CompressionOptionComboBox.SelectedIndex = 0;
            CompressionOptionComboBox.Name = "CompressionOptionComboBox";
            CompressionOptionComboBox.TabIndex = 3;
            // 
            // CompressionTiltLbl
            // 
            CompressionTiltLbl.AutoSize = true;
            CompressionTiltLbl.Font = new Font("Segoe UI", 9.75F);
            CompressionTiltLbl.ForeColor = Color.White;
            CompressionTiltLbl.Location = new Point(24, 58);
            CompressionTiltLbl.Text = "Tilt amount:";
            CompressionTiltLbl.Name = "CompressionTiltLbl";
            CompressionTiltLbl.TabIndex = 4;
            // 
            // CompressionTiltNumericUpDown
            // 
            CompressionTiltNumericUpDown.BackColor = Color.FromArgb(45, 45, 64);
            CompressionTiltNumericUpDown.ForeColor = Color.White;
            CompressionTiltNumericUpDown.Location = new Point(190, 56);
            CompressionTiltNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            CompressionTiltNumericUpDown.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            CompressionTiltNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            CompressionTiltNumericUpDown.Size = new Size(191, 23);
            CompressionTiltNumericUpDown.Name = "CompressionTiltNumericUpDown";
            CompressionTiltNumericUpDown.TabIndex = 5;
            // 
            // CompressionScanBtn
            // 
            CompressionScanBtn.BackColor = Color.FromArgb(52, 60, 111);
            CompressionScanBtn.FlatStyle = FlatStyle.Flat;
            CompressionScanBtn.ForeColor = Color.FromArgb(213, 216, 216);
            CompressionScanBtn.Location = new Point(24, 92);
            CompressionScanBtn.Size = new Size(150, 27);
            CompressionScanBtn.Text = "Scan for blocks";
            CompressionScanBtn.UseVisualStyleBackColor = false;
            CompressionScanBtn.Name = "CompressionScanBtn";
            CompressionScanBtn.TabIndex = 6;
            CompressionScanBtn.Click += CompressionScanBtn_Click;
            // 
            // CompressionScanResultLbl
            // 
            CompressionScanResultLbl.Font = new Font("Segoe UI", 9F);
            CompressionScanResultLbl.ForeColor = Color.Silver;
            CompressionScanResultLbl.Location = new Point(190, 92);
            CompressionScanResultLbl.Size = new Size(210, 70);
            CompressionScanResultLbl.Text = "Scan the loaded file to see which compressed blocks the engine can use.";
            CompressionScanResultLbl.Name = "CompressionScanResultLbl";
            CompressionScanResultLbl.TabIndex = 7;
            // 
            // CompressionFormatsLbl
            // 
            CompressionFormatsLbl.AutoSize = true;
            CompressionFormatsLbl.Font = new Font("Segoe UI", 9.75F);
            CompressionFormatsLbl.ForeColor = Color.White;
            CompressionFormatsLbl.Location = new Point(410, 30);
            CompressionFormatsLbl.Text = "Formats:";
            CompressionFormatsLbl.Name = "CompressionFormatsLbl";
            CompressionFormatsLbl.TabIndex = 8;
            // 
            // CompressionYaz0ChkBox
            // 
            CompressionYaz0ChkBox.AutoSize = true;
            CompressionYaz0ChkBox.ForeColor = Color.White;
            CompressionYaz0ChkBox.Location = new Point(420, 52);
            CompressionYaz0ChkBox.Text = "Yaz0 (N64 / GameCube / Wii)";
            CompressionYaz0ChkBox.UseVisualStyleBackColor = true;
            CompressionYaz0ChkBox.Checked = true;
            CompressionYaz0ChkBox.Name = "CompressionYaz0ChkBox";
            CompressionYaz0ChkBox.TabIndex = 9;
            // 
            // CompressionYay0ChkBox
            // 
            CompressionYay0ChkBox.AutoSize = true;
            CompressionYay0ChkBox.ForeColor = Color.White;
            CompressionYay0ChkBox.Location = new Point(420, 75);
            CompressionYay0ChkBox.Text = "Yay0 (N64)";
            CompressionYay0ChkBox.UseVisualStyleBackColor = true;
            CompressionYay0ChkBox.Checked = true;
            CompressionYay0ChkBox.Name = "CompressionYay0ChkBox";
            CompressionYay0ChkBox.TabIndex = 10;
            // 
            // CompressionMio0ChkBox
            // 
            CompressionMio0ChkBox.AutoSize = true;
            CompressionMio0ChkBox.ForeColor = Color.White;
            CompressionMio0ChkBox.Location = new Point(420, 98);
            CompressionMio0ChkBox.Text = "MIO0 (N64)";
            CompressionMio0ChkBox.UseVisualStyleBackColor = true;
            CompressionMio0ChkBox.Checked = true;
            CompressionMio0ChkBox.Name = "CompressionMio0ChkBox";
            CompressionMio0ChkBox.TabIndex = 11;
            // 
            // CompressionLz10ChkBox
            // 
            CompressionLz10ChkBox.AutoSize = true;
            CompressionLz10ChkBox.ForeColor = Color.White;
            CompressionLz10ChkBox.Location = new Point(420, 121);
            CompressionLz10ChkBox.Text = "LZ10 (GBA / DS)";
            CompressionLz10ChkBox.UseVisualStyleBackColor = true;
            CompressionLz10ChkBox.Checked = true;
            CompressionLz10ChkBox.Name = "CompressionLz10ChkBox";
            CompressionLz10ChkBox.TabIndex = 12;
            // 
            // CompressionLz11ChkBox
            // 
            CompressionLz11ChkBox.AutoSize = true;
            CompressionLz11ChkBox.ForeColor = Color.White;
            CompressionLz11ChkBox.Location = new Point(420, 144);
            CompressionLz11ChkBox.Text = "LZ11 (DS)";
            CompressionLz11ChkBox.UseVisualStyleBackColor = true;
            CompressionLz11ChkBox.Checked = true;
            CompressionLz11ChkBox.Name = "CompressionLz11ChkBox";
            CompressionLz11ChkBox.TabIndex = 13;
            // 
            // CompressionHelpLbl
            // 
            CompressionHelpLbl.Font = new Font("Segoe UI", 9F);
            CompressionHelpLbl.ForeColor = Color.Silver;
            CompressionHelpLbl.Location = new Point(24, 172);
            CompressionHelpLbl.Size = new Size(600, 92);
            CompressionHelpLbl.Text = "Unpacks compressed blocks, corrupts the real data inside and packs it back into the same space, so textures, models and levels glitch instead of the decompressor crashing. Intensity is spread over the blocks in the byte range; in 'Every Nth' mode N counts unpacked bytes. If a block no longer fits, fewer corruptions are tried, then only its literal bytes are changed.";
            CompressionHelpLbl.Name = "CompressionHelpLbl";
            CompressionHelpLbl.TabIndex = 14;
            // 
            // CompressionEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            Controls.Add(CompressionEnginePanel);
            Name = "CompressionEngineControl";
            Size = new Size(642, 268);
            ((System.ComponentModel.ISupportInitialize)CompressionTiltNumericUpDown).EndInit();
            CompressionEnginePanel.ResumeLayout(false);
            CompressionEnginePanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public Panel CompressionEnginePanel;
        private Label HeaderLbl;
        private Label CompressionOptionLbl;
        internal ComboBox CompressionOptionComboBox;
        private Label CompressionTiltLbl;
        internal NumericUpDown CompressionTiltNumericUpDown;
        private Button CompressionScanBtn;
        internal Label CompressionScanResultLbl;
        private Label CompressionFormatsLbl;
        internal CheckBox CompressionYaz0ChkBox;
        internal CheckBox CompressionYay0ChkBox;
        internal CheckBox CompressionMio0ChkBox;
        internal CheckBox CompressionLz10ChkBox;
        internal CheckBox CompressionLz11ChkBox;
        private Label CompressionHelpLbl;
    }
}
