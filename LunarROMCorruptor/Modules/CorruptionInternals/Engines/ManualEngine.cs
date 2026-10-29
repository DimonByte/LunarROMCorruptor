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


using System.Numerics;

namespace LunarROMCorruptor.Modules.CorruptionInternals.Engines
{
    public class ManualEngine
    {
        public enum MathOperation { Multiply, Divide, Power }

        public sealed class Settings
        {
            public bool Increment; public int IncrementValue;
            public bool Shift; public long ShiftValue;
            public bool MakeEqual; public byte EqualValue;
            public bool Replace; public byte ReplaceFrom; public byte ReplaceTo;
            public bool PasteRandom;
            public bool RepeatRandom; public int RepeatCount;
            public bool MathEnabled; public MathOperation Operation; public long MathValue = 1;
        }

        public static void CorruptByte(CorruptionContext ctx, Settings s, long i)
        {
            if (s.Increment)
            {
                ctx.Set(i, CorruptionCore.ClampByte(ctx.Get(i) + s.IncrementValue));
            }
            if (s.Shift)
            {
                //Copies this byte to <shift> bytes ahead, as long as the destination is still inside the selected range.
                long j = i + s.ShiftValue;
                if (j >= ctx.Start && j <= ctx.End)
                {
                    ctx.Set(j, ctx.Get(i));
                }
            }
            if (s.MakeEqual)
            {
                ctx.Set(i, s.EqualValue);
            }
            if (s.Replace)
            {
                if (ctx.Get(i) == s.ReplaceFrom)
                {
                    ctx.Set(i, s.ReplaceTo);
                }
            }
            if (s.PasteRandom)
            {
                ctx.Set(i, ctx.Get(ctx.RandomIndex()));
            }
            if (s.RepeatRandom && s.RepeatCount > 0)
            {
                //Copies <repeat> bytes starting at this byte to random locations in the range.
                int count = (int)Math.Min(s.RepeatCount, ctx.Length - i);
                Span<byte> copied = count <= 4096 ? stackalloc byte[count] : new byte[count];
                for (int n = 0; n < count; n++)
                    copied[n] = ctx.Get(i + n);
                foreach (var b in copied)
                {
                    ctx.Set(ctx.RandomIndex(), b);
                }
            }
            if (s.MathEnabled)
            {
                long value = Math.Max(1, s.MathValue);
                switch (s.Operation)
                {
                    case MathOperation.Multiply:
                        ctx.Set(i, CorruptionCore.ClampByte(Math.Min((long)ctx.Get(i) * value, 255)));
                        break;
                    case MathOperation.Divide:
                        ctx.Set(i, CorruptionCore.ClampByte(ctx.Get(i) / value));
                        break;
                    case MathOperation.Power:
                        //byte ^ value, wrapped to a byte (mod 256). The old code cast a huge double to a byte which gave garbage.
                        ctx.Set(i, (byte)BigInteger.ModPow(ctx.Get(i), value, 256));
                        break;
                }
            }
        }
    }
}
