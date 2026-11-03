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

namespace LunarROMCorruptor.Forms
{
    /// <summary>
    /// Bisect mode window: finds the stash changes that make a corruption crash.
    /// Every test is applied to the save location (and the emulator is started if that's enabled), then the user says whether it still crashes.
    /// </summary>
    public partial class BisectForm : Form
    {
        private readonly MainCorruptionForm _host;
        private List<(long Offset, byte Value)>? _lastTest;
        private bool _working;

        /// <summary>
        /// The running bisect (null when none has been started). ByteView draws the suspects and the half that's left out of the current test.
        /// </summary>
        public BisectSession? Session { get; private set; }

        /// <summary>
        /// Raised when the window is closed by the user (the main form unticks the Bisect Mode checkbox).
        /// </summary>
        public event Action? ClosedByUser;

        public BisectForm(MainCorruptionForm host)
        {
            InitializeComponent();
            _host = host;
            CorruptionCore.Exclusions.Changed += OnExclusionsChanged;
            FormClosing += (_, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true; //Keep the bisect state, just hide the window
                    Hide();
                    ClosedByUser?.Invoke();
                }
            };
            FormClosed += (_, _) => CorruptionCore.Exclusions.Changed -= OnExclusionsChanged;
            UpdateUi();
        }

        private void OnExclusionsChanged()
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(UpdateUi);
        }

        private static string Span(ByteRange? r) => r is { } x ? $"0x{x.Start:X} - 0x{x.End:X}" : "-";

        private void UpdateUi()
        {
            var s = Session;
            bool idle = !_working;
            BisectStartBtn.Enabled = idle;
            BisectCrashBtn.Enabled = idle && s is { TestPending: true };
            BisectWorksBtn.Enabled = idle && s is { TestPending: true };
            BisectRetestBtn.Enabled = idle && _lastTest != null && s is { TestPending: true };
            BisectUndoBtn.Enabled = idle && s is { CanUndo: true };
            BisectExcludeBtn.Enabled = idle && s != null;
            BisectApplyWithoutBtn.Enabled = idle && s != null;
            BisectResetExclusionsBtn.Enabled = idle && CorruptionCore.Exclusions.Count > 0;

            int ranges = CorruptionCore.Exclusions.Count;
            BisectExclusionsLbl.Text = ranges == 0 ? "No excluded ranges." : $"Excluded: {ranges} range(s), {CorruptionCore.FormatSize(CorruptionCore.Exclusions.TotalBytes, out _, out _)} (red in ByteView). Every engine skips them.";

            if (s == null) return;
            if (_working)
            {
                BisectStatusLbl.Text = "Applying the test...";
            }
            else if (s.Finished)
            {
                var (Offset, Value) = s.Suspects.FirstOrDefault();
                BisectStatusLbl.Text = s.SuspectCount == 1
                    ? $"Found it after {s.Round - 1} round(s): the change at 0x{Offset:X} (set to {Value}) makes it crash.\n\nExclude it, or apply the stash without it to get your corruption back without the crash. If that still crashes, start again: there's more than one crashing change."
                    : "No suspects are left. The crash probably needs changes from both halves; undo and answer differently, or start again.";
            }
            else if (s.TestPending)
            {
                BisectStatusLbl.Text = $"Round {s.Round} (about {s.RoundsLeft} left). {s.SuspectCount:N0} suspect change(s) in {Span(s.SuspectSpan)}.\n\nThis test leaves out {Span(s.RemovedSpan)} (cyan in ByteView). Run it: does it still crash?";
            }
            else
            {
                BisectStatusLbl.Text = $"{s.SuspectCount:N0} suspect change(s) in {Span(s.SuspectSpan)}. You can exclude them now or keep going.";
            }
        }

        private async Task RunTest(List<(long Offset, byte Value)> entries, string status)
        {
            _working = true;
            UpdateUi();
            try
            {
                _lastTest = entries;
                bool ok = await _host.ApplyEntriesForBisectAsync(entries, status);
                if (!ok) TraceLogger.Log("Bisect: the test couldn't be applied, see the log.", StatusSeverityType.Warning);
            }
            finally
            {
                _working = false;
                UpdateUi();
                _host.InvalidateByteViewOverlays();
            }
        }

        private async Task NextRound()
        {
            var s = Session!;
            if (s.Finished)
            {
                TraceLogger.Log($"Bisect finished: {string.Join(", ", s.Suspects.Select(e => $"0x{e.Offset:X}"))}");
                UpdateUi();
                _host.InvalidateByteViewOverlays();
                return;
            }
            var test = s.NextTest();
            TraceLogger.Log($"Bisect round {s.Round}: {s.SuspectCount:N0} suspects, leaving out {Span(s.RemovedSpan)}.");
            await RunTest(test, $"Bisect round {s.Round}");
        }

        private async void BisectStartBtn_Click(object? sender, EventArgs e)
        {
            var stash = CorruptionCore.Stash;
            if (stash.HasTextEntries)
            {
                TraceLogger.Log("Bisect only works with byte stashes ([x] File(i).SET(v)), not with Source Code or Datamosh stashes.", StatusSeverityType.Warning, true);
                return;
            }
            if (stash.ByteEntries.Count < 2)
            {
                TraceLogger.Log("Bisect needs a corruption to work on: corrupt the file (or apply a stash) until it crashes, then press Start.", StatusSeverityType.Warning, true);
                return;
            }
            if (Session != null && !Session.Finished && MessageBox.Show(this, "A bisect is already running. Start a new one from the current stash?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            Session = new BisectSession(stash.ByteEntries);
            TraceLogger.Log($"Bisect started with {Session.Total:N0} change(s).");
            await NextRound();
        }

        private async void Answer(bool stillCrashes)
        {
            if (Session is not { TestPending: true }) return;
            Session.Answer(stillCrashes);
            await NextRound();
        }

        private void BisectCrashBtn_Click(object? sender, EventArgs e) => Answer(true);

        private void BisectWorksBtn_Click(object? sender, EventArgs e) => Answer(false);

        private async void BisectRetestBtn_Click(object? sender, EventArgs e)
        {
            if (_lastTest != null) await RunTest(_lastTest, "Bisect re-test");
        }

        private async void BisectUndoBtn_Click(object? sender, EventArgs e)
        {
            if (Session is not { CanUndo: true }) return;
            Session.Undo();
            await NextRound();
        }

        private void ExcludeSuspects()
        {
            var ranges = BisectSession.ToRanges(Session!.Suspects.Select(x => x.Offset), BisectWordAlignChkBox.Checked);
            CorruptionCore.Exclusions.AddRange(ranges);
            TraceLogger.Log($"Bisect: excluded {ranges.Count} range(s): {string.Join(", ", ranges.Take(20))}{(ranges.Count > 20 ? ", ..." : "")}");
        }

        private async void BisectExcludeBtn_Click(object? sender, EventArgs e)
        {
            if (Session == null) return;
            ExcludeSuspects();
            await _host.SyncStashExcludesAsync(); //Redraws ByteView and keeps the current stash's EXCLUDE lines up to date
        }

        private async void BisectApplyWithoutBtn_Click(object? sender, EventArgs e)
        {
            if (Session == null) return;
            ExcludeSuspects();
            var rest = Session.WithoutSuspects();
            Session = null;
            _lastTest = null;
            await RunTest(rest, "Applying the stash without the suspects");
            BisectStatusLbl.Text = $"Applied the stash without the suspects ({rest.Count:N0} change(s)). If it still crashes, press Start again to find the next crashing change.";
        }

        private async void BisectResetExclusionsBtn_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show(this, $"Remove all {CorruptionCore.Exclusions.Count} excluded range(s)? Every engine will be able to change those bytes again.", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            CorruptionCore.Exclusions.Clear();
            TraceLogger.Log("All excluded ranges were reset.");
            await _host.SyncStashExcludesAsync();
        }
    }
}
