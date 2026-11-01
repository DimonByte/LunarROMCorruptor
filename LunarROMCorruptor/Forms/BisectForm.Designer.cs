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
            SuspendLayout();
            // 
            // BisectStatusLbl
            // 
            BisectStatusLbl.ForeColor = Color.White;
            BisectStatusLbl.Location = new Point(12, 10);
            BisectStatusLbl.Size = new Size(356, 96);
            BisectStatusLbl.Text = "Corrupt the file as usual. When it crashes, press Start: LRC leaves out half of the stash each round and asks whether it still crashes.";
            BisectStatusLbl.Name = "BisectStatusLbl";
            BisectStatusLbl.TabIndex = 0;
            // 
            // BisectStartBtn
            // 
            BisectStartBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectStartBtn.FlatStyle = FlatStyle.Flat;
            BisectStartBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectStartBtn.Location = new Point(12, 110);
            BisectStartBtn.Size = new Size(356, 30);
            BisectStartBtn.Text = "Start bisect from the current stash";
            BisectStartBtn.UseVisualStyleBackColor = false;
            BisectStartBtn.Name = "BisectStartBtn";
            BisectStartBtn.TabIndex = 1;
            BisectStartBtn.Click += BisectStartBtn_Click;
            // 
            // BisectCrashBtn
            // 
            BisectCrashBtn.BackColor = Color.FromArgb(120, 40, 50);
            BisectCrashBtn.FlatStyle = FlatStyle.Flat;
            BisectCrashBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectCrashBtn.Location = new Point(12, 146);
            BisectCrashBtn.Size = new Size(174, 34);
            BisectCrashBtn.Text = "💥 Still crashes";
            BisectCrashBtn.UseVisualStyleBackColor = false;
            BisectCrashBtn.Name = "BisectCrashBtn";
            BisectCrashBtn.TabIndex = 2;
            BisectCrashBtn.Click += BisectCrashBtn_Click;
            // 
            // BisectWorksBtn
            // 
            BisectWorksBtn.BackColor = Color.FromArgb(40, 100, 60);
            BisectWorksBtn.FlatStyle = FlatStyle.Flat;
            BisectWorksBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectWorksBtn.Location = new Point(194, 146);
            BisectWorksBtn.Size = new Size(174, 34);
            BisectWorksBtn.Text = "✔ Works now";
            BisectWorksBtn.UseVisualStyleBackColor = false;
            BisectWorksBtn.Name = "BisectWorksBtn";
            BisectWorksBtn.TabIndex = 3;
            BisectWorksBtn.Click += BisectWorksBtn_Click;
            // 
            // BisectRetestBtn
            // 
            BisectRetestBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectRetestBtn.FlatStyle = FlatStyle.Flat;
            BisectRetestBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectRetestBtn.Location = new Point(12, 186);
            BisectRetestBtn.Size = new Size(174, 27);
            BisectRetestBtn.Text = "Re-run this test";
            BisectRetestBtn.UseVisualStyleBackColor = false;
            BisectRetestBtn.Name = "BisectRetestBtn";
            BisectRetestBtn.TabIndex = 4;
            BisectRetestBtn.Click += BisectRetestBtn_Click;
            // 
            // BisectUndoBtn
            // 
            BisectUndoBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectUndoBtn.FlatStyle = FlatStyle.Flat;
            BisectUndoBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectUndoBtn.Location = new Point(194, 186);
            BisectUndoBtn.Size = new Size(174, 27);
            BisectUndoBtn.Text = "Undo last answer";
            BisectUndoBtn.UseVisualStyleBackColor = false;
            BisectUndoBtn.Name = "BisectUndoBtn";
            BisectUndoBtn.TabIndex = 5;
            BisectUndoBtn.Click += BisectUndoBtn_Click;
            // 
            // BisectWordAlignChkBox
            // 
            BisectWordAlignChkBox.AutoSize = true;
            BisectWordAlignChkBox.Checked = true;
            BisectWordAlignChkBox.CheckState = CheckState.Checked;
            BisectWordAlignChkBox.ForeColor = Color.FromArgb(213, 216, 216);
            BisectWordAlignChkBox.Location = new Point(12, 220);
            BisectWordAlignChkBox.Text = "Exclude whole 4-byte words (instructions, pointers, floats)";
            BisectWordAlignChkBox.UseVisualStyleBackColor = true;
            BisectWordAlignChkBox.Name = "BisectWordAlignChkBox";
            BisectWordAlignChkBox.TabIndex = 6;
            // 
            // BisectExcludeBtn
            // 
            BisectExcludeBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectExcludeBtn.FlatStyle = FlatStyle.Flat;
            BisectExcludeBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectExcludeBtn.Location = new Point(12, 244);
            BisectExcludeBtn.Size = new Size(174, 30);
            BisectExcludeBtn.Text = "Exclude suspects";
            BisectExcludeBtn.UseVisualStyleBackColor = false;
            BisectExcludeBtn.Name = "BisectExcludeBtn";
            BisectExcludeBtn.TabIndex = 7;
            BisectExcludeBtn.Click += BisectExcludeBtn_Click;
            // 
            // BisectApplyWithoutBtn
            // 
            BisectApplyWithoutBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectApplyWithoutBtn.FlatStyle = FlatStyle.Flat;
            BisectApplyWithoutBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectApplyWithoutBtn.Location = new Point(194, 244);
            BisectApplyWithoutBtn.Size = new Size(174, 30);
            BisectApplyWithoutBtn.Text = "Exclude + apply the rest";
            BisectApplyWithoutBtn.UseVisualStyleBackColor = false;
            BisectApplyWithoutBtn.Name = "BisectApplyWithoutBtn";
            BisectApplyWithoutBtn.TabIndex = 8;
            BisectApplyWithoutBtn.Click += BisectApplyWithoutBtn_Click;
            // 
            // BisectExclusionsLbl
            // 
            BisectExclusionsLbl.ForeColor = Color.Silver;
            BisectExclusionsLbl.Location = new Point(12, 284);
            BisectExclusionsLbl.Size = new Size(240, 34);
            BisectExclusionsLbl.Text = "No excluded ranges.";
            BisectExclusionsLbl.Name = "BisectExclusionsLbl";
            BisectExclusionsLbl.TabIndex = 9;
            // 
            // BisectResetExclusionsBtn
            // 
            BisectResetExclusionsBtn.BackColor = Color.FromArgb(52, 60, 111);
            BisectResetExclusionsBtn.FlatStyle = FlatStyle.Flat;
            BisectResetExclusionsBtn.ForeColor = Color.FromArgb(213, 216, 216);
            BisectResetExclusionsBtn.Location = new Point(258, 286);
            BisectResetExclusionsBtn.Size = new Size(110, 27);
            BisectResetExclusionsBtn.Text = "Reset exclusions";
            BisectResetExclusionsBtn.UseVisualStyleBackColor = false;
            BisectResetExclusionsBtn.Name = "BisectResetExclusionsBtn";
            BisectResetExclusionsBtn.TabIndex = 10;
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
    }
}
