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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NightmareEngineControl));
            label5 = new Label();
            Label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            NightmareComboBox = new ComboBox();
            IncreDecrenumbnightmare = new NumericUpDown();
            ExclusionTypeComboBox = new ComboBox();
            AvoidCrashValuesChkBox = new CheckBox();
            ExcludedAddressesListbox = new ListBox();
            Label23 = new Label();
            NightmareEnginePanel = new Panel();
            HelpAboutEngineLbl = new Label();
            ToolTipHelper = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)IncreDecrenumbnightmare).BeginInit();
            NightmareEnginePanel.SuspendLayout();
            SuspendLayout();
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
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9.75F);
            label2.ForeColor = Color.White;
            label2.Location = new Point(111, 87);
            label2.Name = "label2";
            label2.Size = new Size(95, 17);
            label2.TabIndex = 20;
            label2.Text = "Exclusion Type:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            label3.ForeColor = Color.FromArgb(192, 255, 255);
            label3.Location = new Point(412, 7);
            label3.Name = "label3";
            label3.Size = new Size(133, 17);
            label3.TabIndex = 93;
            label3.Text = "Excluded Addresses:";
            // 
            // NightmareComboBox
            // 
            NightmareComboBox.BackColor = Color.FromArgb(45, 45, 64);
            NightmareComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            NightmareComboBox.FlatStyle = FlatStyle.Flat;
            NightmareComboBox.ForeColor = Color.White;
            NightmareComboBox.FormattingEnabled = true;
            NightmareComboBox.Items.AddRange(new object[] { "RANDOM", "RANDOMTILT", "TILT" });
            NightmareComboBox.Location = new Point(212, 29);
            NightmareComboBox.Name = "NightmareComboBox";
            NightmareComboBox.Size = new Size(191, 21);
            NightmareComboBox.TabIndex = 90;
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
            ToolTipHelper.SetToolTip(IncreDecrenumbnightmare, "TILT / RANDOMTILT: how much is added to or subtracted from the byte (the result is clamped to 0-255).");
            IncreDecrenumbnightmare.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // ExclusionTypeComboBox
            // 
            ExclusionTypeComboBox.BackColor = Color.FromArgb(45, 45, 64);
            ExclusionTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            ExclusionTypeComboBox.FlatStyle = FlatStyle.Flat;
            ExclusionTypeComboBox.ForeColor = Color.White;
            ExclusionTypeComboBox.FormattingEnabled = true;
            ExclusionTypeComboBox.Location = new Point(212, 86);
            ExclusionTypeComboBox.MaxDropDownItems = 16;
            ExclusionTypeComboBox.Name = "ExclusionTypeComboBox";
            ExclusionTypeComboBox.Size = new Size(191, 21);
            ExclusionTypeComboBox.TabIndex = 92;
            ToolTipHelper.SetToolTip(ExclusionTypeComboBox, "None = the classic Nightmare Engine. Auto = picks the type from the loaded file. Any other type keeps that platform's header/boot ranges (listed on the right) safe from corruption.");
            ExclusionTypeComboBox.SelectedIndexChanged += ExclusionTypeComboBox_SelectedIndexChanged;
            // 
            // AvoidCrashValuesChkBox
            // 
            AvoidCrashValuesChkBox.AutoSize = true;
            AvoidCrashValuesChkBox.ForeColor = Color.White;
            AvoidCrashValuesChkBox.Location = new Point(212, 113);
            AvoidCrashValuesChkBox.Name = "AvoidCrashValuesChkBox";
            AvoidCrashValuesChkBox.Size = new Size(155, 17);
            AvoidCrashValuesChkBox.TabIndex = 93;
            AvoidCrashValuesChkBox.Text = "Avoid crash-prone values";
            ToolTipHelper.SetToolTip(AvoidCrashValuesChkBox, "Random values never become 0x00 or 0xF0-0xFF (these crash games more often). Turned on automatically when you pick an exclusion type.");
            AvoidCrashValuesChkBox.UseVisualStyleBackColor = true;
            // 
            // ExcludedAddressesListbox
            // 
            ExcludedAddressesListbox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ExcludedAddressesListbox.BackColor = Color.FromArgb(39, 43, 83);
            ExcludedAddressesListbox.BorderStyle = BorderStyle.None;
            ExcludedAddressesListbox.ForeColor = Color.FromArgb(213, 216, 216);
            ExcludedAddressesListbox.FormattingEnabled = true;
            ExcludedAddressesListbox.HorizontalScrollbar = true;
            ExcludedAddressesListbox.IntegralHeight = false;
            ExcludedAddressesListbox.Location = new Point(412, 27);
            ExcludedAddressesListbox.Name = "ExcludedAddressesListbox";
            ExcludedAddressesListbox.Size = new Size(221, 230);
            ExcludedAddressesListbox.TabIndex = 100;
            // 
            // Label23
            // 
            Label23.AutoSize = true;
            Label23.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            Label23.ForeColor = Color.FromArgb(192, 255, 255);
            Label23.Location = new Point(3, 2);
            Label23.Name = "Label23";
            Label23.Size = new Size(193, 17);
            Label23.TabIndex = 93;
            Label23.Text = "-Nightmare+ Engine Settings-";
            // 
            // NightmareEnginePanel
            // 
            NightmareEnginePanel.BackColor = Color.FromArgb(40, 40, 60);
            NightmareEnginePanel.Controls.Add(ExcludedAddressesListbox);
            NightmareEnginePanel.Controls.Add(AvoidCrashValuesChkBox);
            NightmareEnginePanel.Controls.Add(ExclusionTypeComboBox);
            NightmareEnginePanel.Controls.Add(HelpAboutEngineLbl);
            NightmareEnginePanel.Controls.Add(Label23);
            NightmareEnginePanel.Controls.Add(IncreDecrenumbnightmare);
            NightmareEnginePanel.Controls.Add(NightmareComboBox);
            NightmareEnginePanel.Controls.Add(label3);
            NightmareEnginePanel.Controls.Add(label2);
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
            HelpAboutEngineLbl.Location = new Point(24, 138);
            HelpAboutEngineLbl.Name = "HelpAboutEngineLbl";
            HelpAboutEngineLbl.Size = new Size(380, 122);
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
            Load += NightmareEngineControl_Load;
            ((System.ComponentModel.ISupportInitialize)IncreDecrenumbnightmare).EndInit();
            NightmareEnginePanel.ResumeLayout(false);
            NightmareEnginePanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label5;
        private Label Label1;
        private Label label2;
        private Label label3;
        internal ComboBox NightmareComboBox;
        internal NumericUpDown IncreDecrenumbnightmare;
        internal ComboBox ExclusionTypeComboBox;
        internal CheckBox AvoidCrashValuesChkBox;
        private ListBox ExcludedAddressesListbox;
        private Label Label23;
        public Panel NightmareEnginePanel;
        private Label HelpAboutEngineLbl;
        private ToolTip ToolTipHelper;
    }
}
