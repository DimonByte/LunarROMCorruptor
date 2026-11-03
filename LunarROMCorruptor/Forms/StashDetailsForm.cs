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
using LunarROMCorruptor.Modules.Helpers;
using System.ComponentModel;

namespace LunarROMCorruptor.Forms
{
    /// <summary>
    /// Shows and edits everything about one stash: screenshots (+ which one is the thumbnail), name, rating, tags, notes and the info
    /// recorded when it was saved (file, checksum, engine, range...). Also relinks a stash to its file when the file has moved.
    /// </summary>
    public partial class StashDetailsForm : Form
    {
        private readonly StashEntry _entry;
        private bool _dirty;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] //Set in code, the designer must not serialize it
        public Func<Bitmap?>? EmulatorScreenshotProvider { get; set; }
        public event Action<StashEntry>? QuickLoadRequested;
        public event Action<StashEntry>? LoadSettingsRequested;

        public StashDetailsForm(StashEntry entry)
        {
            InitializeComponent();
            _entry = entry;
            RatingComboBox.Items.Add("No rating");
            for (int stars = 1; stars <= 5; stars++) RatingComboBox.Items.Add(new string('★', stars) + new string('☆', 5 - stars));
            LoadEntry();
            NameTxt.TextChanged += (_, _) => _dirty = true;
            TagsTxt.TextChanged += (_, _) => _dirty = true;
            NotesTxt.TextChanged += (_, _) => _dirty = true;
            RatingComboBox.SelectedIndexChanged += (_, _) => _dirty = true;
            FormClosing += StashDetailsForm_FormClosing;
            _dirty = false;
        }

        private void LoadEntry()
        {
            var m = _entry.Meta;
            Text = $"Stash details - {m.Name}";
            NameTxt.Text = m.Name;
            RatingComboBox.SelectedIndex = Math.Clamp(m.Rating, 0, 5);
            TagsTxt.Text = string.Join(", ", m.Tags);
            NotesTxt.Text = m.Notes.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            InfoTxt.Text = BuildInfo();
            LoadSettingsBtn.Enabled = !string.IsNullOrEmpty(m.Engine);
            QuickLoadBtn.Enabled = !string.IsNullOrEmpty(m.SourcePath);
            LoadScreenshots();
        }

        private string BuildInfo()
        {
            var m = _entry.Meta;
            var lines = new List<string>();
            bool missing = !string.IsNullOrEmpty(m.SourcePath) && !File.Exists(m.SourcePath);
            lines.Add($"File: {(string.IsNullOrEmpty(m.SourcePath) ? "(unknown, made before stash headers)" : m.SourcePath)}{(missing ? "   ⚠ not found" : "")}");
            if (m.SourceSize >= 0) lines.Add($"Size: {CorruptionCore.FormatSize(m.SourceSize, out _, out _)} ({m.SourceSize:N0} bytes)");
            if (m.SourceCrc32 != null) lines.Add($"CRC32: {m.SourceCrc32}");
            if (!string.IsNullOrEmpty(m.Platform)) lines.Add($"Type: {m.Platform}");
            if (!string.IsNullOrEmpty(m.SaveAsPath)) lines.Add($"Saved to: {m.SaveAsPath}");
            lines.Add("");
            if (!string.IsNullOrEmpty(m.Engine)) lines.Add($"Engine: {m.Engine}{(m.EngineSettings is { Count: > 0 } ? $" ({m.EngineSettings.Count} settings saved)" : "")}");
            if (m.EndByte >= 0) lines.Add($"Range: 0x{m.StartByte:X} - 0x{m.EndByte:X}");
            if (m.Intensity > 0) lines.Add(m.EveryNth ? $"Every {m.Intensity:N0}th byte" : $"Intensity: {m.Intensity:N0}");
            lines.Add($"Changes: {m.Entries:N0}");
            if (!string.IsNullOrEmpty(m.BasedOn)) lines.Add($"Based on: {m.BasedOn}");
            lines.Add("");
            lines.Add($"Saved: {m.Created:yyyy-MM-dd HH:mm:ss}");
            lines.Add($"Changed: {m.Modified:yyyy-MM-dd HH:mm:ss}");
            if (!string.IsNullOrEmpty(m.LrcVersion)) lines.Add($"LRC version: {m.LrcVersion}");
            lines.Add($"Folder: {_entry.Folder}");
            return string.Join(Environment.NewLine, lines);
        }

