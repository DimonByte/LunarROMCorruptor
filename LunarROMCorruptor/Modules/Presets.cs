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

using LunarROMCorruptor.Modules.CorruptionInternals;

namespace LunarROMCorruptor.Modules
{
    /// <summary>
    /// A saved set of engine settings. Presets are JSON files (.lrcpreset) so they can be shared: copy one into the
    /// UserCorruptionEnginePresets folder (or use Import / drag it onto LRC) and it shows up in the preset list of its engine.
    /// Settings are stored by control name, so a preset made with an older version still loads (unknown names are ignored).
    /// </summary>
    public sealed class EnginePreset
    {
        public string Format { get; set; } = EnginePresets.FormatName;
        public int Version { get; set; } = 1;
        public string Engine { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Dictionary<string, string> Settings { get; set; } = [];

        [System.Text.Json.Serialization.JsonIgnore] public bool BuiltIn { get; set; }
        [System.Text.Json.Serialization.JsonIgnore] public string? FilePath { get; set; }

        public override string ToString() => BuiltIn ? $"★ {Name}" : $"👤 {Name}";
    }

    public static class EnginePresets
    {
        public const string FormatName = "LunarROMCorruptor engine preset";
        public const string Extension = ".lrcpreset";
        public static readonly string UserPresetDirectory = Path.Combine(Application.StartupPath, "UserCorruptionEnginePresets");

        //Machine specific values that shouldn't travel with a shared preset
        private static readonly HashSet<string> SkippedControls = new(StringComparer.OrdinalIgnoreCase) { "MergeFileLocationTxt" };

        private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        #region Reading/writing the engine controls

        private static IEnumerable<Control> Walk(Control root)
        {
            foreach (Control child in root.Controls)
            {
                yield return child;
                foreach (var nested in Walk(child))
                    yield return nested;
            }
        }

        private static bool IsSetting(Control c) => !string.IsNullOrEmpty(c.Name) && !SkippedControls.Contains(c.Name) &&
            c is ComboBox or NumericUpDown or CheckBox or RadioButton or TextBox or TrackBar;

        /// <summary>
        /// Reads every setting control of an engine control (combo boxes, numbers, checkboxes, radio buttons, text boxes, trackbars).
        /// </summary>
        public static Dictionary<string, string> Capture(Control engineControl)
        {
            var values = new Dictionary<string, string>();
            foreach (var c in Walk(engineControl).Where(IsSetting))
            {
                values[c.Name] = c switch
                {
                    ComboBox combo => combo.Text,
                    NumericUpDown number => number.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    CheckBox check => check.Checked ? "true" : "false",
                    RadioButton radio => radio.Checked ? "true" : "false",
                    TrackBar bar => bar.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    _ => c.Text
                };
            }
            return values;
        }

        private static int Order(Control c) => c switch
        {
            ComboBox => 0, //Combo boxes first: changing them can update other controls (e.g. Nightmare+ exclusion type ticks "avoid crash values")
            TrackBar => 1,
            NumericUpDown => 2,
            CheckBox or RadioButton => 3,
            _ => 4 //Text last (the Lerp text box moves its trackbar)
        };

