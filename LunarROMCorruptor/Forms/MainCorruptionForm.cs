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

//Advisories:
//This program can irreversibly corrupt personal or critical system data if you're not careful.
//This program comes with no warranty of ANY kind and is provided 'AS IS'.
//You're responsible for backing up your data before use and for any damage that comes with the use or misuse of this program.
//Anti-cheat software may be triggered if corruption is used on multiplayer games.
//You may get banned if this corruptor is used in online games or games with anti-cheat software.
//Don't use this on system32 or any other system files...

using LunarROMCorruptor.EngineControls;
using LunarROMCorruptor.Modules;
using LunarROMCorruptor.Modules.CorruptionInternals;
using LunarROMCorruptor.Modules.CorruptionInternals.Engines;
using LunarROMCorruptor.Modules.Helpers;
using LunarROMCorruptor.Properties;
using System.Diagnostics;
using System.Media;

namespace LunarROMCorruptor
{
    public partial class MainCorruptionForm : Form
    {
        private readonly Random rnd = new(); //Used for random number generation
        private readonly string vernumber = $"v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}"; //"v1.0.4 - Build Number: " + 
        readonly CorruptionQueueForm CorruptionQueueFormSettings = new(); //Creates the corruption queue form and then it will be read later by the main form.
        #region Corruption Engine Frames
        public readonly NightmareEngineControl NightmareEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly LerpEngineControl LerpEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly LogicEngineControl LogicEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly MergeEngineControl MergeEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly VectorEngineControl VectorEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly ManualEngineControl ManualEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly SourceCodeEngineControl SourceCodeEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly DatamoshEngineControl DatamoshEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly BlocksEngineControl BlocksEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly PointerEngineControl PointerEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly CompressionEngineControl CompressionEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        #endregion

        private const int StashDisplayLimit = 50000; //More stash entries than this are not shown in the list (a "LargeStash" placeholder is shown instead)
        private const string MultipleFilesText = "---Multiple files selected---";
        private const string MultipleFilesSaveText = "---Corruption will be applied on the files selected---";

        private CancellationTokenSource? _corruptionCts; //Cancels the running corruption
        private bool _busy; //True while a corruption/restore is running on the background thread
        private bool _automationTickRunning;
        private bool _inPlaceWarningShown;
        private bool _closeAfterBusy; //Set when the user closes LRC while a corruption is running, the form closes once it has been cancelled/reverted
        private long _trackScale = 1; //TrackBars only go up to int.MaxValue, for larger files each trackbar step is _trackScale bytes
        private string _corruptButtonIdleText = "Corrupt File";
        private StashEditor? _stashEditor;
        private readonly Dictionary<Control, bool> _enabledBeforeBusy = [];
        private LunarROMCorruptor.Forms.BisectForm? _bisectForm;

        public MainCorruptionForm()
        {
            InitializeComponent();
            InitializePresetBar();
            InitializeStashBrowser();
            InitializeByteView();
            InitializeSession();
            MainTabControl.SelectedIndexChanged += (_, _) => { if (MainTabControl.SelectedTab == GuidesTab) GuidesView.EnsureLoaded(); };
            FormClosing += MainCorruptionForm_FormClosing;
            CorruptionCore.Exclusions.Changed += OnExclusionsChanged;
            FormClosed += (_, _) =>
            {
                TraceLogger.OnLogEntryAdded -= OnTraceLogEntryAdded;
                CorruptionCore.Exclusions.Changed -= OnExclusionsChanged;
                CorruptionCore.ResetSession();
                CorruptionCore.ResetQueueSession();
                CorruptionHistory.Clear();
            };
        }

        /// <summary>
        /// Excluded ranges changed (Bisect Mode, a stash with EXCLUDE lines or a reset): redraw them in red and update the details.
        /// </summary>
        private void OnExclusionsChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(() =>
            {
                InvalidateByteViewOverlays();
                GenerateSelectedByteDetails();
            });
        }

