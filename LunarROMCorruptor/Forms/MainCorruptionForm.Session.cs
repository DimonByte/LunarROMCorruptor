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
using LunarROMCorruptor.Properties;
using System.Diagnostics;

namespace LunarROMCorruptor
{
    /// <summary>
    /// Keyboard shortcuts, remembering the session (window, tab, file, engine, recent files), the status bar, the ⚙️ Settings tab,
    /// the ⏳ History tab and the File Saves list details (part of the main form).
    /// </summary>
    public partial class MainCorruptionForm
    {
        private const int MaxRecentFiles = 10;
        private bool _sessionRestored; //Layout events before the session is restored aren't remembered
        private bool _loadingSettingsUi;
        private readonly Dictionary<string, (DateTime Read, DateTime Modified, long Size)> _fileSaveInfo = new(StringComparer.OrdinalIgnoreCase);

        private enum StatusKind { Info, Success, Warning }

        private void InitializeSession()
        {
            RestoreWindowBounds();
            Shown += MainCorruptionForm_Shown;

            //File Saves: show the date and size of every save, double-click puts it back at the save location
            FilesaveList.DrawMode = DrawMode.OwnerDrawFixed;
            FilesaveList.ItemHeight = 20;
            FilesaveList.HorizontalScrollbar = false;
            FilesaveList.DrawItem += FilesaveList_DrawItem;
            FilesaveList.DoubleClick += FilesaveList_DoubleClick;
            ToolTipHelper.SetToolTip(FilesaveList, "Double-click a save to copy it to the save location (and run the emulator if 'Run after corruption' is on).");

            //History tab
            CorruptionHistory.Changed += OnHistoryChanged;
            FormClosed += (_, _) => CorruptionHistory.Changed -= OnHistoryChanged;
            SessionHistoryView.ReapplyRequested += ReapplyHistoryEntry;
            SessionHistoryView.SaveRequested += async entry => await SaveHistoryEntryAsync(entry);
            SessionHistoryView.EditRequested += async entry => await OpenInStashEditor(entry.StashPath);

            StashBrowser.StatusRequested += message => SetStatus(message, StatusKind.Success);

            //Shortcut hints
            ToolTipHelper.SetToolTip(CorruptButton, "Corrupt (F5 or Ctrl+Enter). Click again while it runs to cancel.");
            ToolTipHelper.SetToolTip(Restorefilebtn, "Restore the corrupted file to the original (Ctrl+R)");
            ToolTipHelper.SetToolTip(StartEmulatorNowBtn, "Start the emulator program with the corrupted file (F6)");
            ToolTipHelper.SetToolTip(TransferStash, "Save the last corruption as a stash in the selected group (Ctrl+S)");
            UpdateFileStatus();
        }

        #region Keyboard shortcuts

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F5:
                case Keys.Control | Keys.Enter:
                    CorruptButton.PerformClick(); //Also cancels a running corruption, like clicking the button
                    return true;
                case Keys.Control | Keys.R:
                    if (!_busy) Restorefilebtn.PerformClick();
                    return true;
                case Keys.F6:
                    if (!_busy) StartEmulatorNowBtn.PerformClick();
                    return true;
                case Keys.Control | Keys.O:
                    if (!_busy) Openfilebtn.PerformClick();
                    return true;
                case Keys.Control | Keys.S:
                    if (_busy) return true;
                    if (TransferStash.Enabled) _ = SaveCurrentStashAsync();
                    else SetStatus("Turn on Enable Stash Saves (Target Parameters) to save corruptions as stashes.", StatusKind.Warning);
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Remembering the session

        private void RestoreWindowBounds()
        {
            var parts = Settings.Default.WindowBounds.Split(',');
            if (parts.Length != 4 || !parts.All(p => int.TryParse(p, out _))) return;
            var bounds = new Rectangle(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
            bounds.Width = Math.Max(bounds.Width, MinimumSize.Width);
            bounds.Height = Math.Max(bounds.Height, MinimumSize.Height);
            //Only when the title bar is on a screen that is still connected
            var titleBar = new Rectangle(bounds.X + 40, bounds.Y, Math.Max(1, bounds.Width - 80), 30);
            if (!Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(titleBar))) return;
            StartPosition = FormStartPosition.Manual;
            Bounds = bounds;
            if (Settings.Default.WindowMaximized) WindowState = FormWindowState.Maximized;
        }

