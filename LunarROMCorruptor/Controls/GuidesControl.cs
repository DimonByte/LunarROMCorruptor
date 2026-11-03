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

namespace LunarROMCorruptor.Controls
{
    /// <summary>
    /// The 📖 Guides tab: shows the README and docs/*.md guides inside LRC. Each guide is downloaded from GitHub when it's opened, so
    /// updating the .md files in the repository updates the guides in the program. Until the download finishes (or when offline) the
    /// copy shipped with LRC (Guides folder) or the last downloaded copy is shown. Links between guides open inside LRC.
    /// </summary>
    public partial class GuidesControl : UserControl
    {
        private string _current = Guides.All[0].RepoPath;
        private string? _pendingAnchor;
        private readonly Stack<(string Path, string? Anchor)> _history = new();
        private CancellationTokenSource? _loadCts;
        private bool _showingPage;

        public GuidesControl()
        {
            InitializeComponent();
            foreach (var guide in Guides.All) GuideList.Items.Add(guide.Title);
            BackBtn.Enabled = false;
        }

        /// <summary>
        /// Shows the first guide the first time the tab is opened (nothing is downloaded before that).
        /// </summary>
        public void EnsureLoaded()
        {
            if (GuideList.SelectedIndex < 0) GuideList.SelectedIndex = 0;
        }

        /// <summary>
        /// Opens a guide (repository path like "docs/ByteView.md") at an optional #anchor.
        /// </summary>
        public void Open(string repoPath, string? anchor = null, bool remember = true)
        {
            if (remember && _showingPage) _history.Push((_current, null));
            BackBtn.Enabled = _history.Count > 0;
            _current = repoPath;
            int index = Array.FindIndex(Guides.All, g => g.RepoPath.Equals(repoPath, StringComparison.OrdinalIgnoreCase));
            if (index >= 0 && GuideList.SelectedIndex != index)
            {
                GuideList.SelectedIndexChanged -= GuideList_SelectedIndexChanged;
                GuideList.SelectedIndex = index;
                GuideList.SelectedIndexChanged += GuideList_SelectedIndexChanged;
            }
            _ = LoadAsync(repoPath, anchor, forceDownload: false);
        }

        private async Task LoadAsync(string repoPath, string? anchor, bool forceDownload)
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = new CancellationTokenSource();
            var token = _loadCts.Token;
            string title = TitleOf(repoPath);
            var (local, source) = Guides.LoadLocal(repoPath);
            bool shownLocal = false;
            if (local != null && !(forceDownload && source == Guides.Source.Online))
            {
                Show(local, repoPath, title, anchor);
                shownLocal = true;
                SetStatus(source == Guides.Source.Online ? "Latest version from GitHub." : "Checking GitHub for the latest version...");
                if (source == Guides.Source.Online && !forceDownload) return;
            }
            else
            {
                SetStatus("Downloading from GitHub...");
                if (local == null) GuideBrowser.DocumentText = MarkdownRenderer.ToHtmlPage($"# {title}\n\nLoading...", repoPath, title);
            }
            string? online;
            try
            {
                online = await Guides.DownloadAsync(repoPath, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            if (token.IsCancellationRequested || IsDisposed || !_current.Equals(repoPath, StringComparison.OrdinalIgnoreCase)) return;
            if (online == null)
            {
                SetStatus(shownLocal ? (source == Guides.Source.Bundled ? "Offline: showing the copy that came with LRC." : "Offline: showing the last downloaded copy.") : "This guide couldn't be downloaded and there is no offline copy.");
                if (!shownLocal) GuideBrowser.DocumentText = MarkdownRenderer.ToHtmlPage($"# {title}\n\nThis guide couldn't be downloaded (no internet connection?) and there is no offline copy in the Guides folder.\n\n[Try again]({repoPath})", repoPath, title);
                return;
            }
            if (!shownLocal || online != local || forceDownload) Show(online, repoPath, title, _pendingAnchor ?? anchor);
            SetStatus("Latest version from GitHub.");
        }

        private static string TitleOf(string repoPath) => Guides.All.FirstOrDefault(g => g.RepoPath.Equals(repoPath, StringComparison.OrdinalIgnoreCase))?.Title ?? Path.GetFileNameWithoutExtension(repoPath);

        private void Show(string markdown, string repoPath, string title, string? anchor)
        {
            _pendingAnchor = anchor;
            _showingPage = true;
            GuideBrowser.DocumentText = MarkdownRenderer.ToHtmlPage(markdown, repoPath, title);
        }

        private void SetStatus(string text)
        {
            GuideStatusLbl.Text = text;
            ToolTipHelper.SetToolTip(GuideStatusLbl, text);
        }

        private void ScrollTo(string? anchor)
        {
            if (string.IsNullOrEmpty(anchor) || GuideBrowser.Document == null) return;
            var element = GuideBrowser.Document.GetElementById(anchor) ?? GuideBrowser.Document.GetElementById(anchor.ToLowerInvariant());
            element?.ScrollIntoView(true);
        }

        private void GuideBrowser_DocumentCompleted(object? sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ScrollTo(_pendingAnchor);
            _pendingAnchor = null;
        }

        /// <summary>
        /// Guide links (lrcguide:) open inside LRC, #anchors scroll, everything else opens in the web browser.
        /// </summary>
        private void GuideBrowser_Navigating(object? sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.OriginalString;
            if (url.Equals("about:blank", StringComparison.OrdinalIgnoreCase)) return; //Setting DocumentText
            e.Cancel = true;
            if (url.StartsWith(MarkdownRenderer.GuideScheme, StringComparison.OrdinalIgnoreCase))
            {
                string target = url[MarkdownRenderer.GuideScheme.Length..];
                string? anchor = null;
                int hash = target.IndexOf('#');
                if (hash >= 0) { anchor = target[(hash + 1)..]; target = target[..hash]; }
                if (target.Equals(_current, StringComparison.OrdinalIgnoreCase)) ScrollTo(anchor);
                else Open(Uri.UnescapeDataString(target), anchor);
                return;
            }
            if (url.StartsWith("about:blank#", StringComparison.OrdinalIgnoreCase) || url.StartsWith("about:#", StringComparison.OrdinalIgnoreCase))
            {
                ScrollTo(url[(url.IndexOf('#') + 1)..]);
                return;
            }
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                OpenExternal(url);
            }
        }

        private static void OpenExternal(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to open {url}: {ex.Message}", StatusSeverityType.Warning, true);
            }
        }

        private void GuideList_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (GuideList.SelectedIndex < 0) return;
            Open(Guides.All[GuideList.SelectedIndex].RepoPath);
        }

        private void BackBtn_Click(object? sender, EventArgs e)
        {
            if (_history.Count == 0) return;
            var (path, anchor) = _history.Pop();
            Open(path, anchor, remember: false);
        }

        private void ReloadBtn_Click(object? sender, EventArgs e) => _ = LoadAsync(_current, null, forceDownload: true);

        private void OpenOnlineBtn_Click(object? sender, EventArgs e) => OpenExternal(Guides.WebBaseUrl + _current);
    }
}
