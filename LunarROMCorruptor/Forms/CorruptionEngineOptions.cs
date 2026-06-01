//MIT License

//Copyright (c) 2026 DimonByte

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

namespace LunarROMCorruptor
{
    public partial class CorruptionEngineOptions : Form
    {
        public CorruptionEngineOptions()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (OpenFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                try
                {
                    MergeFileLocationTxt.Text = OpenFileDialog1.FileName;
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

        private void LerpSplitValueTrackBar_Scroll(object sender, EventArgs e)
        {
            //Set the value of the trackbar to the textbox and move the decimal point down one, e.g 10 becomes 1.0
            LerpValueTxt.Text = (LerpSplitValueTrackBar.Value / 10.0).ToString();
        }

        private void LerpValueTxt_TextChanged(object sender, EventArgs e)
        {
            try //catch any errors and if there is an error, set the trackbar value to 0
            {
                if (float.TryParse(LerpValueTxt.Text, out float value))
                {
                    LerpSplitValueTrackBar.Value = (int)(value * 10); //Convert the textbox text into a float and set the trackbar value to the float value
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error has occured: {Environment.NewLine + ex.Message}", $"Error parsing text - {nameof(LunarROMCorruptor)}");
                LerpSplitValueTrackBar.Value = 0;
                LerpValueTxt.Text = "0";
            }
        }
    }
}