        #region Screenshots

        private void ClearScreenshotImages()
        {
            foreach (Image image in ScreenshotImages.Images) image.Dispose();
            ScreenshotImages.Images.Clear();
        }

        private void LoadScreenshots(string? select = null)
        {
            ScreenshotList.BeginUpdate();
            ScreenshotList.Items.Clear();
            ClearScreenshotImages();
            foreach (string path in _entry.ScreenshotPaths)
            {
                using var image = StashLibrary.LoadImageUnlocked(path);
                if (image == null) continue;
                using var thumb = StashLibrary.MakeThumbnail(image, 96);
                ScreenshotImages.Images.Add(path, new Bitmap(thumb));
                var item = new ListViewItem(Path.GetFileNameWithoutExtension(path).Replace("screenshot-", ""), path) { Tag = path };
                ScreenshotList.Items.Add(item);
                if (select != null && CorruptionCore.PathsEqual(select, path)) item.Selected = true;
            }
            ScreenshotList.EndUpdate();
            if (ScreenshotList.SelectedItems.Count == 0 && ScreenshotList.Items.Count > 0)
                ScreenshotList.Items[^1].Selected = true; //Newest
            if (ScreenshotList.Items.Count == 0) ShowPreview(_entry.HasThumbnail ? _entry.ThumbnailFile : null);
            UpdateScreenshotButtons();
        }

        private string? SelectedScreenshot => ScreenshotList.SelectedItems.Count > 0 ? ScreenshotList.SelectedItems[0].Tag as string : null;

        private void ShowPreview(string? path)
        {
            var old = PreviewBox.Image;
            PreviewBox.Image = path == null ? null : StashLibrary.LoadImageUnlocked(path);
            old?.Dispose();
        }

        private void UpdateScreenshotButtons()
        {
            ThumbnailBtn.Enabled = RemoveShotBtn.Enabled = SelectedScreenshot != null;
        }

