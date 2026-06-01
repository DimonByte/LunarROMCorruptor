namespace LunarROMCorruptor
{
    partial class CorruptionQueueForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CorruptionQueueForm));
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RunSelectedFileInEmuBTN = new System.Windows.Forms.Button();
            this.SelectedRunFileTXT = new System.Windows.Forms.TextBox();
            this.ClearSelectedRunFileBTN = new System.Windows.Forms.Button();
            this.SelectedEmulatorFilelbl = new System.Windows.Forms.Label();
            this.AddFolderBTN = new System.Windows.Forms.Button();
            this.SendFilestoCorruptorBTN = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.Openfilebtn = new System.Windows.Forms.Button();
            this.ClearBtn = new System.Windows.Forms.Button();
            this.removeselbtn = new System.Windows.Forms.Button();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.CorruptionQueueList = new System.Windows.Forms.ListBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.DragandDropICON = new System.Windows.Forms.PictureBox();
            this.Panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DragandDropICON)).BeginInit();
            this.SuspendLayout();
            // 
            // SaveFileDialog
            // 
            this.SaveFileDialog.Filter = "All Files *.*|*.*";
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(43)))), ((int)(((byte)(83)))));
            this.Panel1.Controls.Add(this.groupBox1);
            this.Panel1.Controls.Add(this.AddFolderBTN);
            this.Panel1.Controls.Add(this.SendFilestoCorruptorBTN);
            this.Panel1.Controls.Add(this.label1);
            this.Panel1.Controls.Add(this.Openfilebtn);
            this.Panel1.Controls.Add(this.ClearBtn);
            this.Panel1.Controls.Add(this.removeselbtn);
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.Panel1.Location = new System.Drawing.Point(0, 0);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(194, 421);
            this.Panel1.TabIndex = 162;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RunSelectedFileInEmuBTN);
            this.groupBox1.Controls.Add(this.SelectedRunFileTXT);
            this.groupBox1.Controls.Add(this.ClearSelectedRunFileBTN);
            this.groupBox1.Controls.Add(this.SelectedEmulatorFilelbl);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.LimeGreen;
            this.groupBox1.Location = new System.Drawing.Point(12, 128);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(170, 144);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Run file after corruption";
            // 
            // RunSelectedFileInEmuBTN
            // 
            this.RunSelectedFileInEmuBTN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(60)))), ((int)(((byte)(111)))));
            this.RunSelectedFileInEmuBTN.FlatAppearance.BorderSize = 0;
            this.RunSelectedFileInEmuBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RunSelectedFileInEmuBTN.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RunSelectedFileInEmuBTN.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(216)))), ((int)(((byte)(216)))));
            this.RunSelectedFileInEmuBTN.Location = new System.Drawing.Point(6, 21);
            this.RunSelectedFileInEmuBTN.Name = "RunSelectedFileInEmuBTN";
            this.RunSelectedFileInEmuBTN.Size = new System.Drawing.Size(158, 39);
            this.RunSelectedFileInEmuBTN.TabIndex = 2;
            this.RunSelectedFileInEmuBTN.Text = "Run selected file in emulator after corruption";
            this.RunSelectedFileInEmuBTN.UseVisualStyleBackColor = false;
            this.RunSelectedFileInEmuBTN.Click += new System.EventHandler(this.RunSelectedFileInEmuBTN_Click);
            // 
            // SelectedRunFileTXT
            // 
            this.SelectedRunFileTXT.Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectedRunFileTXT.Location = new System.Drawing.Point(6, 114);
            this.SelectedRunFileTXT.Name = "SelectedRunFileTXT";
            this.SelectedRunFileTXT.ReadOnly = true;
            this.SelectedRunFileTXT.Size = new System.Drawing.Size(158, 20);
            this.SelectedRunFileTXT.TabIndex = 21;
            // 
            // ClearSelectedRunFileBTN
            // 
            this.ClearSelectedRunFileBTN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(60)))), ((int)(((byte)(111)))));
            this.ClearSelectedRunFileBTN.FlatAppearance.BorderSize = 0;
            this.ClearSelectedRunFileBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ClearSelectedRunFileBTN.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClearSelectedRunFileBTN.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(216)))), ((int)(((byte)(216)))));
            this.ClearSelectedRunFileBTN.Location = new System.Drawing.Point(6, 66);
            this.ClearSelectedRunFileBTN.Name = "ClearSelectedRunFileBTN";
            this.ClearSelectedRunFileBTN.Size = new System.Drawing.Size(158, 25);
            this.ClearSelectedRunFileBTN.TabIndex = 2;
            this.ClearSelectedRunFileBTN.Text = "Clear selected run file";
            this.ClearSelectedRunFileBTN.UseVisualStyleBackColor = false;
            this.ClearSelectedRunFileBTN.Click += new System.EventHandler(this.ClearSelectedRunFileBTN_Click);
            // 
            // SelectedEmulatorFilelbl
            // 
            this.SelectedEmulatorFilelbl.AutoSize = true;
            this.SelectedEmulatorFilelbl.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.SelectedEmulatorFilelbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.SelectedEmulatorFilelbl.Location = new System.Drawing.Point(5, 94);
            this.SelectedEmulatorFilelbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.SelectedEmulatorFilelbl.Name = "SelectedEmulatorFilelbl";
            this.SelectedEmulatorFilelbl.Size = new System.Drawing.Size(104, 17);
            this.SelectedEmulatorFilelbl.TabIndex = 20;
            this.SelectedEmulatorFilelbl.Text = "Selected run file:";
            // 
            // AddFolderBTN
            // 
            this.AddFolderBTN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(60)))), ((int)(((byte)(111)))));
            this.AddFolderBTN.FlatAppearance.BorderSize = 0;
            this.AddFolderBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AddFolderBTN.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(216)))), ((int)(((byte)(216)))));
            this.AddFolderBTN.Image = ((System.Drawing.Image)(resources.GetObject("AddFolderBTN.Image")));
            this.AddFolderBTN.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.AddFolderBTN.Location = new System.Drawing.Point(12, 41);
            this.AddFolderBTN.Name = "AddFolderBTN";
            this.AddFolderBTN.Size = new System.Drawing.Size(170, 23);
            this.AddFolderBTN.TabIndex = 2;
            this.AddFolderBTN.Text = "Add Folder";
            this.AddFolderBTN.UseVisualStyleBackColor = false;
            this.AddFolderBTN.Click += new System.EventHandler(this.AddFolderBTN_Click);
            // 
            // SendFilestoCorruptorBTN
            // 
            this.SendFilestoCorruptorBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SendFilestoCorruptorBTN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(60)))), ((int)(((byte)(111)))));
            this.SendFilestoCorruptorBTN.FlatAppearance.BorderSize = 0;
            this.SendFilestoCorruptorBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SendFilestoCorruptorBTN.Font = new System.Drawing.Font("Segoe UI", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SendFilestoCorruptorBTN.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.SendFilestoCorruptorBTN.Image = global::LunarROMCorruptor.Properties.Resources.arrow_up_circle_solid;
            this.SendFilestoCorruptorBTN.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SendFilestoCorruptorBTN.Location = new System.Drawing.Point(12, 386);
            this.SendFilestoCorruptorBTN.Name = "SendFilestoCorruptorBTN";
            this.SendFilestoCorruptorBTN.Size = new System.Drawing.Size(170, 23);
            this.SendFilestoCorruptorBTN.TabIndex = 2;
            this.SendFilestoCorruptorBTN.Text = "Send files to corruptor";
            this.SendFilestoCorruptorBTN.UseVisualStyleBackColor = false;
            this.SendFilestoCorruptorBTN.Click += new System.EventHandler(this.SendFilestoCorruptorBTN_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.label1.Location = new System.Drawing.Point(11, 275);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(173, 104);
            this.label1.TabIndex = 20;
            this.label1.Text = "Be aware:\r\nThe restore function is disabled,\r\nmeaning the corrupted file\r\nwon\'t b" +
    "e restored before\r\nanother corruption.\r\n\r\nReplace each file after each\r\ncorrupti" +
    "on blast.";
            // 
            // Openfilebtn
            // 
            this.Openfilebtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(60)))), ((int)(((byte)(111)))));
            this.Openfilebtn.FlatAppearance.BorderSize = 0;
            this.Openfilebtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Openfilebtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(216)))), ((int)(((byte)(216)))));
            this.Openfilebtn.Image = ((System.Drawing.Image)(resources.GetObject("Openfilebtn.Image")));
            this.Openfilebtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Openfilebtn.Location = new System.Drawing.Point(12, 12);
            this.Openfilebtn.Name = "Openfilebtn";
            this.Openfilebtn.Size = new System.Drawing.Size(170, 23);
            this.Openfilebtn.TabIndex = 2;
            this.Openfilebtn.Text = "Add File(s)";
            this.Openfilebtn.UseVisualStyleBackColor = false;
            this.Openfilebtn.Click += new System.EventHandler(this.Openfilebtn_Click);
            // 
            // ClearBtn
            // 
            this.ClearBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(60)))), ((int)(((byte)(111)))));
            this.ClearBtn.FlatAppearance.BorderSize = 0;
            this.ClearBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ClearBtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(216)))), ((int)(((byte)(216)))));
            this.ClearBtn.Location = new System.Drawing.Point(12, 99);
            this.ClearBtn.Name = "ClearBtn";
            this.ClearBtn.Size = new System.Drawing.Size(170, 23);
            this.ClearBtn.TabIndex = 2;
            this.ClearBtn.Text = "Clear List";
            this.ClearBtn.UseVisualStyleBackColor = false;
            this.ClearBtn.Click += new System.EventHandler(this.ClearBtn_Click);
            // 
            // removeselbtn
            // 
            this.removeselbtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(60)))), ((int)(((byte)(111)))));
            this.removeselbtn.FlatAppearance.BorderSize = 0;
            this.removeselbtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.removeselbtn.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(213)))), ((int)(((byte)(216)))), ((int)(((byte)(216)))));
            this.removeselbtn.Location = new System.Drawing.Point(12, 70);
            this.removeselbtn.Name = "removeselbtn";
            this.removeselbtn.Size = new System.Drawing.Size(170, 23);
            this.removeselbtn.TabIndex = 2;
            this.removeselbtn.Text = "Remove Selected File(s)";
            this.removeselbtn.UseVisualStyleBackColor = false;
            this.removeselbtn.Click += new System.EventHandler(this.removeselbtn_Click);
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.Filter = "All files (*.*)|*.*";
            this.OpenFileDialog.Multiselect = true;
            // 
            // CorruptionQueueList
            // 
            this.CorruptionQueueList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CorruptionQueueList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(43)))), ((int)(((byte)(83)))));
            this.CorruptionQueueList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.CorruptionQueueList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.CorruptionQueueList.FormattingEnabled = true;
            this.CorruptionQueueList.HorizontalScrollbar = true;
            this.CorruptionQueueList.IntegralHeight = false;
            this.CorruptionQueueList.Location = new System.Drawing.Point(203, 12);
            this.CorruptionQueueList.Name = "CorruptionQueueList";
            this.CorruptionQueueList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.CorruptionQueueList.Size = new System.Drawing.Size(410, 397);
            this.CorruptionQueueList.TabIndex = 163;
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Select a folder to corrupt the files of.";
            // 
            // DragandDropICON
            // 
            this.DragandDropICON.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(27)))), ((int)(((byte)(55)))));
            this.DragandDropICON.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DragandDropICON.Image = global::LunarROMCorruptor.Properties.Resources.dragicon;
            this.DragandDropICON.Location = new System.Drawing.Point(0, 0);
            this.DragandDropICON.Name = "DragandDropICON";
            this.DragandDropICON.Size = new System.Drawing.Size(622, 421);
            this.DragandDropICON.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.DragandDropICON.TabIndex = 165;
            this.DragandDropICON.TabStop = false;
            this.DragandDropICON.Visible = false;
            // 
            // CorruptionQueueForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(45)))));
            this.ClientSize = new System.Drawing.Size(622, 421);
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.CorruptionQueueList);
            this.Controls.Add(this.DragandDropICON);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(638, 460);
            this.Name = "CorruptionQueueForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Corruption Queue - Edit";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.CorruptionQueueForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.CorruptionQueueForm_DragEnter);
            this.DragLeave += new System.EventHandler(this.CorruptionQueueForm_DragLeave);
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DragandDropICON)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.SaveFileDialog SaveFileDialog;
        internal System.Windows.Forms.Panel Panel1;
        internal System.Windows.Forms.Button AddFolderBTN;
        internal System.Windows.Forms.Button SendFilestoCorruptorBTN;
        internal System.Windows.Forms.Button Openfilebtn;
        internal System.Windows.Forms.Button ClearBtn;
        internal System.Windows.Forms.Button removeselbtn;
        internal System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        public System.Windows.Forms.ListBox CorruptionQueueList;
        private System.Windows.Forms.PictureBox DragandDropICON;
        internal System.Windows.Forms.Button RunSelectedFileInEmuBTN;
        internal System.Windows.Forms.Button ClearSelectedRunFileBTN;
        private System.Windows.Forms.Label SelectedEmulatorFilelbl;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.TextBox SelectedRunFileTXT;
        private System.Windows.Forms.Label label1;
    }
}