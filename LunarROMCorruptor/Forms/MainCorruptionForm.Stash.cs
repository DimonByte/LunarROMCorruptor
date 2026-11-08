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

using LunarROMCorruptor.Forms;
using LunarROMCorruptor.Modules;
using LunarROMCorruptor.Modules.CorruptionInternals;
using LunarROMCorruptor.Modules.Helpers;
using LunarROMCorruptor.Properties;

namespace LunarROMCorruptor
{
    /// <summary>
    /// Stash library, quick load, Corruption History and the Stash Editor (part of the main form).
    /// </summary>
    public partial class MainCorruptionForm
    {
        private StashMetadata? _lastCorruption; //How the last corruption was made (saved with the stash)
        private Task<StashMetadata>? _sourceInfo; //Size/CRC32/platform of the loaded file, worked out in the background when it's loaded

        private void InitializeStashBrowser()
        {
            StashBrowser.QuickLoadRequested += async entry => await CorruptFileAndStashAsync(entry.StashFile);
            StashBrowser.ApplyToLoadedRequested += entry => CorruptUsingStashFile(entry.StashFile);
            StashBrowser.EditRequested += async entry => await OpenInStashEditor(entry.StashFile);
            StashBrowser.LoadSettingsRequested += LoadStashSettings;
            StashBrowser.DetailsRequested += ShowStashDetails;
            StashBrowser.EmulatorScreenshotProvider = CaptureEmulatorScreenshot;
            StashBrowser.ByteViewImageProvider = () => _byteViewInfo == null ? null : new Bitmap(_byteViewInfo.Image);
            //Get file info from stash header, and load that into corruptioncore.
            StashBrowser.LoadFileFromStashHeader += entry =>
            {
                if(entry.Meta.SourcePath != null && File.Exists(entry.Meta.SourcePath))
                {
                    TraceLogger.Log($"Quick load from stash headers: {entry.Meta.SourcePath} -> {entry.Meta.SaveAsPath ?? CorruptionCore.SourcePath}");
                    //LoadROM from MainCorruptionForm.
                    LoadFile(entry.Meta.SourcePath).Wait();
                }
                else
                {
                    TraceLogger.Log($"The file this stash was made for isn't there anymore: {entry.Meta.SourcePath}\n\nLoad the file yourself and use \"Apply to current file\" instead.", StatusSeverityType.Warning, true);
                }
                SaveasTxt.Text = entry.Meta.SaveAsPath ?? CorruptionCore.SourcePath;
            };
        }

        /// <summary>
        /// Starts working out the size, CRC32 and platform of the file that was just loaded (used in the stash info).
        /// </summary>
        private void StartSourceInfo(string path)
        {
            _sourceInfo = Task.Run(() =>
            {
                var meta = new StashMetadata { SourcePath = path };
                try
                {
                    StashLibrary.FillSourceInfo(meta);
                }
                catch (Exception ex)
                {
                    TraceLogger.Log($"Unable to read the file information for the stash info: {ex.Message}", StatusSeverityType.Warning);
                }
                return meta;
            });
        }

        /// <summary>
        /// Remembers how a corruption is made (engine + its settings, range, intensity) so it's saved with the stash. Call on the UI thread.
        /// </summary>
        private StashMetadata DescribeCorruption(CorruptionSettings settings, string outputPath)
        {
            var frame = GetEngineFrame(settings.Engine);
            return new StashMetadata
            {
                SourcePath = CorruptionCore.SourcePath,
                SaveAsPath = outputPath,
                Engine = EngineParser.GetEngineDisplayName(settings.Engine),
                EngineSettings = frame == null ? null : EnginePresets.Capture(frame),
                StartByte = settings.StartByte,
                EndByte = settings.EndByte,
                EveryNth = settings.EveryNth,
                Intensity = settings.Intensity
            };
        }

        private async Task<StashMetadata> CompleteMetadataAsync(StashMetadata meta)
        {
            if (_sourceInfo != null && CorruptionCore.PathsEqual(meta.SourcePath, CorruptionCore.SourcePath))
            {
                try
                {
                    var info = await _sourceInfo;
                    meta.SourceSize = info.SourceSize;
                    meta.SourceLastWriteUtc = info.SourceLastWriteUtc;
                    meta.SourceCrc32 = info.SourceCrc32;
                    meta.Platform = info.Platform;
                }
                catch { }
            }
            return meta;
        }

