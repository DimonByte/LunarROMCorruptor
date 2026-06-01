namespace LunarROMCorruptor
{
    partial class SelectProcess
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectProcess));
            this.Panel1 = new System.Windows.Forms.Panel();
            this.Label1 = new System.Windows.Forms.Label();
            this.ProcessList = new System.Windows.Forms.ListBox();
            this.RefreshBtn = new System.Windows.Forms.Button();
            this.CorruptBTN = new System.Windows.Forms.Button();
            this.Panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel1
            // 
            this.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(27)))), ((int)(((byte)(55)))));
            this.Panel1.Controls.Add(this.Label1);
            this.Panel1.Controls.Add(this.RefreshBtn);
            this.Panel1.Controls.Add(this.CorruptBTN);
            this.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel1.Location = new System.Drawing.Point(0, 334);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(420, 55);
            this.Panel1.TabIndex = 162;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.Label1.Location = new System.Drawing.Point(79, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(266, 17);
            this.Label1.TabIndex = 96;
            this.Label1.Text = "Please select a process from the list above...";
            // 
            // ProcessList
            // 
            this.ProcessList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(43)))), ((int)(((byte)(83)))));
            this.ProcessList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ProcessList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProcessList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ProcessList.FormattingEnabled = true;
            this.ProcessList.HorizontalScrollbar = true;
            this.ProcessList.IntegralHeight = false;
            this.ProcessList.Location = new System.Drawing.Point(0, 0);
            this.ProcessList.Name = "ProcessList";
            this.ProcessList.Size = new System.Drawing.Size(420, 334);
            this.ProcessList.TabIndex = 163;
            // 
            // RefreshBtn
            // 
            this.RefreshBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(60)))), ((int)(((byte)(111)))));
            this.RefreshBtn.FlatAppearance.BorderSize = 0;
            this.RefreshBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RefreshBtn.ForeColor = System.Drawing.Color.Aqua;
            this.RefreshBtn.Image = global::LunarROMCorruptor.Properties.Resources.restart;
            this.RefreshBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.RefreshBtn.Location = new System.Drawing.Point(3, 17);
            this.RefreshBtn.Name = "RefreshBtn";
            this.RefreshBtn.Size = new System.Drawing.Size(136, 35);
            this.RefreshBtn.TabIndex = 2;
            this.RefreshBtn.Text = "Refresh List";
            this.RefreshBtn.UseVisualStyleBackColor = false;
            this.RefreshBtn.Click += new System.EventHandler(this.RefreshBtn_Click);
            // 
            // CorruptBTN
            // 
            this.CorruptBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CorruptBTN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(60)))), ((int)(((byte)(111)))));
            this.CorruptBTN.FlatAppearance.BorderSize = 0;
            this.CorruptBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CorruptBTN.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CorruptBTN.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(150)))), ((int)(((byte)(255)))));
            this.CorruptBTN.Image = global::LunarROMCorruptor.Properties.Resources.arrow_up_circle_solid;
            this.CorruptBTN.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.CorruptBTN.Location = new System.Drawing.Point(281, 17);
            this.CorruptBTN.Name = "CorruptBTN";
            this.CorruptBTN.Size = new System.Drawing.Size(136, 35);
            this.CorruptBTN.TabIndex = 2;
            this.CorruptBTN.Text = "Select Process for Corruption";
            this.CorruptBTN.UseVisualStyleBackColor = false;
            this.CorruptBTN.Click += new System.EventHandler(this.CorruptBTN_Click);
            // 
            // SelectProcess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 389);
            this.Controls.Add(this.ProcessList);
            this.Controls.Add(this.Panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(436, 322);
            this.Name = "SelectProcess";
            this.Text = "Select Process for Memory Corruption...";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelectProcess_FormClosing);
            this.Load += new System.EventHandler(this.SelectProcess_Load);
            this.Panel1.ResumeLayout(false);
            this.Panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.Button RefreshBtn;
        internal System.Windows.Forms.Button CorruptBTN;
        internal System.Windows.Forms.Panel Panel1;
        internal System.Windows.Forms.ListBox ProcessList;
        private System.Windows.Forms.Label Label1;
    }
}