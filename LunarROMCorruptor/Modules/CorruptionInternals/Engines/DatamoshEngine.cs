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


using System.Buffers.Binary;
using System.Text;

namespace LunarROMCorruptor.Modules.CorruptionInternals.Engines
{
    /// <summary>
    /// Datamosh Engine: classic video datamoshing for AVI files (Xvid/DivX/MPEG-4 and H.264 in AVI).
    /// - REMOVE_KEYFRAMES ("melt"): keyframes (I-frames) are removed, so the motion of the following frames is applied to the previous picture.
    /// - DUPLICATE_FRAMES ("bloom"): motion frames (P-frames) are repeated, so the same motion keeps smearing the picture.
    /// The file is rebuilt (frames are removed/added), the idx1 index and header frame counts are regenerated.
    /// Every change is stored in the stash as [x] File(chunkOffset).FRAME(frameIndex).SET(DROP|REPLACE|REPEAT,n)
    /// </summary>
    public class DatamoshEngine
    {
        public enum MoshMode
        {
            REMOVE_KEYFRAMES,
            DUPLICATE_FRAMES,
            BOTH
        }

        public enum FrameType { Keyframe, Delta, Empty, Unknown }

        public sealed class Settings
        {
            public MoshMode Mode = MoshMode.REMOVE_KEYFRAMES;
            public int KeepFirstKeyframes = 1; //The first keyframe(s) are kept so the video starts with a picture
            public bool ReplaceRemoved = true; //Replace removed keyframes with the previous frame (keeps the timing/audio sync)
            public int RepeatCount = 10; //How many times a P-frame is repeated in DUPLICATE_FRAMES mode
        }

        public enum ActionKind { Drop, Replace, Repeat }
        public readonly record struct FrameAction(ActionKind Kind, int Count = 0);

        public sealed class MoviChunk
        {
            public required string FourCC;
            public long HeaderOffset; //Absolute offset of the chunk header in the file
            public uint Size; //Data size (without padding)
            public uint Flags; //idx1 flags if known
            public bool IsVideo;
            public FrameType Type = FrameType.Unknown;
            public int VideoIndex = -1; //Index among the video frames
        }

        public sealed class AviFile
        {
            public long MoviListOffset; //Offset of the "LIST" header of the movi list
            public long MoviDataStart; //Offset right after the "movi" fourcc
            public long MoviEnd; //End of the movi list data
            public long AvihOffset = -1; //Offset of the avih chunk data
            public long VideoStrhOffset = -1; //Offset of the video strh chunk data
            public long DmlhOffset = -1; //Offset of the OpenDML dmlh chunk data
            public readonly List<long> IndxOffsets = []; //Offsets of OpenDML 'indx' chunk headers (disabled when rebuilding)
            public readonly List<MoviChunk> Chunks = [];
            public string VideoStream = "00";
            public bool IsH264;
            public long Length;
        }

        #region Entry points

