namespace LunarROMCorruptor.EngineControls
{
    partial class ExclusionEngineControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExclusionEngineControl));
            LerpEnginePanel = new Panel();
            ExcludedAddressesListbox = new ListBox();
            HelpAboutEngineLbl = new Label();
            IncreDecrenumbnightmare = new NumericUpDown();
            ExclusionTypeComboBox = new ComboBox();
            NightmareComboBox = new ComboBox();
            label2 = new Label();
            Label1 = new Label();
            label5 = new Label();
            label3 = new Label();
            Label20 = new Label();
            LerpEnginePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)IncreDecrenumbnightmare).BeginInit();
            SuspendLayout();
            // 
            // LerpEnginePanel
            // 
            LerpEnginePanel.BackColor = Color.DarkSlateGray;
            LerpEnginePanel.Controls.Add(ExcludedAddressesListbox);
            LerpEnginePanel.Controls.Add(HelpAboutEngineLbl);
            LerpEnginePanel.Controls.Add(IncreDecrenumbnightmare);
            LerpEnginePanel.Controls.Add(ExclusionTypeComboBox);
            LerpEnginePanel.Controls.Add(NightmareComboBox);
            LerpEnginePanel.Controls.Add(label2);
            LerpEnginePanel.Controls.Add(Label1);
            LerpEnginePanel.Controls.Add(label5);
            LerpEnginePanel.Controls.Add(label3);
            LerpEnginePanel.Controls.Add(Label20);
            LerpEnginePanel.Dock = DockStyle.Fill;
            LerpEnginePanel.Location = new Point(0, 0);
            LerpEnginePanel.Name = "LerpEnginePanel";
            LerpEnginePanel.Size = new Size(642, 268);
            LerpEnginePanel.TabIndex = 163;
            LerpEnginePanel.Tag = "color:normal";
            // 
            // ExcludedAddressesListbox
            // 
            ExcludedAddressesListbox.FormattingEnabled = true;
            ExcludedAddressesListbox.IntegralHeight = false;
            ExcludedAddressesListbox.Location = new Point(408, 27);
            ExcludedAddressesListbox.Name = "ExcludedAddressesListbox";
            ExcludedAddressesListbox.Size = new Size(221, 223);
            ExcludedAddressesListbox.TabIndex = 100;
            // 
            // HelpAboutEngineLbl
            // 
            HelpAboutEngineLbl.ForeColor = Color.White;
            HelpAboutEngineLbl.Location = new Point(23, 128);
            HelpAboutEngineLbl.Name = "HelpAboutEngineLbl";
            HelpAboutEngineLbl.Size = new Size(379, 122);
            HelpAboutEngineLbl.TabIndex = 99;
            HelpAboutEngineLbl.Text = resources.GetString("HelpAboutEngineLbl.Text");
            // 
            // IncreDecrenumbnightmare
            // 
            IncreDecrenumbnightmare.BackColor = Color.FromArgb(67, 67, 81);
            IncreDecrenumbnightmare.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            IncreDecrenumbnightmare.ForeColor = Color.White;
            IncreDecrenumbnightmare.Location = new Point(211, 54);
            IncreDecrenumbnightmare.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            IncreDecrenumbnightmare.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            IncreDecrenumbnightmare.Name = "IncreDecrenumbnightmare";
            IncreDecrenumbnightmare.Size = new Size(191, 22);
            IncreDecrenumbnightmare.TabIndex = 98;
            IncreDecrenumbnightmare.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // ExclusionTypeComboBox
            // 
            ExclusionTypeComboBox.BackColor = Color.FromArgb(45, 45, 64);
            ExclusionTypeComboBox.FlatStyle = FlatStyle.Flat;
            ExclusionTypeComboBox.ForeColor = Color.White;
            ExclusionTypeComboBox.FormattingEnabled = true;
            ExclusionTypeComboBox.Items.AddRange(new object[] { "NES", "SNES", "N64", "GAMECUBE", "PS1", "PS2", "EXE", "SEGA", "GAMEBOY", "WII", "DREAMCAST", "BIOS" });
            ExclusionTypeComboBox.Location = new Point(211, 82);
            ExclusionTypeComboBox.Name = "ExclusionTypeComboBox";
            ExclusionTypeComboBox.Size = new Size(191, 23);
            ExclusionTypeComboBox.TabIndex = 97;
            ExclusionTypeComboBox.Text = "NES";
            ExclusionTypeComboBox.SelectedIndexChanged += ExclusionTypeComboBox_SelectedIndexChanged;
            // 
            // NightmareComboBox
            // 
            NightmareComboBox.BackColor = Color.FromArgb(45, 45, 64);
            NightmareComboBox.FlatStyle = FlatStyle.Flat;
            NightmareComboBox.ForeColor = Color.White;
            NightmareComboBox.FormattingEnabled = true;
            NightmareComboBox.Items.AddRange(new object[] { "RANDOM", "RANDOMTILT", "TILT" });
            NightmareComboBox.Location = new Point(211, 27);
            NightmareComboBox.Name = "NightmareComboBox";
            NightmareComboBox.Size = new Size(191, 23);
            NightmareComboBox.TabIndex = 97;
            NightmareComboBox.Text = "RANDOM";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9.75F);
            label2.ForeColor = Color.White;
            label2.Location = new Point(110, 83);
            label2.Name = "label2";
            label2.Size = new Size(95, 17);
            label2.TabIndex = 96;
            label2.Text = "Exclusion Type:";
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Font = new Font("Segoe UI", 9.75F);
            Label1.ForeColor = Color.White;
            Label1.Location = new Point(23, 56);
            Label1.Name = "Label1";
            Label1.Size = new Size(182, 17);
            Label1.TabIndex = 95;
            Label1.Text = "Increment/Decrement number";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9.75F);
            label5.ForeColor = Color.White;
            label5.Location = new Point(100, 27);
            label5.Name = "label5";
            label5.Size = new Size(105, 17);
            label5.TabIndex = 96;
            label5.Text = "Corruption Type:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            label3.ForeColor = Color.FromArgb(192, 255, 255);
            label3.Location = new Point(408, 7);
            label3.Name = "label3";
            label3.Size = new Size(133, 17);
            label3.TabIndex = 93;
            label3.Text = "Excluded Addresses:";
            // 
            // Label20
            // 
            Label20.AutoSize = true;
            Label20.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            Label20.ForeColor = Color.FromArgb(192, 255, 255);
            Label20.Location = new Point(3, 2);
            Label20.Name = "Label20";
            Label20.Size = new Size(176, 17);
            Label20.TabIndex = 93;
            Label20.Text = "-Exclusion Engine Settings-";
            // 
            // ExclusionEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(LerpEnginePanel);
            Name = "ExclusionEngineControl";
            Size = new Size(642, 268);
            Load += ExclusionEngineControl_Load;
            LerpEnginePanel.ResumeLayout(false);
            LerpEnginePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)IncreDecrenumbnightmare).EndInit();
            ResumeLayout(false);
        }

        #endregion

        public Panel LerpEnginePanel;
        private Label Label20;
        private Label HelpAboutEngineLbl;
        internal NumericUpDown IncreDecrenumbnightmare;
        internal ComboBox NightmareComboBox;
        private Label Label1;
        private Label label5;
        internal ComboBox ExclusionTypeComboBox;
        private Label label2;
        private ListBox ExcludedAddressesListbox;
        private Label label3;
    }
}
