using LunarROMCorruptor.Modules.CorruptionInternals;
using LunarROMCorruptor.Modules.CorruptionInternals.Engines;

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
                    var mergeFile = new FileInfo(openFileDialog1.FileName);
                    if (mergeFile.Length == 0)
                    {
                        MessageBox.Show("The selected merge file is empty.", "Merge Engine", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    MergeFileLocationTxt.Text = mergeFile.FullName;
                    if (CorruptionCore.IsLoaded && mergeFile.Length < CorruptionCore.SourceLength && ReplaceByteWithSamePos.Checked)
                    {
                        //Smaller merge files used to be rejected (and crashed the engine), now they wrap around.
                        MessageBox.Show("The merge file is smaller than the loaded file. With 'same position' enabled the merge file will wrap around (repeat) past its end.", "Merge Engine", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Merge Engine", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public MergeEngine.Settings GetSettings()
        {
            if (string.IsNullOrWhiteSpace(MergeFileLocationTxt.Text) || !File.Exists(MergeFileLocationTxt.Text))
                throw new InvalidOperationException("Merge file location is empty or the file doesn't exist. Please select a merge file.");
            var option = EngineParser.ParseCorruptionOptions(CorrTypeMerge.Text) ?? throw new InvalidOperationException($"Invalid Merge Engine corruption type '{CorrTypeMerge.Text}'.");
            return new MergeEngine.Settings
            {
                MergeFilePath = MergeFileLocationTxt.Text,
                Option = option,
                SamePosition = ReplaceByteWithSamePos.Checked,
                Mod256 = Mod256MergeEnginechkbox.Checked
            };
        }
    }
}
