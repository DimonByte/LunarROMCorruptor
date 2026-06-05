namespace LunarROMCorruptor.EngineControls
{
    public partial class MergeEngineControl : UserControl
    {
        public MergeEngineControl()
        {
            InitializeComponent();
        }

        private void MergeOpenFilebtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                try
                {
                    MergeFileLocationTxt.Text = openFileDialog1.FileName;
                    FileInfo myFile = new(Program.Form.FileSelectiontxt.Text);
                    long sizeInBytes = myFile.Length;
                    FileInfo myFile2 = new(MergeFileLocationTxt.Text);
                    long sizeInBytes2 = myFile2.Length;
                    if (sizeInBytes2 < sizeInBytes)
                    {
                        MessageBox.Show("This file must be the same/bigger size in order for this engine to work.");
                        MergeFileLocationTxt.Text = "";
                        return;
                    }
                }
                catch (ArgumentException)
                {
                    MessageBox.Show("Please load a file into the corruptor first before loading in a file in the merge engine. (Argument Exception)");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
    }
}
