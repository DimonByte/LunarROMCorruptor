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
using System.ComponentModel;

namespace LunarROMCorruptor.Controls
{
    /// <summary>
    /// The preset bar at the top of the Corruption Engine tab: pick a built-in (★) or user (👤) preset to load it, save the current
    /// engine settings, delete a user preset, import a shared preset or open the UserCorruptionEnginePresets folder.
    /// The main form tells it which engine is shown (<see cref="ShowEngine"/>) and how to find the engine's settings control.
    /// </summary>
    public partial class PresetBarControl : UserControl
    {
        private bool _listUpdating;

        /// <summary>
        /// Returns the settings control of an engine (the presets read/write its controls).
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] //Set in code, the designer must not serialize it
        public Func<CorruptionEngineType, Control?>? EngineFrameProvider { get; set; }

        /// <summary>
        /// Raised when a preset for another engine is loaded/imported: the main form should switch to that engine.
        /// </summary>
        public event Action<CorruptionEngineType>? EngineSwitchRequested;

        public CorruptionEngineType Engine { get; private set; } = CorruptionEngineType.INVALID;

        public PresetBarControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the presets of an engine.
        /// </summary>
        public void ShowEngine(CorruptionEngineType engine, string? selectPath = null)
        {
            Engine = engine;
            _listUpdating = true;
            try
            {
                PresetComboBox.BeginUpdate();
                PresetComboBox.Items.Clear();
                PresetComboBox.Items.Add("(choose a preset to load)");
                foreach (var preset in EnginePresets.BuiltIn(engine)) PresetComboBox.Items.Add(preset);
                foreach (var preset in EnginePresets.LoadUser(engine)) PresetComboBox.Items.Add(preset);
                int index = 0;
                if (selectPath != null)
                {
                    for (int k = 1; k < PresetComboBox.Items.Count; k++)
                        if (PresetComboBox.Items[k] is EnginePreset p && CorruptionCore.PathsEqual(p.FilePath, selectPath)) index = k;
                }
                PresetComboBox.SelectedIndex = index;
            }
            finally
            {
                PresetComboBox.EndUpdate();
                _listUpdating = false;
            }
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            var preset = PresetComboBox.SelectedItem as EnginePreset;
            PresetDescriptionLbl.Text = preset == null
                ? $"{PresetComboBox.Items.Count - 1} preset(s) for the {EngineParser.GetEngineDisplayName(Engine)}."
                : (string.IsNullOrWhiteSpace(preset.Description) ? preset.Name : preset.Description);
            ToolTipHelper.SetToolTip(PresetDescriptionLbl, PresetDescriptionLbl.Text);
            DeletePresetBtn.Enabled = preset is { BuiltIn: false };
        }

        /// <summary>
        /// Loads a preset into its engine (switching to that engine first if needed).
        /// </summary>
        public bool ApplyPreset(EnginePreset preset) => ApplySettings(EngineParser.ParseEngineType(preset.Engine), preset.Settings, preset.Name);

        /// <summary>
        /// Writes engine settings (from a preset or a stash's info) into the engine control.
        /// </summary>
        public bool ApplySettings(CorruptionEngineType engine, IReadOnlyDictionary<string, string> settings, string name)
        {
            if (engine == CorruptionEngineType.INVALID) return false;
            if (engine != Engine) EngineSwitchRequested?.Invoke(engine);
            var frame = EngineFrameProvider?.Invoke(engine);
            if (frame == null) return false;
            int applied = EnginePresets.Apply(frame, settings, out var unknown);
            TraceLogger.Log($"Engine settings loaded: {name} ({EngineParser.GetEngineDisplayName(engine)}, {applied} setting(s)).{(unknown.Count > 0 ? $" Ignored (not in this version): {string.Join(", ", unknown)}" : "")}");
            return true;
        }

        private void PresetComboBox_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (_listUpdating || PresetComboBox.SelectedItem is not EnginePreset preset) return;
            ApplyPreset(preset);
            UpdateDescription();
        }

        private void PresetComboBox_SelectedIndexChanged(object? sender, EventArgs e) => UpdateDescription();

        private void SavePresetBtn_Click(object? sender, EventArgs e)
        {
            var engine = Engine;
            var frame = EngineFrameProvider?.Invoke(engine);
            if (frame == null) return;
            string suggested = PresetComboBox.SelectedItem is EnginePreset current ? (current.BuiltIn ? current.Name + " (copy)" : current.Name) : "";
            string? name = InputBox.Ask($"Preset name for the {EngineParser.GetEngineDisplayName(engine)}", suggested)?.Trim();
            if (string.IsNullOrEmpty(name)) return;
            string description = InputBox.Ask("Description (optional): what does this preset do / what is it for?", PresetComboBox.SelectedItem is EnginePreset p && p.Name == name ? p.Description : "")?.Trim() ?? "";
            var preset = new EnginePreset { Engine = EngineParser.GetEngineDisplayName(engine), Name = name, Description = description, Settings = EnginePresets.Capture(frame) };
            try
            {
                string path;
                try
                {
                    path = EnginePresets.Save(preset, overwrite: false);
                }
                catch (IOException) when (MessageBox.Show($"A preset called '{name}' already exists. Replace it?", $"{nameof(LunarROMCorruptor)} - Save preset", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    path = EnginePresets.Save(preset, overwrite: true);
                }
                ShowEngine(engine, path);
            }
            catch (IOException)
            {
                //Not replaced
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to save the engine preset: {ex.Message}", StatusSeverityType.Error, true);
            }
        }

        private void DeletePresetBtn_Click(object? sender, EventArgs e)
        {
            if (PresetComboBox.SelectedItem is not EnginePreset { BuiltIn: false, FilePath: not null } preset) return;
            if (MessageBox.Show($"Delete the preset '{preset.Name}'?", $"{nameof(LunarROMCorruptor)} - Delete preset", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                File.Delete(preset.FilePath);
                TraceLogger.Log($"Engine preset deleted: {preset.FilePath}");
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to delete the preset: {ex.Message}", StatusSeverityType.Error, true);
            }
            ShowEngine(Engine);
        }

        private void ImportPresetBtn_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog { Filter = $"LRC engine presets (*{EnginePresets.Extension};*.json)|*{EnginePresets.Extension};*.json|All files (*.*)|*.*", Multiselect = true, Title = "Import engine presets" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            foreach (string file in dialog.FileNames) ImportPresetFile(file);
        }

        /// <summary>
        /// Copies a shared preset into the user preset folder, switches to its engine and loads it.
        /// </summary>
        public void ImportPresetFile(string file)
        {
            var preset = EnginePresets.Import(file);
            if (preset == null) return;
            ApplyPreset(preset);
            ShowEngine(EngineParser.ParseEngineType(preset.Engine), preset.FilePath);
            TraceLogger.Log($"Imported the {preset.Engine} preset '{preset.Name}'.", StatusSeverityType.Information);
        }

        private void OpenPresetFolderBtn_Click(object? sender, EventArgs e)
        {
            try
            {
                Directory.CreateDirectory(EnginePresets.UserPresetDirectory);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = EnginePresets.UserPresetDirectory, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to open the preset folder: {ex.Message}", StatusSeverityType.Error, true);
            }
        }
    }
}