        /// <summary>
        /// Puts the tab, engine and stash area splitter back the way they were. Called at the end of loading.
        /// </summary>
        private void RestoreSessionLayout()
        {
            var engine = EngineParser.ParseEngineType(Settings.Default.LastEngine);
            if (engine != CorruptionEngineType.INVALID)
                CorruptionEngineComboBox.Text = EngineParser.GetEngineDisplayName(engine);
            int distance = Settings.Default.TargetSplitterDistance;
            if (distance > 0)
            {
                int max = TargetSplit.Width - TargetSplit.Panel2MinSize - TargetSplit.SplitterWidth;
                if (max >= TargetSplit.Panel1MinSize)
                {
                    try { TargetSplit.SplitterDistance = Math.Clamp(distance, TargetSplit.Panel1MinSize, max); }
                    catch (InvalidOperationException) { }
                }
            }
            //int tab = Settings.Default.LastTabIndex;
            //if (tab >= 0 && tab < MainTabControl.TabCount) MainTabControl.SelectedIndex = tab;
            int area = Settings.Default.StashAreaTabIndex;
            if (area >= 0 && area < StashAreaTabs.TabCount) StashAreaTabs.SelectedIndex = area;
            _sessionRestored = true;
        }

        /// <summary>
        /// Stashes or File Saves, next to the target parameters.
        /// </summary>
        private void StashAreaTabs_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_sessionRestored) Settings.Default.StashAreaTabIndex = StashAreaTabs.SelectedIndex; //Saved when LRC closes
        }

        private void MainCorruptionForm_Shown(object? sender, EventArgs e)
        {
            if (IsDisposed || !Settings.Default.ReopenLastFile) return;
            string last = Settings.Default.LastFile;
            //if (!string.IsNullOrEmpty(last) && File.Exists(last) && !CorruptionCore.IsLoaded)
            //{
            //    TraceLogger.Log($"Reopening the last file: {last}");
            //    LoadFile(last).GetAwaiter();
            //}
        }

        private void TargetSplit_SplitterMoved(object? sender, SplitterEventArgs e)
        {
            if (_sessionRestored) Settings.Default.TargetSplitterDistance = TargetSplit.SplitterDistance; //Saved when LRC closes
        }

        /// <summary>
        /// Remembers the window, tab and engine. Called when LRC closes.
        /// </summary>
        private void SaveSession()
        {
            try
            {
                var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                Settings.Default.WindowBounds = $"{bounds.X},{bounds.Y},{bounds.Width},{bounds.Height}";
                Settings.Default.WindowMaximized = WindowState == FormWindowState.Maximized;
                Settings.Default.LastTabIndex = MainTabControl.SelectedIndex;
                if (SelectedEngine != CorruptionEngineType.INVALID) Settings.Default.LastEngine = CorruptionEngineComboBox.Text;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to save the window layout: {ex.Message}", StatusSeverityType.Warning);
            }
        }

        private static List<string> RecentFiles() =>
            [.. Settings.Default.RecentFiles.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];

        /// <summary>
        /// Puts a file at the top of the recent files and remembers it as the last file.
        /// </summary>
        private static void AddRecentFile(string path)
        {
            var files = RecentFiles();
            files.RemoveAll(f => CorruptionCore.PathsEqual(f, path));
            files.Insert(0, path);
            if (files.Count > MaxRecentFiles) files.RemoveRange(MaxRecentFiles, files.Count - MaxRecentFiles);
            Settings.Default.RecentFiles = string.Join('\n', files);
            Settings.Default.LastFile = path;
            Settings.Default.Save();
        }

        private void RecentFilesBtn_Click(object? sender, EventArgs e)
        {
            RecentFilesMenu.Items.Clear();
            var files = RecentFiles();
            if (files.Count == 0)
                RecentFilesMenu.Items.Add(new ToolStripMenuItem("No recent files yet") { Enabled = false });
            for (int k = 0; k < files.Count; k++)
            {
                string file = files[k];
                bool exists = File.Exists(file);
                var item = new ToolStripMenuItem($"{(k < 9 ? "&" : "")}{k + 1}  {Path.GetFileName(file)}{(exists ? "" : "  (missing)")}")
                {
                    ToolTipText = file,
                    ShortcutKeyDisplayString = Path.GetFileName(Path.GetDirectoryName(file)),
                    Enabled = exists && !_busy
                };
                if (CorruptionCore.PathsEqual(file, CorruptionCore.SourcePath)) item.Checked = true;
                item.Click += (_, _) => OpenRecentFile(file);
                RecentFilesMenu.Items.Add(item);
            }
            if (files.Count > 0)
            {
                RecentFilesMenu.Items.Add(new ToolStripSeparator());
                RecentFilesMenu.Items.Add("Remove missing files", null, (_, _) =>
                {
                    Settings.Default.RecentFiles = string.Join('\n', RecentFiles().Where(File.Exists));
                    Settings.Default.Save();
                });
                RecentFilesMenu.Items.Add("Clear recent files", null, (_, _) =>
                {
                    Settings.Default.RecentFiles = "";
                    Settings.Default.Save();
                });
            }
            RecentFilesMenu.ShowItemToolTips = true;
            RecentFilesMenu.Show(RecentFilesBtn, new Point(0, RecentFilesBtn.Height));
        }

        private void OpenRecentFile(string file)
        {
            if (_busy) return;
            if (CorruptionQueueChkbox.Checked) CorruptionQueueChkbox.Checked = false; //A recent file is a single file
            LoadFile(file).GetAwaiter();
        }

        #endregion

        #region Status bar

        private void SetStatus(string message, StatusKind kind = StatusKind.Info)
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(() => SetStatus(message, kind)); return; }
            StatusMessageLbl.Text = message.Replace('\n', ' ');
            StatusMessageLbl.ToolTipText = message;
            StatusMessageLbl.ForeColor = kind switch
            {
                StatusKind.Success => Color.FromArgb(140, 230, 160),
                StatusKind.Warning => Color.FromArgb(255, 210, 110),
                _ => Color.FromArgb(213, 216, 216)
            };
            StatusTimer.Stop();
            StatusTimer.Start();
        }

        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            StatusTimer.Stop();
            StatusMessageLbl.ForeColor = Color.FromArgb(135, 140, 175); //Old messages fade, but stay readable
        }

        /// <summary>
        /// The file part of the status bar: the loaded file with its type and size, or the queue.
        /// </summary>
        private void UpdateFileStatus()
        {
            if (CorruptionQueueChkbox.Checked)
            {
                int count = CorruptionQueueFormSettings.CorruptionQueueList.Items.Count;
                StatusFileLbl.Text = $"Multi-File: {count} file(s)";
                StatusFileLbl.ToolTipText = "Corruption queue";
                return;
            }
            if (!CorruptionCore.IsLoaded || CorruptionCore.SourcePath == null)
            {
                StatusFileLbl.Text = "No file loaded";
                StatusFileLbl.ToolTipText = "Open a file (Ctrl+O) or drop one on the window";
                return;
            }
            string path = CorruptionCore.SourcePath;
            string text = $"{Path.GetFileName(path)} · {CorruptionCore.rawFileSize} {CorruptionCore.fileSizeUnit}";
            StatusFileLbl.Text = text;
            StatusFileLbl.ToolTipText = path;
            //The file type is worked out in the background when the file is loaded
            var info = _sourceInfo;
            if (info == null) return;
            info.ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully && !string.IsNullOrEmpty(t.Result.Platform) && !IsDisposed && IsHandleCreated)
                    BeginInvoke(() =>
                    {
                        if (CorruptionCore.PathsEqual(CorruptionCore.SourcePath, path) && !CorruptionQueueChkbox.Checked)
                            StatusFileLbl.Text = $"{Path.GetFileName(path)} · {t.Result.Platform} · {CorruptionCore.rawFileSize} {CorruptionCore.fileSizeUnit}";
                    });
            }, TaskScheduler.Default);
        }

        /// <summary>
        /// The state part of the status bar: is the save location corrupted, restored or the original?
        /// </summary>
        private void UpdateStateStatus(string text, string? tooltip = null, bool warning = false)
        {
            StatusStateLbl.Text = text;
            StatusStateLbl.ToolTipText = tooltip ?? text;
            StatusStateLbl.ForeColor = warning ? Color.FromArgb(255, 210, 110) : Color.FromArgb(213, 216, 216);
        }

        private void ShowCorruptedState(string outputPath)
        {
            if (CorruptionCore.IsInPlace(outputPath))
                UpdateStateStatus("⚠ Original corrupted in place", $"{outputPath} itself is corrupted. Restore File (Ctrl+R) undoes it while LRC is open.", true);
            else
                UpdateStateStatus($"Corrupted → {Path.GetFileName(outputPath)}", outputPath);
        }

        private static string FormatElapsed(TimeSpan time) =>
            time.TotalSeconds < 10 ? $"{time.TotalSeconds:0.00} s" : time.TotalMinutes < 1 ? $"{time.TotalSeconds:0.0} s" : $"{(int)time.TotalMinutes}m {time.Seconds:00}s";

        /// <summary>
        /// Shows what the last corruption did and how long it took.
        /// </summary>
        private void ReportCorruption(string what, string changes, TimeSpan elapsed)
        {
            StatusLastLbl.Text = $"Last: {changes} in {FormatElapsed(elapsed)}";
            StatusLastLbl.ToolTipText = $"{what} at {DateTime.Now:HH:mm:ss}: {changes} in {FormatElapsed(elapsed)}";
            SetStatus($"{what}: {changes} in {FormatElapsed(elapsed)}.", StatusKind.Success);
        }

        #endregion

        #region Settings tab

        private void LoadSettingsTab()
        {
            _loadingSettingsUi = true;
            try
            {
                ReopenLastFileChk.Checked = Settings.Default.ReopenLastFile;
                AutoScreenshotChk.Checked = Settings.Default.StashAutoScreenshot;
                LargeStashWarningChk.Checked = Settings.Default.LargeStashWarning;
                LargeStashThresholdNUD.Value = Math.Clamp(Settings.Default.LargeStashThreshold, LargeStashThresholdNUD.Minimum, LargeStashThresholdNUD.Maximum);
                LargeStashThresholdNUD.Enabled = LargeStashWarningChk.Checked;
                DefaultArgsTxt.Text = Settings.Default.DefaultOverrideArguments;
                UseDefaultArgsChk.Checked = Settings.Default.UseDefaultOverrideArguments;
                LoadStashEmulatorChk.Checked = Settings.Default.LoadStashEmulator;
                //The emulator defaults fill in Run Emulator Program
                if (!string.IsNullOrEmpty(Settings.Default.DefaultOverrideArguments)) OverrideArguments.Text = Settings.Default.DefaultOverrideArguments;
                OverrideArgumentschbox.Checked = Settings.Default.UseDefaultOverrideArguments;
                OverrideArguments.Enabled = OverrideArgumentschbox.Checked;
            }
            finally
            {
                _loadingSettingsUi = false;
            }
        }

        private void SaveSetting(Action change)
        {
            if (_loadingSettingsUi) return;
            change();
            Settings.Default.Save();
        }

        private void ReopenLastFileChk_CheckedChanged(object? sender, EventArgs e) => SaveSetting(() => Settings.Default.ReopenLastFile = ReopenLastFileChk.Checked);

        private void AutoScreenshotChk_CheckedChanged(object? sender, EventArgs e) => SaveSetting(() => Settings.Default.StashAutoScreenshot = AutoScreenshotChk.Checked);

        private void LargeStashWarningChk_CheckedChanged(object? sender, EventArgs e)
        {
            LargeStashThresholdNUD.Enabled = LargeStashWarningChk.Checked;
            SaveSetting(() => Settings.Default.LargeStashWarning = LargeStashWarningChk.Checked);
        }

        private void LargeStashThresholdNUD_ValueChanged(object? sender, EventArgs e) => SaveSetting(() => Settings.Default.LargeStashThreshold = (int)LargeStashThresholdNUD.Value);

        private void DefaultArgsTxt_TextChanged(object? sender, EventArgs e)
        {
            if (!_loadingSettingsUi) Settings.Default.DefaultOverrideArguments = DefaultArgsTxt.Text; //Saved when LRC closes
        }

        private void UseDefaultArgsChk_CheckedChanged(object? sender, EventArgs e) => SaveSetting(() => Settings.Default.UseDefaultOverrideArguments = UseDefaultArgsChk.Checked);

        private void LoadStashEmulatorChk_CheckedChanged(object? sender, EventArgs e) => SaveSetting(() => Settings.Default.LoadStashEmulator = LoadStashEmulatorChk.Checked);

        #endregion

        #region History tab

        private void OnHistoryChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(SessionHistoryView.RefreshList);
        }

        #endregion

        #region File Saves details

        private (DateTime Modified, long Size)? FileSaveInfo(string name)
        {
            if (_fileSaveInfo.TryGetValue(name, out var cached) && (DateTime.Now - cached.Read).TotalSeconds < 5)
                return cached.Size < 0 ? null : (cached.Modified, cached.Size);
            DateTime modified = default;
            long size = -1;
            try
            {
                var file = new FileInfo(Path.Combine(IOManager.SavesDirectory, name));
                if (file.Exists) { modified = file.LastWriteTime; size = file.Length; }
            }
            catch { }
            _fileSaveInfo[name] = (DateTime.Now, modified, size);
            return size < 0 ? null : (modified, size);
        }

        private void FilesaveList_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= FilesaveList.Items.Count) return;
            bool selected = (e.State & DrawItemState.Selected) != 0;
            using (var back = new SolidBrush(selected ? Color.FromArgb(70, 85, 160) : FilesaveList.BackColor)) e.Graphics.FillRectangle(back, e.Bounds);
            string name = FilesaveList.GetItemText(FilesaveList.Items[e.Index]) ?? "";
            string details = FileSaveInfo(name) is { } info ? $"{info.Modified:yyyy-MM-dd HH:mm} · {FormatByteCount(info.Size)}" : "missing";
            var detailsSize = TextRenderer.MeasureText(e.Graphics, details, e.Font ?? Font);
            int right = e.Bounds.Right - 6;
            var nameRect = new Rectangle(e.Bounds.X + 4, e.Bounds.Y, Math.Max(10, right - detailsSize.Width - 12 - e.Bounds.X), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, name, e.Font ?? Font, nameRect, FilesaveList.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(e.Graphics, details, e.Font ?? Font, new Rectangle(right - detailsSize.Width, e.Bounds.Y, detailsSize.Width, e.Bounds.Height),
                selected ? Color.FromArgb(185, 190, 225) : Color.FromArgb(135, 140, 175), TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        private void FilesaveList_DoubleClick(object? sender, EventArgs e)
        {
            if (_busy || FilesaveList.SelectedItem == null) return;
            string target = SaveasTxt.Text;
            if (!IOManager.CopySaveToCorruptedFile(target, FilesaveList)) return;
            SetStatus($"Copied {FilesaveList.GetItemText(FilesaveList.SelectedItem)} to {Path.GetFileName(target)}.", StatusKind.Success);
            UpdateStateStatus($"File save → {Path.GetFileName(target)}", target);
            RunEmulatorIfEnabled(target);
            UpdateByteViewIfEnabled();
        }

        #endregion
    }
}
