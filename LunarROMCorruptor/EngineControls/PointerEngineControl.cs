using LunarROMCorruptor.Modules.CorruptionInternals.Engines;
using System.Globalization;

namespace LunarROMCorruptor.EngineControls
{
    public partial class PointerEngineControl : UserControl
    {
        private const string GeneralHelp = "Swaps, shifts or copies pointers so the game loads a different texture, model, sound, animation or text instead of crashing. SWAP inside tables is the most stable. Compressed data can't be seen, use the Compression Engine for it.";

        public PointerEngineControl()
        {
            InitializeComponent();
            UpdateProfileHelp();
        }

        private void PointerProfileComboBox_SelectedIndexChanged(object? sender, EventArgs e) => UpdateProfileHelp();

        private void UpdateProfileHelp()
        {
            var profile = (PointerEngine.PointerProfile)Math.Max(0, PointerProfileComboBox.SelectedIndex);
            PointerHelpLbl.Text = $"{PointerEngine.Describe(profile)}\n{GeneralHelp}";
        }

        private static uint ParseHex(string text, string what)
        {
            string t = text.Trim();
            if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) t = t[2..];
            if (!uint.TryParse(t, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint value))
                throw new InvalidOperationException($"The Pointer Engine custom {what} '{text}' isn't a valid hex number (e.g. 80000000).");
            return value;
        }

        public PointerEngine.Settings GetSettings()
        {
            if (!Enum.TryParse(PointerModeComboBox.Text, out PointerEngine.PointerMode mode))
                throw new InvalidOperationException($"Invalid Pointer Engine corruption type '{PointerModeComboBox.Text}'.");
            var profile = (PointerEngine.PointerProfile)Math.Max(0, PointerProfileComboBox.SelectedIndex); //The combobox items are in the same order as the enum
            var targets = PointerEngine.DisplayListTargets.None;
            if (PointerTexturesChkBox.Checked) targets |= PointerEngine.DisplayListTargets.Textures;
            if (PointerVerticesChkBox.Checked) targets |= PointerEngine.DisplayListTargets.Vertices;
            if (PointerDisplayListsChkBox.Checked) targets |= PointerEngine.DisplayListTargets.DisplayLists;
            if (profile == PointerEngine.PointerProfile.N64DisplayLists && targets == PointerEngine.DisplayListTargets.None)
                throw new InvalidOperationException("Select at least one N64 display list target (textures, vertices or sub display lists).");
            var settings = new PointerEngine.Settings
            {
                Profile = profile,
                Mode = mode,
                ShiftAmount = (int)PointerShiftNumericUpDown.Value,
                TableWindow = (int)PointerWindowNumericUpDown.Value,
                TablesOnly = PointerTablesOnlyChkBox.Checked,
                SkipFunctionPointers = PointerSkipFunctionsChkBox.Checked,
                DisplayListTargets = targets,
                CustomBigEndian = PointerCustomBigEndianChkBox.Checked
            };
            if (profile == PointerEngine.PointerProfile.Custom)
            {
                settings.CustomMin = ParseHex(PointerCustomMinTxt.Text, "minimum");
                settings.CustomMax = ParseHex(PointerCustomMaxTxt.Text, "maximum");
            }
            return settings;
        }
    }
}
