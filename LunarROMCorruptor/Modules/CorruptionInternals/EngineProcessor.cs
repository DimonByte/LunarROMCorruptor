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

namespace LunarROMCorruptor.Modules.CorruptionInternals
{
    /// <summary>
    /// A snapshot of every setting needed for a corruption. It's created on the UI thread right before a corruption starts so the
    /// engines never touch the UI controls (this is what allows corruption to run on a background thread, and it's much faster).
    /// </summary>
    public sealed class CorruptionSettings
    {
        public CorruptionEngineType Engine;
        public long StartByte;
        public long EndByte;
        public bool EveryNth; //True = corrupt every Nth byte, False = intensity (random) mode
        public long Intensity; //Step size in EveryNth mode, number of corruptions in intensity mode

        public NightmareEngine.Settings? Nightmare;
        public LerpEngine.Settings? Lerp;
        public LogicEngine.Settings? Logic;
        public MergeEngine.Settings? Merge;
        public ManualEngine.Settings? Manual;
        public ExclusionEngine.Settings? Exclusion;
        public VectorEngine.Settings? Vector;
        public SourceCodeEngine.Settings? SourceCode;

        /// <summary>
        /// Returns a copy with the byte range clamped to a file of the given length (used by the corruption queue where every file has a different size).
        /// </summary>
        public CorruptionSettings ClampTo(long length)
        {
            var copy = (CorruptionSettings)MemberwiseClone();
            copy.EndByte = Math.Min(EndByte, length - 1);
            copy.StartByte = Math.Min(StartByte, copy.EndByte);
            return copy;
        }
    }

    /// <summary>
    /// Everything an engine needs while it's running: the file data, the byte range, the random generator, the stash and cancellation/progress.
    /// </summary>
    public sealed class CorruptionContext
    {
        public readonly FileBuffer? Data;
        public readonly long Start;
        public readonly long End;
        public readonly bool EveryNth;
        public readonly long Intensity;
        public readonly Random Rnd;
        public readonly StashLog? Stash;
        public readonly CancellationToken Token;
        public readonly IProgress<double>? Progress;

        public CorruptionContext(FileBuffer? data, CorruptionSettings settings, Random rnd, StashLog? stash, CancellationToken token, IProgress<double>? progress)
        {
            Data = data;
            long max = data != null ? data.Length - 1 : long.MaxValue;
            End = Math.Clamp(settings.EndByte, 0, Math.Max(0, max));
            Start = Math.Clamp(settings.StartByte, 0, End);
            EveryNth = settings.EveryNth;
            Intensity = Math.Max(1, settings.Intensity);
            Rnd = rnd;
            Stash = stash;
            Token = token;
            Progress = progress;
        }

        public long Length => Data!.Length;

        public byte Get(long i) => Data![i];

        /// <summary>
        /// Sets a byte and records it in the stash.
        /// </summary>
        public void Set(long i, byte value)
        {
            Data![i] = value;
            Stash?.Add(i, value);
        }

        public bool InFile(long i) => i >= 0 && i < Data!.Length;

        /// <summary>
        /// Random index between Start and End (both inclusive).
        /// </summary>
        public long RandomIndex() => Rnd.NextInt64(Start, End + 1);

        /// <summary>
        /// Call regularly from long loops: checks for cancellation and reports progress (0 to 1).
        /// </summary>
        public void Tick(long done, long total)
        {
            Token.ThrowIfCancellationRequested();
            if (total > 0)
                Progress?.Report(Math.Min(1.0, (double)done / total));
        }
    }

    internal class EngineProcessor
    {
        private const long TickInterval = 1 << 16;

        /// <summary>
        /// Runs the selected byte engine over the context.
        /// </summary>
        public static void Run(CorruptionSettings settings, CorruptionContext ctx)
        {
            switch (settings.Engine)
            {
                case CorruptionEngineType.NightmareEngine:
                    var nightmare = settings.Nightmare ?? throw new InvalidOperationException("Nightmare Engine settings missing.");
                    Drive(ctx, i => NightmareEngine.CorruptByte(ctx, nightmare, i));
                    break;
                case CorruptionEngineType.LerpEngine:
                    var lerp = settings.Lerp ?? throw new InvalidOperationException("Lerp Engine settings missing.");
                    Drive(ctx, i => LerpEngine.CorruptByte(ctx, lerp, i));
                    break;
                case CorruptionEngineType.LogicEngine:
                    var logic = settings.Logic ?? throw new InvalidOperationException("Logic Engine settings missing.");
                    Drive(ctx, i => LogicEngine.CorruptByte(ctx, logic, i));
                    break;
                case CorruptionEngineType.ManualEngine:
                    var manual = settings.Manual ?? throw new InvalidOperationException("Manual Engine settings missing.");
                    Drive(ctx, i => ManualEngine.CorruptByte(ctx, manual, i));
                    break;
                case CorruptionEngineType.MergeEngine:
                    var merge = settings.Merge ?? throw new InvalidOperationException("Merge Engine settings missing.");
                    using (var mergeData = MergeEngine.OpenMergeFile(merge))
                    {
                        Drive(ctx, i => MergeEngine.CorruptByte(ctx, merge, mergeData, i));
                    }
                    break;
                case CorruptionEngineType.ExclusionEngine:
                    ExclusionEngine.Run(ctx, settings.Exclusion ?? throw new InvalidOperationException("Exclusion Engine settings missing."));
                    break;
                case CorruptionEngineType.VectorEngine:
                    VectorEngine.Run(ctx, settings.Vector ?? throw new InvalidOperationException("Vector2 Engine settings missing."));
                    break;
                case CorruptionEngineType.SourceCodeEngine:
                    throw new InvalidOperationException("The Source Code Engine is a text engine and can't be run by the byte engine processor.");
                default:
                    throw new InvalidOperationException($"Corruption engine not found ({settings.Engine}). Corruption has been aborted to prevent unwanted corruption.");
            }
        }

        /// <summary>
        /// The standard byte selection loop used by most engines:
        /// Every Nth byte mode walks from Start to End (inclusive) with a step of Intensity.
        /// Intensity mode picks Intensity random addresses between Start and End (inclusive).
        /// </summary>
        public static void Drive(CorruptionContext ctx, Action<long> corrupt)
        {
            if (ctx.EveryNth)
            {
                long step = ctx.Intensity;
                long total = (ctx.End - ctx.Start) / step + 1;
                long done = 0;
                for (long i = ctx.Start; i <= ctx.End; i += step)
                {
                    corrupt(i);
                    if (++done % TickInterval == 0)
                        ctx.Tick(done, total);
                    if (i > long.MaxValue - step) break;
                }
                ctx.Tick(total, total);
            }
            else
            {
                long total = ctx.Intensity;
                for (long n = 0; n < total; n++)
                {
                    corrupt(ctx.RandomIndex());
                    if ((n + 1) % TickInterval == 0)
                        ctx.Tick(n + 1, total);
                }
                ctx.Tick(total, total);
            }
        }
    }
}
