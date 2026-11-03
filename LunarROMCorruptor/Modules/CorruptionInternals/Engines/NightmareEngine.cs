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


//Acknowledgements:
//This engine is from the WindowsGlitchHarvester, a ROM corruptor. This code is an attempt to get the same results as the original engine.

namespace LunarROMCorruptor.Modules.CorruptionInternals.Engines
{
    /// <summary>
    /// Nightmare+ Engine: the classic Nightmare Engine (RANDOM / RANDOMTILT / TILT) merged with the old Exclusion Engine.
    /// Exclusion type "None" is the original Nightmare Engine (same results). Any other exclusion type leaves the protected header/boot
    /// ranges of that platform (see <see cref="ExclusionEngine.Profiles"/>) out of the corruption, and "Auto" picks the type from the file.
    /// </summary>
    public class NightmareEngine
    {
        public const string NoExclusion = "None";
        public const string AutoExclusion = "Auto";

        public sealed class Settings
        {
            public CorruptionOptions Option = CorruptionOptions.RANDOM;
            public int TiltAmount = 1; //Increment/Decrement number
            public string ExclusionProfile = NoExclusion; //"None", "Auto" or a key of ExclusionEngine.Profiles
            public bool AvoidCrashValues; //RANDOM never writes 0x00 or 0xF0-0xFF (values that crash more often), the old Exclusion Engine behaviour
        }

        /// <summary>
        /// The exclusion types shown in the UI: None, Auto and every platform profile.
        /// </summary>
        public static IEnumerable<string> ExclusionTypes => new[] { NoExclusion, AutoExclusion }.Concat(ExclusionEngine.Profiles.Keys);

