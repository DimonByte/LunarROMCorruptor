namespace LunarROMCorruptor
{
    partial class NightmareEngineControl
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NightmareEngineControl));
            OpenFileDialog1 = new OpenFileDialog();
            label5 = new Label();
            Label1 = new Label();
            NightmareComboBox = new ComboBox();
            IncreDecrenumbnightmare = new NumericUpDown();
            Label23 = new Label();
            NightmareEnginePanel = new Panel();
            HelpAboutEngineLbl = new Label();
            ((System.ComponentModel.ISupportInitialize)IncreDecrenumbnightmare).BeginInit();
            NightmareEnginePanel.SuspendLayout();
            SuspendLayout();
            // 
            // OpenFileDialog1
            // 
            OpenFileDialog1.Filter = "All files (*.*)|*.*";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9.75F);
            label5.ForeColor = Color.White;
            label5.Location = new Point(101, 29);
            label5.Name = "label5";
            label5.Size = new Size(105, 17);
            label5.TabIndex = 20;
            label5.Text = "Corruption Type:";
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Font = new Font("Segoe UI", 9.75F);
            Label1.ForeColor = Color.White;
            Label1.Location = new Point(24, 58);
            Label1.Name = "Label1";
            Label1.Size = new Size(182, 17);
            Label1.TabIndex = 20;
            Label1.Text = "Increment/Decrement number";
            // 
            // NightmareComboBox
            // 
            NightmareComboBox.BackColor = Color.FromArgb(45, 45, 64);
            NightmareComboBox.FlatStyle = FlatStyle.Flat;
            NightmareComboBox.ForeColor = Color.White;
            NightmareComboBox.FormattingEnabled = true;
            NightmareComboBox.Items.AddRange(new object[] { "RANDOM", "RANDOMTILT", "TILT" });
            NightmareComboBox.Location = new Point(212, 29);
            NightmareComboBox.Name = "NightmareComboBox";
            NightmareComboBox.Size = new Size(191, 21);
            NightmareComboBox.TabIndex = 90;
            NightmareComboBox.Text = "RANDOM";
            // 
            // IncreDecrenumbnightmare
            // 
            IncreDecrenumbnightmare.BackColor = Color.FromArgb(67, 67, 81);
            IncreDecrenumbnightmare.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            IncreDecrenumbnightmare.ForeColor = Color.White;
            IncreDecrenumbnightmare.Location = new Point(212, 56);
            IncreDecrenumbnightmare.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            IncreDecrenumbnightmare.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            IncreDecrenumbnightmare.Name = "IncreDecrenumbnightmare";
            IncreDecrenumbnightmare.Size = new Size(191, 22);
            IncreDecrenumbnightmare.TabIndex = 91;
            IncreDecrenumbnightmare.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // Label23
            // 
            Label23.AutoSize = true;
            Label23.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            Label23.ForeColor = Color.FromArgb(192, 255, 255);
            Label23.Location = new Point(3, 2);
            Label23.Name = "Label23";
            Label23.Size = new Size(184, 17);
            Label23.TabIndex = 93;
            Label23.Text = "-Nightmare Engine Settings-";
            // 
            // NightmareEnginePanel
            // 
            NightmareEnginePanel.BackColor = Color.FromArgb(40, 40, 60);
            NightmareEnginePanel.Controls.Add(HelpAboutEngineLbl);
            NightmareEnginePanel.Controls.Add(Label23);
            NightmareEnginePanel.Controls.Add(IncreDecrenumbnightmare);
            NightmareEnginePanel.Controls.Add(NightmareComboBox);
            NightmareEnginePanel.Controls.Add(Label1);
            NightmareEnginePanel.Controls.Add(label5);
            NightmareEnginePanel.Dock = DockStyle.Fill;
            NightmareEnginePanel.Location = new Point(0, 0);
            NightmareEnginePanel.Name = "NightmareEnginePanel";
            NightmareEnginePanel.Size = new Size(642, 268);
            NightmareEnginePanel.TabIndex = 156;
            NightmareEnginePanel.Tag = "color:normal";
            // 
            // HelpAboutEngineLbl
            // 
            HelpAboutEngineLbl.ForeColor = Color.White;
            HelpAboutEngineLbl.Location = new Point(24, 84);
            HelpAboutEngineLbl.Name = "HelpAboutEngineLbl";
            HelpAboutEngineLbl.Size = new Size(571, 52);
            HelpAboutEngineLbl.TabIndex = 94;
            HelpAboutEngineLbl.Text = resources.GetString("HelpAboutEngineLbl.Text");
            // 
            // NightmareEngineControl
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            Controls.Add(NightmareEnginePanel);
            Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "NightmareEngineControl";
            Size = new Size(642, 268);
            ((System.ComponentModel.ISupportInitialize)IncreDecrenumbnightmare).EndInit();
            NightmareEnginePanel.ResumeLayout(false);
            NightmareEnginePanel.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.OpenFileDialog OpenFileDialog1;
        private Label label5;
        private Label Label1;
        internal ComboBox NightmareComboBox;
        internal NumericUpDown IncreDecrenumbnightmare;
        private Label Label23;
        public Panel NightmareEnginePanel;
        private Label HelpAboutEngineLbl;
    }
}