        private void ScreenshotList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (SelectedScreenshot != null) ShowPreview(SelectedScreenshot);
            UpdateScreenshotButtons();
        }

        private void AddScreenshot(Bitmap? image, string why)
        {
            if (image == null)
            {
                MessageBox.Show(this, why, $"{nameof(LunarROMCorruptor)} - Screenshot", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                string path = StashLibrary.AddScreenshot(_entry, image);
                LoadScreenshots(path);
                StashLibrary.RaiseChanged();
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to add the screenshot: {ex.Message}", StatusSeverityType.Error, true);
            }
            finally
            {
                image.Dispose();
            }
        }

        private void EmulatorShotBtn_Click(object? sender, EventArgs e) =>
            AddScreenshot(EmulatorScreenshotProvider?.Invoke(), "The emulator isn't running, or its window can't be captured.\n\nStart it with 'Run program after corruption', or take a screenshot yourself and use Paste.");

        private void PasteShotBtn_Click(object? sender, EventArgs e) =>
            AddScreenshot(Clipboard.ContainsImage() && Clipboard.GetImage() is { } img ? new Bitmap(img) : null, "There is no image on the clipboard.");

        private void FileShotBtn_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog { Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*", Multiselect = true, Title = "Add screenshots" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            foreach (string file in dialog.FileNames)
                AddScreenshot(StashLibrary.LoadImageUnlocked(file) as Bitmap, $"{Path.GetFileName(file)} couldn't be read as an image.");
        }

        private void ThumbnailBtn_Click(object? sender, EventArgs e)
        {
            if (SelectedScreenshot is not { } path) return;
            using var image = StashLibrary.LoadImageUnlocked(path);
            if (image == null) return;
            try
            {
                StashLibrary.SetThumbnail(_entry, image);
                StashLibrary.RaiseChanged();
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to set the thumbnail: {ex.Message}", StatusSeverityType.Error, true);
            }
        }

        private void RemoveShotBtn_Click(object? sender, EventArgs e)
        {
            if (SelectedScreenshot is not { } path) return;
            if (MessageBox.Show(this, "Delete this screenshot?", $"{nameof(LunarROMCorruptor)} - Screenshot", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            ShowPreview(null);
            StashLibrary.RemoveScreenshot(_entry, path);
            LoadScreenshots();
            StashLibrary.RaiseChanged();
        }

        #endregion

        #region Saving and actions

        private bool SaveChanges()
        {
            try
            {
                var m = _entry.Meta;
                m.Rating = Math.Max(0, RatingComboBox.SelectedIndex);
                m.Tags = [.. TagsTxt.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.CurrentCultureIgnoreCase)];
                m.Notes = NotesTxt.Text;
                string name = NameTxt.Text.Trim();
                if (name.Length > 0 && name != m.Name)
                {
                    if (!StashLibrary.Rename(_entry, name)) return false; //Rename saves the info too
                }
                else
                {
                    StashLibrary.SaveMeta(_entry);
                }
                _dirty = false;
                Text = $"Stash details - {m.Name}";
                InfoTxt.Text = BuildInfo();
                StashLibrary.RaiseChanged();
                return true;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to save the stash info: {ex.Message}", StatusSeverityType.Error, true);
                return false;
            }
        }

        private void SaveBtn_Click(object? sender, EventArgs e) => SaveChanges();

        private void CloseBtn_Click(object? sender, EventArgs e) => Close();

        private void StashDetailsForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_dirty) return;
            var answer = MessageBox.Show(this, "Save the changes to this stash?", $"{nameof(LunarROMCorruptor)} - Stash details", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.Cancel || (answer == DialogResult.Yes && !SaveChanges())) e.Cancel = true;
        }

        private void LoadSettingsBtn_Click(object? sender, EventArgs e) => LoadSettingsRequested?.Invoke(_entry);

        private void QuickLoadBtn_Click(object? sender, EventArgs e)
        {
            if (_dirty && !SaveChanges()) return;
            QuickLoadRequested?.Invoke(_entry);
        }

        private void OpenFolderBtn_Click(object? sender, EventArgs e) => CommandRunner.OpenFolderInExplorer(_entry.Folder);

        /// <summary>
        /// The file moved: pick its new location. The stash headers and info are updated, and LRC checks it's the same file.
        /// </summary>
        private void LocateFileBtn_Click(object? sender, EventArgs e)
        {
            var m = _entry.Meta;
            using var dialog = new OpenFileDialog { Title = $"Where is {(string.IsNullOrEmpty(m.SourceFileName) ? "the file" : m.SourceFileName)} now?", FileName = m.SourceFileName, Filter = "All files (*.*)|*.*" };
            if (!string.IsNullOrEmpty(m.SourcePath))
            {
                string? dir = Path.GetDirectoryName(m.SourcePath);
                if (dir != null && Directory.Exists(dir)) dialog.InitialDirectory = dir;
            }
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            string file = dialog.FileName;
            try
            {
                UseWaitCursor = true;
                var check = StashLibrary.CheckSource(m, file);
                UseWaitCursor = false;
                if (check == StashLibrary.SourceCheck.Changed &&
                    MessageBox.Show(this, $"{Path.GetFileName(file)} isn't the same file this stash was made for (different size or checksum).\n\nUse it anyway?", $"{nameof(LunarROMCorruptor)} - Locate file", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
                string? saveAs = m.SaveAsPath;
                if (!string.IsNullOrEmpty(saveAs) && !Directory.Exists(Path.GetDirectoryName(saveAs) ?? ""))
                    saveAs = Path.Combine(Path.GetDirectoryName(file) ?? "", Path.GetFileName(saveAs)); //Keep the save file next to the file
                StashLibrary.UpdateHeaders(_entry.StashFile, file, saveAs);
                m.SourcePath = file;
                m.SaveAsPath = saveAs;
                if (check == StashLibrary.SourceCheck.Changed) StashLibrary.FillSourceInfo(m); //It's the file now, remember its checksum
                StashLibrary.SaveMeta(_entry);
                InfoTxt.Text = BuildInfo();
                QuickLoadBtn.Enabled = true;
                StashLibrary.RaiseChanged();
                TraceLogger.Log($"Stash {m.Name} now points to {file}.");
            }
            catch (Exception ex)
            {
                UseWaitCursor = false;
                TraceLogger.Log($"Unable to update the stash: {ex.Message}", StatusSeverityType.Error, true);
            }
        }

        #endregion
    }
}