        /// <summary>
        /// Writes the preset values into the engine control. Returns how many settings were applied; names that don't exist (preset made
        /// with another version) are returned in <paramref name="unknown"/>.
        /// </summary>
        public static int Apply(Control engineControl, IReadOnlyDictionary<string, string> values, out List<string> unknown)
        {
            var controls = Walk(engineControl).Where(IsSetting).GroupBy(c => c.Name).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            unknown = [.. values.Keys.Where(k => !controls.ContainsKey(k))];
            int applied = 0;
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            foreach (var (name, value) in values.Where(v => controls.ContainsKey(v.Key)).OrderBy(v => Order(controls[v.Key])))
            {
                var c = controls[name];
                switch (c)
                {
                    case ComboBox combo:
                        int index = -1;
                        for (int k = 0; k < combo.Items.Count; k++)
                        {
                            if (string.Equals(combo.GetItemText(combo.Items[k]), value, StringComparison.OrdinalIgnoreCase)) { index = k; break; }
                        }
                        if (index >= 0) combo.SelectedIndex = index;
                        else if (combo.DropDownStyle != ComboBoxStyle.DropDownList) combo.Text = value;
                        else { unknown.Add($"{name}={value}"); continue; }
                        break;
                    case NumericUpDown number:
                        if (!decimal.TryParse(value, System.Globalization.NumberStyles.Number, inv, out decimal d)) { unknown.Add($"{name}={value}"); continue; }
                        number.Value = Math.Clamp(d, number.Minimum, number.Maximum);
                        break;
                    case CheckBox check:
                        check.Checked = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;
                    case RadioButton radio:
                        if (value.Equals("true", StringComparison.OrdinalIgnoreCase)) radio.Checked = true;
                        break;
                    case TrackBar bar:
                        if (!int.TryParse(value, System.Globalization.NumberStyles.Integer, inv, out int t)) { unknown.Add($"{name}={value}"); continue; }
                        bar.Value = Math.Clamp(t, bar.Minimum, bar.Maximum);
                        break;
                    default:
                        c.Text = value;
                        break;
                }
                applied++;
            }
            return applied;
        }

        #endregion

        #region Files

        private static string SafeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            string clean = new([.. name.Select(ch => invalid.Contains(ch) ? '_' : ch)]);
            clean = clean.Trim().TrimEnd('.');
            return clean.Length == 0 ? "Preset" : clean;
        }

