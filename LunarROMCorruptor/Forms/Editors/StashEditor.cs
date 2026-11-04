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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace LunarROMCorruptor
{
    public partial class StashEditor : Form
    {
        private string? LoadedStashLocation;
        private readonly Random _rnd = new();

        //Background work (loading, sorting, mutating, saving). Only one runs at a time and the editing controls are disabled meanwhile.
        private bool _busy;
        private CancellationTokenSource? _loadCts;
        private Task _loadTask = Task.CompletedTask;

        public StashEditor()
        {
            InitializeComponent();
            stashListbox.CountChanged += (_, _) => UpdateCountLabel();
            RemovedItemslstbx.CountChanged += (_, _) => UpdateCountLabel();
            UpdateCountLabel();
        }

        private void UpdateCountLabel() => StashCountLbl.Text = $"{stashListbox.Count:N0} items ({RemovedItemslstbx.Count:N0} removed)";

        private IWin32Window? DialogOwner => Visible ? this : null;

        #region Busy state and progress

        private void BeginBusy(string status, bool cancellable)
        {
            _busy = true;
            Panel1.Enabled = false;
            ToolsPanel.Enabled = false;
            LoadStatusLbl.Text = status;
            LoadProgressBar.Style = cancellable ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            LoadProgressBar.Value = 0;
            LoadCancelBtn.Visible = cancellable;
            LoadCancelBtn.Enabled = cancellable;
            LoadCancelBtn.Text = "Cancel";
            LoadingPanel.Visible = true;
        }

        private void EndBusy()
        {
            _busy = false;
            LoadingPanel.Visible = false;
            Panel1.Enabled = true;
            ToolsPanel.Enabled = true;
            UpdateCountLabel();
        }

        /// <summary>
        /// Runs work that can't be cancelled (sort, mutate, save...) on a background thread with a marquee bar, so the window
        /// stays responsive even for millions of entries.
        /// </summary>
        private async Task<T> RunBusyAsync<T>(string status, Func<T> work)
        {
            BeginBusy(status, cancellable: false);
            try
            {
                return await Task.Run(work);
            }
            finally
            {
                EndBusy();
            }
        }

        private void LoadCancelBtn_Click(object? sender, EventArgs e) => CancelLoad();

        private void CancelLoad()
        {
            if (_loadCts == null || _loadCts.IsCancellationRequested) return;
            _loadCts.Cancel();
            LoadCancelBtn.Enabled = false;
            LoadCancelBtn.Text = "Cancelling...";
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //Escape cancels a load that's running
            if (keyData == Keys.Escape && _loadCts != null)
            {
                CancelLoad();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Loading

        private readonly record struct LoadProgress(long BytesRead, long TotalBytes, int Entries);

        /// <summary>
        /// Loads a stash file into the editor without freezing the window. Shows progress and can be cancelled (Cancel button or Esc).
        /// If it's cancelled or the file isn't valid, the list is left as it was. Starting a new load cancels one that's still running.
        /// </summary>
        public Task AttemptStashLoadAsync(string fileloc) => StartLoad(fileloc, append: false);

        private Task<bool> StartLoad(string path, bool append)
        {
            var task = LoadStashFileAsync(path, append, _loadTask);
            _loadTask = task;
            return task;
        }

        private async Task<bool> LoadStashFileAsync(string path, bool append, Task previousLoad)
        {
            //A new load replaces one that's still running
            _loadCts?.Cancel();
            try { await previousLoad; } catch { }
            if (_busy) return false; //Something else (sort, mutate...) is running, the controls that start a load are disabled anyway

            string name = StashLibrary.EntryForStashFile(path)?.Name ?? Path.GetFileName(path);
            using var cts = new CancellationTokenSource();
            _loadCts = cts;
            BeginBusy($"{(append ? "Appending" : "Loading")} {name}...", cancellable: true);
            var progress = new Progress<LoadProgress>(p =>
            {
                //Progress reports are posted to the UI thread, so a late one can arrive after this load finished
                if (_loadCts == cts) ShowLoadProgress(p, name, append);
            });
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var lines = await Task.Run(() => ReadStashFile(path, progress, cts.Token), cts.Token);
                if (append)
                {
                    stashListbox.AddLines(lines);
                    TraceLogger.Log($"Stash Editor: appended {lines.Count:N0} entries from {name} in {stopwatch.Elapsed.TotalSeconds:0.00}s.");
                }
                else
                {
                    RemovedItemslstbx.ClearLines();
                    stashListbox.SetLines(lines);
                    LoadedStashLocation = path;
                    Text = $"Stash Editor - {name}";
                    TraceLogger.Log($"Stash Editor: loaded {lines.Count:N0} entries from {name} in {stopwatch.Elapsed.TotalSeconds:0.00}s.");
                }
                return true;
            }
            catch (OperationCanceledException)
            {
                TraceLogger.Log($"Stash Editor: {(append ? "appending" : "loading")} {name} was cancelled, the list wasn't changed.");
                return false;
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show(DialogOwner, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(DialogOwner, ex.Message, "Error loading stash", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                _loadCts = null;
                EndBusy();
            }
        }

        private void ShowLoadProgress(LoadProgress p, string name, bool append)
        {
            double fraction = p.TotalBytes > 0 ? Math.Clamp((double)p.BytesRead / p.TotalBytes, 0, 1) : 0;
            LoadProgressBar.Value = (int)Math.Round(fraction * LoadProgressBar.Maximum);
            LoadStatusLbl.Text = $"{(append ? "Appending" : "Loading")} {name}... {p.Entries:N0} entries ({fraction:P0})";
        }

        /// <summary>
        /// Reads and validates a stash file on a background thread. Lines are read in batches and each batch is validated in parallel
        /// (the regex checks are the slow part), keeping the original order. Throws InvalidDataException at the first invalid line.
        /// </summary>
        private static List<string> ReadStashFile(string path, IProgress<LoadProgress> progress, CancellationToken ct)
        {
            const int BatchSize = 32_768;
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1 << 16);
            long total = stream.Length;
            //A byte entry ("[x] File(123456).SET(255)" + newline) is roughly 30 bytes; guessing low avoids reserving memory we don't need
            var lines = new List<string>((int)Math.Clamp(total / 40, 16, 50_000_000));
            var batch = new string[BatchSize];
            var batchLineNumbers = new int[BatchSize];
            int batchCount = 0;
            int lineNumber = 0;
            var sinceReport = Stopwatch.StartNew();

            string? raw;
            while ((raw = reader.ReadLine()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(raw)) continue;
                batch[batchCount] = raw.Trim(); //Trim returns the same string when there's nothing to trim
                batchLineNumbers[batchCount] = lineNumber;
                if (++batchCount == BatchSize) FlushBatch();
            }
            FlushBatch();
            progress.Report(new LoadProgress(total, total, lines.Count));
            return lines;

            void FlushBatch()
            {
                ct.ThrowIfCancellationRequested();
                if (batchCount == 0) return;
                int bad = FindFirstInvalidLine(batch, batchCount, ct);
                if (bad >= 0)
                {
                    string shown = batch[bad].Length > 120 ? batch[bad][..120] + "..." : batch[bad];
                    throw new InvalidDataException($"This file is not a valid stash file (line {batchLineNumbers[bad]:N0}: '{shown}').");
                }
                lines.AddRange(new ArraySegment<string>(batch, 0, batchCount));
                batchCount = 0;
                if (sinceReport.ElapsedMilliseconds >= 50)
                {
                    sinceReport.Restart();
                    progress.Report(new LoadProgress(stream.Position, total, lines.Count));
                }
            }
        }

        /// <summary>Index of the first invalid line in the batch, or -1 if every line is valid.</summary>
        private static int FindFirstInvalidLine(string[] batch, int count, CancellationToken ct)
        {
            int firstBad = int.MaxValue;
            var options = new ParallelOptions { CancellationToken = ct };
            Parallel.ForEach(Partitioner.Create(0, count, 2048), options, range =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    if (i >= Volatile.Read(ref firstBad)) return; //An earlier bad line was already found
                    if (!StashParser.IsValidStashLine(batch[i]))
                    {
                        int seen;
                        while (i < (seen = Volatile.Read(ref firstBad)) && Interlocked.CompareExchange(ref firstBad, i, seen) != seen) { }
                        return;
                    }
                }
            });
            return firstBad == int.MaxValue ? -1 : firstBad;
        }

        private async void Openfilebtn_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(IOManager.CorruptionStashDirectory);
            OpenFileDialog.InitialDirectory = IOManager.CorruptionStashDirectory;
            if (OpenFileDialog.ShowDialog(this) != DialogResult.Cancel)
            {
                await StartLoad(OpenFileDialog.FileName, append: false);
            }
        }

        private async void AppendStashBtn_Click(object? sender, EventArgs e)
        {
            Directory.CreateDirectory(IOManager.CorruptionStashDirectory);
            OpenFileDialog.InitialDirectory = IOManager.CorruptionStashDirectory;
            if (OpenFileDialog.ShowDialog(this) == DialogResult.Cancel) return;
            await StartLoad(OpenFileDialog.FileName, append: true);
        }

        private async void Restorebtn_Click(object sender, EventArgs e)
        {
            if (LoadedStashLocation == null || !File.Exists(LoadedStashLocation))
            {
                MessageBox.Show("No stash file is loaded.", $"Info - {nameof(LunarROMCorruptor)} ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            await StartLoad(LoadedStashLocation, append: false);
        }

        #endregion

        #region List tools (mutate, sort, dedupe)

        private StashTools.MutateMode SelectedMutateMode => MutateModeComboBox.SelectedIndex switch
        {
            1 => StashTools.MutateMode.NudgeValue,
            2 => StashTools.MutateMode.MoveAddress,
            _ => StashTools.MutateMode.RandomValue
        };

        /// <summary>
        /// Mutates the list. Returns false if there was nothing to mutate.
        /// </summary>
        private async Task<bool> MutateListAsync()
        {
            var lines = stashListbox.Lines;
            if (lines.Count == 0)
            {
                MessageBox.Show(this, "The stash list is empty. Load a stash first.", "Mutate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            //Read the settings on the UI thread, the mutation itself runs in the background
            double percent = (double)MutatePercentNumericUpDown.Value;
            var mode = SelectedMutateMode;
            long amount = (long)MutateAmountNumericUpDown.Value;
            long maxOffset = CorruptionCore.IsLoaded ? CorruptionCore.MaxByte : long.MaxValue;
            string modeName = MutateModeComboBox.Text;
            int changed = 0;
            var mutated = await RunBusyAsync($"Mutating {lines.Count:N0} entries...",
                () => StashTools.Mutate(lines, percent, mode, amount, maxOffset, _rnd, out changed));
            if (changed == 0)
            {
                MessageBox.Show(this, "Nothing was mutated. Only byte entries ([x] File(i).SET(v)) can be mutated.", "Mutate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            stashListbox.SetLines(mutated);
            TraceLogger.Log($"Stash Editor: mutated {changed:N0} of {lines.Count:N0} stash entries ({modeName}).");
            return true;
        }

        private async void MutateBtn_Click(object? sender, EventArgs e)
        {
            await MutateListAsync();
        }

        private async void MutateCorruptBtn_Click(object? sender, EventArgs e)
        {
            //The mutation is saved as a new stash file so the original corruption is kept
            if (Program.Form.CorruptionQueueChkbox.Checked)
            {
                MessageBox.Show("Corruption queue is enabled. Please disable it and select a single file before corrupting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!await MutateListAsync()) return;
            string? path = await SaveNewStashFileAsync("mutation");
            if (path != null) Program.Form.CorruptUsingStashFile(path);
        }

        private async void SortStashBtn_Click(object? sender, EventArgs e)
        {
            var lines = stashListbox.Lines;
            if (lines.Count == 0) return;
            var sorted = await RunBusyAsync($"Sorting {lines.Count:N0} entries...", () => StashTools.SortByAddress(lines));
            stashListbox.SetLines(sorted);
        }

        private async void DedupeStashBtn_Click(object? sender, EventArgs e)
        {
            var lines = stashListbox.Lines;
            if (lines.Count == 0) return;
            int removed = 0;
            var result = await RunBusyAsync($"Removing duplicates from {lines.Count:N0} entries...", () => StashTools.RemoveDuplicates(lines, out removed));
            stashListbox.SetLines(result);
            MessageBox.Show(this, $"Removed {removed:N0} entries that were overwritten by a later entry for the same address.", "Remove duplicates", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Editing the list

        private void Removeselbtn_Click(object sender, EventArgs e)
        {
            var taken = stashListbox.TakeSelected();
            if (taken.Count > 0) RemovedItemslstbx.AddLines(taken);
        }

        private void Remove50btn_Click(object sender, EventArgs e)
        {
            //Moves the first half of the stash to the removed list
            int half = (stashListbox.Count + 1) / 2;
            if (half == 0) return;
            RemovedItemslstbx.AddLines(stashListbox.TakeFirst(half));
        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            stashListbox.AddLine(StashParser.FormatByte((long)LocationStash.Value, (byte)valueStashnum.Value));
            stashListbox.EnsureVisible(stashListbox.Count - 1);
        }

        private void Additemsbtn_Click(object sender, EventArgs e)
        {
            //Moves the selected removed items back to the end of the stash list
            var taken = RemovedItemslstbx.TakeSelected();
            if (taken.Count > 0) stashListbox.AddLines(taken);
        }

        private void Removeitembtn_Click(object sender, EventArgs e)
        {
            //Permanently deletes the selected removed items
            RemovedItemslstbx.TakeSelected();
        }

        private void RemoveAllbtn_Click(object sender, EventArgs e)
        {
            //Remove ALL items from the stash list
            stashListbox.ClearLines();
        }

        #endregion

        #region Saving

        private async void Newfilestashbtn_Click(object sender, EventArgs e)
        {
            await SaveNewStashFileAsync();
        }

        /// <summary>
        /// Saves the list as a new stash in the stash library. A variation of a library stash keeps its info (file, engine, settings,
        /// group) and remembers which stash it was based on. Returns the new stash file, or null if it couldn't be saved.
        /// </summary>
        private async Task<string?> SaveNewStashFileAsync(string suffix = "edited")
        {
            var original = StashLibrary.EntryForStashFile(LoadedStashLocation);
            var meta = original?.Meta.Clone() ?? (LoadedStashLocation != null && File.Exists(LoadedStashLocation) ? StashLibrary.MetadataFromStashFile(LoadedStashLocation) : new StashMetadata());
            meta.BasedOn = original?.Name ?? meta.BasedOn;
            meta.Name = string.IsNullOrEmpty(original?.Name) ? $"Stash - {DateTime.Now:yyyy-MM-dd HH.mm.ss}" : $"{original.Name} ({suffix})";
            meta.Rating = 0;
            meta.Entries = 0;
            string group = original?.Group ?? "";
            var lines = stashListbox.Lines;
            try
            {
                var entry = await RunBusyAsync($"Saving {lines.Count:N0} entries...", () => StashLibrary.CreateFromLines(meta, group, lines));
                StashLibrary.RaiseChanged();
                TraceLogger.Log($"{entry.StashFile} - Saved to a brand new stash ({entry.Name}).");
                LoadedStashLocation = entry.StashFile;
                Text = $"Stash Editor - {entry.Name}";
                return entry.StashFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Unable to save the stash: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private static void WriteStash(string path, IReadOnlyList<string> lines)
        {
            using var writer = new StreamWriter(path, false, new UTF8Encoding(false), 1 << 16);
            for (int i = 0; i < lines.Count; i++)
            {
                writer.WriteLine(lines[i]);
            }
        }

        private async Task<bool> SaveToExistingStashFileAsync()
        {
            //Check if a stash file location is loaded first.
            if (LoadedStashLocation == null)
            {
                MessageBox.Show(this, "No stash file is loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            //Check if corruption queue is enabled on the main form, if it is cancel the operation.
            if (Program.Form.CorruptionQueueChkbox.Checked)
            {
                MessageBox.Show(this, "Corruption queue is enabled. Please disable it and select a single file before corrupting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            string path = LoadedStashLocation;
            var lines = stashListbox.Lines;
            try
            {
                await RunBusyAsync($"Saving {lines.Count:N0} entries...", () =>
                {
                    WriteStash(path, lines);
                    StashLibrary.StashFileChanged(path);
                    return true;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Unable to save the stash: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            StashLibrary.RaiseChanged();
            TraceLogger.Log(path + " - Saved to existing stash file.");
            return true;
        }

        private async void SaveAndCorruptbtn_Click(object sender, EventArgs e)
        {
            if (!await SaveToExistingStashFileAsync() || LoadedStashLocation == null) return;
            Program.Form.CorruptUsingStashFile(LoadedStashLocation);
        }

        #endregion

        private void StashEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Closing while loading cancels the load (the window is only hidden, it's reused next time)
            _loadCts?.Cancel();
            e.Cancel = true;
            Hide();
        }
    }
}