        public static void Run(CorruptionContext ctx, Settings s)
        {
            string profile = s.ExclusionProfile;
            if (profile.Equals(AutoExclusion, StringComparison.OrdinalIgnoreCase))
            {
                profile = ctx.Data != null ? ExclusionEngine.DetectProfile(ctx.Data.FilePath, ctx.Data) ?? NoExclusion : NoExclusion;
                TraceLogger.Log($"Nightmare+ Engine: exclusion type Auto picked {(profile == NoExclusion ? "None (the file type wasn't recognised)" : profile)}.");
            }
            if (string.IsNullOrWhiteSpace(profile) || profile.Equals(NoExclusion, StringComparison.OrdinalIgnoreCase))
            {
                //Classic Nightmare Engine
                EngineProcessor.Drive(ctx, i => CorruptByte(ctx, s, i));
                return;
            }
            if (!ExclusionEngine.Profiles.ContainsKey(profile))
                throw new InvalidOperationException($"Unknown Nightmare+ exclusion type '{profile}'.");
            RunExcluded(ctx, s, profile);
        }

        /// <summary>
        /// Exclusion mode. Instead of picking random addresses and retrying when they're protected, the allowed parts of the selected range
        /// are calculated once, so every corruption lands on an allowed byte and there's no retry limit.
        /// </summary>
        private static void RunExcluded(CorruptionContext ctx, Settings s, string profile)
        {
            var ranges = ExclusionEngine.GetProtectedRanges(profile, ctx.Data);
            TraceLogger.Log($"Nightmare+ exclusion type {profile}: {ranges.Count} protected range(s).\n{string.Join("\n", ranges)}");
            if (ctx.Excluded != null)
            {
                //Bisect / EXCLUDE stash ranges are left out of the allowed area too, so every corruption still lands on an allowed byte
                ranges.AddRange(ctx.Excluded.Select(x => new ProtectedRange(x.Start, x.End, "Excluded (Bisect / stash EXCLUDE)")));
            }
            ranges.Sort((a, b) => a.Start.CompareTo(b.Start));

            //Build the allowed intervals = [Start, End] minus the protected ranges.
            var allowed = new List<(long Start, long End)>();
            long cursor = ctx.Start;
            foreach (var range in ranges)
            {
                if (range.End < cursor) continue;
                if (range.Start > ctx.End) break;
                if (range.Start > cursor) allowed.Add((cursor, Math.Min(range.Start - 1, ctx.End)));
                cursor = Math.Max(cursor, range.End + 1);
                if (cursor > ctx.End) break;
            }
            if (cursor <= ctx.End) allowed.Add((cursor, ctx.End));

            long totalAllowed = allowed.Sum(a => a.End - a.Start + 1);
            if (totalAllowed == 0)
            {
                TraceLogger.Log($"The selected byte range is completely protected by the {profile} exclusion type. Nothing was corrupted.", StatusSeverityType.Warning, true);
                return;
            }

            if (ctx.EveryNth)
            {
                long step = ctx.Intensity;
                long total = (ctx.End - ctx.Start) / step + 1;
                long done = 0;
                for (long i = ctx.Start; i <= ctx.End; i += step)
                {
                    if (!ExclusionEngine.IsProtected(i, ranges))
                        CorruptByte(ctx, s, i);
                    if (++done % 65536 == 0) ctx.Tick(done, total);
                }
                ctx.Tick(total, total);
            }
            else
            {
                //Prefix sums so a random number in [0, totalAllowed) maps to an allowed address
                var prefix = new long[allowed.Count];
                long running = 0;
                for (int k = 0; k < allowed.Count; k++)
                {
                    prefix[k] = running;
                    running += allowed[k].End - allowed[k].Start + 1;
                }
                for (long n = 0; n < ctx.Intensity; n++)
                {
                    long pick = ctx.Rnd.NextInt64(0, totalAllowed);
                    int idx = Array.BinarySearch(prefix, pick);
                    if (idx < 0) idx = ~idx - 1;
                    CorruptByte(ctx, s, allowed[idx].Start + (pick - prefix[idx]));
                    if ((n + 1) % 65536 == 0) ctx.Tick(n + 1, ctx.Intensity);
                }
                ctx.Tick(ctx.Intensity, ctx.Intensity);
            }
        }

        private static byte NewRandom(CorruptionContext ctx, Settings s)
        {
            byte value = (byte)ctx.Rnd.Next(0, 256);
            if (s.AvoidCrashValues && ShouldAvoidValue(value))
                value = AlternativeValues[ctx.Rnd.Next(AlternativeValues.Length)];
            return value;
        }

        public static void CorruptByte(CorruptionContext ctx, Settings s, long i)
        {
            switch (s.Option)
            {
                case CorruptionOptions.RANDOM:
                    ctx.Set(i, NewRandom(ctx, s));
                    break;

                case CorruptionOptions.RANDOMTILT:
                    switch (ctx.Rnd.Next(0, 3))
                    {
                        case 0:
                            ctx.Set(i, NewRandom(ctx, s));
                            break;
                        case 1:
                            ctx.Set(i, CorruptionCore.ClampByte(ctx.Get(i) + s.TiltAmount));
                            break;
                        default:
                            ctx.Set(i, CorruptionCore.ClampByte(ctx.Get(i) - s.TiltAmount));
                            break;
                    }
                    break;

                case CorruptionOptions.TILT:
                    if (ctx.Rnd.Next(0, 2) == 0)
                        ctx.Set(i, CorruptionCore.ClampByte(ctx.Get(i) + s.TiltAmount));
                    else
                        ctx.Set(i, CorruptionCore.ClampByte(ctx.Get(i) - s.TiltAmount));
                    break;

                default:
                    throw new InvalidOperationException($"The Nightmare+ Engine doesn't support the corruption type {s.Option}.");
            }
        }

        /// <summary>
        /// 0x00 and 0xF0-0xFF have a high crash potential (NOPs/BRK, branch/return opcodes, "end of list" markers).
        /// </summary>
        private static bool ShouldAvoidValue(byte value) => value == 0x00 || value >= 0xF0;

        private static readonly byte[] AlternativeValues =
        [
            0x7F, 0x80, 0x42, 0x69, 0xA5, 0xCD, 0x13, 0x88,
            0x2A, 0x76, 0x65, 0x8C
        ];
    }
}
