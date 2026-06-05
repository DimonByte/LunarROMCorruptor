namespace LunarROMCorruptor.EngineControls
{
    partial class VectorEngineControl
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
            Vector2EnginePanel = new Panel();
            Label21 = new Label();
            Vector2EnginePanel.SuspendLayout();
            SuspendLayout();
            // 
            // Vector2EnginePanel
            // 
            Vector2EnginePanel.BackColor = Color.FromArgb(10, 56, 71);
            Vector2EnginePanel.Controls.Add(Label21);
            Vector2EnginePanel.Dock = DockStyle.Fill;
            Vector2EnginePanel.Location = new Point(0, 0);
            Vector2EnginePanel.Name = "Vector2EnginePanel";
            Vector2EnginePanel.Size = new Size(642, 268);
            Vector2EnginePanel.TabIndex = 163;
            Vector2EnginePanel.Tag = "color:normal";
            // 
            // Label21
            // 
            Label21.AutoSize = true;
            Label21.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold | FontStyle.Underline);
            Label21.ForeColor = Color.FromArgb(192, 255, 255);
            Label21.Location = new Point(3, 0);
            Label21.Name = "Label21";
            Label21.Size = new Size(164, 17);
            Label21.TabIndex = 93;
            Label21.Text = "-Vector2 Engine Settings-";
            // 
            // VectorEngineControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(Vector2EnginePanel);
            Name = "VectorEngineControl";
            Size = new Size(642, 268);
            Vector2EnginePanel.ResumeLayout(false);
            Vector2EnginePanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        public Panel Vector2EnginePanel;
        private Label Label21;
    }
}
