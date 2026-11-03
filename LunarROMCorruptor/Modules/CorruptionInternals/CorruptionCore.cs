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


using LunarROMCorruptor.Modules.CorruptionInternals.Engines;
using LunarROMCorruptor.Modules.Helpers;
using System.Text;
using System.Text.RegularExpressions;

namespace LunarROMCorruptor.Modules.CorruptionInternals
{
    public enum CorruptionResult
    {
        Success,
        Cancelled,
        Failed
    }

    /// <summary>
    /// The core of LRC. Files are no longer loaded into memory with File.ReadAllBytes; instead the output file is opened through a
    /// paged <see cref="FileBuffer"/> and only the pages that the engines touch are read/written. This removes the old 2 GB limit
    /// and keeps memory usage flat no matter how large the file is.
    ///
    /// Restore model:
    ///  - Save location != original file: the original is never written to. Before every corruption the output is brought back to the
    ///    original state (either by re-copying only the pages that were changed last time, or with a full copy), then corrupted.
    ///    Restore copies the original content back over the output.
    ///  - Save location == original file (in-place): before a page is modified for the first time its original content is written to an
    ///    undo journal in %TEMP%. Every corruption starts by replaying the journal (so corruptions don't stack), and Restore replays it too.
    /// </summary>
    public class CorruptionCore
    {
        internal static string? SourcePath; //The file that is loaded into LRC. In single file mode this file is only written to when the save location is the same file.
        internal static long SourceLength; //The size of the loaded file in bytes
        internal static long MaxByte; //This stores the maximum byte index of the file that is loaded (SourceLength - 1)
        internal static double rawFileSize; //This stores the file size of the loaded ROM, used for displaying the size
        internal static string? fileSizeUnit; //This stores the unit of the file size (E.g. MB, GB)
        internal static string? LastOutputPath; //The last file that was written by a corruption (used by ByteView)
        internal static readonly StashLog Stash = new(); //Records every byte that the engines change (replaces the old InternalStashItems string list)
        /// <summary>
        /// Byte ranges of the loaded file that no engine may change (from Bisect mode or EXCLUDE stash lines). Cleared when a different file is loaded.
        /// </summary>
        internal static readonly ExcludeList Exclusions = new();
        /// <summary>
        /// Set from the "Fix Checksums" checkbox. When true, known header checksums (N64, GB, GBA, SNES, Genesis, CD sector EDC/ECC) are recalculated after corrupting.
        /// </summary>
        internal static bool FixChecksums = true;

        public static bool IsLoaded => SourcePath != null;

        private static OutputState? _output;
        private static InPlaceJournal? _journal;
        private static readonly Dictionary<string, InPlaceJournal> _queueJournals = new(StringComparer.OrdinalIgnoreCase); //Undo journals for corruption queue files
        private static readonly Lock _sessionLock = new();

        private sealed class OutputState
        {
            public required string Path;
            public readonly HashSet<long> TouchedPages = [];
            public DateTime LastWriteUtc;
            public bool Valid; //False if the output file content is unknown (e.g. Source Code Engine rewrote it, or the user replaced it)
        }

        /// <summary>
        /// Ensures that byte values don't go under 0 or above 255.
        /// </summary>
        public static byte ClampByte(long x)
        {
            if (x < 0)
                return 0;
            if (x > 255)
                return 255;
            return (byte)x;
        }

