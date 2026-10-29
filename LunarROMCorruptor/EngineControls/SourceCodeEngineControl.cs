using LunarROMCorruptor.Modules.CorruptionInternals.Engines;

namespace LunarROMCorruptor.EngineControls
{
    public partial class SourceCodeEngineControl : UserControl
    {
        public SourceCodeEngineControl()
        {
            InitializeComponent();
        }

        public SourceCodeEngine.Settings GetSettings()
        {
            var settings = new SourceCodeEngine.Settings
            {
                Booleans = SourceBooleansChkBox.Checked,
                Integers = SourceIntegersChkBox.Checked,
                Floats = SourceFloatsChkBox.Checked,
                Strings = SourceStringsChkBox.Checked,
                Nulls = SourceNullsChkBox.Checked,
                Operators = SourceOperatorsChkBox.Checked,
                Identifiers = SourceIdentifiersChkBox.Checked,
                NumberStrength = (double)SourceStrengthNumericUpDown.Value,
                Numbers = Enum.TryParse(SourceNumberModeComboBox.Text, out SourceCodeEngine.NumberMode numberMode) ? numberMode : SourceCodeEngine.NumberMode.RANDOM,
                StringCorruption = Enum.TryParse(SourceStringModeComboBox.Text, out SourceCodeEngine.StringMode stringMode) ? stringMode : SourceCodeEngine.StringMode.RANDOM,
                Comments = Enum.TryParse(SourceCommentStyleComboBox.Text, out SourceCodeEngine.CommentStyle style) ? style : SourceCodeEngine.CommentStyle.AUTO
            };
            if (!(settings.Booleans || settings.Integers || settings.Floats || settings.Strings || settings.Nulls || settings.Operators || settings.Identifiers))
                throw new InvalidOperationException("Select at least one value type for the Source Code Engine to corrupt.");
            return settings;
        }
    }
}
