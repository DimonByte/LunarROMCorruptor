namespace LunarROMCorruptor.EngineControls
{
    partial class VectorEngineControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VectorEngineControl));
            Vector2EnginePanel = new Panel();
            HeaderLbl = new Label();
            VectorModeLbl = new Label();
            VectorModeComboBox = new ComboBox();
            VectorValuesLbl = new Label();
            VectorValuesTxt = new TextBox();
            VectorNudgeLbl = new Label();
            VectorNudgeNumericUpDown = new NumericUpDown();
            VectorMinLbl = new Label();
            VectorMinNumericUpDown = new NumericUpDown();
            VectorMaxLbl = new Label();
            VectorMaxNumericUpDown = new NumericUpDown();
            VectorPairsChkBox = new CheckBox();
            VectorAlignedChkBox = new CheckBox();
            VectorBigEndianChkBox = new CheckBox();
            VectorIncludeZeroChkBox = new CheckBox();
            VectorSkipCodeChkBox = new CheckBox();
            VectorHelpLbl = new Label();
            Vector2EnginePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)VectorNudgeNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)VectorMinNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)VectorMaxNumericUpDown).BeginInit();
            SuspendLayout();
            // 
            // Vector2EnginePanel
            // 
            Vector2EnginePanel.BackColor = Color.FromArgb(10, 56, 71);
            Vector2EnginePanel.Controls.Add(HeaderLbl);
            Vector2EnginePanel.Controls.Add(VectorModeLbl);
            Vector2EnginePanel.Controls.Add(VectorModeComboBox);
            Vector2EnginePanel.Controls.Add(VectorValuesLbl);
            Vector2EnginePanel.Controls.Add(VectorValuesTxt);
            Vector2EnginePanel.Controls.Add(VectorNudgeLbl);
            Vector2EnginePanel.Controls.Add(VectorNudgeNumericUpDown);
            Vector2EnginePanel.Controls.Add(VectorMinLbl);
            Vector2EnginePanel.Controls.Add(VectorMinNumericUpDown);
            Vector2EnginePanel.Controls.Add(VectorMaxLbl);
            Vector2EnginePanel.Controls.Add(VectorMaxNumericUpDown);
            Vector2EnginePanel.Controls.Add(VectorPairsChkBox);
            Vector2EnginePanel.Controls.Add(VectorAlignedChkBox);
            Vector2EnginePanel.Controls.Add(VectorBigEndianChkBox);
            Vector2EnginePanel.Controls.Add(VectorIncludeZeroChkBox);
            Vector2EnginePanel.Controls.Add(VectorSkipCodeChkBox);
            Vector2EnginePanel.Controls.Add(VectorHelpLbl);
            Vector2EnginePanel.Dock = DockStyle.Fill;
            Vector2EnginePanel.Location = new Point(0, 0);
            Vector2EnginePanel.Name = "Vector2EnginePanel";
            Vector2EnginePanel.Size = new Size(642, 268);
            Vector2EnginePanel.TabIndex = 0;
            Vector2EnginePanel.Tag = "color:normal";
            // 
            // HeaderLbl
            // 
            HeaderLbl.AutoSize = true;
            HeaderLbl.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            HeaderLbl.ForeColor = Color.FromArgb(192, 255, 255);
            HeaderLbl.Location = new Point(3, 2);
            HeaderLbl.Name = "HeaderLbl";
            HeaderLbl.Size = new Size(164, 17);
            HeaderLbl.TabIndex = 0;
            HeaderLbl.Text = "-Vector2 Engine Settings-";
            // 
            // VectorModeLbl
            // 
            VectorModeLbl.AutoSize = true;
            VectorModeLbl.Font = new Font("Segoe UI", 9.75F);
            VectorModeLbl.ForeColor = Color.White;
            VectorModeLbl.Location = new Point(24, 30);
            VectorModeLbl.Name = "VectorModeLbl";
            VectorModeLbl.Size = new Size(105, 17);
            VectorModeLbl.TabIndex = 1;
            VectorModeLbl.Text = "Corruption Type:";
            // 
            // VectorModeComboBox
            // 
            VectorModeComboBox.BackColor = Color.FromArgb(45, 45, 64);
            VectorModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            VectorModeComboBox.FlatStyle = FlatStyle.Flat;
            VectorModeComboBox.ForeColor = Color.White;
            VectorModeComboBox.FormattingEnabled = true;
            VectorModeComboBox.Items.AddRange(new object[] { "REPLACE", "NUDGE", "NEGATE", "RANDOM", "SWAPXY" });
            VectorModeComboBox.Location = new Point(190, 28);
            VectorModeComboBox.Text = "NUDGE";
            VectorModeComboBox.Name = "VectorModeComboBox";
            VectorModeComboBox.Size = new Size(191, 23);
            VectorModeComboBox.TabIndex = 2;
            // 
            // VectorValuesLbl
            // 
            VectorValuesLbl.AutoSize = true;
            VectorValuesLbl.Font = new Font("Segoe UI", 9.75F);
            VectorValuesLbl.ForeColor = Color.White;
            VectorValuesLbl.Location = new Point(24, 58);
            VectorValuesLbl.Name = "VectorValuesLbl";
            VectorValuesLbl.Size = new Size(126, 17);
            VectorValuesLbl.TabIndex = 3;
            VectorValuesLbl.Text = "Replacement values:";
            // 
            // VectorValuesTxt
            // 
            VectorValuesTxt.BackColor = Color.FromArgb(45, 45, 64);
            VectorValuesTxt.BorderStyle = BorderStyle.FixedSingle;
            VectorValuesTxt.ForeColor = Color.White;
            VectorValuesTxt.Location = new Point(190, 56);
            VectorValuesTxt.Name = "VectorValuesTxt";
            VectorValuesTxt.Size = new Size(191, 23);
            VectorValuesTxt.TabIndex = 4;
            VectorValuesTxt.Text = "-1, -0.5, 0, 0.5, 1, 2, 10, 100";
            // 
            // VectorNudgeLbl
            // 
            VectorNudgeLbl.AutoSize = true;
            VectorNudgeLbl.Font = new Font("Segoe UI", 9.75F);
            VectorNudgeLbl.ForeColor = Color.White;
            VectorNudgeLbl.Location = new Point(24, 86);
            VectorNudgeLbl.Name = "VectorNudgeLbl";
            VectorNudgeLbl.Size = new Size(122, 17);
            VectorNudgeLbl.TabIndex = 5;
            VectorNudgeLbl.Text = "Nudge amount (%):";
            // 
            // VectorNudgeNumericUpDown
            // 
            VectorNudgeNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            VectorNudgeNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            VectorNudgeNumericUpDown.ForeColor = Color.White;
            VectorNudgeNumericUpDown.Location = new Point(190, 84);
            VectorNudgeNumericUpDown.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            VectorNudgeNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            VectorNudgeNumericUpDown.Name = "VectorNudgeNumericUpDown";
            VectorNudgeNumericUpDown.Size = new Size(191, 22);
            VectorNudgeNumericUpDown.TabIndex = 6;
            VectorNudgeNumericUpDown.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // VectorMinLbl
            // 
            VectorMinLbl.AutoSize = true;
            VectorMinLbl.Font = new Font("Segoe UI", 9.75F);
            VectorMinLbl.ForeColor = Color.White;
            VectorMinLbl.Location = new Point(24, 114);
            VectorMinLbl.Name = "VectorMinLbl";
            VectorMinLbl.Size = new Size(105, 17);
            VectorMinLbl.TabIndex = 7;
            VectorMinLbl.Text = "Only floats from:";
            // 
            // VectorMinNumericUpDown
            // 
            VectorMinNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            VectorMinNumericUpDown.DecimalPlaces = 4;
            VectorMinNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            VectorMinNumericUpDown.ForeColor = Color.White;
            VectorMinNumericUpDown.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
            VectorMinNumericUpDown.Location = new Point(190, 112);
            VectorMinNumericUpDown.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            VectorMinNumericUpDown.Name = "VectorMinNumericUpDown";
            VectorMinNumericUpDown.Size = new Size(191, 22);
            VectorMinNumericUpDown.TabIndex = 8;
            VectorMinNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 262144 });
            // 
            // VectorMaxLbl
            // 
            VectorMaxLbl.AutoSize = true;
            VectorMaxLbl.Font = new Font("Segoe UI", 9.75F);
            VectorMaxLbl.ForeColor = Color.White;
            VectorMaxLbl.Location = new Point(24, 142);
            VectorMaxLbl.Name = "VectorMaxLbl";
            VectorMaxLbl.Size = new Size(118, 17);
            VectorMaxLbl.TabIndex = 9;
            VectorMaxLbl.Text = "Up to (magnitude):";
            // 
            // VectorMaxNumericUpDown
            // 
            VectorMaxNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            VectorMaxNumericUpDown.DecimalPlaces = 4;
            VectorMaxNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            VectorMaxNumericUpDown.ForeColor = Color.White;
            VectorMaxNumericUpDown.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            VectorMaxNumericUpDown.Location = new Point(190, 140);
            VectorMaxNumericUpDown.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            VectorMaxNumericUpDown.Name = "VectorMaxNumericUpDown";
            VectorMaxNumericUpDown.Size = new Size(191, 22);
            VectorMaxNumericUpDown.TabIndex = 10;
            VectorMaxNumericUpDown.Value = new decimal(new int[] { 100000, 0, 0, 0 });
            // 
            // VectorPairsChkBox
            // 
            VectorPairsChkBox.AutoSize = true;
            VectorPairsChkBox.Checked = true;
            VectorPairsChkBox.CheckState = CheckState.Checked;
            VectorPairsChkBox.ForeColor = Color.White;
            VectorPairsChkBox.Location = new Point(410, 30);
            VectorPairsChkBox.Name = "VectorPairsChkBox";
            VectorPairsChkBox.Size = new Size(164, 19);
            VectorPairsChkBox.TabIndex = 11;
            VectorPairsChkBox.Text = "Treat as Vector2 (X,Y pairs)";
            VectorPairsChkBox.UseVisualStyleBackColor = true;
            // 
            // VectorAlignedChkBox
            // 
            VectorAlignedChkBox.AutoSize = true;
            VectorAlignedChkBox.Checked = true;
            VectorAlignedChkBox.CheckState = CheckState.Checked;
            VectorAlignedChkBox.ForeColor = Color.White;
            VectorAlignedChkBox.Location = new Point(410, 55);
            VectorAlignedChkBox.Name = "VectorAlignedChkBox";
            VectorAlignedChkBox.Size = new Size(160, 19);
            VectorAlignedChkBox.TabIndex = 12;
            VectorAlignedChkBox.Text = "4-byte aligned floats only";
            VectorAlignedChkBox.UseVisualStyleBackColor = true;
            // 
            // VectorBigEndianChkBox
            // 
            VectorBigEndianChkBox.AutoSize = true;
            VectorBigEndianChkBox.ForeColor = Color.White;
            VectorBigEndianChkBox.Location = new Point(410, 80);
            VectorBigEndianChkBox.Name = "VectorBigEndianChkBox";
            VectorBigEndianChkBox.Size = new Size(180, 19);
            VectorBigEndianChkBox.TabIndex = 13;
            VectorBigEndianChkBox.Text = "Big-endian (GC/Wii/PS3/360)";
            VectorBigEndianChkBox.UseVisualStyleBackColor = true;
            // 
            // VectorIncludeZeroChkBox
            // 
            VectorIncludeZeroChkBox.AutoSize = true;
            VectorIncludeZeroChkBox.ForeColor = Color.White;
            VectorIncludeZeroChkBox.Location = new Point(410, 105);
            VectorIncludeZeroChkBox.Name = "VectorIncludeZeroChkBox";
            VectorIncludeZeroChkBox.Size = new Size(119, 19);
            VectorIncludeZeroChkBox.TabIndex = 14;
            VectorIncludeZeroChkBox.Text = "Include 0.0 values";
            VectorIncludeZeroChkBox.UseVisualStyleBackColor = true;
            // 
            // VectorSkipCodeChkBox
            // 
            VectorSkipCodeChkBox.AutoSize = true;
            VectorSkipCodeChkBox.Checked = true;
            VectorSkipCodeChkBox.CheckState = CheckState.Checked;
            VectorSkipCodeChkBox.ForeColor = Color.White;
            VectorSkipCodeChkBox.Location = new Point(410, 130);
            VectorSkipCodeChkBox.Name = "VectorSkipCodeChkBox";
            VectorSkipCodeChkBox.Size = new Size(178, 19);
            VectorSkipCodeChkBox.TabIndex = 16;
            VectorSkipCodeChkBox.Text = "Skip code / compressed data";
            VectorSkipCodeChkBox.UseVisualStyleBackColor = true;
            // 
            // VectorHelpLbl
            // 
            VectorHelpLbl.ForeColor = Color.Silver;
            VectorHelpLbl.Location = new Point(24, 172);
            VectorHelpLbl.Name = "VectorHelpLbl";
            VectorHelpLbl.Size = new Size(600, 90);
            VectorHelpLbl.TabIndex = 15;
            VectorHelpLbl.Text = resources.GetString("VectorHelpLbl.Text");
            // 
            // VectorEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            Controls.Add(Vector2EnginePanel);
            Name = "VectorEngineControl";
            Size = new Size(642, 268);
            Vector2EnginePanel.ResumeLayout(false);
            Vector2EnginePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)VectorNudgeNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)VectorMinNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)VectorMaxNumericUpDown).EndInit();
            ResumeLayout(false);
        }

        #endregion

        public Panel Vector2EnginePanel;
        private Label HeaderLbl;
        private Label VectorModeLbl;
        internal ComboBox VectorModeComboBox;
        private Label VectorValuesLbl;
        internal TextBox VectorValuesTxt;
        private Label VectorNudgeLbl;
        internal NumericUpDown VectorNudgeNumericUpDown;
        private Label VectorMinLbl;
        internal NumericUpDown VectorMinNumericUpDown;
        private Label VectorMaxLbl;
        internal NumericUpDown VectorMaxNumericUpDown;
        internal CheckBox VectorPairsChkBox;
        internal CheckBox VectorAlignedChkBox;
        internal CheckBox VectorBigEndianChkBox;
        internal CheckBox VectorIncludeZeroChkBox;
        internal CheckBox VectorSkipCodeChkBox;
        private Label VectorHelpLbl;
    }
}

