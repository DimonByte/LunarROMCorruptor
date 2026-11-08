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

namespace LunarROMCorruptor.Modules.CorruptionInternals.Engines
{
    /// <summary>
    /// Compression Engine: finds compressed blocks (Yaz0, Yay0, MIO0 on N64/GameCube/Wii, LZ10/LZ11 on GBA/DS), unpacks them, corrupts the
    /// real (decompressed) data and packs it again into the same space. Corrupting compressed bytes directly usually breaks the decompressor
    /// and crashes the game, corrupting the unpacked data gives texture/model/sound/level glitches instead.
    /// If the corrupted data no longer fits in the original space, fewer corruptions are tried, and as a last resort only the literal bytes
    /// of the compressed stream are changed (which keeps the size the same).
    /// </summary>
    public class CompressionEngine
    {
        [Flags]
        public enum Formats
        {
            None = 0,
            Yaz0 = 1,
            Yay0 = 2,
            MIO0 = 4,
            LZ10 = 8,
            LZ11 = 16,
            All = Yaz0 | Yay0 | MIO0 | LZ10 | LZ11
        }

        public sealed class Settings
        {
            public Formats Formats = Formats.All;
            public CorruptionOptions Option = CorruptionOptions.RANDOM;
            public int TiltAmount = 1;
        }

        public readonly record struct Block(long Offset, int CompressedLength, int DecompressedSize, Formats Format);

        public const int MaxDecompressedSize = 16 * 1024 * 1024;
        private const int MinDecompressedSize = 32; //Smaller "blocks" are almost always false positives

        #region Finding blocks

        /// <summary>
        /// Finds every compressed block that lies completely inside [start, end]. Yaz0/Yay0/MIO0 are found by their signature, LZ10/LZ11 have no
        /// signature so they're only tried where something points at them: GBA ROM pointers (0x08xxxxxx), DS file system files, or the start of the file.
        /// </summary>
        public static List<Block> FindBlocks(FileBuffer d, Formats formats, long start, long end, CancellationToken token = default, int maxBlocks = int.MaxValue)
        {
            var blocks = new List<Block>();
            end = Math.Min(end, d.Length - 1);
            if ((formats & (Formats.Yaz0 | Formats.Yay0 | Formats.MIO0)) != 0)
            {
                byte[] buffer = new byte[1 << 20];
                long nextFree = long.MinValue; //End of the last block found: signatures before it are inside that block's data (or already seen)
                for (long chunk = start & ~3L; chunk <= end; chunk += buffer.Length - 4)
                {
                    token.ThrowIfCancellationRequested();
                    int n = d.Read(chunk, buffer);
                    for (int k = 0; k + 4 <= n; k += 4) //Blocks are always 4 byte aligned (usually 16)
                    {
                        long offset = chunk + k;
                        if (offset < start || offset > end || offset < nextFree) continue;
                        Formats f = buffer[k] switch
                        {
                            (byte)'Y' when buffer[k + 1] == 'a' && buffer[k + 2] == 'z' && buffer[k + 3] == '0' => Formats.Yaz0,
                            (byte)'Y' when buffer[k + 1] == 'a' && buffer[k + 2] == 'y' && buffer[k + 3] == '0' => Formats.Yay0,
                            (byte)'M' when buffer[k + 1] == 'I' && buffer[k + 2] == 'O' && buffer[k + 3] == '0' => Formats.MIO0,
                            _ => Formats.None
                        };
                        if (f == Formats.None || (formats & f) == 0) continue;
                        var block = TryReadBlock(d, offset, f);
                        if (block != null && block.Value.Offset + block.Value.CompressedLength - 1 <= end)
                        {
                            blocks.Add(block.Value);
                            if (blocks.Count >= maxBlocks) return blocks;
                            //Skip over the block. The chunks overlap by 4 bytes and a block can run into the next chunk, so the skip is
                            //tracked as a file offset; otherwise a block at a chunk boundary was found (and corrupted) twice.
                            nextFree = offset + Math.Max(4, block.Value.CompressedLength);
                        }
                    }
                    if (n < buffer.Length) break;
                }
            }
            if ((formats & (Formats.LZ10 | Formats.LZ11)) != 0)
            {
                var taken = blocks.Select(b => new ByteRange(b.Offset, b.Offset + b.CompressedLength - 1)).ToList();
                foreach (long offset in LzCandidates(d, start, end, token))
                {
                    if (taken.Any(r => offset >= r.Start && offset <= r.End)) continue;
                    byte type = d[offset];
                    Formats f = type == 0x10 ? Formats.LZ10 : type == 0x11 ? Formats.LZ11 : Formats.None;
                    if (f == Formats.None || (formats & f) == 0) continue;
                    var block = TryReadBlock(d, offset, f);
                    if (block == null || block.Value.Offset + block.Value.CompressedLength - 1 > end) continue;
                    blocks.Add(block.Value);
                    taken.Add(new ByteRange(offset, offset + block.Value.CompressedLength - 1));
                    if (blocks.Count >= maxBlocks) break;
                }
            }
            return [.. blocks.OrderBy(b => b.Offset)];
        }

