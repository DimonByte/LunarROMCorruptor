namespace LunarROMCorruptor
{
    partial class CorruptionEngineOptions
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
            this.OpenFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.CorrTypeMerge = new System.Windows.Forms.ComboBox();
            this.Mod256MergeEnginechkbox = new System.Windows.Forms.CheckBox();
            this.ReplaceByteWithSamePos = new System.Windows.Forms.CheckBox();
            this.LogicEnginePanel = new System.Windows.Forms.Panel();
            this.BitwiseComboBox = new System.Windows.Forms.ComboBox();
            this.LogicRandomizeValueCheckBox = new System.Windows.Forms.CheckBox();
            this.LogicRandomizeTypeCheckbox = new System.Windows.Forms.CheckBox();
            this.ValueBitwise = new System.Windows.Forms.NumericUpDown();
            this.Label14 = new System.Windows.Forms.Label();
            this.Label12 = new System.Windows.Forms.Label();
            this.Label10 = new System.Windows.Forms.Label();
            this.Label11 = new System.Windows.Forms.Label();
            this.Label15 = new System.Windows.Forms.Label();
            this.LerpEnginePanel = new System.Windows.Forms.Panel();
            this.LerpSplitValueTrackBar = new System.Windows.Forms.TrackBar();
            this.LerpValueTxt = new System.Windows.Forms.TextBox();
            this.Label17 = new System.Windows.Forms.Label();
            this.Label20 = new System.Windows.Forms.Label();
            this.Vector2EnginePanel = new System.Windows.Forms.Panel();
            this.FloatBitOutput = new System.Windows.Forms.RichTextBox();
            this.ChunkSearchVector2rb = new System.Windows.Forms.RadioButton();
            this.RandomSearchVector2rb = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.Label19 = new System.Windows.Forms.Label();
            this.Label22 = new System.Windows.Forms.Label();
            this.Label21 = new System.Windows.Forms.Label();
            this.Label13 = new System.Windows.Forms.Label();
            this.NightmareEnginePanel = new System.Windows.Forms.Panel();
            this.Label23 = new System.Windows.Forms.Label();
            this.IncreDecrenumbnightmare = new System.Windows.Forms.NumericUpDown();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.NightmareComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.MergeOpenFilebtn = new System.Windows.Forms.Button();
            this.MergeEnginePanel = new System.Windows.Forms.Panel();
            this.MergeFileLocationTxt = new System.Windows.Forms.TextBox();
            this.Label9 = new System.Windows.Forms.Label();
            this.LogicEnginePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ValueBitwise)).BeginInit();
            this.LerpEnginePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LerpSplitValueTrackBar)).BeginInit();
            this.Vector2EnginePanel.SuspendLayout();
            this.NightmareEnginePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IncreDecrenumbnightmare)).BeginInit();
            this.MergeEnginePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // OpenFileDialog1
            // 
            this.OpenFileDialog1.Filter = "All files (*.*)|*.*";
            // 
            // CorrTypeMerge
            // 
            this.CorrTypeMerge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CorrTypeMerge.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(64)))));
            this.CorrTypeMerge.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CorrTypeMerge.ForeColor = System.Drawing.Color.White;
            this.CorrTypeMerge.FormattingEnabled = true;
            this.CorrTypeMerge.Items.AddRange(new object[] {
            "NONE",
            "RANGE"});
            this.CorrTypeMerge.Location = new System.Drawing.Point(498, 18);
            this.CorrTypeMerge.Name = "CorrTypeMerge";
            this.CorrTypeMerge.Size = new System.Drawing.Size(104, 21);
            this.CorrTypeMerge.TabIndex = 99;
            this.CorrTypeMerge.Text = "NONE";
            // 
            // Mod256MergeEnginechkbox
            // 
            this.Mod256MergeEnginechkbox.AutoSize = true;
            this.Mod256MergeEnginechkbox.BackColor = System.Drawing.Color.Transparent;
            this.Mod256MergeEnginechkbox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Mod256MergeEnginechkbox.ForeColor = System.Drawing.Color.White;
            this.Mod256MergeEnginechkbox.Location = new System.Drawing.Point(403, 69);
            this.Mod256MergeEnginechkbox.Name = "Mod256MergeEnginechkbox";
            this.Mod256MergeEnginechkbox.Size = new System.Drawing.Size(71, 17);
            this.Mod256MergeEnginechkbox.TabIndex = 98;
            this.Mod256MergeEnginechkbox.Text = "Mod 256";
            this.Mod256MergeEnginechkbox.UseVisualStyleBackColor = false;
            // 
            // ReplaceByteWithSamePos
            // 
            this.ReplaceByteWithSamePos.AutoSize = true;
            this.ReplaceByteWithSamePos.BackColor = System.Drawing.Color.Transparent;
            this.ReplaceByteWithSamePos.Checked = true;
            this.ReplaceByteWithSamePos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ReplaceByteWithSamePos.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReplaceByteWithSamePos.ForeColor = System.Drawing.Color.White;
            this.ReplaceByteWithSamePos.Location = new System.Drawing.Point(127, 69);
            this.ReplaceByteWithSamePos.Name = "ReplaceByteWithSamePos";
            this.ReplaceByteWithSamePos.Size = new System.Drawing.Size(270, 17);
            this.ReplaceByteWithSamePos.TabIndex = 98;
            this.ReplaceByteWithSamePos.Text = "Replace byte with the byte at the same position";
            this.ReplaceByteWithSamePos.UseVisualStyleBackColor = false;
            // 
            // LogicEnginePanel
            // 
            this.LogicEnginePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LogicEnginePanel.BackColor = System.Drawing.Color.Indigo;
            this.LogicEnginePanel.Controls.Add(this.BitwiseComboBox);
            this.LogicEnginePanel.Controls.Add(this.LogicRandomizeValueCheckBox);
            this.LogicEnginePanel.Controls.Add(this.LogicRandomizeTypeCheckbox);
            this.LogicEnginePanel.Controls.Add(this.ValueBitwise);
            this.LogicEnginePanel.Controls.Add(this.Label14);
            this.LogicEnginePanel.Controls.Add(this.Label12);
            this.LogicEnginePanel.Controls.Add(this.Label10);
            this.LogicEnginePanel.Controls.Add(this.Label11);
            this.LogicEnginePanel.Location = new System.Drawing.Point(12, 14);
            this.LogicEnginePanel.Name = "LogicEnginePanel";
            this.LogicEnginePanel.Size = new System.Drawing.Size(618, 240);
            this.LogicEnginePanel.TabIndex = 159;
            this.LogicEnginePanel.Tag = "color:normal";
            // 
            // BitwiseComboBox
            // 
            this.BitwiseComboBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(64)))));
            this.BitwiseComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BitwiseComboBox.ForeColor = System.Drawing.Color.White;
            this.BitwiseComboBox.FormattingEnabled = true;
            this.BitwiseComboBox.Items.AddRange(new object[] {
            "AND",
            "OR",
            "XOR",
            "NOT",
            "NAND",
            "NOR",
            "SWAP",
            "SHIFT"});
            this.BitwiseComboBox.Location = new System.Drawing.Point(118, 27);
            this.BitwiseComboBox.Name = "BitwiseComboBox";
            this.BitwiseComboBox.Size = new System.Drawing.Size(111, 21);
            this.BitwiseComboBox.TabIndex = 167;
            this.BitwiseComboBox.Text = "AND";
            // 
            // LogicRandomizeValueCheckBox
            // 
            this.LogicRandomizeValueCheckBox.AutoSize = true;
            this.LogicRandomizeValueCheckBox.ForeColor = System.Drawing.Color.White;
            this.LogicRandomizeValueCheckBox.Location = new System.Drawing.Point(235, 57);
            this.LogicRandomizeValueCheckBox.Name = "LogicRandomizeValueCheckBox";
            this.LogicRandomizeValueCheckBox.Size = new System.Drawing.Size(114, 17);
            this.LogicRandomizeValueCheckBox.TabIndex = 96;
            this.LogicRandomizeValueCheckBox.Text = "Randomize Value";
            this.LogicRandomizeValueCheckBox.UseVisualStyleBackColor = true;
            // 
            // LogicRandomizeTypeCheckbox
            // 
            this.LogicRandomizeTypeCheckbox.AutoSize = true;
            this.LogicRandomizeTypeCheckbox.ForeColor = System.Drawing.Color.White;
            this.LogicRandomizeTypeCheckbox.Location = new System.Drawing.Point(235, 28);
            this.LogicRandomizeTypeCheckbox.Name = "LogicRandomizeTypeCheckbox";
            this.LogicRandomizeTypeCheckbox.Size = new System.Drawing.Size(178, 17);
            this.LogicRandomizeTypeCheckbox.TabIndex = 96;
            this.LogicRandomizeTypeCheckbox.Text = "Select random operation type";
            this.LogicRandomizeTypeCheckbox.UseVisualStyleBackColor = true;
            // 
            // ValueBitwise
            // 
            this.ValueBitwise.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(81)))));
            this.ValueBitwise.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ValueBitwise.ForeColor = System.Drawing.Color.White;
            this.ValueBitwise.Location = new System.Drawing.Point(118, 54);
            this.ValueBitwise.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.ValueBitwise.Name = "ValueBitwise";
            this.ValueBitwise.Size = new System.Drawing.Size(111, 22);
            this.ValueBitwise.TabIndex = 95;
            this.ValueBitwise.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // Label14
            // 
            this.Label14.AutoSize = true;
            this.Label14.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Label14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Label14.Location = new System.Drawing.Point(18, 85);
            this.Label14.Name = "Label14";
            this.Label14.Size = new System.Drawing.Size(245, 15);
            this.Label14.TabIndex = 94;
            this.Label14.Text = "The Operation Type: NOT does not use value.";
            // 
            // Label12
            // 
            this.Label12.AutoSize = true;
            this.Label12.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Label12.ForeColor = System.Drawing.Color.White;
            this.Label12.Location = new System.Drawing.Point(16, 27);
            this.Label12.Name = "Label12";
            this.Label12.Size = new System.Drawing.Size(101, 17);
            this.Label12.TabIndex = 94;
            this.Label12.Text = "Operation Type:";
            // 
            // Label10
            // 
            this.Label10.AutoSize = true;
            this.Label10.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Label10.ForeColor = System.Drawing.Color.White;
            this.Label10.Location = new System.Drawing.Point(74, 56);
            this.Label10.Name = "Label10";
            this.Label10.Size = new System.Drawing.Size(42, 17);
            this.Label10.TabIndex = 94;
            this.Label10.Text = "Value:";
            // 
            // Label11
            // 
            this.Label11.AutoSize = true;
            this.Label11.Font = new System.Drawing.Font("Segoe UI", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.Label11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Label11.Location = new System.Drawing.Point(3, 2);
            this.Label11.Name = "Label11";
            this.Label11.Size = new System.Drawing.Size(151, 17);
            this.Label11.TabIndex = 93;
            this.Label11.Text = "-Logic Engine Settings-";
            // 
            // Label15
            // 
            this.Label15.AutoSize = true;
            this.Label15.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Label15.Location = new System.Drawing.Point(368, 17);
            this.Label15.Name = "Label15";
            this.Label15.Size = new System.Drawing.Size(124, 20);
            this.Label15.TabIndex = 97;
            this.Label15.Text = "Corruption Type:";
            // 
            // LerpEnginePanel
            // 
            this.LerpEnginePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LerpEnginePanel.BackColor = System.Drawing.Color.DarkOliveGreen;
            this.LerpEnginePanel.Controls.Add(this.LerpSplitValueTrackBar);
            this.LerpEnginePanel.Controls.Add(this.LerpValueTxt);
            this.LerpEnginePanel.Controls.Add(this.Label17);
            this.LerpEnginePanel.Controls.Add(this.Label20);
            this.LerpEnginePanel.Location = new System.Drawing.Point(12, 14);
            this.LerpEnginePanel.Name = "LerpEnginePanel";
            this.LerpEnginePanel.Size = new System.Drawing.Size(618, 240);
            this.LerpEnginePanel.TabIndex = 161;
            this.LerpEnginePanel.Tag = "color:normal";
            // 
            // LerpSplitValueTrackBar
            // 
            this.LerpSplitValueTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LerpSplitValueTrackBar.Location = new System.Drawing.Point(11, 53);
            this.LerpSplitValueTrackBar.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.LerpSplitValueTrackBar.Name = "LerpSplitValueTrackBar";
            this.LerpSplitValueTrackBar.Size = new System.Drawing.Size(592, 45);
            this.LerpSplitValueTrackBar.TabIndex = 98;
            this.LerpSplitValueTrackBar.TabStop = false;
            this.LerpSplitValueTrackBar.TickFrequency = 0;
            this.LerpSplitValueTrackBar.Value = 5;
            this.LerpSplitValueTrackBar.Scroll += new System.EventHandler(this.LerpSplitValueTrackBar_Scroll);
            // 
            // LerpValueTxt
            // 
            this.LerpValueTxt.Location = new System.Drawing.Point(78, 25);
            this.LerpValueTxt.Name = "LerpValueTxt";
            this.LerpValueTxt.Size = new System.Drawing.Size(100, 22);
            this.LerpValueTxt.TabIndex = 97;
            this.LerpValueTxt.Text = "0.5";
            this.LerpValueTxt.TextChanged += new System.EventHandler(this.LerpValueTxt_TextChanged);
            // 
            // Label17
            // 
            this.Label17.AutoSize = true;
            this.Label17.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Label17.ForeColor = System.Drawing.Color.White;
            this.Label17.Location = new System.Drawing.Point(9, 26);
            this.Label17.Name = "Label17";
            this.Label17.Size = new System.Drawing.Size(71, 17);
            this.Label17.TabIndex = 96;
            this.Label17.Text = "Split Value:";
            // 
            // Label20
            // 
            this.Label20.AutoSize = true;
            this.Label20.Font = new System.Drawing.Font("Segoe UI", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.Label20.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Label20.Location = new System.Drawing.Point(3, 2);
            this.Label20.Name = "Label20";
            this.Label20.Size = new System.Drawing.Size(145, 17);
            this.Label20.TabIndex = 93;
            this.Label20.Text = "-Lerp Engine Settings-";
            // 
            // Vector2EnginePanel
            // 
            this.Vector2EnginePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Vector2EnginePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(56)))), ((int)(((byte)(71)))));
            this.Vector2EnginePanel.Controls.Add(this.FloatBitOutput);
            this.Vector2EnginePanel.Controls.Add(this.ChunkSearchVector2rb);
            this.Vector2EnginePanel.Controls.Add(this.RandomSearchVector2rb);
            this.Vector2EnginePanel.Controls.Add(this.label3);
            this.Vector2EnginePanel.Controls.Add(this.Label19);
            this.Vector2EnginePanel.Controls.Add(this.Label22);
            this.Vector2EnginePanel.Controls.Add(this.Label21);
            this.Vector2EnginePanel.Location = new System.Drawing.Point(12, 14);
            this.Vector2EnginePanel.Name = "Vector2EnginePanel";
            this.Vector2EnginePanel.Size = new System.Drawing.Size(618, 240);
            this.Vector2EnginePanel.TabIndex = 162;
            this.Vector2EnginePanel.Tag = "color:normal";
            // 
            // FloatBitOutput
            // 
            this.FloatBitOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FloatBitOutput.Location = new System.Drawing.Point(16, 92);
            this.FloatBitOutput.Name = "FloatBitOutput";
            this.FloatBitOutput.Size = new System.Drawing.Size(577, 135);
            this.FloatBitOutput.TabIndex = 95;
            this.FloatBitOutput.Text = "";
            // 
            // ChunkSearchVector2rb
            // 
            this.ChunkSearchVector2rb.AutoSize = true;
            this.ChunkSearchVector2rb.ForeColor = System.Drawing.Color.White;
            this.ChunkSearchVector2rb.Location = new System.Drawing.Point(139, 50);
            this.ChunkSearchVector2rb.Name = "ChunkSearchVector2rb";
            this.ChunkSearchVector2rb.Size = new System.Drawing.Size(96, 17);
            this.ChunkSearchVector2rb.TabIndex = 94;
            this.ChunkSearchVector2rb.Text = "Chunk Search";
            this.ChunkSearchVector2rb.UseVisualStyleBackColor = true;
            // 
            // RandomSearchVector2rb
            // 
            this.RandomSearchVector2rb.AutoSize = true;
            this.RandomSearchVector2rb.Checked = true;
            this.RandomSearchVector2rb.ForeColor = System.Drawing.Color.White;
            this.RandomSearchVector2rb.Location = new System.Drawing.Point(28, 49);
            this.RandomSearchVector2rb.Name = "RandomSearchVector2rb";
            this.RandomSearchVector2rb.Size = new System.Drawing.Size(105, 17);
            this.RandomSearchVector2rb.TabIndex = 94;
            this.RandomSearchVector2rb.TabStop = true;
            this.RandomSearchVector2rb.Text = "Random Search";
            this.RandomSearchVector2rb.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(13, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(119, 17);
            this.label3.TabIndex = 93;
            this.label3.Text = "32bit output of file:";
            // 
            // Label19
            // 
            this.Label19.AutoSize = true;
            this.Label19.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Label19.ForeColor = System.Drawing.Color.White;
            this.Label19.Location = new System.Drawing.Point(8, 26);
            this.Label19.Name = "Label19";
            this.Label19.Size = new System.Drawing.Size(79, 17);
            this.Label19.TabIndex = 93;
            this.Label19.Text = "Search type:";
            // 
            // Label22
            // 
            this.Label22.AutoSize = true;
            this.Label22.Font = new System.Drawing.Font("Segoe UI", 9.75F, ((System.Drawing.FontStyle)(((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic) 
                | System.Drawing.FontStyle.Underline))));
            this.Label22.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.Label22.Location = new System.Drawing.Point(388, 11);
            this.Label22.Name = "Label22";
            this.Label22.Size = new System.Drawing.Size(215, 34);
            this.Label22.TabIndex = 93;
            this.Label22.Text = "ENGINE DOES NOT WORK \r\nWITH CORRUPT NTH BYTE MODE";
            // 
            // Label21
            // 
            this.Label21.AutoSize = true;
            this.Label21.Font = new System.Drawing.Font("Segoe UI", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.Label21.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Label21.Location = new System.Drawing.Point(3, 2);
            this.Label21.Name = "Label21";
            this.Label21.Size = new System.Drawing.Size(164, 17);
            this.Label21.TabIndex = 93;
            this.Label21.Text = "-Vector2 Engine Settings-";
            // 
            // Label13
            // 
            this.Label13.AutoSize = true;
            this.Label13.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Label13.Location = new System.Drawing.Point(123, 19);
            this.Label13.Name = "Label13";
            this.Label13.Size = new System.Drawing.Size(103, 20);
            this.Label13.TabIndex = 97;
            this.Label13.Text = "File Selection:";
            // 
            // NightmareEnginePanel
            // 
            this.NightmareEnginePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NightmareEnginePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(60)))));
            this.NightmareEnginePanel.Controls.Add(this.Label23);
            this.NightmareEnginePanel.Controls.Add(this.IncreDecrenumbnightmare);
            this.NightmareEnginePanel.Controls.Add(this.comboBox1);
            this.NightmareEnginePanel.Controls.Add(this.NightmareComboBox);
            this.NightmareEnginePanel.Controls.Add(this.label2);
            this.NightmareEnginePanel.Controls.Add(this.Label1);
            this.NightmareEnginePanel.Controls.Add(this.label5);
            this.NightmareEnginePanel.Location = new System.Drawing.Point(12, 12);
            this.NightmareEnginePanel.Name = "NightmareEnginePanel";
            this.NightmareEnginePanel.Size = new System.Drawing.Size(618, 244);
            this.NightmareEnginePanel.TabIndex = 156;
            this.NightmareEnginePanel.Tag = "color:normal";
            // 
            // Label23
            // 
            this.Label23.AutoSize = true;
            this.Label23.Font = new System.Drawing.Font("Segoe UI", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.Label23.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Label23.Location = new System.Drawing.Point(3, 2);
            this.Label23.Name = "Label23";
            this.Label23.Size = new System.Drawing.Size(184, 17);
            this.Label23.TabIndex = 93;
            this.Label23.Text = "-Nightmare Engine Settings-";
            // 
            // IncreDecrenumbnightmare
            // 
            this.IncreDecrenumbnightmare.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(81)))));
            this.IncreDecrenumbnightmare.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IncreDecrenumbnightmare.ForeColor = System.Drawing.Color.White;
            this.IncreDecrenumbnightmare.Location = new System.Drawing.Point(212, 56);
            this.IncreDecrenumbnightmare.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.IncreDecrenumbnightmare.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.IncreDecrenumbnightmare.Name = "IncreDecrenumbnightmare";
            this.IncreDecrenumbnightmare.Size = new System.Drawing.Size(191, 22);
            this.IncreDecrenumbnightmare.TabIndex = 91;
            this.IncreDecrenumbnightmare.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // comboBox1
            // 
            this.comboBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(64)))));
            this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox1.ForeColor = System.Drawing.Color.White;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "8-bit",
            "16-bit",
            "32-bit",
            "64-bit"});
            this.comboBox1.Location = new System.Drawing.Point(212, 85);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(191, 21);
            this.comboBox1.TabIndex = 90;
            this.comboBox1.Text = "8-bit";
            this.comboBox1.Visible = false;
            // 
            // NightmareComboBox
            // 
            this.NightmareComboBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(64)))));
            this.NightmareComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NightmareComboBox.ForeColor = System.Drawing.Color.White;
            this.NightmareComboBox.FormattingEnabled = true;
            this.NightmareComboBox.Items.AddRange(new object[] {
            "RANDOM",
            "RANDOMTILT",
            "TILT"});
            this.NightmareComboBox.Location = new System.Drawing.Point(212, 29);
            this.NightmareComboBox.Name = "NightmareComboBox";
            this.NightmareComboBox.Size = new System.Drawing.Size(191, 21);
            this.NightmareComboBox.TabIndex = 90;
            this.NightmareComboBox.Text = "RANDOM";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(125, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 17);
            this.label2.TabIndex = 20;
            this.label2.Text = "Bit Precision:";
            this.label2.Visible = false;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Label1.ForeColor = System.Drawing.Color.White;
            this.Label1.Location = new System.Drawing.Point(24, 58);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(182, 17);
            this.Label1.TabIndex = 20;
            this.Label1.Text = "Increment/Decrement number";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(101, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 17);
            this.label5.TabIndex = 20;
            this.label5.Text = "Corruption Type:";
            // 
            // MergeOpenFilebtn
            // 
            this.MergeOpenFilebtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(80)))));
            this.MergeOpenFilebtn.FlatAppearance.BorderSize = 0;
            this.MergeOpenFilebtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MergeOpenFilebtn.ForeColor = System.Drawing.Color.Aqua;
            this.MergeOpenFilebtn.Image = global::LunarROMCorruptor.Properties.Resources.upload;
            this.MergeOpenFilebtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MergeOpenFilebtn.Location = new System.Drawing.Point(5, 41);
            this.MergeOpenFilebtn.Name = "MergeOpenFilebtn";
            this.MergeOpenFilebtn.Size = new System.Drawing.Size(116, 23);
            this.MergeOpenFilebtn.TabIndex = 96;
            this.MergeOpenFilebtn.Text = "Open File";
            this.MergeOpenFilebtn.UseVisualStyleBackColor = false;
            this.MergeOpenFilebtn.Click += new System.EventHandler(this.Button1_Click);
            // 
            // MergeEnginePanel
            // 
            this.MergeEnginePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MergeEnginePanel.BackColor = System.Drawing.Color.Teal;
            this.MergeEnginePanel.Controls.Add(this.CorrTypeMerge);
            this.MergeEnginePanel.Controls.Add(this.Mod256MergeEnginechkbox);
            this.MergeEnginePanel.Controls.Add(this.ReplaceByteWithSamePos);
            this.MergeEnginePanel.Controls.Add(this.Label15);
            this.MergeEnginePanel.Controls.Add(this.Label13);
            this.MergeEnginePanel.Controls.Add(this.MergeOpenFilebtn);
            this.MergeEnginePanel.Controls.Add(this.MergeFileLocationTxt);
            this.MergeEnginePanel.Controls.Add(this.Label9);
            this.MergeEnginePanel.Location = new System.Drawing.Point(13, 14);
            this.MergeEnginePanel.Name = "MergeEnginePanel";
            this.MergeEnginePanel.Size = new System.Drawing.Size(618, 240);
            this.MergeEnginePanel.TabIndex = 158;
            this.MergeEnginePanel.Tag = "color:normal";
            // 
            // MergeFileLocationTxt
            // 
            this.MergeFileLocationTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MergeFileLocationTxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(81)))));
            this.MergeFileLocationTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MergeFileLocationTxt.ForeColor = System.Drawing.Color.White;
            this.MergeFileLocationTxt.Location = new System.Drawing.Point(127, 42);
            this.MergeFileLocationTxt.Name = "MergeFileLocationTxt";
            this.MergeFileLocationTxt.ReadOnly = true;
            this.MergeFileLocationTxt.Size = new System.Drawing.Size(475, 22);
            this.MergeFileLocationTxt.TabIndex = 94;
            // 
            // Label9
            // 
            this.Label9.AutoSize = true;
            this.Label9.Font = new System.Drawing.Font("Segoe UI", 9.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))));
            this.Label9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Label9.Location = new System.Drawing.Point(3, 2);
            this.Label9.Name = "Label9";
            this.Label9.Size = new System.Drawing.Size(157, 17);
            this.Label9.TabIndex = 93;
            this.Label9.Text = "-Merge Engine Settings-";
            // 
            // CorruptionEngineOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(27)))), ((int)(((byte)(55)))));
            this.ClientSize = new System.Drawing.Size(642, 268);
            this.Controls.Add(this.Vector2EnginePanel);
            this.Controls.Add(this.NightmareEnginePanel);
            this.Controls.Add(this.MergeEnginePanel);
            this.Controls.Add(this.LogicEnginePanel);
            this.Controls.Add(this.LerpEnginePanel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "CorruptionEngineOptions";
            this.Text = "CorruptionEngineOptions";
            this.LogicEnginePanel.ResumeLayout(false);
            this.LogicEnginePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ValueBitwise)).EndInit();
            this.LerpEnginePanel.ResumeLayout(false);
            this.LerpEnginePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LerpSplitValueTrackBar)).EndInit();
            this.Vector2EnginePanel.ResumeLayout(false);
            this.Vector2EnginePanel.PerformLayout();
            this.NightmareEnginePanel.ResumeLayout(false);
            this.NightmareEnginePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IncreDecrenumbnightmare)).EndInit();
            this.MergeEnginePanel.ResumeLayout(false);
            this.MergeEnginePanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.OpenFileDialog OpenFileDialog1;
        public System.Windows.Forms.ComboBox CorrTypeMerge;
        public System.Windows.Forms.CheckBox Mod256MergeEnginechkbox;
        public System.Windows.Forms.CheckBox ReplaceByteWithSamePos;
        public System.Windows.Forms.Panel LogicEnginePanel;
        internal System.Windows.Forms.ComboBox BitwiseComboBox;
        internal System.Windows.Forms.NumericUpDown ValueBitwise;
        private System.Windows.Forms.Label Label14;
        private System.Windows.Forms.Label Label12;
        private System.Windows.Forms.Label Label10;
        private System.Windows.Forms.Label Label11;
        internal System.Windows.Forms.Label Label15;
        public System.Windows.Forms.Panel LerpEnginePanel;
        internal System.Windows.Forms.TextBox LerpValueTxt;
        private System.Windows.Forms.Label Label17;
        private System.Windows.Forms.Label Label20;
        public System.Windows.Forms.Panel Vector2EnginePanel;
        internal System.Windows.Forms.RadioButton ChunkSearchVector2rb;
        internal System.Windows.Forms.RadioButton RandomSearchVector2rb;
        private System.Windows.Forms.Label Label19;
        private System.Windows.Forms.Label Label22;
        private System.Windows.Forms.Label Label21;
        internal System.Windows.Forms.Label Label13;
        public System.Windows.Forms.Panel NightmareEnginePanel;
        private System.Windows.Forms.Label Label23;
        internal System.Windows.Forms.NumericUpDown IncreDecrenumbnightmare;
        internal System.Windows.Forms.ComboBox NightmareComboBox;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Label label5;
        internal System.Windows.Forms.Button MergeOpenFilebtn;
        public System.Windows.Forms.Panel MergeEnginePanel;
        public System.Windows.Forms.TextBox MergeFileLocationTxt;
        private System.Windows.Forms.Label Label9;
        internal System.Windows.Forms.CheckBox LogicRandomizeTypeCheckbox;
        internal System.Windows.Forms.CheckBox LogicRandomizeValueCheckBox;
        internal System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.RichTextBox FloatBitOutput;
        private System.Windows.Forms.TrackBar LerpSplitValueTrackBar;
    }
}