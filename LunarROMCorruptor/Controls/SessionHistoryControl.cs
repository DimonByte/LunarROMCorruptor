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
using System.ComponentModel;

namespace LunarROMCorruptor.Controls
{
    /// <summary>
    /// The ⏳ Session History tab: every corruption of this session (newest first) with its engine, file, range and changes.
    /// Double-click (or "Revert to this") re-applies one, "Save as stash" keeps it in the stash list. The main form does the actual work through the events.
    /// </summary>
    [DesignerCategory("Code")]
    public class SessionHistoryControl : UserControl
    {
        private static readonly Color Back = Color.FromArgb(28, 27, 55), ListBack = Color.FromArgb(39, 43, 83), ButtonBack = Color.FromArgb(52, 60, 111), Fore = Color.FromArgb(213, 216, 216);

        private readonly ListView _list = new();
        private readonly Button _reapplyBtn, _saveBtn, _editBtn;
        private readonly Label _infoLbl = new();
        private bool _busy;

        /// <summary>Re-apply this corruption to the save location ("revert to this").</summary>
        public event Action<CorruptionHistoryEntry>? ReapplyRequested;
        public event Action<CorruptionHistoryEntry>? SaveRequested;
        public event Action<CorruptionHistoryEntry>? EditRequested;

        public SessionHistoryControl()
        {
            BackColor = Back;
            ForeColor = Fore;

            _list.Dock = DockStyle.Fill;
            _list.View = View.Details;
            _list.FullRowSelect = true;
            _list.MultiSelect = false;
            _list.HideSelection = false;
            _list.BorderStyle = BorderStyle.None;
            _list.BackColor = ListBack;
            _list.ForeColor = Fore;
            _list.Columns.Add("#", 36);
            _list.Columns.Add("Time", 70);
            _list.Columns.Add("Engine", 130);
            _list.Columns.Add("File", 170);
            _list.Columns.Add("Changes", 80, HorizontalAlignment.Right);
            _list.Columns.Add("Range / intensity", 260);
            _list.SelectedIndexChanged += (_, _) => UpdateButtons();
            _list.DoubleClick += (_, _) => { if (Selected is { } e && _reapplyBtn!.Enabled) ReapplyRequested?.Invoke(e); };

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36, Padding = new Padding(4, 4, 4, 0), BackColor = Back };
            _reapplyBtn = MakeButton("↩ Revert to this", "Re-applies the selected corruption to the save location (double-click does the same).", () => ReapplyRequested);
            _saveBtn = MakeButton("💾 Save as stash", "Saves the selected corruption to the stash list, with its engine settings.", () => SaveRequested);
            _editBtn = MakeButton("Open in Stash Editor", "Opens the selected corruption in the Stash Editor.", () => EditRequested);
            buttons.Controls.AddRange([_reapplyBtn, _saveBtn, _editBtn]);

            _infoLbl.Dock = DockStyle.Top;
            _infoLbl.Height = 26;
            _infoLbl.Padding = new Padding(4, 6, 4, 0);
            _infoLbl.ForeColor = Color.FromArgb(150, 155, 190);
            _infoLbl.Text = InfoText(0);

            Controls.Add(_list);
            Controls.Add(buttons);
            Controls.Add(_infoLbl);
            UpdateButtons();
        }

        private static string InfoText(int count) =>
            $"The last {CorruptionHistory.MaxEntries} corruptions of this session (newest first, needs Enable Stash Saves). {count} kept. They're deleted when LRC closes: save the good ones as stashes.";

        private Button MakeButton(string text, string tip, Func<Action<CorruptionHistoryEntry>?> handler)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = ButtonBack,
                ForeColor = Fore,
                Margin = new Padding(0, 0, 6, 0),
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderSize = 0;
            new ToolTip().SetToolTip(button, tip);
            button.Click += (_, _) => { if (Selected is { } e) handler()?.Invoke(e); };
            return button;
        }

        private CorruptionHistoryEntry? Selected => _list.SelectedItems.Count == 1 ? _list.SelectedItems[0].Tag as CorruptionHistoryEntry : null;

        /// <summary>
        /// While a corruption is running the buttons are disabled.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Busy
        {
            get => _busy;
            set { _busy = value; UpdateButtons(); }
        }

        private void UpdateButtons()
        {
            bool ok = Selected is { } e && File.Exists(e.StashPath);
            _reapplyBtn.Enabled = _saveBtn.Enabled = _editBtn.Enabled = ok && !_busy;
            _saveBtn.Enabled = ok; //Saving doesn't touch the file, so it works while a corruption runs
        }

        /// <summary>
        /// Shows the history again (keeps the selection when the entry is still there).
        /// </summary>
        public void RefreshList()
        {
            var selected = Selected;
            var entries = CorruptionHistory.Entries;
            _list.BeginUpdate();
            _list.Items.Clear();
            int total = CorruptionHistory.TotalAdded;
            for (int k = 0; k < entries.Count; k++)
            {
                var e = entries[k];
                var m = e.Meta;
                string range = m == null || m.EndByte < 0 ? "" : $"0x{m.StartByte:X} - 0x{m.EndByte:X}, {(m.EveryNth ? $"every {m.Intensity:N0}th byte" : $"intensity {m.Intensity:N0}")}";
                var item = new ListViewItem([$"{total - k}", e.Time.ToString("HH:mm:ss"), e.EngineName, Path.GetFileName(e.SourcePath), e.Changes.ToString("N0"), range]) { Tag = e };
                if (!File.Exists(e.StashPath)) item.ForeColor = Color.Gray;
                _list.Items.Add(item);
                if (selected == e) item.Selected = true;
            }
            _list.EndUpdate();
            _infoLbl.Text = InfoText(entries.Count);
            UpdateButtons();
        }
    }
}
