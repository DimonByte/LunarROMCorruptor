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
using LunarROMCorruptor.Modules.Helpers;
using LunarROMCorruptor.Properties;
using System.ComponentModel;

namespace LunarROMCorruptor.Controls
{
    /// <summary>
    /// The stash library as a tree: groups (folders) and stashes. Drag stashes/groups onto a group to move them, drop .lrcstash/.stash
    /// files to import them, F2 renames, Del deletes, Enter/double-click opens a stash's details. Hovering a stash shows its
    /// thumbnail and info. The main form handles the corruption actions through the events.
    /// </summary>
    public partial class StashBrowserControl : UserControl
    {
        private const int IconGroup = 0, IconGroupOpen = 1, IconStash = 2, IconStashPicture = 3, IconStashMissing = 4;

        /// <summary>
        /// What a tree node is: a group (Entry is null) or a stash.
        /// </summary>
        private sealed class NodeInfo
        {
            public required string Folder;
            public StashEntry? Entry;
            public bool SourceMissing;
            public bool IsGroup => Entry == null;
        }

        private readonly HashSet<string> _expanded = new(StringComparer.OrdinalIgnoreCase);
        private StashLibrary.GroupNode? _library;
        private bool _reloading;
        private StashEntry? _previewEntry;
        private TreeNode? _dropTarget;

        public event Action<StashEntry>? QuickLoadRequested; //Load the stash's own file and replay it
        public event Action<StashEntry>? ApplyToLoadedRequested; //Replay it on the loaded file
        public event Action<StashEntry>? EditRequested; //Open in the Stash Editor
        public event Action<StashEntry>? LoadSettingsRequested; //Load engine/range/intensity
        public event Action<StashEntry>? DetailsRequested;
        public event Action<StashEntry>? LoadStashFileFromHeaderOnly; //Reads header of stash and loads that.

        /// <summary>
        /// Takes a screenshot of the running emulator (null if it isn't running).
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] //Set in code, the designer must not serialize it
        public Func<Bitmap?>? EmulatorScreenshotProvider { get; set; }

