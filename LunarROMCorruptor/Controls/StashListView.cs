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

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace LunarROMCorruptor.Controls
{
    /// <summary>
    /// A list of stash lines that can hold millions of items. A normal ListBox stores every item inside the native control,
    /// so adding 2 million lines takes minutes and freezes the UI. This is a virtual ListView: the lines live in a
    /// List&lt;string&gt; and Windows only asks for the rows that are on screen, so setting the list is instant at any size.
    /// Change the lines through the methods here (not <see cref="ListView.Items"/>, which isn't available in virtual mode).
    /// </summary>
    [DesignerCategory("Code")]
    public class StashListView : ListView
    {
        private List<string> _lines = [];
        private readonly ColumnHeader _column = new() { Text = "", Width = 300 };
        private int _longestLength;
        private string _longestLine = "";

        /// <summary>Raised whenever the number of lines changes.</summary>
        public event EventHandler? CountChanged;

        public StashListView()
        {
            View = View.Details;
            HeaderStyle = ColumnHeaderStyle.None;
            FullRowSelect = true;
            MultiSelect = true;
            HideSelection = false;
            VirtualMode = true;
            VirtualListSize = 0;
            //Stops flicker when scrolling fast through big lists
            DoubleBuffered = true;
        }

        /// <summary>The lines in the list. Read only: use the methods on this control to change them.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<string> Lines => _lines;

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Count => _lines.Count;

        /// <summary>Replaces every line. The list is taken over by the control (it isn't copied), so don't change it afterwards.</summary>
        public void SetLines(List<string> lines)
        {
            ClearSelection();
            _lines = lines;
            RecalculateLongest();
            SyncSize(scrollToTop: true);
        }

        public void AddLine(string line)
        {
            _lines.Add(line);
            TrackLongest(line);
            SyncSize(scrollToTop: false);
        }

        public void AddLines(IEnumerable<string> lines)
        {
            int before = _lines.Count;
            _lines.AddRange(lines);
            for (int i = before; i < _lines.Count; i++) TrackLongest(_lines[i]);
            SyncSize(scrollToTop: false);
        }

        public void ClearLines() => SetLines([]);

        /// <summary>Removes the selected lines and returns them in list order.</summary>
        public List<string> TakeSelected()
        {
            int[] selected = GetSelectedIndices();
            if (selected.Length == 0) return [];
            var taken = new List<string>(selected.Length);
            var kept = new List<string>(Math.Max(0, _lines.Count - selected.Length));
            int next = 0;
            for (int i = 0; i < _lines.Count; i++)
            {
                if (next < selected.Length && selected[next] == i)
                {
                    taken.Add(_lines[i]);
                    next++;
                }
                else kept.Add(_lines[i]);
            }
            int firstSelected = selected[0];
            SetLinesKeepScroll(kept);
            //Put the focus back roughly where the removed items were, so removing in steps feels natural
            if (kept.Count > 0) EnsureVisible(Math.Min(firstSelected, kept.Count - 1));
            return taken;
        }

        /// <summary>Removes the first <paramref name="count"/> lines and returns them.</summary>
        public List<string> TakeFirst(int count)
        {
            count = Math.Clamp(count, 0, _lines.Count);
            if (count == 0) return [];
            var taken = _lines.GetRange(0, count);
            var kept = _lines.GetRange(count, _lines.Count - count);
            SetLinesKeepScroll(kept);
            return taken;
        }

        /// <summary>Selected row indices in ascending order. Fast even when every row is selected.</summary>
        public int[] GetSelectedIndices()
        {
            if (!IsHandleCreated || _lines.Count == 0) return [];
            int count = SelectedIndices.Count;
            if (count == 0) return [];
            if (count == _lines.Count) return Enumerable.Range(0, count).ToArray();
            var result = new int[count];
            int n = 0;
            int index = -1;
            //LVM_GETNEXTITEM walks the selection directly, which is much faster than SelectedIndices' enumerator on huge lists
            while (n < count && (index = (int)SendMessage(Handle, LVM_GETNEXTITEM, index, LVNI_SELECTED)) >= 0)
                result[n++] = index;
            return n == count ? result : result[..n];
        }

        public void SelectAllRows()
        {
            if (!IsHandleCreated || _lines.Count == 0) return;
            SetAllItemsState(LVIS_SELECTED, LVIS_SELECTED);
        }

        public void ClearSelection()
        {
            if (!IsHandleCreated || VirtualListSize == 0) return;
            SetAllItemsState(0, LVIS_SELECTED | LVIS_FOCUSED);
        }

        protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        {
            //Windows can ask for a row that was just removed while the list size is changing, so never index out of range
            int i = e.ItemIndex;
            e.Item = new ListViewItem((uint)i < (uint)_lines.Count ? _lines[i] : "");
            base.OnRetrieveVirtualItem(e);
        }

        protected override void OnSearchForVirtualItem(SearchForVirtualItemEventArgs e)
        {
            //Typing jumps to the next line that starts with the typed text (like a ListBox)
            if (!string.IsNullOrEmpty(e.Text))
            {
                int start = Math.Max(0, e.StartIndex);
                for (int pass = 0; pass < 2 && e.Index < 0; pass++)
                {
                    int from = pass == 0 ? start : 0, to = pass == 0 ? _lines.Count : start;
                    for (int i = from; i < to; i++)
                    {
                        if (_lines[i].StartsWith(e.Text, StringComparison.OrdinalIgnoreCase)) { e.Index = i; break; }
                    }
                }
            }
            base.OnSearchForVirtualItem(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                SelectAllRows();
                e.Handled = e.SuppressKeyPress = true;
                return;
            }
            base.OnKeyDown(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            UpdateColumnWidth();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateColumnWidth();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            //The column is added here instead of the constructor so the Visual Studio designer never serializes it into
            //InitializeComponent (which would give the list a second column)
            if (!DesignMode && !Columns.Contains(_column)) Columns.Add(_column);
            UpdateColumnWidth();
        }

        private void SetLinesKeepScroll(List<string> lines)
        {
            ClearSelection();
            _lines = lines;
            RecalculateLongest();
            SyncSize(scrollToTop: false);
        }

        private void SyncSize(bool scrollToTop)
        {
            int oldSize = VirtualListSize;
            if (oldSize != _lines.Count)
            {
                //Shrinking below the focused/top row can make the native control ask for rows that no longer exist; the guard in
                //OnRetrieveVirtualItem handles that, and we scroll back into range below
                VirtualListSize = _lines.Count;
            }
            if (scrollToTop && _lines.Count > 0 && IsHandleCreated) EnsureVisible(0);
            UpdateColumnWidth();
            Invalidate();
            if (oldSize != _lines.Count || scrollToTop) CountChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TrackLongest(string line)
        {
            if (line.Length > _longestLength)
            {
                _longestLength = line.Length;
                _longestLine = line;
            }
        }

        private void RecalculateLongest()
        {
            _longestLength = 0;
            _longestLine = "";
            var lines = _lines;
            for (int i = 0; i < lines.Count; i++) TrackLongest(lines[i]);
        }

        private int _lastMeasuredLength = -1;
        private int _lastMeasuredWidth;

        private void UpdateColumnWidth()
        {
            if (_lastMeasuredLength != _longestLength)
            {
                _lastMeasuredWidth = _longestLine.Length == 0 ? 0 : TextRenderer.MeasureText(_longestLine + "  ", Font).Width;
                _lastMeasuredLength = _longestLength;
            }
            //Fill the visible width (so a selected row is highlighted edge to edge) or the longest line, whichever is wider.
            //The scrollbar width is left free so a vertical scrollbar never forces a horizontal one.
            int width = Math.Max(ClientSize.Width - SystemInformation.VerticalScrollBarWidth, _lastMeasuredWidth);
            width = Math.Max(width, 20);
            if (_column.Width != width) _column.Width = width;
        }

        #region Native

        private const int LVM_FIRST = 0x1000;
        private const int LVM_SETITEMSTATE = LVM_FIRST + 43;
        private const int LVM_GETNEXTITEM = LVM_FIRST + 12;
        private const int LVNI_SELECTED = 0x0002;
        private const int LVIS_FOCUSED = 0x0001;
        private const int LVIS_SELECTED = 0x0002;
        private const int LVIF_STATE = 0x0008;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LVITEM
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
            public int iGroupId;
            public int cColumns;
            public IntPtr puColumns;
            public IntPtr piColFmt;
            public int iGroup;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, ref LVITEM lParam);

        /// <summary>Sets the state of every row in one message (item index -1), instead of one call per row.</summary>
        private void SetAllItemsState(int state, int mask)
        {
            var item = new LVITEM { mask = LVIF_STATE, state = state, stateMask = mask };
            SendMessage(Handle, LVM_SETITEMSTATE, -1, ref item);
        }

        #endregion
    }
}
