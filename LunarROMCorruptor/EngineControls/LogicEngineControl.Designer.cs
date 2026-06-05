namespace LunarROMCorruptor.EngineControls
{
    partial class LogicEngineControl
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
            LogicEnginePanel = new Panel();
            BitwiseComboBox = new ComboBox();
            LogicRandomizeValueCheckBox = new CheckBox();
            LogicRandomizeTypeCheckbox = new CheckBox();
            ValueBitwise = new NumericUpDown();
            Label14 = new Label();
            Label12 = new Label();
            Label10 = new Label();
            Label11 = new Label();
            LogicEnginePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ValueBitwise).BeginInit();
            SuspendLayout();
            // 
            // LogicEnginePanel
            // 
            LogicEnginePanel.BackColor = Color.Indigo;
            LogicEnginePanel.Controls.Add(BitwiseComboBox);
            LogicEnginePanel.Controls.Add(LogicRandomizeValueCheckBox);
            LogicEnginePanel.Controls.Add(LogicRandomizeTypeCheckbox);
            LogicEnginePanel.Controls.Add(ValueBitwise);
            LogicEnginePanel.Controls.Add(Label14);
            LogicEnginePanel.Controls.Add(Label12);
            LogicEnginePanel.Controls.Add(Label10);
            LogicEnginePanel.Controls.Add(Label11);
            LogicEnginePanel.Dock = DockStyle.Fill;
            LogicEnginePanel.Location = new Point(0, 0);
            LogicEnginePanel.Name = "LogicEnginePanel";
            LogicEnginePanel.Size = new Size(642, 268);
            LogicEnginePanel.TabIndex = 160;
            LogicEnginePanel.Tag = "color:normal";
            // 
            // BitwiseComboBox
            // 
            BitwiseComboBox.BackColor = Color.FromArgb(45, 45, 64);
            BitwiseComboBox.FlatStyle = FlatStyle.Flat;
            BitwiseComboBox.ForeColor = Color.White;
            BitwiseComboBox.FormattingEnabled = true;
            BitwiseComboBox.Items.AddRange(new object[] { "AND", "OR", "XOR", "NOT", "NAND", "NOR", "SWAP", "SHIFT" });
            BitwiseComboBox.Location = new Point(118, 27);
            BitwiseComboBox.Name = "BitwiseComboBox";
            BitwiseComboBox.Size = new Size(111, 23);
            BitwiseComboBox.TabIndex = 167;
            BitwiseComboBox.Text = "AND";
            // 
            // LogicRandomizeValueCheckBox
            // 
            LogicRandomizeValueCheckBox.AutoSize = true;
            LogicRandomizeValueCheckBox.ForeColor = Color.White;
            LogicRandomizeValueCheckBox.Location = new Point(235, 57);
            LogicRandomizeValueCheckBox.Name = "LogicRandomizeValueCheckBox";
            LogicRandomizeValueCheckBox.Size = new Size(116, 19);
            LogicRandomizeValueCheckBox.TabIndex = 96;
            LogicRandomizeValueCheckBox.Text = "Randomize Value";
            LogicRandomizeValueCheckBox.UseVisualStyleBackColor = true;
            // 
            // LogicRandomizeTypeCheckbox
            // 
            LogicRandomizeTypeCheckbox.AutoSize = true;
            LogicRandomizeTypeCheckbox.ForeColor = Color.White;
            LogicRandomizeTypeCheckbox.Location = new Point(235, 28);
            LogicRandomizeTypeCheckbox.Name = "LogicRandomizeTypeCheckbox";
            LogicRandomizeTypeCheckbox.Size = new Size(182, 19);
            LogicRandomizeTypeCheckbox.TabIndex = 96;
            LogicRandomizeTypeCheckbox.Text = "Select random operation type";
            LogicRandomizeTypeCheckbox.UseVisualStyleBackColor = true;
            // 
            // ValueBitwise
            // 
            ValueBitwise.BackColor = Color.FromArgb(67, 67, 81);
            ValueBitwise.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ValueBitwise.ForeColor = Color.White;
            ValueBitwise.Location = new Point(118, 54);
            ValueBitwise.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            ValueBitwise.Name = "ValueBitwise";
            ValueBitwise.Size = new Size(111, 22);
            ValueBitwise.TabIndex = 95;
            ValueBitwise.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // Label14
            // 
            Label14.AutoSize = true;
            Label14.Font = new Font("Segoe UI", 9F);
            Label14.ForeColor = Color.FromArgb(224, 224, 224);
            Label14.Location = new Point(18, 85);
            Label14.Name = "Label14";
            Label14.Size = new Size(245, 15);
            Label14.TabIndex = 94;
            Label14.Text = "The Operation Type: NOT does not use value.";
            // 
            // Label12
            // 
            Label12.AutoSize = true;
            Label12.Font = new Font("Segoe UI", 9.75F);
            Label12.ForeColor = Color.White;
            Label12.Location = new Point(16, 27);
            Label12.Name = "Label12";
            Label12.Size = new Size(101, 17);
            Label12.TabIndex = 94;
            Label12.Text = "Operation Type:";
            // 
            // Label10
            // 
            Label10.AutoSize = true;
            Label10.Font = new Font("Segoe UI", 9.75F);
            Label10.ForeColor = Color.White;
            Label10.Location = new Point(74, 56);
            Label10.Name = "Label10";
            Label10.Size = new Size(42, 17);
            Label10.TabIndex = 94;
            Label10.Text = "Value:";
            // 
            // Label11
            // 
            Label11.AutoSize = true;
            Label11.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            Label11.ForeColor = Color.FromArgb(192, 255, 255);
            Label11.Location = new Point(3, 2);
            Label11.Name = "Label11";
            Label11.Size = new Size(151, 17);
            Label11.TabIndex = 93;
            Label11.Text = "-Logic Engine Settings-";
            // 
            // LogicEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(LogicEnginePanel);
            Name = "LogicEngineControl";
            Size = new Size(642, 268);
            LogicEnginePanel.ResumeLayout(false);
            LogicEnginePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ValueBitwise).EndInit();
            ResumeLayout(false);
        }

        #endregion

        public Panel LogicEnginePanel;
        internal ComboBox BitwiseComboBox;
        internal CheckBox LogicRandomizeValueCheckBox;
        internal CheckBox LogicRandomizeTypeCheckbox;
        internal NumericUpDown ValueBitwise;
        private Label Label14;
        private Label Label12;
        private Label Label10;
        private Label Label11;
    }
}
