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

using LunarROMCorruptor.Modules.CorruptionInternals;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LunarROMCorruptor.Modules
{
    /// <summary>
    /// Everything LRC knows about a saved corruption (info.json in the stash folder). Only Name is required: stashes from older versions
    /// get their metadata from the stash headers when they're migrated.
    /// </summary>
    public sealed class StashMetadata
    {
        public string Format { get; set; } = StashLibrary.FormatName;
        public int Version { get; set; } = 1;
        public string Name { get; set; } = "";
        public string Notes { get; set; } = "";
        public List<string> Tags { get; set; } = [];
        public int Rating { get; set; } //0-5 stars
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Modified { get; set; } = DateTime.Now;
        public string LrcVersion { get; set; } = StashLibrary.AppVersion;

        //The file the stash was made for
        public string? SourcePath { get; set; }
        public long SourceSize { get; set; } = -1;
        public DateTime? SourceLastWriteUtc { get; set; }
        public string? SourceCrc32 { get; set; } //Hex, null when the file was too big to hash
        public string? Platform { get; set; } //Detected file type (N64, SNES, ...)
        public string? SaveAsPath { get; set; }

        //How it was made
        public string? Engine { get; set; }
        public Dictionary<string, string>? EngineSettings { get; set; } //Same format as engine presets
        public long StartByte { get; set; }
        public long EndByte { get; set; } = -1;
        public bool EveryNth { get; set; }
        public long Intensity { get; set; }
        public long Entries { get; set; }
        public string? BasedOn { get; set; } //Name of the stash this one was mutated/edited from

        //The emulator it was tested with (loaded again with the stash unless that's turned off in Settings)
        public string? EmulatorPath { get; set; }
        public string? EmulatorArguments { get; set; } //Only set when Override Arguments was ticked

        public List<string> Screenshots { get; set; } = []; //File names inside the screenshots folder, oldest first

        [JsonIgnore] public string SourceFileName => string.IsNullOrEmpty(SourcePath) ? "" : Path.GetFileName(SourcePath);

        public StashMetadata Clone()
        {
            var copy = (StashMetadata)MemberwiseClone();
            copy.Tags = [.. Tags];
            copy.Screenshots = [];
            copy.EngineSettings = EngineSettings == null ? null : new Dictionary<string, string>(EngineSettings);
            return copy;
        }

        /// <summary>
        /// One line per fact, used for tooltips and the details window.
        /// </summary>
        public string Describe()
        {
            var sb = new StringBuilder();
            if (Rating > 0) sb.AppendLine(new string('★', Math.Clamp(Rating, 0, 5)) + new string('☆', 5 - Math.Clamp(Rating, 0, 5)));
            if (!string.IsNullOrEmpty(SourcePath)) sb.AppendLine($"File: {SourceFileName}{(string.IsNullOrEmpty(Platform) ? "" : $" ({Platform})")}");
            if (!string.IsNullOrEmpty(Engine)) sb.AppendLine($"Engine: {Engine}");
            if (!string.IsNullOrEmpty(EmulatorPath)) sb.AppendLine($"Emulator: {Path.GetFileName(EmulatorPath)}{(EmulatorArguments != null ? " (custom arguments)" : "")}");
            if (EndByte >= 0) sb.AppendLine($"Range: 0x{StartByte:X} - 0x{EndByte:X}, {(EveryNth ? $"every {Intensity:N0}th" : $"intensity {Intensity:N0}")}");
            sb.AppendLine($"Changes: {Entries:N0}");
            sb.AppendLine($"Saved: {Created:yyyy-MM-dd HH:mm}");
            if (!string.IsNullOrEmpty(BasedOn)) sb.AppendLine($"Based on: {BasedOn}");
            if (Tags.Count > 0) sb.AppendLine($"Tags: {string.Join(", ", Tags)}");
            if (!string.IsNullOrWhiteSpace(Notes)) sb.AppendLine(Notes.Length > 200 ? Notes[..200] + "…" : Notes);
            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>
    /// A stash in the library: a folder with the stash file, info.json, an optional thumbnail and screenshots.
    /// </summary>
    public sealed class StashEntry
    {
        public required string Folder { get; set; }
        public required StashMetadata Meta { get; set; }

        public string StashFile => Path.Combine(Folder, StashLibrary.StashFileName);
        public string MetaFile => Path.Combine(Folder, StashLibrary.MetaFileName);
        public string ThumbnailFile => Path.Combine(Folder, StashLibrary.ThumbnailFileName);
        public string ScreenshotFolder => Path.Combine(Folder, StashLibrary.ScreenshotFolderName);
        public bool HasThumbnail => File.Exists(ThumbnailFile);
        public string Name => Meta.Name;

        /// <summary>
        /// The group (folder path relative to the library root, "" = top level).
        /// </summary>
        public string Group => StashLibrary.RelativeGroup(Path.GetDirectoryName(Folder)!);

        public IEnumerable<string> ScreenshotPaths => Meta.Screenshots.Select(s => Path.Combine(ScreenshotFolder, s)).Where(File.Exists);

        public override string ToString() => Name;
    }

    /// <summary>
    /// The stash library (the CorruptionStashList folder). Every stash is its own folder:
    ///   &lt;group&gt;\&lt;name&gt;\stash.stash      the stash itself (same text format as always)
    ///   &lt;group&gt;\&lt;name&gt;\info.json        metadata (file, engine + settings, range, notes, tags, rating...)
    ///   &lt;group&gt;\&lt;name&gt;\thumbnail.png    preview shown in the stash list
    ///   &lt;group&gt;\&lt;name&gt;\screenshots\     screenshots of the corruption
    /// Groups are plain folders (any folder that doesn't contain a stash.stash), so they can be nested and organised in Explorer too.
    /// Loose .stash files (older versions, copied in by hand) are moved into their own stash folder automatically.
    /// </summary>
    public static class StashLibrary
    {
        public const string FormatName = "LunarROMCorruptor stash";
        public const string StashFileName = "stash.stash";
        public const string MetaFileName = "info.json";
        public const string ThumbnailFileName = "thumbnail.png";
        public const string ScreenshotFolderName = "screenshots";
        public const string ExportExtension = ".lrcstash";
        public const int ThumbnailSize = 256;
        public const long HashLimit = 1L << 30; //Files up to 1 GB get a CRC32 so a changed/different file can be detected before replaying

        public static string Root => IOManager.CorruptionStashDirectory;
        public static string AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";

        /// <summary>
        /// Raised (on the calling thread) when stashes or groups were added, changed, moved or deleted.
        /// </summary>
        public static event Action? Changed;
        public static void RaiseChanged() => Changed?.Invoke();

        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        #region Paths and names

        public static bool IsStashFolder(string folder) => File.Exists(Path.Combine(folder, StashFileName));

        public static string RelativeGroup(string folder)
        {
            string rel = Path.GetRelativePath(Root, folder);
            return rel == "." ? "" : rel;
        }

        public static string GroupPath(string group) => string.IsNullOrEmpty(group) ? Root : Path.Combine(Root, group);

        /// <summary>
        /// Makes a name safe for a folder name (invalid characters replaced, reserved names avoided, trimmed to 80 characters).
        /// </summary>
        public static string SafeName(string name)
        {
            //Windows' rules (also when a shared stash is imported elsewhere): no \ / : * ? " < > | or control characters
            string clean = new([.. (name ?? "").Select(ch => ch < 32 || "\\/:*?\"<>|".Contains(ch) ? '_' : ch)]);
            clean = clean.Trim().TrimEnd('.', ' ');
            if (clean.Length > 80) clean = clean[..80].TrimEnd('.', ' ');
            if (clean.Length == 0) clean = "Stash";
            string upper = clean.ToUpperInvariant();
            if (upper is "CON" or "PRN" or "AUX" or "NUL" || (upper.Length == 4 && (upper.StartsWith("COM") || upper.StartsWith("LPT")) && char.IsDigit(upper[3])))
                clean = "_" + clean;
            return clean;
        }

        private static string UniqueFolder(string parent, string name, string? ignore = null)
        {
            string baseName = SafeName(name);
            string path = Path.Combine(parent, baseName);
            for (int n = 2; Directory.Exists(path) || File.Exists(path); n++)
            {
                if (ignore != null && CorruptionCore.PathsEqual(path, ignore)) break;
                path = Path.Combine(parent, $"{baseName} ({n})");
            }
            return path;
        }

        private static bool IsInside(string path, string folder)
        {
            string a = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string b = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return a.StartsWith(b, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Reading the library

        public sealed class GroupNode
        {
            public required string Path { get; init; }
            public required string Name { get; init; }
            public string Group => RelativeGroup(Path);
            public List<GroupNode> Groups { get; } = [];
            public List<StashEntry> Stashes { get; } = [];
            public int TotalStashes => Stashes.Count + Groups.Sum(g => g.TotalStashes);
        }

        /// <summary>
        /// Reads the whole library (migrating loose .stash files first). Safe to call from a background thread.
        /// </summary>
        public static GroupNode Load()
        {
            Directory.CreateDirectory(Root);
            MigrateLooseStashes(Root);
            return LoadGroup(Root, "");
        }

        private static GroupNode LoadGroup(string folder, string name)
        {
            var node = new GroupNode { Path = folder, Name = name };
            IEnumerable<string> dirs;
            try
            {
                dirs = [.. Directory.EnumerateDirectories(folder)];
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to read the stash folder {folder}: {ex.Message}", StatusSeverityType.Warning);
                return node;
            }
            foreach (string dir in dirs)
            {
                if (IsStashFolder(dir))
                {
                    var entry = TryLoadEntry(dir);
                    if (entry != null) node.Stashes.Add(entry);
                }
                else
                {
                    node.Groups.Add(LoadGroup(dir, Path.GetFileName(dir)));
                }
            }
            node.Groups.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
            return node;
        }

        /// <summary>
        /// Loads a stash folder. A missing or broken info.json is rebuilt from the stash headers.
        /// </summary>
        public static StashEntry? TryLoadEntry(string folder)
        {
            try
            {
                string metaFile = Path.Combine(folder, MetaFileName);
                StashMetadata? meta = null;
                if (File.Exists(metaFile))
                {
                    try
                    {
                        meta = JsonSerializer.Deserialize<StashMetadata>(File.ReadAllText(metaFile), JsonOptions);
                    }
                    catch (Exception ex)
                    {
                        TraceLogger.Log($"The stash info {metaFile} is damaged ({ex.Message}), it's rebuilt from the stash file.", StatusSeverityType.Warning);
                    }
                }
                bool save = false;
                if (meta == null)
                {
                    meta = MetadataFromStashFile(Path.Combine(folder, StashFileName));
                    save = true;
                }
                if (string.IsNullOrWhiteSpace(meta.Name)) { meta.Name = Path.GetFileName(folder); save = true; }
                meta.Tags ??= [];
                meta.Screenshots ??= [];
                var entry = new StashEntry { Folder = folder, Meta = meta };
                if (save) SaveMeta(entry, touch: false);
                return entry;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to read the stash {folder}: {ex.Message}", StatusSeverityType.Warning);
                return null;
            }
        }

        /// <summary>
        /// Builds metadata for a stash file that has none (older stashes): name from the file, file location from the headers.
        /// </summary>
        public static StashMetadata MetadataFromStashFile(string stashFile)
        {
            var info = new FileInfo(stashFile);
            var meta = new StashMetadata
            {
                Name = Path.GetFileNameWithoutExtension(stashFile).Equals("stash", StringComparison.OrdinalIgnoreCase) ? Path.GetFileName(Path.GetDirectoryName(stashFile)!) : Path.GetFileNameWithoutExtension(stashFile),
                Created = info.Exists ? info.LastWriteTime : DateTime.Now,
                Modified = info.Exists ? info.LastWriteTime : DateTime.Now,
                LrcVersion = ""
            };
            if (info.Exists)
            {
                if (StashParser.TryReadHeaders(stashFile, out var open, out var saveAs))
                {
                    meta.SourcePath = open;
                    meta.SaveAsPath = saveAs;
                }
                meta.Entries = CountEntries(stashFile);
            }
            return meta;
        }

        public static long CountEntries(string stashFile)
        {
            long count = 0;
            try
            {
                foreach (var line in File.ReadLines(stashFile))
                {
                    string t = line.Trim();
                    if (t.Length > 0 && !StashParser.TryParseHeader(t, out _, out _)) count++;
                }
            }
            catch { }
            return count;
        }

        /// <summary>
        /// Moves every loose .stash file (old stash list, files copied in by hand) into its own stash folder.
        /// </summary>
        public static int MigrateLooseStashes(string folder)
        {
            int moved = 0;
            try
            {
                foreach (string file in Directory.GetFiles(folder, "*.stash"))
                {
                    try
                    {
                        var meta = MetadataFromStashFile(file);
                        string target = UniqueFolder(folder, meta.Name);
                        Directory.CreateDirectory(target);
                        File.Move(file, Path.Combine(target, StashFileName));
                        SaveMeta(new StashEntry { Folder = target, Meta = meta }, touch: false);
                        moved++;
                    }
                    catch (Exception ex)
                    {
                        TraceLogger.Log($"Unable to move the stash {file} into its own folder: {ex.Message}", StatusSeverityType.Warning);
                    }
                }
                foreach (string dir in Directory.GetDirectories(folder))
                {
                    if (!IsStashFolder(dir)) moved += MigrateLooseStashes(dir);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to check {folder} for old stash files: {ex.Message}", StatusSeverityType.Warning);
            }
            if (moved > 0 && CorruptionCore.PathsEqual(folder, Root))
                TraceLogger.Log($"Moved {moved} stash file(s) into their own stash folders (stash + info + screenshots).");
            return moved;
        }

        /// <summary>
        /// The library entry that a stash file belongs to, or null if the file isn't a library stash (e.g. a history stash).
        /// </summary>
        public static StashEntry? EntryForStashFile(string? stashFile)
        {
            if (string.IsNullOrEmpty(stashFile) || !Path.GetFileName(stashFile).Equals(StashFileName, StringComparison.OrdinalIgnoreCase)) return null;
            string? folder = Path.GetDirectoryName(stashFile);
            return folder != null && IsInside(folder, Root) ? TryLoadEntry(folder) : null;
        }

        #endregion

        #region Creating and changing stashes

        public static void SaveMeta(StashEntry entry, bool touch = true)
        {
            if (touch) entry.Meta.Modified = DateTime.Now;
            string temp = entry.MetaFile + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(entry.Meta, JsonOptions));
            File.Move(temp, entry.MetaFile, true);
        }

        /// <summary>
        /// Creates a new stash folder in a group. <paramref name="writeStash"/> writes the stash file (e.g. StashLog.WriteTo).
        /// </summary>
        public static StashEntry Create(StashMetadata meta, string group, Action<string> writeStash)
        {
            string parent = GroupPath(group);
            Directory.CreateDirectory(parent);
            if (string.IsNullOrWhiteSpace(meta.Name)) meta.Name = "Stash " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            string folder = UniqueFolder(parent, meta.Name);
            Directory.CreateDirectory(folder);
            meta.Name = Path.GetFileName(folder); //Keep the name and folder name the same (the folder may have got a (2) suffix)
            meta.Created = meta.Modified = DateTime.Now;
            meta.LrcVersion = AppVersion;
            var entry = new StashEntry { Folder = folder, Meta = meta };
            try
            {
                writeStash(entry.StashFile);
                if (meta.Entries <= 0) meta.Entries = CountEntries(entry.StashFile);
                SaveMeta(entry, touch: false);
            }
            catch
            {
                try { Directory.Delete(folder, true); } catch { }
                throw;
            }
            TraceLogger.Log($"Stash saved: {folder}");
            return entry;
        }

        public static StashEntry CreateFromLines(StashMetadata meta, string group, IEnumerable<string> lines)
        {
            return Create(meta, group, path =>
            {
                using var writer = new StreamWriter(path, false, new UTF8Encoding(false), 1 << 16);
                foreach (var line in lines) writer.WriteLine(line);
            });
        }

        /// <summary>
        /// Called after the stash file of an entry was rewritten (Stash Editor): updates the entry count and the modified time.
        /// </summary>
        public static void StashFileChanged(string stashFile)
        {
            var entry = EntryForStashFile(stashFile);
            if (entry == null) return;
            try
            {
                entry.Meta.Entries = CountEntries(stashFile);
                SaveMeta(entry);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to update the stash info: {ex.Message}", StatusSeverityType.Warning);
            }
        }

        /// <summary>
        /// Renames a stash (its folder and its name). Returns false if the name is empty or taken.
        /// </summary>
        public static bool Rename(StashEntry entry, string newName)
        {
            newName = newName.Trim();
            if (newName.Length == 0) return false;
            string parent = Path.GetDirectoryName(entry.Folder)!;
            string target = Path.Combine(parent, SafeName(newName));
            if (!string.Equals(target, entry.Folder, StringComparison.Ordinal))
            {
                //Only a different letter case is still a rename (the folder is renamed too)
                if (!CorruptionCore.PathsEqual(target, entry.Folder) && (Directory.Exists(target) || File.Exists(target)))
                {
                    TraceLogger.Log($"There is already a stash or group called '{SafeName(newName)}' here.", StatusSeverityType.Warning, true);
                    return false;
                }
                MoveDirectory(entry.Folder, target);
                entry.Folder = target;
            }
            entry.Meta.Name = newName;
            SaveMeta(entry);
            return true;
        }

        /// <summary>
        /// Moves a stash or a group folder into another group. Returns the new folder, or null if it couldn't be moved.
        /// </summary>
        public static string? MoveToGroup(string folder, string group)
        {
            string parent = GroupPath(group);
            if (CorruptionCore.PathsEqual(Path.GetDirectoryName(folder), parent)) return folder;
            if (IsInside(parent, folder))
            {
                TraceLogger.Log("A group can't be moved into itself.", StatusSeverityType.Warning, true);
                return null;
            }
            Directory.CreateDirectory(parent);
            string target = UniqueFolder(parent, Path.GetFileName(folder));
            MoveDirectory(folder, target);
            //The stash name follows the folder name if it had to be renamed (" (2)")
            if (IsStashFolder(target) && !Path.GetFileName(target).Equals(Path.GetFileName(folder), StringComparison.OrdinalIgnoreCase))
            {
                var entry = TryLoadEntry(target);
                if (entry != null) { entry.Meta.Name = Path.GetFileName(target); SaveMeta(entry, touch: false); }
            }
            return target;
        }

        private static void MoveDirectory(string from, string to)
        {
            //Directory.Move can't change only the letter case of a name on Windows, go through a temporary name
            if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase) && from != to)
            {
                string temp = to + ".renaming-" + Guid.NewGuid().ToString("N")[..6];
                Directory.Move(from, temp);
                Directory.Move(temp, to);
                return;
            }
            Directory.Move(from, to);
        }

        public static void Delete(string folder)
        {
            if (!IsInside(folder, Root) || CorruptionCore.PathsEqual(folder, Root))
                throw new InvalidOperationException("Only stashes and groups inside the stash library can be deleted.");
            Directory.Delete(folder, true);
        }

        public static string CreateGroup(string parentGroup, string name)
        {
            string parent = GroupPath(parentGroup);
            Directory.CreateDirectory(parent);
            string folder = UniqueFolder(parent, name);
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static bool RenameGroup(string folder, string newName)
        {
            newName = newName.Trim();
            if (newName.Length == 0) return false;
            string target = Path.Combine(Path.GetDirectoryName(folder)!, SafeName(newName));
            if (string.Equals(target, folder, StringComparison.Ordinal)) return true;
            if (!CorruptionCore.PathsEqual(target, folder) && (Directory.Exists(target) || File.Exists(target)))
            {
                TraceLogger.Log($"There is already a group or stash called '{SafeName(newName)}' here.", StatusSeverityType.Warning, true);
                return false;
            }
            MoveDirectory(folder, target);
            return true;
        }

        /// <summary>
        /// Rewrites the OpenLocation/SaveAsLocation headers of a stash file (used when the file moved and the user picked its new location).
        /// </summary>
        public static void UpdateHeaders(string stashFile, string? openLocation, string? saveAsLocation)
        {
            string temp = stashFile + ".tmp";
            using (var writer = new StreamWriter(temp, false, new UTF8Encoding(false), 1 << 16))
            {
                if (!string.IsNullOrEmpty(openLocation)) writer.WriteLine(StashParser.FormatOpenLocation(openLocation));
                if (!string.IsNullOrEmpty(saveAsLocation)) writer.WriteLine(StashParser.FormatSaveAsLocation(saveAsLocation));
                foreach (var line in File.ReadLines(stashFile))
                {
                    if (StashParser.TryParseHeader(line.Trim(), out _, out _)) continue;
                    writer.WriteLine(line);
                }
            }
            File.Move(temp, stashFile, true);
        }

        #endregion

        #region Screenshots and thumbnails

        /// <summary>
        /// Scales an image down to fit in size x size (keeps the aspect ratio). The caller owns the result.
        /// </summary>
        public static Bitmap MakeThumbnail(Image image, int size = ThumbnailSize)
        {
            double scale = Math.Min(1.0, Math.Min((double)size / image.Width, (double)size / image.Height));
            int w = Math.Max(1, (int)Math.Round(image.Width * scale)), h = Math.Max(1, (int)Math.Round(image.Height * scale));
            var thumb = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(thumb);
            //Pixel art (small emulator screenshots, ByteView) stays sharp when it's scaled up, smooth when it's scaled down
            g.InterpolationMode = scale >= 1 ? System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor : System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            g.DrawImage(image, 0, 0, w, h);
            return thumb;
        }

        public static void SetThumbnail(StashEntry entry, Image image)
        {
            using var thumb = MakeThumbnail(image);
            string temp = entry.ThumbnailFile + ".tmp";
            thumb.Save(temp, ImageFormat.Png);
            File.Move(temp, entry.ThumbnailFile, true);
            ThumbnailCache.Invalidate(entry.ThumbnailFile);
        }

        /// <summary>
        /// Adds a screenshot to a stash (and makes it the thumbnail if the stash has none yet). Returns the screenshot's path.
        /// </summary>
        public static string AddScreenshot(StashEntry entry, Image image, bool makeThumbnail = false)
        {
            Directory.CreateDirectory(entry.ScreenshotFolder);
            string name = $"screenshot-{DateTime.Now:yyyyMMdd-HHmmss-fff}.png";
            string path = Path.Combine(entry.ScreenshotFolder, name);
            image.Save(path, ImageFormat.Png);
            entry.Meta.Screenshots.Add(name);
            if (makeThumbnail || !entry.HasThumbnail) SetThumbnail(entry, image);
            SaveMeta(entry);
            return path;
        }

        public static void RemoveScreenshot(StashEntry entry, string screenshotPath)
        {
            string name = Path.GetFileName(screenshotPath);
            entry.Meta.Screenshots.RemoveAll(s => s.Equals(name, StringComparison.OrdinalIgnoreCase));
            try { File.Delete(screenshotPath); } catch (Exception ex) { TraceLogger.Log($"Unable to delete the screenshot: {ex.Message}", StatusSeverityType.Warning); }
            SaveMeta(entry);
        }

        /// <summary>
        /// Loads an image without keeping the file locked (Image.FromFile keeps it open until the image is disposed).
        /// </summary>
        public static Image? LoadImageUnlocked(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                byte[] bytes = File.ReadAllBytes(path);
                using var ms = new MemoryStream(bytes);
                using var image = Image.FromStream(ms);
                return new Bitmap(image);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to load the image {path}: {ex.Message}", StatusSeverityType.Warning);
                return null;
            }
        }

        /// <summary>
        /// Small cache of thumbnails for the stash list hover previews (keyed by path + last write time).
        /// </summary>
        public static class ThumbnailCache
        {
            private const int MaxImages = 64;
            private static readonly Dictionary<string, (DateTime Time, Image Image)> _cache = new(StringComparer.OrdinalIgnoreCase);
            private static readonly LinkedList<string> _order = new();

            public static Image? Get(string path)
            {
                if (!File.Exists(path)) return null;
                DateTime time = File.GetLastWriteTimeUtc(path);
                if (_cache.TryGetValue(path, out var cached))
                {
                    if (cached.Time == time) return cached.Image;
                    Invalidate(path);
                }
                var image = LoadImageUnlocked(path);
                if (image == null) return null;
                _cache[path] = (time, image);
                _order.AddFirst(path);
                while (_order.Count > MaxImages)
                {
                    string old = _order.Last!.Value;
                    _order.RemoveLast();
                    if (_cache.Remove(old, out var removed)) removed.Image.Dispose();
                }
                return image;
            }

            public static void Invalidate(string path)
            {
                if (_cache.Remove(path, out var old))
                {
                    _order.Remove(path);
                    old.Image.Dispose();
                }
            }

            public static void Clear()
            {
                foreach (var (_, item) in _cache) item.Image.Dispose();
                _cache.Clear();
                _order.Clear();
            }
        }

        #endregion

        #region Source file checks

        /// <summary>
        /// Fills in size, last write time, CRC32 (files up to <see cref="HashLimit"/>) and platform of the stash's source file.
        /// Reads the whole file, so run it on a background thread.
        /// </summary>
        public static void FillSourceInfo(StashMetadata meta, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(meta.SourcePath) || !File.Exists(meta.SourcePath)) return;
            var info = new FileInfo(meta.SourcePath);
            meta.SourceSize = info.Length;
            meta.SourceLastWriteUtc = info.LastWriteTimeUtc;
            meta.SourceCrc32 = info.Length <= HashLimit ? Crc32.OfFile(meta.SourcePath, token).ToString("X8") : null;
            try
            {
                using var data = FileBuffer.OpenRead(meta.SourcePath, 4);
                meta.Platform ??= CorruptionInternals.Engines.ExclusionEngine.DetectProfile(meta.SourcePath, data);
            }
            catch { }
        }

        public enum SourceCheck { Same, Unknown, Changed }

        /// <summary>
        /// Checks whether a file is still the one the stash was made for. Same size + same last write time counts as unchanged,
        /// otherwise the CRC32 decides (when there is one). Can read the whole file, run it on a background thread.
        /// </summary>
        public static SourceCheck CheckSource(StashMetadata meta, string path, CancellationToken token = default)
        {
            try
            {
                var info = new FileInfo(path);
                if (!info.Exists) return SourceCheck.Changed;
                if (meta.SourceSize >= 0 && info.Length != meta.SourceSize) return SourceCheck.Changed;
                if (meta.SourceSize >= 0 && meta.SourceLastWriteUtc == info.LastWriteTimeUtc) return SourceCheck.Same;
                if (meta.SourceCrc32 == null || info.Length > HashLimit) return SourceCheck.Unknown;
                return Crc32.OfFile(path, token).ToString("X8") == meta.SourceCrc32 ? SourceCheck.Same : SourceCheck.Changed;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return SourceCheck.Unknown;
            }
        }

        #endregion

        #region Sharing (.lrcstash)

        /// <summary>
        /// Exports a stash folder (stash, info, thumbnail, screenshots) as one .lrcstash file (a zip) to share.
        /// </summary>
        public static void Export(StashEntry entry, string zipPath)
        {
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(entry.Folder, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false);
            TraceLogger.Log($"Stash exported: {zipPath}");
        }

        /// <summary>
        /// Imports a shared .lrcstash (zip) or a plain .stash file into a group. Returns the new entry.
        /// </summary>
        public static StashEntry? Import(string file, string group)
        {
            try
            {
                if (Path.GetExtension(file).Equals(".stash", StringComparison.OrdinalIgnoreCase))
                {
                    var meta = MetadataFromStashFile(file);
                    return Create(meta, group, path => File.Copy(file, path, true));
                }
                using var zip = ZipFile.OpenRead(file);
                if (zip.GetEntry(StashFileName) == null)
                    throw new InvalidDataException($"This isn't a shared LRC stash ({StashFileName} is missing).");
                string parent = GroupPath(group);
                Directory.CreateDirectory(parent);
                string folder = UniqueFolder(parent, Path.GetFileNameWithoutExtension(file));
                Directory.CreateDirectory(folder);
                string root = Path.GetFullPath(folder) + Path.DirectorySeparatorChar;
                foreach (var item in zip.Entries)
                {
                    if (string.IsNullOrEmpty(item.Name)) continue; //Folder entries
                    string target = Path.GetFullPath(Path.Combine(folder, item.FullName));
                    if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase)) continue; //No paths outside the stash folder ("zip slip")
                    if (item.Length > 512L * 1024 * 1024) continue;
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    item.ExtractToFile(target, true);
                }
                var entry = TryLoadEntry(folder);
                if (entry != null && !entry.Meta.Name.Equals(Path.GetFileName(folder), StringComparison.OrdinalIgnoreCase))
                {
                    //Keep the name the sharer gave it, if it's free here
                    string wanted = entry.Meta.Name;
                    if (!Rename(entry, wanted)) { entry.Meta.Name = Path.GetFileName(folder); SaveMeta(entry, touch: false); }
                }
                TraceLogger.Log($"Stash imported: {entry?.Folder}");
                return entry;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to import the stash {file}: {ex.Message}", StatusSeverityType.Error, true);
                return null;
            }
        }

        #endregion
    }

    /// <summary>
    /// CRC32 (IEEE, the same as zip/PNG) used to recognise the file a stash was made for.
    /// </summary>
    public static class Crc32
    {
        private static readonly uint[] Table = CreateTable();

        private static uint[] CreateTable()
        {
            var table = new uint[256 * 8];
            for (uint i = 0; i < 256; i++)
            {
                uint c = i;
                for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
                table[i] = c;
            }
            //Slicing-by-8 tables
            for (int i = 0; i < 256; i++)
                for (int s = 1; s < 8; s++)
                    table[s * 256 + i] = (table[(s - 1) * 256 + i] >> 8) ^ table[table[(s - 1) * 256 + i] & 0xFF];
            return table;
        }

        public static uint Append(uint crc, ReadOnlySpan<byte> data)
        {
            crc = ~crc;
            int i = 0;
            while (data.Length - i >= 8)
            {
                uint one = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[i..]) ^ crc;
                uint two = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data[(i + 4)..]);
                crc = Table[7 * 256 + (one & 0xFF)] ^ Table[6 * 256 + ((one >> 8) & 0xFF)] ^ Table[5 * 256 + ((one >> 16) & 0xFF)] ^ Table[4 * 256 + (one >> 24)] ^
                      Table[3 * 256 + (two & 0xFF)] ^ Table[2 * 256 + ((two >> 8) & 0xFF)] ^ Table[256 + ((two >> 16) & 0xFF)] ^ Table[two >> 24];
                i += 8;
            }
            for (; i < data.Length; i++)
                crc = Table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
            return ~crc;
        }

        public static uint OfFile(string path, CancellationToken token = default)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16, FileOptions.SequentialScan);
            var buffer = new byte[1 << 20];
            uint crc = 0;
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                token.ThrowIfCancellationRequested();
                crc = Append(crc, buffer.AsSpan(0, read));
            }
            return crc;
        }
    }
}
