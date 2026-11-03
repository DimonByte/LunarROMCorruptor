namespace LunarROMCorruptor.Forms
{
    partial class BisectForm
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
            components = new System.ComponentModel.Container();
            BisectStatusLbl = new Label();
            BisectStartBtn = new Button();
            BisectCrashBtn = new Button();
            BisectWorksBtn = new Button();
            BisectRetestBtn = new Button();
            BisectUndoBtn = new Button();
            BisectWordAlignChkBox = new CheckBox();
            BisectExcludeBtn = new Button();
            BisectApplyWithoutBtn = new Button();
            BisectExclusionsLbl = new Label();
            BisectResetExclusionsBtn = new Button();
            BisectToolTip = new ToolTip(components);
            SuspendLayout();
            // 
            // BisectStatusLbl
            // 
            BisectStatusLbl.ForeColor = Color.White;
            BisectStatusLbl.Location = new Point(12, 10);
            BisectStatusLbl.Name = "BisectStatusLbl";
            BisectStatusLbl.Size = new Size(356, 96);
            BisectStatusLbl.TabIndex = 0;
            BisectStatusLbl.Text = "Corrupt the file as usual. When it crashes, press Start: LRC leaves out half of the stash each round and asks whether it still crashes.";
            // 
            // BisectStartBtn
            // 
            BisectStartBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectStartBtn.FlatStyle = FlatStyle.Flat;
            BisectStartBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectStartBtn.Location = new Point(12, 110);
            BisectStartBtn.Name = "BisectStartBtn";
            BisectStartBtn.Size = new Size(356, 30);
            BisectStartBtn.TabIndex = 1;
            BisectStartBtn.Text = "Start bisect from the current stash";
            BisectStartBtn.UseVisualStyleBackColor = false;
            BisectStartBtn.Click += BisectStartBtn_Click;
            // 
            // BisectCrashBtn
            // 
            BisectCrashBtn.BackColor = Color.FromArgb(120, 40, 50);
            BisectCrashBtn.FlatStyle = FlatStyle.Flat;
            BisectCrashBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectCrashBtn.Location = new Point(12, 146);
            BisectCrashBtn.Name = "BisectCrashBtn";
            BisectCrashBtn.Size = new Size(174, 34);
            BisectCrashBtn.TabIndex = 2;
            BisectCrashBtn.Text = "💥 Still crashes";
            BisectCrashBtn.UseVisualStyleBackColor = false;
            BisectCrashBtn.Click += BisectCrashBtn_Click;
            // 
            // BisectWorksBtn
            // 
            BisectWorksBtn.BackColor = Color.FromArgb(40, 100, 60);
            BisectWorksBtn.FlatStyle = FlatStyle.Flat;
            BisectWorksBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectWorksBtn.Location = new Point(194, 146);
            BisectWorksBtn.Name = "BisectWorksBtn";
            BisectWorksBtn.Size = new Size(174, 34);
            BisectWorksBtn.TabIndex = 3;
            BisectWorksBtn.Text = "✔ Works now";
            BisectWorksBtn.UseVisualStyleBackColor = false;
            BisectWorksBtn.Click += BisectWorksBtn_Click;
            // 
            // BisectRetestBtn
            // 
            BisectRetestBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectRetestBtn.FlatStyle = FlatStyle.Flat;
            BisectRetestBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectRetestBtn.Location = new Point(12, 186);
            BisectRetestBtn.Name = "BisectRetestBtn";
            BisectRetestBtn.Size = new Size(174, 27);
            BisectRetestBtn.TabIndex = 4;
            BisectRetestBtn.Text = "Re-run this test";
            BisectRetestBtn.UseVisualStyleBackColor = false;
            BisectRetestBtn.Click += BisectRetestBtn_Click;
            // 
            // BisectUndoBtn
            // 
            BisectUndoBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectUndoBtn.FlatStyle = FlatStyle.Flat;
            BisectUndoBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectUndoBtn.Location = new Point(194, 186);
            BisectUndoBtn.Name = "BisectUndoBtn";
            BisectUndoBtn.Size = new Size(174, 27);
            BisectUndoBtn.TabIndex = 5;
            BisectUndoBtn.Text = "Undo last answer";
            BisectUndoBtn.UseVisualStyleBackColor = false;
            BisectUndoBtn.Click += BisectUndoBtn_Click;
            // 
            // BisectWordAlignChkBox
            // 
            BisectWordAlignChkBox.AutoSize = true;
            BisectWordAlignChkBox.Checked = true;
            BisectWordAlignChkBox.CheckState = CheckState.Checked;
            BisectWordAlignChkBox.ForeColor = Color.FromArgb(213, 216, 216);
            BisectWordAlignChkBox.Location = new Point(12, 220);
            BisectWordAlignChkBox.Name = "BisectWordAlignChkBox";
            BisectWordAlignChkBox.Size = new Size(330, 19);
            BisectWordAlignChkBox.TabIndex = 6;
            BisectWordAlignChkBox.Text = "Exclude whole 4-byte words (instructions, pointers, floats)";
            BisectWordAlignChkBox.UseVisualStyleBackColor = true;
            // 
            // BisectExcludeBtn
            // 
            BisectExcludeBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectExcludeBtn.FlatStyle = FlatStyle.Flat;
            BisectExcludeBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectExcludeBtn.Location = new Point(12, 244);
            BisectExcludeBtn.Name = "BisectExcludeBtn";
            BisectExcludeBtn.Size = new Size(174, 30);
            BisectExcludeBtn.TabIndex = 7;
            BisectExcludeBtn.Text = "Exclude suspects only";
            BisectToolTip.SetToolTip(BisectExcludeBtn, "Adds the suspect changes to the excluded ranges (red in ByteView), so no engine changes those bytes again.\nNothing is re-applied and the bisect keeps going, so you can exclude them early and carry on.");
            BisectExcludeBtn.UseVisualStyleBackColor = false;
            BisectExcludeBtn.Click += BisectExcludeBtn_Click;
            // 
            // BisectApplyWithoutBtn
            // 
            BisectApplyWithoutBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectApplyWithoutBtn.FlatStyle = FlatStyle.Flat;
            BisectApplyWithoutBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectApplyWithoutBtn.Location = new Point(194, 244);
            BisectApplyWithoutBtn.Name = "BisectApplyWithoutBtn";
            BisectApplyWithoutBtn.Size = new Size(174, 30);
            BisectApplyWithoutBtn.TabIndex = 8;
            BisectApplyWithoutBtn.Text = "Apply stash without suspects";
            BisectToolTip.SetToolTip(BisectApplyWithoutBtn, "Excludes the suspects, then applies the whole original stash except those changes to the save location (and starts the emulator if that's on).\nThis ends the bisect: you get your corruption back without the crash. If it still crashes, press Start again to find the next culprit.");
            BisectApplyWithoutBtn.UseVisualStyleBackColor = false;
            BisectApplyWithoutBtn.Click += BisectApplyWithoutBtn_Click;
            // 
            // BisectExclusionsLbl
            // 
            BisectExclusionsLbl.ForeColor = Color.Silver;
            BisectExclusionsLbl.Location = new Point(12, 284);
            BisectExclusionsLbl.Name = "BisectExclusionsLbl";
            BisectExclusionsLbl.Size = new Size(240, 34);
            BisectExclusionsLbl.TabIndex = 9;
            BisectExclusionsLbl.Text = "No excluded ranges.";
            // 
            // BisectResetExclusionsBtn
            // 
            BisectResetExclusionsBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectResetExclusionsBtn.FlatStyle = FlatStyle.Flat;
            BisectResetExclusionsBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectResetExclusionsBtn.Location = new Point(258, 286);
            BisectResetExclusionsBtn.Name = "BisectResetExclusionsBtn";
            BisectResetExclusionsBtn.Size = new Size(110, 27);
            BisectResetExclusionsBtn.TabIndex = 10;
            BisectResetExclusionsBtn.Text = "Reset exclusions";
            BisectResetExclusionsBtn.UseVisualStyleBackColor = false;
            BisectResetExclusionsBtn.Click += BisectResetExclusionsBtn_Click;
            // 
            // BisectForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(28, 27, 55);
            ClientSize = new Size(380, 326);
            Controls.Add(BisectStatusLbl);
            Controls.Add(BisectStartBtn);
            Controls.Add(BisectCrashBtn);
            Controls.Add(BisectWorksBtn);
            Controls.Add(BisectRetestBtn);
            Controls.Add(BisectUndoBtn);
            Controls.Add(BisectWordAlignChkBox);
            Controls.Add(BisectExcludeBtn);
            Controls.Add(BisectApplyWithoutBtn);
            Controls.Add(BisectExclusionsLbl);
            Controls.Add(BisectResetExclusionsBtn);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BisectForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "LRC - Bisect Mode";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label BisectStatusLbl;
        private Button BisectStartBtn;
        private Button BisectCrashBtn;
        private Button BisectWorksBtn;
        private Button BisectRetestBtn;
        private Button BisectUndoBtn;
        private CheckBox BisectWordAlignChkBox;
        private Button BisectExcludeBtn;
        private Button BisectApplyWithoutBtn;
        private Label BisectExclusionsLbl;
        private Button BisectResetExclusionsBtn;
        private ToolTip BisectToolTip;
    }
}
