namespace LunarROMCorruptor.EngineControls
{
    partial class SourceCodeEngineControl
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
            SourceCodeEnginePanel = new Panel();
            HeaderLbl = new Label();
            SourceValuesGroupBox = new GroupBox();
            SourceBooleansChkBox = new CheckBox();
            SourceIntegersChkBox = new CheckBox();
            SourceFloatsChkBox = new CheckBox();
            SourceStringsChkBox = new CheckBox();
            SourceNullsChkBox = new CheckBox();
            SourceOperatorsChkBox = new CheckBox();
            SourceIdentifiersChkBox = new CheckBox();
            SourceNumberModeLbl = new Label();
            SourceNumberModeComboBox = new ComboBox();
            SourceStrengthLbl = new Label();
            SourceStrengthNumericUpDown = new NumericUpDown();
            SourceStringModeLbl = new Label();
            SourceStringModeComboBox = new ComboBox();
            SourceCommentLbl = new Label();
            SourceCommentStyleComboBox = new ComboBox();
            SourceHelpLbl = new Label();
            ((System.ComponentModel.ISupportInitialize)SourceStrengthNumericUpDown).BeginInit();
            SourceCodeEnginePanel.SuspendLayout();
            SourceValuesGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // SourceCodeEnginePanel
            // 
            SourceCodeEnginePanel.BackColor = Color.FromArgb(40, 40, 60);
            SourceCodeEnginePanel.Controls.Add(HeaderLbl);
            SourceCodeEnginePanel.Controls.Add(SourceValuesGroupBox);
            SourceCodeEnginePanel.Controls.Add(SourceNumberModeLbl);
            SourceCodeEnginePanel.Controls.Add(SourceNumberModeComboBox);
            SourceCodeEnginePanel.Controls.Add(SourceStrengthLbl);
            SourceCodeEnginePanel.Controls.Add(SourceStrengthNumericUpDown);
            SourceCodeEnginePanel.Controls.Add(SourceStringModeLbl);
            SourceCodeEnginePanel.Controls.Add(SourceStringModeComboBox);
            SourceCodeEnginePanel.Controls.Add(SourceCommentLbl);
            SourceCodeEnginePanel.Controls.Add(SourceCommentStyleComboBox);
            SourceCodeEnginePanel.Controls.Add(SourceHelpLbl);
            SourceCodeEnginePanel.Dock = DockStyle.Fill;
            SourceCodeEnginePanel.Location = new Point(0, 0);
            SourceCodeEnginePanel.Name = "SourceCodeEnginePanel";
            SourceCodeEnginePanel.Size = new Size(642, 268);
            SourceCodeEnginePanel.TabIndex = 0;
            SourceCodeEnginePanel.Tag = "color:normal";
            // 
            // HeaderLbl
            // 
            HeaderLbl.AutoSize = true;
            HeaderLbl.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            HeaderLbl.ForeColor = Color.FromArgb(192, 255, 255);
            HeaderLbl.Location = new Point(3, 2);
            HeaderLbl.Name = "HeaderLbl";
            HeaderLbl.TabIndex = 0;
            HeaderLbl.Text = "-Source Code Engine Settings-";
            // 
            // SourceValuesGroupBox
            // 
            SourceValuesGroupBox.Controls.Add(SourceBooleansChkBox);
            SourceValuesGroupBox.Controls.Add(SourceIntegersChkBox);
            SourceValuesGroupBox.Controls.Add(SourceFloatsChkBox);
            SourceValuesGroupBox.Controls.Add(SourceStringsChkBox);
            SourceValuesGroupBox.Controls.Add(SourceNullsChkBox);
            SourceValuesGroupBox.Controls.Add(SourceOperatorsChkBox);
            SourceValuesGroupBox.Controls.Add(SourceIdentifiersChkBox);
            SourceValuesGroupBox.ForeColor = Color.White;
            SourceValuesGroupBox.Location = new Point(18, 26);
            SourceValuesGroupBox.Size = new Size(200, 196);
            SourceValuesGroupBox.Text = "Values to corrupt";
            SourceValuesGroupBox.Name = "SourceValuesGroupBox";
            SourceValuesGroupBox.TabIndex = 1;
            // 
            // SourceBooleansChkBox
            // 
            SourceBooleansChkBox.AutoSize = true;
            SourceBooleansChkBox.ForeColor = Color.White;
            SourceBooleansChkBox.Location = new Point(12, 22);
            SourceBooleansChkBox.Text = "Booleans (true/false)";
            SourceBooleansChkBox.UseVisualStyleBackColor = true;
            SourceBooleansChkBox.Checked = true;
            SourceBooleansChkBox.Name = "SourceBooleansChkBox";
            SourceBooleansChkBox.TabIndex = 2;
            // 
            // SourceIntegersChkBox
            // 
            SourceIntegersChkBox.AutoSize = true;
            SourceIntegersChkBox.ForeColor = Color.White;
            SourceIntegersChkBox.Location = new Point(12, 46);
            SourceIntegersChkBox.Text = "Integers (10, 0xFF)";
            SourceIntegersChkBox.UseVisualStyleBackColor = true;
            SourceIntegersChkBox.Checked = true;
            SourceIntegersChkBox.Name = "SourceIntegersChkBox";
            SourceIntegersChkBox.TabIndex = 3;
            // 
            // SourceFloatsChkBox
            // 
            SourceFloatsChkBox.AutoSize = true;
            SourceFloatsChkBox.ForeColor = Color.White;
            SourceFloatsChkBox.Location = new Point(12, 70);
            SourceFloatsChkBox.Text = "Decimals (3.14)";
            SourceFloatsChkBox.UseVisualStyleBackColor = true;
            SourceFloatsChkBox.Checked = true;
            SourceFloatsChkBox.Name = "SourceFloatsChkBox";
            SourceFloatsChkBox.TabIndex = 4;
            // 
            // SourceStringsChkBox
            // 
            SourceStringsChkBox.AutoSize = true;
            SourceStringsChkBox.ForeColor = Color.White;
            SourceStringsChkBox.Location = new Point(12, 94);
            SourceStringsChkBox.Text = "Strings (\"text\")";
            SourceStringsChkBox.UseVisualStyleBackColor = true;
            SourceStringsChkBox.Checked = true;
            SourceStringsChkBox.Name = "SourceStringsChkBox";
            SourceStringsChkBox.TabIndex = 5;
            // 
            // SourceNullsChkBox
            // 
            SourceNullsChkBox.AutoSize = true;
            SourceNullsChkBox.ForeColor = Color.White;
            SourceNullsChkBox.Location = new Point(12, 118);
            SourceNullsChkBox.Text = "Null values (null/nil/None)";
            SourceNullsChkBox.UseVisualStyleBackColor = true;
            SourceNullsChkBox.Checked = false;
            SourceNullsChkBox.Name = "SourceNullsChkBox";
            SourceNullsChkBox.TabIndex = 6;
            // 
            // SourceOperatorsChkBox
            // 
            SourceOperatorsChkBox.AutoSize = true;
            SourceOperatorsChkBox.ForeColor = Color.White;
            SourceOperatorsChkBox.Location = new Point(12, 142);
            SourceOperatorsChkBox.Text = "Operators (== < + &&)";
            SourceOperatorsChkBox.UseVisualStyleBackColor = true;
            SourceOperatorsChkBox.Checked = false;
            SourceOperatorsChkBox.Name = "SourceOperatorsChkBox";
            SourceOperatorsChkBox.TabIndex = 7;
            // 
            // SourceIdentifiersChkBox
            // 
            SourceIdentifiersChkBox.AutoSize = true;
            SourceIdentifiersChkBox.ForeColor = Color.White;
            SourceIdentifiersChkBox.Location = new Point(12, 166);
            SourceIdentifiersChkBox.Text = "Variable names (swap)";
            SourceIdentifiersChkBox.UseVisualStyleBackColor = true;
            SourceIdentifiersChkBox.Checked = false;
            SourceIdentifiersChkBox.Name = "SourceIdentifiersChkBox";
            SourceIdentifiersChkBox.TabIndex = 8;
            // 
            // SourceNumberModeLbl
            // 
            SourceNumberModeLbl.AutoSize = true;
            SourceNumberModeLbl.Font = new Font("Segoe UI", 9.75F);
            SourceNumberModeLbl.ForeColor = Color.White;
            SourceNumberModeLbl.Location = new Point(236, 30);
            SourceNumberModeLbl.Text = "Number corruption:";
            SourceNumberModeLbl.Name = "SourceNumberModeLbl";
            SourceNumberModeLbl.TabIndex = 9;
            // 
            // SourceNumberModeComboBox
            // 
            SourceNumberModeComboBox.BackColor = Color.FromArgb(45, 45, 64);
            SourceNumberModeComboBox.FlatStyle = FlatStyle.Flat;
            SourceNumberModeComboBox.ForeColor = Color.White;
            SourceNumberModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            SourceNumberModeComboBox.FormattingEnabled = true;
            SourceNumberModeComboBox.Items.AddRange(new object[] { "RANDOM", "NUDGE", "SCALE", "SIGNFLIP", "MAGIC", "DIGITS" });
            SourceNumberModeComboBox.Location = new Point(380, 28);
            SourceNumberModeComboBox.Size = new Size(200, 23);
            SourceNumberModeComboBox.SelectedIndex = 0;
            SourceNumberModeComboBox.Name = "SourceNumberModeComboBox";
            SourceNumberModeComboBox.TabIndex = 10;
            // 
            // SourceStrengthLbl
            // 
            SourceStrengthLbl.AutoSize = true;
            SourceStrengthLbl.Font = new Font("Segoe UI", 9.75F);
            SourceStrengthLbl.ForeColor = Color.White;
            SourceStrengthLbl.Location = new Point(236, 58);
            SourceStrengthLbl.Text = "Strength (%):";
            SourceStrengthLbl.Name = "SourceStrengthLbl";
            SourceStrengthLbl.TabIndex = 11;
            // 
            // SourceStrengthNumericUpDown
            // 
            SourceStrengthNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            SourceStrengthNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            SourceStrengthNumericUpDown.ForeColor = Color.White;
            SourceStrengthNumericUpDown.Location = new Point(380, 56);
            SourceStrengthNumericUpDown.DecimalPlaces = 0;
            SourceStrengthNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            SourceStrengthNumericUpDown.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            SourceStrengthNumericUpDown.Value = new decimal(new int[] { 50, 0, 0, 0 });
            SourceStrengthNumericUpDown.Size = new Size(200, 23);
            SourceStrengthNumericUpDown.Name = "SourceStrengthNumericUpDown";
            SourceStrengthNumericUpDown.TabIndex = 12;
            // 
            // SourceStringModeLbl
            // 
            SourceStringModeLbl.AutoSize = true;
            SourceStringModeLbl.Font = new Font("Segoe UI", 9.75F);
            SourceStringModeLbl.ForeColor = Color.White;
            SourceStringModeLbl.Location = new Point(236, 86);
            SourceStringModeLbl.Text = "String corruption:";
            SourceStringModeLbl.Name = "SourceStringModeLbl";
            SourceStringModeLbl.TabIndex = 13;
            // 
            // SourceStringModeComboBox
            // 
            SourceStringModeComboBox.BackColor = Color.FromArgb(45, 45, 64);
            SourceStringModeComboBox.FlatStyle = FlatStyle.Flat;
            SourceStringModeComboBox.ForeColor = Color.White;
            SourceStringModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            SourceStringModeComboBox.FormattingEnabled = true;
            SourceStringModeComboBox.Items.AddRange(new object[] { "RANDOM", "SHUFFLE", "REVERSE", "TRUNCATE", "SWAP", "SUBSTITUTE", "REPEAT" });
            SourceStringModeComboBox.Location = new Point(380, 84);
            SourceStringModeComboBox.Size = new Size(200, 23);
            SourceStringModeComboBox.SelectedIndex = 0;
            SourceStringModeComboBox.Name = "SourceStringModeComboBox";
            SourceStringModeComboBox.TabIndex = 14;
            // 
            // SourceCommentLbl
            // 
            SourceCommentLbl.AutoSize = true;
            SourceCommentLbl.Font = new Font("Segoe UI", 9.75F);
            SourceCommentLbl.ForeColor = Color.White;
            SourceCommentLbl.Location = new Point(236, 114);
            SourceCommentLbl.Text = "Comment style:";
            SourceCommentLbl.Name = "SourceCommentLbl";
            SourceCommentLbl.TabIndex = 15;
            // 
            // SourceCommentStyleComboBox
            // 
            SourceCommentStyleComboBox.BackColor = Color.FromArgb(45, 45, 64);
            SourceCommentStyleComboBox.FlatStyle = FlatStyle.Flat;
            SourceCommentStyleComboBox.ForeColor = Color.White;
            SourceCommentStyleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            SourceCommentStyleComboBox.FormattingEnabled = true;
            SourceCommentStyleComboBox.Items.AddRange(new object[] { "AUTO", "CLIKE", "HASH", "DASHDASH", "SEMICOLON", "XML", "BASIC", "NONE" });
            SourceCommentStyleComboBox.Location = new Point(380, 112);
            SourceCommentStyleComboBox.Size = new Size(200, 23);
            SourceCommentStyleComboBox.SelectedIndex = 0;
            SourceCommentStyleComboBox.Name = "SourceCommentStyleComboBox";
            SourceCommentStyleComboBox.TabIndex = 16;
            // 
            // SourceHelpLbl
            // 
            SourceHelpLbl.ForeColor = Color.Silver;
            SourceHelpLbl.Location = new Point(236, 144);
            SourceHelpLbl.Size = new Size(390, 118);
            SourceHelpLbl.Text = "Reads the file as text and corrupts values instead of raw bytes, e.g. true becomes false, 3.14 becomes 4, strings get shuffled. Comments are skipped (AUTO picks the comment style from the file extension). Intensity = how many values to corrupt, Every Nth = every Nth value. Each change is saved to the stash as File(offset).VARLINE(\"name\",line).SET(\"value\").";
            SourceHelpLbl.Name = "SourceHelpLbl";
            SourceHelpLbl.TabIndex = 17;
            // 
            // SourceCodeEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            Controls.Add(SourceCodeEnginePanel);
            Name = "SourceCodeEngineControl";
            Size = new Size(642, 268);
            ((System.ComponentModel.ISupportInitialize)SourceStrengthNumericUpDown).EndInit();
            SourceValuesGroupBox.ResumeLayout(false);
            SourceValuesGroupBox.PerformLayout();
            SourceCodeEnginePanel.ResumeLayout(false);
            SourceCodeEnginePanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public Panel SourceCodeEnginePanel;
        private Label HeaderLbl;
        private GroupBox SourceValuesGroupBox;
        internal CheckBox SourceBooleansChkBox;
        internal CheckBox SourceIntegersChkBox;
        internal CheckBox SourceFloatsChkBox;
        internal CheckBox SourceStringsChkBox;
        internal CheckBox SourceNullsChkBox;
        internal CheckBox SourceOperatorsChkBox;
        internal CheckBox SourceIdentifiersChkBox;
        private Label SourceNumberModeLbl;
        internal ComboBox SourceNumberModeComboBox;
        private Label SourceStrengthLbl;
        internal NumericUpDown SourceStrengthNumericUpDown;
        private Label SourceStringModeLbl;
        internal ComboBox SourceStringModeComboBox;
        private Label SourceCommentLbl;
        internal ComboBox SourceCommentStyleComboBox;
        private Label SourceHelpLbl;
    }
}