        /// <summary>
        /// Offsets worth trying as LZ10/LZ11 blocks.
        /// </summary>
        private static IEnumerable<long> LzCandidates(FileBuffer d, long start, long end, CancellationToken token)
        {
            var result = new SortedSet<long>();
            long len = d.Length;
            if (len >= 0x200 && d.ReadUInt16(0x15C, false) == 0xCF56)
            {
                //Nintendo DS: every file in the FAT
                uint fatOffset = d.ReadUInt32(0x48, false), fatSize = d.ReadUInt32(0x4C, false);
                if (fatOffset + (long)fatSize <= len)
                {
                    for (long e = fatOffset; e + 8 <= fatOffset + fatSize; e += 8)
                    {
                        uint s = d.ReadUInt32(e, false), en = d.ReadUInt32(e + 4, false);
                        if (en > s && en <= len) result.Add(s);
                    }
                }
            }
            else if (len >= 0xC0 && d[0xB2] == 0x96)
            {
                //GBA: every aligned ROM pointer (0x08000000 + offset) that points at a 0x10/0x11 byte
                byte[] buffer = new byte[1 << 20];
                for (long chunk = 0; chunk < len; chunk += buffer.Length)
                {
                    token.ThrowIfCancellationRequested();
                    int n = d.Read(chunk, buffer);
                    for (int k = 0; k + 4 <= n; k += 4)
                    {
                        uint p = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(k));
                        if (p < 0x08000000 || p > 0x09FFFFFF || (p & 3) != 0) continue;
                        long target = p - 0x08000000L;
                        if (target < len && target >= start && target <= end && d[target] is 0x10 or 0x11)
                            result.Add(target);
                    }
                }
            }
            else
            {
                result.Add(0);
            }
            return result.Where(o => o >= start && o <= end);
        }

        /// <summary>
        /// Decodes the block at an offset to check it's valid. Returns null if it isn't a valid compressed block.
        /// </summary>
        public static Block? TryReadBlock(FileBuffer d, long offset, Formats format)
        {
            int declared = DeclaredSize(d, offset, format);
            if (declared < MinDecompressedSize || declared > MaxDecompressedSize) return null;
            byte[] src = ReadSource(d, offset, declared);
            var decoded = Decode(src, format, null);
            if (decoded == null) return null;
            return new Block(offset, decoded.Value.Consumed, decoded.Value.Data.Length, format);
        }

        private static int DeclaredSize(FileBuffer d, long offset, Formats format)
        {
            if (offset + 16 > d.Length && format is Formats.Yaz0 or Formats.Yay0 or Formats.MIO0) return -1;
            if (offset + 4 > d.Length) return -1;
            return format switch
            {
                Formats.Yaz0 or Formats.Yay0 or Formats.MIO0 => (int)Math.Min(int.MaxValue, d.ReadUInt32(offset + 4, true)),
                _ => (int)(d.ReadUInt32(offset, false) >> 8)
            };
        }

