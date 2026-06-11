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
        private void LoadAddresses()
        {
            var safeLocations = ExclusionEngine.SafeLocations.GetValueOrDefault(ExclusionTypeComboBox.Text, []);
            ExcludedAddressesListbox.Items.Clear();
            foreach (var safeLocation in safeLocations)
            {
                ExcludedAddressesListbox.Items.Add(safeLocation);
            }
        }

        private void ExclusionEngineControl_Load(object sender, EventArgs e)
        {
            LoadAddresses();
        }
    }
}
