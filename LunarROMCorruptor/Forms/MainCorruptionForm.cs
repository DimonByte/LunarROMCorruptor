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
using LunarROMCorruptor.Modules.Helpers;
using LunarROMCorruptor.Properties;
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
        public readonly ExclusionEngineControl ExclusionEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly SourceCodeEngineControl SourceCodeEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly DatamoshEngineControl DatamoshEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        public readonly BlocksEngineControl BlocksEngineFrame = new(); //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
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
        private ByteViewImage? _byteViewInfo; //The image currently shown in ByteView and how its pixels map to file offsets
        private bool _byteViewDragging;
        private long _byteViewDragStart;
        private long _byteViewDragEnd;
        private int _byteViewRenderVersion;

        public MainCorruptionForm()
        {
            InitializeComponent();
            FormClosing += MainCorruptionForm_FormClosing;
            //ByteView targeting: drag on the image to set the start/end byte
            ByteViewPictureBox.Cursor = Cursors.Cross;
            ByteViewPictureBox.MouseDown += ByteViewPictureBox_MouseDown;
            ByteViewPictureBox.MouseMove += ByteViewPictureBox_MouseMove;
            ByteViewPictureBox.MouseUp += ByteViewPictureBox_MouseUp;
            ByteViewPictureBox.Paint += ByteViewPictureBox_Paint;
            ByteViewPictureBox.Resize += (_, _) => ByteViewPictureBox.Invalidate();
            ByteViewModeComboBox.SelectedIndexChanged += (_, _) => { if (CorruptionCore.IsLoaded && _byteViewInfo != null) UpdateByteView(); };
            FormClosed += (_, _) =>
            {
                TraceLogger.OnLogEntryAdded -= OnTraceLogEntryAdded;
                CorruptionCore.ResetSession();
                CorruptionCore.ResetQueueSession();
                CorruptionHistory.Clear();
            };
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
            IOManager.RefreshCorruptionStashList(StashFileList);
            IOManager.RefreshFileSaves(FilesaveList);
            LoadSettings();
            AttentionPictureBox.BringToFront();
            //This code under this comment fixes a .net bug where the trackbar allocates a huge amount of memory if the trackbar maximum value is set to a large amount.
            EndByteTrackbar.TickStyle = TickStyle.None;
            StartByteTrackBar.TickStyle = TickStyle.None;
            CorrupteverynthbyteTrackbar.TickStyle = TickStyle.None;
            IntensityTrackbar.TickStyle = TickStyle.None;
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
            CorruptionCore.AttemptOverrideOnWrite = AttemptProtectedFileOverrideChkBox.Checked;
        }

        #region File loading and byte range

        public void LoadFile(string FileLocation)
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
                FileSelectiontxt.Text = CorruptionCore.SourcePath;
                string directory = Path.GetDirectoryName(CorruptionCore.SourcePath!) ?? "";
                SaveasTxt.Text = Path.Combine(directory, "CorruptedFile" + Path.GetExtension(CorruptionCore.SourcePath));
                MainSaveFileDialog.InitialDirectory = directory;
                _corruptButtonIdleText = $"Corrupt File ({CorruptionCore.rawFileSize} {CorruptionCore.fileSizeUnit})";
                CorruptButton.Text = _corruptButtonIdleText;
                StashBytesList.Items.Clear();
                _inPlaceWarningShown = false;
                ClearByteView();
                if (ExclusionEngineFrame.Visible) ExclusionEngineFrame.LoadAddresses();
                TraceLogger.Log($"File loaded successfully and UI updated for file: {FileLocation}");
            }
            else
            {
                TraceLogger.Log($"Failed to load file: {FileLocation}", StatusSeverityType.Error);
            }
            AttentionPictureBox.Hide();
            AttentionPictureBox.Image = Resources.dragicon;
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
                return;
            }
            //Main Function - If the user didn't cancel, load file.
            if (MainOpenFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                LoadFile(MainOpenFileDialog.FileName);
            }
        }

        private void StartByteTrackBar_Scroll(object sender, EventArgs e)
        {
            StartByteNumb.Value = FromTrackValue(StartByteTrackBar, StartByteNumb);
        }

        private void EndByteTrackbar_Scroll(object sender, EventArgs e)
        {
            EndByteNumb.Value = FromTrackValue(EndByteTrackbar, EndByteNumb);
        }

        private void StartByteNumb_ValueChanged(object sender, EventArgs e)
        {
            StartByteTrackBar.Value = ToTrackValue(StartByteNumb.Value, StartByteTrackBar);
            ByteViewPictureBox.Invalidate(); //Redraw the range overlay
        }

        private void EndByteNumb_ValueChanged(object sender, EventArgs e)
        {
            EndByteTrackbar.Value = ToTrackValue(EndByteNumb.Value, EndByteTrackbar);
            ByteViewPictureBox.Invalidate();
        }

        private void CorrupteverynthbyteTrackbar_Scroll(object sender, EventArgs e)
        {
            EveryNthByte.Value = Math.Clamp(CorrupteverynthbyteTrackbar.Value, EveryNthByte.Minimum, EveryNthByte.Maximum);
        }

        private void IntensityTrackbar_Scroll(object sender, EventArgs e)
        {
            Intensity.Value = Math.Clamp(IntensityTrackbar.Value, Intensity.Minimum, Intensity.Maximum);
        }

        private void Intensity_ValueChanged(object sender, EventArgs e)
        {
            IntensityTrackbar.Value = (int)Math.Clamp(Intensity.Value, IntensityTrackbar.Minimum, IntensityTrackbar.Maximum);
        }

        private void EveryNthByte_ValueChanged(object sender, EventArgs e)
        {
            CorrupteverynthbyteTrackbar.Value = (int)Math.Clamp(EveryNthByte.Value, CorrupteverynthbyteTrackbar.Minimum, CorrupteverynthbyteTrackbar.Maximum);
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
            UserControl? newControl = null;
            switch (selectedEngine)
            {
                case CorruptionEngineType.MergeEngine:
                    newControl = MergeEngineFrame;
                    newControl.Name = "MergeEngineControl";
                    break;

                case CorruptionEngineType.LogicEngine:
                    newControl = LogicEngineFrame;
                    newControl.Name = "LogicEngineControl";
                    break;

                case CorruptionEngineType.NightmareEngine:
                    newControl = NightmareEngineFrame;
                    newControl.Name = "NightmareEngineControl";
                    break;

                case CorruptionEngineType.LerpEngine:
                    newControl = LerpEngineFrame;
                    newControl.Name = "LerpEngineControl";
                    break;

                case CorruptionEngineType.VectorEngine:
                    newControl = VectorEngineFrame;
                    newControl.Name = "VectorEngineControl";
                    break;

                case CorruptionEngineType.ManualEngine:
                    newControl = ManualEngineFrame;
                    newControl.Name = "ManualEngineControl";
                    break;

                case CorruptionEngineType.ExclusionEngine:
                    newControl = ExclusionEngineFrame;
                    newControl.Name = "ExclusionEngineControl";
                    ExclusionEngineFrame.LoadAddresses(); //The loaded file may have changed since the profile was last shown
                    break;

                case CorruptionEngineType.DatamoshEngine:
                    newControl = DatamoshEngineFrame;
                    newControl.Name = "DatamoshEngineControl";
                    break;

                case CorruptionEngineType.BlocksEngine:
                    newControl = BlocksEngineFrame;
                    newControl.Name = "BlocksEngineControl";
                    break;

                case CorruptionEngineType.SourceCodeEngine:
                    newControl = SourceCodeEngineFrame;
                    newControl.Name = "SourceCodeEngineControl";
                    break;
            }

            if (newControl != null)
            {
                newControl.Dock = DockStyle.Fill;
                newControl.Visible = true;
                if (!CorruptionEngineTab.Controls.Contains(newControl))
                {
                    CorruptionEngineTab.Controls.Add(newControl);
                }
            }
        }

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
                Intensity = CorruptEverynthByteRadioBtn.Checked ? (long)EveryNthByte.Value : (long)Intensity.Value
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
                    case CorruptionEngineType.ExclusionEngine: settings.Exclusion = ExclusionEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.VectorEngine: settings.Vector = VectorEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.SourceCodeEngine: settings.SourceCode = SourceCodeEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.BlocksEngine: settings.Blocks = BlocksEngineFrame.GetSettings(); break;
                    case CorruptionEngineType.DatamoshEngine: settings.Datamosh = DatamoshEngineFrame.GetSettings(); break;
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
            return result;
        }

        private void BeginBusy(string status)
        {
            _busy = true;
            _corruptionCts?.Dispose();
            _corruptionCts = new CancellationTokenSource();
            _enabledBeforeBusy.Clear();
            foreach (Control control in new Control[] { Openfilebtn, Restorefilebtn, Changesaveasbtn, Corruptusingstashbtn, StashEditorbtn, TransferStash, CopySaveToCorruptedFileBtn, FilesaveSavebtn, CorruptionQueueChkbox, ByteViewRefreshBtn, CorruptionEngineComboBox })
            {
                _enabledBeforeBusy[control] = control.Enabled;
                control.Enabled = false;
            }
            AllowDrop = false;
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
        }

        private bool ValidateSingleFileSelection()
        {
            if (string.IsNullOrEmpty(FileSelectiontxt.Text) || FileSelectiontxt.Text == "No file selected." || FileSelectiontxt.Text == MultipleFilesText || !CorruptionCore.IsLoaded || !File.Exists(CorruptionCore.SourcePath))
            {
                TraceLogger.Log("No valid file selected for corruption. Please select a valid file before attempting to corrupt.", StatusSeverityType.Error, true);
                return false;
            }
            if (string.IsNullOrWhiteSpace(SaveasTxt.Text) || SaveasTxt.Text == MultipleFilesSaveText)
            {
                TraceLogger.Log("No valid save location set. Please choose where the corrupted file should be saved.", StatusSeverityType.Error, true);
                return false;
            }
            if (CorruptionCore.IsInPlace(SaveasTxt.Text) && !_inPlaceWarningShown)
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
            StashBytesList.Items.Clear();

            BeginBusy("Corrupting");
            CorruptionResult result;
            try
            {
                var token = _corruptionCts!.Token;
                var progress = CreateProgress("Corrupting");
                result = await Task.Run(() =>
                {
                    //The stash is always recorded so the corruption history works, the stash checkbox only controls the stash list/saving
                    var r = CorruptionCore.StartCorruption(settings, outputPath, true, token, progress);
                    if (r == CorruptionResult.Success)
                        CorruptionHistory.Add(CorruptionCore.Stash, engineName, sourcePath, EnableStashSavesChkbox.Checked);
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
                return result;
            }

            if (FilesaveEnableAutoSaves.Checked) //Save the corrupted file copy into the file saves if auto saves is enabled.
            {
                string? saved = await Task.Run(() => IOManager.CopyToSaves(outputPath)); //Can be a large file now, so it's copied in the background
                if (saved != null) FilesaveList.Items.Add(saved);
            }

            RunEmulatorIfEnabled(outputPath);
            await HandleStashSavingAsync();
            UpdateByteViewIfEnabled();
            StartSuccessFeedback();
            return result;
        }

        private async Task HandleStashSavingAsync()
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

        private void LoadStashFileIntoList(string stashFileLocation)
        {
            StashBytesList.BeginUpdate();
            StashBytesList.Items.Clear();
            int count = 0;
            foreach (var item in File.ReadLines(stashFileLocation))
            {
                if (++count > StashDisplayLimit)
                {
                    StashBytesList.Items.Clear();
                    StashBytesList.Items.Add("LargeStash");
                    break;
                }
                StashBytesList.Items.Add(item);
            }
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
            }
            else
            {
                string path = files[0];
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
                LoadFile(path);
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
                    MessageBox.Show("None of the queue files are currently corrupted by LRC, there is nothing to restore.", $"{nameof(LunarROMCorruptor)} - Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show($"{count} queue file(s) restored to their originals.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("ROM restored successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateByteViewIfEnabled();
            }
        }

        /// <summary>
        /// Shows the last corruptions with options to re-apply them, save them to the stash list or open them in the stash editor.
        /// </summary>
        private void CorruptionHistoryBtn_Click(object? sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            var entries = CorruptionHistory.Entries;
            if (entries.Count == 0)
            {
                menu.Items.Add(new ToolStripMenuItem($"No corruptions yet (the last {CorruptionHistory.MaxEntries} are kept)") { Enabled = false });
            }
            for (int k = 0; k < entries.Count; k++)
            {
                var entry = entries[k];
                var item = new ToolStripMenuItem($"#{k + 1}  {entry}");
                item.DropDownItems.Add("Re-apply this corruption", null, (_, _) => ReapplyHistoryEntry(entry));
                item.DropDownItems.Add("Save to stash list", null, (_, _) =>
                {
                    if (CorruptionHistory.SaveToStashList(entry) != null)
                        IOManager.RefreshCorruptionStashList(StashFileList);
                });
                item.DropDownItems.Add("Open in Stash Editor", null, (_, _) =>
                {
                    if (_stashEditor == null || _stashEditor.IsDisposed)
                        _stashEditor = new StashEditor();
                    _stashEditor.AttemptStashLoad(entry.StashPath);
                    _stashEditor.ShowDialog();
                });
                item.Enabled = !_busy && File.Exists(entry.StashPath);
                menu.Items.Add(item);
            }
            menu.Closed += (_, _) => BeginInvoke(menu.Dispose);
            menu.Show(CorruptionHistoryBtn, new Point(0, CorruptionHistoryBtn.Height));
        }

        private void ReapplyHistoryEntry(CorruptionHistoryEntry entry)
        {
            if (CorruptionQueueChkbox.Checked)
            {
                TraceLogger.Log("Corruption history can only be re-applied in single file mode.", StatusSeverityType.Warning, true);
                return;
            }
            if (!CorruptionCore.PathsEqual(entry.SourcePath, CorruptionCore.SourcePath) &&
                MessageBox.Show($"This corruption was made for {Path.GetFileName(entry.SourcePath)}, but {Path.GetFileName(CorruptionCore.SourcePath ?? "(no file)")} is loaded. Apply it anyway?",
                    $"{nameof(LunarROMCorruptor)} - Corruption History", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            CorruptUsingStashFile(entry.StashPath);
        }

        private void TransferStash_Click(object sender, EventArgs e) //This saves the stash
        {
            IOManager.TransferStash(CorruptionCore.Stash, StashFileList, FileSelectiontxt.Text, rnd);
        }

        private void StashEditorbtn_Click(object sender, EventArgs e)
        {
            //Reuse one editor window (it hides instead of closing), the old code created a new hidden window on every click.
            if (_stashEditor == null || _stashEditor.IsDisposed)
                _stashEditor = new StashEditor();
            if (StashFileList.SelectedItems.Count > 0)
            {
                TraceLogger.Log("Attempting to load stash file into stash editor");
                if (StashFileList.SelectedItem == null)
                {
                    TraceLogger.Log("No stash file selected to load into stash editor.", StatusSeverityType.Warning, true);
                }
                else
                    _stashEditor.AttemptStashLoad(Path.Combine(IOManager.CorruptionStashDirectory, StashFileList.GetItemText(StashFileList.SelectedItem)!));
            }
            _stashEditor.ShowDialog();
        }

        private void RenameStash_Click(object sender, EventArgs e)
        {
            IOManager.RenameStash(StashFileList);
        }

        private void RefreshStash_Click(object sender, EventArgs e)
        {
            IOManager.RefreshCorruptionStashList(StashFileList);
        }

        private void DeleteStash_Click(object sender, EventArgs e)
        {
            IOManager.DeleteStash(StashFileList);
        }

        private void CopySaveToCorruptedFileBtn_Click(object sender, EventArgs e)
        {
            IOManager.CopySaveToCorruptedFile(SaveasTxt.Text, FilesaveList);
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

        private void Corruptusingstash_Click(object sender, EventArgs e)
        {
            //Check if a StashFile is selected before running code
            if (StashFileList.SelectedItems.Count == 0)
            {
                TraceLogger.Log("Attempted to corrupt using stash file but no stash file is selected. Please select a stash file before attempting to corrupt.", StatusSeverityType.Error, true);
                return;
            }
            if (StashFileList.SelectedItem == null)
            {
                TraceLogger.Log("No stash file selected to corrupt with.", StatusSeverityType.Warning, true);
                return;
            }
            CorruptUsingStashFile(Path.Combine(IOManager.CorruptionStashDirectory, StashFileList.GetItemText(StashFileList.SelectedItem)!));
        }

        /// <summary>
        /// Applies a stash file to a fresh copy of the loaded file and saves it to the save location.
        /// </summary>
        public async void CorruptUsingStashFile(string StashFileLocation)
        {
            if (_busy) return;
            if (!File.Exists(StashFileLocation))
            {
                TraceLogger.Log($"Stash file not found: {StashFileLocation}", StatusSeverityType.Error, true);
                return;
            }
            if (!ValidateSingleFileSelection()) return;

            string outputPath = SaveasTxt.Text;
            BeginBusy("Applying stash");
            CorruptionResult result;
            try
            {
                var token = _corruptionCts!.Token;
                var progress = CreateProgress("Applying stash");
                result = await Task.Run(() => CorruptionCore.ApplyStashFile(StashFileLocation, outputPath, token, progress));
            }
            finally
            {
                EndBusy();
            }
            if (result != CorruptionResult.Success)
            {
                if (result == CorruptionResult.Failed) StartFailureFeedback();
                return;
            }

            TraceLogger.Log($"Corrupted file with stash file successfully saved: {outputPath}");
            StartSuccessFeedback();
            RunEmulatorIfEnabled(outputPath);
            try
            {
                //Set StashByteList to the files contents, each line is each item on the list
                LoadStashFileIntoList(StashFileLocation);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to show the stash file contents: {ex.Message}", StatusSeverityType.Warning);
            }
            UpdateByteViewIfEnabled();
        }

        private void EnableStashSavesChkbox_CheckedChanged(object sender, EventArgs e)
        {
            StashBytesList.Enabled = EnableStashSavesChkbox.Checked;
            TransferStash.Enabled = EnableStashSavesChkbox.Checked;
            //Save changes to settings
            Settings.Default.StashSavesEnabled = EnableStashSavesChkbox.Checked;
            Settings.Default.Save();
        }

        private void StashList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                StashFileList.SelectedIndex = StashFileList.IndexFromPoint(e.X, e.Y);
                contextStripStash.Show(Cursor.Position);
            }
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

        #region Automation

        private void StartAutomationBtn_Click(object sender, EventArgs e)
        {
            //If the buttons text is "Start Automation", then start automation and change the text and color to stop and OrangeRed
            if (StartAutomationBtn.Text == "Start Automation")
            {
                StartAutomationBtn.Text = "Stop Automation";
                StartAutomationBtn.ForeColor = Color.OrangeRed;
                TraceLogger.SuppressMessageBoxes = true; //Errors go to the log instead of stacking up message boxes
                AutomationTimer.Start();
            }
            else
            {
                StopAutomation();
            }
        }

        private void StopAutomation()
        {
            StartAutomationBtn.Text = "Start Automation";
            StartAutomationBtn.ForeColor = Color.SpringGreen;
            AutomationTimer.Stop();
            TraceLogger.SuppressMessageBoxes = false;
        }

        private void RemoveTaskBtn_Click(object sender, EventArgs e)
        {
            //Removes selected item from the automation list
            if (AutomationList.SelectedItem != null)
                AutomationList.Items.Remove(AutomationList.SelectedItem);
        }

        private void AddTaskBtn_Click(object sender, EventArgs e)
        {
            //Adds automation task from the combo box into the automation list
            if (AutomationTaskComboBox.SelectedItem != null)
                AutomationList.Items.Add(AutomationTaskComboBox.SelectedItem);
        }

        private void DelayInAutomationNUD_ValueChanged(object sender, EventArgs e)
        {
            //Converts the NumericUpDown number from seconds to miliseconds to use on the timer
            AutomationTimer.Interval = (int)Math.Max(1, DelayInAutomationNUD.Value * 1000);
        }

        private long RandomDecimal(decimal min, decimal max) => rnd.NextInt64((long)min, (long)max + 1);

        private async void AutomationTimer_Tick(object sender, EventArgs e)
        {
            if (_busy || _automationTickRunning) return; //Don't stack corruptions if the previous one is still running
            _automationTickRunning = true;
            try
            {
                //Snapshot the task list, the user can change it while a corruption is running
                var tasks = AutomationList.Items.Cast<object>().Select(i => i?.ToString() ?? "").ToList();
                foreach (var item in tasks)
                {
                    if (!AutomationTimer.Enabled) break;
                    switch (item)
                    {
                        case "Run Corruption":
                            var result = await RunCorruptionAsync();
                            if (result == null || result == CorruptionResult.Failed)
                            {
                                StopAutomation();
                                TraceLogger.Log("Automation stopped because the corruption couldn't run. Check the log for the reason.", StatusSeverityType.Error, true);
                                return;
                            }
                            break;
                        case "Randomize Intensity":
                            if (CorruptEverynthByteRadioBtn.Checked)
                                EveryNthByte.Value = RandomDecimal(EveryNthByte.Minimum, EveryNthByte.Maximum);
                            else
                                Intensity.Value = RandomDecimal(Intensity.Minimum, Intensity.Maximum);
                            break;
                        case "Randomize Start Byte":
                            //Keeps the start byte at or below the end byte
                            StartByteNumb.Value = RandomDecimal(StartByteNumb.Minimum, EndByteNumb.Value);
                            break;
                        case "Randomize End Byte":
                            EndByteNumb.Value = RandomDecimal(StartByteNumb.Value, EndByteNumb.Maximum);
                            break;
                        case "Randomize both Start byte and End byte":
                            {
                                long start = RandomDecimal(StartByteNumb.Minimum, StartByteNumb.Maximum);
                                long end = rnd.NextInt64(start, (long)EndByteNumb.Maximum + 1);
                                StartByteNumb.Value = Math.Min(start, (long)StartByteNumb.Maximum);
                                EndByteNumb.Value = Math.Min(end, (long)EndByteNumb.Maximum);
                            }
                            break;
                        case "Randomize Corruption Engine":
                            {
                                //Only byte engines are randomized. Merge needs a merge file and the Source Code Engine only makes sense for text files.
                                var engines = new List<CorruptionEngineType> { CorruptionEngineType.NightmareEngine, CorruptionEngineType.LerpEngine, CorruptionEngineType.LogicEngine, CorruptionEngineType.ManualEngine, CorruptionEngineType.ExclusionEngine, CorruptionEngineType.VectorEngine, CorruptionEngineType.BlocksEngine };
                                if (File.Exists(MergeEngineFrame.MergeFileLocationTxt.Text))
                                    engines.Add(CorruptionEngineType.MergeEngine);
                                CorruptionEngineComboBox.Text = EngineParser.GetEngineDisplayName(engines[rnd.Next(engines.Count)]);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                StopAutomation();
                TraceLogger.Log($"Automation stopped because of an error: {ex.Message}", StatusSeverityType.Error, true);
            }
            finally
            {
                _automationTickRunning = false;
            }
        }

        private void MoveTaskUpBtn_Click(object sender, EventArgs e)
        {
            //Moves selected item on automation list up
            if (AutomationList.SelectedIndex > 0 && AutomationList.SelectedItem is object item)
            {
                int index = AutomationList.SelectedIndex;
                AutomationList.Items.RemoveAt(index);
                AutomationList.Items.Insert(index - 1, item);
                AutomationList.SelectedIndex = index - 1;
            }
        }

        private void MoveTaskDownBtn_Click(object sender, EventArgs e)
        {
            //Moves selected item on automation list down
            if (AutomationList.SelectedIndex >= 0 && AutomationList.SelectedIndex < AutomationList.Items.Count - 1 && AutomationList.SelectedItem is object item)
            {
                int index = AutomationList.SelectedIndex;
                AutomationList.Items.RemoveAt(index);
                AutomationList.Items.Insert(index + 1, item);
                AutomationList.SelectedIndex = index + 1;
            }
        }

        #endregion

        #region Help, ByteView, logs and misc

        private void RandomByteCorruptRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            EverynthbyteGroupbox.Visible = false;
        }

        private void CorruptEverynthByteRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            EverynthbyteGroupbox.Visible = true;
        }

        private void StashAndAutoSaveHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show($"Auto Saves and Stash Files are two different ways of saving your corruptions.{Environment.NewLine}{Environment.NewLine}Auto Saves save the entire file after corruption in the Auto Saves folder in the {nameof(LunarROMCorruptor)} directory.{Environment.NewLine}{Environment.NewLine}Stash files only save the bytes that are changed, so it takes up less storage space and allow the user to also edit each byte that was affected by using the Stash Editor. (e.g. changing a characters colour){Environment.NewLine}{Environment.NewLine}The Source Code Engine saves its changes in the stash as File(offset).VARLINE(\"name\",line).SET(\"value\") and the Datamosh Engine as File(offset).FRAME(frame).SET(DROP/REPLACE/REPEAT,n).{Environment.NewLine}{Environment.NewLine}Corruption History keeps the stash of the last {CorruptionHistory.MaxEntries} corruptions, so you can re-apply one or save it to the stash list. The Stash Editor's Mutate tool makes variations of a corruption you like.", $"{nameof(LunarROMCorruptor)} - Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void ByteModeHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show($"With Corrupt Every Nth Byte, you can apply regular corruptions to your file by specifying the interval to corrupt (e.g., every 1st byte, every 2nd byte, etc.).{Environment.NewLine}{Environment.NewLine}On the other hand, Intensity allows for randomized corruptions, where the corruptor selects random addresses to modify.", $"{nameof(LunarROMCorruptor)} - Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void ByteViewHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show($"ByteView shows the bytes of a file as an image. Convert/Update shows the latest corrupted file (or the loaded file if nothing has been corrupted yet). Large files are downsampled.{Environment.NewLine}{Environment.NewLine}" +
                $"Modes:{Environment.NewLine}- Normal: every 3 bytes are one pixel (colour or greyscale).{Environment.NewLine}- Diff: the file in grey with every byte changed by the last corruption in red.{Environment.NewLine}- Entropy: how random each part of the file is. Dark = empty/padding, blue/green = tables and text, yellow = code, red = compressed or encrypted data.{Environment.NewLine}{Environment.NewLine}" +
                $"Targeting: drag over the image to set the Start/End byte to that area (e.g. drag over the graphics you see in Normal mode or a blue area in Entropy mode). The current range is highlighted in yellow. Right-click to reset the range to the whole file. Hover to see the offset under the mouse.",
                $"{nameof(LunarROMCorruptor)} - Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void ByteViewSaveImageBtn_Click(object sender, EventArgs e)
        {
            if (ByteViewPictureBox.Image != null)
            {
                using SaveFileDialog saveFileDialog = new();
                saveFileDialog.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Image";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using Bitmap imageToSave = new(ByteViewPictureBox.Image);
                        var format = Path.GetExtension(saveFileDialog.FileName).ToLowerInvariant() is ".jpg" or ".jpeg" ? System.Drawing.Imaging.ImageFormat.Jpeg : System.Drawing.Imaging.ImageFormat.Png;
                        imageToSave.Save(saveFileDialog.FileName, format);
                    }
                    catch (Exception ex)
                    {
                        TraceLogger.Log($"Unable to save the ByteView image: {ex.Message}", StatusSeverityType.Error, true);
                    }
                }
            }
        }

        private void ByteViewRefreshBtn_Click(object sender, EventArgs e)
        {
            if (!CorruptionCore.IsLoaded)
            {
                TraceLogger.Log("Attempted to refresh ByteView but no file is loaded. Please load a file before attempting to use ByteView.", StatusSeverityType.Error, true);
                return;
            }
            UpdateByteView();
        }

        private void UpdateByteViewIfEnabled()
        {
            if (ByteViewupdateWhenCorruptedChkBox.Checked)
                UpdateByteView();
        }

        /// <summary>
        /// Renders the latest corrupted file (or the loaded file if nothing has been corrupted yet) in ByteView on a background thread.
        /// </summary>
        private async void UpdateByteView()
        {
            string? path = CorruptionCore.LastOutputPath != null && File.Exists(CorruptionCore.LastOutputPath) ? CorruptionCore.LastOutputPath : CorruptionCore.SourcePath;
            if (path == null) return;
            bool colour = byteViewColourChkbox.Checked, flipH = flipHorizontalChkbox.Checked, flipV = flipVerticalChkbox.Checked;
            var mode = ByteViewModeComboBox.SelectedIndex switch { 1 => ByteViewMode.Diff, 2 => ByteViewMode.Entropy, _ => ByteViewMode.Normal };
            string? original = CorruptionCore.SourcePath;
            //For in-place corruption the original isn't on disk anymore, so the Diff uses the stash instead
            long[]? stashOffsets = mode == ByteViewMode.Diff && CorruptionCore.PathsEqual(path, original) && CorruptionCore.Stash.Count > 0 ? [.. CorruptionCore.Stash.ByteOffsets] : null;
            int version = ++_byteViewRenderVersion;
            ByteViewRefreshBtn.Enabled = false;
            ByteViewInfoLbl.Text = "Rendering...";
            try
            {
                var rendered = await Task.Run(() => ByteView.Render(path, mode, colour, flipH, flipV, original, stashOffsets));
                if (version != _byteViewRenderVersion || rendered == null)
                {
                    rendered?.Dispose(); //A newer render was started (or this one failed)
                    if (rendered == null) TraceLogger.Log("ByteView conversion failed: no image produced.", StatusSeverityType.Error);
                    return;
                }
                var old = _byteViewInfo;
                _byteViewInfo = rendered;
                ByteViewPictureBox.Image = rendered.Image;
                old?.Dispose();
                ByteViewInfoLbl.Text = DescribeByteView(rendered);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"ByteView failed: {ex.Message}", StatusSeverityType.Error);
                ByteViewInfoLbl.Text = "ByteView failed, see the log.";
            }
            finally
            {
                ByteViewRefreshBtn.Enabled = !_busy;
            }
        }

        private static string DescribeByteView(ByteViewImage view)
        {
            string scale = $"1 px = {view.BytesPerPixel:N0} bytes";
            return view.Mode switch
            {
                ByteViewMode.Diff when view.ChangedBytes >= 0 => $"{view.ChangedBytes:N0} changed bytes (red). {scale}",
                ByteViewMode.Entropy => $"Dark = empty, blue/green = text/tables, yellow = code, red = compressed. {scale}",
                _ => $"Drag to set the byte range, right-click to reset. {scale}"
            };
        }

        private void ClearByteView()
        {
            _byteViewRenderVersion++;
            ByteViewPictureBox.Image = null;
            _byteViewInfo?.Dispose();
            _byteViewInfo = null;
            ByteViewInfoLbl.Text = "Drag on the image to set the byte range.";
        }

        /// <summary>
        /// Where the image is drawn inside the picture box (SizeMode = Zoom keeps the aspect ratio and centres it).
        /// </summary>
        private RectangleF GetByteViewImageRectangle(ByteViewImage view)
        {
            var client = ByteViewPictureBox.ClientSize;
            float scale = Math.Min((float)client.Width / view.Width, (float)client.Height / view.Height);
            float w = view.Width * scale, h = view.Height * scale;
            return new RectangleF((client.Width - w) / 2f, (client.Height - h) / 2f, w, h);
        }

        /// <summary>
        /// Converts a mouse position on the picture box to a file offset. Returns -1 outside of the image.
        /// </summary>
        private long ByteViewOffsetAt(Point location, bool clampToImage)
        {
            var view = _byteViewInfo;
            if (view == null) return -1;
            var rect = GetByteViewImageRectangle(view);
            if (!clampToImage && !rect.Contains(location)) return -1;
            int x = (int)Math.Clamp((location.X - rect.X) * view.Width / rect.Width, 0, view.Width - 1);
            int y = (int)Math.Clamp((location.Y - rect.Y) * view.Height / rect.Height, 0, view.Height - 1);
            long offset = view.OffsetOf(x, y);
            if (offset < 0 && clampToImage) offset = view.FileLength - 1; //Past the end of the data on the last row
            return offset;
        }

        private void ByteViewPictureBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_byteViewInfo == null || !CorruptionCore.IsLoaded || CorruptionQueueChkbox.Checked || _busy) return;
            if (e.Button == MouseButtons.Right)
            {
                //Right-click resets the range to the whole file
                StartByteNumb.Value = 0;
                EndByteNumb.Value = EndByteNumb.Maximum;
                TraceLogger.Log("ByteView: byte range reset to the whole file.");
                return;
            }
            if (e.Button != MouseButtons.Left) return;
            long offset = ByteViewOffsetAt(e.Location, false);
            if (offset < 0) return;
            _byteViewDragging = true;
            _byteViewDragStart = _byteViewDragEnd = offset;
            ByteViewPictureBox.Invalidate();
        }

        private void ByteViewPictureBox_MouseMove(object? sender, MouseEventArgs e)
        {
            var view = _byteViewInfo;
            if (view == null) return;
            if (_byteViewDragging)
            {
                _byteViewDragEnd = ByteViewOffsetAt(e.Location, true);
                long a = Math.Min(_byteViewDragStart, _byteViewDragEnd), b = Math.Max(_byteViewDragStart, _byteViewDragEnd) + view.BytesPerPixel - 1;
                ByteViewInfoLbl.Text = $"0x{a:X} - 0x{Math.Min(b, view.FileLength - 1):X}\n({FormatByteCount(Math.Min(b, view.FileLength - 1) - a + 1)})";
                ByteViewPictureBox.Invalidate();
                return;
            }
            long offset = ByteViewOffsetAt(e.Location, false);
            ByteViewInfoLbl.Text = offset >= 0 ? $"Offset 0x{offset:X}\n({offset:N0})\n1 px = {view.BytesPerPixel:N0} bytes" : DescribeByteView(view);
        }

        private void ByteViewPictureBox_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!_byteViewDragging || _byteViewInfo == null) return;
            _byteViewDragging = false;
            var view = _byteViewInfo;
            long start = Math.Min(_byteViewDragStart, _byteViewDragEnd);
            long end = Math.Max(_byteViewDragStart, _byteViewDragEnd) + view.BytesPerPixel - 1; //Include every byte of the last pixel
            if (_byteViewDragStart == _byteViewDragEnd)
            {
                //A single click just shows the offset, it doesn't change the range
                ByteViewPictureBox.Invalidate();
                return;
            }
            start = Math.Clamp(start, 0, (long)EndByteNumb.Maximum);
            end = Math.Clamp(end, start, (long)EndByteNumb.Maximum);
            //Set the end first if the new start is past the current end, so start <= end is kept
            if (start > EndByteNumb.Value)
            {
                EndByteNumb.Value = end;
                StartByteNumb.Value = start;
            }
            else
            {
                StartByteNumb.Value = start;
                EndByteNumb.Value = end;
            }
            TraceLogger.Log($"ByteView: byte range set to 0x{start:X} - 0x{end:X} ({FormatByteCount(end - start + 1)}).");
            ByteViewInfoLbl.Text = $"Range set:\n0x{start:X} - 0x{end:X}\n({FormatByteCount(end - start + 1)})";
            ByteViewPictureBox.Invalidate();
        }

        private static string FormatByteCount(long bytes) => CorruptionCore.FormatSize(bytes, out _, out _);

        /// <summary>
        /// Draws the selected byte range (while dragging) or the current start/end range over the ByteView image.
        /// </summary>
        private void ByteViewPictureBox_Paint(object? sender, PaintEventArgs e)
        {
            var view = _byteViewInfo;
            if (view == null || ByteViewPictureBox.Image == null) return;
            long a, b;
            if (_byteViewDragging)
            {
                a = Math.Min(_byteViewDragStart, _byteViewDragEnd);
                b = Math.Max(_byteViewDragStart, _byteViewDragEnd) + view.BytesPerPixel - 1;
            }
            else
            {
                if (!CorruptionCore.IsLoaded || CorruptionQueueChkbox.Checked) return;
                a = (long)StartByteNumb.Value;
                b = (long)EndByteNumb.Value;
                if (a == 0 && b >= view.FileLength - 1) return; //Whole file selected, nothing to highlight
            }

            long pixels = (long)view.Width * view.Height;
            long pa = Math.Clamp(a / view.BytesPerPixel, 0, pixels - 1);
            long pb = Math.Clamp(b / view.BytesPerPixel, pa, pixels - 1);
            long ra = pa / view.Width, rb = pb / view.Width;
            var rects = new List<Rectangle>();
            if (ra == rb)
            {
                rects.Add(new Rectangle((int)(pa % view.Width), (int)ra, (int)(pb - pa + 1), 1));
            }
            else
            {
                rects.Add(new Rectangle((int)(pa % view.Width), (int)ra, (int)(view.Width - pa % view.Width), 1));
                if (rb - ra > 1) rects.Add(new Rectangle(0, (int)ra + 1, view.Width, (int)(rb - ra - 1)));
                rects.Add(new Rectangle(0, (int)rb, (int)(pb % view.Width + 1), 1));
            }

            var area = GetByteViewImageRectangle(view);
            float sx = area.Width / view.Width, sy = area.Height / view.Height;
            using var fill = new SolidBrush(Color.FromArgb(_byteViewDragging ? 110 : 70, 255, 230, 0));
            using var outline = new Pen(Color.FromArgb(220, 255, 230, 0));
            foreach (var r in rects)
            {
                int x = view.FlipH ? view.Width - (r.X + r.Width) : r.X;
                int y = view.FlipV ? view.Height - (r.Y + r.Height) : r.Y;
                var rf = new RectangleF(area.X + x * sx, area.Y + y * sy, Math.Max(1f, r.Width * sx), Math.Max(1f, r.Height * sy));
                e.Graphics.FillRectangle(fill, rf);
                e.Graphics.DrawRectangle(outline, rf.X, rf.Y, rf.Width, rf.Height);
            }
        }

        private void ByteViewColourChkbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.ByteViewColourMode = byteViewColourChkbox.Checked;
            Settings.Default.Save();
        }

        private void ByteViewupdateWhenCorruptedChkBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.ByteViewUpdateOnCorruption = ByteViewupdateWhenCorruptedChkBox.Checked;
            Settings.Default.Save();
        }

        private void SaveLogBtn_Click(object sender, EventArgs e)
        {
            try
            {
                //write to working directory of the program
                string logFilePath = Path.Combine(Application.StartupPath, $"LRC_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(logFilePath, TraceLoggerTxtBox.Text);
                MessageBox.Show($"Log saved successfully to {logFilePath}", $"{nameof(LunarROMCorruptor)} - Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save log: {ex.Message}", $"{nameof(LunarROMCorruptor)} - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenOriginalFolderLocationBtn_Click(object sender, EventArgs e)
        {
            CommandRunner.OpenFolderInExplorer(FileSelectiontxt.Text);
        }

        private void OpenSaveFolderLocationBtn_Click(object sender, EventArgs e)
        {
            CommandRunner.OpenFolderInExplorer(SaveasTxt.Text);
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
        }

        #endregion
    }
}
