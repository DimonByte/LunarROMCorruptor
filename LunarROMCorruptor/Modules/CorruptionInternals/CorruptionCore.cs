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
        public static string? SourcePath; //The file that is loaded into LRC. In single file mode this file is only written to when the save location is the same file.
        public static long SourceLength; //The size of the loaded file in bytes
        public static long MaxByte; //This stores the maximum byte index of the file that is loaded (SourceLength - 1)
        public static double rawFileSize; //This stores the file size of the loaded ROM, used for displaying the size
        public static string? fileSizeUnit; //This stores the unit of the file size (E.g. MB, GB)
        public static string? LastOutputPath; //The last file that was written by a corruption (used by ByteView)
        public static readonly StashLog Stash = new(); //Records every byte that the engines change (replaces the old InternalStashItems string list)

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

            SourcePath = Path.GetFullPath(FileLocation);
            SourceLength = new FileInfo(SourcePath).Length;
            MaxByte = SourceLength - 1;
            FormatSize(SourceLength, out rawFileSize, out string unit);
            fileSizeUnit = unit;
            Stash.Clear();
            TraceLogger.Log($"File loaded: {SourcePath} ({SourceLength} bytes, {rawFileSize} {fileSizeUnit})");
            return true;
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
            var rnd = new Random();

            if (settings.Engine is CorruptionEngineType.SourceCodeEngine or CorruptionEngineType.DatamoshEngine)
            {
                return RunTextEngine(settings, outputPath, rnd, token, progress);
            }

            FileBuffer? buffer = null;
            try
            {
                buffer = PrepareOutput(outputPath);
                var ctx = new CorruptionContext(buffer, settings, rnd, recordStash ? Stash : null, token, progress);
                EngineProcessor.Run(settings, ctx);
                token.ThrowIfCancellationRequested();
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
                var ctx = new CorruptionContext(buffer, fileSettings, rnd, null, token, progress);
                EngineProcessor.Run(fileSettings, ctx);
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

            FileBuffer? buffer = null;
            try
            {
                buffer = PrepareOutput(outputPath);
                for (int n = 0; n < byteEntries.Count; n++)
                {
                    var (offset, value) = byteEntries[n];
                    buffer[offset] = value;
                    Stash.Add(offset, value);
                    if ((n & 0xFFFF) == 0)
                    {
                        token.ThrowIfCancellationRequested();
                        progress?.Report((double)n / byteEntries.Count);
                    }
                }
                CommitOutput(buffer, outputPath);
                buffer = null;
                TraceLogger.Log($"Finished applying stash file: {stashFileLocation}. Total stash items applied: {byteEntries.Count}");
                return CorruptionResult.Success;
            }
            catch (OperationCanceledException)
            {
                AbortOutput(buffer, outputPath);
                return CorruptionResult.Cancelled;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Failed to apply stash: {ex.Message}\n{ex}", StatusSeverityType.Error, true);
                AbortOutput(buffer, outputPath);
                return CorruptionResult.Failed;
            }
        }

        #endregion

        #region Output preparation, commit, abort and restore

        /// <summary>
        /// Set from the "Take File Ownership" checkbox. When true, the permission override is attempted if a file can't be read/written.
        /// </summary>
        public static bool AttemptOverrideOnWrite = false;

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

        private sealed class Page
        {
            public long Index;
            public readonly byte[] Data = new byte[PageSize];
            public int Count;
            public bool Dirty;
            public LinkedListNode<Page>? Node;
        }

        /// <summary>
        /// Default cache budget: a quarter of the available memory, between 64 MB and 2 GB.
        /// </summary>
        public static long? BudgetOverrideBytes; //Only used for testing the large-file (LRU) code path

        public static long DefaultBudgetBytes
        {
            get
            {
                if (BudgetOverrideBytes.HasValue) return BudgetOverrideBytes.Value;
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
                    if (PageFirstModified != null && _notifiedPages.Add(page.Index))
                        PageFirstModified(page.Index, page.Data, page.Count);
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
    /// Compact record of every byte that has been changed by a corruption. Formatting to the "[x] File(i).SET(v)" text is only done when needed.
    /// </summary>
    public sealed class StashLog
    {
        public const int MaxByteEntries = 10_000_000; //~160 MB of memory, more than that and the stash stops recording.
        private readonly List<(long Offset, byte Value)> _bytes = [];
        private readonly List<string> _lines = [];
        public bool Enabled = true;
        public bool Overflowed { get; private set; }
        public long Count => _bytes.Count + _lines.Count;

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
            if (_bytes.Capacity > 1_000_000) _bytes.TrimExcess();
            Overflowed = false;
        }

        public IEnumerable<string> FormatLines(int max = int.MaxValue)
        {
            int n = 0;
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

        public static bool IsValidStashLine(string line) => TryParseByte(line, out _, out _) || TryParseVarLine(line, out _) || TryParseFrame(line, out _);

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
}