        /// <summary>
        /// A copy of the current ByteView image (null if there is none).
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] //Set in code, the designer must not serialize it
        public Func<Bitmap?>? ByteViewImageProvider { get; set; }

        public StashBrowserControl()
        {
            InitializeComponent();
            CreateIcons();
            if (!IsInDesignMode())
            {
                StashLibrary.Changed += OnLibraryChanged;
                SortNewestMenuItem.Checked = Settings.Default.StashSortNewestFirst;
                AutoScreenshotMenuItem.Checked = Settings.Default.StashAutoScreenshot;
            }
            for (int stars = 0; stars <= 5; stars++)
            {
                int value = stars;
                var item = new ToolStripMenuItem(stars == 0 ? "No rating" : new string('★', stars) + new string('☆', 5 - stars)) { Tag = value };
                item.Click += (_, _) => SetRating(value);
                RatingMenuItem.DropDownItems.Add(item);
            }
        }

        private bool IsInDesignMode() => DesignMode || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;

        /// <summary>
        /// Whether a stash saved from the main form should get a screenshot of the running emulator.
        /// </summary>
        public bool AutoScreenshot => AutoScreenshotMenuItem.Checked;

        #region Icons

        private void CreateIcons()
        {
            NodeImages.Images.Add(DrawIcon(g => DrawFolder(g, false)));
            NodeImages.Images.Add(DrawIcon(g => DrawFolder(g, true)));
            NodeImages.Images.Add(DrawIcon(g => DrawPage(g, Color.FromArgb(120, 170, 255), false)));
            NodeImages.Images.Add(DrawIcon(g => DrawPage(g, Color.FromArgb(120, 220, 160), true)));
            NodeImages.Images.Add(DrawIcon(g => DrawPage(g, Color.FromArgb(150, 150, 150), false, missing: true)));
        }

        private static Bitmap DrawIcon(Action<Graphics> draw)
        {
            var bmp = new Bitmap(16, 16);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            draw(g);
            return bmp;
        }

        private static void DrawFolder(Graphics g, bool open)
        {
            using var back = new SolidBrush(Color.FromArgb(200, 160, 60));
            using var front = new SolidBrush(open ? Color.FromArgb(255, 215, 110) : Color.FromArgb(240, 195, 80));
            g.FillRectangle(back, 1, 3, 6, 3);
            g.FillRectangle(back, 1, 4, 14, 10);
            if (open) g.FillPolygon(front, [new Point(3, 7), new Point(15, 7), new Point(13, 14), new Point(1, 14)]);
            else g.FillRectangle(front, 1, 6, 14, 8);
        }

        private static void DrawPage(Graphics g, Color colour, bool picture, bool missing = false)
        {
            using var page = new SolidBrush(colour);
            using var lines = new Pen(Color.FromArgb(40, 45, 80));
            g.FillRectangle(page, 3, 1, 10, 14);
            if (picture)
            {
                using var sky = new SolidBrush(Color.FromArgb(40, 45, 80));
                g.FillRectangle(sky, 5, 4, 6, 5);
                using var sun = new SolidBrush(Color.FromArgb(255, 220, 90));
                g.FillEllipse(sun, 8, 5, 2, 2);
                g.DrawLine(lines, 5, 11, 11, 11);
            }
            else
            {
                for (int y = 4; y <= 11; y += 3) g.DrawLine(lines, 5, y, 11, y);
            }
            if (missing)
            {
                using var red = new Pen(Color.FromArgb(230, 80, 80), 2);
                g.DrawLine(red, 2, 14, 14, 2);
            }
        }

        #endregion

        #region Loading the tree

        private void OnLibraryChanged()
        {
            if (IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(new Action(() => Reload())); return; }
            Reload();
        }

        /// <summary>
        /// Re-reads the library (keeping the expanded groups and the selection). Pass a folder to select it afterwards.
        /// </summary>
        public void Reload(string? selectFolder = null)
        {
            if (IsInDesignMode()) return;
            selectFolder ??= (StashTree.SelectedNode?.Tag as NodeInfo)?.Folder;
            try
            {
                _library = StashLibrary.Load();
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to read the stash library: {ex.Message}", StatusSeverityType.Error);
                _library = null;
            }
            BuildTree(selectFolder);
        }

        private static bool Matches(StashEntry e, string[] words)
        {
            if (words.Length == 0) return true;
            var m = e.Meta;
            string haystack = $"{m.Name} {m.SourceFileName} {m.Engine} {m.Platform} {string.Join(' ', m.Tags)} {m.Notes} {e.Group}";
            return words.All(w => haystack.Contains(w, StringComparison.CurrentCultureIgnoreCase));
        }

        private void BuildTree(string? selectFolder)
        {
            _reloading = true;
            StashTree.BeginUpdate();
            try
            {
                StashTree.Nodes.Clear();
                if (_library == null) return;
                string[] words = FilterTxt.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                _existsCache.Clear();
                AddGroupChildren(StashTree.Nodes, _library, words);
                if (selectFolder != null)
                {
                    var node = FindNode(StashTree.Nodes, selectFolder);
                    if (node != null)
                    {
                        StashTree.SelectedNode = node;
                        node.EnsureVisible();
                    }
                }
            }
            finally
            {
                StashTree.EndUpdate();
                _reloading = false;
            }
        }

        /// <summary>
        /// Adds a group's subgroups and stashes. Returns false if nothing in it matches the filter.
        /// </summary>
        private bool AddGroupChildren(TreeNodeCollection nodes, StashLibrary.GroupNode group, string[] words)
        {
            bool any = false;
            foreach (var sub in group.Groups)
            {
                var node = new TreeNode(sub.Name, IconGroup, IconGroup) { Tag = new NodeInfo { Folder = sub.Path } };
                bool hasMatches = AddGroupChildren(node.Nodes, sub, words);
                if (words.Length > 0 && !hasMatches) continue;
                nodes.Add(node);
                if (words.Length > 0 || _expanded.Contains(sub.Path))
                {
                    node.Expand();
                    node.ImageIndex = node.SelectedImageIndex = IconGroupOpen;
                }
                any = true;
            }
            IEnumerable<StashEntry> stashes = group.Stashes.Where(e => Matches(e, words));
            stashes = SortNewestMenuItem.Checked
                ? stashes.OrderByDescending(e => e.Meta.Created)
                : stashes.OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase);
            foreach (var entry in stashes)
            {
                bool missing = SourceMissing(entry.Meta.SourcePath);
                int icon = missing ? IconStashMissing : entry.HasThumbnail ? IconStashPicture : IconStash;
                nodes.Add(new TreeNode(entry.Name, icon, icon) { Tag = new NodeInfo { Folder = entry.Folder, Entry = entry, SourceMissing = missing } });
                any = true;
            }
            return any;
        }

        private readonly Dictionary<string, bool> _existsCache = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// True when a stash's file isn't there anymore. Network paths aren't checked (an offline share can take seconds per check),
        /// and each path is only checked once per refresh.
        /// </summary>
        private bool SourceMissing(string? path)
        {
            if (string.IsNullOrEmpty(path) || path.StartsWith(@"\\")) return false;
            if (!_existsCache.TryGetValue(path, out bool exists))
            {
                try { exists = File.Exists(path); } catch { exists = true; }
                _existsCache[path] = exists;
            }
            return !exists;
        }

        private static TreeNode? FindNode(TreeNodeCollection nodes, string folder)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is NodeInfo info && Modules.CorruptionInternals.CorruptionCore.PathsEqual(info.Folder, folder)) return node;
                var found = FindNode(node.Nodes, folder);
                if (found != null) return found;
            }
            return null;
        }

        private void StashTree_AfterExpandCollapse(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is not NodeInfo { IsGroup: true } info) return;
            e.Node.ImageIndex = e.Node.SelectedImageIndex = e.Node.IsExpanded ? IconGroupOpen : IconGroup;
            if (_reloading || FilterTxt.TextLength > 0) return; //Expansions while filtering aren't remembered
            if (e.Node.IsExpanded) _expanded.Add(info.Folder); else _expanded.Remove(info.Folder);
        }

        private void FilterTxt_TextChanged(object? sender, EventArgs e)
        {
            FilterTimer.Stop();
            FilterTimer.Start();
        }

        private void FilterTimer_Tick(object? sender, EventArgs e)
        {
            FilterTimer.Stop();
            BuildTree((StashTree.SelectedNode?.Tag as NodeInfo)?.Folder);
        }

        #endregion

        #region Selection

        public StashEntry? SelectedEntry => (StashTree.SelectedNode?.Tag as NodeInfo)?.Entry;

        /// <summary>
        /// The group new stashes are saved into: the selected group, the group of the selected stash, or the top level.
        /// </summary>
        public string SelectedGroup
        {
            get
            {
                if (StashTree.SelectedNode?.Tag is not NodeInfo info) return "";
                return info.IsGroup ? StashLibrary.RelativeGroup(info.Folder) : info.Entry!.Group;
            }
        }

        public void SelectFolder(string folder)
        {
            var node = FindNode(StashTree.Nodes, folder);
            if (node == null) return;
            StashTree.SelectedNode = node;
            node.EnsureVisible();
        }

        private StashEntry? RequireEntry()
        {
            var entry = SelectedEntry;
            if (entry == null) TraceLogger.Log("Select a stash first.", StatusSeverityType.Warning, true);
            return entry;
        }

        private void StashTree_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right) StashTree.SelectedNode = e.Node; //Right-click selects, so the menu works on that node
        }

        private void StashTree_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Node?.Tag is NodeInfo { Entry: not null } info)
                DetailsRequested?.Invoke(info.Entry);
        }

        private void StashTree_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter when SelectedEntry != null:
                    DetailsRequested?.Invoke(SelectedEntry);
                    e.Handled = e.SuppressKeyPress = true;
                    break;
                case Keys.F2:
                    RenameSelected();
                    e.Handled = true;
                    break;
                case Keys.Delete:
                    DeleteSelected();
                    e.Handled = true;
                    break;
                case Keys.F5:
                    Reload();
                    e.Handled = true;
                    break;
                case Keys.F when e.Control:
                    FilterTxt.Focus();
                    FilterTxt.SelectAll();
                    e.Handled = e.SuppressKeyPress = true;
                    break;
            }
        }

        #endregion

        #region Drawing and hover preview

        private void StashTree_DrawNode(object? sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null || e.Bounds.IsEmpty) return;
            var info = e.Node.Tag as NodeInfo;
            bool selected = (e.State & TreeNodeStates.Selected) != 0;
            bool dropTarget = e.Node == _dropTarget;
            var bounds = new Rectangle(e.Bounds.X, e.Bounds.Y, Math.Max(e.Bounds.Width, StashTree.ClientSize.Width - e.Bounds.X), e.Bounds.Height);
            Color back = dropTarget ? Color.FromArgb(90, 110, 190) : selected ? (StashTree.Focused ? Color.FromArgb(70, 85, 160) : Color.FromArgb(60, 66, 110)) : StashTree.BackColor;
            using (var brush = new SolidBrush(back)) e.Graphics.FillRectangle(brush, bounds);
            Color fore = info?.SourceMissing == true ? Color.FromArgb(150, 150, 165) : info?.IsGroup == true ? Color.FromArgb(255, 225, 150) : StashTree.ForeColor;
            var font = info?.IsGroup == true ? new Font(StashTree.Font, FontStyle.Bold) : StashTree.Font;
            try
            {
                string text = e.Node.Text;
                if (info?.IsGroup == true && e.Node.Nodes.Count > 0) text += $"  ({CountStashes(e.Node)})";
                TextRenderer.DrawText(e.Graphics, text, font, new Point(e.Bounds.X, e.Bounds.Y + 2), fore, TextFormatFlags.NoPrefix);
                if (info?.Entry is { Meta.Rating: > 0 } entry)
                {
                    int x = e.Bounds.X + TextRenderer.MeasureText(e.Graphics, text, font, Size.Empty, TextFormatFlags.NoPrefix).Width;
                    TextRenderer.DrawText(e.Graphics, new string('★', Math.Min(5, entry.Meta.Rating)), StashTree.Font, new Point(x, e.Bounds.Y + 2), Color.Gold, TextFormatFlags.NoPrefix);
                }
            }
            finally
            {
                if (font != StashTree.Font) font.Dispose();
            }
        }

        private static int CountStashes(TreeNode node)
        {
            int count = 0;
            foreach (TreeNode child in node.Nodes)
                count += child.Tag is NodeInfo { IsGroup: true } ? CountStashes(child) : 1;
            return count;
        }

        private void StashTree_NodeMouseHover(object? sender, TreeNodeMouseHoverEventArgs e)
        {
            if (e.Node?.Tag is not NodeInfo { Entry: not null } info || StashMenu.Visible) return;
            _previewEntry = info.Entry;
            var pos = StashTree.PointToClient(Cursor.Position);
            PreviewTip.Show(info.Entry.Name, StashTree, pos.X + 16, pos.Y + 16, PreviewTip.AutoPopDelay);
        }

        private void StashTree_MouseLeave(object? sender, EventArgs e)
        {
            PreviewTip.Hide(StashTree);
            _previewEntry = null;
        }

        private const int PreviewWidth = 260, PreviewImageHeight = 170, PreviewPadding = 8;

        private static string PreviewText(StashEntry entry)
        {
            string text = entry.Meta.Describe();
            if (!string.IsNullOrEmpty(entry.Meta.SourcePath) && !File.Exists(entry.Meta.SourcePath))
                text += $"\n⚠ The file isn't at {entry.Meta.SourcePath} anymore.";
            return text;
        }

        private void PreviewTip_Popup(object? sender, PopupEventArgs e)
        {
            if (_previewEntry == null) { e.Cancel = true; return; }
            var image = StashLibrary.ThumbnailCache.Get(_previewEntry.ThumbnailFile);
            int imageHeight = image == null ? 0 : Math.Min(PreviewImageHeight, (int)Math.Round(image.Height * Math.Min(1.0, (double)(PreviewWidth - 2 * PreviewPadding) / image.Width))) + PreviewPadding;
            using var bold = new Font(Font, FontStyle.Bold);
            var titleSize = TextRenderer.MeasureText(_previewEntry.Name, bold, new Size(PreviewWidth - 2 * PreviewPadding, 0), TextFormatFlags.WordBreak);
            var textSize = TextRenderer.MeasureText(PreviewText(_previewEntry), Font, new Size(PreviewWidth - 2 * PreviewPadding, 0), TextFormatFlags.WordBreak);
            e.ToolTipSize = new Size(PreviewWidth, PreviewPadding * 2 + imageHeight + titleSize.Height + 4 + textSize.Height);
        }

        private void PreviewTip_Draw(object? sender, DrawToolTipEventArgs e)
        {
            using (var back = new SolidBrush(Color.FromArgb(28, 27, 55))) e.Graphics.FillRectangle(back, e.Bounds);
            using (var border = new Pen(Color.FromArgb(90, 110, 190))) e.Graphics.DrawRectangle(border, 0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1);
            if (_previewEntry == null) return;
            int y = PreviewPadding, width = e.Bounds.Width - 2 * PreviewPadding;
            var image = StashLibrary.ThumbnailCache.Get(_previewEntry.ThumbnailFile);
            if (image != null)
            {
                double scale = Math.Min(Math.Min(1.0, (double)width / image.Width), (double)PreviewImageHeight / image.Height);
                int w = (int)(image.Width * scale), h = (int)(image.Height * scale);
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(image, PreviewPadding + (width - w) / 2, y, w, h);
                y += h + PreviewPadding;
            }
            using var bold = new Font(Font, FontStyle.Bold);
            var titleRect = new Rectangle(PreviewPadding, y, width, e.Bounds.Height - y);
            TextRenderer.DrawText(e.Graphics, _previewEntry.Name, bold, titleRect, Color.FromArgb(192, 255, 255), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            y += TextRenderer.MeasureText(_previewEntry.Name, bold, new Size(width, 0), TextFormatFlags.WordBreak).Height + 4;
            TextRenderer.DrawText(e.Graphics, PreviewText(_previewEntry), Font, new Rectangle(PreviewPadding, y, width, e.Bounds.Height - y), Color.FromArgb(213, 216, 216), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
        }

        #endregion

        #region Context menu

        private void StashMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            PreviewTip.Hide(StashTree);
            var info = StashTree.SelectedNode?.Tag as NodeInfo;
            bool stash = info?.Entry != null;
            foreach (var item in new ToolStripItem[] { QuickLoadMenuItem, ApplyToLoadedMenuItem, DetailsMenuItem, EditMenuItem, LoadSettingsMenuItem, ScreenshotMenuItem, RatingMenuItem, ExportMenuItem })
                item.Enabled = stash;
            LoadSettingsMenuItem.Enabled = stash && !string.IsNullOrEmpty(info!.Entry!.Meta.Engine);
            QuickLoadMenuItem.Enabled = stash && !string.IsNullOrEmpty(info!.Entry!.Meta.SourcePath);
            RenameMenuItem.Enabled = DeleteMenuItem.Enabled = MoveToMenuItem.Enabled = info != null;
            OpenFolderMenuItem.Enabled = true;
            if (stash)
            {
                foreach (ToolStripMenuItem item in RatingMenuItem.DropDownItems)
                    item.Checked = (int)item.Tag! == info!.Entry!.Meta.Rating;
            }
            BuildMoveMenu(info);
        }

        private void BuildMoveMenu(NodeInfo? info)
        {
            MoveToMenuItem.DropDownItems.Clear();
            if (info == null || _library == null) return;
            void Add(StashLibrary.GroupNode group, int depth)
            {
                bool self = Modules.CorruptionInternals.CorruptionCore.PathsEqual(group.Path, info.Folder);
                if (info.IsGroup && self) return; //A group can't go into itself (or its subgroups)
                string target = group.Group;
                var item = new ToolStripMenuItem((depth == 0 ? "(top level)" : new string(' ', (depth - 1) * 4) + "📁 " + group.Name))
                {
                    Enabled = !Modules.CorruptionInternals.CorruptionCore.PathsEqual(Path.GetDirectoryName(info.Folder), group.Path)
                };
                item.Click += (_, _) => MoveTo(info.Folder, target);
                MoveToMenuItem.DropDownItems.Add(item);
                foreach (var sub in group.Groups) Add(sub, depth + 1);
            }
            Add(_library, 0);
        }

        private void QuickLoadMenuItem_Click(object? sender, EventArgs e) { if (RequireEntry() is { } entry) QuickLoadRequested?.Invoke(entry); }
        private void ApplyToLoadedMenuItem_Click(object? sender, EventArgs e) { if (RequireEntry() is { } entry) ApplyToLoadedRequested?.Invoke(entry); }
        private void DetailsMenuItem_Click(object? sender, EventArgs e) { if (RequireEntry() is { } entry) DetailsRequested?.Invoke(entry); }
        private void EditMenuItem_Click(object? sender, EventArgs e) { if (RequireEntry() is { } entry) EditRequested?.Invoke(entry); }
        private void LoadSettingsMenuItem_Click(object? sender, EventArgs e) { if (RequireEntry() is { } entry) LoadSettingsRequested?.Invoke(entry); }
        private void RenameMenuItem_Click(object? sender, EventArgs e) => RenameSelected();
        private void DeleteMenuItem_Click(object? sender, EventArgs e) => DeleteSelected();
        private void RefreshMenuItem_Click(object? sender, EventArgs e) => Reload();

        private void SetRating(int stars)
        {
            if (RequireEntry() is not { } entry) return;
            try
            {
                entry.Meta.Rating = stars;
                StashLibrary.SaveMeta(entry);
                StashTree.Invalidate();
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to save the rating: {ex.Message}", StatusSeverityType.Error, true);
            }
        }

        private void SortNewestMenuItem_Click(object? sender, EventArgs e)
        {
            Settings.Default.StashSortNewestFirst = SortNewestMenuItem.Checked;
            Settings.Default.Save();
            BuildTree((StashTree.SelectedNode?.Tag as NodeInfo)?.Folder);
        }

        private void AutoScreenshotMenuItem_Click(object? sender, EventArgs e)
        {
            Settings.Default.StashAutoScreenshot = AutoScreenshotMenuItem.Checked;
            Settings.Default.Save();
        }

        private void NewGroupMenuItem_Click(object? sender, EventArgs e)
        {
            string? name = InputBox.Ask("New group name (e.g. SNES Mario)")?.Trim();
            if (string.IsNullOrEmpty(name)) return;
            try
            {
                string folder = StashLibrary.CreateGroup(SelectedGroup, name);
                _expanded.Add(Path.GetDirectoryName(folder)!);
                Reload(folder);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to create the group: {ex.Message}", StatusSeverityType.Error, true);
            }
        }

        public void RenameSelected()
        {
            var node = StashTree.SelectedNode;
            if (node == null) { TraceLogger.Log("Select a stash or group to rename.", StatusSeverityType.Warning, true); return; }
            node.BeginEdit();
        }

        private void StashTree_BeforeLabelEdit(object? sender, NodeLabelEditEventArgs e)
        {
            if (e.Node?.Tag is not NodeInfo) e.CancelEdit = true;
        }

        private void StashTree_AfterLabelEdit(object? sender, NodeLabelEditEventArgs e)
        {
            e.CancelEdit = true; //The tree is rebuilt with the real (folder safe) name
            if (e.Label == null || e.Node?.Tag is not NodeInfo info) return;
            string name = e.Label.Trim();
            if (name.Length == 0) return;
            BeginInvoke(() =>
            {
                try
                {
                    if (info.Entry != null)
                    {
                        if (StashLibrary.Rename(info.Entry, name)) Reload(info.Entry.Folder);
                    }
                    else
                    {
                        string target = Path.Combine(Path.GetDirectoryName(info.Folder)!, StashLibrary.SafeName(name));
                        if (StashLibrary.RenameGroup(info.Folder, name))
                        {
                            if (_expanded.Remove(info.Folder)) _expanded.Add(target);
                            Reload(target);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TraceLogger.Log($"Unable to rename: {ex.Message}", StatusSeverityType.Error, true);
                }
            });
        }

        public void DeleteSelected()
        {
            if (StashTree.SelectedNode?.Tag is not NodeInfo info)
            {
                TraceLogger.Log("Select a stash or group to delete.", StatusSeverityType.Warning, true);
                return;
            }
            string question = info.IsGroup
                ? $"Delete the group '{Path.GetFileName(info.Folder)}' and the {CountStashes(StashTree.SelectedNode)} stash(es) in it?\n\nThis can't be undone."
                : $"Delete the stash '{info.Entry!.Name}' (with its screenshots and info)?\n\nThis can't be undone.";
            if (MessageBox.Show(question, $"{nameof(LunarROMCorruptor)} - Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                if (info.Entry != null) StashLibrary.ThumbnailCache.Invalidate(info.Entry.ThumbnailFile);
                StashLibrary.Delete(info.Folder);
                TraceLogger.Log($"Deleted: {info.Folder}");
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to delete {info.Folder}: {ex.Message}", StatusSeverityType.Error, true);
            }
            Reload(Path.GetDirectoryName(info.Folder));
        }

        private void MoveTo(string folder, string group)
        {
            try
            {
                string? moved = StashLibrary.MoveToGroup(folder, group);
                if (moved == null) return;
                if (!string.IsNullOrEmpty(group)) _expanded.Add(StashLibrary.GroupPath(group));
                Reload(moved);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to move {Path.GetFileName(folder)}: {ex.Message}", StatusSeverityType.Error, true);
            }
        }

        private void ExportMenuItem_Click(object? sender, EventArgs e)
        {
            if (RequireEntry() is not { } entry) return;
            using var dialog = new SaveFileDialog { Filter = $"Shared LRC stash (*{StashLibrary.ExportExtension})|*{StashLibrary.ExportExtension}", FileName = StashLibrary.SafeName(entry.Name) + StashLibrary.ExportExtension, Title = "Export stash to share" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                StashLibrary.Export(entry, dialog.FileName);
                MessageBox.Show($"Exported to {dialog.FileName}.\n\nOthers can import it with Import... or by dropping it on the stash list. They need their own copy of {(string.IsNullOrEmpty(entry.Meta.SourceFileName) ? "the file" : entry.Meta.SourceFileName)}.", $"{nameof(LunarROMCorruptor)} - Export stash", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to export the stash: {ex.Message}", StatusSeverityType.Error, true);
            }
        }

        private void ImportMenuItem_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog { Filter = $"LRC stashes (*{StashLibrary.ExportExtension};*.stash)|*{StashLibrary.ExportExtension};*.stash|All files (*.*)|*.*", Multiselect = true, Title = "Import stashes" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            ImportFiles(dialog.FileNames, SelectedGroup);
        }

        /// <summary>
        /// Imports shared .lrcstash or plain .stash files into a group.
        /// </summary>
        public void ImportFiles(IEnumerable<string> files, string group)
        {
            string? last = null;
            foreach (string file in files)
            {
                var entry = StashLibrary.Import(file, group);
                if (entry != null) last = entry.Folder;
            }
            if (last != null)
            {
                if (!string.IsNullOrEmpty(group)) _expanded.Add(StashLibrary.GroupPath(group));
                Reload(last);
            }
        }

        private void OpenFolderMenuItem_Click(object? sender, EventArgs e)
        {
            string folder = (StashTree.SelectedNode?.Tag as NodeInfo)?.Folder ?? StashLibrary.Root;
            Directory.CreateDirectory(StashLibrary.Root);
            CommandRunner.OpenFolderInExplorer(folder);
        }

        #endregion

        #region Screenshots

        private void AddScreenshot(Func<Bitmap?> source, string what)
        {
            if (RequireEntry() is not { } entry) return;
            Bitmap? image = null;
            try
            {
                image = source();
                if (image == null)
                {
                    TraceLogger.Log($"No screenshot was added: {what}", StatusSeverityType.Warning, true);
                    return;
                }
                StashLibrary.AddScreenshot(entry, image);
                TraceLogger.Log($"Screenshot added to {entry.Name}.");
                Reload(entry.Folder);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to add the screenshot: {ex.Message}", StatusSeverityType.Error, true);
            }
            finally
            {
                image?.Dispose();
            }
        }

        private void ScreenshotEmulatorMenuItem_Click(object? sender, EventArgs e) =>
            AddScreenshot(() => EmulatorScreenshotProvider?.Invoke(), "the emulator isn't running (or its window can't be captured). Start it with 'Run program after corruption', or paste a screenshot instead.");

        private void ScreenshotClipboardMenuItem_Click(object? sender, EventArgs e) =>
            AddScreenshot(() => Clipboard.ContainsImage() && Clipboard.GetImage() is { } img ? new Bitmap(img) : null, "there is no image on the clipboard.");

        private void ScreenshotFileMenuItem_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog { Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*", Title = "Add a screenshot" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            AddScreenshot(() => StashLibrary.LoadImageUnlocked(dialog.FileName) as Bitmap, "the image couldn't be read.");
        }

        private void ScreenshotByteViewMenuItem_Click(object? sender, EventArgs e) =>
            AddScreenshot(() => ByteViewImageProvider?.Invoke(), "there is no ByteView image yet (press Convert/Update on the ByteView tab).");

        #endregion

        #region Drag and drop

        private void StashTree_ItemDrag(object? sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode { Tag: NodeInfo } node && e.Button == MouseButtons.Left)
            {
                PreviewTip.Hide(StashTree);
                StashTree.SelectedNode = node;
                DoDragDrop(node, DragDropEffects.Move);
            }
        }

        /// <summary>
        /// The group a drop at this point goes into: a group node, the group of a stash node, or the top level.
        /// </summary>
        private (TreeNode? Node, string Group) DropGroupAt(Point screen)
        {
            var node = StashTree.GetNodeAt(StashTree.PointToClient(screen));
            if (node?.Tag is NodeInfo info)
            {
                if (info.IsGroup) return (node, StashLibrary.RelativeGroup(info.Folder));
                return (node.Parent, info.Entry!.Group);
            }
            return (null, "");
        }

        private void StashTree_DragOver(object? sender, DragEventArgs e)
        {
            var (target, group) = DropGroupAt(new Point(e.X, e.Y));
            if (e.Data?.GetData(typeof(TreeNode)) is TreeNode { Tag: NodeInfo info })
            {
                string targetPath = StashLibrary.GroupPath(group);
                bool intoItself = info.IsGroup && (Path.GetFullPath(targetPath) + Path.DirectorySeparatorChar).StartsWith(Path.GetFullPath(info.Folder) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                bool sameGroup = Modules.CorruptionInternals.CorruptionCore.PathsEqual(Path.GetDirectoryName(info.Folder), targetPath);
                e.Effect = intoItself || sameGroup ? DragDropEffects.None : DragDropEffects.Move;
            }
            else if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Any(IsImportable))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
            if (target != _dropTarget)
            {
                _dropTarget = e.Effect == DragDropEffects.None ? null : target;
                StashTree.Invalidate();
            }
            //Scroll while dragging near the top/bottom edge
            var client = StashTree.PointToClient(new Point(e.X, e.Y));
            var under = StashTree.GetNodeAt(client);
            if (client.Y < 12) under?.PrevVisibleNode?.EnsureVisible();
            else if (client.Y > StashTree.ClientSize.Height - 12) under?.NextVisibleNode?.EnsureVisible();
        }

        private static bool IsImportable(string file) =>
            Path.GetExtension(file).Equals(StashLibrary.ExportExtension, StringComparison.OrdinalIgnoreCase) || Path.GetExtension(file).Equals(".stash", StringComparison.OrdinalIgnoreCase);

        private void StashTree_DragLeave(object? sender, EventArgs e)
        {
            _dropTarget = null;
            StashTree.Invalidate();
        }

        private void StashTree_DragDrop(object? sender, DragEventArgs e)
        {
            _dropTarget = null;
            StashTree.Invalidate();
            var (_, group) = DropGroupAt(new Point(e.X, e.Y));
            if (e.Data?.GetData(typeof(TreeNode)) is TreeNode { Tag: NodeInfo info })
                MoveTo(info.Folder, group);
            else if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
                ImportFiles(files.Where(IsImportable), group);
        }

        #endregion

        private void LoadStashsFileOnlyBtn_Click(object sender, EventArgs e)
        {
            //Use LoadStashFileFromHeaderOnly
            if (RequireEntry() is { } entry2)
            {
                try
                {
                    if (entry2.Meta.Engine == null)
                    {
                        TraceLogger.Log($"Cannot load stash file from header only: Engine is null for {entry2.Name}", StatusSeverityType.Error, true);
                        return;
                    }
                    else
                    {
                        LoadStashFileFromHeaderOnly!(entry2);
                    }
                    TraceLogger.Log($"Loaded stash file from header only: {entry2.Name}");
                }
                catch (Exception ex)
                {
                    TraceLogger.Log($"Unable to load stash file from header only: {ex.Message}", StatusSeverityType.Error, true);
                }
            }
            if (RequireEntry() is { } entry) LoadSettingsRequested?.Invoke(entry);
        }
    }
}
