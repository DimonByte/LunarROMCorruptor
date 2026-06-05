namespace LunarROMCorruptor.EngineControls
{
    public partial class LerpEngineControl : UserControl
    {
        public LerpEngineControl()
        {
            InitializeComponent();
        }

        private void LerpSplitValueTrackBar_Scroll(object sender, EventArgs e)
        {
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
