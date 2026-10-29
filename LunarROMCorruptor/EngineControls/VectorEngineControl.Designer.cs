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
            VectorHelpLbl = new Label();
            ((System.ComponentModel.ISupportInitialize)VectorNudgeNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)VectorMinNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)VectorMaxNumericUpDown).BeginInit();
            Vector2EnginePanel.SuspendLayout();
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
            HeaderLbl.TabIndex = 0;
            HeaderLbl.Text = "-Vector2 Engine Settings-";
            // 
            // VectorModeLbl
            // 
            VectorModeLbl.AutoSize = true;
            VectorModeLbl.Font = new Font("Segoe UI", 9.75F);
            VectorModeLbl.ForeColor = Color.White;
            VectorModeLbl.Location = new Point(24, 30);
            VectorModeLbl.Text = "Corruption Type:";
            VectorModeLbl.Name = "VectorModeLbl";
            VectorModeLbl.TabIndex = 1;
            // 
            // VectorModeComboBox
            // 
            VectorModeComboBox.BackColor = Color.FromArgb(45, 45, 64);
            VectorModeComboBox.FlatStyle = FlatStyle.Flat;
            VectorModeComboBox.ForeColor = Color.White;
            VectorModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            VectorModeComboBox.FormattingEnabled = true;
            VectorModeComboBox.Items.AddRange(new object[] { "REPLACE", "NUDGE", "NEGATE", "RANDOM", "SWAPXY" });
            VectorModeComboBox.Location = new Point(190, 28);
            VectorModeComboBox.Size = new Size(191, 23);
            VectorModeComboBox.SelectedIndex = 1;
            VectorModeComboBox.Name = "VectorModeComboBox";
            VectorModeComboBox.TabIndex = 2;
            // 
            // VectorValuesLbl
            // 
            VectorValuesLbl.AutoSize = true;
            VectorValuesLbl.Font = new Font("Segoe UI", 9.75F);
            VectorValuesLbl.ForeColor = Color.White;
            VectorValuesLbl.Location = new Point(24, 58);
            VectorValuesLbl.Text = "Replacement values:";
            VectorValuesLbl.Name = "VectorValuesLbl";
            VectorValuesLbl.TabIndex = 3;
            // 
            // VectorValuesTxt
            // 
            VectorValuesTxt.BackColor = Color.FromArgb(45, 45, 64);
            VectorValuesTxt.BorderStyle = BorderStyle.FixedSingle;
            VectorValuesTxt.ForeColor = Color.White;
            VectorValuesTxt.Location = new Point(190, 56);
            VectorValuesTxt.Size = new Size(191, 23);
            VectorValuesTxt.Text = "-1, -0.5, 0, 0.5, 1, 2, 10, 100";
            VectorValuesTxt.Name = "VectorValuesTxt";
            VectorValuesTxt.TabIndex = 4;
            // 
            // VectorNudgeLbl
            // 
            VectorNudgeLbl.AutoSize = true;
            VectorNudgeLbl.Font = new Font("Segoe UI", 9.75F);
            VectorNudgeLbl.ForeColor = Color.White;
            VectorNudgeLbl.Location = new Point(24, 86);
            VectorNudgeLbl.Text = "Nudge amount (%):";
            VectorNudgeLbl.Name = "VectorNudgeLbl";
            VectorNudgeLbl.TabIndex = 5;
            // 
            // VectorNudgeNumericUpDown
            // 
            VectorNudgeNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            VectorNudgeNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            VectorNudgeNumericUpDown.ForeColor = Color.White;
            VectorNudgeNumericUpDown.Location = new Point(190, 84);
            VectorNudgeNumericUpDown.DecimalPlaces = 0;
            VectorNudgeNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            VectorNudgeNumericUpDown.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            VectorNudgeNumericUpDown.Value = new decimal(new int[] { 25, 0, 0, 0 });
            VectorNudgeNumericUpDown.Size = new Size(191, 23);
            VectorNudgeNumericUpDown.Name = "VectorNudgeNumericUpDown";
            VectorNudgeNumericUpDown.TabIndex = 6;
            // 
            // VectorMinLbl
            // 
            VectorMinLbl.AutoSize = true;
            VectorMinLbl.Font = new Font("Segoe UI", 9.75F);
            VectorMinLbl.ForeColor = Color.White;
            VectorMinLbl.Location = new Point(24, 114);
            VectorMinLbl.Text = "Only floats from:";
            VectorMinLbl.Name = "VectorMinLbl";
            VectorMinLbl.TabIndex = 7;
            // 
            // VectorMinNumericUpDown
            // 
            VectorMinNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            VectorMinNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            VectorMinNumericUpDown.ForeColor = Color.White;
            VectorMinNumericUpDown.Location = new Point(190, 112);
            VectorMinNumericUpDown.DecimalPlaces = 4;
            VectorMinNumericUpDown.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            VectorMinNumericUpDown.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            VectorMinNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 262144 });
            VectorMinNumericUpDown.Size = new Size(191, 23);
            VectorMinNumericUpDown.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
            VectorMinNumericUpDown.Name = "VectorMinNumericUpDown";
            VectorMinNumericUpDown.TabIndex = 8;
            // 
            // VectorMaxLbl
            // 
            VectorMaxLbl.AutoSize = true;
            VectorMaxLbl.Font = new Font("Segoe UI", 9.75F);
            VectorMaxLbl.ForeColor = Color.White;
            VectorMaxLbl.Location = new Point(24, 142);
            VectorMaxLbl.Text = "Up to (magnitude):";
            VectorMaxLbl.Name = "VectorMaxLbl";
            VectorMaxLbl.TabIndex = 9;
            // 
            // VectorMaxNumericUpDown
            // 
            VectorMaxNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            VectorMaxNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            VectorMaxNumericUpDown.ForeColor = Color.White;
            VectorMaxNumericUpDown.Location = new Point(190, 140);
            VectorMaxNumericUpDown.DecimalPlaces = 4;
            VectorMaxNumericUpDown.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            VectorMaxNumericUpDown.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            VectorMaxNumericUpDown.Value = new decimal(new int[] { 100000, 0, 0, 0 });
            VectorMaxNumericUpDown.Size = new Size(191, 23);
            VectorMaxNumericUpDown.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            VectorMaxNumericUpDown.Name = "VectorMaxNumericUpDown";
            VectorMaxNumericUpDown.TabIndex = 10;
            // 
            // VectorPairsChkBox
            // 
            VectorPairsChkBox.AutoSize = true;
            VectorPairsChkBox.ForeColor = Color.White;
            VectorPairsChkBox.Location = new Point(410, 30);
            VectorPairsChkBox.Text = "Treat as Vector2 (X,Y pairs)";
            VectorPairsChkBox.UseVisualStyleBackColor = true;
            VectorPairsChkBox.Checked = true;
            VectorPairsChkBox.Name = "VectorPairsChkBox";
            VectorPairsChkBox.TabIndex = 11;
            // 
            // VectorAlignedChkBox
            // 
            VectorAlignedChkBox.AutoSize = true;
            VectorAlignedChkBox.ForeColor = Color.White;
            VectorAlignedChkBox.Location = new Point(410, 55);
            VectorAlignedChkBox.Text = "4-byte aligned floats only";
            VectorAlignedChkBox.UseVisualStyleBackColor = true;
            VectorAlignedChkBox.Checked = true;
            VectorAlignedChkBox.Name = "VectorAlignedChkBox";
            VectorAlignedChkBox.TabIndex = 12;
            // 
            // VectorBigEndianChkBox
            // 
            VectorBigEndianChkBox.AutoSize = true;
            VectorBigEndianChkBox.ForeColor = Color.White;
            VectorBigEndianChkBox.Location = new Point(410, 80);
            VectorBigEndianChkBox.Text = "Big-endian (N64/GC/Wii/PS3/360)";
            VectorBigEndianChkBox.UseVisualStyleBackColor = true;
            VectorBigEndianChkBox.Checked = false;
            VectorBigEndianChkBox.Name = "VectorBigEndianChkBox";
            VectorBigEndianChkBox.TabIndex = 13;
            // 
            // VectorIncludeZeroChkBox
            // 
            VectorIncludeZeroChkBox.AutoSize = true;
            VectorIncludeZeroChkBox.ForeColor = Color.White;
            VectorIncludeZeroChkBox.Location = new Point(410, 105);
            VectorIncludeZeroChkBox.Text = "Include 0.0 values";
            VectorIncludeZeroChkBox.UseVisualStyleBackColor = true;
            VectorIncludeZeroChkBox.Checked = false;
            VectorIncludeZeroChkBox.Name = "VectorIncludeZeroChkBox";
            VectorIncludeZeroChkBox.TabIndex = 14;
            // 
            // VectorHelpLbl
            // 
            VectorHelpLbl.ForeColor = Color.Silver;
            VectorHelpLbl.Location = new Point(24, 172);
            VectorHelpLbl.Size = new Size(600, 90);
            VectorHelpLbl.Text = "The Vector2 Engine looks for 32-bit floats (positions, velocities, sizes, colours) and corrupts them. Only values inside the min/max magnitude are treated as floats, which avoids corrupting code and empty data. In 'Every Nth' mode N counts floats/vectors instead of bytes. Most PC/PS1/PS2/DS/3DS/Switch data is little-endian.";
            VectorHelpLbl.Name = "VectorHelpLbl";
            VectorHelpLbl.TabIndex = 15;
            // 
            // VectorEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            Controls.Add(Vector2EnginePanel);
            Name = "VectorEngineControl";
            Size = new Size(642, 268);
            ((System.ComponentModel.ISupportInitialize)VectorNudgeNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)VectorMinNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)VectorMaxNumericUpDown).EndInit();
            Vector2EnginePanel.ResumeLayout(false);
            Vector2EnginePanel.PerformLayout();
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
        private Label VectorHelpLbl;
    }
}