        /// <summary>
        /// Reads enough bytes for the decoder: compressed data is never more than ~1/8 larger than the data it unpacks to.
        /// </summary>
        private static byte[] ReadSource(FileBuffer d, long offset, int decompressedSize)
        {
            long want = Math.Min(d.Length - offset, decompressedSize + decompressedSize / 4 + 64);
            byte[] src = new byte[want];
            d.Read(offset, src);
            return src;
        }

        #endregion

        #region Decoders

        /// <summary>
        /// Decodes a block. Returns the data and how many compressed bytes were used, or null if the stream is invalid.
        /// When <paramref name="literals"/> is given, the source position of every literal byte is added to it.
        /// </summary>
        public static (byte[] Data, int Consumed)? Decode(byte[] src, Formats format, List<int>? literals) => format switch
        {
            Formats.Yaz0 => DecodeYaz0(src, literals),
            Formats.Yay0 => DecodeYay0(src, false, literals),
            Formats.MIO0 => DecodeYay0(src, true, literals),
            Formats.LZ10 => DecodeLz(src, false, literals),
            Formats.LZ11 => DecodeLz(src, true, literals),
            _ => null
        };

        private static (byte[], int)? DecodeYaz0(byte[] src, List<int>? literals)
        {
            if (src.Length < 16) return null;
            int size = (int)BinaryPrimitives.ReadUInt32BigEndian(src.AsSpan(4));
            if (size <= 0 || size > MaxDecompressedSize) return null;
            byte[] dst = new byte[size];
            int s = 16, o = 0;
            while (o < size)
            {
                if (s >= src.Length) return null;
                int code = src[s++];
                for (int bit = 0; bit < 8 && o < size; bit++, code <<= 1)
                {
                    if ((code & 0x80) != 0)
                    {
                        if (s >= src.Length) return null;
                        literals?.Add(s);
                        dst[o++] = src[s++];
                        continue;
                    }
                    if (s + 1 >= src.Length) return null;
                    int b0 = src[s++], b1 = src[s++];
                    int dist = ((b0 & 0x0F) << 8 | b1) + 1;
                    int n = b0 >> 4;
                    if (n == 0)
                    {
                        if (s >= src.Length) return null;
                        n = src[s++] + 0x12;
                    }
                    else n += 2;
                    if (dist > o) return null;
                    for (int k = 0; k < n && o < size; k++, o++) dst[o] = dst[o - dist];
                }
            }
            return (dst, s);
        }

        private static (byte[], int)? DecodeYay0(byte[] src, bool mio0, List<int>? literals)
        {
            if (src.Length < 16) return null;
            int size = (int)BinaryPrimitives.ReadUInt32BigEndian(src.AsSpan(4));
            long linkStart = BinaryPrimitives.ReadUInt32BigEndian(src.AsSpan(8));
            long chunkStart = BinaryPrimitives.ReadUInt32BigEndian(src.AsSpan(12));
            if (size <= 0 || size > MaxDecompressedSize || linkStart < 16 || chunkStart < 16 || linkStart >= src.Length || chunkStart > src.Length) return null;
            byte[] dst = new byte[size];
            int m = 16, l = (int)linkStart, c = (int)chunkStart, o = 0;
            uint mask = 0;
            int bitsLeft = 0;
            while (o < size)
            {
                if (bitsLeft == 0)
                {
                    if (m + 4 > src.Length) return null;
                    mask = BinaryPrimitives.ReadUInt32BigEndian(src.AsSpan(m));
                    m += 4;
                    bitsLeft = 32;
                }
                bool literal = (mask & 0x80000000) != 0;
                mask <<= 1;
                bitsLeft--;
                if (literal)
                {
                    if (c >= src.Length) return null;
                    literals?.Add(c);
                    dst[o++] = src[c++];
                    continue;
                }
                if (l + 2 > src.Length) return null;
                int link = src[l] << 8 | src[l + 1];
                l += 2;
                int dist = (link & 0xFFF) + 1;
                int n;
                if (mio0) n = (link >> 12) + 3;
                else
                {
                    n = link >> 12;
                    if (n == 0)
                    {
                        if (c >= src.Length) return null;
                        n = src[c++] + 0x12;
                    }
                    else n += 2;
                }
                if (dist > o) return null;
                for (int k = 0; k < n && o < size; k++, o++) dst[o] = dst[o - dist];
            }
            //The mask, link and chunk streams overlap nothing else, the block ends where the last of them ends
            if (m > linkStart || l > chunkStart && linkStart < chunkStart) return null;
            return (dst, Math.Max(m, Math.Max(l, c)));
        }