        public static void Run(Settings s, CorruptionContext ctx, string inputPath, string outputPath)
        {
            var avi = Parse(inputPath, ctx.Token);
            var video = avi.Chunks.Where(c => c.IsVideo).ToList();
            int keyframes = video.Count(c => c.Type == FrameType.Keyframe);
            int deltas = video.Count(c => c.Type == FrameType.Delta);
            TraceLogger.Log($"Datamosh Engine: {video.Count} video frames ({keyframes} keyframes, {deltas} motion frames), {avi.Chunks.Count - video.Count} other chunks.");
            if (video.Count == 0)
                throw new InvalidDataException("No video frames were found in this AVI file.");
            if (keyframes == 0 && deltas == 0)
                throw new InvalidDataException("Unable to tell keyframes from motion frames in this video. Re-encode it as Xvid/MPEG-4 AVI (see the Datamosh Engine help).");
            if (avi.IsH264 && s.Mode != MoshMode.DUPLICATE_FRAMES)
                TraceLogger.Log("This video is H.264. H.264 players often skip frames after a keyframe is removed, so the melt effect can be weaker. For the classic datamosh look convert it to Xvid first (see the Datamosh Engine help). DUPLICATE_FRAMES works well with H.264.", StatusSeverityType.Warning);

            var actions = new Dictionary<MoviChunk, FrameAction>();
            if (s.Mode is MoshMode.REMOVE_KEYFRAMES or MoshMode.BOTH)
            {
                var candidates = video.Where(c => c.Type == FrameType.Keyframe).Skip(Math.Max(0, s.KeepFirstKeyframes))
                                      .Where(c => c.HeaderOffset >= ctx.Start && c.HeaderOffset <= ctx.End).ToList();
                foreach (var c in Select(candidates, ctx))
                    actions[c] = new FrameAction(s.ReplaceRemoved ? ActionKind.Replace : ActionKind.Drop);
                if (candidates.Count == 0)
                    TraceLogger.Log("Datamosh Engine: no keyframes to remove in the selected range (the first keyframe(s) are always kept).", StatusSeverityType.Warning);
            }
            if (s.Mode is MoshMode.DUPLICATE_FRAMES or MoshMode.BOTH)
            {
                var candidates = video.Where(c => c.Type == FrameType.Delta && !actions.ContainsKey(c))
                                      .Where(c => c.HeaderOffset >= ctx.Start && c.HeaderOffset <= ctx.End).ToList();
                foreach (var c in Select(candidates, ctx))
                    actions[c] = new FrameAction(ActionKind.Repeat, Math.Clamp(s.RepeatCount, 1, 1000));
                if (candidates.Count == 0)
                    TraceLogger.Log("Datamosh Engine: no motion frames to duplicate in the selected range.", StatusSeverityType.Warning);
            }

            if (actions.Count == 0)
            {
                TraceLogger.Log("The Datamosh Engine didn't find any frames to change. Check the byte range and the mode.", StatusSeverityType.Warning, true);
                return;
            }

            foreach (var (chunk, action) in actions.OrderBy(a => a.Key.HeaderOffset))
                ctx.Stash?.AddLine(FormatFrameLine(chunk.HeaderOffset, chunk.VideoIndex, action));

            Rebuild(avi, inputPath, outputPath, actions, ctx.Token, ctx.Progress);
            TraceLogger.Log($"Datamosh Engine changed {actions.Count} frame(s) ({actions.Values.Count(a => a.Kind != ActionKind.Repeat)} keyframes removed, {actions.Values.Count(a => a.Kind == ActionKind.Repeat)} frames duplicated) and saved {outputPath}");
        }

        /// <summary>
        /// Re-applies FRAME stash entries to the original file.
        /// </summary>
        public static void ApplyStash(List<StashParser.FrameEntry> entries, string inputPath, string outputPath, StashLog stash)
        {
            var avi = Parse(inputPath, CancellationToken.None);
            var byOffset = avi.Chunks.Where(c => c.IsVideo).ToDictionary(c => c.HeaderOffset);
            var actions = new Dictionary<MoviChunk, FrameAction>();
            int missing = 0;
            foreach (var e in entries)
            {
                if (!byOffset.TryGetValue(e.Offset, out var chunk))
                {
                    missing++;
                    continue;
                }
                actions[chunk] = e.Action;
                stash.AddLine(FormatFrameLine(e.Offset, chunk.VideoIndex, e.Action));
            }
            Rebuild(avi, inputPath, outputPath, actions, CancellationToken.None, null);
            TraceLogger.Log($"Applied {actions.Count} Datamosh stash entries ({missing} skipped) to {outputPath}");
            if (missing > 0)
                TraceLogger.Log($"{missing} stash entries didn't match a video frame in this file. Was this stash made from a different video?", StatusSeverityType.Warning, true);
        }

        public static string FormatFrameLine(long offset, int frameIndex, FrameAction action) => action.Kind switch
        {
            ActionKind.Repeat => $"[x] File({offset}).FRAME({frameIndex}).SET(REPEAT,{action.Count})",
            ActionKind.Replace => $"[x] File({offset}).FRAME({frameIndex}).SET(REPLACE)",
            _ => $"[x] File({offset}).FRAME({frameIndex}).SET(DROP)"
        };

        private static List<MoviChunk> Select(List<MoviChunk> candidates, CorruptionContext ctx)
        {
            var selected = new List<MoviChunk>();
            if (ctx.EveryNth)
            {
                for (long n = 0; n < candidates.Count; n += ctx.Intensity)
                    selected.Add(candidates[(int)n]);
            }
            else
            {
                int count = (int)Math.Min(ctx.Intensity, candidates.Count);
                var pool = candidates.ToArray();
                for (int n = 0; n < count; n++)
                {
                    int j = ctx.Rnd.Next(n, pool.Length);
                    (pool[n], pool[j]) = (pool[j], pool[n]);
                    selected.Add(pool[n]);
                }
            }
            return selected;
        }

        #endregion

        #region AVI parsing

        private static string ReadFourCC(FileStream fs, long offset)
        {
            Span<byte> b = stackalloc byte[4];
            fs.Position = offset;
            if (fs.Read(b) < 4) return "";
            return Encoding.ASCII.GetString(b);
        }

