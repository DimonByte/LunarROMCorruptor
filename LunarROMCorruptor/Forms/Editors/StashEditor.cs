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
using System.Text;

namespace LunarROMCorruptor
{
    public partial class StashEditor : Form
    {
        private string? LoadedStashLocation;
        private readonly Random _rnd = new();
        private readonly System.Windows.Forms.Timer _countTimer = new() { Interval = 500 };

        public StashEditor()
        {
            InitializeComponent();
            //Keeps the item count label up to date (ListBox has no "items changed" event)
            _countTimer.Tick += (_, _) => StashCountLbl.Text = $"{stashListbox.Items.Count:N0} items ({RemovedItemslstbx.Items.Count:N0} removed)";
            _countTimer.Start();
        }

        private List<string> CurrentLines() => [.. stashListbox.Items.Cast<object>().Select(i => i?.ToString() ?? "")];

        private void SetLines(IEnumerable<string> lines)
        {
            stashListbox.BeginUpdate();
            stashListbox.Items.Clear();
            stashListbox.Items.AddRange([.. lines.Cast<object>()]);
            stashListbox.EndUpdate();
        }

        private StashTools.MutateMode SelectedMutateMode => MutateModeComboBox.SelectedIndex switch
        {
            1 => StashTools.MutateMode.NudgeValue,
            2 => StashTools.MutateMode.MoveAddress,
            _ => StashTools.MutateMode.RandomValue
        };