        private static (byte[], int)? DecodeLz(byte[] src, bool lz11, List<int>? literals)
        {
            if (src.Length < 4 || src[0] != (lz11 ? 0x11 : 0x10)) return null;
            int size = src[1] | src[2] << 8 | src[3] << 16;
            if (size <= 0 || size > MaxDecompressedSize) return null;
            byte[] dst = new byte[size];
            int s = 4, o = 0;
            while (o < size)
            {
                if (s >= src.Length) return null;
                int flags = src[s++];
                for (int bit = 0; bit < 8 && o < size; bit++, flags <<= 1)
                {
                    if ((flags & 0x80) == 0)
                    {
                        if (s >= src.Length) return null;
                        literals?.Add(s);
                        dst[o++] = src[s++];
                        continue;
                    }
                    int n, dist;
                    if (s + 1 >= src.Length) return null;
                    int b0 = src[s++], b1 = src[s++];
                    if (!lz11)
                    {
                        n = (b0 >> 4) + 3;
                        dist = ((b0 & 0xF) << 8 | b1) + 1;
                    }
                    else
                    {
                        switch (b0 >> 4)
                        {
                            case 0:
                                {
                                    if (s >= src.Length) return null;
                                    int b2 = src[s++];
                                    n = ((b0 & 0xF) << 4 | b1 >> 4) + 0x11;
                                    dist = ((b1 & 0xF) << 8 | b2) + 1;
                                    break;
                                }
                            case 1:
                                {
                                    if (s + 1 >= src.Length) return null;
                                    int b2 = src[s++], b3 = src[s++];
                                    n = ((b0 & 0xF) << 12 | b1 << 4 | b2 >> 4) + 0x111;
                                    dist = ((b2 & 0xF) << 8 | b3) + 1;
                                    break;
                                }
                            default:
                                n = (b0 >> 4) + 1;
                                dist = ((b0 & 0xF) << 8 | b1) + 1;
                                break;
                        }
                    }
                    if (dist > o) return null;
                    for (int k = 0; k < n && o < size; k++, o++) dst[o] = dst[o - dist];
                }
            }
            return (dst, s);
        }

        #endregion

        #region Encoders

        private readonly record struct Token(int Length, int Distance, byte Literal)
        {
            public bool IsMatch => Length > 0;
        }

        /// <summary>
        /// LZ match finder (hash chains + one step lazy matching) shared by every format.
        /// </summary>
        private static List<Token> FindTokens(byte[] data, int maxLength, int minDistance)
        {
            const int MinLength = 3, Window = 4096, MaxChain = 256, HashSize = 1 << 15;
            var tokens = new List<Token>(data.Length / 2);
            int[] head = new int[HashSize];
            int[] prev = new int[data.Length];
            Array.Fill(head, -1);
            int Hash(int i) => ((data[i] << 10) ^ (data[i + 1] << 5) ^ data[i + 2]) & (HashSize - 1);
            void Insert(int i)
            {
                if (i + 2 >= data.Length) return;
                int h = Hash(i);
                prev[i] = head[h];
                head[h] = i;
            }
            (int Length, int Distance) Longest(int i)
            {
                if (i + MinLength > data.Length) return (0, 0);
                int best = 0, bestDist = 0, limit = Math.Min(maxLength, data.Length - i);
                int candidate = head[Hash(i)];
                for (int chain = 0; candidate >= 0 && chain < MaxChain; chain++, candidate = prev[candidate])
                {
                    int dist = i - candidate;
                    if (dist > Window) break;
                    if (dist < minDistance) continue;
                    if (data[candidate + best] != data[i + best]) continue;
                    int n = 0;
                    while (n < limit && data[candidate + n] == data[i + n]) n++;
                    if (n > best)
                    {
                        best = n;
                        bestDist = dist;
                        if (n == limit) break;
                    }
                }
                return best >= MinLength ? (best, bestDist) : (0, 0);
            }

            int pos = 0;
            while (pos < data.Length)
            {
                var (len, dist) = Longest(pos);
                if (len > 0 && len < maxLength && pos + 1 < data.Length)
                {
                    Insert(pos);
                    var (nextLen, _) = Longest(pos + 1);
                    if (nextLen > len + 1)
                    {
                        tokens.Add(new Token(0, 0, data[pos]));
                        pos++;
                        continue; //pos is already inserted, the match at pos + 1 is found again next loop
                    }
                    tokens.Add(new Token(len, dist, 0));
                    for (int k = 1; k < len; k++) Insert(pos + k);
                    pos += len;
                    continue;
                }
                if (len > 0)
                {
                    tokens.Add(new Token(len, dist, 0));
                    for (int k = 0; k < len; k++) Insert(pos + k);
                    pos += len;
                }
                else
                {
                    tokens.Add(new Token(0, 0, data[pos]));
                    Insert(pos);
                    pos++;
                }
            }
            return tokens;
        }

