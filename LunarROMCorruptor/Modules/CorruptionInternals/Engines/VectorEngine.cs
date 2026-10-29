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
    /// random bytes, code and zero padding from being treated as floats.
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

        public static void Run(CorruptionContext ctx, Settings s)
        {
            int elementSize = s.Vector2Pairs ? 8 : 4;
            int alignment = s.Aligned ? 4 : 1;
            if (s.Mode == VectorMode.REPLACE && s.Values.Length == 0)
                throw new InvalidOperationException("The Vector2 Engine value list is empty. Enter at least one number (e.g. -1, 0, 1, 100).");
            if (s.Mode == VectorMode.SWAPXY && !s.Vector2Pairs)
                throw new InvalidOperationException("SWAPXY needs 'Treat as Vector2 pairs' to be enabled.");

            long first = AlignUp(ctx.Start, alignment);
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
                    if (TryCorrupt(ctx, s, p)) corrupted++; else skipped++;
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
                        done = TryCorrupt(ctx, s, p);
                    }
                    if (done) corrupted++; else skipped++;
                    if ((n + 1) % 4096 == 0) ctx.Tick(n + 1, ctx.Intensity);
                }
            }
            TraceLogger.Log($"Vector2 Engine corrupted {corrupted} float value(s). {skipped} attempt(s) didn't find a float that matched the limits.");
            if (corrupted == 0)
                TraceLogger.Log("Vector2 Engine didn't find any floats within the min/max limits in the selected range. Try a wider range, disabling alignment or switching endianness.", StatusSeverityType.Warning, true);
        }

        private static long AlignUp(long value, int alignment) => alignment <= 1 ? value : (value + alignment - 1) / alignment * alignment;

        private static bool IsPlausible(float f, Settings s)
        {
            if (!float.IsFinite(f) || float.IsSubnormal(f)) return false;
            if (f == 0f) return s.IncludeZero;
            double magnitude = Math.Abs(f);
            return magnitude >= s.MinMagnitude && magnitude <= s.MaxMagnitude;
        }

        private static float ReadFloat(CorruptionContext ctx, long p, bool bigEndian)
        {
            Span<byte> b = stackalloc byte[4];
            for (int k = 0; k < 4; k++) b[k] = ctx.Get(p + k);
            return bigEndian ? BinaryPrimitives.ReadSingleBigEndian(b) : BinaryPrimitives.ReadSingleLittleEndian(b);
        }

        private static void WriteFloat(CorruptionContext ctx, long p, float value, bool bigEndian)
        {
            Span<byte> b = stackalloc byte[4];
            if (bigEndian) BinaryPrimitives.WriteSingleBigEndian(b, value);
            else BinaryPrimitives.WriteSingleLittleEndian(b, value);
            for (int k = 0; k < 4; k++)
            {
                if (ctx.Get(p + k) != b[k])
                    ctx.Set(p + k, b[k]);
            }
        }

        private static bool TryCorrupt(CorruptionContext ctx, Settings s, long p)
        {
            float x = ReadFloat(ctx, p, s.BigEndian);
            if (!IsPlausible(x, s)) return false;

            if (!s.Vector2Pairs)
            {
                WriteFloat(ctx, p, Corrupt(ctx.Rnd, s, x), s.BigEndian);
                return true;
            }

            float y = ReadFloat(ctx, p + 4, s.BigEndian);
            if (!IsPlausible(y, s)) return false;
            if (s.Mode == VectorMode.SWAPXY)
            {
                WriteFloat(ctx, p, y, s.BigEndian);
                WriteFloat(ctx, p + 4, x, s.BigEndian);
            }
            else
            {
                WriteFloat(ctx, p, Corrupt(ctx.Rnd, s, x), s.BigEndian);
                WriteFloat(ctx, p + 4, Corrupt(ctx.Rnd, s, y), s.BigEndian);
            }
            return true;
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