        public static bool PathsEqual(string? a, string? b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
            try
            {
                return string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsInPlace(string outputPath) => PathsEqual(SourcePath, outputPath);

        /// <summary>
        /// True when the loaded file has been corrupted in place and hasn't been restored yet.
        /// </summary>
        public static bool HasUnrestoredInPlaceChanges => _journal != null && _journal.HasChanges;

        /// <summary>
        /// True when at least one corruption queue file is currently corrupted and can be restored.
        /// </summary>
        public static bool HasUnrestoredQueueChanges
        {
            get
            {
                lock (_sessionLock)
                    return _queueJournals.Values.Any(j => j.HasChanges);
            }
        }

        private static InPlaceJournal GetQueueJournal(string filePath)
        {
            string full = Path.GetFullPath(filePath);
            if (!_queueJournals.TryGetValue(full, out var journal))
            {
                journal = new InPlaceJournal(full);
                _queueJournals[full] = journal;
            }
            return journal;
        }

        /// <summary>
        /// Restores every corruption queue file back to its original content. Returns how many files were restored.
        /// </summary>
        public static int RestoreQueueFiles()
        {
            int restored = 0;
            lock (_sessionLock)
            {
                foreach (var (path, journal) in _queueJournals)
                {
                    if (!journal.HasChanges) continue;
                    try
                    {
                        journal.RevertAll();
                        restored++;
                    }
                    catch (Exception ex)
                    {
                        TraceLogger.Log($"Unable to restore queue file {path}: {ex.Message}", StatusSeverityType.Error);
                    }
                }
            }
            TraceLogger.Log($"Restored {restored} corruption queue file(s) to their originals.");
            return restored;
        }

        /// <summary>
        /// Deletes the undo journals of the corruption queue (called when LRC closes).
        /// </summary>
        public static void ResetQueueSession()
        {
            lock (_sessionLock)
            {
                foreach (var journal in _queueJournals.Values)
                    journal.Dispose();
                _queueJournals.Clear();
            }
        }

        public static string FormatSize(long bytes, out double value, out string unit)
        {
            if (bytes >= 1073741824) // 1 GiB or more
            {
                unit = "GB";
                value = Math.Round((double)bytes / 1073741824, 2);
            }
            else if (bytes < 1000000) // Less than 1 MB
            {
                unit = "KB";
                value = Math.Round((double)bytes / 1000, 2);
            }
            else
            {
                unit = "MB";
                value = Math.Round((double)bytes / 1000000, 2);
            }
            return $"{value} {unit}";
        }

        /// <summary>
        /// Checks that a file exists, isn't empty and can be read. If it can't be read and the permission override is enabled, the override is attempted.
        /// </summary>
        public static bool ValidateReadableFile(string FileLocation, bool attemptOverride)
        {
            if (string.IsNullOrWhiteSpace(FileLocation))
            {
                TraceLogger.Log("No file location given.", StatusSeverityType.Error);
                return false;
            }
            FileInfo fileInfo;
            try
            {
                fileInfo = new FileInfo(FileLocation);
                if (!fileInfo.Exists)
                {
                    TraceLogger.Log($"File does not exist (or LRC isn't allowed to see it): {FileLocation}", StatusSeverityType.Error);
                    return false;
                }
                if (fileInfo.Length == 0)
                {
                    TraceLogger.Log($"The selected file is empty: {FileLocation}", StatusSeverityType.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to read file information for {FileLocation}: {ex.Message}", StatusSeverityType.Error);
                return false;
            }

            if (FileOverrideHelper.CanOpen(FileLocation, FileAccess.Read, out string? reason))
                return true;

            TraceLogger.Log($"Unable to open {FileLocation} for reading: {reason}", StatusSeverityType.Warning);
            if (attemptOverride && FileOverrideHelper.AttemptProtectedFileOverride(FileLocation) && FileOverrideHelper.CanOpen(FileLocation, FileAccess.Read, out reason))
                return true;

            TraceLogger.Log($"File can't be read: {FileLocation}. {reason}", StatusSeverityType.Error);
            return false;
        }

        /// <summary>
        /// "Loads" a file into LRC. Nothing is read into memory anymore, the file is only validated and its size recorded. This doesn't handle GUI updates.
        /// </summary>
        public static bool LoadROM(string FileLocation, bool attemptOverride = false)
        {
            TraceLogger.Log($"Attempting to load ROM from: {FileLocation}");
            if (!ValidateReadableFile(FileLocation, attemptOverride))
            {
                TraceLogger.Log("Aborting load.", StatusSeverityType.Error, true);
                return false;
            }

            if (HasUnrestoredInPlaceChanges)
            {
                TraceLogger.Log($"Note: {SourcePath} was corrupted in place and wasn't restored before loading a new file. Its undo journal has been discarded.", StatusSeverityType.Warning);
            }
            ResetSession();

            string newPath = Path.GetFullPath(FileLocation);
            if (Exclusions.Count > 0 && !PathsEqual(newPath, SourcePath))
            {
                TraceLogger.Log($"Excluded byte ranges were cleared ({Exclusions.Count} range(s)), they belonged to the previous file.");
                Exclusions.Clear();
            }
            SourcePath = newPath;
            SourceLength = new FileInfo(SourcePath).Length;
            MaxByte = SourceLength - 1;
            FormatSize(SourceLength, out rawFileSize, out string unit);
            fileSizeUnit = unit;
            Stash.Clear();
            TraceLogger.Log($"File loaded: {SourcePath} ({SourceLength} bytes, {rawFileSize} {fileSizeUnit})");
            return true;
        }

        /// <summary>
        /// Adds the stash headers (the loaded file and the save location) so "Corrupt File and Stash" can load the file and replay the stash in one click.
        /// Written as [x] File.OpenLocation("path") and [x] File.SaveAsLocation("path") at the top of the stash.
        /// </summary>
        internal static void AddStashHeaders(string saveAsPath)
        {
            Stash.SetHeaders(SourcePath, saveAsPath);
        }

        /// <summary>
        /// Clears the undo journal and the output tracking. Called when a new file is loaded or the program closes.
        /// </summary>
        public static void ResetSession()
        {
            lock (_sessionLock)
            {
                _journal?.Dispose();
                _journal = null;
                _output = null;
                LastOutputPath = null;
            }
        }

        /// <summary>
        /// Marks the given output file as "unknown content", so the next corruption does a full copy of the original first.
        /// </summary>
        public static void InvalidateOutput(string path)
        {
            lock (_sessionLock)
            {
                if (_output != null && PathsEqual(_output.Path, path))
                    _output.Valid = false;
                if (IsInPlace(path) && _journal != null)
                {
                    TraceLogger.Log("The original file was replaced from outside the corruption core, the undo journal for it has been discarded.", StatusSeverityType.Warning);
                    _journal.Dispose();
                    _journal = null;
                }
            }
        }

        #region Corruption entry points

        /// <summary>
        /// Main corruption function (single file mode). Runs on a background thread.
        /// The output is first brought back to the original state, then the engine corrupts it.
        /// </summary>
        public static CorruptionResult StartCorruption(CorruptionSettings settings, string outputPath, bool recordStash, CancellationToken token, IProgress<double>? progress)
        {
            if (!IsLoaded || SourcePath == null)
            {
                TraceLogger.Log("No file is loaded. Aborting corruption.", StatusSeverityType.Error, true);
                return CorruptionResult.Failed;
            }
            TraceLogger.Log($"Starting corruption with the following settings:\nSource: {SourcePath}\nOutput: {outputPath}\nStart Byte: {settings.StartByte}\nEnd Byte: {settings.EndByte}\nCorrupt Every Nth Byte: {settings.EveryNth}\nIntensity: {settings.Intensity}\nCorruption Engine: {EngineParser.GetEngineDisplayName(settings.Engine)}");

            Stash.Clear();
            Stash.Enabled = recordStash;
            if (recordStash)
                AddStashHeaders(outputPath);
            var rnd = new Random();

            if (settings.Engine is CorruptionEngineType.SourceCodeEngine or CorruptionEngineType.DatamoshEngine)
            {
                return RunTextEngine(settings, outputPath, rnd, token, progress);
            }

            FileBuffer? buffer = null;
            try
            {
                buffer = PrepareOutput(outputPath);
                var checksums = ChecksumFixer.Detect(buffer); //Checked on the original bytes, before the engine runs
                var ctx = new CorruptionContext(buffer, settings, rnd, recordStash ? Stash : null, token, progress);
                EngineProcessor.Run(settings, ctx);
                token.ThrowIfCancellationRequested();
                Stash.SetExcludes(settings.Excluded);
                if (ctx.SkippedExcluded > 0)
                    TraceLogger.Log($"{ctx.SkippedExcluded} change(s) were blocked by the excluded ranges.");
                checksums?.Fix(buffer);
                CommitOutput(buffer, outputPath);
                buffer = null;
                TraceLogger.Log($"Corruption finished. {Stash.Count} byte changes recorded.");
                return CorruptionResult.Success;
            }
            catch (OperationCanceledException)
            {
                TraceLogger.Log("Corruption cancelled by the user. Reverting the output file to its original state...", StatusSeverityType.Warning);
                AbortOutput(buffer, outputPath);
                return CorruptionResult.Cancelled;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Corruption failed: {ex.Message}\n{ex}", StatusSeverityType.Error, true);
                AbortOutput(buffer, outputPath);
                return CorruptionResult.Failed;
            }
        }

        /// <summary>
        /// Engines that rewrite the whole file (the file length can change): Source Code Engine and Datamosh Engine.
        /// </summary>
        private static void RunRewriteEngine(CorruptionSettings settings, CorruptionContext ctx, string inputPath, string outputPath)
        {
            switch (settings.Engine)
            {
                case CorruptionEngineType.SourceCodeEngine:
                    SourceCodeEngine.Run(settings.SourceCode ?? throw new InvalidOperationException("Source Code Engine settings missing."), ctx, inputPath, outputPath);
                    break;
                case CorruptionEngineType.DatamoshEngine:
                    DatamoshEngine.Run(settings.Datamosh ?? throw new InvalidOperationException("Datamosh Engine settings missing."), ctx, inputPath, outputPath);
                    break;
                default:
                    throw new InvalidOperationException($"{settings.Engine} isn't a rewrite engine.");
            }
        }

        private static CorruptionResult RunTextEngine(CorruptionSettings settings, string outputPath, Random rnd, CancellationToken token, IProgress<double>? progress)
        {
            try
            {
                PrepareTextOutput(outputPath);
                var ctx = new CorruptionContext(null, settings, rnd, Stash.Enabled ? Stash : null, token, progress);
                RunRewriteEngine(settings, ctx, SourcePath!, outputPath);
                FinishTextOutput(outputPath);
                return CorruptionResult.Success;
            }
            catch (OperationCanceledException)
            {
                TraceLogger.Log("Corruption cancelled by the user. The output file was not changed.", StatusSeverityType.Warning);
                FinishTextOutput(outputPath);
                return CorruptionResult.Cancelled;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"{EngineParser.GetEngineDisplayName(settings.Engine)} failed: {ex.Message}\n{ex}", StatusSeverityType.Error, true);
                FinishTextOutput(outputPath);
                return CorruptionResult.Failed;
            }
        }

        /// <summary>
        /// Corruption queue mode: corrupts a file in place. Like in-place single file corruption, the original bytes are journaled first,
        /// so every queue corruption starts from the original files and Restore puts all the queue files back.
        /// </summary>
        public static CorruptionResult CorruptFileInPlace(CorruptionSettings settings, string filePath, bool attemptOverride, CancellationToken token, IProgress<double>? progress)
        {
            TraceLogger.Log($"Starting queue corruption for file: {filePath}");
            if (!ValidateReadableFile(filePath, attemptOverride))
                return CorruptionResult.Failed;
            if (!EnsureWritable(filePath, attemptOverride))
                return CorruptionResult.Failed;

            InPlaceJournal journal;
            try
            {
                lock (_sessionLock)
                {
                    journal = GetQueueJournal(filePath);
                    journal.RevertAll(); //Restore the original before corrupting again
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to restore {filePath} to its original before corrupting: {ex.Message}", StatusSeverityType.Error);
                return CorruptionResult.Failed;
            }

            var rnd = new Random();
            if (settings.Engine is CorruptionEngineType.SourceCodeEngine or CorruptionEngineType.DatamoshEngine)
            {
                //Text and video engines change the file length, so a full backup is kept instead of page journaling
                try
                {
                    lock (_sessionLock) journal.EnsureFullBackup();
                    var ctx = new CorruptionContext(null, settings, rnd, null, token, progress);
                    RunRewriteEngine(settings, ctx, filePath, filePath);
                    return CorruptionResult.Success;
                }
                catch (OperationCanceledException)
                {
                    lock (_sessionLock) journal.RevertAll();
                    return CorruptionResult.Cancelled;
                }
                catch (Exception ex)
                {
                    TraceLogger.Log($"{EngineParser.GetEngineDisplayName(settings.Engine)} failed for {filePath}: {ex.Message}", StatusSeverityType.Error);
                    try { lock (_sessionLock) journal.RevertAll(); } catch { }
                    return CorruptionResult.Failed;
                }
            }

            FileBuffer? buffer = null;
            try
            {
                buffer = FileBuffer.OpenReadWrite(filePath);
                buffer.PageFirstModified = journal.Record;
                var fileSettings = settings.ClampTo(buffer.Length);
                fileSettings.Excluded = null; //Excluded ranges belong to the loaded file, not to queue files
                var checksums = ChecksumFixer.Detect(buffer);
                var ctx = new CorruptionContext(buffer, fileSettings, rnd, null, token, progress);
                EngineProcessor.Run(fileSettings, ctx);
                token.ThrowIfCancellationRequested();
                checksums?.Fix(buffer);
                buffer.Flush();
                return CorruptionResult.Success;
            }
            catch (OperationCanceledException)
            {
                buffer?.Dispose(flush: false);
                buffer = null;
                lock (_sessionLock) journal.RevertAll();
                TraceLogger.Log($"Queue corruption cancelled. {filePath} has been restored to its original.", StatusSeverityType.Warning);
                return CorruptionResult.Cancelled;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Failed to corrupt {filePath}: {ex.Message}\n{ex}", StatusSeverityType.Error);
                buffer?.Dispose(flush: false);
                buffer = null;
                try { lock (_sessionLock) journal.RevertAll(); } catch { }
                return CorruptionResult.Failed;
            }
            finally
            {
                buffer?.Dispose();
            }
        }

        /// <summary>
        /// Applies a stash file to a fresh copy of the loaded file. Supports both the byte format and the Source Code Engine VARLINE format.
        /// </summary>
        public static CorruptionResult ApplyStashFile(string stashFileLocation, string outputPath, CancellationToken token, IProgress<double>? progress)
        {
            if (!IsLoaded || SourcePath == null)
            {
                TraceLogger.Log("Attempted to corrupt using stash file but no file is loaded.", StatusSeverityType.Error, true);
                return CorruptionResult.Failed;
            }

            var byteEntries = new List<(long Offset, byte Value)>();
            var textEntries = new List<StashParser.VarLineEntry>();
            var frameEntries = new List<StashParser.FrameEntry>();
            var excludeEntries = new List<ByteRange>();
            long lineNumber = 0;
            try
            {
                foreach (var rawLine in File.ReadLines(stashFileLocation))
                {
                    lineNumber++;
                    string line = rawLine.Trim();
                    if (line.Length == 0) continue;
                    if (StashParser.TryParseByte(line, out long offset, out byte value))
                    {
                        if (offset >= SourceLength)
                        {
                            TraceLogger.Log($"Stash line {lineNumber} points outside of the file (offset {offset}, file size {SourceLength}). Corruption has been aborted.", StatusSeverityType.Error, true);
                            return CorruptionResult.Failed;
                        }
                        byteEntries.Add((offset, value));
                    }
                    else if (StashParser.TryParseVarLine(line, out var entry))
                    {
                        textEntries.Add(entry);
                    }
                    else if (StashParser.TryParseFrame(line, out var frame))
                    {
                        frameEntries.Add(frame);
                    }
                    else if (StashParser.TryParseExclude(line, out var exclude))
                    {
                        excludeEntries.Add(exclude);
                    }
                    else if (StashParser.TryParseHeader(line, out _, out _))
                    {
                        //OpenLocation/SaveAsLocation headers are only used by "Corrupt File and Stash" to load the file first
                    }
                    else
                    {
                        TraceLogger.Log($"Stash line {lineNumber} is invalid: '{line}'. Corruption has been aborted.", StatusSeverityType.Error, true);
                        return CorruptionResult.Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error reading stash file {stashFileLocation}: {ex.Message}", StatusSeverityType.Error, true);
                return CorruptionResult.Failed;
            }

            Stash.Clear();
            Stash.Enabled = true;
            AddStashHeaders(outputPath);

            if (excludeEntries.Count > 0)
            {
                //EXCLUDE lines take priority over every SET line and stay active for the following corruptions until they're reset
                Exclusions.AddRange(excludeEntries);
                TraceLogger.Log($"The stash has {excludeEntries.Count} EXCLUDE line(s). These ranges are now excluded from every engine until the exclusions are reset.");
            }

            if (frameEntries.Count > 0)
            {
                if (byteEntries.Count > 0 || textEntries.Count > 0)
                    TraceLogger.Log("This stash contains Datamosh (FRAME) entries and other entries. Only the FRAME entries will be applied.", StatusSeverityType.Warning);
                try
                {
                    PrepareTextOutput(outputPath);
                    DatamoshEngine.ApplyStash(frameEntries, SourcePath, outputPath, Stash);
                    FinishTextOutput(outputPath);
                    return CorruptionResult.Success;
                }
                catch (Exception ex)
                {
                    TraceLogger.Log($"Failed to apply Datamosh stash: {ex.Message}\n{ex}", StatusSeverityType.Error, true);
                    FinishTextOutput(outputPath);
                    return CorruptionResult.Failed;
                }
            }

            if (textEntries.Count > 0)
            {
                if (byteEntries.Count > 0)
                    TraceLogger.Log("This stash contains both byte entries and Source Code Engine entries. Only the Source Code Engine (VARLINE) entries will be applied.", StatusSeverityType.Warning);
                try
                {
                    PrepareTextOutput(outputPath);
                    SourceCodeEngine.ApplyStash(textEntries, SourcePath, outputPath, Stash);
                    FinishTextOutput(outputPath);
                    return CorruptionResult.Success;
                }
                catch (Exception ex)
                {
                    TraceLogger.Log($"Failed to apply Source Code stash: {ex.Message}\n{ex}", StatusSeverityType.Error, true);
                    FinishTextOutput(outputPath);
                    return CorruptionResult.Failed;
                }
            }

            var result = ApplyByteEntries(byteEntries, outputPath, token, progress);
            if (result == CorruptionResult.Success)
                TraceLogger.Log($"Finished applying stash file: {stashFileLocation}. Total stash items applied: {Stash.ByteEntries.Count}");
            return result;
        }

        /// <summary>
        /// Writes a list of byte changes to a fresh copy of the loaded file (used by stash files and Bisect mode). Excluded ranges are skipped,
        /// checksums are fixed if enabled, and the applied entries become the current stash.
        /// </summary>
        public static CorruptionResult ApplyByteEntries(IReadOnlyList<(long Offset, byte Value)> entries, string outputPath, CancellationToken token, IProgress<double>? progress)
        {
            if (!IsLoaded || SourcePath == null)
            {
                TraceLogger.Log("No file is loaded.", StatusSeverityType.Error, true);
                return CorruptionResult.Failed;
            }
            var excluded = Exclusions.Snapshot();
            Stash.Clear();
            Stash.Enabled = true;
            AddStashHeaders(outputPath);
            Stash.SetExcludes(excluded);
            FileBuffer? buffer = null;
            try
            {
                buffer = PrepareOutput(outputPath);
                var checksums = ChecksumFixer.Detect(buffer);
                long skipped = 0;
                for (int n = 0; n < entries.Count; n++)
                {
                    var (offset, value) = entries[n];
                    if (offset < 0 || offset >= buffer.Length) continue;
                    if (ExcludeList.Contains(excluded, offset))
                    {
                        skipped++;
                        continue;
                    }
                    buffer[offset] = value;
                    Stash.Add(offset, value);
                    if ((n & 0xFFFF) == 0)
                    {
                        token.ThrowIfCancellationRequested();
                        progress?.Report((double)n / Math.Max(1, entries.Count));
                    }
                }
                if (skipped > 0)
                    TraceLogger.Log($"{skipped} stash change(s) were skipped because they're inside excluded ranges.");
                checksums?.Fix(buffer);
                CommitOutput(buffer, outputPath);
                buffer = null;
                return CorruptionResult.Success;
            }
            catch (OperationCanceledException)
            {
                AbortOutput(buffer, outputPath);
                return CorruptionResult.Cancelled;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Failed to apply the stash changes: {ex.Message}\n{ex}", StatusSeverityType.Error, true);
                AbortOutput(buffer, outputPath);
                return CorruptionResult.Failed;
            }
        }

        #endregion

        #region Output preparation, commit, abort and restore

        /// <summary>
        /// Set from the "Take File Ownership" checkbox. When true, the permission override is attempted if a file can't be read/written.
        /// </summary>
        internal static bool AttemptOverrideOnWrite = false;

        /// <summary>
        /// Checks the output can be written to (or created), attempting the permission override if enabled.
        /// </summary>
        private static bool EnsureWritable(string path, bool attemptOverride)
        {
            if (File.Exists(path))
            {
                if (FileOverrideHelper.CanOpen(path, FileAccess.ReadWrite, out string? reason))
                    return true;
                TraceLogger.Log($"Can't write to {path}: {reason}", StatusSeverityType.Warning);
                if (attemptOverride && FileOverrideHelper.AttemptProtectedFileOverride(path) && FileOverrideHelper.CanOpen(path, FileAccess.ReadWrite, out reason))
                    return true;
                TraceLogger.Log($"Unable to write to {path}. {reason}", StatusSeverityType.Error, true);
                return false;
            }

            string? dir = Path.GetDirectoryName(Path.GetFullPath(path));
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                TraceLogger.Log($"The save folder doesn't exist: {dir}", StatusSeverityType.Error, true);
                return false;
            }
            if (FileOverrideHelper.CanCreateIn(dir, out string? dirReason))
                return true;
            TraceLogger.Log($"Can't create files in {dir}: {dirReason}", StatusSeverityType.Warning);
            if (attemptOverride && FileOverrideHelper.AttemptProtectedFileOverride(dir) && FileOverrideHelper.CanCreateIn(dir, out dirReason))
                return true;
            TraceLogger.Log($"Unable to create the output file in {dir}. {dirReason}", StatusSeverityType.Error, true);
            return false;
        }

        /// <summary>
        /// Brings the output file to the original state and returns a writable buffer over it that tracks every page that gets modified.
        /// </summary>
        private static FileBuffer PrepareOutput(string outputPath)
        {
            if (!EnsureWritable(outputPath, AttemptOverrideOnWrite))
                throw new IOException($"The output file can't be written: {outputPath}");

            lock (_sessionLock)
            {
                if (IsInPlace(outputPath))
                {
                    TraceLogger.Log("The save location is the original file, corrupting in place. Original bytes are journaled so Restore still works while LRC stays open.", StatusSeverityType.Warning);
                    _journal ??= new InPlaceJournal(SourcePath!);
                    _journal.RevertAll();
                    var inPlace = FileBuffer.OpenReadWrite(SourcePath!);
                    var journal = _journal;
                    inPlace.PageFirstModified = journal.Record;
                    return inPlace;
                }

                string full = Path.GetFullPath(outputPath);
                bool canPatch = _output != null && _output.Valid && PathsEqual(_output.Path, full) && File.Exists(full);
                if (canPatch)
                {
                    var fi = new FileInfo(full);
                    canPatch = fi.Length == SourceLength && fi.LastWriteTimeUtc == _output!.LastWriteUtc;
                }

                if (canPatch)
                {
                    TraceLogger.Log($"Reverting {_output!.TouchedPages.Count} changed page(s) from the previous corruption instead of copying the whole file.");
                    RevertPagesFromSource(full, _output.TouchedPages);
                }
                else
                {
                    TraceLogger.Log($"Copying the original file to the save location: {full}");
                    CopyOriginalTo(full);
                    _output = new OutputState { Path = full, Valid = true };
                }

                var buffer = FileBuffer.OpenReadWrite(full);
                var state = _output!;
                state.Valid = false; //Not valid until committed
                buffer.PageFirstModified = (pageIndex, _, _) => state.TouchedPages.Add(pageIndex);
                return buffer;
            }
        }

        private static void CommitOutput(FileBuffer buffer, string outputPath)
        {
            buffer.Flush();
            buffer.Dispose();
            lock (_sessionLock)
            {
                if (!IsInPlace(outputPath) && _output != null)
                {
                    _output.LastWriteUtc = File.GetLastWriteTimeUtc(_output.Path);
                    _output.Valid = true;
                }
                LastOutputPath = Path.GetFullPath(outputPath);
            }
        }

        private static void AbortOutput(FileBuffer? buffer, string outputPath)
        {
            try
            {
                buffer?.Dispose(flush: false);
                lock (_sessionLock)
                {
                    if (IsInPlace(outputPath))
                    {
                        _journal?.RevertAll();
                    }
                    else if (_output != null && PathsEqual(_output.Path, outputPath) && File.Exists(_output.Path))
                    {
                        RevertPagesFromSource(_output.Path, _output.TouchedPages);
                        _output.LastWriteUtc = File.GetLastWriteTimeUtc(_output.Path);
                        _output.Valid = true;
                    }
                }
                TraceLogger.Log("The output file has been reverted to the original state.");
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to revert the output file after the corruption stopped: {ex.Message}", StatusSeverityType.Error);
                InvalidateOutput(outputPath);
            }
        }

        private static void PrepareTextOutput(string outputPath)
        {
            if (!EnsureWritable(outputPath, AttemptOverrideOnWrite))
                throw new IOException($"The output file can't be written: {outputPath}");
            lock (_sessionLock)
            {
                if (IsInPlace(outputPath))
                {
                    _journal ??= new InPlaceJournal(SourcePath!);
                    _journal.RevertAll();
                    _journal.EnsureFullBackup(); //The text engine changes the file length, so page journaling isn't enough here.
                }
                else if (_output != null && PathsEqual(_output.Path, outputPath))
                {
                    _output.Valid = false;
                }
            }
        }

        private static void FinishTextOutput(string outputPath)
        {
            lock (_sessionLock)
            {
                if (_output != null && PathsEqual(_output.Path, outputPath))
                    _output.Valid = false; //Content and length are now different from the original, a full copy is needed next time.
                LastOutputPath = Path.GetFullPath(outputPath);
            }
        }

        private static void CopyOriginalTo(string destination)
        {
            if (File.Exists(destination))
            {
                var attributes = File.GetAttributes(destination);
                if (attributes.HasFlag(FileAttributes.ReadOnly))
                    File.SetAttributes(destination, attributes & ~FileAttributes.ReadOnly);
            }
            File.Copy(SourcePath!, destination, true);
            var newAttributes = File.GetAttributes(destination);
            if (newAttributes.HasFlag(FileAttributes.ReadOnly)) //File.Copy keeps the read-only attribute of the original
                File.SetAttributes(destination, newAttributes & ~FileAttributes.ReadOnly);
        }

        private static void RevertPagesFromSource(string outputPath, HashSet<long> pages)
        {
            if (pages.Count == 0) return;
            byte[] page = new byte[FileBuffer.PageSize];
            using var src = new FileStream(SourcePath!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1);
            using var dst = new FileStream(outputPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 1);
            foreach (long pageIndex in pages.OrderBy(p => p))
            {
                long offset = pageIndex * FileBuffer.PageSize;
                int count = FileBuffer.ReadFully(src.SafeFileHandle, page, offset);
                if (count > 0)
                    RandomAccess.Write(dst.SafeFileHandle, page.AsSpan(0, count), offset);
            }
            pages.Clear();
        }

        /// <summary>
        /// Restores the save location back to the original file content.
        /// </summary>
        public static bool RestoreROM(string FileLocation)
        {
            TraceLogger.Log($"Restoring file: {FileLocation}");
            if (!IsLoaded)
            {
                TraceLogger.Log("No file loaded to restore.", StatusSeverityType.Warning, true);
                return false;
            }
            try
            {
                lock (_sessionLock)
                {
                    if (IsInPlace(FileLocation))
                    {
                        if (_journal == null || !_journal.HasChanges)
                        {
                            TraceLogger.Log("The original file hasn't been changed by LRC in this session, nothing to restore.", StatusSeverityType.Information, true);
                            return true;
                        }
                        _journal.RevertAll();
                    }
                    else
                    {
                        if (!EnsureWritable(FileLocation, AttemptOverrideOnWrite))
                            return false;
                        string full = Path.GetFullPath(FileLocation);
                        bool canPatch = _output != null && _output.Valid && PathsEqual(_output.Path, full) && File.Exists(full)
                                        && new FileInfo(full).Length == SourceLength && File.GetLastWriteTimeUtc(full) == _output.LastWriteUtc;
                        if (canPatch)
                        {
                            RevertPagesFromSource(full, _output!.TouchedPages);
                        }
                        else
                        {
                            CopyOriginalTo(full);
                            _output = new OutputState { Path = full };
                        }
                        _output!.LastWriteUtc = File.GetLastWriteTimeUtc(full);
                        _output.Valid = true;
                    }
                }
                TraceLogger.Log("ROM restored successfully.");
                return true;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error when restoring ROM: {ex}", StatusSeverityType.Error, true);
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Undo journal for in-place corruption. Stores the original content of every page before it's first modified.
        /// </summary>
        private sealed class InPlaceJournal : IDisposable
        {
            private readonly string _target;
            private readonly string _journalPath;
            private readonly FileStream _journal;
            private readonly Dictionary<long, (long JournalOffset, int Count)> _pages = [];
            private string? _fullBackupPath;
            public bool HasChanges { get; private set; }

            public InPlaceJournal(string target)
            {
                _target = target;
                string dir = Path.Combine(Path.GetTempPath(), "LunarROMCorruptor");
                Directory.CreateDirectory(dir);
                _journalPath = Path.Combine(dir, $"undo-{Environment.ProcessId}-{Guid.NewGuid():N}.journal");
                _journal = new FileStream(_journalPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);
                TraceLogger.Log($"Created undo journal for in-place corruption: {_journalPath}");
            }

            public void Record(long pageIndex, byte[] originalData, int count)
            {
                HasChanges = true;
                if (_pages.ContainsKey(pageIndex)) return; //Only the very first (original) version of a page is kept
                long offset = _journal.Length;
                RandomAccess.Write(_journal.SafeFileHandle, originalData.AsSpan(0, count), offset);
                _pages[pageIndex] = (offset, count);
            }

            public void EnsureFullBackup()
            {
                HasChanges = true;
                if (_fullBackupPath != null && File.Exists(_fullBackupPath)) return;
                _fullBackupPath = Path.ChangeExtension(_journalPath, ".backup");
                File.Copy(_target, _fullBackupPath, true);
                TraceLogger.Log($"Created full backup of the original for in-place text corruption: {_fullBackupPath}");
            }

            public void RevertAll()
            {
                if (!HasChanges) return;
                if (_fullBackupPath != null && File.Exists(_fullBackupPath))
                {
                    var attributes = File.GetAttributes(_target);
                    if (attributes.HasFlag(FileAttributes.ReadOnly))
                        File.SetAttributes(_target, attributes & ~FileAttributes.ReadOnly);
                    File.Copy(_fullBackupPath, _target, true); //Full original content, this also covers the journaled pages.
                }
                else if (_pages.Count > 0)
                {
                    byte[] page = new byte[FileBuffer.PageSize];
                    using var dst = new FileStream(_target, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 1);
                    foreach (var (pageIndex, (journalOffset, count)) in _pages)
                    {
                        RandomAccess.Read(_journal.SafeFileHandle, page.AsSpan(0, count), journalOffset);
                        RandomAccess.Write(dst.SafeFileHandle, page.AsSpan(0, count), pageIndex * FileBuffer.PageSize);
                    }
                }
                HasChanges = false; //The journal stays valid (the original page data doesn't change), it's reused for the next corruption.
                TraceLogger.Log($"Reverted the in-place changes on {_target}.");
            }

            public void Dispose()
            {
                try { _journal.Dispose(); } catch { }
                try { if (_fullBackupPath != null && File.Exists(_fullBackupPath)) File.Delete(_fullBackupPath); } catch { }
            }
        }
    }

    /// <summary>
    /// Paged random-access view of a file. Only the pages that are accessed are kept in memory, dirty pages are written back on
    /// eviction and on <see cref="Flush"/>. The file length never changes.
    /// If the file fits in the memory budget every page is kept once loaded (fast random access, like the old in-memory model),
    /// otherwise a least-recently-used cache of the budget size is used, so files of any size can be corrupted.
    /// </summary>
    public sealed class FileBuffer : IDisposable
    {
        public const int PageSize = 4096;
        private const int PageShift = 12;
        private const int ReadAheadPages = 64; //256 KB reads when the whole file fits in the cache or access is sequential
        private readonly FileStream _stream;
        private readonly long _maxPages;
        private readonly Page?[]? _direct; //Used when the whole file fits in the budget
        private readonly Dictionary<long, Page>? _pages; //Used for files larger than the budget
        private readonly LinkedList<Page>? _lru;
        private readonly HashSet<long> _notifiedPages = [];
        private long _cachedCount;
        private long _lastMissIndex = -2;
        private Page? _lastPage;
        private bool _disposed;

        public long Length { get; }
        public bool Writable { get; }
        public string FilePath { get; }

        /// <summary>
        /// Called once per page, right before the page is modified for the first time. Arguments: page index, the page's current (unmodified) data, valid byte count.
        /// </summary>
        public Action<long, byte[], int>? PageFirstModified;

        /// <summary>
        /// Index of every page that has been written to through this buffer (page = offset / <see cref="PageSize"/>).
        /// </summary>
        public IReadOnlyCollection<long> ModifiedPages => _notifiedPages;

        private sealed class Page
        {
            public long Index;
            public readonly byte[] Data = new byte[PageSize];
            public int Count;
            public bool Dirty;
            public LinkedListNode<Page>? Node;
        }

        public static long DefaultBudgetBytes
        {
            get
            {
                long available = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
                if (available <= 0) available = 4L * 1024 * 1024 * 1024;
                return Math.Clamp(available / 4, 64L * 1024 * 1024, 2L * 1024 * 1024 * 1024);
            }
        }

        private FileBuffer(string path, bool writable, long budgetBytes)
        {
            FilePath = path;
            Writable = writable;
            _stream = new FileStream(path, FileMode.Open, writable ? FileAccess.ReadWrite : FileAccess.Read, FileShare.ReadWrite, 1, FileOptions.RandomAccess);
            Length = _stream.Length;
            _maxPages = Math.Max(16, budgetBytes / PageSize);
            long pageCount = (Length + PageSize - 1) / PageSize;
            if (pageCount <= _maxPages)
            {
                _direct = new Page?[pageCount];
            }
            else
            {
                _pages = [];
                _lru = new LinkedList<Page>();
            }
        }

        public static FileBuffer OpenRead(string path, int cacheMegabytes = 64) => new(path, false, cacheMegabytes * 1024L * 1024);
        public static FileBuffer OpenReadWrite(string path) => new(path, true, DefaultBudgetBytes);
        public static FileBuffer OpenReadWrite(string path, int cacheMegabytes) => new(path, true, cacheMegabytes * 1024L * 1024);

        public byte this[long position]
        {
            get
            {
                if ((ulong)position >= (ulong)Length)
                    throw new ArgumentOutOfRangeException(nameof(position), $"Position {position} is outside of the file (length {Length}).");
                return GetPage(position >> PageShift).Data[(int)(position & (PageSize - 1))];
            }
            set
            {
                if (!Writable)
                    throw new InvalidOperationException("This file buffer is read only.");
                if ((ulong)position >= (ulong)Length)
                    throw new ArgumentOutOfRangeException(nameof(position), $"Position {position} is outside of the file (length {Length}).");
                var page = GetPage(position >> PageShift);
                if (!page.Dirty)
                {
                    if (_notifiedPages.Add(page.Index))
                        PageFirstModified?.Invoke(page.Index, page.Data, page.Count);
                    page.Dirty = true;
                }
                page.Data[(int)(position & (PageSize - 1))] = value;
            }
        }

        /// <summary>
        /// Reads a block of bytes starting at offset. Returns the amount of bytes read (less than the destination size at the end of the file).
        /// </summary>
        public int Read(long offset, Span<byte> destination)
        {
            int total = 0;
            while (total < destination.Length && offset + total < Length)
            {
                long pos = offset + total;
                var page = GetPage(pos >> PageShift);
                int inPage = (int)(pos & (PageSize - 1));
                int count = Math.Min(page.Count - inPage, destination.Length - total);
                if (count <= 0) break;
                page.Data.AsSpan(inPage, count).CopyTo(destination[total..]);
                total += count;
            }
            return total;
        }

        public uint ReadUInt32(long offset, bool bigEndian)
        {
            Span<byte> b = stackalloc byte[4];
            if (offset < 0 || Read(offset, b) < 4) return 0;
            return bigEndian ? System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(b) : System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(b);
        }

        public ushort ReadUInt16(long offset, bool bigEndian)
        {
            Span<byte> b = stackalloc byte[2];
            if (offset < 0 || Read(offset, b) < 2) return 0;
            return bigEndian ? System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(b) : System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(b);
        }

        private Page GetPage(long index)
        {
            var last = _lastPage;
            if (last != null && last.Index == index)
                return last;

            Page? page;
            if (_direct != null)
            {
                page = _direct[index] ?? LoadPages(index);
            }
            else if (_pages!.TryGetValue(index, out page))
            {
                _lru!.Remove(page.Node!);
                _lru.AddFirst(page.Node!);
            }
            else
            {
                page = LoadPages(index);
            }
            _lastPage = page;
            return page;
        }

        private bool IsCached(long index) => _direct != null ? _direct[index] != null : _pages!.ContainsKey(index);

        /// <summary>
        /// Loads the requested page. When the whole file fits in the cache, or the access looks sequential, the following pages are read in the same call.
        /// </summary>
        private Page LoadPages(long index)
        {
            long pageCount = (Length + PageSize - 1) / PageSize;
            bool readAhead = _direct != null || index == _lastMissIndex + 1;
            int count = 1;
            if (readAhead)
            {
                long limit = _direct != null ? ReadAheadPages : Math.Min(ReadAheadPages, _maxPages / 2);
                while (count < limit && index + count < pageCount && !IsCached(index + count))
                    count++;
            }
            _lastMissIndex = index + count - 1;

            byte[] block = count == 1 ? [] : new byte[count * PageSize];
            int read;
            if (count == 1)
            {
                var single = new Page { Index = index };
                single.Count = ReadFully(_stream.SafeFileHandle, single.Data, index * PageSize);
                AddToCache(single);
                return single;
            }
            read = ReadFully(_stream.SafeFileHandle, block, index * PageSize);
            Page? first = null;
            for (int k = 0; k < count; k++)
            {
                var page = new Page { Index = index + k };
                int available = Math.Clamp(read - k * PageSize, 0, PageSize);
                Buffer.BlockCopy(block, k * PageSize, page.Data, 0, available);
                page.Count = available;
                AddToCache(page);
                first ??= page;
            }
            return first!;
        }

        private void AddToCache(Page page)
        {
            if (_direct != null)
            {
                _direct[page.Index] = page;
                _cachedCount++;
                return;
            }
            if (_pages!.Count >= _maxPages)
            {
                var victim = _lru!.Last!.Value;
                WritePage(victim);
                _lru.RemoveLast();
                _pages.Remove(victim.Index);
                if (ReferenceEquals(_lastPage, victim)) _lastPage = null;
            }
            page.Node = _lru!.AddFirst(page);
            _pages[page.Index] = page;
            _cachedCount = _pages.Count;
        }

        internal static int ReadFully(Microsoft.Win32.SafeHandles.SafeFileHandle handle, byte[] buffer, long offset)
        {
            int total = 0;
            while (total < buffer.Length)
            {
                int read = RandomAccess.Read(handle, buffer.AsSpan(total), offset + total);
                if (read <= 0) break;
                total += read;
            }
            return total;
        }

        private void WritePage(Page page)
        {
            if (!page.Dirty) return;
            RandomAccess.Write(_stream.SafeFileHandle, page.Data.AsSpan(0, page.Count), page.Index * PageSize);
            page.Dirty = false;
        }

        private IEnumerable<Page> DirtyPagesInOrder()
        {
            if (_direct != null)
            {
                foreach (var page in _direct)
                    if (page != null && page.Dirty) yield return page;
            }
            else
            {
                foreach (var page in _pages!.Values.Where(p => p.Dirty).OrderBy(p => p.Index))
                    yield return page;
            }
        }

        public void Flush()
        {
            if (!Writable || _disposed) return;
            //Contiguous dirty pages are written together (up to 1 MB per write) to keep the number of writes low.
            const int maxRun = 256;
            byte[] run = new byte[maxRun * PageSize];
            long runStart = -1;
            int runPages = 0;
            int runBytes = 0;
            void FlushRun()
            {
                if (runPages == 0) return;
                RandomAccess.Write(_stream.SafeFileHandle, run.AsSpan(0, runBytes), runStart * PageSize);
                runPages = 0;
                runBytes = 0;
            }
            foreach (var page in DirtyPagesInOrder())
            {
                if (runPages > 0 && (page.Index != runStart + runPages || runPages == maxRun || runBytes != runPages * PageSize))
                    FlushRun();
                if (runPages == 0) runStart = page.Index;
                Buffer.BlockCopy(page.Data, 0, run, runPages * PageSize, page.Count);
                runBytes += page.Count;
                runPages++;
                page.Dirty = false;
            }
            FlushRun();
            _stream.Flush(true);
        }

        public void Dispose() => Dispose(true);

        public void Dispose(bool flush)
        {
            if (_disposed) return;
            try
            {
                if (flush) Flush();
            }
            finally
            {
                _disposed = true;
                _pages?.Clear();
                _lru?.Clear();
                if (_direct != null) Array.Clear(_direct);
                _lastPage = null;
                _stream.Dispose();
            }
        }
    }

    /// <summary>
    /// An inclusive range of byte offsets.
    /// </summary>
    public readonly record struct ByteRange(long Start, long End)
    {
        public long Length => End - Start + 1;
        public override string ToString() => $"0x{Start:X} - 0x{End:X}";
    }

    /// <summary>
    /// Sorted, merged list of byte ranges that the engines are not allowed to change. Engines get an immutable snapshot (see <see cref="Snapshot"/>).
    /// </summary>
    public sealed class ExcludeList
    {
        private readonly List<ByteRange> _ranges = [];
        private readonly Lock _lock = new();

        /// <summary>
        /// Raised (on the thread that made the change) whenever ranges are added or removed.
        /// </summary>
        public event Action? Changed;

        public int Count { get { lock (_lock) return _ranges.Count; } }

        public long TotalBytes { get { lock (_lock) return _ranges.Sum(r => r.Length); } }

        public void Add(long start, long end) => AddRange([new ByteRange(Math.Min(start, end), Math.Max(start, end))]);

        public void AddRange(IEnumerable<ByteRange> ranges)
        {
            lock (_lock)
            {
                _ranges.AddRange(ranges.Where(r => r.Start >= 0 && r.End >= r.Start));
                var merged = Merge(_ranges);
                _ranges.Clear();
                _ranges.AddRange(merged);
            }
            Changed?.Invoke();
        }

        public void Clear()
        {
            lock (_lock) _ranges.Clear();
            Changed?.Invoke();
        }

        public ByteRange[] Snapshot()
        {
            lock (_lock) return [.. _ranges];
        }

        /// <summary>
        /// Sorts and merges overlapping/touching ranges.
        /// </summary>
        public static List<ByteRange> Merge(IEnumerable<ByteRange> ranges)
        {
            var result = new List<ByteRange>();
            foreach (var r in ranges.OrderBy(r => r.Start))
            {
                if (result.Count > 0 && r.Start <= result[^1].End + 1)
                    result[^1] = result[^1] with { End = Math.Max(result[^1].End, r.End) };
                else
                    result.Add(r);
            }
            return result;
        }

        /// <summary>
        /// True if any byte of [start, end] is inside one of the sorted ranges.
        /// </summary>
        public static bool Overlaps(ByteRange[]? sorted, long start, long end)
        {
            if (sorted == null || sorted.Length == 0) return false;
            int lo = 0, hi = sorted.Length - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) >>> 1;
                if (sorted[mid].End < start) lo = mid + 1;
                else if (sorted[mid].Start > end) hi = mid - 1;
                else return true;
            }
            return false;
        }

        public static bool Contains(ByteRange[]? sorted, long position) => Overlaps(sorted, position, position);
    }

    /// <summary>
    /// Compact record of every byte that has been changed by a corruption. Formatting to the "[x] File(i).SET(v)" text is only done when needed.
    /// </summary>
    public sealed class StashLog
    {
        public const int MaxByteEntries = 10_000_000; //~160 MB of memory, more than that and the stash stops recording.
        private readonly List<(long Offset, byte Value)> _bytes = [];
        private readonly List<string> _lines = [];
        private readonly List<ByteRange> _excludes = [];
        public bool Enabled = true;
        public bool Overflowed { get; private set; }
        public long Count => _bytes.Count + _lines.Count + _excludes.Count;

        /// <summary>
        /// Stash headers: the file that was loaded and where the corrupted file was saved. Written as the first two lines of the stash
        /// ([x] File.OpenLocation("path") / [x] File.SaveAsLocation("path")) and used by "Corrupt File and Stash". They aren't counted as entries.
        /// </summary>
        public string? OpenLocation { get; private set; }
        public string? SaveAsLocation { get; private set; }

        public void SetHeaders(string? openLocation, string? saveAsLocation)
        {
            OpenLocation = string.IsNullOrWhiteSpace(openLocation) ? null : openLocation;
            SaveAsLocation = string.IsNullOrWhiteSpace(saveAsLocation) ? null : saveAsLocation;
        }

        /// <summary>
        /// The recorded byte changes in the order they were made (Bisect mode works on these).
        /// </summary>
        public IReadOnlyList<(long Offset, byte Value)> ByteEntries => _bytes;

        /// <summary>
        /// True when the stash has Source Code (VARLINE) or Datamosh (FRAME) entries.
        /// </summary>
        public bool HasTextEntries => _lines.Count > 0;

        /// <summary>
        /// The excluded ranges that were active for this corruption. They are written as [x] File.EXCLUDE(start,end) lines at the top of the stash.
        /// </summary>
        public IReadOnlyList<ByteRange> Excludes => _excludes;

        public void SetExcludes(IEnumerable<ByteRange>? ranges)
        {
            _excludes.Clear();
            if (ranges != null) _excludes.AddRange(ranges);
        }

        /// <summary>
        /// Offsets of every recorded byte change (used by the ByteView Diff mode for in-place corruption).
        /// </summary>
        public IEnumerable<long> ByteOffsets
        {
            get
            {
                foreach (var (offset, _) in _bytes) yield return offset;
            }
        }

        public void Add(long offset, byte value)
        {
            if (!Enabled) return;
            if (_bytes.Count >= MaxByteEntries)
            {
                if (!Overflowed)
                {
                    Overflowed = true;
                    TraceLogger.Log($"The stash reached {MaxByteEntries:N0} entries and has stopped recording. The stash for this corruption will be incomplete.", StatusSeverityType.Warning);
                }
                return;
            }
            _bytes.Add((offset, value));
        }

        public void AddLine(string line)
        {
            if (!Enabled) return;
            _lines.Add(line);
        }

        public void Clear()
        {
            _bytes.Clear();
            _lines.Clear();
            _excludes.Clear();
            OpenLocation = null;
            SaveAsLocation = null;
            if (_bytes.Capacity > 1_000_000) _bytes.TrimExcess();
            Overflowed = false;
        }

        public IEnumerable<string> FormatLines(int max = int.MaxValue)
        {
            int n = 0;
            if (OpenLocation != null)
            {
                if (n++ >= max) yield break;
                yield return StashParser.FormatOpenLocation(OpenLocation);
            }
            if (SaveAsLocation != null)
            {
                if (n++ >= max) yield break;
                yield return StashParser.FormatSaveAsLocation(SaveAsLocation);
            }
            foreach (var range in _excludes)
            {
                if (n++ >= max) yield break;
                yield return StashParser.FormatExclude(range.Start, range.End);
            }
            foreach (var line in _lines)
            {
                if (n++ >= max) yield break;
                yield return line;
            }
            foreach (var (offset, value) in _bytes)
            {
                if (n++ >= max) yield break;
                yield return StashParser.FormatByte(offset, value);
            }
        }

        public void WriteTo(string path)
        {
            using var writer = new StreamWriter(path, false, new UTF8Encoding(false), 1 << 16);
            foreach (var line in FormatLines())
                writer.WriteLine(line);
        }
    }

    /// <summary>
    /// Reads and writes the stash text formats:
    /// [x] File(offset).SET(value)
    /// [x] File(offset).VARLINE("name",line).SET("new text")   (Source Code Engine)
    /// </summary>
    public static partial class StashParser
    {
        public readonly record struct VarLineEntry(long Offset, string Name, int Line, string Value);
        public readonly record struct FrameEntry(long Offset, int FrameIndex, DatamoshEngine.FrameAction Action);

        [GeneratedRegex(@"^\[x\] File\((\d+)\)\.FRAME\((\d+)\)\.SET\((DROP|REPLACE|REPEAT,(\d+))\)$")]
        private static partial Regex FrameLineRegex();

        public static bool TryParseFrame(string line, out FrameEntry entry)
        {
            entry = default;
            var m = FrameLineRegex().Match(line);
            if (!m.Success || !long.TryParse(m.Groups[1].Value, out long offset) || !int.TryParse(m.Groups[2].Value, out int frame))
                return false;
            var action = m.Groups[3].Value switch
            {
                "DROP" => new DatamoshEngine.FrameAction(DatamoshEngine.ActionKind.Drop),
                "REPLACE" => new DatamoshEngine.FrameAction(DatamoshEngine.ActionKind.Replace),
                _ => new DatamoshEngine.FrameAction(DatamoshEngine.ActionKind.Repeat, int.TryParse(m.Groups[4].Value, out int n) ? Math.Clamp(n, 1, 1000) : 1)
            };
            entry = new FrameEntry(offset, frame, action);
            return true;
        }

        [GeneratedRegex(@"^\[x\] File\((\d+)\)\.SET\((\d+)\)$")]
        private static partial Regex ByteLineRegex();

        [GeneratedRegex(@"^\[x\] File\.EXCLUDE\((\d+),(\d+)\)$", RegexOptions.IgnoreCase)]
        private static partial Regex ExcludeLineRegex();

        /// <summary>
        /// [x] File.EXCLUDE(start,end): no engine may change the bytes from start to end (inclusive). EXCLUDE takes priority over SET lines.
        /// </summary>
        public static string FormatExclude(long start, long end) => $"[x] File.EXCLUDE({start},{end})";

        public static bool TryParseExclude(string line, out ByteRange range)
        {
            range = default;
            var m = ExcludeLineRegex().Match(line);
            if (!m.Success || !long.TryParse(m.Groups[1].Value, out long a) || !long.TryParse(m.Groups[2].Value, out long b))
                return false;
            range = new ByteRange(Math.Min(a, b), Math.Max(a, b));
            return true;
        }

        [GeneratedRegex(@"^\[x\] File\((\d+)\)\.VARLINE\(""((?:[^""\\]|\\.)*)"",(\d+)\)\.SET\(""((?:[^""\\]|\\.)*)""\)$")]
        private static partial Regex VarLineRegex();

        public static string FormatByte(long offset, byte value) => $"[x] File({offset}).SET({value})";

        public static string FormatVarLine(long offset, string name, int line, string value) => $"[x] File({offset}).VARLINE(\"{Escape(name)}\",{line}).SET(\"{Escape(value)}\")";

        public static bool TryParseByte(string line, out long offset, out byte value)
        {
            offset = 0;
            value = 0;
            var m = ByteLineRegex().Match(line);
            return m.Success && long.TryParse(m.Groups[1].Value, out offset) && byte.TryParse(m.Groups[2].Value, out value);
        }

        public static bool TryParseVarLine(string line, out VarLineEntry entry)
        {
            entry = default;
            var m = VarLineRegex().Match(line);
            if (!m.Success || !long.TryParse(m.Groups[1].Value, out long offset) || !int.TryParse(m.Groups[3].Value, out int lineNo))
                return false;
            entry = new VarLineEntry(offset, Unescape(m.Groups[2].Value), lineNo, Unescape(m.Groups[4].Value));
            return true;
        }

        public static bool IsValidStashLine(string line) => TryParseByte(line, out _, out _) || TryParseVarLine(line, out _) || TryParseFrame(line, out _) || TryParseExclude(line, out _) || TryParseHeader(line, out _, out _);

        public enum HeaderKind { OpenLocation, SaveAsLocation }

        //Windows paths can't contain quotes, so the path is stored as it is (no escaping, backslashes stay readable)
        [GeneratedRegex(@"^\[x\] File\.(OpenLocation|SaveAsLocation)\(""([^""]*)""\)$", RegexOptions.IgnoreCase)]
        private static partial Regex HeaderLineRegex();

        public static string FormatOpenLocation(string path) => $"[x] File.OpenLocation(\"{path}\")";
        public static string FormatSaveAsLocation(string path) => $"[x] File.SaveAsLocation(\"{path}\")";

        /// <summary>
        /// [x] File.OpenLocation("path") / [x] File.SaveAsLocation("path"): the file the stash was made for and where it was saved.
        /// </summary>
        public static bool TryParseHeader(string line, out HeaderKind kind, out string path)
        {
            kind = default;
            path = "";
            var m = HeaderLineRegex().Match(line);
            if (!m.Success) return false;
            kind = m.Groups[1].Value.Equals("OpenLocation", StringComparison.OrdinalIgnoreCase) ? HeaderKind.OpenLocation : HeaderKind.SaveAsLocation;
            path = m.Groups[2].Value.Trim();
            return true;
        }

        public static bool IsHeaderOrExclude(string line) => TryParseHeader(line, out _, out _) || TryParseExclude(line, out _);

        /// <summary>
        /// Reads the OpenLocation/SaveAsLocation headers of a stash file. Only the top of the file is read (headers are written first),
        /// so this is quick even for huge stashes. Returns false if the stash has no OpenLocation header (stashes made before headers existed).
        /// </summary>
        public static bool TryReadHeaders(string stashFile, out string? openLocation, out string? saveAsLocation)
        {
            openLocation = null;
            saveAsLocation = null;
            int scanned = 0;
            foreach (var raw in File.ReadLines(stashFile))
            {
                string line = raw.Trim();
                if (line.Length == 0) continue;
                if (TryParseHeader(line, out var kind, out string path))
                {
                    if (kind == HeaderKind.OpenLocation) openLocation ??= path;
                    else saveAsLocation ??= path;
                    if (openLocation != null && saveAsLocation != null) break;
                }
                else if (!TryParseExclude(line, out _) && ++scanned > 16)
                {
                    break; //Past the header block
                }
            }
            if (openLocation == "") openLocation = null;
            if (saveAsLocation == "") saveAsLocation = null;
            return openLocation != null;
        }

        public static string Escape(string s)
        {
            var sb = new StringBuilder(s.Length + 8);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append(@"\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append(@"\n"); break;
                    case '\r': sb.Append(@"\r"); break;
                    case '\t': sb.Append(@"\t"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }

        public static string Unescape(string s)
        {
            var sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\\' && i + 1 < s.Length)
                {
                    char n = s[++i];
                    sb.Append(n switch { 'n' => '\n', 'r' => '\r', 't' => '\t', _ => n });
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Stash tools used by the Stash Editor: mutate (make variations of a corruption), sort and remove duplicates.
    /// Only byte entries ([x] File(i).SET(v)) are mutated, Source Code (VARLINE) and Datamosh (FRAME) entries are kept as they are.
    /// </summary>
    public static class StashTools
    {
        public enum MutateMode
        {
            RandomValue, //Give the byte a new random value
            NudgeValue, //Add/subtract up to <amount> from the value
            MoveAddress //Move the entry up to <amount> bytes forwards/backwards
        }

        public static List<string> Mutate(IReadOnlyList<string> lines, double percent, MutateMode mode, long amount, long maxOffset, Random rnd, out int changed)
        {
            var result = new List<string>(lines);
            var byteLines = new List<(int Index, long Offset, byte Value)>();
            for (int i = 0; i < result.Count; i++)
            {
                if (StashParser.TryParseByte(result[i], out long offset, out byte value))
                    byteLines.Add((i, offset, value));
            }
            changed = 0;
            if (byteLines.Count == 0) return result;

            int count = (int)Math.Clamp(Math.Round(byteLines.Count * Math.Clamp(percent, 0, 100) / 100.0), 1, byteLines.Count);
            amount = Math.Max(1, amount);
            maxOffset = Math.Max(0, maxOffset);
            //Partial Fisher-Yates so each entry is mutated at most once
            var pool = byteLines.ToArray();
            for (int n = 0; n < count; n++)
            {
                int j = rnd.Next(n, pool.Length);
                (pool[n], pool[j]) = (pool[j], pool[n]);
                var (index, offset, value) = pool[n];
                switch (mode)
                {
                    case MutateMode.RandomValue:
                        byte newValue;
                        do { newValue = (byte)rnd.Next(256); } while (newValue == value);
                        value = newValue;
                        break;
                    case MutateMode.NudgeValue:
                        long delta = rnd.NextInt64(1, Math.Min(amount, 255) + 1) * (rnd.Next(2) == 0 ? 1 : -1);
                        long nudged = value + delta;
                        if (nudged < 0 || nudged > 255) nudged = value - delta; //Go the other way instead of clamping
                        value = (byte)Math.Clamp(nudged, 0, 255);
                        break;
                    case MutateMode.MoveAddress:
                        long move = rnd.NextInt64(1, amount + 1) * (rnd.Next(2) == 0 ? 1 : -1);
                        long moved = offset + move;
                        if (moved < 0 || moved > maxOffset) moved = offset - move;
                        offset = Math.Clamp(moved, 0, maxOffset);
                        break;
                }
                string line = StashParser.FormatByte(offset, value);
                if (line != result[index]) changed++;
                result[index] = line;
            }
            return result;
        }

        /// <summary>
        /// Sorts byte entries by address (entries for the same address keep their order, so the result of applying the stash doesn't change).
        /// Other entries (VARLINE/FRAME) are kept first, in their original order.
        /// </summary>
        public static List<string> SortByAddress(IReadOnlyList<string> lines)
        {
            var others = new List<string>();
            var bytes = new List<(long Offset, int Order, string Line)>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (StashParser.TryParseByte(lines[i], out long offset, out _)) bytes.Add((offset, i, lines[i]));
                else others.Add(lines[i]);
            }
            return [.. others, .. bytes.OrderBy(b => b.Offset).ThenBy(b => b.Order).Select(b => b.Line)];
        }

        /// <summary>
        /// Removes entries that are overwritten by a later entry for the same address (the result of applying the stash doesn't change).
        /// </summary>
        public static List<string> RemoveDuplicates(IReadOnlyList<string> lines, out int removed)
        {
            var lastIndex = new Dictionary<string, int>();
            for (int i = 0; i < lines.Count; i++)
                lastIndex[KeyOf(lines[i])] = i;
            var result = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lastIndex[KeyOf(lines[i])] == i)
                    result.Add(lines[i]);
            }
            removed = lines.Count - result.Count;
            return result;
        }

        private static string KeyOf(string line)
        {
            if (StashParser.TryParseByte(line, out long offset, out _)) return "B" + offset;
            if (StashParser.TryParseVarLine(line, out var v)) return "V" + v.Offset;
            if (StashParser.TryParseFrame(line, out var f)) return "F" + f.Offset;
            return "L" + line;
        }
    }

    /// <summary>
    /// N64 ROM helpers. The IPL3 boot code checks a checksum (CRC1/CRC2 at 0x10 in the header) over 0x1000 - 0x100FFF, so corrupting
    /// anything in the first 1 MB of game data hangs on a black screen on real hardware and on accurate emulators (ares, simple64,
    /// Gopher64, CEN64, Mupen64Plus with LLE boot...). The checksum is rewritten after corrupting so the ROM still boots.
    /// Works with .z64 (big-endian), .v64 (byte-swapped) and .n64 (little-endian) dumps.
    /// </summary>
    public static class N64Rom
    {
        public enum ByteOrder
        {
            BigEndian, //.z64
            ByteSwapped, //.v64 (every 16-bit word swapped)
            LittleEndian //.n64 (every 32-bit word reversed)
        }

        public sealed record Info(ByteOrder Order, int Cic);

        public const long BootCodeEnd = 0x1000; //Header (0x00-0x3F) + IPL3 boot code (0x40-0xFFF)
        private const int ChecksumStart = 0x1000;
        private const int ChecksumLength = 0x100000;

        public static ByteOrder? DetectByteOrder(FileBuffer d)
        {
            if (d.Length < 0x40) return null;
            return d.ReadUInt32(0, true) switch
            {
                0x80371240 => ByteOrder.BigEndian,
                0x37804012 => ByteOrder.ByteSwapped,
                0x40123780 => ByteOrder.LittleEndian,
                _ => null
            };
        }

        /// <summary>
        /// Converts bytes between the file's byte order and big-endian (the conversion is its own inverse). The span must start on a 4 byte boundary of the file.
        /// </summary>
        public static void SwapToBigEndian(Span<byte> b, ByteOrder order)
        {
            if (order == ByteOrder.ByteSwapped)
            {
                for (int k = 0; k + 1 < b.Length; k += 2)
                    (b[k], b[k + 1]) = (b[k + 1], b[k]);
            }
            else if (order == ByteOrder.LittleEndian)
            {
                for (int k = 0; k + 3 < b.Length; k += 4)
                {
                    (b[k], b[k + 3]) = (b[k + 3], b[k]);
                    (b[k + 1], b[k + 2]) = (b[k + 2], b[k + 1]);
                }
            }
        }

        /// <summary>
        /// Returns the ROM info when the file is an N64 ROM with a known boot chip (CIC) and a correct checksum. Call this on the original
        /// (uncorrupted) data: the checksum is only rewritten later if LRC can reproduce the original one, so an unknown/homebrew ROM is never touched.
        /// </summary>
        public static Info? Detect(FileBuffer d)
        {
            try
            {
                var order = DetectByteOrder(d);
                if (order == null) return null;
                if (d.Length < ChecksumStart + ChecksumLength)
                {
                    TraceLogger.Log("N64 ROM detected, but it's smaller than 1 MB + 4 KB so it has no boot checksum to fix.");
                    return null;
                }
                byte[] rom = ReadBigEndian(d, order.Value);
                int cic = DetectCic(rom);
                if (cic == 0)
                {
                    TraceLogger.Log("N64 ROM detected, but the boot chip (CIC) is unknown. The boot checksum won't be updated, so corrupting 0x1000 - 0x100FFF may stop it booting on accurate emulators.", StatusSeverityType.Warning);
                    return null;
                }
                var (crc1, crc2) = Calculate(rom, cic);
                uint h1 = ReadBE(rom, 0x10), h2 = ReadBE(rom, 0x14);
                if (crc1 != h1 || crc2 != h2)
                {
                    TraceLogger.Log($"N64 ROM detected (CIC-{cic}), but its header checksum doesn't match the data (bad dump/hack?). The checksum won't be updated.", StatusSeverityType.Warning);
                    return null;
                }
                TraceLogger.Log($"N64 ROM detected ({order.Value}, CIC-NUS-{cic}).");
                return new Info(order.Value, cic);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to check the N64 header: {ex.Message}", StatusSeverityType.Warning);
                return null;
            }
        }

        /// <summary>
        /// Recalculates CRC1/CRC2 and writes them to the header (only the bytes that changed are written).
        /// </summary>
        public static void FixChecksum(FileBuffer d, Info info)
        {
            byte[] rom = ReadBigEndian(d, info.Order);
            var (crc1, crc2) = Calculate(rom, info.Cic);
            if (crc1 == ReadBE(rom, 0x10) && crc2 == ReadBE(rom, 0x14))
                return; //Nothing inside the checksummed area was changed
            Span<byte> crc = stackalloc byte[8];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(crc, crc1);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(crc[4..], crc2);
            SwapToBigEndian(crc, info.Order);
            for (int k = 0; k < 8; k++)
            {
                if (d[0x10 + k] != crc[k])
                    d[0x10 + k] = crc[k];
            }
            TraceLogger.Log($"N64 boot checksum updated: CRC1 = 0x{crc1:X8}, CRC2 = 0x{crc2:X8}.");
        }

        private static byte[] ReadBigEndian(FileBuffer d, ByteOrder order)
        {
            byte[] rom = new byte[ChecksumStart + ChecksumLength];
            d.Read(0, rom);
            SwapToBigEndian(rom, order);
            return rom;
        }

        private static uint ReadBE(byte[] b, int offset) => System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(b.AsSpan(offset, 4));

        /// <summary>
        /// Identifies the boot chip from the sum of the IPL3 boot code words (0x40 - 0xFFF), the same way Mupen64Plus does.
        /// 7101/7102 (PAL) use the 6101/6102 algorithm. Other chips (5101, 5167, 8303...) aren't supported and return 0.
        /// </summary>
        private static int DetectCic(byte[] rom)
        {
            ulong sum = 0;
            for (int i = 0x40; i < 0x1000; i += 4)
                sum += ReadBE(rom, i);
            return sum switch
            {
                0x000000D0027FDF31 or 0x000000CFFB631223 => 6101, //CIC-NUS-6101 / 7102
                0x000000D057C85244 => 6102, //CIC-NUS-6102 / 7101
                0x000000D6497E414B => 6103, //CIC-NUS-6103 / 7103
                0x0000011A49F60E96 => 6105, //CIC-NUS-6105 / 7105
                0x000000D6D5BE5580 => 6106, //CIC-NUS-6106 / 7106
                _ => 0
            };
        }

        /// <summary>
        /// The IPL3 checksum algorithm (same as the n64crc tool).
        /// </summary>
        private static (uint Crc1, uint Crc2) Calculate(byte[] rom, int cic)
        {
            uint seed = cic switch
            {
                6103 => 0xA3886759,
                6105 => 0xDF26F436,
                6106 => 0x1FEA617A,
                _ => 0xF8CA4DDC //6101 / 6102
            };
            uint t1 = seed, t2 = seed, t3 = seed, t4 = seed, t5 = seed, t6 = seed;
            for (int i = ChecksumStart; i < ChecksumStart + ChecksumLength; i += 4)
            {
                uint d = ReadBE(rom, i);
                if (unchecked(t6 + d) < t6) t4++;
                t6 = unchecked(t6 + d);
                t3 ^= d;
                uint r = System.Numerics.BitOperations.RotateLeft(d, (int)(d & 0x1F));
                t5 = unchecked(t5 + r);
                if (t2 > d) t2 ^= r;
                else t2 ^= t6 ^ d;
                if (cic == 6105) t1 = unchecked(t1 + (ReadBE(rom, 0x40 + 0x0710 + (i & 0xFF)) ^ d));
                else t1 = unchecked(t1 + (t5 ^ d));
            }
            return cic switch
            {
                6103 => (unchecked((t6 ^ t4) + t3), unchecked((t5 ^ t2) + t1)),
                6106 => (unchecked(t6 * t4 + t3), unchecked(t5 * t2 + t1)),
                _ => (t6 ^ t4 ^ t3, t5 ^ t2 ^ t1)
            };
        }
    }

    /// <summary>
    /// Optional checksum fixing ("Fix Checksums" checkbox). <see cref="Detect"/> runs on the original bytes before the engine; a fixer is only
    /// used when LRC can reproduce the checksum that's already in the file, so unknown formats, hacks and bad dumps are never touched.
    /// <see cref="Fix"/> recalculates every detected checksum after the engine has run.
    /// Supported: N64 (CRC1/CRC2), Game Boy (header + global), GBA (header complement), SNES (internal checksum), Genesis / Mega Drive,
    /// raw CD images (Mode 1 / Mode 2 sector EDC/ECC for PS1, Saturn, Sega CD... BIN files).
    /// </summary>
    public sealed class ChecksumFixer
    {
        private readonly List<(string Name, Action<FileBuffer> Fix)> _fixes = [];

        public IEnumerable<string> Names => _fixes.Select(f => f.Name);

        public static ChecksumFixer? Detect(FileBuffer d)
        {
            if (!CorruptionCore.FixChecksums) return null;
            var fixer = new ChecksumFixer();
            void Try(string name, Func<FileBuffer, Action<FileBuffer>?> detect)
            {
                try
                {
                    var fix = detect(d);
                    if (fix != null) fixer._fixes.Add((name, fix));
                }
                catch (Exception ex)
                {
                    TraceLogger.Log($"Checksum check ({name}) failed: {ex.Message}", StatusSeverityType.Warning);
                }
            }
            Try("N64", DetectN64);
            Try("Game Boy", DetectGameBoy);
            Try("GBA", DetectGba);
            Try("SNES", DetectSnes);
            Try("Genesis", DetectGenesis);
            Try("CD sector EDC/ECC", DetectCd);
            if (fixer._fixes.Count == 0) return null;
            TraceLogger.Log($"Checksums that will be fixed after corrupting: {string.Join(", ", fixer.Names)}.");
            return fixer;
        }

        public void Fix(FileBuffer d)
        {
            foreach (var (name, fix) in _fixes)
            {
                try
                {
                    fix(d);
                }
                catch (Exception ex)
                {
                    TraceLogger.Log($"Unable to fix the {name} checksum: {ex.Message}", StatusSeverityType.Warning);
                }
            }
        }

        private static bool Magic(FileBuffer d, long offset, ReadOnlySpan<byte> magic)
        {
            if (offset < 0 || offset + magic.Length > d.Length) return false;
            Span<byte> b = stackalloc byte[magic.Length];
            d.Read(offset, b);
            return b.SequenceEqual(magic);
        }

        /// <summary>
        /// Sum of every byte in [start, end).
        /// </summary>
        private static ulong SumBytes(FileBuffer d, long start, long end)
        {
            ulong sum = 0;
            byte[] chunk = new byte[1 << 16];
            for (long pos = start; pos < end; pos += chunk.Length)
            {
                int n = d.Read(pos, chunk.AsSpan(0, (int)Math.Min(chunk.Length, end - pos)));
                for (int k = 0; k < n; k++) sum += chunk[k];
                if (n == 0) break;
            }
            return sum;
        }

        private static void WriteIfChanged(FileBuffer d, long offset, byte value)
        {
            if (d[offset] != value) d[offset] = value;
        }

        // ---------------- N64 ----------------
        private static Action<FileBuffer>? DetectN64(FileBuffer d)
        {
            var info = N64Rom.Detect(d);
            return info == null ? null : b => N64Rom.FixChecksum(b, info);
        }

        // ---------------- Game Boy / Game Boy Color ----------------
        private static readonly byte[] GbLogoStart = [0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B];

        private static byte GbHeaderChecksum(FileBuffer d)
        {
            byte x = 0;
            for (long i = 0x134; i <= 0x14C; i++) x = (byte)(x - d[i] - 1);
            return x;
        }

        private static ushort GbGlobalChecksum(FileBuffer d) => (ushort)(SumBytes(d, 0, d.Length) - d[0x14E] - d[0x14F]);

        private static Action<FileBuffer>? DetectGameBoy(FileBuffer d)
        {
            if (d.Length < 0x8000 || !Magic(d, 0x104, GbLogoStart)) return null;
            bool header = GbHeaderChecksum(d) == d[0x14D];
            bool global = GbGlobalChecksum(d) == (ushort)(d[0x14E] << 8 | d[0x14F]);
            if (!header && !global) return null;
            return b =>
            {
                if (header) WriteIfChanged(b, 0x14D, GbHeaderChecksum(b));
                if (global)
                {
                    ushort g = GbGlobalChecksum(b);
                    WriteIfChanged(b, 0x14E, (byte)(g >> 8));
                    WriteIfChanged(b, 0x14F, (byte)g);
                }
            };
        }

        // ---------------- GBA ----------------
        private static byte GbaComplement(FileBuffer d)
        {
            int chk = 0;
            for (long i = 0xA0; i <= 0xBC; i++) chk -= d[i];
            return (byte)((chk - 0x19) & 0xFF);
        }

        private static Action<FileBuffer>? DetectGba(FileBuffer d)
        {
            if (d.Length < 0xC0 || d[0xB2] != 0x96 || GbaComplement(d) != d[0xBD]) return null;
            return b => WriteIfChanged(b, 0xBD, GbaComplement(b));
        }

        // ---------------- SNES ----------------
        /// <summary>
        /// SNES checksum: 16-bit sum of every byte. ROMs that aren't a power of two in size have their last part mirrored up to the next
        /// power of two (only the common "power of two + smaller power of two" layouts are supported).
        /// </summary>
        private static ushort? SnesSum(FileBuffer d, long copier, long headerOffset)
        {
            long size = d.Length - copier;
            long p = 1L << (63 - System.Numerics.BitOperations.LeadingZeroCount((ulong)size)); //Largest power of two <= size
            long rest = size - p;
            ulong total;
            long headerCopies = 1; //How many times the header appears in the mirrored image
            if (rest == 0)
            {
                total = SumBytes(d, copier, d.Length);
            }
            else
            {
                if ((rest & (rest - 1)) != 0 || p % rest != 0) return null;
                total = SumBytes(d, copier, copier + p) + SumBytes(d, copier + p, d.Length) * (ulong)(p / rest);
                if (headerOffset >= p) headerCopies = p / rest;
            }
            //Count the checksum + complement bytes as 0xFF + 0xFF (that's what they always add up to once they're written correctly)
            long h = copier + headerOffset;
            ulong current = (ulong)d[h + 0x1C] + d[h + 0x1D] + d[h + 0x1E] + d[h + 0x1F];
            total = total - current * (ulong)headerCopies + 0x1FEul * (ulong)headerCopies;
            return (ushort)total;
        }

        private static Action<FileBuffer>? DetectSnes(FileBuffer d)
        {
            long copier = d.Length % 1024 == 512 ? 512 : 0;
            foreach (long header in new long[] { 0x7FC0, 0xFFC0, 0x40FFC0 })
            {
                long h = copier + header;
                if (h + 0x20 > d.Length) continue;
                ushort complement = d.ReadUInt16(h + 0x1C, false), checksum = d.ReadUInt16(h + 0x1E, false);
                if ((complement ^ checksum) != 0xFFFF) continue;
                var sum = SnesSum(d, copier, header);
                if (sum != checksum) continue;
                long headerOffset = header;
                return b =>
                {
                    var s = SnesSum(b, copier, headerOffset);
                    if (s == null) return;
                    ushort c = s.Value, k = (ushort)(c ^ 0xFFFF);
                    long at = copier + headerOffset;
                    WriteIfChanged(b, at + 0x1C, (byte)k);
                    WriteIfChanged(b, at + 0x1D, (byte)(k >> 8));
                    WriteIfChanged(b, at + 0x1E, (byte)c);
                    WriteIfChanged(b, at + 0x1F, (byte)(c >> 8));
                };
            }
            return null;
        }

        // ---------------- Genesis / Mega Drive ----------------
        private static ushort GenesisSum(FileBuffer d)
        {
            ushort sum = 0;
            byte[] chunk = new byte[1 << 16];
            long end = d.Length & ~1L;
            for (long pos = 0x200; pos < end; pos += chunk.Length)
            {
                int n = d.Read(pos, chunk.AsSpan(0, (int)Math.Min(chunk.Length, end - pos))) & ~1;
                for (int k = 0; k < n; k += 2) sum += (ushort)(chunk[k] << 8 | chunk[k + 1]);
                if (n == 0) break;
            }
            return sum;
        }

        private static Action<FileBuffer>? DetectGenesis(FileBuffer d)
        {
            if (d.Length < 0x400 || !(Magic(d, 0x100, "SEGA"u8) || Magic(d, 0x101, "SEGA"u8))) return null;
            if (GenesisSum(d) != d.ReadUInt16(0x18E, true)) return null;
            return b =>
            {
                ushort s = GenesisSum(b);
                WriteIfChanged(b, 0x18E, (byte)(s >> 8));
                WriteIfChanged(b, 0x18F, (byte)s);
            };
        }

        // ---------------- Raw CD sectors (2352 bytes): EDC + ECC ----------------
        private const int Sector = 2352;
        private static readonly byte[] EccF = new byte[256], EccB = new byte[256];
        private static readonly uint[] EdcTable = new uint[256];

        static ChecksumFixer()
        {
            for (int i = 0; i < 256; i++)
            {
                int j = (i << 1) ^ ((i & 0x80) != 0 ? 0x11D : 0);
                EccF[i] = (byte)j;
                EccB[i ^ j] = (byte)i;
                uint edc = (uint)i;
                for (int k = 0; k < 8; k++) edc = (edc >> 1) ^ ((edc & 1) != 0 ? 0xD8018001 : 0);
                EdcTable[i] = edc;
            }
        }

        private static uint Edc(ReadOnlySpan<byte> data)
        {
            uint edc = 0;
            foreach (byte b in data) edc = (edc >> 8) ^ EdcTable[(edc ^ b) & 0xFF];
            return edc;
        }

        private static void EccBlock(Span<byte> sector, int majorCount, int minorCount, int majorMult, int minorInc, int destOffset)
        {
            var src = sector[0xC..];
            int size = majorCount * minorCount;
            for (int major = 0; major < majorCount; major++)
            {
                int index = (major >> 1) * majorMult + (major & 1);
                byte a = 0, b = 0;
                for (int minor = 0; minor < minorCount; minor++)
                {
                    byte temp = src[index];
                    index += minorInc;
                    if (index >= size) index -= size;
                    a ^= temp;
                    b ^= temp;
                    a = EccF[a];
                }
                a = EccB[EccF[a] ^ b];
                sector[destOffset + major] = a;
                sector[destOffset + major + majorCount] = (byte)(a ^ b);
            }
        }

        private static void Ecc(Span<byte> sector, bool zeroAddress)
        {
            Span<byte> address = stackalloc byte[4];
            sector.Slice(12, 4).CopyTo(address);
            if (zeroAddress) sector.Slice(12, 4).Clear();
            EccBlock(sector, 86, 24, 2, 86, 0x81C);
            EccBlock(sector, 52, 43, 86, 88, 0x8C8);
            address.CopyTo(sector.Slice(12, 4));
        }

        private static bool HasSync(ReadOnlySpan<byte> s)
        {
            if (s[0] != 0 || s[11] != 0) return false;
            for (int i = 1; i < 11; i++) if (s[i] != 0xFF) return false;
            return true;
        }

        private enum SectorKind { None, Mode1, Mode2Form1, Mode2Form2 }

        private static SectorKind KindOf(ReadOnlySpan<byte> s)
        {
            if (!HasSync(s)) return SectorKind.None;
            if (s[15] == 1) return SectorKind.Mode1;
            if (s[15] != 2) return SectorKind.None;
            if (s[0x12] != s[0x16]) return SectorKind.None; //The two subheader copies disagree (the subheader itself was corrupted)
            return (s[0x12] & 0x20) != 0 ? SectorKind.Mode2Form2 : SectorKind.Mode2Form1;
        }

        /// <summary>
        /// Recalculates the EDC (and ECC) of a sector in place. Returns false for sectors without a data header (audio).
        /// </summary>
        private static bool RebuildSector(Span<byte> s)
        {
            switch (KindOf(s))
            {
                case SectorKind.Mode1:
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(s[0x810..], Edc(s[..0x810]));
                    s.Slice(0x814, 8).Clear();
                    Ecc(s, false);
                    return true;
                case SectorKind.Mode2Form1:
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(s[0x818..], Edc(s.Slice(0x10, 0x808)));
                    Ecc(s, true);
                    return true;
                case SectorKind.Mode2Form2:
                    if (System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(s[0x92C..]) == 0) return true; //No EDC used on this sector
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(s[0x92C..], Edc(s.Slice(0x10, 0x91C)));
                    return true;
                default:
                    return false;
            }
        }

        private static Action<FileBuffer>? DetectCd(FileBuffer d)
        {
            if (d.Length < Sector * 32 || d.Length % Sector != 0) return null;
            byte[] s = new byte[Sector], copy = new byte[Sector];
            d.Read(0, s);
            if (!HasSync(s)) return null;
            //Check a few data sectors: the recalculated EDC/ECC must match what's on the disc
            int checkedSectors = 0;
            for (long sector = 16; sector < Math.Min(d.Length / Sector, 40) && checkedSectors < 4; sector++)
            {
                d.Read(sector * Sector, s);
                if (KindOf(s) is SectorKind.None or SectorKind.Mode2Form2) continue;
                s.CopyTo(copy, 0);
                RebuildSector(copy);
                if (!s.AsSpan().SequenceEqual(copy)) return null;
                checkedSectors++;
            }
            if (checkedSectors == 0) return null;
            return b =>
            {
                var sectors = b.ModifiedPages.SelectMany(page =>
                {
                    long first = page * FileBuffer.PageSize / Sector, last = Math.Min(((page + 1) * FileBuffer.PageSize - 1) / Sector, b.Length / Sector - 1);
                    return Enumerable.Range(0, (int)(last - first + 1)).Select(k => first + k);
                }).Distinct().OrderBy(x => x).ToList();
                byte[] sec = new byte[Sector];
                int rebuilt = 0;
                foreach (long sector in sectors)
                {
                    long at = sector * Sector;
                    b.Read(at, sec);
                    byte[] before = (byte[])sec.Clone();
                    if (!RebuildSector(sec)) continue;
                    for (int k = 0; k < Sector; k++)
                        if (before[k] != sec[k]) b[at + k] = sec[k];
                    rebuilt++;
                }
                if (rebuilt > 0) TraceLogger.Log($"CD sector EDC/ECC recalculated for {rebuilt} sector(s).");
            };
        }
    }

    /// <summary>
    /// Bisect mode: finds the stash changes that make a corruption crash by halving the suspects each round.
    /// Each test applies every change except half of the suspects (and except changes already proven not to matter):
    /// - still crashes: the removed half doesn't matter, the crash is in the other half
    /// - works: the crash is in the removed half
    /// When one change is left (or the user stops early), the suspects can be excluded and the rest of the stash re-applied.
    /// </summary>
    public sealed class BisectSession
    {
        private readonly (long Offset, byte Value)[] _all; //One entry per offset (last value wins), sorted by offset so halves are contiguous areas
        private List<int> _suspects;
        private HashSet<int> _cleared = [];
        private List<int>? _removed; //The half left out of the current test
        private readonly Stack<(List<int> Suspects, HashSet<int> Cleared)> _history = new();

        public BisectSession(IEnumerable<(long Offset, byte Value)> entries)
        {
            var last = new Dictionary<long, byte>();
            foreach (var (offset, value) in entries) last[offset] = value;
            _all = [.. last.OrderBy(p => p.Key).Select(p => (p.Key, p.Value))];
            _suspects = [.. Enumerable.Range(0, _all.Length)];
        }

        public int Total => _all.Length;
        public int SuspectCount => _suspects.Count;
        public int Round => _history.Count + 1;
        public bool Finished => _suspects.Count <= 1;
        public bool CanUndo => _history.Count > 0;
        public bool TestPending => _removed != null;
        public int RoundsLeft => _suspects.Count <= 1 ? 0 : (int)Math.Ceiling(Math.Log2(_suspects.Count));

        public IEnumerable<(long Offset, byte Value)> Suspects => _suspects.Select(i => _all[i]);

        /// <summary>
        /// Offset range covered by the suspects (for display).
        /// </summary>
        public ByteRange? SuspectSpan => _suspects.Count == 0 ? null : new ByteRange(_all[_suspects[0]].Offset, _all[_suspects[^1]].Offset);

        /// <summary>
        /// Offset range covered by the half that is left out of the current test.
        /// </summary>
        public ByteRange? RemovedSpan => _removed is { Count: > 0 } ? new ByteRange(_all[_removed[0]].Offset, _all[_removed[^1]].Offset) : null;

        /// <summary>
        /// Every change except the suspects: the stash without whatever makes it crash.
        /// </summary>
        public List<(long Offset, byte Value)> WithoutSuspects()
        {
            var suspects = new HashSet<int>(_suspects);
            return [.. Enumerable.Range(0, _all.Length).Where(i => !suspects.Contains(i)).Select(i => _all[i])];
        }

        /// <summary>
        /// Picks the half of the suspects to leave out and returns the changes to apply for this test.
        /// </summary>
        public List<(long Offset, byte Value)> NextTest()
        {
            if (Finished) throw new InvalidOperationException("Bisect has finished, there's only one suspect left.");
            _removed = _suspects.GetRange(0, _suspects.Count / 2);
            var skip = new HashSet<int>(_removed);
            skip.UnionWith(_cleared);
            return [.. Enumerable.Range(0, _all.Length).Where(i => !skip.Contains(i)).Select(i => _all[i])];
        }

        /// <summary>
        /// Records the result of the current test.
        /// </summary>
        public void Answer(bool stillCrashes)
        {
            if (_removed == null) throw new InvalidOperationException("Run a test before answering.");
            _history.Push(([.. _suspects], [.. _cleared]));
            var removed = new HashSet<int>(_removed);
            if (stillCrashes)
            {
                _cleared.UnionWith(_removed); //Not needed for the crash
                _suspects = [.. _suspects.Where(i => !removed.Contains(i))];
            }
            else
            {
                _suspects = _removed; //The crash went away when this half was left out
            }
            _removed = null;
        }

        public void Undo()
        {
            if (_history.Count == 0) return;
            (_suspects, _cleared) = _history.Pop();
            _removed = null;
        }

        /// <summary>
        /// Turns offsets into merged ranges. With wordAlign each offset is widened to its 4 byte word (whole instruction/pointer/float).
        /// </summary>
        public static List<ByteRange> ToRanges(IEnumerable<long> offsets, bool wordAlign) =>
            ExcludeList.Merge(offsets.Select(o => wordAlign ? new ByteRange(o & ~3L, (o & ~3L) + 3) : new ByteRange(o, o)));
    }
}
