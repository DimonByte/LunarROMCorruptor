namespace LunarROMCorruptor.EngineControls
{
    partial class PointerEngineControl
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
            PointerEnginePanel = new Panel();
            HeaderLbl = new Label();
            PointerProfileLbl = new Label();
            PointerProfileComboBox = new ComboBox();
            PointerModeLbl = new Label();
            PointerModeComboBox = new ComboBox();
            PointerShiftLbl = new Label();
            PointerShiftNumericUpDown = new NumericUpDown();
            PointerWindowLbl = new Label();
            PointerWindowNumericUpDown = new NumericUpDown();
            PointerCustomLbl = new Label();
            PointerCustomMinTxt = new TextBox();
            PointerCustomMaxTxt = new TextBox();
            PointerTablesOnlyChkBox = new CheckBox();
            PointerSkipFunctionsChkBox = new CheckBox();
            PointerCustomBigEndianChkBox = new CheckBox();
            PointerDisplayListLbl = new Label();
            PointerTexturesChkBox = new CheckBox();
            PointerVerticesChkBox = new CheckBox();
            PointerDisplayListsChkBox = new CheckBox();
            PointerHelpLbl = new Label();
            ((System.ComponentModel.ISupportInitialize)PointerShiftNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)PointerWindowNumericUpDown).BeginInit();
            PointerEnginePanel.SuspendLayout();
            SuspendLayout();
            // 
            // PointerEnginePanel
            // 
            PointerEnginePanel.BackColor = Color.FromArgb(10, 56, 71);
            PointerEnginePanel.Controls.Add(HeaderLbl);
            PointerEnginePanel.Controls.Add(PointerProfileLbl);
            PointerEnginePanel.Controls.Add(PointerProfileComboBox);
            PointerEnginePanel.Controls.Add(PointerModeLbl);
            PointerEnginePanel.Controls.Add(PointerModeComboBox);
            PointerEnginePanel.Controls.Add(PointerShiftLbl);
            PointerEnginePanel.Controls.Add(PointerShiftNumericUpDown);
            PointerEnginePanel.Controls.Add(PointerWindowLbl);
            PointerEnginePanel.Controls.Add(PointerWindowNumericUpDown);
            PointerEnginePanel.Controls.Add(PointerCustomLbl);
            PointerEnginePanel.Controls.Add(PointerCustomMinTxt);
            PointerEnginePanel.Controls.Add(PointerCustomMaxTxt);
            PointerEnginePanel.Controls.Add(PointerTablesOnlyChkBox);
            PointerEnginePanel.Controls.Add(PointerSkipFunctionsChkBox);
            PointerEnginePanel.Controls.Add(PointerCustomBigEndianChkBox);
            PointerEnginePanel.Controls.Add(PointerDisplayListLbl);
            PointerEnginePanel.Controls.Add(PointerTexturesChkBox);
            PointerEnginePanel.Controls.Add(PointerVerticesChkBox);
            PointerEnginePanel.Controls.Add(PointerDisplayListsChkBox);
            PointerEnginePanel.Controls.Add(PointerHelpLbl);
            PointerEnginePanel.Dock = DockStyle.Fill;
            PointerEnginePanel.Location = new Point(0, 0);
            PointerEnginePanel.Name = "PointerEnginePanel";
            PointerEnginePanel.Size = new Size(642, 268);
            PointerEnginePanel.TabIndex = 0;
            PointerEnginePanel.Tag = "color:normal";
            // 
            // HeaderLbl
            // 
            HeaderLbl.AutoSize = true;
            HeaderLbl.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            HeaderLbl.ForeColor = Color.FromArgb(192, 255, 255);
            HeaderLbl.Location = new Point(3, 2);
            HeaderLbl.Text = "-Pointer Engine Settings-";
            HeaderLbl.Name = "HeaderLbl";
            HeaderLbl.TabIndex = 1;
            // 
            // PointerProfileLbl
            // 
            PointerProfileLbl.AutoSize = true;
            PointerProfileLbl.Font = new Font("Segoe UI", 9.75F);
            PointerProfileLbl.ForeColor = Color.White;
            PointerProfileLbl.Location = new Point(24, 30);
            PointerProfileLbl.Text = "Pointer type:";
            PointerProfileLbl.Name = "PointerProfileLbl";
            PointerProfileLbl.TabIndex = 2;
            // 
            // PointerProfileComboBox
            // 
            PointerProfileComboBox.BackColor = Color.FromArgb(45, 45, 64);
            PointerProfileComboBox.FlatStyle = FlatStyle.Flat;
            PointerProfileComboBox.ForeColor = Color.White;
            PointerProfileComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            PointerProfileComboBox.FormattingEnabled = true;
            PointerProfileComboBox.Items.AddRange(new object[] { "Auto", "N64 RAM pointers", "N64 display lists", "GBA ROM pointers", "DS RAM pointers", "PS1 RAM pointers", "GameCube/Wii RAM pointers", "File offsets (little-endian)", "File offsets (big-endian)", "Custom range" });
            PointerProfileComboBox.Location = new Point(190, 28);
            PointerProfileComboBox.Size = new Size(191, 23);
            PointerProfileComboBox.SelectedIndex = 0;
            PointerProfileComboBox.Name = "PointerProfileComboBox";
            PointerProfileComboBox.TabIndex = 3;
            // 
            // PointerModeLbl
            // 
            PointerModeLbl.AutoSize = true;
            PointerModeLbl.Font = new Font("Segoe UI", 9.75F);
            PointerModeLbl.ForeColor = Color.White;
            PointerModeLbl.Location = new Point(24, 58);
            PointerModeLbl.Text = "Corruption Type:";
            PointerModeLbl.Name = "PointerModeLbl";
            PointerModeLbl.TabIndex = 4;
            // 
            // PointerModeComboBox
            // 
            PointerModeComboBox.BackColor = Color.FromArgb(45, 45, 64);
            PointerModeComboBox.FlatStyle = FlatStyle.Flat;
            PointerModeComboBox.ForeColor = Color.White;
            PointerModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            PointerModeComboBox.FormattingEnabled = true;
            PointerModeComboBox.Items.AddRange(new object[] { "SWAP", "SHIFT", "DUPLICATE", "RANDOMTARGET" });
            PointerModeComboBox.Location = new Point(190, 56);
            PointerModeComboBox.Size = new Size(191, 23);
            PointerModeComboBox.SelectedIndex = 0;
            PointerModeComboBox.Name = "PointerModeComboBox";
            PointerModeComboBox.TabIndex = 5;
            // 
            // PointerShiftLbl
            // 
            PointerShiftLbl.AutoSize = true;
            PointerShiftLbl.Font = new Font("Segoe UI", 9.75F);
            PointerShiftLbl.ForeColor = Color.White;
            PointerShiftLbl.Location = new Point(24, 86);
            PointerShiftLbl.Text = "Max shift (bytes):";
            PointerShiftLbl.Name = "PointerShiftLbl";
            PointerShiftLbl.TabIndex = 6;
            // 
            // PointerShiftNumericUpDown
            // 
            PointerShiftNumericUpDown.BackColor = Color.FromArgb(45, 45, 64);
            PointerShiftNumericUpDown.ForeColor = Color.White;
            PointerShiftNumericUpDown.Location = new Point(190, 84);
            PointerShiftNumericUpDown.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            PointerShiftNumericUpDown.Maximum = new decimal(new int[] { 65536, 0, 0, 0 });
            PointerShiftNumericUpDown.Value = new decimal(new int[] { 64, 0, 0, 0 });
            PointerShiftNumericUpDown.Size = new Size(191, 23);
            PointerShiftNumericUpDown.Name = "PointerShiftNumericUpDown";
            PointerShiftNumericUpDown.TabIndex = 7;
            // 
            // PointerWindowLbl
            // 
            PointerWindowLbl.AutoSize = true;
            PointerWindowLbl.Font = new Font("Segoe UI", 9.75F);
            PointerWindowLbl.ForeColor = Color.White;
            PointerWindowLbl.Location = new Point(24, 114);
            PointerWindowLbl.Text = "Table window (bytes):";
            PointerWindowLbl.Name = "PointerWindowLbl";
            PointerWindowLbl.TabIndex = 8;
            // 
            // PointerWindowNumericUpDown
            // 
            PointerWindowNumericUpDown.BackColor = Color.FromArgb(45, 45, 64);
            PointerWindowNumericUpDown.ForeColor = Color.White;
            PointerWindowNumericUpDown.Location = new Point(190, 112);
            PointerWindowNumericUpDown.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            PointerWindowNumericUpDown.Maximum = new decimal(new int[] { 65536, 0, 0, 0 });
            PointerWindowNumericUpDown.Value = new decimal(new int[] { 64, 0, 0, 0 });
            PointerWindowNumericUpDown.Size = new Size(191, 23);
            PointerWindowNumericUpDown.Name = "PointerWindowNumericUpDown";
            PointerWindowNumericUpDown.TabIndex = 9;
            // 
            // PointerCustomLbl
            // 
            PointerCustomLbl.AutoSize = true;
            PointerCustomLbl.Font = new Font("Segoe UI", 9.75F);
            PointerCustomLbl.ForeColor = Color.White;
            PointerCustomLbl.Location = new Point(24, 142);
            PointerCustomLbl.Text = "Custom range (hex):";
            PointerCustomLbl.Name = "PointerCustomLbl";
            PointerCustomLbl.TabIndex = 10;
            // 
            // PointerCustomMinTxt
            // 
            PointerCustomMinTxt.BackColor = Color.FromArgb(45, 45, 64);
            PointerCustomMinTxt.BorderStyle = BorderStyle.FixedSingle;
            PointerCustomMinTxt.ForeColor = Color.White;
            PointerCustomMinTxt.Location = new Point(190, 140);
            PointerCustomMinTxt.Size = new Size(90, 23);
            PointerCustomMinTxt.Text = "80000000";
            PointerCustomMinTxt.Name = "PointerCustomMinTxt";
            PointerCustomMinTxt.TabIndex = 11;
            // 
            // PointerCustomMaxTxt
            // 
            PointerCustomMaxTxt.BackColor = Color.FromArgb(45, 45, 64);
            PointerCustomMaxTxt.BorderStyle = BorderStyle.FixedSingle;
            PointerCustomMaxTxt.ForeColor = Color.White;
            PointerCustomMaxTxt.Location = new Point(291, 140);
            PointerCustomMaxTxt.Size = new Size(90, 23);
            PointerCustomMaxTxt.Text = "807FFFFF";
            PointerCustomMaxTxt.Name = "PointerCustomMaxTxt";
            PointerCustomMaxTxt.TabIndex = 12;
            // 
            // PointerTablesOnlyChkBox
            // 
            PointerTablesOnlyChkBox.AutoSize = true;
            PointerTablesOnlyChkBox.ForeColor = Color.White;
            PointerTablesOnlyChkBox.Location = new Point(410, 30);
            PointerTablesOnlyChkBox.Text = "Only pointers in tables";
            PointerTablesOnlyChkBox.UseVisualStyleBackColor = true;
            PointerTablesOnlyChkBox.Checked = true;
            PointerTablesOnlyChkBox.Name = "PointerTablesOnlyChkBox";
            PointerTablesOnlyChkBox.TabIndex = 13;
            // 
            // PointerSkipFunctionsChkBox
            // 
            PointerSkipFunctionsChkBox.AutoSize = true;
            PointerSkipFunctionsChkBox.ForeColor = Color.White;
            PointerSkipFunctionsChkBox.Location = new Point(410, 55);
            PointerSkipFunctionsChkBox.Text = "Skip function pointers";
            PointerSkipFunctionsChkBox.UseVisualStyleBackColor = true;
            PointerSkipFunctionsChkBox.Checked = true;
            PointerSkipFunctionsChkBox.Name = "PointerSkipFunctionsChkBox";
            PointerSkipFunctionsChkBox.TabIndex = 14;
            // 
            // PointerCustomBigEndianChkBox
            // 
            PointerCustomBigEndianChkBox.AutoSize = true;
            PointerCustomBigEndianChkBox.ForeColor = Color.White;
            PointerCustomBigEndianChkBox.Location = new Point(410, 80);
            PointerCustomBigEndianChkBox.Text = "Custom range is big-endian";
            PointerCustomBigEndianChkBox.UseVisualStyleBackColor = true;
            PointerCustomBigEndianChkBox.Checked = true;
            PointerCustomBigEndianChkBox.Name = "PointerCustomBigEndianChkBox";
            PointerCustomBigEndianChkBox.TabIndex = 15;
            // 
            // PointerDisplayListLbl
            // 
            PointerDisplayListLbl.AutoSize = true;
            PointerDisplayListLbl.Font = new Font("Segoe UI", 9.75F);
            PointerDisplayListLbl.ForeColor = Color.White;
            PointerDisplayListLbl.Location = new Point(410, 108);
            PointerDisplayListLbl.Text = "N64 display list targets:";
            PointerDisplayListLbl.Name = "PointerDisplayListLbl";
            PointerDisplayListLbl.TabIndex = 16;
            // 
            // PointerTexturesChkBox
            // 
            PointerTexturesChkBox.AutoSize = true;
            PointerTexturesChkBox.ForeColor = Color.White;
            PointerTexturesChkBox.Location = new Point(420, 130);
            PointerTexturesChkBox.Text = "Textures (G_SETTIMG)";
            PointerTexturesChkBox.UseVisualStyleBackColor = true;
            PointerTexturesChkBox.Checked = true;
            PointerTexturesChkBox.Name = "PointerTexturesChkBox";
            PointerTexturesChkBox.TabIndex = 17;
            // 
            // PointerVerticesChkBox
            // 
            PointerVerticesChkBox.AutoSize = true;
            PointerVerticesChkBox.ForeColor = Color.White;
            PointerVerticesChkBox.Location = new Point(420, 153);
            PointerVerticesChkBox.Text = "Vertices (G_VTX)";
            PointerVerticesChkBox.UseVisualStyleBackColor = true;
            PointerVerticesChkBox.Checked = false;
            PointerVerticesChkBox.Name = "PointerVerticesChkBox";
            PointerVerticesChkBox.TabIndex = 18;
            // 
            // PointerDisplayListsChkBox
            // 
            PointerDisplayListsChkBox.AutoSize = true;
            PointerDisplayListsChkBox.ForeColor = Color.White;
            PointerDisplayListsChkBox.Location = new Point(420, 176);
            PointerDisplayListsChkBox.Text = "Sub display lists (G_DL)";
            PointerDisplayListsChkBox.UseVisualStyleBackColor = true;
            PointerDisplayListsChkBox.Checked = false;
            PointerDisplayListsChkBox.Name = "PointerDisplayListsChkBox";
            PointerDisplayListsChkBox.TabIndex = 19;
            // 
            // PointerHelpLbl
            // 
            PointerHelpLbl.Font = new Font("Segoe UI", 9F);
            PointerHelpLbl.ForeColor = Color.Silver;
            PointerHelpLbl.Location = new Point(24, 202);
            PointerHelpLbl.Size = new Size(600, 62);
            PointerHelpLbl.Text = "Swaps, shifts or copies pointers so the game loads a different texture, model, sound, animation or text instead of crashing. SWAP inside tables is the most stable. 'N64 display lists' changes the texture/vertex addresses inside model display lists. Compressed data can't be seen, use the Compression Engine for it.";
            PointerHelpLbl.Name = "PointerHelpLbl";
            PointerHelpLbl.TabIndex = 20;
            // 
            // PointerEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            Controls.Add(PointerEnginePanel);
            Name = "PointerEngineControl";
            Size = new Size(642, 268);
            ((System.ComponentModel.ISupportInitialize)PointerShiftNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)PointerWindowNumericUpDown).EndInit();
            PointerEnginePanel.ResumeLayout(false);
            PointerEnginePanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public Panel PointerEnginePanel;
        private Label HeaderLbl;
        private Label PointerProfileLbl;
        internal ComboBox PointerProfileComboBox;
        private Label PointerModeLbl;
        internal ComboBox PointerModeComboBox;
        private Label PointerShiftLbl;
        internal NumericUpDown PointerShiftNumericUpDown;
        private Label PointerWindowLbl;
        internal NumericUpDown PointerWindowNumericUpDown;
        private Label PointerCustomLbl;
        internal TextBox PointerCustomMinTxt;
        internal TextBox PointerCustomMaxTxt;
        internal CheckBox PointerTablesOnlyChkBox;
        internal CheckBox PointerSkipFunctionsChkBox;
        internal CheckBox PointerCustomBigEndianChkBox;
        private Label PointerDisplayListLbl;
        internal CheckBox PointerTexturesChkBox;
        internal CheckBox PointerVerticesChkBox;
        internal CheckBox PointerDisplayListsChkBox;
        private Label PointerHelpLbl;
    }
}
