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
using System.Globalization;

namespace LunarROMCorruptor.Modules.CorruptionInternals.Engines
{
    /// <summary>
    /// Vector2 Engine: looks for 32-bit IEEE floats (positions, velocities, scales, colours...) and corrupts them.
    /// Only values that look like "real" floats are touched (finite, inside the min/max magnitude), which keeps
    /// random bytes, code and zero padding from being treated as floats. With <see cref="Settings.SkipCode"/> the words around a
    /// candidate are checked too, because a lot of MIPS code (lui/mtc1/lwc1/FPU instructions) and compressed data also decode as plausible floats.
    /// Changes are written byte by byte, so the normal stash format still works.
    /// </summary>
    public class VectorEngine
    {
        public enum VectorMode
        {
            REPLACE, //Replace with a value from the value list
            NUDGE, //Add/subtract a random percentage
            NEGATE, //Flip the sign
            RANDOM, //Random value between -max and +max
            SWAPXY //Swap X and Y (Vector2 pairs only)
        }

        /// <summary>
        /// How a 32-bit float is stored in the file.
        /// </summary>
        private enum FloatOrder
        {
            Little,
            Big,
            ByteSwapped16 //Big-endian data stored with every 16-bit word swapped (.v64 N64 dumps)
        }

        public sealed class Settings
        {
            public VectorMode Mode = VectorMode.NUDGE;
            public float[] Values = [-1f, -0.5f, 0f, 0.5f, 1f, 2f, 10f, 100f];
            public double NudgePercent = 25;
            public double MinMagnitude = 0.0001;
            public double MaxMagnitude = 100000;
            public bool Vector2Pairs = true;
            public bool Aligned = true;
            public bool BigEndian;
            public bool IncludeZero;
            public bool SkipCode = true; //Only corrupt floats that sit among other float-like data (skips code and compressed data)

