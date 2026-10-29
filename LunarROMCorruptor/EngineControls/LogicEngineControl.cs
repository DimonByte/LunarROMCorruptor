using LunarROMCorruptor.Modules.CorruptionInternals;
using LunarROMCorruptor.Modules.CorruptionInternals.Engines;

namespace LunarROMCorruptor.EngineControls
{
    public partial class LogicEngineControl : UserControl
    {
        public LogicEngineControl()
        {
            InitializeComponent();
        }

        public LogicEngine.Settings GetSettings()
        {
            var option = EngineParser.ParseCorruptionOptions(BitwiseComboBox.Text) ?? throw new InvalidOperationException($"Invalid Logic Engine operation '{BitwiseComboBox.Text}'.");
            return new LogicEngine.Settings
            {
                Option = option,
                Value = (int)ValueBitwise.Value,
                RandomizeType = LogicRandomizeTypeCheckbox.Checked,
                RandomizeValue = LogicRandomizeValueCheckBox.Checked,
                ValueMin = (int)ValueBitwise.Minimum,
                ValueMax = (int)ValueBitwise.Maximum
            };
        }
    }
}
