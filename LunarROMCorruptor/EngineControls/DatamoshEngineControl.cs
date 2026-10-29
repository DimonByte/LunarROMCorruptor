using LunarROMCorruptor.Modules.CorruptionInternals.Engines;

namespace LunarROMCorruptor.EngineControls
{
    public partial class DatamoshEngineControl : UserControl
    {
        public DatamoshEngineControl()
        {
            InitializeComponent();
        }

        public DatamoshEngine.Settings GetSettings()
        {
            if (!Enum.TryParse(MoshModeComboBox.Text, out DatamoshEngine.MoshMode mode))
                throw new InvalidOperationException($"Invalid Datamosh Engine mosh type '{MoshModeComboBox.Text}'.");
            return new DatamoshEngine.Settings
            {
                Mode = mode,
                KeepFirstKeyframes = (int)MoshKeepNumericUpDown.Value,
                RepeatCount = (int)MoshRepeatNumericUpDown.Value,
                ReplaceRemoved = MoshReplaceChkBox.Checked
            };
        }
    }
}