            public static float[] ParseValues(string text)
            {
                var list = new List<float>();
                foreach (var part in text.Split([',', ';', ' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries))
                {
                    if (float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) && float.IsFinite(v))
                        list.Add(v);
                }
                return [.. list];
            }
        }

        private const int MaxRandomTries = 256;
        private const int NeighbourWords = 8; //Words checked on each side of a candidate by the code/compressed data filter

        public static void Run(CorruptionContext ctx, Settings s)
        {
            int elementSize = s.Vector2Pairs ? 8 : 4;
            int alignment = s.Aligned ? 4 : 1;
            if (s.Mode == VectorMode.REPLACE && s.Values.Length == 0)
                throw new InvalidOperationException("The Vector2 Engine value list is empty. Enter at least one number (e.g. -1, 0, 1, 100).");
            if (s.Mode == VectorMode.SWAPXY && !s.Vector2Pairs)
                throw new InvalidOperationException("SWAPXY needs 'Treat as Vector2 pairs' to be enabled.");

            var order = s.BigEndian ? FloatOrder.Big : FloatOrder.Little;
            long start = ctx.Start;
            var n64 = ctx.Data != null ? N64Rom.DetectByteOrder(ctx.Data) : null;
            if (n64 != null)
            {
                //N64 data is big-endian. Reading a .z64 as little-endian turns code and random data into "floats", so the byte order is taken from the header.
                var detected = n64 switch
                {
                    N64Rom.ByteOrder.ByteSwapped => FloatOrder.ByteSwapped16,
                    N64Rom.ByteOrder.LittleEndian => FloatOrder.Little,
                    _ => FloatOrder.Big
                };
                if (detected != order)
                    TraceLogger.Log($"Vector2 Engine: N64 ROM detected ({n64}), reading floats as {detected} instead of {order}.", StatusSeverityType.Warning);
                order = detected;
                if (start < N64Rom.BootCodeEnd)
                {
                    TraceLogger.Log("Vector2 Engine: skipping the N64 header and IPL3 boot code (0x0 - 0xFFF).");
                    start = N64Rom.BootCodeEnd;
                }
            }
            if (order == FloatOrder.ByteSwapped16) alignment = Math.Max(alignment, 2); //16-bit swapped words must start on an even offset

            long first = AlignUp(start, alignment);
            long last = ctx.End - elementSize + 1; //Last position where a whole element still fits in the range
            if (last < first)
            {
                TraceLogger.Log("Vector2 Engine: the selected range is too small to contain a float.", StatusSeverityType.Warning, true);
                return;
            }

            long corrupted = 0, skipped = 0;
            if (ctx.EveryNth)
            {
                //Every Nth float/vector instead of every Nth byte
                long step = ctx.Intensity * (s.Aligned ? elementSize : 1);
                long total = (last - first) / step + 1;
                long done = 0;
                for (long p = first; p <= last; p += step)
                {
                    if (TryCorrupt(ctx, s, order, p)) corrupted++; else skipped++;
                    if (++done % 65536 == 0) ctx.Tick(done, total);
                }
            }
            else
            {
                long positions = (last - first) / alignment + 1;
                for (long n = 0; n < ctx.Intensity; n++)
                {
                    bool done = false;
                    for (int attempt = 0; attempt < MaxRandomTries && !done; attempt++)
                    {
                        long p = first + ctx.Rnd.NextInt64(0, positions) * alignment;
                        done = TryCorrupt(ctx, s, order, p);
                    }
                    if (done) corrupted++; else skipped++;
                    if ((n + 1) % 4096 == 0) ctx.Tick(n + 1, ctx.Intensity);
                }
            }
            TraceLogger.Log($"Vector2 Engine corrupted {corrupted} float value(s). {skipped} attempt(s) didn't find a float that matched the limits.");
            if (corrupted == 0)
                TraceLogger.Log("Vector2 Engine didn't find any floats within the min/max limits in the selected range. Try a wider range, disabling alignment, switching endianness or disabling 'Skip code / compressed data'.", StatusSeverityType.Warning, true);
        }

        private static long AlignUp(long value, int alignment) => alignment <= 1 ? value : (value + alignment - 1) / alignment * alignment;

        private static bool IsPlausible(float f, Settings s)
        {
            if (!float.IsFinite(f) || float.IsSubnormal(f)) return false;
            if (f == 0f) return s.IncludeZero;
            double magnitude = Math.Abs(f);
            return magnitude >= s.MinMagnitude && magnitude <= s.MaxMagnitude;
        }

        /// <summary>
        /// Index of the k-th byte of a big/little-endian value inside the file (16-bit swapped data has every pair of bytes swapped).
        /// </summary>
        private static int ByteIndex(int k, FloatOrder order) => order == FloatOrder.ByteSwapped16 ? k ^ 1 : k;

        private static uint ReadWord(CorruptionContext ctx, long p, FloatOrder order)
        {
            Span<byte> b = stackalloc byte[4];
            for (int k = 0; k < 4; k++) b[k] = ctx.Get(p + ByteIndex(k, order));
            return order == FloatOrder.Little ? BinaryPrimitives.ReadUInt32LittleEndian(b) : BinaryPrimitives.ReadUInt32BigEndian(b);
        }

        private static float ReadFloat(CorruptionContext ctx, long p, FloatOrder order) => BitConverter.UInt32BitsToSingle(ReadWord(ctx, p, order));

        private static void WriteFloat(CorruptionContext ctx, long p, float value, FloatOrder order)
        {
            Span<byte> b = stackalloc byte[4];
            if (order == FloatOrder.Little) BinaryPrimitives.WriteSingleLittleEndian(b, value);
            else BinaryPrimitives.WriteSingleBigEndian(b, value);
            for (int k = 0; k < 4; k++)
            {
                long i = p + ByteIndex(k, order);
                if (ctx.Get(i) != b[k])
                    ctx.Set(i, b[k]);
            }
        }

        private static bool TryCorrupt(CorruptionContext ctx, Settings s, FloatOrder order, long p)
        {
            float x = ReadFloat(ctx, p, order);
            if (!IsPlausible(x, s)) return false;

            if (!s.Vector2Pairs)
            {
                if (s.SkipCode && !LooksLikeFloatData(ctx, p, 1, order)) return false;
                WriteFloat(ctx, p, Corrupt(ctx.Rnd, s, x), order);
                return true;
            }

            float y = ReadFloat(ctx, p + 4, order);
            if (!IsPlausible(y, s)) return false;
            if (s.SkipCode && !LooksLikeFloatData(ctx, p, 2, order)) return false;
            if (s.Mode == VectorMode.SWAPXY)
            {
                WriteFloat(ctx, p, y, order);
                WriteFloat(ctx, p + 4, x, order);
            }
            else
            {
                WriteFloat(ctx, p, Corrupt(ctx.Rnd, s, x), order);
                WriteFloat(ctx, p + 4, Corrupt(ctx.Rnd, s, y), order);
            }
            return true;
        }

        /// <summary>
        /// Checks the words around a candidate float. Real float data (tables, positions, structs) sits among other floats, zeros and small
        /// ints. Code and compressed data only decode as floats now and then, and code contains instruction patterns that are never sensible floats.
        /// </summary>
        private static bool LooksLikeFloatData(CorruptionContext ctx, long p, int elementWords, FloatOrder order)
        {
            int words = 0, floatLike = 0, dataLike = 0, code = 0;
            for (int w = -NeighbourWords; w < NeighbourWords + elementWords; w++)
            {
                if (w >= 0 && w < elementWords) continue; //The candidate itself
                long q = p + w * 4L;
                if (q < 0 || q + 3 >= ctx.Length) continue;
                uint word = ReadWord(ctx, q, order);
                words++;
                if (word == 0 || IsLooseFloat(word))
                {
                    floatLike++;
                    dataLike++;
                }
                else if (word <= 0xFFFF || word >= 0xFFFF0000 || (word & 0xFF000000) == 0x80000000)
                {
                    dataLike++; //Small (signed) ints and RAM pointers (0x80xxxxxx on MIPS consoles) are common in structs next to floats
                }
                if (LooksLikeMipsCode(word) && ++code >= 2) return false;
            }
            if (words == 0) return true;
            return floatLike * 10 >= words * 4 && dataLike * 10 >= words * 6;
        }

        private static bool IsLooseFloat(uint word)
        {
            float f = BitConverter.UInt32BitsToSingle(word);
            if (!float.IsFinite(f) || float.IsSubnormal(f)) return false;
            float m = Math.Abs(f);
            return m >= 1e-6f && m <= 1e7f;
        }

        /// <summary>
        /// MIPS instructions (N64/PS1/PS2/PSP) that are very common in code and that are never sensible floats (all of them decode as tiny
        /// numbers), so seeing a couple of them around a candidate means it's in code.
        /// </summary>
        private static bool LooksLikeMipsCode(uint w)
        {
            if (w == 0) return false; //nop / padding
            if (w == 0x03E00008) return true; //jr ra
            uint top16 = w & 0xFFFF0000;
            if (top16 == 0x27BD0000) return true; //addiu sp, sp, imm
            uint top11 = w & 0xFFE00000;
            if (top11 == 0xAFA00000 || top11 == 0x8FA00000) return true; //sw/lw reg, imm(sp)
            uint op = w >> 26;
            switch (op)
            {
                case 0x03: //jal
                case 0x04: //beq
                case 0x05: //bne
                case 0x09: //addiu
                    return true;
                case 0x00: //SPECIAL: register ALU ops with a real destination register
                    {
                        uint rd = (w >> 11) & 0x1F, shamt = (w >> 6) & 0x1F, funct = w & 0x3F, rs = (w >> 21) & 0x1F;
                        if ((w & 0xFC1FFFFF) == 0x00000008) return true; //jr reg
                        if (rd == 0) return false;
                        if (shamt == 0 && funct is 0x21 or 0x23 or 0x24 or 0x25 or 0x26 or 0x27 or 0x2A or 0x2B or 0x2D or 0x2F) return true; //addu/subu/and/or/xor/nor/slt/sltu/daddu/dsubu
                        if (rs == 0 && funct is 0x00 or 0x02 or 0x03 && shamt != 0) return true; //sll/srl/sra
                        return false;
                    }
                default:
                    return false;
            }
        }

        private static float Corrupt(Random rnd, Settings s, float value)
        {
            switch (s.Mode)
            {
                case VectorMode.REPLACE:
                    return s.Values[rnd.Next(s.Values.Length)];
                case VectorMode.NUDGE:
                    double amount = s.NudgePercent / 100.0 * (rnd.NextDouble() * 2 - 1);
                    double nudged = value == 0f ? amount : value * (1 + amount);
                    return (float)Math.Clamp(nudged, -float.MaxValue, float.MaxValue);
                case VectorMode.NEGATE:
                    return -value;
                case VectorMode.RANDOM:
                    return (float)((rnd.NextDouble() * 2 - 1) * Math.Min(s.MaxMagnitude, float.MaxValue));
                default:
                    return value;
            }
        }
    }
}