        private static uint ReadUInt32(FileStream fs, long offset)
        {
            Span<byte> b = stackalloc byte[4];
            fs.Position = offset;
            if (fs.Read(b) < 4) return 0;
            return BinaryPrimitives.ReadUInt32LittleEndian(b);
        }

        public static AviFile Parse(string path, CancellationToken token)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16);
            var avi = new AviFile { Length = fs.Length };
            if (fs.Length < 12 || ReadFourCC(fs, 0) != "RIFF" || ReadFourCC(fs, 8) != "AVI ")
                throw new InvalidDataException("This isn't an AVI file. The Datamosh Engine needs an AVI video (convert it with ffmpeg, see the Datamosh Engine help).");
            long riffEnd = Math.Min(fs.Length, 8L + ReadUInt32(fs, 4));
            if (riffEnd + 12 <= fs.Length && ReadFourCC(fs, riffEnd + (riffEnd & 1)) == "RIFF" && ReadFourCC(fs, riffEnd + (riffEnd & 1) + 8) == "AVIX")
                throw new InvalidDataException("This is a large OpenDML AVI (more than 1 GB, with AVIX sections), which isn't supported. Use a shorter clip.");

            long idx1Offset = -1;
            uint idx1Size = 0;
            int streamIndex = 0;
            string? videoStream = null;
            ParseList(fs, 12, riffEnd, avi, ref idx1Offset, ref idx1Size, ref streamIndex, ref videoStream, token);
            if (avi.MoviListOffset == 0)
                throw new InvalidDataException("This AVI file doesn't have a 'movi' list (no video data found).");
            avi.VideoStream = videoStream ?? "00";

            foreach (var c in avi.Chunks)
                c.IsVideo = c.FourCC.StartsWith(avi.VideoStream, StringComparison.Ordinal) && (c.FourCC.EndsWith("dc", StringComparison.Ordinal) || c.FourCC.EndsWith("db", StringComparison.Ordinal));

            //idx1 flags (used when the frame type can't be read from the frame data)
            if (idx1Offset > 0)
                ReadIdx1Flags(fs, avi, idx1Offset, idx1Size);

