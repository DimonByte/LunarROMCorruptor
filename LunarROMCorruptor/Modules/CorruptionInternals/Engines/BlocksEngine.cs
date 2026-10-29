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


namespace LunarROMCorruptor.Modules.CorruptionInternals.Engines
{
    /// <summary>
    /// Blocks Engine: works on whole blocks of data instead of single bytes, like the "data moshing" / "pixel sorting" techniques used in glitch art.
    /// A block is split into elements (1 byte, or e.g. 3 bytes for RGB pixels / 2 bytes for 16-bit audio) and the elements are
    /// sorted, reversed, shuffled, rotated, smeared or echoed. Only bytes that actually change are written (and recorded in the stash).
    /// Intensity mode corrupts N random blocks, Every Nth mode corrupts every Nth block starting at the start byte.
    /// </summary>
    public class BlocksEngine
    {
        public enum BlockMode
        {
            RANDOM, //A random mode per block
            SORT, //Sort elements by value (pixel sorting)
            REVERSE, //Reverse the order of the elements
            SHUFFLE, //Shuffle the elements
            ROTATE, //Rotate the elements by a random amount
            SMEAR, //Repeat one element across the whole block
            ECHO //Mix the block with the data <EchoDistance> bytes earlier (delay/echo effect)
        }

        public sealed class Settings
        {
            public BlockMode Mode = BlockMode.SORT;
            public int BlockSize = 256; //Bytes
            public int ElementSize = 1; //Bytes per element (3 = RGB pixel, 2 = 16-bit sample, 4 = 32-bit value)
            public long EchoDistance = 4096; //Bytes
            public bool Descending;
        }

        public const int MaxBlockSize = 16 * 1024 * 1024;
        private static readonly BlockMode[] RandomModes = [BlockMode.SORT, BlockMode.REVERSE, BlockMode.SHUFFLE, BlockMode.ROTATE, BlockMode.SMEAR, BlockMode.ECHO];

        public static void Run(CorruptionContext ctx, Settings s)
        {
            long rangeLength = ctx.End - ctx.Start + 1;
            int element = Math.Clamp(s.ElementSize, 1, 64);
            long block = Math.Clamp(s.BlockSize, element * 2L, MaxBlockSize);
            block = Math.Min(block, rangeLength) / element * element; //Whole elements that fit in the range
            if (block < element * 2L)
            {
                TraceLogger.Log("Blocks Engine: the selected byte range is too small for the block/element size.", StatusSeverityType.Warning, true);
                return;
            }
            var buffer = new byte[block];
            var scratch = new byte[block];
            long processed = 0;

            if (ctx.EveryNth)
            {
                long step = block * ctx.Intensity;
                long total = (rangeLength - block) / step + 1;
                for (long pos = ctx.Start; pos + block - 1 <= ctx.End; pos += step)
                {
                    ProcessBlock(ctx, s, pos, (int)block, element, buffer, scratch);
                    ctx.Tick(++processed, total);
                }
            }
            else
            {
                long positions = (rangeLength - block) / element + 1; //Block starts stay aligned to the element size
                for (long n = 0; n < ctx.Intensity; n++)
                {
                    long pos = ctx.Start + ctx.Rnd.NextInt64(0, positions) * element;
                    ProcessBlock(ctx, s, pos, (int)block, element, buffer, scratch);
                    ctx.Tick(++processed, ctx.Intensity);
                }
            }
            TraceLogger.Log($"Blocks Engine processed {processed} block(s) of {block} bytes ({block / element} elements of {element} byte(s)).");
        }

        private static void ProcessBlock(CorruptionContext ctx, Settings s, long pos, int length, int element, byte[] buffer, byte[] scratch)
        {
            for (int k = 0; k < length; k++)
                buffer[k] = ctx.Get(pos + k);
            Array.Copy(buffer, scratch, length);

            var mode = s.Mode == BlockMode.RANDOM ? RandomModes[ctx.Rnd.Next(RandomModes.Length)] : s.Mode;
            int count = length / element;
            switch (mode)
            {
                case BlockMode.SORT:
                    {
                        var order = new int[count];
                        var keys = new int[count];
                        for (int e = 0; e < count; e++)
                        {
                            order[e] = e;
                            int key = 0;
                            for (int b = 0; b < element; b++) key += scratch[e * element + b]; //Brightness-like key for multi byte elements
                            keys[e] = s.Descending ? -key : key;
                        }
                        Array.Sort(keys, order); //Sorts the element order by key
                        for (int e = 0; e < count; e++)
                            Buffer.BlockCopy(scratch, order[e] * element, buffer, e * element, element);
                        break;
                    }
                case BlockMode.REVERSE:
                    for (int e = 0; e < count; e++)
                        Buffer.BlockCopy(scratch, (count - 1 - e) * element, buffer, e * element, element);
                    break;
                case BlockMode.SHUFFLE:
                    {
                        var order = Enumerable.Range(0, count).ToArray();
                        for (int e = count - 1; e > 0; e--)
                        {
                            int j = ctx.Rnd.Next(e + 1);
                            (order[e], order[j]) = (order[j], order[e]);
                        }
                        for (int e = 0; e < count; e++)
                            Buffer.BlockCopy(scratch, order[e] * element, buffer, e * element, element);
                        break;
                    }
                case BlockMode.ROTATE:
                    {
                        int shift = ctx.Rnd.Next(1, count);
                        for (int e = 0; e < count; e++)
                            Buffer.BlockCopy(scratch, (e + shift) % count * element, buffer, e * element, element);
                        break;
                    }
                case BlockMode.SMEAR:
                    {
                        int source = ctx.Rnd.Next(count);
                        for (int e = 0; e < count; e++)
                            Buffer.BlockCopy(scratch, source * element, buffer, e * element, element);
                        break;
                    }
                case BlockMode.ECHO:
                    {
                        long distance = Math.Max(1, s.EchoDistance);
                        for (int k = 0; k < length; k++)
                        {
                            long src = pos + k - distance;
                            if (src < 0) continue;
                            buffer[k] = (byte)((scratch[k] + ctx.Get(src)) / 2);
                        }
                        break;
                    }
            }

            for (int k = 0; k < length; k++)
            {
                if (buffer[k] != scratch[k])
                    ctx.Set(pos + k, buffer[k]);
            }
        }
    }
}
