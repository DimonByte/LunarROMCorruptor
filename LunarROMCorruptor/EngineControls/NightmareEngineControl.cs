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

using LunarROMCorruptor.Modules;
using LunarROMCorruptor.Modules.CorruptionInternals;
using LunarROMCorruptor.Modules.CorruptionInternals.Engines;

namespace LunarROMCorruptor
{
    /// <summary>
    /// Nightmare+ Engine settings: the classic Nightmare Engine plus the exclusion types of the old Exclusion Engine.
    /// </summary>
    public partial class NightmareEngineControl : UserControl
    {
        private string _lastExclusionType = NightmareEngine.NoExclusion;

        public NightmareEngineControl()
        {
            InitializeComponent();
            foreach (string type in NightmareEngine.ExclusionTypes)
                ExclusionTypeComboBox.Items.Add(type);
            NightmareComboBox.SelectedIndex = 0;
            ExclusionTypeComboBox.SelectedIndex = 0; //None = classic Nightmare Engine
        }

        private void NightmareEngineControl_Load(object? sender, EventArgs e) => LoadAddresses();

        private void ExclusionTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string type = ExclusionTypeComboBox.Text;
            bool wasNone = _lastExclusionType == NightmareEngine.NoExclusion;
            bool isNone = type == NightmareEngine.NoExclusion;
            //Exclusion types used to always avoid crash-prone values (old Exclusion Engine), None is the plain Nightmare Engine
            if (wasNone != isNone) AvoidCrashValuesChkBox.Checked = !isNone;
            _lastExclusionType = type;
            LoadAddresses();
        }

        /// <summary>
        /// The exclusion type that will be used for the loaded file (Auto is resolved from the file header).
        /// </summary>
        private string ResolveType(FileBuffer? data)
        {
            string type = ExclusionTypeComboBox.Text;
            if (type == NightmareEngine.AutoExclusion)
                return data != null ? ExclusionEngine.DetectProfile(data.FilePath, data) ?? NightmareEngine.NoExclusion : NightmareEngine.NoExclusion;
            return type;
        }

        /// <summary>
        /// Shows the protected ranges for the selected exclusion type, including the ranges calculated from the loaded file.
        /// </summary>
        public void LoadAddresses()
        {
            ExcludedAddressesListbox.BeginUpdate();
            ExcludedAddressesListbox.Items.Clear();
            try
            {
                if (ExclusionTypeComboBox.Text == NightmareEngine.NoExclusion)
                {
                    ExcludedAddressesListbox.Items.Add("(None: every byte in the range can be corrupted)");
                    return;
                }
                List<ProtectedRange> ranges;
                string type;
                try
                {
                    if (CorruptionCore.IsLoaded && CorruptionCore.SourcePath != null && File.Exists(CorruptionCore.SourcePath))
                    {
                        using var data = FileBuffer.OpenRead(CorruptionCore.SourcePath, 4);
                        type = ResolveType(data);
                        ranges = type == NightmareEngine.NoExclusion ? [] : ExclusionEngine.GetProtectedRanges(type, data);
                    }
                    else
                    {
                        type = ResolveType(null);
                        ranges = type == NightmareEngine.NoExclusion ? [] : ExclusionEngine.GetProtectedRanges(type, null);
                        ExcludedAddressesListbox.Items.Add("(load a file to see ranges read from its header)");
                    }
                }
                catch (Exception ex)
                {
                    TraceLogger.Log($"Unable to read protected ranges from the loaded file: {ex.Message}", StatusSeverityType.Warning);
                    type = ExclusionTypeComboBox.Text;
                    ranges = ExclusionEngine.Profiles.ContainsKey(type) ? ExclusionEngine.GetProtectedRanges(type, null) : [];
                }
                if (ExclusionTypeComboBox.Text == NightmareEngine.AutoExclusion)
                    ExcludedAddressesListbox.Items.Add(type == NightmareEngine.NoExclusion ? "(Auto: file type not recognised, nothing is excluded)" : $"(Auto: {type})");
                foreach (var range in ranges)
                    ExcludedAddressesListbox.Items.Add(range);
                if (ranges.Count == 0 && type != NightmareEngine.NoExclusion)
                    ExcludedAddressesListbox.Items.Add("(ranges for this type are read from the loaded file)");
            }
            finally
            {
                ExcludedAddressesListbox.EndUpdate();
            }
        }

        public NightmareEngine.Settings GetSettings()
        {
            var option = EngineParser.ParseCorruptionOptions(NightmareComboBox.Text) ?? throw new InvalidOperationException($"Invalid Nightmare+ Engine corruption type '{NightmareComboBox.Text}'.");
            string type = ExclusionTypeComboBox.Text;
            if (type != NightmareEngine.NoExclusion && type != NightmareEngine.AutoExclusion && !ExclusionEngine.Profiles.ContainsKey(type))
                throw new InvalidOperationException($"Unknown Nightmare+ exclusion type '{type}'.");
            return new NightmareEngine.Settings
            {
                Option = option,
                TiltAmount = (int)IncreDecrenumbnightmare.Value,
                ExclusionProfile = type,
                AvoidCrashValues = AvoidCrashValuesChkBox.Checked
            };
        }
    }
}