            int videoIndex = 0;
            byte[] probe = new byte[4096];
            foreach (var c in avi.Chunks.Where(c => c.IsVideo))
            {
                token.ThrowIfCancellationRequested();
                c.VideoIndex = videoIndex++;
                if (c.Size == 0)
                {
                    c.Type = FrameType.Empty;
                    continue;
                }
                fs.Position = c.HeaderOffset + 8;
                int read = fs.Read(probe, 0, (int)Math.Min(probe.Length, c.Size));
                c.Type = DetectFrameType(probe.AsSpan(0, read), out bool h264);
                avi.IsH264 |= h264;
                if (c.Type == FrameType.Unknown && idx1Offset > 0)
                    c.Type = (c.Flags & 0x10) != 0 ? FrameType.Keyframe : FrameType.Delta;
            }
            return avi;
        }

        private static void ParseList(FileStream fs, long start, long end, AviFile avi, ref long idx1Offset, ref uint idx1Size, ref int streamIndex, ref string? videoStream, CancellationToken token, bool inMovi = false)
        {
            long pos = start;
            while (pos + 8 <= end)
            {
                token.ThrowIfCancellationRequested();
                string id = ReadFourCC(fs, pos);
                uint size = ReadUInt32(fs, pos + 4);
                long dataStart = pos + 8;
                long next = dataStart + size + (size & 1);
                if (next > end + 1) next = end; //Truncated file: stop at the end
                if (id == "LIST" && size >= 4)
                {
                    string listType = ReadFourCC(fs, dataStart);
                    if (listType == "movi")
                    {
                        avi.MoviListOffset = pos;
                        avi.MoviDataStart = dataStart + 4;
                        avi.MoviEnd = Math.Min(dataStart + size, end);
                        ParseList(fs, dataStart + 4, avi.MoviEnd, avi, ref idx1Offset, ref idx1Size, ref streamIndex, ref videoStream, token, inMovi: true);
                    }
                    else
                    {
                        //hdrl, strl, odml, rec ...
                        ParseList(fs, dataStart + 4, Math.Min(dataStart + size, end), avi, ref idx1Offset, ref idx1Size, ref streamIndex, ref videoStream, token, inMovi);
                    }
                }
                else if (inMovi)
                {
                    if (!id.StartsWith("ix", StringComparison.Ordinal) && id != "JUNK") //OpenDML standard indexes are dropped and rebuilt as idx1
                        avi.Chunks.Add(new MoviChunk { FourCC = id, HeaderOffset = pos, Size = size });
                }
                else
                {
                    switch (id)
                    {
                        case "avih": avi.AvihOffset = dataStart; break;
                        case "strh":
                            if (ReadFourCC(fs, dataStart) == "vids" && videoStream == null)
                            {
                                videoStream = streamIndex.ToString("00");
                                avi.VideoStrhOffset = dataStart;
                            }
                            streamIndex++;
                            break;
                        case "dmlh": avi.DmlhOffset = dataStart; break;
                        case "indx": avi.IndxOffsets.Add(pos); break;
                        case "idx1":
                            idx1Offset = dataStart;
                            idx1Size = size;
                            break;
                    }
                }
                if (next <= pos) break;
                pos = next;
            }
        }

        private static void ReadIdx1Flags(FileStream fs, AviFile avi, long offset, uint size)
        {
            //idx1 offsets are relative to the 'movi' fourcc (usually) or absolute; detect using the first entry
            var byOffset = avi.Chunks.ToDictionary(c => c.HeaderOffset);
            byte[] entries = new byte[Math.Min(size, 64u * 1024 * 1024)];
            fs.Position = offset;
            int read = fs.Read(entries, 0, entries.Length);
            if (read < 16) return;
            long firstOffset = BinaryPrimitives.ReadUInt32LittleEndian(entries.AsSpan(8));
            long baseOffset = byOffset.ContainsKey(firstOffset) ? 0 : avi.MoviDataStart - 4;
            for (int k = 0; k + 16 <= read; k += 16)
            {
                uint flags = BinaryPrimitives.ReadUInt32LittleEndian(entries.AsSpan(k + 4));
                long chunkOffset = baseOffset + BinaryPrimitives.ReadUInt32LittleEndian(entries.AsSpan(k + 8));
                if (byOffset.TryGetValue(chunkOffset, out var chunk))
                    chunk.Flags = flags;
            }
        }

        /// <summary>
        /// Reads the frame type from the frame data: MPEG-4 Part 2 (Xvid/DivX) VOP headers or H.264 NAL units.
        /// </summary>
        public static FrameType DetectFrameType(ReadOnlySpan<byte> data, out bool h264)
        {
            h264 = false;
            bool sawH264Slice = false;
            for (int i = 0; i + 4 < data.Length; i++)
            {
                if (data[i] != 0 || data[i + 1] != 0 || data[i + 2] != 1) continue;
                byte code = data[i + 3];
                if (code == 0xB6) //MPEG-4 VOP start code, the next 2 bits are the coding type
                {
                    if (i + 4 >= data.Length) break;
                    int vopType = data[i + 4] >> 6;
                    return vopType == 0 ? FrameType.Keyframe : FrameType.Delta;
                }
                int nalType = code & 0x1F;
                if ((code & 0x80) == 0 && nalType == 5) //H.264 IDR slice
                {
                    h264 = true;
                    return FrameType.Keyframe;
                }
                if ((code & 0x80) == 0 && nalType == 1) sawH264Slice = true; //H.264 non-IDR slice
            }
            h264 = sawH264Slice;
            return sawH264Slice ? FrameType.Delta : FrameType.Unknown;
        }

        #endregion

        #region Rebuilding

        private static void Rebuild(AviFile avi, string inputPath, string outputPath, Dictionary<MoviChunk, FrameAction> actions, CancellationToken token, IProgress<double>? progress)
        {
            string tempPath = outputPath + ".lrctmp";
            try
            {
                using (var src = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 16))
                using (var dst = new FileStream(tempPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1 << 16))
                {
                    //Everything before the movi list (headers) is copied as is
                    CopyRange(src, dst, 0, avi.MoviListOffset, token);
                    long moviListOffset = dst.Position;
                    dst.Write("LIST"u8);
                    dst.Write(new byte[4]); //Size, patched later
                    dst.Write("movi"u8);
                    long moviDataStart = dst.Position;

                    var index = new List<(string FourCC, uint Flags, uint Offset, uint Size)>();
                    MoviChunk? lastVideo = null;
                    int videoFrames = 0;
                    for (int k = 0; k < avi.Chunks.Count; k++)
                    {
                        var chunk = avi.Chunks[k];
                        if (k % 256 == 0)
                        {
                            token.ThrowIfCancellationRequested();
                            progress?.Report((double)k / avi.Chunks.Count);
                        }
                        int copies = 1;
                        MoviChunk source = chunk;
                        if (chunk.IsVideo && actions.TryGetValue(chunk, out var action))
                        {
                            switch (action.Kind)
                            {
                                case ActionKind.Drop:
                                    copies = 0;
                                    break;
                                case ActionKind.Replace:
                                    if (lastVideo != null) source = lastVideo; //Repeat the previous frame instead of the keyframe
                                    else copies = 0;
                                    break;
                                case ActionKind.Repeat:
                                    copies = 1 + action.Count;
                                    break;
                            }
                        }
                        for (int c = 0; c < copies; c++)
                        {
                            long headerPos = dst.Position;
                            WriteChunk(src, dst, chunk.IsVideo ? chunk.FourCC : source.FourCC, source, token);
                            uint flags = source.IsVideo ? (source.Type == FrameType.Keyframe ? 0x10u : 0u) : (source.Flags != 0 ? source.Flags : 0x10u);
                            index.Add((chunk.FourCC, flags, (uint)(headerPos - (moviDataStart - 4)), source.Size));
                            if (chunk.IsVideo) videoFrames++;
                        }
                        if (chunk.IsVideo && copies > 0 && source.Size > 0) lastVideo = source;
                    }
                    long moviEnd = dst.Position;

                    //New idx1 (offsets relative to the 'movi' fourcc)
                    dst.Write("idx1"u8);
                    WriteUInt32(dst, (uint)(index.Count * 16));
                    var entry = new byte[16];
                    foreach (var (fourcc, flags, offset, size) in index)
                    {
                        Encoding.ASCII.GetBytes(fourcc.PadRight(4).AsSpan()[..4], entry);
                        BinaryPrimitives.WriteUInt32LittleEndian(entry.AsSpan(4), flags);
                        BinaryPrimitives.WriteUInt32LittleEndian(entry.AsSpan(8), offset);
                        BinaryPrimitives.WriteUInt32LittleEndian(entry.AsSpan(12), size);
                        dst.Write(entry);
                    }
                    long fileEnd = dst.Position;

                    //Patch sizes and frame counts
                    PatchUInt32(dst, 4, (uint)(fileEnd - 8));
                    PatchUInt32(dst, moviListOffset + 4, (uint)(moviEnd - (moviListOffset + 8)));
                    if (avi.AvihOffset > 0) PatchUInt32(dst, avi.AvihOffset + 16, (uint)videoFrames); //dwTotalFrames
                    if (avi.VideoStrhOffset > 0) PatchUInt32(dst, avi.VideoStrhOffset + 32, (uint)videoFrames); //dwLength
                    if (avi.DmlhOffset > 0) PatchUInt32(dst, avi.DmlhOffset, (uint)videoFrames); //dwTotalFrames (OpenDML)
                    foreach (long indx in avi.IndxOffsets)
                    {
                        //The OpenDML super index points at the old ix## chunks, turn it into JUNK so players use the new idx1
                        dst.Position = indx;
                        dst.Write("JUNK"u8);
                    }
                    dst.Flush(true);
                }
                File.Move(tempPath, outputPath, true);
                progress?.Report(1.0);
            }
            finally
            {
                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
            }
        }

        private static void WriteChunk(FileStream src, FileStream dst, string fourcc, MoviChunk source, CancellationToken token)
        {
            dst.Write(Encoding.ASCII.GetBytes(fourcc.PadRight(4)[..4]));
            WriteUInt32(dst, source.Size);
            CopyRange(src, dst, source.HeaderOffset + 8, source.Size, token);
            if ((source.Size & 1) != 0) dst.WriteByte(0); //Chunks are padded to an even size
        }

        private static void CopyRange(FileStream src, FileStream dst, long offset, long count, CancellationToken token)
        {
            byte[] buffer = new byte[1 << 16];
            src.Position = offset;
            while (count > 0)
            {
                token.ThrowIfCancellationRequested();
                int read = src.Read(buffer, 0, (int)Math.Min(buffer.Length, count));
                if (read <= 0) throw new EndOfStreamException("The AVI file ended unexpectedly.");
                dst.Write(buffer, 0, read);
                count -= read;
            }
        }

        private static void WriteUInt32(FileStream fs, uint value)
        {
            Span<byte> b = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(b, value);
            fs.Write(b);
        }

        private static void PatchUInt32(FileStream fs, long offset, uint value)
        {
            long position = fs.Position;
            fs.Position = offset;
            WriteUInt32(fs, value);
            fs.Position = position;
        }

        #endregion
    }
}