        private static string DefaultStashName(StashMetadata meta)
        {
            string file = string.IsNullOrEmpty(meta.SourcePath) ? "Stash" : Path.GetFileNameWithoutExtension(meta.SourcePath);
            string engine = string.IsNullOrEmpty(meta.Engine) ? "" : " - " + meta.Engine.Replace(" Engine", "");
            return $"{file}{engine} - {DateTime.Now:yyyy-MM-dd HH.mm.ss}";
        }

        /// <summary>
        /// Takes a screenshot of the emulator LRC started (or another running instance of it). Null if it isn't running.
        /// </summary>
        private Bitmap? CaptureEmulatorScreenshot()
        {
            var process = CommandRunner.FindEmulatorProcess(EmulatorLocationtxt.Text);
            return process == null ? null : WindowCapture.CaptureMainWindow(process, Handle);
        }

        #region Saving stashes

        private async void TransferStash_Click(object sender, EventArgs e) => await SaveCurrentStashAsync();

        /// <summary>
        /// Saves the current corruption as a new stash in the selected group, with its info, a screenshot of the emulator (if it's
        /// running and auto screenshots are on) or the ByteView image as the thumbnail.
        /// </summary>
        private async Task SaveCurrentStashAsync()
        {
            var stash = CorruptionCore.Stash;
            if (stash.Count == 0)
            {
                TraceLogger.Log("There is no corruption to save yet. Corrupt the file first (with Enable Stash Saves on).", StatusSeverityType.Warning, true);
                return;
            }
            if (Settings.Default.LargeStashWarning && stash.Count > Settings.Default.LargeStashThreshold &&
                MessageBox.Show($"This is a large stash ({stash.Count:N0} entries) which may take a while to load in the future. Are you sure you want to save anyway?",
                    $"Warning - {nameof(LunarROMCorruptor)}", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                return;
            if (stash.Overflowed)
                TraceLogger.Log("Note: this stash reached its size limit during corruption, so it doesn't contain every change.", StatusSeverityType.Warning, true);

            var meta = _lastCorruption?.Clone() ?? new StashMetadata { SourcePath = stash.OpenLocation ?? CorruptionCore.SourcePath, SaveAsPath = stash.SaveAsLocation };
            await CompleteMetadataAsync(meta);
            meta.Name = DefaultStashName(meta);
            meta.Entries = stash.Count;
            StashEntry entry;
            try
            {
                entry = StashLibrary.Create(meta, StashBrowser.SelectedGroup, stash.WriteTo);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to save the stash: {ex.Message}", StatusSeverityType.Error, true);
                return;
            }
            AddAutomaticPreview(entry);
            StashBrowser.Reload(entry.Folder);
            TraceLogger.Log($"Stash saved as '{entry.Name}'. Rename it with F2, double-click it for details and screenshots.");
            SetStatus($"Stash saved as '{entry.Name}' (F2 renames it).", StatusKind.Success);
        }

        /// <summary>
        /// Screenshot of the running emulator (when enabled), otherwise the ByteView image as the thumbnail.
        /// </summary>
        private void AddAutomaticPreview(StashEntry entry)
        {
            try
            {
                if (StashBrowser.AutoScreenshot)
                {
                    using var shot = CaptureEmulatorScreenshot();
                    if (shot != null)
                    {
                        StashLibrary.AddScreenshot(entry, shot, makeThumbnail: true);
                        return;
                    }
                }
                if (_byteViewInfo != null && !entry.HasThumbnail && CorruptionCore.PathsEqual(CorruptionCore.LastOutputPath, entry.Meta.SaveAsPath))
                    StashLibrary.SetThumbnail(entry, _byteViewInfo.Image);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to add a preview to the stash: {ex.Message}", StatusSeverityType.Warning);
            }
        }

        #endregion

        #region Replaying stashes

        private void Corruptusingstash_Click(object sender, EventArgs e)
        {
            //Applies the selected stash to whatever file is loaded right now (use "Replay on original file" to load the stash's own file first)
            if (SelectedStashOrWarn() is { } entry) CorruptUsingStashFile(entry.StashFile);
        }

        private async void CorruptFileAndStashBtn_Click(object? sender, EventArgs e)
        {
            if (SelectedStashOrWarn() is { } entry) await CorruptFileAndStashAsync(entry.StashFile);
        }

        private StashEntry? SelectedStashOrWarn()
        {
            var entry = StashBrowser.SelectedEntry;
            if (entry == null) TraceLogger.Log("No stash selected. Please select a stash first.", StatusSeverityType.Warning, true);
            return entry;
        }

        /// <summary>
        /// Quick load: loads the file from the stash's OpenLocation header, sets the save location from its SaveAsLocation header,
        /// then applies the stash, so the result is exactly the same corruption that the stash was saved from.
        /// If the file moved, the user can point LRC to it; if it's a different version of the file, the user is warned first.
        /// </summary>
        public async Task<bool> CorruptFileAndStashAsync(string stashFileLocation)
        {
            if (_busy)
            {
                TraceLogger.Log("A corruption is still running. Wait for it to finish (or cancel it) first.", StatusSeverityType.Warning, true);
                return false;
            }
            if (!File.Exists(stashFileLocation))
            {
                TraceLogger.Log($"Stash file not found: {stashFileLocation}", StatusSeverityType.Error, true);
                return false;
            }
            string? openLocation, saveAsLocation;
            try
            {
                if (!StashParser.TryReadHeaders(stashFileLocation, out openLocation, out saveAsLocation))
                {
                    TraceLogger.Log($"This stash has no file headers (it was made before stash headers were added), so LRC doesn't know which file it belongs to.\n\nLoad the file yourself and use \"Apply to current file\" instead.", StatusSeverityType.Warning, true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to read the stash headers of {stashFileLocation}: {ex.Message}", StatusSeverityType.Error, true);
                return false;
            }
            var entry = StashLibrary.EntryForStashFile(stashFileLocation);
            if (!File.Exists(openLocation))
            {
                if (MessageBox.Show($"The file this stash was made for isn't there anymore:\n{openLocation}\n\nDo you want to find it? (The stash will remember the new location.)",
                    $"{nameof(LunarROMCorruptor)} - File not found", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return false;
                string? located = LocateMovedFile(openLocation!, stashFileLocation, entry, ref saveAsLocation);
                if (located == null) return false;
                openLocation = located;
            }
            //Warn before replacing a different loaded file
            bool replacesLoadedFile = CorruptionCore.IsLoaded && !CorruptionCore.PathsEqual(CorruptionCore.SourcePath, openLocation);
            if (replacesLoadedFile && MessageBox.Show($"This stash was made for {Path.GetFileName(openLocation)}.\n\nDo you want to load that file and corrupt using the selected stash?\n\nThe currently loaded file ({Path.GetFileName(CorruptionCore.SourcePath)}) will be replaced.",
                $"{nameof(LunarROMCorruptor)} - Replay on original file", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return false;
            //Is it still the same file? (A different version/region of a ROM gives a different corruption.)
            if (entry != null && (entry.Meta.SourceSize >= 0 || entry.Meta.SourceCrc32 != null))
            {
                UseWaitCursor = true;
                var check = await Task.Run(() => StashLibrary.CheckSource(entry.Meta, openLocation!));
                UseWaitCursor = false;
                if (check == StashLibrary.SourceCheck.Changed &&
                    MessageBox.Show($"{Path.GetFileName(openLocation)} has changed since this stash was made (different size or checksum), so the corruption may not look the same.\n\nContinue anyway?",
                        $"{nameof(LunarROMCorruptor)} - File changed", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return false;
            }
            if (CorruptionQueueChkbox.Checked)
                CorruptionQueueChkbox.Checked = false; //Stashes are single file only
            if (!CorruptionCore.IsLoaded || !CorruptionCore.PathsEqual(CorruptionCore.SourcePath, openLocation) || FileSelectiontxt.Text != CorruptionCore.SourcePath)
            {
                await LoadFile(openLocation!);
                if (!CorruptionCore.IsLoaded || !CorruptionCore.PathsEqual(CorruptionCore.SourcePath, openLocation))
                    return false; //LoadFile already told the user why
            }
            if (!string.IsNullOrWhiteSpace(saveAsLocation))
            {
                string? folder = Path.GetDirectoryName(saveAsLocation);
                if (string.IsNullOrEmpty(folder) || Directory.Exists(folder))
                {
                    if (!CorruptionCore.PathsEqual(SaveasTxt.Text, saveAsLocation)) _inPlaceWarningShown = false;
                    SaveasTxt.Text = saveAsLocation;
                }
                else
                {
                    TraceLogger.Log($"The stash's save location folder doesn't exist ({folder}), the default save location is used instead: {SaveasTxt.Text}", StatusSeverityType.Warning);
                }
            }
            //Passed all checks, now apply the stash to the loaded file and save it to the save location
            //Load settings as well, so the corruption is exactly the same as when the stash was made
            if (entry != null) LoadStashSettings(entry);
            TraceLogger.Log($"Quick load from stash headers: {openLocation} -> {SaveasTxt.Text}");
            return await CorruptUsingStashFileAsync(stashFileLocation);
        }

        /// <summary>
        /// Asks where a moved file is now and updates the stash (headers + info). Returns the new path or null.
        /// </summary>
        private string? LocateMovedFile(string oldPath, string stashFile, StashEntry? entry, ref string? saveAsLocation)
        {
            using var dialog = new OpenFileDialog { Title = $"Where is {Path.GetFileName(oldPath)} now?", FileName = Path.GetFileName(oldPath), Filter = "All files (*.*)|*.*" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return null;
            string file = dialog.FileName;
            if (!string.IsNullOrEmpty(saveAsLocation) && !Directory.Exists(Path.GetDirectoryName(saveAsLocation) ?? ""))
                saveAsLocation = Path.Combine(Path.GetDirectoryName(file) ?? "", Path.GetFileName(saveAsLocation));
            try
            {
                StashLibrary.UpdateHeaders(stashFile, file, saveAsLocation);
                if (entry != null)
                {
                    entry.Meta.SourcePath = file;
                    entry.Meta.SaveAsPath = saveAsLocation;
                    StashLibrary.SaveMeta(entry);
                    StashLibrary.RaiseChanged();
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to update the stash with the new file location: {ex.Message}", StatusSeverityType.Warning);
            }
            return file;
        }

        /// <summary>
        /// Applies a stash file to a fresh copy of the loaded file and saves it to the save location.
        /// </summary>
        public async void CorruptUsingStashFile(string StashFileLocation) => await CorruptUsingStashFileAsync(StashFileLocation);

        public async Task<bool> CorruptUsingStashFileAsync(string StashFileLocation)
        {
            if (_busy) return false;
            if (!File.Exists(StashFileLocation))
            {
                TraceLogger.Log($"Stash file not found: {StashFileLocation}", StatusSeverityType.Error, true);
                return false;
            }
            if (!ValidateSingleFileSelection()) return false;

            string outputPath = SaveasTxt.Text;
            BeginBusy("Applying stash");
            var timer = System.Diagnostics.Stopwatch.StartNew();
            //Reset exclusion ranges to avoid conflicts with the stash file
            CorruptionCore.Exclusions.Clear();
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
                return false;
            }

            //Saving this stash again (or a Stash Editor variation of it) keeps the original's info
            var entry = StashLibrary.EntryForStashFile(StashFileLocation);
            _lastCorruption = entry?.Meta.Clone() ?? new StashMetadata { Engine = "Stash" };
            if (entry != null) _lastCorruption.BasedOn = entry.Name;
            _lastCorruption.SourcePath = CorruptionCore.SourcePath;
            _lastCorruption.SaveAsPath = outputPath;

            TraceLogger.Log($"Corrupted file with stash file successfully saved: {outputPath}");
            ReportCorruption(entry != null ? $"Stash '{entry.Name}' applied" : "Stash applied", $"{CorruptionCore.Stash.Count:N0} change(s)", timer.Elapsed);
            ShowCorruptedState(outputPath);
            StartSuccessFeedback();
            RunEmulatorIfEnabled(outputPath);
            try
            {
                LoadStashFileIntoList(StashFileLocation);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to show the stash file contents: {ex.Message}", StatusSeverityType.Warning);
            }
            UpdateByteViewIfEnabled();
            GenerateSelectedByteDetails();
            return true;
        }

        private void LoadStashFileIntoList(string stashFileLocation)
        {
            StashBytesList.BeginUpdate();
            try
            {
                StashBytesList.Items.Clear();
                if (!EnableStashSavesChkbox.Checked) return;
                var lines = new List<object>();
                foreach (var item in File.ReadLines(stashFileLocation))
                {
                    if (lines.Count >= StashDisplayLimit)
                    {
                        lines.Clear();
                        lines.Add("LargeStash");
                        break;
                    }
                    lines.Add(item);
                }
                StashBytesList.Items.AddRange([.. lines]);
            }
            finally
            {
                StashBytesList.EndUpdate();
            }
        }

        /// <summary>
        /// Loads the engine settings, byte range and corruption mode/intensity a stash was made with.
        /// </summary>
        private void LoadStashSettings(StashEntry entry)
        {
            var m = entry.Meta;
            var engine = EngineParser.ParseEngineType(m.Engine ?? "");
            if (engine == CorruptionEngineType.INVALID)
            {
                TraceLogger.Log("This stash doesn't have any engine settings saved (it was made before stash info existed, or by applying a stash).", StatusSeverityType.Warning, true);
                return;
            }
            if (m.EngineSettings != null) PresetBar.ApplySettings(engine, m.EngineSettings, m.Name);
            else CorruptionEngineComboBox.Text = EngineParser.GetEngineDisplayName(engine);
            if (m.Intensity > 0)
            {
                if (m.EveryNth) CorruptEverynthByteRadioBtn.Checked = true; else RandomByteCorruptRadioBtn.Checked = true;
                if (m.Intensity > Intensity.Maximum && !AllowLargeIntensity.Checked) AllowLargeIntensity.Checked = true;
                var box = m.EveryNth ? EveryNthByte : Intensity;
                box.Value = Math.Clamp(m.Intensity, box.Minimum, box.Maximum);
            }
            if (m.EndByte >= 0 && CorruptionCore.IsLoaded)
            {
                long max = (long)EndByteNumb.Maximum;
                if (m.EndByte > max) TraceLogger.Log($"The stash's byte range ends at 0x{m.EndByte:X}, but the loaded file is smaller. The range was cut to the end of the file.", StatusSeverityType.Warning);
                EndByteNumb.Value = Math.Clamp(m.EndByte, 0, max);
                StartByteNumb.Value = Math.Clamp(m.StartByte, 0, (long)EndByteNumb.Value);
            }
            else if (m.EndByte >= 0)
            {
                TraceLogger.Log("Load a file to also use the stash's byte range.", StatusSeverityType.Information);
            }
            TraceLogger.Log($"Loaded the corruption settings of '{m.Name}'.");
        }

        private void ShowStashDetails(StashEntry entry)
        {
            //Read it again: the tree's copy can be older than the files (e.g. another details window saved it)
            var fresh = StashLibrary.TryLoadEntry(entry.Folder) ?? entry;
            var form = new StashDetailsForm(fresh) { EmulatorScreenshotProvider = CaptureEmulatorScreenshot };
            form.QuickLoadRequested += async e => await CorruptFileAndStashAsync(e.StashFile);
            form.LoadSettingsRequested += LoadStashSettings;
            form.FormClosed += (_, _) => form.Dispose();
            form.Show(this);
        }

        #endregion

        #region Stash Editor and Corruption History

        private async void StashEditorbtn_Click(object sender, EventArgs e)
        {
            if (StashBrowser.SelectedEntry is { } entry)
            {
                await OpenInStashEditor(entry.StashFile);
            }
            else await OpenInStashEditor(null);
        }

        private async Task OpenInStashEditor(string? stashFile)
        {
            //Reuse one editor window (it hides instead of closing)
            if (_stashEditor == null || _stashEditor.IsDisposed)
                _stashEditor = new StashEditor();
            //Start the load first and then show the window, so big stashes show their progress (and can be cancelled) in the editor
            //instead of the main window freezing until the whole file is read. ShowDialog keeps pumping messages, so the load carries on.
            Task? load = stashFile != null ? _stashEditor.AttemptStashLoadAsync(stashFile) : null;
            if (!_stashEditor.Visible) _stashEditor.ShowDialog(this);
            if (load != null) await load;
        }

        private void RenameStash_Click(object sender, EventArgs e) => StashBrowser.RenameSelected();

        private void RefreshStash_Click(object sender, EventArgs e) => StashBrowser.Reload();

        private void DeleteStash_Click(object sender, EventArgs e) => StashBrowser.DeleteSelected();

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
                item.DropDownItems.Add("Save to stash list", null, async (_, _) => await SaveHistoryEntryAsync(entry));
                item.DropDownItems.Add("Open in Stash Editor", null, async (_, _) => await OpenInStashEditor(entry.StashPath));
                item.Enabled = !_busy && File.Exists(entry.StashPath);
                menu.Items.Add(item);
            }
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Show all in the ⏳ History tab", null, (_, _) => MainTabControl.SelectedTab = HistoryTab);
            menu.Closed += (_, _) => BeginInvoke(menu.Dispose);
            menu.Show(CorruptionHistoryBtn, new Point(0, CorruptionHistoryBtn.Height));
        }

        private async Task SaveHistoryEntryAsync(CorruptionHistoryEntry history)
        {
            var meta = history.Meta?.Clone() ?? new StashMetadata { SourcePath = history.SourcePath, Engine = history.EngineName };
            await CompleteMetadataAsync(meta);
            meta.Name = $"{(string.IsNullOrEmpty(meta.SourcePath) ? "Stash" : Path.GetFileNameWithoutExtension(meta.SourcePath))} - {history.EngineName.Replace(" Engine", "")} - {history.Time:yyyy-MM-dd HH.mm.ss}";
            meta.Entries = history.Changes;
            try
            {
                var entry = StashLibrary.Create(meta, StashBrowser.SelectedGroup, path => File.Copy(history.StashPath, path, true));
                StashBrowser.Reload(entry.Folder);
                TraceLogger.Log($"History entry saved to the stash list as '{entry.Name}'.");
                SetStatus($"Saved to the stash list as '{entry.Name}'.", StatusKind.Success);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to save the history entry: {ex.Message}", StatusSeverityType.Error, true);
            }
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

        #endregion

        #region Bisect Mode

        private void BisectModeChkBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (BisectModeChkBox.Checked)
            {
                var screen = Screen.FromControl(this).WorkingArea;
                if (_bisectForm == null || _bisectForm.IsDisposed)
                {
                    _bisectForm = new LunarROMCorruptor.Forms.BisectForm(this);
                    _bisectForm.ClosedByUser += () => BisectModeChkBox.Checked = false;
                }
                if (!_bisectForm.Visible)
                {
                    _bisectForm.Location = new Point(Math.Clamp(Left + (Width - _bisectForm.Width) / 2, screen.Left, screen.Right - _bisectForm.Width), Math.Clamp(Top + (Height - _bisectForm.Height) / 2, screen.Top, screen.Bottom - _bisectForm.Height));
                    _bisectForm.Show(this);
                    _bisectForm.Activate();
                }
            }
            else
            {
                _bisectForm?.Hide();
            }
            InvalidateByteViewOverlays();
        }

        /// <summary>
        /// Applies one Bisect test (a subset of the stash) to the save location, then runs the emulator and refreshes the stash list and ByteView.
        /// </summary>
        internal async Task<bool> ApplyEntriesForBisectAsync(IReadOnlyList<(long Offset, byte Value)> entries, string status)
        {
            if (_busy)
            {
                TraceLogger.Log("A corruption is still running, wait for it to finish first.", StatusSeverityType.Warning, true);
                return false;
            }
            if (CorruptionQueueChkbox.Checked)
            {
                TraceLogger.Log("Bisect Mode only works with a single file, turn off Multi-File first.", StatusSeverityType.Warning, true);
                return false;
            }
            if (!ValidateSingleFileSelection()) return false;
            string outputPath = SaveasTxt.Text;
            StashBytesList.Items.Clear();
            BeginBusy(status);
            var timer = System.Diagnostics.Stopwatch.StartNew();
            CorruptionResult result;
            try
            {
                var token = _corruptionCts!.Token;
                var progress = CreateProgress(status);
                result = await Task.Run(() => CorruptionCore.ApplyByteEntries(entries, outputPath, token, progress));
            }
            finally
            {
                EndBusy();
            }
            if (result != CorruptionResult.Success)
            {
                if (result == CorruptionResult.Failed) StartFailureFeedback();
                return false;
            }
            TraceLogger.Log($"{status}: applied {CorruptionCore.Stash.ByteEntries.Count:N0} change(s) to {outputPath}.");
            ReportCorruption(status, $"{CorruptionCore.Stash.ByteEntries.Count:N0} change(s)", timer.Elapsed);
            ShowCorruptedState(outputPath);
            RunEmulatorIfEnabled(outputPath);
            await HandleStashRenderingAsync();
            UpdateByteViewIfEnabled();
            InvalidateByteViewOverlays();
            GenerateSelectedByteDetails();
            return true;
        }

        #endregion
    }
}
