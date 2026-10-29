namespace LunarROMCorruptor.EngineControls
{
    partial class BlocksEngineControl
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
            BlocksEnginePanel = new Panel();
            HeaderLbl = new Label();
            BlocksModeLbl = new Label();
            BlocksModeComboBox = new ComboBox();
            BlocksSizeLbl = new Label();
            BlocksSizeNumericUpDown = new NumericUpDown();
            BlocksElementLbl = new Label();
            BlocksElementNumericUpDown = new NumericUpDown();
            BlocksEchoLbl = new Label();
            BlocksEchoNumericUpDown = new NumericUpDown();
            BlocksDescendingChkBox = new CheckBox();
            BlocksElementHintLbl = new Label();
            BlocksHelpLbl = new Label();
            ((System.ComponentModel.ISupportInitialize)BlocksSizeNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BlocksElementNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BlocksEchoNumericUpDown).BeginInit();
            BlocksEnginePanel.SuspendLayout();
            SuspendLayout();
            // 
            // BlocksEnginePanel
            // 
            BlocksEnginePanel.BackColor = Color.FromArgb(40, 40, 60);
            BlocksEnginePanel.Controls.Add(HeaderLbl);
            BlocksEnginePanel.Controls.Add(BlocksModeLbl);
            BlocksEnginePanel.Controls.Add(BlocksModeComboBox);
            BlocksEnginePanel.Controls.Add(BlocksSizeLbl);
            BlocksEnginePanel.Controls.Add(BlocksSizeNumericUpDown);
            BlocksEnginePanel.Controls.Add(BlocksElementLbl);
            BlocksEnginePanel.Controls.Add(BlocksElementNumericUpDown);
            BlocksEnginePanel.Controls.Add(BlocksEchoLbl);
            BlocksEnginePanel.Controls.Add(BlocksEchoNumericUpDown);
            BlocksEnginePanel.Controls.Add(BlocksDescendingChkBox);
            BlocksEnginePanel.Controls.Add(BlocksElementHintLbl);
            BlocksEnginePanel.Controls.Add(BlocksHelpLbl);
            BlocksEnginePanel.Dock = DockStyle.Fill;
            BlocksEnginePanel.Location = new Point(0, 0);
            BlocksEnginePanel.Name = "BlocksEnginePanel";
            BlocksEnginePanel.Size = new Size(642, 268);
            BlocksEnginePanel.TabIndex = 0;
            BlocksEnginePanel.Tag = "color:normal";
            // 
            // HeaderLbl
            // 
            HeaderLbl.AutoSize = true;
            HeaderLbl.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            HeaderLbl.ForeColor = Color.FromArgb(192, 255, 255);
            HeaderLbl.Location = new Point(3, 2);
            HeaderLbl.Name = "HeaderLbl";
            HeaderLbl.TabIndex = 0;
            HeaderLbl.Text = "-Blocks Engine Settings-";
            // 
            // BlocksModeLbl
            // 
            BlocksModeLbl.AutoSize = true;
            BlocksModeLbl.Font = new Font("Segoe UI", 9.75F);
            BlocksModeLbl.ForeColor = Color.White;
            BlocksModeLbl.Location = new Point(24, 30);
            BlocksModeLbl.Text = "Block operation:";
            BlocksModeLbl.Name = "BlocksModeLbl";
            BlocksModeLbl.TabIndex = 1;
            // 
            // BlocksModeComboBox
            // 
            BlocksModeComboBox.BackColor = Color.FromArgb(45, 45, 64);
            BlocksModeComboBox.FlatStyle = FlatStyle.Flat;
            BlocksModeComboBox.ForeColor = Color.White;
            BlocksModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            BlocksModeComboBox.FormattingEnabled = true;
            BlocksModeComboBox.Items.AddRange(new object[] { "RANDOM", "SORT", "REVERSE", "SHUFFLE", "ROTATE", "SMEAR", "ECHO" });
            BlocksModeComboBox.Location = new Point(190, 28);
            BlocksModeComboBox.Size = new Size(191, 23);
            BlocksModeComboBox.SelectedIndex = 1;
            BlocksModeComboBox.Name = "BlocksModeComboBox";
            BlocksModeComboBox.TabIndex = 2;
            // 
            // BlocksSizeLbl
            // 
            BlocksSizeLbl.AutoSize = true;
            BlocksSizeLbl.Font = new Font("Segoe UI", 9.75F);
            BlocksSizeLbl.ForeColor = Color.White;
            BlocksSizeLbl.Location = new Point(24, 58);
            BlocksSizeLbl.Text = "Block size (bytes):";
            BlocksSizeLbl.Name = "BlocksSizeLbl";
            BlocksSizeLbl.TabIndex = 3;
            // 
            // BlocksSizeNumericUpDown
            // 
            BlocksSizeNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            BlocksSizeNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            BlocksSizeNumericUpDown.ForeColor = Color.White;
            BlocksSizeNumericUpDown.Location = new Point(190, 56);
            BlocksSizeNumericUpDown.DecimalPlaces = 0;
            BlocksSizeNumericUpDown.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            BlocksSizeNumericUpDown.Maximum = new decimal(new int[] { 16777216, 0, 0, 0 });
            BlocksSizeNumericUpDown.Value = new decimal(new int[] { 256, 0, 0, 0 });
            BlocksSizeNumericUpDown.Size = new Size(191, 23);
            BlocksSizeNumericUpDown.Name = "BlocksSizeNumericUpDown";
            BlocksSizeNumericUpDown.TabIndex = 4;
            // 
            // BlocksElementLbl
            // 
            BlocksElementLbl.AutoSize = true;
            BlocksElementLbl.Font = new Font("Segoe UI", 9.75F);
            BlocksElementLbl.ForeColor = Color.White;
            BlocksElementLbl.Location = new Point(24, 86);
            BlocksElementLbl.Text = "Element size (bytes):";
            BlocksElementLbl.Name = "BlocksElementLbl";
            BlocksElementLbl.TabIndex = 5;
            // 
            // BlocksElementNumericUpDown
            // 
            BlocksElementNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            BlocksElementNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            BlocksElementNumericUpDown.ForeColor = Color.White;
            BlocksElementNumericUpDown.Location = new Point(190, 84);
            BlocksElementNumericUpDown.DecimalPlaces = 0;
            BlocksElementNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            BlocksElementNumericUpDown.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
            BlocksElementNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            BlocksElementNumericUpDown.Size = new Size(191, 23);
            BlocksElementNumericUpDown.Name = "BlocksElementNumericUpDown";
            BlocksElementNumericUpDown.TabIndex = 6;
            // 
            // BlocksEchoLbl
            // 
            BlocksEchoLbl.AutoSize = true;
            BlocksEchoLbl.Font = new Font("Segoe UI", 9.75F);
            BlocksEchoLbl.ForeColor = Color.White;
            BlocksEchoLbl.Location = new Point(24, 114);
            BlocksEchoLbl.Text = "Echo distance (bytes):";
            BlocksEchoLbl.Name = "BlocksEchoLbl";
            BlocksEchoLbl.TabIndex = 7;
            // 
            // BlocksEchoNumericUpDown
            // 
            BlocksEchoNumericUpDown.BackColor = Color.FromArgb(67, 67, 81);
            BlocksEchoNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            BlocksEchoNumericUpDown.ForeColor = Color.White;
            BlocksEchoNumericUpDown.Location = new Point(190, 112);
            BlocksEchoNumericUpDown.DecimalPlaces = 0;
            BlocksEchoNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            BlocksEchoNumericUpDown.Maximum = new decimal(new int[] { -727379968, 232, 0, 0 });
            BlocksEchoNumericUpDown.Value = new decimal(new int[] { 4096, 0, 0, 0 });
            BlocksEchoNumericUpDown.Size = new Size(191, 23);
            BlocksEchoNumericUpDown.Name = "BlocksEchoNumericUpDown";
            BlocksEchoNumericUpDown.TabIndex = 8;
            // 
            // BlocksDescendingChkBox
            // 
            BlocksDescendingChkBox.AutoSize = true;
            BlocksDescendingChkBox.ForeColor = Color.White;
            BlocksDescendingChkBox.Location = new Point(410, 30);
            BlocksDescendingChkBox.Text = "Sort descending";
            BlocksDescendingChkBox.UseVisualStyleBackColor = true;
            BlocksDescendingChkBox.Checked = false;
            BlocksDescendingChkBox.Name = "BlocksDescendingChkBox";
            BlocksDescendingChkBox.TabIndex = 9;
            // 
            // BlocksElementHintLbl
            // 
            BlocksElementHintLbl.ForeColor = Color.Silver;
            BlocksElementHintLbl.Location = new Point(410, 56);
            BlocksElementHintLbl.Size = new Size(220, 80);
            BlocksElementHintLbl.Text = "Element size: 1 = bytes, 2 = 16-bit audio samples, 3 = 24-bit RGB pixels, 4 = 32-bit values/RGBA pixels.";
            BlocksElementHintLbl.Name = "BlocksElementHintLbl";
            BlocksElementHintLbl.TabIndex = 10;
            // 
            // BlocksHelpLbl
            // 
            BlocksHelpLbl.ForeColor = Color.Silver;
            BlocksHelpLbl.Location = new Point(24, 150);
            BlocksHelpLbl.Size = new Size(600, 110);
            BlocksHelpLbl.Text = "Works on whole blocks instead of single bytes, like pixel sorting and data moshing in glitch art. SORT orders the elements by value, REVERSE/SHUFFLE/ROTATE move them around, SMEAR repeats one element across the block and ECHO mixes the block with the data that came earlier. Intensity = number of random blocks, Every Nth = every Nth block from the start byte. Tip: use ByteView to find image/audio data and drag over it to target it.";
            BlocksHelpLbl.Name = "BlocksHelpLbl";
            BlocksHelpLbl.TabIndex = 11;
            // 
            // BlocksEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            Controls.Add(BlocksEnginePanel);
            Name = "BlocksEngineControl";
            Size = new Size(642, 268);
            ((System.ComponentModel.ISupportInitialize)BlocksSizeNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)BlocksElementNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)BlocksEchoNumericUpDown).EndInit();
            BlocksEnginePanel.ResumeLayout(false);
            BlocksEnginePanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public Panel BlocksEnginePanel;
        private Label HeaderLbl;
        private Label BlocksModeLbl;
        internal ComboBox BlocksModeComboBox;
        private Label BlocksSizeLbl;
        internal NumericUpDown BlocksSizeNumericUpDown;
        private Label BlocksElementLbl;
        internal NumericUpDown BlocksElementNumericUpDown;
        private Label BlocksEchoLbl;
        internal NumericUpDown BlocksEchoNumericUpDown;
        internal CheckBox BlocksDescendingChkBox;
        private Label BlocksElementHintLbl;
        private Label BlocksHelpLbl;
    }
}

