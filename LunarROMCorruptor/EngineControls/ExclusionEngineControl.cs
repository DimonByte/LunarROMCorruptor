using LunarROMCorruptor.Modules;
using LunarROMCorruptor.Modules.CorruptionInternals;
using LunarROMCorruptor.Modules.CorruptionInternals.Engines;

namespace LunarROMCorruptor.EngineControls
{
    public partial class ExclusionEngineControl : UserControl
    {
        public ExclusionEngineControl()
        {
            InitializeComponent();
        }

        private void ExclusionTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadAddresses();
        }

        /// <summary>
        /// Shows the protected ranges for the selected profile, including the ranges calculated from the loaded file.
        /// </summary>
        public void LoadAddresses()
        {
            ExcludedAddressesListbox.BeginUpdate();
            ExcludedAddressesListbox.Items.Clear();
            List<ProtectedRange> ranges;
            try
            {
                if (CorruptionCore.IsLoaded && CorruptionCore.SourcePath != null && File.Exists(CorruptionCore.SourcePath))
                {
                    using var data = FileBuffer.OpenRead(CorruptionCore.SourcePath, 4);
                    ranges = ExclusionEngine.GetProtectedRanges(ExclusionTypeComboBox.Text, data);
                }
                else
                {
                    ranges = ExclusionEngine.GetProtectedRanges(ExclusionTypeComboBox.Text, null);
                    ExcludedAddressesListbox.Items.Add("(load a file to see ranges read from its header)");
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to read protected ranges from the loaded file: {ex.Message}", StatusSeverityType.Warning);
                ranges = ExclusionEngine.GetProtectedRanges(ExclusionTypeComboBox.Text, null);
            }
            foreach (var range in ranges)
            {
                ExcludedAddressesListbox.Items.Add(range);
            }
            if (ranges.Count == 0)
                ExcludedAddressesListbox.Items.Add("(ranges for this profile are read from the loaded file)");
            ExcludedAddressesListbox.EndUpdate();
        }

        private void ExclusionEngineControl_Load(object sender, EventArgs e)
        {
            LoadAddresses();
        }

        public ExclusionEngine.Settings GetSettings()
        {
            var option = EngineParser.ParseCorruptionOptions(NightmareComboBox.Text) ?? throw new InvalidOperationException($"Invalid Exclusion Engine corruption type '{NightmareComboBox.Text}'.");
            if (!ExclusionEngine.Profiles.ContainsKey(ExclusionTypeComboBox.Text))
                throw new InvalidOperationException($"Unknown Exclusion Engine profile '{ExclusionTypeComboBox.Text}'.");
            return new ExclusionEngine.Settings
            {
                Profile = ExclusionTypeComboBox.Text,
                Option = option,
                TiltAmount = (int)IncreDecrenumbnightmare.Value
            };
        }
    }
}
