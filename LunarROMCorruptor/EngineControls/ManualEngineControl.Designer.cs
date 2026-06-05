namespace LunarROMCorruptor.EngineControls
{
    partial class ManualEngineControl
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
            ManualEnginePanel = new Panel();
            Label9 = new Label();
            GroupBox8 = new GroupBox();
            PasterandombitCHECK = new CheckBox();
            ByteEqualNumericUpDown = new NumericUpDown();
            MakeBitEqualCHECK = new CheckBox();
            GroupBox7 = new GroupBox();
            DoubleCheck = new RadioButton();
            DivideRadio = new RadioButton();
            MultiRadio = new RadioButton();
            MathOperationNumericUpDown = new NumericUpDown();
            MULTIORDIVIDECHeck = new CheckBox();
            Label25 = new Label();
            GroupBox3 = new GroupBox();
            ReplaceCHECK = new CheckBox();
            ReplaceNumericUpDown1 = new NumericUpDown();
            ReplaceNumericUpDown2 = new NumericUpDown();
            Label22 = new Label();
            Label16 = new Label();
            GroupBox4 = new GroupBox();
            SHIFTBYTECHECK = new CheckBox();
            Label12 = new Label();
            ShiftNumericUpDown = new NumericUpDown();
            RepeatRandomBitCHECK = new CheckBox();
            GroupBox5 = new GroupBox();
            IncrementCHECK = new CheckBox();
            Label14 = new Label();
            IncrementNumericUpDown = new NumericUpDown();
            GroupBox2 = new GroupBox();
            Label8 = new Label();
            RepeatNumericUpDown = new NumericUpDown();
            ManualEnginePanel.SuspendLayout();
            GroupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ByteEqualNumericUpDown).BeginInit();
            GroupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MathOperationNumericUpDown).BeginInit();
            GroupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ReplaceNumericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ReplaceNumericUpDown2).BeginInit();
            GroupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ShiftNumericUpDown).BeginInit();
            GroupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)IncrementNumericUpDown).BeginInit();
            GroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)RepeatNumericUpDown).BeginInit();
            SuspendLayout();
            // 
            // ManualEnginePanel
            // 
            ManualEnginePanel.BackColor = Color.FromArgb(39, 43, 83);
            ManualEnginePanel.Controls.Add(Label9);
            ManualEnginePanel.Controls.Add(GroupBox8);
            ManualEnginePanel.Controls.Add(GroupBox7);
            ManualEnginePanel.Controls.Add(GroupBox3);
            ManualEnginePanel.Controls.Add(GroupBox4);
            ManualEnginePanel.Controls.Add(RepeatRandomBitCHECK);
            ManualEnginePanel.Controls.Add(GroupBox5);
            ManualEnginePanel.Controls.Add(GroupBox2);
            ManualEnginePanel.Dock = DockStyle.Fill;
            ManualEnginePanel.Location = new Point(0, 0);
            ManualEnginePanel.Margin = new Padding(2, 3, 2, 3);
            ManualEnginePanel.Name = "ManualEnginePanel";
            ManualEnginePanel.Size = new Size(642, 268);
            ManualEnginePanel.TabIndex = 138;
            ManualEnginePanel.Tag = "color:normal";
            // 
            // Label9
            // 
            Label9.AutoSize = true;
            Label9.Font = new Font("Segoe UI", 9.75F, FontStyle.Underline);
            Label9.ForeColor = Color.Turquoise;
            Label9.Location = new Point(4, -2);
            Label9.Margin = new Padding(4, 0, 4, 0);
            Label9.Name = "Label9";
            Label9.Size = new Size(154, 17);
            Label9.TabIndex = 94;
            Label9.Text = "-Manual Engine Settings-";
            // 
            // GroupBox8
            // 
            GroupBox8.Controls.Add(PasterandombitCHECK);
            GroupBox8.Controls.Add(ByteEqualNumericUpDown);
            GroupBox8.Controls.Add(MakeBitEqualCHECK);
            GroupBox8.ForeColor = Color.FromArgb(213, 216, 216);
            GroupBox8.Location = new Point(379, 98);
            GroupBox8.Margin = new Padding(2, 3, 2, 3);
            GroupBox8.Name = "GroupBox8";
            GroupBox8.Padding = new Padding(2, 3, 2, 3);
            GroupBox8.Size = new Size(223, 73);
            GroupBox8.TabIndex = 84;
            GroupBox8.TabStop = false;
            GroupBox8.Text = "Other";
            // 
            // PasterandombitCHECK
            // 
            PasterandombitCHECK.AutoSize = true;
            PasterandombitCHECK.ForeColor = Color.FromArgb(213, 216, 216);
            PasterandombitCHECK.Location = new Point(6, 15);
            PasterandombitCHECK.Margin = new Padding(2, 3, 2, 3);
            PasterandombitCHECK.Name = "PasterandombitCHECK";
            PasterandombitCHECK.Size = new Size(173, 19);
            PasterandombitCHECK.TabIndex = 65;
            PasterandombitCHECK.Text = "Paste byte to Random Place";
            PasterandombitCHECK.UseVisualStyleBackColor = true;
            // 
            // ByteEqualNumericUpDown
            // 
            ByteEqualNumericUpDown.BackColor = Color.FromArgb(29, 32, 32);
            ByteEqualNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ByteEqualNumericUpDown.ForeColor = Color.FromArgb(213, 216, 216);
            ByteEqualNumericUpDown.Location = new Point(135, 38);
            ByteEqualNumericUpDown.Margin = new Padding(2, 3, 2, 3);
            ByteEqualNumericUpDown.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            ByteEqualNumericUpDown.Name = "ByteEqualNumericUpDown";
            ByteEqualNumericUpDown.Size = new Size(79, 22);
            ByteEqualNumericUpDown.TabIndex = 63;
            ByteEqualNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // MakeBitEqualCHECK
            // 
            MakeBitEqualCHECK.AutoSize = true;
            MakeBitEqualCHECK.ForeColor = Color.FromArgb(213, 216, 216);
            MakeBitEqualCHECK.Location = new Point(6, 42);
            MakeBitEqualCHECK.Margin = new Padding(2, 3, 2, 3);
            MakeBitEqualCHECK.Name = "MakeBitEqualCHECK";
            MakeBitEqualCHECK.Size = new Size(119, 19);
            MakeBitEqualCHECK.TabIndex = 65;
            MakeBitEqualCHECK.Text = "Make byte equal: ";
            MakeBitEqualCHECK.UseVisualStyleBackColor = true;
            // 
            // GroupBox7
            // 
            GroupBox7.Controls.Add(DoubleCheck);
            GroupBox7.Controls.Add(DivideRadio);
            GroupBox7.Controls.Add(MultiRadio);
            GroupBox7.Controls.Add(MathOperationNumericUpDown);
            GroupBox7.Controls.Add(MULTIORDIVIDECHeck);
            GroupBox7.Controls.Add(Label25);
            GroupBox7.ForeColor = Color.White;
            GroupBox7.Location = new Point(201, 29);
            GroupBox7.Margin = new Padding(2, 3, 2, 3);
            GroupBox7.Name = "GroupBox7";
            GroupBox7.Padding = new Padding(2, 3, 2, 3);
            GroupBox7.Size = new Size(266, 62);
            GroupBox7.TabIndex = 83;
            GroupBox7.TabStop = false;
            // 
            // DoubleCheck
            // 
            DoubleCheck.AutoSize = true;
            DoubleCheck.ForeColor = Color.FromArgb(213, 216, 216);
            DoubleCheck.Location = new Point(85, 18);
            DoubleCheck.Margin = new Padding(2, 3, 2, 3);
            DoubleCheck.Name = "DoubleCheck";
            DoubleCheck.Size = new Size(33, 19);
            DoubleCheck.TabIndex = 76;
            DoubleCheck.TabStop = true;
            DoubleCheck.Text = "^";
            DoubleCheck.UseVisualStyleBackColor = true;
            // 
            // DivideRadio
            // 
            DivideRadio.AutoSize = true;
            DivideRadio.ForeColor = Color.FromArgb(213, 216, 216);
            DivideRadio.Location = new Point(50, 18);
            DivideRadio.Margin = new Padding(2, 3, 2, 3);
            DivideRadio.Name = "DivideRadio";
            DivideRadio.Size = new Size(30, 19);
            DivideRadio.TabIndex = 76;
            DivideRadio.TabStop = true;
            DivideRadio.Text = "/";
            DivideRadio.UseVisualStyleBackColor = true;
            // 
            // MultiRadio
            // 
            MultiRadio.AutoSize = true;
            MultiRadio.ForeColor = Color.FromArgb(213, 216, 216);
            MultiRadio.Location = new Point(8, 18);
            MultiRadio.Margin = new Padding(2, 3, 2, 3);
            MultiRadio.Name = "MultiRadio";
            MultiRadio.Size = new Size(30, 19);
            MultiRadio.TabIndex = 76;
            MultiRadio.TabStop = true;
            MultiRadio.Text = "*";
            MultiRadio.UseVisualStyleBackColor = true;
            // 
            // MathOperationNumericUpDown
            // 
            MathOperationNumericUpDown.BackColor = Color.FromArgb(29, 32, 32);
            MathOperationNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            MathOperationNumericUpDown.ForeColor = Color.FromArgb(213, 216, 216);
            MathOperationNumericUpDown.Location = new Point(166, 16);
            MathOperationNumericUpDown.Margin = new Padding(2, 3, 2, 3);
            MathOperationNumericUpDown.Maximum = new decimal(new int[] { 99999999, 0, 0, 0 });
            MathOperationNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            MathOperationNumericUpDown.Name = "MathOperationNumericUpDown";
            MathOperationNumericUpDown.Size = new Size(93, 22);
            MathOperationNumericUpDown.TabIndex = 63;
            MathOperationNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // MULTIORDIVIDECHeck
            // 
            MULTIORDIVIDECHeck.AutoSize = true;
            MULTIORDIVIDECHeck.ForeColor = Color.FromArgb(213, 216, 216);
            MULTIORDIVIDECHeck.Location = new Point(4, -2);
            MULTIORDIVIDECHeck.Margin = new Padding(2, 3, 2, 3);
            MULTIORDIVIDECHeck.Name = "MULTIORDIVIDECHeck";
            MULTIORDIVIDECHeck.Size = new Size(174, 19);
            MULTIORDIVIDECHeck.TabIndex = 75;
            MULTIORDIVIDECHeck.Text = "Do math operations on byte";
            MULTIORDIVIDECHeck.UseVisualStyleBackColor = true;
            // 
            // Label25
            // 
            Label25.AutoSize = true;
            Label25.ForeColor = Color.FromArgb(213, 216, 216);
            Label25.Location = new Point(120, 21);
            Label25.Margin = new Padding(2, 0, 2, 0);
            Label25.Name = "Label25";
            Label25.Size = new Size(37, 15);
            Label25.TabIndex = 69;
            Label25.Text = "bit by";
            // 
            // GroupBox3
            // 
            GroupBox3.Controls.Add(ReplaceCHECK);
            GroupBox3.Controls.Add(ReplaceNumericUpDown1);
            GroupBox3.Controls.Add(ReplaceNumericUpDown2);
            GroupBox3.Controls.Add(Label22);
            GroupBox3.Controls.Add(Label16);
            GroupBox3.ForeColor = Color.White;
            GroupBox3.Location = new Point(9, 178);
            GroupBox3.Margin = new Padding(2, 3, 2, 3);
            GroupBox3.Name = "GroupBox3";
            GroupBox3.Padding = new Padding(2, 3, 2, 3);
            GroupBox3.Size = new Size(317, 62);
            GroupBox3.TabIndex = 82;
            GroupBox3.TabStop = false;
            // 
            // ReplaceCHECK
            // 
            ReplaceCHECK.AutoSize = true;
            ReplaceCHECK.ForeColor = Color.FromArgb(213, 216, 216);
            ReplaceCHECK.Location = new Point(7, 1);
            ReplaceCHECK.Margin = new Padding(2, 3, 2, 3);
            ReplaceCHECK.Name = "ReplaceCHECK";
            ReplaceCHECK.Size = new Size(67, 19);
            ReplaceCHECK.TabIndex = 65;
            ReplaceCHECK.Text = "Replace";
            ReplaceCHECK.UseVisualStyleBackColor = true;
            // 
            // ReplaceNumericUpDown1
            // 
            ReplaceNumericUpDown1.BackColor = Color.FromArgb(29, 32, 32);
            ReplaceNumericUpDown1.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ReplaceNumericUpDown1.ForeColor = Color.FromArgb(213, 216, 216);
            ReplaceNumericUpDown1.Location = new Point(70, 22);
            ReplaceNumericUpDown1.Margin = new Padding(2, 3, 2, 3);
            ReplaceNumericUpDown1.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            ReplaceNumericUpDown1.Name = "ReplaceNumericUpDown1";
            ReplaceNumericUpDown1.Size = new Size(93, 22);
            ReplaceNumericUpDown1.TabIndex = 63;
            ReplaceNumericUpDown1.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // ReplaceNumericUpDown2
            // 
            ReplaceNumericUpDown2.BackColor = Color.FromArgb(29, 32, 32);
            ReplaceNumericUpDown2.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ReplaceNumericUpDown2.ForeColor = Color.FromArgb(213, 216, 216);
            ReplaceNumericUpDown2.Location = new Point(209, 22);
            ReplaceNumericUpDown2.Margin = new Padding(2, 3, 2, 3);
            ReplaceNumericUpDown2.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            ReplaceNumericUpDown2.Name = "ReplaceNumericUpDown2";
            ReplaceNumericUpDown2.Size = new Size(93, 22);
            ReplaceNumericUpDown2.TabIndex = 63;
            ReplaceNumericUpDown2.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // Label22
            // 
            Label22.AutoSize = true;
            Label22.ForeColor = Color.FromArgb(213, 216, 216);
            Label22.Location = new Point(7, 24);
            Label22.Margin = new Padding(2, 0, 2, 0);
            Label22.Name = "Label22";
            Label22.Size = new Size(48, 15);
            Label22.TabIndex = 69;
            Label22.Text = "Replace";
            // 
            // Label16
            // 
            Label16.AutoSize = true;
            Label16.ForeColor = Color.FromArgb(213, 216, 216);
            Label16.Location = new Point(169, 28);
            Label16.Margin = new Padding(2, 0, 2, 0);
            Label16.Name = "Label16";
            Label16.Size = new Size(30, 15);
            Label16.TabIndex = 69;
            Label16.Text = "with";
            // 
            // GroupBox4
            // 
            GroupBox4.Controls.Add(SHIFTBYTECHECK);
            GroupBox4.Controls.Add(Label12);
            GroupBox4.Controls.Add(ShiftNumericUpDown);
            GroupBox4.Location = new Point(201, 99);
            GroupBox4.Margin = new Padding(2, 3, 2, 3);
            GroupBox4.Name = "GroupBox4";
            GroupBox4.Padding = new Padding(2, 3, 2, 3);
            GroupBox4.Size = new Size(174, 72);
            GroupBox4.TabIndex = 80;
            GroupBox4.TabStop = false;
            // 
            // SHIFTBYTECHECK
            // 
            SHIFTBYTECHECK.AutoSize = true;
            SHIFTBYTECHECK.ForeColor = Color.FromArgb(213, 216, 216);
            SHIFTBYTECHECK.Location = new Point(4, 0);
            SHIFTBYTECHECK.Margin = new Padding(2, 3, 2, 3);
            SHIFTBYTECHECK.Name = "SHIFTBYTECHECK";
            SHIFTBYTECHECK.Size = new Size(122, 19);
            SHIFTBYTECHECK.TabIndex = 67;
            SHIFTBYTECHECK.Text = "Shift Right X bytes";
            SHIFTBYTECHECK.UseVisualStyleBackColor = true;
            // 
            // Label12
            // 
            Label12.AutoSize = true;
            Label12.Font = new Font("Segoe UI", 9.75F);
            Label12.ForeColor = Color.FromArgb(213, 216, 216);
            Label12.Location = new Point(7, 28);
            Label12.Margin = new Padding(2, 0, 2, 0);
            Label12.Name = "Label12";
            Label12.Size = new Size(54, 17);
            Label12.TabIndex = 16;
            Label12.Text = "Shift int:";
            // 
            // ShiftNumericUpDown
            // 
            ShiftNumericUpDown.BackColor = Color.FromArgb(29, 32, 32);
            ShiftNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ShiftNumericUpDown.ForeColor = Color.FromArgb(213, 216, 216);
            ShiftNumericUpDown.Location = new Point(75, 27);
            ShiftNumericUpDown.Margin = new Padding(2, 3, 2, 3);
            ShiftNumericUpDown.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            ShiftNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            ShiftNumericUpDown.Name = "ShiftNumericUpDown";
            ShiftNumericUpDown.Size = new Size(70, 22);
            ShiftNumericUpDown.TabIndex = 63;
            ShiftNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // RepeatRandomBitCHECK
            // 
            RepeatRandomBitCHECK.AutoSize = true;
            RepeatRandomBitCHECK.ForeColor = Color.FromArgb(213, 216, 216);
            RepeatRandomBitCHECK.Location = new Point(18, 98);
            RepeatRandomBitCHECK.Margin = new Padding(2, 3, 2, 3);
            RepeatRandomBitCHECK.Name = "RepeatRandomBitCHECK";
            RepeatRandomBitCHECK.Size = new Size(88, 19);
            RepeatRandomBitCHECK.TabIndex = 78;
            RepeatRandomBitCHECK.Text = "Repeat byte";
            RepeatRandomBitCHECK.UseVisualStyleBackColor = true;
            // 
            // GroupBox5
            // 
            GroupBox5.Controls.Add(IncrementCHECK);
            GroupBox5.Controls.Add(Label14);
            GroupBox5.Controls.Add(IncrementNumericUpDown);
            GroupBox5.Location = new Point(9, 20);
            GroupBox5.Margin = new Padding(2, 3, 2, 3);
            GroupBox5.Name = "GroupBox5";
            GroupBox5.Padding = new Padding(2, 3, 2, 3);
            GroupBox5.Size = new Size(184, 72);
            GroupBox5.TabIndex = 81;
            GroupBox5.TabStop = false;
            // 
            // IncrementCHECK
            // 
            IncrementCHECK.AutoSize = true;
            IncrementCHECK.ForeColor = Color.FromArgb(213, 216, 216);
            IncrementCHECK.Location = new Point(11, 0);
            IncrementCHECK.Margin = new Padding(2, 3, 2, 3);
            IncrementCHECK.Name = "IncrementCHECK";
            IncrementCHECK.Size = new Size(106, 19);
            IncrementCHECK.TabIndex = 67;
            IncrementCHECK.Text = "Increment byte";
            IncrementCHECK.UseVisualStyleBackColor = true;
            // 
            // Label14
            // 
            Label14.AutoSize = true;
            Label14.Font = new Font("Segoe UI", 9.75F);
            Label14.ForeColor = Color.FromArgb(213, 216, 216);
            Label14.Location = new Point(7, 24);
            Label14.Margin = new Padding(2, 0, 2, 0);
            Label14.Name = "Label14";
            Label14.Size = new Size(90, 17);
            Label14.TabIndex = 16;
            Label14.Text = "Increment Int :";
            // 
            // IncrementNumericUpDown
            // 
            IncrementNumericUpDown.BackColor = Color.FromArgb(29, 32, 32);
            IncrementNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            IncrementNumericUpDown.ForeColor = Color.FromArgb(213, 216, 216);
            IncrementNumericUpDown.Location = new Point(118, 23);
            IncrementNumericUpDown.Margin = new Padding(2, 3, 2, 3);
            IncrementNumericUpDown.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            IncrementNumericUpDown.Minimum = new decimal(new int[] { 99999, 0, 0, int.MinValue });
            IncrementNumericUpDown.Name = "IncrementNumericUpDown";
            IncrementNumericUpDown.Size = new Size(57, 22);
            IncrementNumericUpDown.TabIndex = 63;
            IncrementNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // GroupBox2
            // 
            GroupBox2.Controls.Add(Label8);
            GroupBox2.Controls.Add(RepeatNumericUpDown);
            GroupBox2.Location = new Point(9, 98);
            GroupBox2.Margin = new Padding(2, 3, 2, 3);
            GroupBox2.Name = "GroupBox2";
            GroupBox2.Padding = new Padding(2, 3, 2, 3);
            GroupBox2.Size = new Size(184, 73);
            GroupBox2.TabIndex = 79;
            GroupBox2.TabStop = false;
            // 
            // Label8
            // 
            Label8.AutoSize = true;
            Label8.Font = new Font("Segoe UI", 9.75F);
            Label8.ForeColor = Color.FromArgb(213, 216, 216);
            Label8.Location = new Point(6, 24);
            Label8.Margin = new Padding(2, 0, 2, 0);
            Label8.Name = "Label8";
            Label8.Size = new Size(106, 17);
            Label8.TabIndex = 16;
            Label8.Text = "Selection Length:";
            // 
            // RepeatNumericUpDown
            // 
            RepeatNumericUpDown.BackColor = Color.FromArgb(29, 32, 32);
            RepeatNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            RepeatNumericUpDown.ForeColor = Color.FromArgb(213, 216, 216);
            RepeatNumericUpDown.Location = new Point(131, 23);
            RepeatNumericUpDown.Margin = new Padding(2, 3, 2, 3);
            RepeatNumericUpDown.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
            RepeatNumericUpDown.Name = "RepeatNumericUpDown";
            RepeatNumericUpDown.Size = new Size(48, 22);
            RepeatNumericUpDown.TabIndex = 63;
            RepeatNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // ManualEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(ManualEnginePanel);
            Name = "ManualEngineControl";
            Size = new Size(642, 268);
            ManualEnginePanel.ResumeLayout(false);
            ManualEnginePanel.PerformLayout();
            GroupBox8.ResumeLayout(false);
            GroupBox8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ByteEqualNumericUpDown).EndInit();
            GroupBox7.ResumeLayout(false);
            GroupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)MathOperationNumericUpDown).EndInit();
            GroupBox3.ResumeLayout(false);
            GroupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ReplaceNumericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)ReplaceNumericUpDown2).EndInit();
            GroupBox4.ResumeLayout(false);
            GroupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ShiftNumericUpDown).EndInit();
            GroupBox5.ResumeLayout(false);
            GroupBox5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)IncrementNumericUpDown).EndInit();
            GroupBox2.ResumeLayout(false);
            GroupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)RepeatNumericUpDown).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel ManualEnginePanel;
        private Label Label9;
        internal GroupBox GroupBox8;
        internal CheckBox PasterandombitCHECK;
        internal NumericUpDown ByteEqualNumericUpDown;
        internal CheckBox MakeBitEqualCHECK;
        internal GroupBox GroupBox7;
        internal RadioButton DoubleCheck;
        internal RadioButton DivideRadio;
        internal RadioButton MultiRadio;
        internal NumericUpDown MathOperationNumericUpDown;
        internal CheckBox MULTIORDIVIDECHeck;
        internal Label Label25;
        internal GroupBox GroupBox3;
        internal CheckBox ReplaceCHECK;
        internal NumericUpDown ReplaceNumericUpDown1;
        internal NumericUpDown ReplaceNumericUpDown2;
        internal Label Label22;
        internal Label Label16;
        internal GroupBox GroupBox4;
        internal CheckBox SHIFTBYTECHECK;
        private Label Label12;
        internal NumericUpDown ShiftNumericUpDown;
        internal CheckBox RepeatRandomBitCHECK;
        internal GroupBox GroupBox5;
        internal CheckBox IncrementCHECK;
        private Label Label14;
        internal NumericUpDown IncrementNumericUpDown;
        internal GroupBox GroupBox2;
        private Label Label8;
        internal NumericUpDown RepeatNumericUpDown;
    }
}