        public static EnginePreset? Read(string path, bool logErrors = true)
        {
            try
            {
                //Read by hand so hand-written presets can use plain JSON numbers/booleans ("Value": 40) as well as strings ("Value": "40")
                using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path), new System.Text.Json.JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = System.Text.Json.JsonCommentHandling.Skip });
                var root = doc.RootElement;
                string Str(string name) => root.TryGetProperty(name, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.String ? v.GetString() ?? "" : "";
                var preset = new EnginePreset { Engine = Str("Engine"), Name = Str("Name"), Description = Str("Description") };
                if (root.TryGetProperty("Version", out var version) && version.TryGetInt32(out int ver)) preset.Version = ver;
                if (string.IsNullOrWhiteSpace(preset.Engine) || !root.TryGetProperty("Settings", out var settings) || settings.ValueKind != System.Text.Json.JsonValueKind.Object)
                    throw new InvalidDataException("This isn't an LRC engine preset (Engine or Settings is missing).");
                foreach (var setting in settings.EnumerateObject())
                {
                    preset.Settings[setting.Name] = setting.Value.ValueKind switch
                    {
                        System.Text.Json.JsonValueKind.String => setting.Value.GetString() ?? "",
                        System.Text.Json.JsonValueKind.True => "true",
                        System.Text.Json.JsonValueKind.False => "false",
                        _ => setting.Value.GetRawText()
                    };
                }
                if (EngineParser.ParseEngineType(preset.Engine) == CorruptionEngineType.INVALID)
                    throw new InvalidDataException($"Unknown engine '{preset.Engine}'.");
                if (string.IsNullOrWhiteSpace(preset.Name)) preset.Name = Path.GetFileNameWithoutExtension(path);
                preset.FilePath = path;
                return preset;
            }
            catch (Exception ex)
            {
                if (logErrors) TraceLogger.Log($"Unable to read the engine preset {path}: {ex.Message}", StatusSeverityType.Error, true);
                return null;
            }
        }

        /// <summary>
        /// User presets for one engine (from the UserCorruptionEnginePresets folder, subfolders included).
        /// </summary>
        public static List<EnginePreset> LoadUser(CorruptionEngineType engine)
        {
            var list = new List<EnginePreset>();
            try
            {
                if (!Directory.Exists(UserPresetDirectory)) return list;
                foreach (string file in Directory.EnumerateFiles(UserPresetDirectory, "*" + Extension, SearchOption.AllDirectories))
                {
                    var preset = Read(file, logErrors: false);
                    if (preset == null)
                    {
                        TraceLogger.Log($"Skipped an invalid engine preset: {file}", StatusSeverityType.Warning);
                        continue;
                    }
                    if (EngineParser.ParseEngineType(preset.Engine) == engine) list.Add(preset);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to read the user engine presets: {ex.Message}", StatusSeverityType.Warning);
            }
            return [.. list.OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)];
        }

        /// <summary>
        /// Saves a preset into the UserCorruptionEnginePresets folder as "&lt;Engine&gt; - &lt;Name&gt;.lrcpreset". Returns the file path.
        /// </summary>
        public static string Save(EnginePreset preset, bool overwrite)
        {
            Directory.CreateDirectory(UserPresetDirectory);
            string path = Path.Combine(UserPresetDirectory, $"{SafeFileName(preset.Engine)} - {SafeFileName(preset.Name)}{Extension}");
            if (File.Exists(path) && !overwrite) throw new IOException($"A preset called '{preset.Name}' already exists.");
            File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(preset, JsonOptions));
            preset.FilePath = path;
            TraceLogger.Log($"Engine preset saved: {path}");
            return path;
        }

        /// <summary>
        /// Copies a preset file someone shared into the user preset folder. Returns the imported preset or null.
        /// </summary>
        public static EnginePreset? Import(string path)
        {
            var preset = Read(path);
            if (preset == null) return null;
            preset.BuiltIn = false;
            string target = Path.Combine(UserPresetDirectory, $"{SafeFileName(preset.Engine)} - {SafeFileName(preset.Name)}{Extension}");
            if (CorruptionCore.PathsEqual(target, path)) return preset;
            if (File.Exists(target) && MessageBox.Show($"A preset called '{preset.Name}' for the {preset.Engine} already exists. Replace it?", $"{nameof(LunarROMCorruptor)} - Import preset", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return null;
            Save(preset, overwrite: true);
            return preset;
        }

        #endregion

        /// <summary>
        /// The presets that ship with LRC (see <see cref="BuiltInEnginePresets"/>).
        /// </summary>
        public static List<EnginePreset> BuiltIn(CorruptionEngineType engine) => BuiltInEnginePresets.For(engine);

    }

    /// <summary>
    /// The engine presets that ship with LRC (hard coded, read only, shown with a ★). To add one: add a P(...) line with the engine,
    /// a name, a description and the settings by control name (the same names a saved .lrcpreset uses).
    /// </summary>
    public static class BuiltInEnginePresets
    {

        private static EnginePreset P(CorruptionEngineType engine, string name, string description, params (string Key, object Value)[] settings)
        {
            var dict = new Dictionary<string, string>();
            foreach (var (key, value) in settings)
            {
                dict[key] = value switch
                {
                    bool b => b ? "true" : "false",
                    IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
                    _ => value.ToString() ?? ""
                };
            }
            return new EnginePreset { Engine = EngineParser.GetEngineDisplayName(engine), Name = name, Description = description, Settings = dict, BuiltIn = true };
        }

        /// <summary>
        /// The presets that ship with LRC (hard coded, read only). Save a copy to change one.
        /// </summary>
        public static List<EnginePreset> For(CorruptionEngineType engine)
        {
            const CorruptionEngineType NM = CorruptionEngineType.NightmareEngine, LE = CorruptionEngineType.LerpEngine, LO = CorruptionEngineType.LogicEngine,
                MA = CorruptionEngineType.ManualEngine, ME = CorruptionEngineType.MergeEngine, VE = CorruptionEngineType.VectorEngine,
                BL = CorruptionEngineType.BlocksEngine, PO = CorruptionEngineType.PointerEngine, CO = CorruptionEngineType.CompressionEngine,
                DA = CorruptionEngineType.DatamoshEngine, SC = CorruptionEngineType.SourceCodeEngine;
            //Manual Engine presets turn every option off first, then enable the ones they use
            (string, object)[] manualOff = [("IncrementCHECK", false), ("SHIFTBYTECHECK", false), ("MakeBitEqualCHECK", false), ("ReplaceCHECK", false),
                ("PasterandombitCHECK", false), ("RepeatRandomBitCHECK", false), ("MULTIORDIVIDECHeck", false)];
            (string, object)[] Manual(params (string, object)[] on) => [.. manualOff.Where(o => !on.Any(x => x.Item1 == o.Item1)), .. on];
            (string, object)[] sourceOff = [("SourceBooleansChkBox", false), ("SourceIntegersChkBox", false), ("SourceFloatsChkBox", false), ("SourceStringsChkBox", false),
                ("SourceNullsChkBox", false), ("SourceOperatorsChkBox", false), ("SourceIdentifiersChkBox", false)];
            (string, object)[] Source(params (string, object)[] on) => [.. sourceOff.Where(o => !on.Any(x => x.Item1 == o.Item1)), .. on];

            var all = new List<EnginePreset>
            {
                //Nightmare+
                P(NM, "Classic Nightmare", "The original Nightmare Engine: random bytes anywhere in the range, nothing excluded.",
                    ("NightmareComboBox", "RANDOM"), ("IncreDecrenumbnightmare", 1), ("ExclusionTypeComboBox", "None"), ("AvoidCrashValuesChkBox", false)),
                P(NM, "Safe (auto-detect platform)", "Random bytes, but the header/boot code of the detected platform is left alone and crash-prone values are avoided. A good first try for any ROM.",
                    ("NightmareComboBox", "RANDOM"), ("IncreDecrenumbnightmare", 1), ("ExclusionTypeComboBox", "Auto"), ("AvoidCrashValuesChkBox", true)),
                P(NM, "Gentle tilt", "Small +/- 3 changes instead of random values: colours, stats and positions drift instead of breaking. Header protected (Auto).",
                    ("NightmareComboBox", "TILT"), ("IncreDecrenumbnightmare", 3), ("ExclusionTypeComboBox", "Auto"), ("AvoidCrashValuesChkBox", true)),
                P(NM, "N64 safe mix", "N64: RANDOMTILT with +/- 8, ROM header, IPL3 and entry code protected (the checksum is fixed automatically).",
                    ("NightmareComboBox", "RANDOMTILT"), ("IncreDecrenumbnightmare", 8), ("ExclusionTypeComboBox", "N64"), ("AvoidCrashValuesChkBox", true)),
                P(NM, "NES / SNES / Genesis safe", "Cartridge ROMs: random values with the header and interrupt vectors of the detected console protected.",
                    ("NightmareComboBox", "RANDOM"), ("IncreDecrenumbnightmare", 1), ("ExclusionTypeComboBox", "Auto"), ("AvoidCrashValuesChkBox", true)),
                P(NM, "Disc image (PS1) safe", "Raw PS1 BIN: the system area, volume descriptors and root directory stay intact so the disc still boots.",
                    ("NightmareComboBox", "RANDOM"), ("IncreDecrenumbnightmare", 1), ("ExclusionTypeComboBox", "PS1"), ("AvoidCrashValuesChkBox", true)),

                //Lerp
                P(LE, "Even blend (0.5)", "Each corrupted byte becomes the average of its two neighbours: smooths textures/audio into soft smears.", ("LerpValueTxt", "0.5")),
                P(LE, "Copy next byte (0.1)", "Mostly copies the next byte: subtle stretching of graphics and sound.", ("LerpValueTxt", "0.1")),
                P(LE, "Copy previous byte (0.9)", "Mostly copies the previous byte: smears data forwards.", ("LerpValueTxt", "0.9")),

                //Logic
                P(LO, "Bit flip (XOR 1)", "Flips the lowest bit: the smallest possible change, good for subtle glitches.",
                    ("BitwiseComboBox", "XOR"), ("ValueBitwise", 1), ("LogicRandomizeValueCheckBox", false), ("LogicRandomizeTypeCheckbox", false)),
                P(LO, "Colour scramble (XOR 128)", "Flips the top bit: palettes and sprite colours jump to the other half of the range.",
                    ("BitwiseComboBox", "XOR"), ("ValueBitwise", 128), ("LogicRandomizeValueCheckBox", false), ("LogicRandomizeTypeCheckbox", false)),
                P(LO, "Darken (AND 0x0F)", "Clears the top 4 bits: values get smaller, graphics get darker / sparser.",
                    ("BitwiseComboBox", "AND"), ("ValueBitwise", 15), ("LogicRandomizeValueCheckBox", false), ("LogicRandomizeTypeCheckbox", false)),
                P(LO, "Byte swap", "Swaps each corrupted byte with the next one: scrambles 16-bit values and text.",
                    ("BitwiseComboBox", "SWAP"), ("ValueBitwise", 1), ("LogicRandomizeValueCheckBox", false), ("LogicRandomizeTypeCheckbox", false)),
                P(LO, "Logic chaos", "A random operation with a random value for every byte.",
                    ("BitwiseComboBox", "XOR"), ("ValueBitwise", 1), ("LogicRandomizeValueCheckBox", true), ("LogicRandomizeTypeCheckbox", true)),

                //Manual
                P(MA, "Increment +1", "Adds 1 to every corrupted byte (clamped at 255).", Manual(("IncrementCHECK", true), ("IncrementNumericUpDown", 1))),
                P(MA, "Decrement -1", "Subtracts 1 from every corrupted byte (clamped at 0).", Manual(("IncrementCHECK", true), ("IncrementNumericUpDown", -1))),
                P(MA, "Zero out", "Sets every corrupted byte to 0x00: holes in graphics, silent audio, removed objects.", Manual(("MakeBitEqualCHECK", true), ("ByteEqualNumericUpDown", 0))),
                P(MA, "Halve values", "Divides every corrupted byte by 2.", Manual(("MULTIORDIVIDECHeck", true), ("DivideRadio", true), ("MathOperationNumericUpDown", 2))),
                P(MA, "Repeat chunks", "Copies 8 bytes from each corrupted position to random places in the range.", Manual(("RepeatRandomBitCHECK", true), ("RepeatNumericUpDown", 8))),
                P(MA, "Paste random bytes", "Each corrupted byte becomes a copy of a random byte from the range.", Manual(("PasterandombitCHECK", true))),

                //Merge
                P(ME, "Overlay", "Copies the merge file's byte at the same position over the corrupted byte.",
                    ("CorrTypeMerge", "NONE"), ("ReplaceByteWithSamePos", true), ("Mod256MergeEnginechkbox", false)),
                P(ME, "Add (wrap around)", "Adds the merge file's byte to the corrupted byte (mod 256).",
                    ("CorrTypeMerge", "NONE"), ("ReplaceByteWithSamePos", true), ("Mod256MergeEnginechkbox", true)),
                P(ME, "Difference", "The corrupted byte becomes the difference between it and the merge file's byte.",
                    ("CorrTypeMerge", "RANGE"), ("ReplaceByteWithSamePos", true), ("Mod256MergeEnginechkbox", false)),
                P(ME, "Random merge bytes", "Copies random bytes from anywhere in the merge file.",
                    ("CorrTypeMerge", "NONE"), ("ReplaceByteWithSamePos", false), ("Mod256MergeEnginechkbox", false)),

                //Vector2
                P(VE, "N64 models (big-endian)", "N64: nudges big-endian floats by up to 25% (model positions, scales, physics). The byte order is detected from the ROM header and code is skipped.",
                    ("VectorModeComboBox", "NUDGE"), ("VectorNudgeNumericUpDown", 25), ("VectorMinNumericUpDown", 0.0001m), ("VectorMaxNumericUpDown", 100000),
                    ("VectorPairsChkBox", true), ("VectorAlignedChkBox", true), ("VectorBigEndianChkBox", true), ("VectorIncludeZeroChkBox", false), ("VectorSkipCodeChkBox", true)),
                P(VE, "N64 wild", "N64: random floats between -1000 and 1000. Models explode and physics go crazy (crashes more often).",
                    ("VectorModeComboBox", "RANDOM"), ("VectorNudgeNumericUpDown", 25), ("VectorMinNumericUpDown", 0.001m), ("VectorMaxNumericUpDown", 1000),
                    ("VectorPairsChkBox", true), ("VectorAlignedChkBox", true), ("VectorBigEndianChkBox", true), ("VectorIncludeZeroChkBox", false), ("VectorSkipCodeChkBox", true)),
                P(VE, "Generic 3D model (little-endian)", "PC / PS1-PS2-PSP / Xbox / Switch model files: each float (X, Y, Z of a vertex) nudged by up to 15% on its own.",
                    ("VectorModeComboBox", "NUDGE"), ("VectorNudgeNumericUpDown", 15), ("VectorMinNumericUpDown", 0.0001m), ("VectorMaxNumericUpDown", 100000),
                    ("VectorPairsChkBox", false), ("VectorAlignedChkBox", true), ("VectorBigEndianChkBox", false), ("VectorIncludeZeroChkBox", false), ("VectorSkipCodeChkBox", true)),
                P(VE, "Generic 3D model (big-endian)", "GameCube / Wii / PS3 / Xbox 360 model files: each float nudged by up to 15%.",
                    ("VectorModeComboBox", "NUDGE"), ("VectorNudgeNumericUpDown", 15), ("VectorMinNumericUpDown", 0.0001m), ("VectorMaxNumericUpDown", 100000),
                    ("VectorPairsChkBox", false), ("VectorAlignedChkBox", true), ("VectorBigEndianChkBox", true), ("VectorIncludeZeroChkBox", false), ("VectorSkipCodeChkBox", true)),
                P(VE, "Subtle wobble", "Nudges by up to 5%: things are slightly off without breaking. Little-endian, change the byte order for console files.",
                    ("VectorModeComboBox", "NUDGE"), ("VectorNudgeNumericUpDown", 5), ("VectorMinNumericUpDown", 0.0001m), ("VectorMaxNumericUpDown", 100000),
                    ("VectorPairsChkBox", true), ("VectorAlignedChkBox", true), ("VectorIncludeZeroChkBox", false), ("VectorSkipCodeChkBox", true)),
                P(VE, "Mirror (negate)", "Flips the sign of floats: mirrored geometry, reversed speeds, upside-down cameras.",
                    ("VectorModeComboBox", "NEGATE"), ("VectorMinNumericUpDown", 0.0001m), ("VectorMaxNumericUpDown", 100000),
                    ("VectorPairsChkBox", true), ("VectorAlignedChkBox", true), ("VectorIncludeZeroChkBox", false), ("VectorSkipCodeChkBox", true)),
                P(VE, "Snap to -1 / 0 / 1", "Replaces floats with -1, 0 or 1: flattened models, stuck or reversed movement.",
                    ("VectorModeComboBox", "REPLACE"), ("VectorValuesTxt", "-1, 0, 1"), ("VectorMinNumericUpDown", 0.0001m), ("VectorMaxNumericUpDown", 100000),
                    ("VectorPairsChkBox", true), ("VectorAlignedChkBox", true), ("VectorIncludeZeroChkBox", false), ("VectorSkipCodeChkBox", true)),
                P(VE, "Swap X/Y", "Swaps the two floats of each pair: 2D positions, UVs and speeds rotate 90 degrees.",
                    ("VectorModeComboBox", "SWAPXY"), ("VectorMinNumericUpDown", 0.0001m), ("VectorMaxNumericUpDown", 100000),
                    ("VectorPairsChkBox", true), ("VectorAlignedChkBox", true), ("VectorIncludeZeroChkBox", false), ("VectorSkipCodeChkBox", true)),

                //Blocks
                P(BL, "Pixel sort (RGB)", "Sorts 3-byte RGB pixels in 3 KB blocks: the classic pixel sorting glitch for raw/BMP images.",
                    ("BlocksModeComboBox", "SORT"), ("BlocksSizeNumericUpDown", 3072), ("BlocksElementNumericUpDown", 3), ("BlocksDescendingChkBox", false)),
                P(BL, "Pixel sort (RGBA / 32-bit)", "Sorts 4-byte pixels in 4 KB blocks (32-bit images, textures).",
                    ("BlocksModeComboBox", "SORT"), ("BlocksSizeNumericUpDown", 4096), ("BlocksElementNumericUpDown", 4), ("BlocksDescendingChkBox", false)),
                P(BL, "Audio echo (16-bit stereo)", "Mixes each 4 KB block with the audio 0.1 s earlier (44.1 kHz, 16-bit stereo WAV).",
                    ("BlocksModeComboBox", "ECHO"), ("BlocksSizeNumericUpDown", 4096), ("BlocksElementNumericUpDown", 4), ("BlocksEchoNumericUpDown", 17640)),
                P(BL, "Audio stutter", "Repeats one 16-bit sample across 2 KB blocks: stutters and buzzes.",
                    ("BlocksModeComboBox", "SMEAR"), ("BlocksSizeNumericUpDown", 2048), ("BlocksElementNumericUpDown", 2)),
                P(BL, "Shuffle chunks", "Shuffles 16-byte pieces inside 256 byte blocks: tiles, text and tables get mixed up.",
                    ("BlocksModeComboBox", "SHUFFLE"), ("BlocksSizeNumericUpDown", 256), ("BlocksElementNumericUpDown", 16)),
                P(BL, "Random block glitch", "A random block mode for every block.",
                    ("BlocksModeComboBox", "RANDOM"), ("BlocksSizeNumericUpDown", 256), ("BlocksElementNumericUpDown", 1), ("BlocksEchoNumericUpDown", 4096)),

                //Pointer
                P(PO, "Auto: swap in tables (safe)", "Detects the platform and swaps pointers inside pointer tables: the game loads the wrong asset instead of crashing.",
                    ("PointerProfileComboBox", "Auto"), ("PointerModeComboBox", "SWAP"), ("PointerWindowNumericUpDown", 64), ("PointerTablesOnlyChkBox", true), ("PointerSkipFunctionsChkBox", true)),
                P(PO, "N64 texture swap", "Swaps texture addresses inside N64 model display lists: models wear the wrong textures.",
                    ("PointerProfileComboBox", "N64 display lists"), ("PointerModeComboBox", "SWAP"), ("PointerWindowNumericUpDown", 256),
                    ("PointerTexturesChkBox", true), ("PointerVerticesChkBox", false), ("PointerDisplayListsChkBox", false)),
                P(PO, "N64 model mix", "Swaps vertex and sub display list addresses: body parts and models get mixed up.",
                    ("PointerProfileComboBox", "N64 display lists"), ("PointerModeComboBox", "SWAP"), ("PointerWindowNumericUpDown", 256),
                    ("PointerTexturesChkBox", false), ("PointerVerticesChkBox", true), ("PointerDisplayListsChkBox", true)),
                P(PO, "GBA asset swap", "Swaps 0x08xxxxxx ROM pointers in tables (graphics, palettes, text, scripts).",
                    ("PointerProfileComboBox", "GBA ROM pointers"), ("PointerModeComboBox", "SWAP"), ("PointerWindowNumericUpDown", 64), ("PointerTablesOnlyChkBox", true), ("PointerSkipFunctionsChkBox", true)),
                P(PO, "SNES table swap", "Swaps 24-bit long pointers in tables (LoROM/HiROM detected).",
                    ("PointerProfileComboBox", "SNES 24-bit pointers"), ("PointerModeComboBox", "SWAP"), ("PointerWindowNumericUpDown", 48), ("PointerTablesOnlyChkBox", true)),
                P(PO, "NES table swap", "Swaps 16-bit pointers in PRG-ROM tables (text, level data, music).",
                    ("PointerProfileComboBox", "NES 16-bit pointers"), ("PointerModeComboBox", "SWAP"), ("PointerWindowNumericUpDown", 32), ("PointerTablesOnlyChkBox", true)),
                P(PO, "Off by a bit (shift)", "Moves pointers a few bytes: textures and animations start in the wrong place.",
                    ("PointerProfileComboBox", "Auto"), ("PointerModeComboBox", "SHIFT"), ("PointerShiftNumericUpDown", 16), ("PointerTablesOnlyChkBox", true), ("PointerSkipFunctionsChkBox", true)),
                P(PO, "Chaos (random targets)", "Any pointer can point at any other target, tables or not. Wild and crash-prone.",
                    ("PointerProfileComboBox", "Auto"), ("PointerModeComboBox", "RANDOMTARGET"), ("PointerTablesOnlyChkBox", false), ("PointerSkipFunctionsChkBox", true)),

                //Compression
                P(CO, "All formats, random", "Unpacks every supported compressed block, randomises bytes inside and packs it again.",
                    ("CompressionOptionComboBox", "RANDOM"), ("CompressionTiltNumericUpDown", 1), ("CompressionYaz0ChkBox", true), ("CompressionYay0ChkBox", true),
                    ("CompressionMio0ChkBox", true), ("CompressionLz10ChkBox", true), ("CompressionLz11ChkBox", true)),
                P(CO, "N64 gentle tilt", "N64 (Yaz0/Yay0/MIO0) only, +/- 4 tilt: compressed textures and models change colour/shape without falling apart.",
                    ("CompressionOptionComboBox", "TILT"), ("CompressionTiltNumericUpDown", 4), ("CompressionYaz0ChkBox", true), ("CompressionYay0ChkBox", true),
                    ("CompressionMio0ChkBox", true), ("CompressionLz10ChkBox", false), ("CompressionLz11ChkBox", false)),
                P(CO, "GBA / DS (LZ10/LZ11)", "LZ10/LZ11 blocks only (GBA/DS graphics and data).",
                    ("CompressionOptionComboBox", "RANDOMTILT"), ("CompressionTiltNumericUpDown", 8), ("CompressionYaz0ChkBox", false), ("CompressionYay0ChkBox", false),
                    ("CompressionMio0ChkBox", false), ("CompressionLz10ChkBox", true), ("CompressionLz11ChkBox", true)),

                //Datamosh
                P(DA, "Classic datamosh", "Removes every keyframe after the first: motion from the next scene smears over the old picture.",
                    ("MoshModeComboBox", "REMOVE_KEYFRAMES"), ("MoshKeepNumericUpDown", 1), ("MoshReplaceChkBox", true)),
                P(DA, "Bloom", "Repeats P-frames 10 times: movement keeps pushing the pixels (the 'bloom'/'melt' effect).",
                    ("MoshModeComboBox", "DUPLICATE_FRAMES"), ("MoshRepeatNumericUpDown", 10)),
                P(DA, "Heavy mosh", "Both: keyframes removed and P-frames repeated 20 times.",
                    ("MoshModeComboBox", "BOTH"), ("MoshKeepNumericUpDown", 1), ("MoshRepeatNumericUpDown", 20), ("MoshReplaceChkBox", true)),
                P(DA, "Hard cut", "Drops keyframes without replacing them: shorter video, jumpier smears (audio can drift).",
                    ("MoshModeComboBox", "REMOVE_KEYFRAMES"), ("MoshKeepNumericUpDown", 1), ("MoshReplaceChkBox", false)),

                //Source Code
                P(SC, "Config tweak", "Only numbers, nudged by up to 25%: game configs, JSON and INI files stay valid but values change.",
                    Source(("SourceIntegersChkBox", true), ("SourceFloatsChkBox", true), ("SourceNumberModeComboBox", "NUDGE"), ("SourceStrengthNumericUpDown", 25), ("SourceCommentStyleComboBox", "AUTO"))),
                P(SC, "Logic flip", "Booleans and operators: true/false, == and < get swapped, so conditions behave differently.",
                    Source(("SourceBooleansChkBox", true), ("SourceOperatorsChkBox", true), ("SourceCommentStyleComboBox", "AUTO"))),
                P(SC, "Text glitch", "Only strings, shuffled: dialogue, names and messages get scrambled.",
                    Source(("SourceStringsChkBox", true), ("SourceStringModeComboBox", "SHUFFLE"), ("SourceCommentStyleComboBox", "AUTO"))),
                P(SC, "Magic numbers", "Integers become 'interesting' values (0, -1, 255, 65535...), great for finding edge cases.",
                    Source(("SourceIntegersChkBox", true), ("SourceNumberModeComboBox", "MAGIC"), ("SourceCommentStyleComboBox", "AUTO"))),
                P(SC, "Total chaos", "Everything, including variable names and nulls, with random values.",
                    Source(("SourceBooleansChkBox", true), ("SourceIntegersChkBox", true), ("SourceFloatsChkBox", true), ("SourceStringsChkBox", true),
                        ("SourceNullsChkBox", true), ("SourceOperatorsChkBox", true), ("SourceIdentifiersChkBox", true),
                        ("SourceNumberModeComboBox", "RANDOM"), ("SourceStrengthNumericUpDown", 100), ("SourceStringModeComboBox", "RANDOM"), ("SourceCommentStyleComboBox", "AUTO"))),
            };
            return [.. all.Where(p => EngineParser.ParseEngineType(p.Engine) == engine)];
        }

    }
}
