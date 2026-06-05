namespace LunarROMCorruptor.EngineControls
{
    partial class LerpEngineControl
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
            LerpEnginePanel = new Panel();
            LerpSplitValueTrackBar = new TrackBar();
            LerpValueTxt = new TextBox();
            Label17 = new Label();
            Label20 = new Label();
            LerpEnginePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LerpSplitValueTrackBar).BeginInit();
            SuspendLayout();
            // 
            // LerpEnginePanel
            // 
            LerpEnginePanel.BackColor = Color.DarkOliveGreen;
            LerpEnginePanel.Controls.Add(LerpSplitValueTrackBar);
            LerpEnginePanel.Controls.Add(LerpValueTxt);
            LerpEnginePanel.Controls.Add(Label17);
            LerpEnginePanel.Controls.Add(Label20);
            LerpEnginePanel.Dock = DockStyle.Fill;
            LerpEnginePanel.Location = new Point(0, 0);
            LerpEnginePanel.Name = "LerpEnginePanel";
            LerpEnginePanel.Size = new Size(642, 268);
            LerpEnginePanel.TabIndex = 162;
            LerpEnginePanel.Tag = "color:normal";
            // 
            // LerpSplitValueTrackBar
            // 
            LerpSplitValueTrackBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LerpSplitValueTrackBar.Location = new Point(0, 53);
            LerpSplitValueTrackBar.Margin = new Padding(2, 3, 2, 3);
            LerpSplitValueTrackBar.Name = "LerpSplitValueTrackBar";
            LerpSplitValueTrackBar.Size = new Size(642, 45);
            LerpSplitValueTrackBar.TabIndex = 98;
            LerpSplitValueTrackBar.TabStop = false;
            LerpSplitValueTrackBar.TickFrequency = 0;
            LerpSplitValueTrackBar.Value = 5;
            LerpSplitValueTrackBar.Scroll += LerpSplitValueTrackBar_Scroll;
            // 
            // LerpValueTxt
            // 
            LerpValueTxt.Location = new Point(78, 25);
            LerpValueTxt.Name = "LerpValueTxt";
            LerpValueTxt.Size = new Size(100, 23);
            LerpValueTxt.TabIndex = 97;
            LerpValueTxt.Text = "0.5";
            LerpValueTxt.TextChanged += LerpValueTxt_TextChanged;
            // 
            // Label17
            // 
            Label17.AutoSize = true;
            Label17.Font = new Font("Segoe UI", 9.75F);
            Label17.ForeColor = Color.White;
            Label17.Location = new Point(9, 26);
            Label17.Name = "Label17";
            Label17.Size = new Size(71, 17);
            Label17.TabIndex = 96;
            Label17.Text = "Split Value:";
            // 
            // Label20
            // 
            Label20.AutoSize = true;
            Label20.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            Label20.ForeColor = Color.FromArgb(192, 255, 255);
            Label20.Location = new Point(3, 2);
            Label20.Name = "Label20";
            Label20.Size = new Size(145, 17);
            Label20.TabIndex = 93;
            Label20.Text = "-Lerp Engine Settings-";
            // 
            // LerpEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(LerpEnginePanel);
            Name = "LerpEngineControl";
            Size = new Size(642, 268);
            LerpEnginePanel.ResumeLayout(false);
            LerpEnginePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)LerpSplitValueTrackBar).EndInit();
            ResumeLayout(false);
        }

        #endregion

        public Panel LerpEnginePanel;
        private TrackBar LerpSplitValueTrackBar;
        internal TextBox LerpValueTxt;
        private Label Label17;
        private Label Label20;
    }
}