        public static byte[] Encode(byte[] data, Formats format) => format switch
        {
            Formats.Yaz0 => EncodeYaz0(data),
            Formats.Yay0 => EncodeYay0(data, false),
            Formats.MIO0 => EncodeYay0(data, true),
            Formats.LZ10 => EncodeLz(data, false),
            Formats.LZ11 => EncodeLz(data, true),
            _ => throw new ArgumentException($"Unknown compression format {format}.")
        };

        private static byte[] EncodeYaz0(byte[] data)
        {
            var output = new List<byte>(data.Length / 2 + 32) { (byte)'Y', (byte)'a', (byte)'z', (byte)'0' };
            output.AddRange(BigEndian(data.Length));
            output.AddRange(new byte[8]);
            var tokens = FindTokens(data, 0xFF + 0x12, 1);
            for (int t = 0; t < tokens.Count; t += 8)
            {
                int codeAt = output.Count;
                output.Add(0);
                byte code = 0;
                for (int k = 0; k < 8 && t + k < tokens.Count; k++)
                {
                    var tok = tokens[t + k];
                    if (!tok.IsMatch)
                    {
                        code |= (byte)(0x80 >> k);
                        output.Add(tok.Literal);
                        continue;
                    }
                    int d = tok.Distance - 1;
                    if (tok.Length >= 0x12)
                    {
                        output.Add((byte)(d >> 8));
                        output.Add((byte)d);
                        output.Add((byte)(tok.Length - 0x12));
                    }
                    else
                    {
                        output.Add((byte)((tok.Length - 2) << 4 | d >> 8));
                        output.Add((byte)d);
                    }
                }
                output[codeAt] = code;
            }
            return [.. output];
        }

        private static byte[] EncodeYay0(byte[] data, bool mio0)
        {
            var tokens = FindTokens(data, mio0 ? 18 : 0xFF + 0x12, 1);
            var masks = new List<byte>();
            var links = new List<byte>();
            var chunks = new List<byte>();
            for (int t = 0; t < tokens.Count; t += 32)
            {
                uint mask = 0;
                for (int k = 0; k < 32 && t + k < tokens.Count; k++)
                {
                    var tok = tokens[t + k];
                    if (!tok.IsMatch)
                    {
                        mask |= 0x80000000u >> k;
                        chunks.Add(tok.Literal);
                        continue;
                    }
                    int d = tok.Distance - 1;
                    int link;
                    if (mio0) link = (tok.Length - 3) << 12 | d;
                    else if (tok.Length >= 0x12)
                    {
                        link = d;
                        chunks.Add((byte)(tok.Length - 0x12));
                    }
                    else link = (tok.Length - 2) << 12 | d;
                    links.Add((byte)(link >> 8));
                    links.Add((byte)link);
                }
                masks.AddRange(BigEndian((int)mask));
            }
            int linkOffset = 16 + masks.Count;
            int chunkOffset = linkOffset + links.Count;
            var output = new List<byte>(chunkOffset + chunks.Count);
            output.AddRange(mio0 ? "MIO0"u8.ToArray() : "Yay0"u8.ToArray());
            output.AddRange(BigEndian(data.Length));
            output.AddRange(BigEndian(linkOffset));
            output.AddRange(BigEndian(chunkOffset));
            output.AddRange(masks);
            output.AddRange(links);
            output.AddRange(chunks);
            return [.. output];
        }