        /// <summary>
        /// Mutates the list. Returns false if there was nothing to mutate.
        /// </summary>
        private bool MutateList()
        {
            var lines = CurrentLines();
            if (lines.Count == 0)
            {
                MessageBox.Show("The stash list is empty. Load a stash first.", "Mutate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            long maxOffset = CorruptionCore.IsLoaded ? CorruptionCore.MaxByte : long.MaxValue;
            var mutated = StashTools.Mutate(lines, (double)MutatePercentNumericUpDown.Value, SelectedMutateMode, (long)MutateAmountNumericUpDown.Value, maxOffset, _rnd, out int changed);
            if (changed == 0)
            {
                MessageBox.Show("Nothing was mutated. Only byte entries ([x] File(i).SET(v)) can be mutated.", "Mutate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            SetLines(mutated);
            TraceLogger.Log($"Stash Editor: mutated {changed} of {lines.Count} stash entries ({MutateModeComboBox.Text}).");
            return true;
        }

        private void MutateBtn_Click(object? sender, EventArgs e)
        {
            MutateList();
        }

        private void MutateCorruptBtn_Click(object? sender, EventArgs e)
        {
            //The mutation is saved as a new stash file so the original corruption is kept
            if (Program.Form.CorruptionQueueChkbox.Checked)
            {
                MessageBox.Show("Corruption queue is enabled. Please disable it and select a single file before corrupting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!MutateList()) return;
            string path = SaveNewStashFile();
            Program.Form.CorruptUsingStashFile(path);
        }

        private void AppendStashBtn_Click(object? sender, EventArgs e)
        {
            Directory.CreateDirectory(IOManager.CorruptionStashDirectory);
            OpenFileDialog.InitialDirectory = IOManager.CorruptionStashDirectory;
            if (OpenFileDialog.ShowDialog() == DialogResult.Cancel) return;
            try
            {
                var added = new List<string>();
                foreach (var raw in File.ReadLines(OpenFileDialog.FileName))
                {
                    string line = raw.Trim();
                    if (line.Length == 0) continue;
                    if (!StashParser.IsValidStashLine(line))
                    {
                        MessageBox.Show($"This file is not a valid stash file ('{line}').", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    added.Add(line);
                }
                stashListbox.BeginUpdate();
                stashListbox.Items.AddRange([.. added.Cast<object>()]);
                stashListbox.EndUpdate();
                TraceLogger.Log($"Stash Editor: appended {added.Count} entries from {Path.GetFileName(OpenFileDialog.FileName)}.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading stash", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SortStashBtn_Click(object? sender, EventArgs e)
        {
            SetLines(StashTools.SortByAddress(CurrentLines()));
        }

        private void DedupeStashBtn_Click(object? sender, EventArgs e)
        {
            var result = StashTools.RemoveDuplicates(CurrentLines(), out int removed);
            SetLines(result);
            MessageBox.Show($"Removed {removed} entries that were overwritten by a later entry for the same address.", "Remove duplicates", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public void AttemptStashLoad(string fileloc)
        {
            try
            {
                var lines = new List<string>();
                int lineNumber = 0;
                foreach (var rawLine in File.ReadLines(fileloc))
                {
                    lineNumber++;
                    string line = rawLine.Trim();
                    if (line.Length == 0) continue;
                    //Accepts both "[x] File(i).SET(v)" and the Source Code Engine "[x] File(i).VARLINE("name",line).SET("value")" format
                    if (!StashParser.IsValidStashLine(line))
                    {
                        MessageBox.Show($"This file is not a valid stash file (line {lineNumber}: '{line}').", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    lines.Add(line);
                }
                stashListbox.BeginUpdate();
                stashListbox.Items.Clear();
                RemovedItemslstbx.Items.Clear();
                stashListbox.Items.AddRange([.. lines]);
                stashListbox.EndUpdate();
                LoadedStashLocation = fileloc;
                Text = $"Stash Editor - {Path.GetFileName(fileloc)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading stash", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Openfilebtn_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory(IOManager.CorruptionStashDirectory);
            OpenFileDialog.InitialDirectory = IOManager.CorruptionStashDirectory;
            if (OpenFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                AttemptStashLoad(OpenFileDialog.FileName);
            }
        }

        private void Removeselbtn_Click(object sender, EventArgs e)
        {
            foreach (var item in stashListbox.SelectedItems)
            {
                RemovedItemslstbx.Items.Add(item);
            }
            while (stashListbox.SelectedItems.Count > 0)
            {
                if (stashListbox.SelectedItems[0] == null) break;
                stashListbox.Items.Remove(stashListbox.SelectedItems[0]!);
            }
        }

        private void Remove50btn_Click(object sender, EventArgs e)
        {
            //Moves the first half of the stash to the removed list (the old loop removed while indexing forward, so it skipped every other item)
            int half = (stashListbox.Items.Count + 1) / 2;
            if (half == 0) return;
            var moved = new object[half];
            for (int i = 0; i < half; i++)
                moved[i] = stashListbox.Items[i];
            stashListbox.BeginUpdate();
            RemovedItemslstbx.BeginUpdate();
            RemovedItemslstbx.Items.AddRange(moved);
            for (int i = 0; i < half; i++)
                stashListbox.Items.RemoveAt(0);
            RemovedItemslstbx.EndUpdate();
            stashListbox.EndUpdate();
        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            stashListbox.Items.Add("[x] File(" + LocationStash.Value + ").SET(" + valueStashnum.Value + ")");
        }

        private void Additemsbtn_Click(object sender, EventArgs e)
        {
            //Add each selected item from RemovedItemslstbx to the stashlistbox items, once added, remove the items that are selected from RemovedItemslstbx
            foreach (var item in RemovedItemslstbx.SelectedItems)
            {
                stashListbox.Items.Add(item);
            }
            while (RemovedItemslstbx.SelectedItems.Count > 0)
            {
                if (RemovedItemslstbx.SelectedItems[0] == null) break;
                RemovedItemslstbx.Items.Remove(RemovedItemslstbx.SelectedItems[0]!);
            }
        }

        private void Removeitembtn_Click(object sender, EventArgs e)
        {
            while (RemovedItemslstbx.SelectedItems.Count > 0)
            {
                if (RemovedItemslstbx.SelectedItems[0] == null) break;
                RemovedItemslstbx.Items.Remove(RemovedItemslstbx.SelectedItems[0]!);
            }
        }

        private void Newfilestashbtn_Click(object sender, EventArgs e)
        {
            SaveNewStashFile();
        }
        private string SaveNewStashFile()
        {
            Directory.CreateDirectory(IOManager.CorruptionStashDirectory);
            string path;
            do
            {
                path = Path.Combine(IOManager.CorruptionStashDirectory, new Random().Next(1000, 999999999) + ".stash");
            } while (File.Exists(path));
            WriteStash(path);
            IOManager.RefreshCorruptionStashList(Program.Form.StashFileList);
            TraceLogger.Log(path + " - Saved to a brand new stash file.");
            LoadedStashLocation = path;
            return path;
        }

        private void WriteStash(string path)
        {
            using var writer = new StreamWriter(path, false, new UTF8Encoding(false));
            foreach (var listitem in stashListbox.Items)
            {
                writer.WriteLine(listitem);
            }
        }

        private void SaveToExistingStashFile()
        {
            //Check if a stash file location is loaded first.
            if (LoadedStashLocation == null)
            {
                MessageBox.Show("No stash file is loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //Check if corruption queue is enabled on the main form, if it is cancel the operation.
            if (Program.Form.CorruptionQueueChkbox.Checked)
            {
                MessageBox.Show("Corruption queue is enabled. Please disable it and select a single file before corrupting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                WriteStash(LoadedStashLocation);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save the stash: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            IOManager.RefreshCorruptionStashList(Program.Form.StashFileList);
            TraceLogger.Log(LoadedStashLocation + " - Saved to existing stash file.");
        }
        private void StashEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void Restorebtn_Click(object sender, EventArgs e)
        {
            if (LoadedStashLocation == null || !File.Exists(LoadedStashLocation))
            {
                MessageBox.Show("No stash file is loaded.", $"Info - {nameof(LunarROMCorruptor)} ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            stashListbox.Items.Clear();
            RemovedItemslstbx.Items.Clear();
            try
            {
                foreach (var line in File.ReadLines(LoadedStashLocation))
                {
                    stashListbox.Items.Add(line);
                }
            }
            catch
            {
                MessageBox.Show("Can't restore stash items.", $"Info - {nameof(LunarROMCorruptor)} ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RemoveAllbtn_Click(object sender, EventArgs e)
        {
            //Remove ALL items from the stashlistbox
            stashListbox.Items.Clear();
        }

        private void SaveAndCorruptbtn_Click(object sender, EventArgs e)
        {
            //Run a void function on Form1 that is called CorruptUsingStashFile
            SaveToExistingStashFile();
            if (LoadedStashLocation == null || Program.Form.CorruptionQueueChkbox.Checked)
            {
                return;
            }
            Program.Form.CorruptUsingStashFile(LoadedStashLocation);
        }
    }
}