        private void OnTraceLogEntryAdded(string logEntry, LunarROMCorruptor.Modules.StatusSeverityType severity)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string, LunarROMCorruptor.Modules.StatusSeverityType>(OnTraceLogEntryAdded), logEntry, severity);
                return;
            }
            if (TraceLoggerTxtBox.TextLength > 2_000_000) //Keep the log box from growing forever during long automation sessions
            {
                TraceLoggerTxtBox.Select(0, TraceLoggerTxtBox.TextLength / 2);
                TraceLoggerTxtBox.SelectedText = "";
            }
            TraceLoggerTxtBox.AppendText($"{logEntry}{Environment.NewLine}");
            TraceLoggerTxtBox.SelectionStart = TraceLoggerTxtBox.Text.Length;
            TraceLoggerTxtBox.ScrollToCaret();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            TraceLogger.OnLogEntryAdded += OnTraceLogEntryAdded;
            TraceLogger.Log("LunarROMCorruptor started.");
            Text = $"{nameof(LunarROMCorruptor)} - " + vernumber;
            AboutVerLabel.Text = vernumber;
            string savesDirectory = Path.Combine(Application.StartupPath, "Saves");
            if (!Directory.Exists(savesDirectory)) //If the folder doesn't exist, assume it's the first time the program has been run.
            {
                if (MessageBox.Show($"Welcome to {nameof(LunarROMCorruptor)}!{Environment.NewLine}{Environment.NewLine}Disclaimer:{Environment.NewLine}{nameof(LunarROMCorruptor)} is distributed under an MIT license.{Environment.NewLine}{Environment.NewLine}By clicking OK, you agree to that license and also understand the risks and disclaimers provided.{Environment.NewLine}{Environment.NewLine}This program can irreversibly corrupt personal or critical system data if you're not careful.{Environment.NewLine}This program comes with no warranty of ANY kind and is provided 'AS IS'.{Environment.NewLine}You're responsible for backing up your data before use and for any damage that comes with the use or misuse of this program.{Environment.NewLine}{Environment.NewLine}This message will not show up again but you can read the license again by going to the 'About' tab.{Environment.NewLine}{Environment.NewLine}Enjoy!", $"{nameof(LunarROMCorruptor)} - INFO", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
                {
                    TraceLogger.Log("User did not accept the disclaimer. Closing.");
                    Close();
                    return;
                }
            }
            try
            {
                Directory.CreateDirectory(savesDirectory);
                Directory.CreateDirectory(IOManager.CorruptionStashDirectory);
                Directory.CreateDirectory(EnginePresets.UserPresetDirectory);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error trying to create directory folders. Your anti-virus or ransomware protection may be enabled and is blocking LRC from creating these directories. Please make an exception for LRC in your antivirus/ransomware protection software since LRC requires permission to edit files. Error: {ex.Message}", StatusSeverityType.Error, true);
            }
            CorruptionHistory.HistoryDirectory = Path.Combine(Application.StartupPath, "CorruptionHistory");
            CorruptionHistory.Clear(); //History is per session
            CorruptionHistoryBtn.Click += CorruptionHistoryBtn_Click;
            CorruptionEngineComboBox.Text = EngineParser.GetEngineDisplayName(CorruptionEngineType.NightmareEngine);
            AllowDrop = true;
            BrowseEmulatorbutton.Enabled = false;
            EmulatorLocationtxt.BackColor = Color.Gray;
            StashBrowser.Reload();
            IOManager.RefreshFileSaves(FilesaveList);
            LoadSettings();
            LoadSettingsTab();
            GenerateSelectedByteDetails();
            InitFormDesignFixes();
            ByteViewModeComboBox.Text = "Regions";
            AttentionPictureBox.BringToFront();
            RestoreSessionLayout();
            SessionHistoryView.RefreshList();
        }

        private void InitFormDesignFixes()
        {
            //This code under this comment fixes a .net bug where the trackbar allocates a huge amount of memory if the trackbar maximum value is set to a large amount.
            EndByteTrackbar.TickStyle = TickStyle.None;
            StartByteTrackBar.TickStyle = TickStyle.None;
            CorrupteverynthbyteTrackbar.TickStyle = TickStyle.None;
            IntensityTrackbar.TickStyle = TickStyle.None;
            //HACK: For some bizzare reason VS Designer is resetting the sizes of these elements so they go off-screen. This code attempts to keep them in line...
            AutomationList.Size = new Size(598, 408);
            ByteViewTab.Size = new Size(192, 72);
            ByteViewContainer.Size = new Size(192, 72);
            ByteViewContainer.SplitterDistance = 120;
            LicenseRTB.Size = new Size(611, 373);
        }

        public void LoadSettings()
        {
            // Read & check if the emulator path is valid
            if (!string.IsNullOrEmpty(Settings.Default.EmulatorPath) && File.Exists(Settings.Default.EmulatorPath))
            {
                EmulatorLocationtxt.Text = Settings.Default.EmulatorPath;
            }
            // Read and set checkbox states
            SilentCorruptionchbox.Checked = Settings.Default.SilentCorruption;
            EnableStashSavesChkbox.Checked = Settings.Default.StashSavesEnabled;
            UseHexchbox.Checked = Settings.Default.UseHex;
            Runemulatorchbox.Checked = Settings.Default.RunEmulator;
            ReopenChbox.Checked = Settings.Default.ReopenEmulator;
            FilesaveEnableAutoSaves.Checked = Settings.Default.AutoFileSaveEnabled;
            byteViewColourChkbox.Checked = Settings.Default.ByteViewColourMode;
            ByteViewupdateWhenCorruptedChkBox.Checked = Settings.Default.ByteViewUpdateOnCorruption;
            AttemptProtectedFileOverrideChkBox.Checked = Settings.Default.AttemptProtectedFileOverride;
            FixChecksumsChkBox.Checked = Settings.Default.FixChecksums;
            CorruptionCore.FixChecksums = FixChecksumsChkBox.Checked;
            CorruptionCore.AttemptOverrideOnWrite = AttemptProtectedFileOverrideChkBox.Checked;
        }

        #region File loading and byte range

        public async Task LoadFile(string FileLocation)
        {
            if (_busy)
            {
                TraceLogger.Log("A corruption is still running. Wait for it to finish (or cancel it) before loading another file.", StatusSeverityType.Warning, true);
                return;
            }
            if (CorruptionCore.HasUnrestoredInPlaceChanges)
            {
                var answer = MessageBox.Show($"{CorruptionCore.SourcePath} is currently corrupted in place.\n\nRestore it to the original before loading the new file? (The undo data is lost once a new file is loaded.)", $"{nameof(LunarROMCorruptor)} - Restore?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (answer == DialogResult.Cancel) return;
                if (answer == DialogResult.Yes) CorruptionCore.RestoreROM(CorruptionCore.SourcePath!);
            }
            TraceLogger.Log($"Attempting to load file: {FileLocation}");
            AttentionPictureBox.Image = Resources.Loading;
            AttentionPictureBox.Show();
            Refresh();
            if (CorruptionCore.LoadROM(FileLocation, AttemptProtectedFileOverrideChkBox.Checked))
            {
                TraceLogger.Log($"File loaded in to corruptioncore: {FileLocation}");
                SetByteRange(CorruptionCore.MaxByte);
                _lastCorruption = null;
                StartSourceInfo(CorruptionCore.SourcePath!);
                FileSelectiontxt.Text = CorruptionCore.SourcePath;
                string directory = Path.GetDirectoryName(CorruptionCore.SourcePath!) ?? "";
                SaveasTxt.Text = Path.Combine(directory, "CorruptedFile" + Path.GetExtension(CorruptionCore.SourcePath));
                MainSaveFileDialog.InitialDirectory = directory;
                _corruptButtonIdleText = $"Corrupt File ({CorruptionCore.rawFileSize} {CorruptionCore.fileSizeUnit})";
                CorruptButton.Text = _corruptButtonIdleText;
                _inPlaceWarningShown = false;
                ClearByteView();
                if (NightmareEngineFrame.Visible) NightmareEngineFrame.LoadAddresses();
                GenerateSelectedByteDetails();
                UpdateByteViewIfEnabled();
                AddRecentFile(CorruptionCore.SourcePath!);
                StashBrowser.LoadedFile = CorruptionCore.SourcePath;
                UpdateFileStatus();
                UpdateStateStatus("Not corrupted yet");
                StatusLastLbl.Text = "No corruption yet";
                SetStatus($"Loaded {Path.GetFileName(CorruptionCore.SourcePath)}.");
                TraceLogger.Log($"File loaded successfully and UI updated for file: {FileLocation}");
            }
            else
            {
                TraceLogger.Log($"Failed to load file: {FileLocation}", StatusSeverityType.Error);
                SetStatus($"Couldn't load {Path.GetFileName(FileLocation)} (see the Logs tab).", StatusKind.Warning);
            }
            AttentionPictureBox.Hide();
            AttentionPictureBox.Image = Resources.dragicon;
        }

        /// <summary>
        /// Opens a guide on the 📖 Guides tab (e.g. "docs/Corruption-Engines.md", "vector2-engine").
        /// </summary>
        public void ShowGuide(string repoPath, string? anchor = null)
        {
            MainTabControl.SelectedTab = GuidesTab;
            GuidesView.Open(repoPath, anchor);
        }

        private static string FormatByteCount(long bytes) => CorruptionCore.FormatSize(bytes, out _, out _);

        private void GenerateSelectedByteDetails()
        {
            //Generate details of the bytes selected in
            //ByteSelectionDetailsLbl.Text
            //"Start: 0x00000000 (0) - End: 0x00000000 (0) - Length: 0 bytes (5.00 KB)"
            //Similar to ByteViewInfoLbl.Text = $"0x{a:X} - 0x{Math.Min(b, view.FileLength - 1):X}\n({FormatByteCount(Math.Min(b, view.FileLength - 1) - a + 1)})";
            //But with FormatByteCount, that uses long instead of decimal, so use a long for the length calculation
            try
            {
                int NumberOfBytesToCorrupt = 0;
                if (CorruptEverynthByteRadioBtn.Checked)
                {
                    if (EveryNthByte.Value > 0)
                    {
                        NumberOfBytesToCorrupt = (int)((EndByteNumb.Value - StartByteNumb.Value + 1) / EveryNthByte.Value);
                    }
                }
                else
                {
                    NumberOfBytesToCorrupt = (int)Intensity.Value;
                }
                ByteSelectionDetailsLbl.Text = $"Start: 0x{(long)StartByteNumb.Value:X} ({StartByteNumb.Value})\n" +
                    $"End: 0x{(long)EndByteNumb.Value:X} ({EndByteNumb.Value})\n" +
                    $"Length: {FormatByteCount((long)EndByteNumb.Value - (long)StartByteNumb.Value + 1)}\n" +
                    $"Corrupted Bytes in Stash List Count: {CorruptionCore.Stash.Count}\n" +
                    $"Number of Bytes to Corrupt: {NumberOfBytesToCorrupt}{(CorruptionCore.Exclusions.Count > 0 ? $"\n" +
                    $"Excluded Ranges: {CorruptionCore.Exclusions.Count} ({FormatByteCount(CorruptionCore.Exclusions.TotalBytes)})" : "")}";
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error generating byte selection details. {ex.Message}", StatusSeverityType.Error);
            }
        }

        /// <summary>
        /// Sets the start/end byte controls for a file. TrackBars are limited to int.MaxValue, so for files larger than 2 GB the trackbars
        /// move in steps of _trackScale bytes, while the number boxes stay exact.
        /// </summary>
        private void SetByteRange(long maxByte)
        {
            maxByte = Math.Max(0, maxByte);
            _trackScale = maxByte > int.MaxValue ? maxByte / int.MaxValue + 1 : 1;
            int trackMax = (int)(maxByte / _trackScale);
            StartByteTrackBar.Value = 0;
            EndByteTrackbar.Value = 0;
            StartByteTrackBar.Maximum = trackMax;
            EndByteTrackbar.Maximum = trackMax;
            StartByteNumb.Value = 0;
            EndByteNumb.Value = 0;
            StartByteNumb.Maximum = maxByte;
            EndByteNumb.Maximum = maxByte;
            EndByteNumb.Value = maxByte;
            StartByteNumb.Value = 0;
            EndByteTrackbar.Value = trackMax;
            if (_trackScale > 1)
                TraceLogger.Log($"File is larger than 2 GB: the byte trackbars move in steps of {_trackScale} bytes, use the number boxes for exact values.");
        }

        private int ToTrackValue(decimal value, TrackBar bar) => (int)Math.Clamp((long)value / _trackScale, bar.Minimum, bar.Maximum);

        private decimal FromTrackValue(TrackBar bar, NumericUpDown box)
        {
            if (bar.Value >= bar.Maximum) return box.Maximum;
            return Math.Clamp((decimal)bar.Value * _trackScale, box.Minimum, box.Maximum);
        }

        private void Openfilebtn_Click(object sender, EventArgs e)
        {
            if (CorruptionQueueChkbox.Checked)
            {
                CorruptionQueueFormSettings.ShowDialog();
                UpdateFileStatus();
                return;
            }
            //Main Function - If the user didn't cancel, load file.
            if (MainOpenFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                LoadFile(MainOpenFileDialog.FileName).GetAwaiter();
            }
        }

        private void StartByteTrackBar_Scroll(object sender, EventArgs e)
        {
            StartByteNumb.Value = FromTrackValue(StartByteTrackBar, StartByteNumb);
            //If startbyte is greater than endbyte, set endbyte to startbyte
            if (StartByteNumb.Value > EndByteNumb.Value)
            {
                EndByteNumb.Value = StartByteNumb.Value;
                EndByteTrackbar.Value = ToTrackValue(EndByteNumb.Value, EndByteTrackbar);
            }
            GenerateSelectedByteDetails();
        }

        private void EndByteTrackbar_Scroll(object sender, EventArgs e)
        {
            EndByteNumb.Value = FromTrackValue(EndByteTrackbar, EndByteNumb);
            //If endbyte is less than startbyte, set startbyte to endbyte
            if (EndByteNumb.Value < StartByteNumb.Value)
            {
                StartByteNumb.Value = EndByteNumb.Value;
                StartByteTrackBar.Value = ToTrackValue(StartByteNumb.Value, StartByteTrackBar);
            }
            GenerateSelectedByteDetails();
        }

        private void StartByteNumb_ValueChanged(object sender, EventArgs e)
        {
            StartByteTrackBar.Value = ToTrackValue(StartByteNumb.Value, StartByteTrackBar);
            //If startbyte is greater than endbyte, set endbyte to startbyte
            if (StartByteNumb.Value > EndByteNumb.Value)
            {
                EndByteNumb.Value = StartByteNumb.Value;
                EndByteTrackbar.Value = ToTrackValue(EndByteNumb.Value, EndByteTrackbar);
            }
            InvalidateByteViewOverlays(); //Redraw the range overlay
            GenerateSelectedByteDetails();
        }

        private void EndByteNumb_ValueChanged(object sender, EventArgs e)
        {
            EndByteTrackbar.Value = ToTrackValue(EndByteNumb.Value, EndByteTrackbar);
            //If endbyte is less than startbyte, set startbyte to endbyte
            if (EndByteNumb.Value < StartByteNumb.Value)
            {
                StartByteNumb.Value = EndByteNumb.Value;
                StartByteTrackBar.Value = ToTrackValue(StartByteNumb.Value, StartByteTrackBar);
            }
            InvalidateByteViewOverlays();
            GenerateSelectedByteDetails();
        }

        private void CorrupteverynthbyteTrackbar_Scroll(object sender, EventArgs e)
        {
            EveryNthByte.Value = Math.Clamp(CorrupteverynthbyteTrackbar.Value, EveryNthByte.Minimum, EveryNthByte.Maximum);
            GenerateSelectedByteDetails();
        }

        private void IntensityTrackbar_Scroll(object sender, EventArgs e)
        {
            Intensity.Value = Math.Clamp(IntensityTrackbar.Value, Intensity.Minimum, Intensity.Maximum);
            GenerateSelectedByteDetails();
        }

        private void Intensity_ValueChanged(object sender, EventArgs e)
        {
            IntensityTrackbar.Value = (int)Math.Clamp(Intensity.Value, IntensityTrackbar.Minimum, IntensityTrackbar.Maximum);
            GenerateSelectedByteDetails();
        }

        private void EveryNthByte_ValueChanged(object sender, EventArgs e)
        {
            CorrupteverynthbyteTrackbar.Value = (int)Math.Clamp(EveryNthByte.Value, CorrupteverynthbyteTrackbar.Minimum, CorrupteverynthbyteTrackbar.Maximum);
            GenerateSelectedByteDetails();
        }

        private void AllowLargeIntensity_CheckedChanged(object sender, EventArgs e)
        {
            int maxIntensity = AllowLargeIntensity.Checked ? 10_000_000 : 1000; //This changes the value based on the checked state of the checkbox
            if (Intensity.Value > maxIntensity) Intensity.Value = maxIntensity;
            if (EveryNthByte.Value > maxIntensity) EveryNthByte.Value = maxIntensity;
            Intensity.Maximum = maxIntensity;
            IntensityTrackbar.Maximum = maxIntensity;
            CorrupteverynthbyteTrackbar.Maximum = maxIntensity;
            EveryNthByte.Maximum = maxIntensity;
        }

        private void UseHexchbox_CheckedChanged(object sender, EventArgs e)
        {
            StartByteNumb.Hexadecimal = UseHexchbox.Checked;
            EndByteNumb.Hexadecimal = UseHexchbox.Checked;
            //Save changes to settings
            Settings.Default.UseHex = UseHexchbox.Checked;
            Settings.Default.Save();
        }

        private void Changesaveasbtn_Click(object sender, EventArgs e)
        {
            string exc1 = Path.GetExtension(FileSelectiontxt.Text);
            MainSaveFileDialog.FileName = "CorruptedFile" + exc1;
            if (MainSaveFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                SaveasTxt.Text = MainSaveFileDialog.FileName;
                _inPlaceWarningShown = false;
            }
        }

        #endregion

        #region Engine selection

        private void CorruptionEngineComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedEngine = EngineParser.ParseEngineType(CorruptionEngineComboBox.Text);
            foreach (Control control in CorruptionEngineTab.Controls)
            {
                if (control is UserControl && control.Name.EndsWith("EngineControl"))
                {
                    control.Visible = false; //DO NOT dispose of the user controls. Hide them since the data in those controls are used throughout the corruption process and disposing them will cause issues.
                }
            }
            UserControl? newControl = GetEngineFrame(selectedEngine);
            newControl?.Name = newControl.GetType().Name; //The engine tab finds the engine controls by their "...EngineControl" name
            if (selectedEngine == CorruptionEngineType.NightmareEngine)
                NightmareEngineFrame.LoadAddresses(); //The loaded file may have changed since the exclusion type was last shown

            if (newControl != null)
            {
                newControl.Dock = DockStyle.Fill;
                newControl.Visible = true;
                if (!CorruptionEngineTab.Controls.Contains(newControl))
                {
                    CorruptionEngineTab.Controls.Add(newControl);
                }
                newControl.BringToFront(); //Docked last, so it fills the space under the preset bar
            }
            PresetBar.ShowEngine(selectedEngine);
        }

        /// <summary>
        /// The settings control of an engine (null for INVALID).
        /// </summary>
        private UserControl? GetEngineFrame(CorruptionEngineType engine) => engine switch
        {
            CorruptionEngineType.MergeEngine => MergeEngineFrame,
            CorruptionEngineType.LogicEngine => LogicEngineFrame,
            CorruptionEngineType.NightmareEngine => NightmareEngineFrame,
            CorruptionEngineType.LerpEngine => LerpEngineFrame,
            CorruptionEngineType.VectorEngine => VectorEngineFrame,
            CorruptionEngineType.ManualEngine => ManualEngineFrame,
            CorruptionEngineType.DatamoshEngine => DatamoshEngineFrame,
            CorruptionEngineType.BlocksEngine => BlocksEngineFrame,
            CorruptionEngineType.PointerEngine => PointerEngineFrame,
            CorruptionEngineType.CompressionEngine => CompressionEngineFrame,
            CorruptionEngineType.SourceCodeEngine => SourceCodeEngineFrame,
            _ => null
        };

        #endregion

        #region Engine presets

        private CorruptionEngineType SelectedEngine => EngineParser.ParseEngineType(CorruptionEngineComboBox.Text);

        private void InitializePresetBar()
        {
            PresetBar.EngineFrameProvider = GetEngineFrame;
            PresetBar.EngineSwitchRequested += engine => CorruptionEngineComboBox.Text = EngineParser.GetEngineDisplayName(engine);
        }

        /// <summary>
        /// Copies a shared preset into the user preset folder, switches to its engine and loads it.
        /// </summary>
        public void ImportPresetFile(string file) => PresetBar.ImportPresetFile(file);

        #endregion

        #region Corruption settings

        /// <summary>
        /// Takes a snapshot of every setting on the UI thread so the corruption can safely run on a background thread.
        /// Returns null (and tells the user why) if a setting is invalid.
        /// </summary>
        private CorruptionSettings? BuildSettings(bool queueMode)
        {
            if (StartByteNumb.Value > EndByteNumb.Value)
            {
                TraceLogger.Log("Start Byte value is greater than End Byte value. Aborting corruption process.", StatusSeverityType.Error, true);
                return null;
            }
            var engine = EngineParser.ParseEngineType(CorruptionEngineComboBox.Text);
            if (engine == CorruptionEngineType.INVALID)
            {
                TraceLogger.Log($"Invalid corruption engine selected: '{CorruptionEngineComboBox.Text}'. Aborting corruption.", StatusSeverityType.Error, true);
                return null;
            }

            var settings = new CorruptionSettings
            {
                Engine = engine,
                StartByte = (long)StartByteNumb.Value,
                //In queue mode every file has a different size, "end of file" is kept as "end of each file"
                EndByte = queueMode && EndByteNumb.Value == EndByteNumb.Maximum ? long.MaxValue : (long)EndByteNumb.Value,
                EveryNth = CorruptEverynthByteRadioBtn.Checked,
                Intensity = CorruptEverynthByteRadioBtn.Checked ? (long)EveryNthByte.Value : (long)Intensity.Value,
                //Excluded ranges belong to the loaded file, so they aren't used for the corruption queue
                Excluded = queueMode ? null : CorruptionCore.Exclusions.Snapshot()
            };
            try
            {
                switch (engine)
                {
                    case CorruptionEngineType.NightmareEngine: settings.Nightmare = NightmareEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.LerpEngine: settings.Lerp = LerpEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.LogicEngine: settings.Logic = LogicEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.MergeEngine: settings.Merge = MergeEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.ManualEngine: settings.Manual = ManualEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.VectorEngine: settings.Vector = VectorEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.SourceCodeEngine: settings.SourceCode = SourceCodeEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.BlocksEngine: settings.Blocks = BlocksEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.DatamoshEngine: settings.Datamosh = DatamoshEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.PointerEngine: settings.Pointer = PointerEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.CompressionEngine: settings.Compression = CompressionEngineFrame.GetSettings(); break;
                }
            }
            catch (InvalidOperationException ex)
            {
                TraceLogger.Log(ex.Message, StatusSeverityType.Error, true);
                return null;
            }
            return settings;
        }

        #endregion

        #region Corruption (runs on a background thread)

        private async void CorruptButton_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                //While a corruption is running the corrupt button is the cancel button
                if (_corruptionCts != null && !_corruptionCts.IsCancellationRequested)
                {
                    TraceLogger.Log("Cancel requested by the user.");
                    _corruptionCts.Cancel();
                    CorruptButton.Text = "Cancelling...";
                }
                return;
            }
            await RunCorruptionAsync();
        }

        /// <summary>
        /// Starts a corruption (single file or queue). Returns the result, or null if nothing was started.
        /// </summary>
        public async Task<CorruptionResult?> RunCorruptionAsync()
        {
            if (_busy) return null;
            TraceLogger.Log("Corruption process started by user.");
            CorruptionResult? result = CorruptionQueueChkbox.Checked ? await ProcessMultiFileCorruptionAsync() : await ProcessSingleFileCorruptionAsync();
            TraceLogger.Log($"Corruption process completed. Result: {(result?.ToString() ?? "not started")}. Stash Item Count: {CorruptionCore.Stash.Count}");
            GenerateSelectedByteDetails();
            return result;
        }

        private void BeginBusy(string status)
        {
            _busy = true;
            _corruptionCts?.Dispose();
            _corruptionCts = new CancellationTokenSource();
            _enabledBeforeBusy.Clear();
            foreach (Control control in new Control[] { Openfilebtn, Restorefilebtn, Changesaveasbtn, Corruptusingstashbtn, CorruptFileAndStashBtn, StashEditorbtn, TransferStash, CopySaveToCorruptedFileBtn, FilesaveSavebtn, CorruptionQueueChkbox, ByteViewRefreshBtn, CorruptionEngineComboBox })
            {
                _enabledBeforeBusy[control] = control.Enabled;
                control.Enabled = false;
            }
            AllowDrop = false;
            SessionHistoryView.Busy = true;
            CorruptionProgressBar.Value = 0;
            CorruptionProgressBar.Visible = true;
            CorruptButton.Text = $"{status} - click to cancel";
            UseWaitCursor = true;
            CorruptButton.UseWaitCursor = false;
        }

        private void EndBusy()
        {
            foreach (var (control, enabled) in _enabledBeforeBusy)
                control.Enabled = enabled;
            _enabledBeforeBusy.Clear();
            AllowDrop = true;
            SessionHistoryView.Busy = false;
            CorruptionProgressBar.Visible = false;
            CorruptButton.Text = CorruptionQueueChkbox.Checked ? "Corrupt Files" : _corruptButtonIdleText;
            UseWaitCursor = false;
            _busy = false;
            if (_closeAfterBusy)
                BeginInvoke(Close);
        }

        private Progress<double> CreateProgress(string status, int fileIndex = 0, int fileCount = 1)
        {
            return new Progress<double>(p =>
            {
                if (!_busy) return;
                double overall = (fileIndex + Math.Clamp(p, 0, 1)) / Math.Max(1, fileCount);
                CorruptionProgressBar.Value = (int)Math.Clamp(overall * CorruptionProgressBar.Maximum, 0, CorruptionProgressBar.Maximum);
                if (_corruptionCts != null && !_corruptionCts.IsCancellationRequested)
                    CorruptButton.Text = $"{status} {overall:P0} - click to cancel";
            });
        }

        private void StartSuccessFeedback()
        {
            CorruptButton.BackColor = Color.Green;
            if (!SilentCorruptionchbox.Checked)
            {
                using var soundPlayer = new SoundPlayer(Resources.success3);
                soundPlayer.Play();
            }

            CorruptButtonColorChanger.Start(); //Change the corruption button colour back to normal after the timer ticks.
        }

        private void StartFailureFeedback()
        {
            CorruptButton.BackColor = Color.DarkRed;
            CorruptButtonColorChanger.Start();
        }

        private async Task<CorruptionResult?> ProcessMultiFileCorruptionAsync()
        {
            TraceLogger.Log("Corruption Queue enabled. Starting multi-file corruption process.");

            if (!ValidateMultiFileCorruption())
            {
                return null;
            }

            var settings = BuildSettings(queueMode: true);
            if (settings == null) return null;
            var selectedFiles = GetSelectedFilesFromQueue().ToList();
            bool attemptOverride = AttemptProtectedFileOverrideChkBox.Checked;

            // Clear stash list (stashes aren't recorded in queue mode)
            StashBytesList.Items.Clear();
            CorruptionCore.Stash.Clear();

            BeginBusy("Corrupting queue");
            var timer = Stopwatch.StartNew();
            var token = _corruptionCts!.Token;
            int succeeded = 0, failed = 0;
            CorruptionResult overall = CorruptionResult.Success;
            try
            {
                for (int k = 0; k < selectedFiles.Count; k++)
                {
                    string filePath = selectedFiles[k];
                    var progress = CreateProgress($"File {k + 1}/{selectedFiles.Count}", k, selectedFiles.Count);
                    var result = await Task.Run(() => CorruptionCore.CorruptFileInPlace(settings, filePath, attemptOverride, token, progress));
                    if (result == CorruptionResult.Success) succeeded++;
                    else if (result == CorruptionResult.Failed) failed++;
                    else
                    {
                        overall = CorruptionResult.Cancelled;
                        break;
                    }
                }
            }
            finally
            {
                EndBusy();
            }
            TraceLogger.Log($"Queue corruption finished (each file was restored to its original first): {succeeded} file(s) corrupted, {failed} failed{(overall == CorruptionResult.Cancelled ? ", cancelled by the user" : "")}.", failed > 0 ? StatusSeverityType.Warning : StatusSeverityType.Information, failed > 0);
            ResetMultiFileUI();
            if (overall == CorruptionResult.Cancelled) return overall;
            if (succeeded == 0)
            {
                StartFailureFeedback();
                return CorruptionResult.Failed;
            }
            ReportCorruption("Multi-File corruption", $"{succeeded:N0} file(s){(failed > 0 ? $", {failed:N0} failed" : "")}", timer.Elapsed);
            UpdateStateStatus($"Queue: {succeeded:N0} file(s) corrupted", "Restore File (Ctrl+R) puts every queue file back.");
            //Run the file picked in the queue window, or the first file of the queue
            string runFile = !string.IsNullOrWhiteSpace(CorruptionQueueFormSettings.SelectedRunFileTXT.Text) ? CorruptionQueueFormSettings.SelectedRunFileTXT.Text : selectedFiles[0];
            RunEmulatorIfEnabled(runFile);
            StartSuccessFeedback();
            return CorruptionResult.Success;
        }

        private bool ValidateMultiFileCorruption()
        {
            // Validate queue is not empty and handle multiple files selection
            if (CorruptionQueueFormSettings.CorruptionQueueList.Items.Count == 0)
            {
                TraceLogger.Log("Corruption Queue is empty. Aborting corruption process.", StatusSeverityType.Error, true);
                return false;
            }

            if (FileSelectiontxt.Text != MultipleFilesText)
            {
                TraceLogger.Log("Corruption Queue has files but the main form is still showing a single file selected. This likely means the user hasn't sent the files to the corruptor yet. Aborting corruption process.", StatusSeverityType.Error, true);
                return false;
            }

            if (CorruptionQueueFormSettings.CorruptionQueueList.Items.Count >= 100)
            {
                TraceLogger.Log("Corruption Queue has a large number of files (100 or more). This may lead to long corruption times and potential performance issues.", StatusSeverityType.Warning);
                MessageBox.Show("The corruption queue is rather full of files.\n\nBe aware that this will impact corruption times.", "Full Queue", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Validate each file in the queue exists
            foreach (var item in GetSelectedFilesFromQueue())
            {
                if (!File.Exists(item))
                {
                    TraceLogger.Log($"File in corruption queue does not exist: {item}. Aborting corruption process.", StatusSeverityType.Error, true);
                    return false;
                }
            }

            return true;
        }

        private IEnumerable<string> GetSelectedFilesFromQueue()
        {
            //The queue is a ListBox of file path strings (the old code cast them to ListViewItem, which threw an InvalidCastException)
            return CorruptionQueueFormSettings.CorruptionQueueList.Items.Cast<object>().Select(i => i?.ToString() ?? "").Where(i => i.Length > 0);
        }

        private void ResetMultiFileUI()
        {
            FileSelectiontxt.Text = MultipleFilesText;
            SaveasTxt.Text = MultipleFilesSaveText;
        }

        private void RunEmulatorIfEnabled(string fileToRun)
        {
            if (Runemulatorchbox.Checked)
            {
                RunEmulatorAfterCheck(fileToRun);
            }
        }

        private void RunEmulatorAfterCheck(string fileToRun)
        {
            if (File.Exists(EmulatorLocationtxt.Text))
            {
                CommandRunner.StartEmulator(
                    ReopenChbox.Checked,
                    EmulatorLocationtxt.Text,
                    fileToRun,
                    OverrideArgumentschbox.Checked,
                    OverrideArguments.Text
                );
            }
            else
            {
                TraceLogger.Log("Emulator path is set to run after corruption but the file does not exist. Please check your emulator path settings. Aborting emulator launch.", StatusSeverityType.Error, true);
            }
        }

        private bool ValidateSingleFileSelection(bool skipIsInPlaceCheck = false, bool isDoingFolderCheck = false)
        {
            if (string.IsNullOrEmpty(FileSelectiontxt.Text) || FileSelectiontxt.Text == "No file selected." || FileSelectiontxt.Text == MultipleFilesText || !CorruptionCore.IsLoaded || !File.Exists(CorruptionCore.SourcePath))
            {
                if (isDoingFolderCheck)
                {
                    TraceLogger.Log("No valid path was detected. Please check your folder selection.", StatusSeverityType.Error, true);
                }
                else
                {
                    TraceLogger.Log("No valid file selected for corruption. Please select a valid file before attempting to corrupt.", StatusSeverityType.Error, true);
                }
                return false;
            }
            if (string.IsNullOrWhiteSpace(SaveasTxt.Text) || SaveasTxt.Text == MultipleFilesSaveText)
            {
                TraceLogger.Log("No valid save location set. Please choose where the corrupted file should be saved.", StatusSeverityType.Error, true);
                return false;
            }
            if (CorruptionCore.IsInPlace(SaveasTxt.Text) && !_inPlaceWarningShown && !skipIsInPlaceCheck)
            {
                if (MessageBox.Show("The save location is the original file, so the original will be corrupted directly.\n\nRestore can undo this while LRC stays open (the original bytes are kept in a temporary undo journal), but it's still recommended to save to a different file.\n\nContinue?",
                    $"{nameof(LunarROMCorruptor)} - Corrupting the original file", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    return false;
                }
                _inPlaceWarningShown = true;
            }
            return true;
        }

        private async Task<CorruptionResult?> ProcessSingleFileCorruptionAsync()
        {
            if (!ValidateSingleFileSelection()) return null;
            var settings = BuildSettings(queueMode: false);
            if (settings == null) return null;

            string outputPath = SaveasTxt.Text;
            string sourcePath = CorruptionCore.SourcePath!;
            string engineName = EngineParser.GetEngineDisplayName(settings.Engine);
            bool stashEnabled = EnableStashSavesChkbox.Checked; //Controls are only read on the UI thread
            var description = DescribeCorruption(settings, outputPath);
            StashBytesList.Items.Clear();

            BeginBusy("Corrupting");
            var timer = Stopwatch.StartNew();
            CorruptionResult result;
            try
            {
                var token = _corruptionCts!.Token;
                var progress = CreateProgress("Corrupting");
                result = await Task.Run(() =>
                {
                    //The stash is always recorded so the corruption history works, the stash checkbox only controls the stash list/saving
                    var r = CorruptionCore.StartCorruption(settings, outputPath, stashEnabled, token, progress);
                    if (r == CorruptionResult.Success)
                        CorruptionHistory.Add(CorruptionCore.Stash, engineName, sourcePath, stashEnabled, description);
                    return r;
                });
            }
            finally
            {
                EndBusy();
            }

            if (result != CorruptionResult.Success)
            {
                if (result == CorruptionResult.Failed) StartFailureFeedback();
                SetStatus(result == CorruptionResult.Cancelled ? "Corruption cancelled." : "Corruption failed (see the Logs tab).", StatusKind.Warning);
                return result;
            }
            _lastCorruption = description;
            ReportCorruption(engineName, $"{CorruptionCore.Stash.Count:N0} change(s)", timer.Elapsed);
            ShowCorruptedState(outputPath);

            if (FilesaveEnableAutoSaves.Checked) //Save the corrupted file copy into the file saves if auto saves is enabled.
            {
                string? saved = await Task.Run(() => IOManager.CopyToSaves(outputPath)); //Can be a large file now, so it's copied in the background
                if (saved != null) FilesaveList.Items.Add(saved);
            }

            RunEmulatorIfEnabled(outputPath);
            await HandleStashRenderingAsync();
            UpdateByteViewIfEnabled();
            StartSuccessFeedback();
            return result;
        }

        private async Task HandleStashRenderingAsync()
        {
            StashBytesList.Items.Clear();
            if (!EnableStashSavesChkbox.Checked) return;
            var stash = CorruptionCore.Stash;
            if (stash.Count > StashDisplayLimit)
            {
                StashBytesList.Items.Add("LargeStash");
                return;
            }
            //Format the entries off the UI thread, then add them in one go
            var objects = await Task.Run(() => stash.FormatLines(StashDisplayLimit).Cast<object>().ToArray());
            StashBytesList.BeginUpdate();
            StashBytesList.Items.AddRange(objects);
            StashBytesList.EndUpdate();
        }

        private void CorruptButtonColorChanger_Tick(object sender, EventArgs e)
        {
            CorruptButton.BackColor = Color.FromArgb(255, 29, 32, 32);
            CorruptButtonColorChanger.Stop(); //On tick, change the corrupt button colour back to normal and stop the timer.
        }

        #endregion

        #region Drag and drop

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            AttentionPictureBox.Hide();
            if (_busy) return;
            if (e.Data?.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0)
                return;

            if (files.Length > 1 || Directory.Exists(files[0]))
            {
                CorruptionQueueChkbox.Checked = true;
                foreach (var item in files)
                {
                    TraceLogger.Log($"Added to corruption queue via drag and drop: {item}");
                    CorruptionQueueFormSettings.AddFiles(Directory.Exists(item) ? Directory.GetFiles(item) : [item]);
                }
                CorruptionQueueFormSettings.TopMost = true;
                CorruptionQueueFormSettings.ShowDialog();
                CorruptionQueueFormSettings.TopMost = false;
                UpdateFileStatus();
            }
            else
            {
                string path = files[0];
                if (Path.GetExtension(path).Equals(StashLibrary.ExportExtension, StringComparison.OrdinalIgnoreCase) ||
                    Path.GetExtension(path).Equals(".stash", StringComparison.OrdinalIgnoreCase))
                {
                    StashBrowser.ImportFiles([path], StashBrowser.SelectedGroup); //A shared stash goes into the stash list
                    return;
                }
                if (Path.GetExtension(path).Equals(EnginePresets.Extension, StringComparison.OrdinalIgnoreCase))
                {
                    ImportPresetFile(path); //A shared engine preset
                    return;
                }
                if (Path.GetExtension(path).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    var result = MessageBox.Show(
                        "This file is an executable. Do you want to use it as an emulator?",
                        "Executable File Detected",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        EmulatorLocationtxt.Text = path;
                        Settings.Default.EmulatorPath = path;
                        Settings.Default.Save();
                        return;
                    }
                }

                if (CorruptionQueueChkbox.Checked)
                    CorruptionQueueChkbox.Checked = false;
                TraceLogger.Log($"File dropped onto main form: {path}. Attempting to load this file.");
                LoadFile(path).GetAwaiter();
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (_busy || e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return;
            AttentionPictureBox.Show();
            e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragLeave(object sender, EventArgs e)
        {
            AttentionPictureBox.Hide();
        }

        #endregion

        #region Restore, stash and file saves

        private async void Restorefilebtn_Click(object sender, EventArgs e)
        {
            if (_busy) return;
            if (CorruptionQueueChkbox.Checked)
            {
                //Queue mode: every queue file that LRC corrupted is put back to its original
                if (!CorruptionCore.HasUnrestoredQueueChanges)
                {
                    SetStatus("None of the queue files are corrupted by LRC, there is nothing to restore.", StatusKind.Warning);
                    return;
                }
                BeginBusy("Restoring queue");
                int count;
                try
                {
                    count = await Task.Run(CorruptionCore.RestoreQueueFiles);
                }
                finally
                {
                    EndBusy();
                }
                SetStatus($"{count} queue file(s) restored to their originals.", StatusKind.Success);
                UpdateStateStatus("Queue restored");
                return;
            }
            if (!CorruptionCore.IsLoaded)
            {
                TraceLogger.Log("Attempted to restore but no file is loaded. Please load a file before attempting to restore.", StatusSeverityType.Error, true);
                return;
            }
            string target = SaveasTxt.Text;
            BeginBusy("Restoring");
            bool restored;
            try
            {
                restored = await Task.Run(() => CorruptionCore.RestoreROM(target));
            }
            finally
            {
                EndBusy();
            }
            if (restored)
            {
                SetStatus($"{Path.GetFileName(target)} restored to the original.", StatusKind.Success);
                UpdateStateStatus($"Restored → {Path.GetFileName(target)}", target);
                UpdateByteViewIfEnabled();
            }
            else
            {
                SetStatus("Nothing was restored (see the Logs tab).", StatusKind.Warning);
            }
        }

        private void CopySaveToCorruptedFileBtn_Click(object sender, EventArgs e)
        {
            if (IOManager.CopySaveToCorruptedFile(SaveasTxt.Text, FilesaveList))
            {
                SetStatus($"Copied {FilesaveList.GetItemText(FilesaveList.SelectedItem)} to {Path.GetFileName(SaveasTxt.Text)}.", StatusKind.Success);
                UpdateStateStatus($"File save → {Path.GetFileName(SaveasTxt.Text)}", SaveasTxt.Text);
            }
        }

        private void FilesaveReloadbtn_Click(object sender, EventArgs e)
        {
            IOManager.RefreshFileSaves(FilesaveList);
        }

        private void FilesaveSavebtn_Click(object sender, EventArgs e)
        {
            IOManager.SaveCorruptedFileCopy(SaveasTxt.Text, FilesaveList);
        }

        private void FilesaveRenameBtn_Click(object sender, EventArgs e)
        {
            IOManager.RenameFileSave(FilesaveList);
        }

        private void FilesaveDelete_Click(object sender, EventArgs e)
        {
            IOManager.DeleteFileSave(FilesaveList);
        }

        private void EnableStashSavesChkbox_CheckedChanged(object sender, EventArgs e)
        {
            StashBytesList.Enabled = EnableStashSavesChkbox.Checked;
            TransferStash.Enabled = EnableStashSavesChkbox.Checked;
            BisectModeChkBox.Enabled = EnableStashSavesChkbox.Checked;
            HandleStashRenderingAsync().ConfigureAwait(false);
            //Save changes to settings
            Settings.Default.StashSavesEnabled = EnableStashSavesChkbox.Checked;
            Settings.Default.Save();
        }

        #endregion

        #region Settings checkboxes

        private void BrowseEmulatorbutton_Click(object sender, EventArgs e)
        {
            if (EmulatorFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                EmulatorLocationtxt.Text = EmulatorFileDialog.FileName;
                //Save selected path to settings
                Settings.Default.EmulatorPath = EmulatorFileDialog.FileName;
                Settings.Default.Save();
            }
        }

        private void OverrideArgumentschbox_CheckedChanged(object sender, EventArgs e)
        {
            OverrideArguments.Enabled = OverrideArgumentschbox.Checked;
        }

        private void Runemulatorchbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Runemulatorchbox.Checked)
            {
                EmulatorLocationtxt.BackColor = Color.White;
                BrowseEmulatorbutton.Enabled = true;
            }
            else
            {
                EmulatorLocationtxt.BackColor = Color.Gray;
                BrowseEmulatorbutton.Enabled = false;
            }
            //Save changes to settings
            Settings.Default.RunEmulator = Runemulatorchbox.Checked;
            Settings.Default.Save();
        }

        private void SilentCorruptionchbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.SilentCorruption = SilentCorruptionchbox.Checked;
            Settings.Default.Save();
        }

        private void ReopenChbox_CheckedChanged(object sender, EventArgs e)
        {
            //Save changes to settings
            Settings.Default.ReopenEmulator = ReopenChbox.Checked;
            Settings.Default.Save();
        }

        private void FilesaveEnableAutoSaves_CheckedChanged(object sender, EventArgs e)
        {
            //Save changes to settings
            Settings.Default.AutoFileSaveEnabled = FilesaveEnableAutoSaves.Checked;
            Settings.Default.Save();
        }

        private void CorruptionQueueChkbox_CheckedChanged(object sender, EventArgs e)
        {
            //Disable Stash Saves if queue is enabled as well as file saves
            if (CorruptionQueueChkbox.Checked)
            {
                Openfilebtn.Image = Resources.edit_pencil;
                Openfilebtn.Text = "Edit Queue...";
                EnableStashSavesChkbox.Checked = false;
                FilesaveEnableAutoSaves.Checked = false;
                CorruptButton.Text = "Corrupt Files";
            }
            else
            {
                Openfilebtn.Text = "Open File";
                Openfilebtn.Image = Resources.upload;
                CorruptButton.Text = _corruptButtonIdleText;
                if (FileSelectiontxt.Text == MultipleFilesText)
                {
                    //Go back to the single file that is loaded (if any)
                    FileSelectiontxt.Text = CorruptionCore.SourcePath ?? "No file selected.";
                    SaveasTxt.Text = CorruptionCore.SourcePath != null ? Path.Combine(Path.GetDirectoryName(CorruptionCore.SourcePath) ?? "", "CorruptedFile" + Path.GetExtension(CorruptionCore.SourcePath)) : "No save location set.";
                }
            }
            //Disable the ability to change the checkboxes
            EnableStashSavesChkbox.Enabled = !CorruptionQueueChkbox.Checked;
            FilesaveEnableAutoSaves.Enabled = !CorruptionQueueChkbox.Checked;
            //Restore works in both modes (queue files are journaled and restored before every corruption)
            //Disable corrupt using stash file
            Corruptusingstashbtn.Enabled = !CorruptionQueueChkbox.Checked;
            //Disable the save as button
            Changesaveasbtn.Enabled = !CorruptionQueueChkbox.Checked;
            //Disable stash editor button
            StashEditorbtn.Enabled = !CorruptionQueueChkbox.Checked;
            StashBrowser.LoadedFile = CorruptionQueueChkbox.Checked ? null : CorruptionCore.SourcePath;
            UpdateFileStatus();
        }

        private void FileSaveOpenLocationBtn_Click(object sender, EventArgs e)
        {
            CommandRunner.OpenFolderInExplorer(Path.Combine(Application.StartupPath, "Saves"));
        }

        private void AttemptProtectedFileOverrideChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AttemptProtectedFileOverrideChkBox.Checked == true && Settings.Default.AttemptProtectedFileOverride == false) //User has enabled this option and it hasn't been saved to settings, so this is not the program setting it up via LoadSettings
            {
                if (MessageBox.Show("WARNING: You are about to enable ownership override. When LRC can't read or write a file (or create a file in a folder), it will take ownership of it and give your account full control (a UAC prompt will appear if LRC isn't running as admin). This can allow corruption of critical protected system files if you select the wrong file(s). Ensure you understand the implications before proceeding.\nDo you wish to enable this?", "WARNING - Critical Action", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    TraceLogger.Log("User declined to enable protected file override after warning prompt. Protected file override will remain disabled.");
                    AttemptProtectedFileOverrideChkBox.Checked = false;
                }
            }
            CorruptionCore.AttemptOverrideOnWrite = AttemptProtectedFileOverrideChkBox.Checked;
            Settings.Default.AttemptProtectedFileOverride = AttemptProtectedFileOverrideChkBox.Checked;
            Settings.Default.Save();
        }

        #endregion

        #region Help, ByteView, logs and misc

        private void RandomByteCorruptRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            EverynthbyteGroupbox.Visible = false;
            GenerateSelectedByteDetails();
        }

        private void CorruptEverynthByteRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            EverynthbyteGroupbox.Visible = true;
            GenerateSelectedByteDetails();
        }

        private void StashAndAutoSaveHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => ShowGuide("docs/Stash-and-Saves.md");

        private void ByteModeHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => ShowGuide("docs/Getting-Started.md", "target-parameters");

        private void FixChecksumsChkBox_CheckedChanged(object? sender, EventArgs e)
        {
            CorruptionCore.FixChecksums = FixChecksumsChkBox.Checked;
            Settings.Default.FixChecksums = FixChecksumsChkBox.Checked;
            Settings.Default.Save();
        }

        private void SaveLogBtn_Click(object sender, EventArgs e)
        {
            try
            {
                //write to working directory of the program
                string logFilePath = Path.Combine(Application.StartupPath, $"LRC_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(logFilePath, TraceLoggerTxtBox.Text);
                SetStatus($"Log saved to {logFilePath}", StatusKind.Success);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save log: {ex.Message}", $"{nameof(LunarROMCorruptor)} - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenOriginalFolderLocationBtn_Click(object sender, EventArgs e)
        {
            if (ValidateSingleFileSelection(true, true))
            {
                CommandRunner.OpenFolderInExplorer(FileSelectiontxt.Text);
            }
        }

        private void OpenSaveFolderLocationBtn_Click(object sender, EventArgs e)
        {
            if (ValidateSingleFileSelection(true, true))
            {
                CommandRunner.OpenFolderInExplorer(SaveasTxt.Text);
            }
        }

        private void OpenEmulatorFolderLocationBtn_Click(object sender, EventArgs e)
        {
            CommandRunner.OpenFolderInExplorer(EmulatorLocationtxt.Text);
        }

        private void TerminateEmulatorBtn_Click(object sender, EventArgs e)
        {
            CommandRunner.TerminateEmulator(EmulatorLocationtxt.Text);
        }

        private void MainCorruptionForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_busy)
            {
                //Wait for the background corruption to stop (and revert the output) before closing, so files aren't left half written.
                e.Cancel = true;
                if (_closeAfterBusy || MessageBox.Show("A corruption is still running. Cancel it and exit?", nameof(LunarROMCorruptor), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _closeAfterBusy = true;
                    _corruptionCts?.Cancel();
                    StopAutomation();
                }
                return;
            }
            if (CorruptionCore.HasUnrestoredInPlaceChanges && CorruptionCore.SourcePath != null)
            {
                var answer = MessageBox.Show($"{CorruptionCore.SourcePath} was corrupted in place.\n\nRestore the original file before exiting? (The undo data is deleted when LRC closes.)", $"{nameof(LunarROMCorruptor)} - Restore before exit?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (answer == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (answer == DialogResult.Yes)
                    CorruptionCore.RestoreROM(CorruptionCore.SourcePath);
            }
            if (CorruptionCore.HasUnrestoredQueueChanges)
            {
                var answer = MessageBox.Show("Some corruption queue files are still corrupted.\n\nRestore them to their originals before exiting? (The undo data is deleted when LRC closes.)", $"{nameof(LunarROMCorruptor)} - Restore queue files?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (answer == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
                if (answer == DialogResult.Yes)
                    CorruptionCore.RestoreQueueFiles();
            }
            SaveSession();
        }

        #endregion

        private void TransferFileLocationToSavePointBtn_Click(object sender, EventArgs e)
        {
            if (ValidateSingleFileSelection(true, true))
            {
                SaveasTxt.Text = FileSelectiontxt.Text;
                TraceLogger.Log($"Save location set to the original file: {FileSelectiontxt.Text}");
            }
        }

        private void RevertSaveLocationBtn_Click(object sender, EventArgs e)
        {
            if (ValidateSingleFileSelection(true, true))
            {
                SaveasTxt.Text = Path.Combine(Path.GetDirectoryName(FileSelectiontxt.Text) ?? "", "CorruptedFile" + Path.GetExtension(FileSelectiontxt.Text));
                TraceLogger.Log($"Save location reverted to default: {SaveasTxt.Text}");
            }
        }

        private void FileSelectiontxt_TextChanged(object sender, EventArgs e)
        {
            FileSelectiontxt.SelectionStart = FileSelectiontxt.Text.Length;
            FileSelectiontxt.SelectionLength = 0;
        }

        private void SaveasTxt_TextChanged(object sender, EventArgs e)
        {
            SaveasTxt.SelectionStart = SaveasTxt.Text.Length;
            SaveasTxt.SelectionLength = 0;
        }

        /// <summary>
        /// Deletes the corrupted file at the save location. (It used to delete FileSelectiontxt, i.e. the ORIGINAL file.)
        /// </summary>
        private void DeleteCorruptedFileBtn_Click(object sender, EventArgs e)
        {
            string target = SaveasTxt.Text;
            if (_busy || CorruptionQueueChkbox.Checked || string.IsNullOrWhiteSpace(target) || target == MultipleFilesSaveText) return;
            if (CorruptionCore.IsInPlace(target))
            {
                TraceLogger.Log("The save location is the original file, so it isn't deleted. Use Restore File to undo the corruption instead.", StatusSeverityType.Warning, true);
                return;
            }
            if (!File.Exists(target))
            {
                TraceLogger.Log($"There is no corrupted file to delete at {target}.", StatusSeverityType.Information, true);
                return;
            }
            IOManager.DeleteFile(target, true);
            if (!File.Exists(target))
            {
                CorruptionCore.InvalidateOutput(target); //The next corruption makes a full copy of the original again
                UpdateByteViewIfEnabled();
            }
        }

        private async void ExcludeRangeBtn_Click(object sender, EventArgs e)
        {
            if (!CanEditExclusions()) return;
            long start = (long)StartByteNumb.Value, end = (long)EndByteNumb.Value; //The NumericUpDowns keep start <= end
            if (start == 0 && end >= CorruptionCore.MaxByte &&
                MessageBox.Show(this, "This excludes the whole file, so no engine will be able to change anything. Continue?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;
            CorruptionCore.Exclusions.Add(start, end);
            TraceLogger.Log($"Excluded 0x{start:X} - 0x{end:X} ({FormatByteCount(end - start + 1)}). Every engine skips it from the next corruption on.");
            await SyncStashExcludesAsync();
        }

        private async void RemoveSelctedExcludeRangeBtn_Click(object sender, EventArgs e)
        {
            if (!CanEditExclusions()) return;
            long start = (long)StartByteNumb.Value, end = (long)EndByteNumb.Value;
            long removed = CorruptionCore.Exclusions.Remove(start, end);
            if (removed == 0)
            {
                TraceLogger.Log($"Nothing in 0x{start:X} - 0x{end:X} is excluded. Set the byte range over a red area on ByteView first.", StatusSeverityType.Information, true);
                return;
            }
            TraceLogger.Log($"Removed {FormatByteCount(removed)} of excluded bytes in 0x{start:X} - 0x{end:X}.");
            await SyncStashExcludesAsync();
        }

        private async void ResetExclusionsBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, $"Remove all {CorruptionCore.Exclusions.Count} excluded range(s)? Every engine will be able to change those bytes again.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            CorruptionCore.Exclusions.Clear();
            TraceLogger.Log("All excluded ranges were reset.");
            await SyncStashExcludesAsync();
        }

        private bool CanEditExclusions()
        {
            if (_busy)
            {
                TraceLogger.Log("Wait for the corruption to finish before changing the excluded ranges.", StatusSeverityType.Warning, true);
                return false;
            }
            if (!CorruptionCore.IsLoaded)
            {
                TraceLogger.Log("Load a file first. Excluded ranges belong to the loaded file.", StatusSeverityType.Warning, true);
                return false;
            }
            if (CorruptionQueueChkbox.Checked)
            {
                TraceLogger.Log("Excluded ranges only work on the loaded file, not in Multi-File mode.", StatusSeverityType.Warning, true);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Writes the current excluded ranges into the current stash (as its EXCLUDE lines) and refreshes the stash list and ByteView,
        /// so a stash saved now keeps the exclusions. Does nothing when there is no corruption yet: the next one records them anyway.
        /// </summary>
        internal async Task SyncStashExcludesAsync()
        {
            InvalidateByteViewOverlays();
            var stash = CorruptionCore.Stash;
            if (_busy || stash.Count == 0 && stash.OpenLocation == null) return;
            stash.SetExcludes(CorruptionCore.Exclusions.Snapshot());
            await HandleStashRenderingAsync();
        }

        private void StartEmulatorNowBtn_Click(object sender, EventArgs e)
        {
            RunEmulatorAfterCheck(SaveasTxt.Text);
        }

        private void ResetSettingsBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Reset all settings to their defaults? This will not affect the current file or stash.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Settings.Default.Reset();
                LoadSettings();
                TraceLogger.Log("All settings were reset to their defaults.");
            }
        }
    }
}