        private static byte[] EncodeLz(byte[] data, bool lz11)
        {
            if (data.Length > 0xFFFFFF) throw new InvalidOperationException("LZ10/LZ11 blocks can't be larger than 16 MB.");
            //Back-references of distance 1 are avoided: the GBA/DS BIOS "VRAM" decompressors write 16 bits at a time and can't use them
            var tokens = FindTokens(data, lz11 ? 0xFFFF + 0x111 : 18, 2);
            var output = new List<byte>(data.Length / 2 + 8) { (byte)(lz11 ? 0x11 : 0x10), (byte)data.Length, (byte)(data.Length >> 8), (byte)(data.Length >> 16) };
            for (int t = 0; t < tokens.Count; t += 8)
            {
                int flagsAt = output.Count;
                output.Add(0);
                byte flags = 0;
                for (int k = 0; k < 8 && t + k < tokens.Count; k++)
                {
                    var tok = tokens[t + k];
                    if (!tok.IsMatch)
                    {
                        output.Add(tok.Literal);
                        continue;
                    }
                    flags |= (byte)(0x80 >> k);
                    int d = tok.Distance - 1, n = tok.Length;
                    if (!lz11)
                    {
                        output.Add((byte)((n - 3) << 4 | d >> 8));
                        output.Add((byte)d);
                    }
                    else if (n <= 16)
                    {
                        output.Add((byte)((n - 1) << 4 | d >> 8));
                        output.Add((byte)d);
                    }
                    else if (n <= 0x110)
                    {
                        int e = n - 0x11;
                        output.Add((byte)(e >> 4));
                        output.Add((byte)((e & 0xF) << 4 | d >> 8));
                        output.Add((byte)d);
                    }
                    else
                    {
                        int e = n - 0x111;
                        output.Add((byte)(0x10 | e >> 12));
                        output.Add((byte)(e >> 4));
                        output.Add((byte)((e & 0xF) << 4 | d >> 8));
                        output.Add((byte)d);
                    }
                }
                output[flagsAt] = flags;
            }
            while (output.Count % 4 != 0) output.Add(0);
            return [.. output];
        }

        private static byte[] BigEndian(int value)
        {
            byte[] b = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(b, (uint)value);
            return b;
        }

        #endregion

        #region Engine

