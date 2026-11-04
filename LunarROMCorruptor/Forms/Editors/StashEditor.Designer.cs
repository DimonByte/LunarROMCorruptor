namespace LunarROMCorruptor
{
    partial class StashEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StashEditor));
            stashListbox = new Controls.StashListView();
            RemovedItemslstbx = new Controls.StashListView();
            LoadingPanel = new Panel();
            LoadStatusLbl = new Label();
            LoadProgressBar = new ProgressBar();
            LoadCancelBtn = new Button();
            valueStashnum = new NumericUpDown();
            LocationStash = new NumericUpDown();
            Label3 = new Label();
            Label2 = new Label();
            Label4 = new Label();
            OpenFileDialog = new OpenFileDialog();
            Label1 = new Label();
            newfilestashbtn = new Button();
            removeitembtn = new Button();
            additemsbtn = new Button();
            addbtn = new Button();
            remove50btn = new Button();
            removeselbtn = new Button();
            Panel1 = new Panel();
            restorebtn = new Button();
            SaveAndCorruptbtn = new Button();
            Openfilebtn = new Button();
            RemoveAllbtn = new Button();
            ToolsPanel = new Panel();
            MutateTitleLbl = new Label();
            MutatePercentLbl = new Label();
            MutatePercentNumericUpDown = new NumericUpDown();
            MutateModeLbl = new Label();
            MutateModeComboBox = new ComboBox();
            MutateAmountLbl = new Label();
            MutateAmountNumericUpDown = new NumericUpDown();
            MutateBtn = new Button();
            MutateCorruptBtn = new Button();
            MutateHelpLbl = new Label();
            ListToolsLbl = new Label();
            AppendStashBtn = new Button();
            SortStashBtn = new Button();
            DedupeStashBtn = new Button();
            StashCountLbl = new Label();
            SaveFileDialog = new SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)valueStashnum).BeginInit();
            ((System.ComponentModel.ISupportInitialize)LocationStash).BeginInit();
            Panel1.SuspendLayout();
            ToolsPanel.SuspendLayout();
            LoadingPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MutatePercentNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MutateAmountNumericUpDown).BeginInit();
            SuspendLayout();
            // 
            // stashListbox
            // 
            stashListbox.BackColor = Color.FromArgb(39, 43, 83);
            stashListbox.BorderStyle = BorderStyle.None;
            stashListbox.Dock = DockStyle.Fill;
            stashListbox.ForeColor = Color.FromArgb(224, 224, 224);
            stashListbox.Location = new Point(160, 0);
            stashListbox.Name = "stashListbox";
            stashListbox.Size = new Size(309, 485);
            stashListbox.TabIndex = 161;
            // 
            // RemovedItemslstbx
            // 
            RemovedItemslstbx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            RemovedItemslstbx.BackColor = Color.FromArgb(52, 60, 111);
            RemovedItemslstbx.BorderStyle = BorderStyle.None;
            RemovedItemslstbx.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point, 0);
            RemovedItemslstbx.ForeColor = Color.FromArgb(224, 224, 224);
            RemovedItemslstbx.Location = new Point(12, 282);
            RemovedItemslstbx.Name = "RemovedItemslstbx";
            RemovedItemslstbx.Size = new Size(136, 148);
            RemovedItemslstbx.TabIndex = 160;
            // 
            // LoadingPanel
            // 
            LoadingPanel.BackColor = Color.FromArgb(28, 27, 55);
            LoadingPanel.Controls.Add(LoadStatusLbl);
            LoadingPanel.Controls.Add(LoadProgressBar);
            LoadingPanel.Controls.Add(LoadCancelBtn);
            LoadingPanel.Dock = DockStyle.Bottom;
            LoadingPanel.Location = new Point(160, 485);
            LoadingPanel.Name = "LoadingPanel";
            LoadingPanel.Size = new Size(309, 44);
            LoadingPanel.TabIndex = 163;
            LoadingPanel.Visible = false;
            // 
            // LoadStatusLbl
            // 
            LoadStatusLbl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LoadStatusLbl.AutoEllipsis = true;
            LoadStatusLbl.Font = new Font("Segoe UI", 7.5F);
            LoadStatusLbl.ForeColor = Color.FromArgb(224, 224, 224);
            LoadStatusLbl.Location = new Point(6, 4);
            LoadStatusLbl.Name = "LoadStatusLbl";
            LoadStatusLbl.Size = new Size(225, 15);
            LoadStatusLbl.TabIndex = 0;
            LoadStatusLbl.Text = "Loading...";
            // 
            // LoadProgressBar
            // 
            LoadProgressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LoadProgressBar.Location = new Point(6, 22);
            LoadProgressBar.Maximum = 1000;
            LoadProgressBar.MarqueeAnimationSpeed = 30;
            LoadProgressBar.Name = "LoadProgressBar";
            LoadProgressBar.Size = new Size(225, 14);
            LoadProgressBar.TabIndex = 1;
            // 
            // LoadCancelBtn
            // 
            LoadCancelBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LoadCancelBtn.BackColor = Color.FromArgb(52, 60, 111);
            LoadCancelBtn.FlatAppearance.BorderSize = 0;
            LoadCancelBtn.FlatStyle = FlatStyle.Flat;
            LoadCancelBtn.ForeColor = Color.FromArgb(213, 216, 216);
            LoadCancelBtn.Location = new Point(237, 11);
            LoadCancelBtn.Name = "LoadCancelBtn";
            LoadCancelBtn.Size = new Size(66, 23);
            LoadCancelBtn.TabIndex = 2;
            LoadCancelBtn.Text = "Cancel";
            LoadCancelBtn.UseVisualStyleBackColor = false;
            LoadCancelBtn.Click += LoadCancelBtn_Click;
            // 
            // valueStashnum
            // 
            valueStashnum.BackColor = Color.FromArgb(52, 60, 111);
            valueStashnum.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            valueStashnum.ForeColor = Color.FromArgb(230, 230, 230);
            valueStashnum.Location = new Point(70, 209);
            valueStashnum.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            valueStashnum.Name = "valueStashnum";
            valueStashnum.Size = new Size(78, 22);
            valueStashnum.TabIndex = 97;
            valueStashnum.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // LocationStash
            // 
            LocationStash.BackColor = Color.FromArgb(52, 60, 111);
            LocationStash.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            LocationStash.ForeColor = Color.FromArgb(230, 230, 230);
            LocationStash.Location = new Point(70, 183);
            LocationStash.Maximum = new decimal(new int[] { -727379969, 232, 0, 0 });
            LocationStash.Name = "LocationStash";
            LocationStash.Size = new Size(78, 22);
            LocationStash.TabIndex = 97;
            LocationStash.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // Label3
            // 
            Label3.AutoSize = true;
            Label3.Font = new Font("Segoe UI", 9.75F);
            Label3.ForeColor = Color.FromArgb(230, 230, 230);
            Label3.Location = new Point(22, 210);
            Label3.Name = "Label3";
            Label3.Size = new Size(42, 17);
            Label3.TabIndex = 95;
            Label3.Text = "Value:";
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Font = new Font("Segoe UI", 9.75F);
            Label2.ForeColor = Color.FromArgb(230, 230, 230);
            Label2.Location = new Point(9, 183);
            Label2.Name = "Label2";
            Label2.Size = new Size(60, 17);
            Label2.TabIndex = 95;
            Label2.Text = "Location:";
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.Font = new Font("Segoe UI", 9.75F);
            Label4.ForeColor = Color.FromArgb(230, 230, 230);
            Label4.Location = new Point(12, 262);
            Label4.Name = "Label4";
            Label4.Size = new Size(101, 17);
            Label4.TabIndex = 95;
            Label4.Text = "Removed items:";
            // 
            // OpenFileDialog
            // 
            OpenFileDialog.Filter = "All files (*.*)|*.*";
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Font = new Font("Segoe UI", 9.75F);
            Label1.ForeColor = Color.FromArgb(230, 230, 230);
            Label1.Location = new Point(9, 162);
            Label1.Name = "Label1";
            Label1.Size = new Size(139, 17);
            Label1.TabIndex = 95;
            Label1.Text = "Create new stash item:";
            // 
            // newfilestashbtn
            // 
            newfilestashbtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            newfilestashbtn.BackColor = Color.FromArgb(52, 60, 111);
            newfilestashbtn.FlatAppearance.BorderSize = 0;
            newfilestashbtn.FlatStyle = FlatStyle.Flat;
            newfilestashbtn.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            newfilestashbtn.ForeColor = Color.FromArgb(213, 216, 216);
            newfilestashbtn.Location = new Point(12, 465);
            newfilestashbtn.Name = "newfilestashbtn";
            newfilestashbtn.Size = new Size(136, 23);
            newfilestashbtn.TabIndex = 2;
            newfilestashbtn.Text = "Save as new stash";
            newfilestashbtn.UseVisualStyleBackColor = false;
            newfilestashbtn.Click += Newfilestashbtn_Click;
            // 
            // removeitembtn
            // 
            removeitembtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            removeitembtn.BackColor = Color.FromArgb(52, 60, 111);
            removeitembtn.FlatAppearance.BorderSize = 0;
            removeitembtn.FlatStyle = FlatStyle.Flat;
            removeitembtn.ForeColor = Color.FromArgb(213, 216, 216);
            removeitembtn.Location = new Point(87, 436);
            removeitembtn.Name = "removeitembtn";
            removeitembtn.Size = new Size(61, 23);
            removeitembtn.TabIndex = 2;
            removeitembtn.Text = "Remove";
            removeitembtn.UseVisualStyleBackColor = false;
            removeitembtn.Click += Removeitembtn_Click;
            // 
            // additemsbtn
            // 
            additemsbtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            additemsbtn.BackColor = Color.FromArgb(52, 60, 111);
            additemsbtn.FlatAppearance.BorderSize = 0;
            additemsbtn.FlatStyle = FlatStyle.Flat;
            additemsbtn.ForeColor = Color.FromArgb(213, 216, 216);
            additemsbtn.Location = new Point(12, 436);
            additemsbtn.Name = "additemsbtn";
            additemsbtn.Size = new Size(57, 23);
            additemsbtn.TabIndex = 2;
            additemsbtn.Text = "Add";
            additemsbtn.UseVisualStyleBackColor = false;
            additemsbtn.Click += Additemsbtn_Click;
            // 
            // addbtn
            // 
            addbtn.BackColor = Color.FromArgb(52, 60, 111);
            addbtn.FlatAppearance.BorderSize = 0;
            addbtn.FlatStyle = FlatStyle.Flat;
            addbtn.ForeColor = Color.FromArgb(213, 216, 216);
            addbtn.Location = new Point(12, 236);
            addbtn.Name = "addbtn";
            addbtn.Size = new Size(136, 23);
            addbtn.TabIndex = 2;
            addbtn.Text = "Add";
            addbtn.UseVisualStyleBackColor = false;
            addbtn.Click += Addbtn_Click;
            // 
            // remove50btn
            // 
            remove50btn.BackColor = Color.FromArgb(52, 60, 111);
            remove50btn.FlatAppearance.BorderSize = 0;
            remove50btn.FlatStyle = FlatStyle.Flat;
            remove50btn.ForeColor = Color.FromArgb(213, 216, 216);
            remove50btn.Location = new Point(12, 99);
            remove50btn.Name = "remove50btn";
            remove50btn.Size = new Size(136, 23);
            remove50btn.TabIndex = 2;
            remove50btn.Text = "Remove 50%";
            remove50btn.UseVisualStyleBackColor = false;
            remove50btn.Click += Remove50btn_Click;
            // 
            // removeselbtn
            // 
            removeselbtn.BackColor = Color.FromArgb(52, 60, 111);
            removeselbtn.FlatAppearance.BorderSize = 0;
            removeselbtn.FlatStyle = FlatStyle.Flat;
            removeselbtn.ForeColor = Color.FromArgb(213, 216, 216);
            removeselbtn.Location = new Point(12, 70);
            removeselbtn.Name = "removeselbtn";
            removeselbtn.Size = new Size(136, 23);
            removeselbtn.TabIndex = 2;
            removeselbtn.Text = "Remove Selected";
            removeselbtn.UseVisualStyleBackColor = false;
            removeselbtn.Click += Removeselbtn_Click;
            // 
            // Panel1
            // 
            Panel1.BackColor = Color.FromArgb(28, 27, 55);
            Panel1.Controls.Add(RemovedItemslstbx);
            Panel1.Controls.Add(valueStashnum);
            Panel1.Controls.Add(LocationStash);
            Panel1.Controls.Add(Label3);
            Panel1.Controls.Add(Label2);
            Panel1.Controls.Add(Label4);
            Panel1.Controls.Add(Label1);
            Panel1.Controls.Add(restorebtn);
            Panel1.Controls.Add(SaveAndCorruptbtn);
            Panel1.Controls.Add(newfilestashbtn);
            Panel1.Controls.Add(removeitembtn);
            Panel1.Controls.Add(additemsbtn);
            Panel1.Controls.Add(addbtn);
            Panel1.Controls.Add(Openfilebtn);
            Panel1.Controls.Add(RemoveAllbtn);
            Panel1.Controls.Add(remove50btn);
            Panel1.Controls.Add(removeselbtn);
            Panel1.Dock = DockStyle.Left;
            Panel1.Location = new Point(0, 0);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(160, 529);
            Panel1.TabIndex = 160;
            // 
            // restorebtn
            // 
            restorebtn.BackColor = Color.FromArgb(52, 60, 111);
            restorebtn.FlatAppearance.BorderSize = 0;
            restorebtn.FlatStyle = FlatStyle.Flat;
            restorebtn.ForeColor = Color.FromArgb(213, 216, 216);
            restorebtn.Image = Properties.Resources.restart;
            restorebtn.ImageAlign = ContentAlignment.MiddleLeft;
            restorebtn.Location = new Point(12, 41);
            restorebtn.Name = "restorebtn";
            restorebtn.Size = new Size(136, 23);
            restorebtn.TabIndex = 2;
            restorebtn.Text = "Restore";
            restorebtn.UseVisualStyleBackColor = false;
            restorebtn.Click += Restorebtn_Click;
            // 
            // SaveAndCorruptbtn
            // 
            SaveAndCorruptbtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            SaveAndCorruptbtn.BackColor = Color.FromArgb(52, 60, 111);
            SaveAndCorruptbtn.FlatAppearance.BorderSize = 0;
            SaveAndCorruptbtn.FlatStyle = FlatStyle.Flat;
            SaveAndCorruptbtn.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            SaveAndCorruptbtn.ForeColor = Color.FromArgb(200, 150, 255);
            SaveAndCorruptbtn.Location = new Point(12, 494);
            SaveAndCorruptbtn.Name = "SaveAndCorruptbtn";
            SaveAndCorruptbtn.Size = new Size(136, 23);
            SaveAndCorruptbtn.TabIndex = 2;
            SaveAndCorruptbtn.Text = "Save stash and Corrupt";
            SaveAndCorruptbtn.UseVisualStyleBackColor = false;
            SaveAndCorruptbtn.Click += SaveAndCorruptbtn_Click;
            // 
            // Openfilebtn
            // 
            Openfilebtn.BackColor = Color.FromArgb(52, 60, 111);
            Openfilebtn.FlatAppearance.BorderSize = 0;
            Openfilebtn.FlatStyle = FlatStyle.Flat;
            Openfilebtn.ForeColor = Color.FromArgb(213, 216, 216);
            Openfilebtn.Image = Properties.Resources.upload;
            Openfilebtn.ImageAlign = ContentAlignment.MiddleLeft;
            Openfilebtn.Location = new Point(12, 12);
            Openfilebtn.Name = "Openfilebtn";
            Openfilebtn.Size = new Size(136, 23);
            Openfilebtn.TabIndex = 2;
            Openfilebtn.Text = "Open File";
            Openfilebtn.UseVisualStyleBackColor = false;
            Openfilebtn.Click += Openfilebtn_Click;
            // 
            // RemoveAllbtn
            // 
            RemoveAllbtn.BackColor = Color.FromArgb(52, 60, 111);
            RemoveAllbtn.FlatAppearance.BorderSize = 0;
            RemoveAllbtn.FlatStyle = FlatStyle.Flat;
            RemoveAllbtn.Font = new Font("Segoe UI", 6F, FontStyle.Regular, GraphicsUnit.Point, 0);
            RemoveAllbtn.ForeColor = Color.FromArgb(213, 216, 216);
            RemoveAllbtn.Location = new Point(12, 128);
            RemoveAllbtn.Name = "RemoveAllbtn";
            RemoveAllbtn.Size = new Size(136, 23);
            RemoveAllbtn.TabIndex = 2;
            RemoveAllbtn.Text = "Permanently Remove ALL from list";
            RemoveAllbtn.UseVisualStyleBackColor = false;
            RemoveAllbtn.Click += RemoveAllbtn_Click;
            // 
            // ToolsPanel
            // 
            ToolsPanel.Controls.Add(MutateTitleLbl);
            ToolsPanel.Controls.Add(MutatePercentLbl);
            ToolsPanel.Controls.Add(MutatePercentNumericUpDown);
            ToolsPanel.Controls.Add(MutateModeLbl);
            ToolsPanel.Controls.Add(MutateModeComboBox);
            ToolsPanel.Controls.Add(MutateAmountLbl);
            ToolsPanel.Controls.Add(MutateAmountNumericUpDown);
            ToolsPanel.Controls.Add(MutateBtn);
            ToolsPanel.Controls.Add(MutateCorruptBtn);
            ToolsPanel.Controls.Add(MutateHelpLbl);
            ToolsPanel.Controls.Add(ListToolsLbl);
            ToolsPanel.Controls.Add(AppendStashBtn);
            ToolsPanel.Controls.Add(SortStashBtn);
            ToolsPanel.Controls.Add(DedupeStashBtn);
            ToolsPanel.Controls.Add(StashCountLbl);
            ToolsPanel.Dock = DockStyle.Right;
            ToolsPanel.Location = new Point(469, 0);
            ToolsPanel.Name = "ToolsPanel";
            ToolsPanel.Size = new Size(170, 529);
            ToolsPanel.TabIndex = 162;
            // 
            // MutateTitleLbl
            // 
            MutateTitleLbl.AutoSize = true;
            MutateTitleLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            MutateTitleLbl.ForeColor = Color.FromArgb(224, 224, 224);
            MutateTitleLbl.Location = new Point(10, 12);
            MutateTitleLbl.Name = "MutateTitleLbl";
            MutateTitleLbl.Size = new Size(79, 15);
            MutateTitleLbl.TabIndex = 200;
            MutateTitleLbl.Text = "Mutate stash";
            // 
            // MutatePercentLbl
            // 
            MutatePercentLbl.AutoSize = true;
            MutatePercentLbl.Font = new Font("Segoe UI", 7.5F);
            MutatePercentLbl.ForeColor = Color.FromArgb(224, 224, 224);
            MutatePercentLbl.Location = new Point(10, 36);
            MutatePercentLbl.Name = "MutatePercentLbl";
            MutatePercentLbl.Size = new Size(104, 12);
            MutatePercentLbl.TabIndex = 201;
            MutatePercentLbl.Text = "Mutate % of the items:";
            // 
            // MutatePercentNumericUpDown
            // 
            MutatePercentNumericUpDown.BackColor = Color.FromArgb(52, 60, 111);
            MutatePercentNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            MutatePercentNumericUpDown.ForeColor = Color.FromArgb(230, 230, 230);
            MutatePercentNumericUpDown.Location = new Point(12, 52);
            MutatePercentNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            MutatePercentNumericUpDown.Name = "MutatePercentNumericUpDown";
            MutatePercentNumericUpDown.Size = new Size(146, 22);
            MutatePercentNumericUpDown.TabIndex = 202;
            MutatePercentNumericUpDown.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // MutateModeLbl
            // 
            MutateModeLbl.AutoSize = true;
            MutateModeLbl.Font = new Font("Segoe UI", 7.5F);
            MutateModeLbl.ForeColor = Color.FromArgb(224, 224, 224);
            MutateModeLbl.Location = new Point(10, 80);
            MutateModeLbl.Name = "MutateModeLbl";
            MutateModeLbl.Size = new Size(47, 12);
            MutateModeLbl.TabIndex = 203;
            MutateModeLbl.Text = "Mutation:";
            // 
            // MutateModeComboBox
            // 
            MutateModeComboBox.BackColor = Color.FromArgb(52, 60, 111);
            MutateModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            MutateModeComboBox.FlatStyle = FlatStyle.Flat;
            MutateModeComboBox.ForeColor = Color.FromArgb(213, 216, 216);
            MutateModeComboBox.FormattingEnabled = true;
            MutateModeComboBox.Items.AddRange(new object[] { "Random value", "Nudge value", "Move address" });
            MutateModeComboBox.Location = new Point(12, 96);
            MutateModeComboBox.Name = "MutateModeComboBox";
            MutateModeComboBox.Size = new Size(146, 21);
            MutateModeComboBox.TabIndex = 204;
            // 
            // MutateAmountLbl
            // 
            MutateAmountLbl.AutoSize = true;
            MutateAmountLbl.Font = new Font("Segoe UI", 7.5F);
            MutateAmountLbl.ForeColor = Color.FromArgb(224, 224, 224);
            MutateAmountLbl.Location = new Point(10, 124);
            MutateAmountLbl.Name = "MutateAmountLbl";
            MutateAmountLbl.Size = new Size(116, 12);
            MutateAmountLbl.TabIndex = 205;
            MutateAmountLbl.Text = "Amount (nudge / move):";
            // 
            // MutateAmountNumericUpDown
            // 
            MutateAmountNumericUpDown.BackColor = Color.FromArgb(52, 60, 111);
            MutateAmountNumericUpDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            MutateAmountNumericUpDown.ForeColor = Color.FromArgb(230, 230, 230);
            MutateAmountNumericUpDown.Location = new Point(12, 140);
            MutateAmountNumericUpDown.Maximum = new decimal(new int[] { -727379969, 232, 0, 0 });
            MutateAmountNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            MutateAmountNumericUpDown.Name = "MutateAmountNumericUpDown";
            MutateAmountNumericUpDown.Size = new Size(146, 22);
            MutateAmountNumericUpDown.TabIndex = 206;
            MutateAmountNumericUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // MutateBtn
            // 
            MutateBtn.BackColor = Color.FromArgb(52, 60, 111);
            MutateBtn.FlatAppearance.BorderSize = 0;
            MutateBtn.FlatStyle = FlatStyle.Flat;
            MutateBtn.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            MutateBtn.ForeColor = Color.FromArgb(213, 216, 216);
            MutateBtn.Location = new Point(12, 170);
            MutateBtn.Name = "MutateBtn";
            MutateBtn.Size = new Size(146, 23);
            MutateBtn.TabIndex = 207;
            MutateBtn.Text = "Mutate list";
            MutateBtn.UseVisualStyleBackColor = false;
            MutateBtn.Click += MutateBtn_Click;
            // 
            // MutateCorruptBtn
            // 
            MutateCorruptBtn.BackColor = Color.FromArgb(52, 60, 111);
            MutateCorruptBtn.FlatAppearance.BorderSize = 0;
            MutateCorruptBtn.FlatStyle = FlatStyle.Flat;
            MutateCorruptBtn.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            MutateCorruptBtn.ForeColor = Color.FromArgb(213, 216, 216);
            MutateCorruptBtn.Location = new Point(12, 199);
            MutateCorruptBtn.Name = "MutateCorruptBtn";
            MutateCorruptBtn.Size = new Size(146, 23);
            MutateCorruptBtn.TabIndex = 208;
            MutateCorruptBtn.Text = "Mutate, save new && corrupt";
            MutateCorruptBtn.UseVisualStyleBackColor = false;
            MutateCorruptBtn.Click += MutateCorruptBtn_Click;
            // 
            // MutateHelpLbl
            // 
            MutateHelpLbl.Font = new Font("Segoe UI", 7.5F);
            MutateHelpLbl.ForeColor = Color.Silver;
            MutateHelpLbl.Location = new Point(10, 226);
            MutateHelpLbl.Name = "MutateHelpLbl";
            MutateHelpLbl.Size = new Size(146, 54);
            MutateHelpLbl.TabIndex = 209;
            MutateHelpLbl.Text = "Makes a variation of this corruption. Only byte entries are mutated, the stash file is not changed until you save.";
            // 
            // ListToolsLbl
            // 
            ListToolsLbl.AutoSize = true;
            ListToolsLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            ListToolsLbl.ForeColor = Color.FromArgb(224, 224, 224);
            ListToolsLbl.Location = new Point(10, 286);
            ListToolsLbl.Name = "ListToolsLbl";
            ListToolsLbl.Size = new Size(56, 15);
            ListToolsLbl.TabIndex = 210;
            ListToolsLbl.Text = "List tools";
            // 
            // AppendStashBtn
            // 
            AppendStashBtn.BackColor = Color.FromArgb(52, 60, 111);
            AppendStashBtn.FlatAppearance.BorderSize = 0;
            AppendStashBtn.FlatStyle = FlatStyle.Flat;
            AppendStashBtn.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            AppendStashBtn.ForeColor = Color.FromArgb(213, 216, 216);
            AppendStashBtn.Location = new Point(12, 308);
            AppendStashBtn.Name = "AppendStashBtn";
            AppendStashBtn.Size = new Size(146, 23);
            AppendStashBtn.TabIndex = 211;
            AppendStashBtn.Text = "Append stash file...";
            AppendStashBtn.UseVisualStyleBackColor = false;
            AppendStashBtn.Click += AppendStashBtn_Click;
            // 
            // SortStashBtn
            // 
            SortStashBtn.BackColor = Color.FromArgb(52, 60, 111);
            SortStashBtn.FlatAppearance.BorderSize = 0;
            SortStashBtn.FlatStyle = FlatStyle.Flat;
            SortStashBtn.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            SortStashBtn.ForeColor = Color.FromArgb(213, 216, 216);
            SortStashBtn.Location = new Point(12, 337);
            SortStashBtn.Name = "SortStashBtn";
            SortStashBtn.Size = new Size(146, 23);
            SortStashBtn.TabIndex = 212;
            SortStashBtn.Text = "Sort by address";
            SortStashBtn.UseVisualStyleBackColor = false;
            SortStashBtn.Click += SortStashBtn_Click;
            // 
            // DedupeStashBtn
            // 
            DedupeStashBtn.BackColor = Color.FromArgb(52, 60, 111);
            DedupeStashBtn.FlatAppearance.BorderSize = 0;
            DedupeStashBtn.FlatStyle = FlatStyle.Flat;
            DedupeStashBtn.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            DedupeStashBtn.ForeColor = Color.FromArgb(213, 216, 216);
            DedupeStashBtn.Location = new Point(12, 366);
            DedupeStashBtn.Name = "DedupeStashBtn";
            DedupeStashBtn.Size = new Size(146, 23);
            DedupeStashBtn.TabIndex = 213;
            DedupeStashBtn.Text = "Remove duplicates";
            DedupeStashBtn.UseVisualStyleBackColor = false;
            DedupeStashBtn.Click += DedupeStashBtn_Click;
            // 
            // StashCountLbl
            // 
            StashCountLbl.AutoSize = true;
            StashCountLbl.Font = new Font("Segoe UI", 7.5F);
            StashCountLbl.ForeColor = Color.Silver;
            StashCountLbl.Location = new Point(10, 396);
            StashCountLbl.Name = "StashCountLbl";
            StashCountLbl.Size = new Size(36, 12);
            StashCountLbl.TabIndex = 214;
            StashCountLbl.Text = "0 items";
            // 
            // SaveFileDialog
            // 
            SaveFileDialog.Filter = "All Files *.*|*.*";
            // 
            // StashEditor
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(29, 29, 35);
            ClientSize = new Size(639, 529);
            Controls.Add(stashListbox);
            Controls.Add(LoadingPanel);
            Controls.Add(Panel1);
            Controls.Add(ToolsPanel);
            Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(655, 568);
            Name = "StashEditor";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Stash Editor";
            FormClosing += StashEditor_FormClosing;
            ((System.ComponentModel.ISupportInitialize)valueStashnum).EndInit();
            ((System.ComponentModel.ISupportInitialize)LocationStash).EndInit();
            Panel1.ResumeLayout(false);
            Panel1.PerformLayout();
            ToolsPanel.ResumeLayout(false);
            ToolsPanel.PerformLayout();
            LoadingPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)MutatePercentNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)MutateAmountNumericUpDown).EndInit();
            ResumeLayout(false);

        }

        #endregion

        internal LunarROMCorruptor.Controls.StashListView stashListbox;
        internal LunarROMCorruptor.Controls.StashListView RemovedItemslstbx;
        internal System.Windows.Forms.Panel LoadingPanel;
        internal System.Windows.Forms.Label LoadStatusLbl;
        internal System.Windows.Forms.ProgressBar LoadProgressBar;
        internal System.Windows.Forms.Button LoadCancelBtn;
        internal System.Windows.Forms.NumericUpDown valueStashnum;
        internal System.Windows.Forms.NumericUpDown LocationStash;
        private System.Windows.Forms.Label Label3;
        private System.Windows.Forms.Label Label2;
        private System.Windows.Forms.Label Label4;
        internal System.Windows.Forms.OpenFileDialog OpenFileDialog;
        private System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Button restorebtn;
        internal System.Windows.Forms.Button newfilestashbtn;
        internal System.Windows.Forms.Button removeitembtn;
        internal System.Windows.Forms.Button additemsbtn;
        internal System.Windows.Forms.Button addbtn;
        internal System.Windows.Forms.Button Openfilebtn;
        internal System.Windows.Forms.Button remove50btn;
        internal System.Windows.Forms.Button removeselbtn;
        internal System.Windows.Forms.Panel Panel1;
        internal System.Windows.Forms.SaveFileDialog SaveFileDialog;
        internal System.Windows.Forms.Button RemoveAllbtn;
        internal System.Windows.Forms.Button SaveAndCorruptbtn;
        internal System.Windows.Forms.Panel ToolsPanel;
        internal System.Windows.Forms.Label MutateTitleLbl;
        internal System.Windows.Forms.Label MutatePercentLbl;
        internal System.Windows.Forms.NumericUpDown MutatePercentNumericUpDown;
        internal System.Windows.Forms.Label MutateModeLbl;
        internal System.Windows.Forms.ComboBox MutateModeComboBox;
        internal System.Windows.Forms.Label MutateAmountLbl;
        internal System.Windows.Forms.NumericUpDown MutateAmountNumericUpDown;
        internal System.Windows.Forms.Button MutateBtn;
        internal System.Windows.Forms.Button MutateCorruptBtn;
        internal System.Windows.Forms.Label MutateHelpLbl;
        internal System.Windows.Forms.Label ListToolsLbl;
        internal System.Windows.Forms.Button AppendStashBtn;
        internal System.Windows.Forms.Button SortStashBtn;
        internal System.Windows.Forms.Button DedupeStashBtn;
        internal System.Windows.Forms.Label StashCountLbl;
    }
}