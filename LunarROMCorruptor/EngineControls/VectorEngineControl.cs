using LunarROMCorruptor.Modules.CorruptionInternals.Engines;

namespace LunarROMCorruptor.EngineControls
{
    public partial class VectorEngineControl : UserControl
    {
        public VectorEngineControl()
        {
            InitializeComponent();
        }

        public VectorEngine.Settings GetSettings()
        {
            if (!Enum.TryParse(VectorModeComboBox.Text, out VectorEngine.VectorMode mode))
                throw new InvalidOperationException($"Invalid Vector2 Engine corruption type '{VectorModeComboBox.Text}'.");
            var values = VectorEngine.Settings.ParseValues(VectorValuesTxt.Text);
            if (mode == VectorEngine.VectorMode.REPLACE && values.Length == 0)
                throw new InvalidOperationException("The Vector2 Engine replacement values list is empty or invalid. Enter numbers separated by commas, e.g. -1, 0, 0.5, 100");
            double min = (double)VectorMinNumericUpDown.Value;
            double max = (double)VectorMaxNumericUpDown.Value;
            if (max < min)
                throw new InvalidOperationException("The Vector2 Engine maximum magnitude is smaller than the minimum.");
            return new VectorEngine.Settings
            {
                Mode = mode,
                Values = values,
                NudgePercent = (double)VectorNudgeNumericUpDown.Value,
                MinMagnitude = min,
                MaxMagnitude = max,
                Vector2Pairs = VectorPairsChkBox.Checked,
                Aligned = VectorAlignedChkBox.Checked,
                BigEndian = VectorBigEndianChkBox.Checked,
                IncludeZero = VectorIncludeZeroChkBox.Checked
            };
        }
    }
}