        public static void Run(CorruptionContext ctx, Settings s)
        {
            if (s.Formats == Formats.None)
                throw new InvalidOperationException("Select at least one compression format for the Compression Engine.");
            var data = ctx.Data!;
            TraceLogger.Log("Compression Engine: searching for compressed blocks...");
            var blocks = FindBlocks(data, s.Formats, ctx.Start, ctx.End, ctx.Token)
                .Where(b => !ctx.IsExcluded(b.Offset, b.CompressedLength)).ToList();
            if (blocks.Count == 0)
            {
                TraceLogger.Log("Compression Engine didn't find any compressed blocks (of the selected formats) completely inside the selected byte range.", StatusSeverityType.Warning, true);
                return;
            }
            TraceLogger.Log($"Compression Engine found {blocks.Count} block(s): {string.Join(", ", blocks.GroupBy(b => b.Format).Select(g => $"{g.Count()} {g.Key}"))}.");

            //How many corruptions each block gets
            var plan = new Dictionary<int, long>();
            if (ctx.EveryNth)
            {
                for (int k = 0; k < blocks.Count; k++)
                    plan[k] = Math.Max(1, blocks[k].DecompressedSize / ctx.Intensity);
            }
            else
            {
                long totalSize = blocks.Sum(b => (long)b.DecompressedSize);
                for (long n = 0; n < ctx.Intensity; n++)
                {
                    long pick = ctx.Rnd.NextInt64(0, totalSize);
                    int k = 0;
                    while (pick >= blocks[k].DecompressedSize)
                    {
                        pick -= blocks[k].DecompressedSize;
                        k++;
                    }
                    plan[k] = plan.GetValueOrDefault(k) + 1;
                }
            }

            int repacked = 0, patched = 0, failed = 0, done = 0;
            foreach (var (index, count) in plan.OrderBy(p => p.Key))
            {
                ctx.Tick(done++, plan.Count);
                var block = blocks[index];
                try
                {
                    switch (CorruptBlock(ctx, s, block, count))
                    {
                        case BlockResult.Repacked: repacked++; break;
                        case BlockResult.LiteralPatch: patched++; break;
                        default: failed++; break;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    failed++;
                    TraceLogger.Log($"Compression Engine: unable to corrupt the {block.Format} block at 0x{block.Offset:X}: {ex.Message}", StatusSeverityType.Warning);
                }
            }
            ctx.Tick(plan.Count, plan.Count);
            TraceLogger.Log($"Compression Engine: {repacked} block(s) unpacked, corrupted and repacked, {patched} block(s) corrupted through their literal bytes (the repacked data didn't fit), {failed} block(s) skipped.");
        }

        private enum BlockResult { Repacked, LiteralPatch, Failed }

        private static BlockResult CorruptBlock(CorruptionContext ctx, Settings s, Block block, long count)
        {
            var data = ctx.Data!;
            byte[] src = new byte[block.CompressedLength];
            data.Read(block.Offset, src);
            var decoded = Decode(src, block.Format, null);
            if (decoded == null) return BlockResult.Failed;
            byte[] original = decoded.Value.Data;

            //Try the full amount of corruption first, then fewer corruptions if the repacked block doesn't fit
            for (int attempt = 0; attempt < 4 && count > 0; attempt++, count = Math.Max(1, count / 2))
            {
                byte[] work = (byte[])original.Clone();
                if (ctx.EveryNth)
                {
                    long step = Math.Max(1, work.Length / Math.Max(1, count));
                    for (long i = ctx.Rnd.NextInt64(0, step); i < work.Length; i += step)
                        work[i] = CorruptValue(ctx.Rnd, s, work[i]);
                }
                else
                {
                    for (long n = 0; n < count; n++)
                    {
                        int i = ctx.Rnd.Next(work.Length);
                        work[i] = CorruptValue(ctx.Rnd, s, work[i]);
                    }
                }
                byte[] packed = Encode(work, block.Format);
                if (packed.Length > block.CompressedLength) continue;
                //Write the new block and clear the rest of the old space (the decompressors stop at the unpacked size, so the padding is never read)
                for (int k = 0; k < block.CompressedLength; k++)
                {
                    byte value = k < packed.Length ? packed[k] : (byte)0;
                    if (src[k] != value) ctx.Set(block.Offset + k, value);
                }
                return BlockResult.Repacked;
            }

            //Last resort: change literal bytes inside the compressed stream, the size stays exactly the same
            var literals = new List<int>();
            Decode(src, block.Format, literals);
            if (literals.Count == 0) return BlockResult.Failed;
            long patches = Math.Max(1, Math.Min(count, literals.Count));
            for (long n = 0; n < patches; n++)
            {
                int at = literals[ctx.Rnd.Next(literals.Count)];
                long offset = block.Offset + at;
                ctx.Set(offset, CorruptValue(ctx.Rnd, s, ctx.Get(offset)));
            }
            return BlockResult.LiteralPatch;
        }

        private static byte CorruptValue(Random rnd, Settings s, byte value) => s.Option switch
        {
            CorruptionOptions.RANDOMTILT => rnd.Next(3) switch
            {
                0 => (byte)rnd.Next(256),
                1 => CorruptionCore.ClampByte(value + s.TiltAmount),
                _ => CorruptionCore.ClampByte(value - s.TiltAmount)
            },
            CorruptionOptions.TILT => rnd.Next(2) == 0 ? CorruptionCore.ClampByte(value + s.TiltAmount) : CorruptionCore.ClampByte(value - s.TiltAmount),
            _ => (byte)rnd.Next(256)
        };

        #endregion
    }
}
