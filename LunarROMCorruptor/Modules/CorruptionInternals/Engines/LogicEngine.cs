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
    public class LogicEngine
    {
        public sealed class Settings
        {
            public CorruptionOptions Option = CorruptionOptions.XOR;
            public int Value = 1; //Bitwise value (0-255)
            public bool RandomizeType;
            public bool RandomizeValue;
            public int ValueMin = 0;
            public int ValueMax = 255;
        }

        private static readonly CorruptionOptions[] AllOperations =
        [
            CorruptionOptions.AND, CorruptionOptions.OR, CorruptionOptions.XOR, CorruptionOptions.NOT,
            CorruptionOptions.NAND, CorruptionOptions.NOR, CorruptionOptions.SWAP, CorruptionOptions.SHIFT
        ];

        public static void CorruptByte(CorruptionContext ctx, Settings s, long i)
        {
            //Randomizing is done per byte on local values instead of changing the UI controls for every byte.
            CorruptionOptions option = s.RandomizeType ? AllOperations[ctx.Rnd.Next(AllOperations.Length)] : s.Option;
            int value = s.RandomizeValue ? ctx.Rnd.Next(Math.Min(s.ValueMin, s.ValueMax), Math.Max(s.ValueMin, s.ValueMax) + 1) : s.Value;
            byte bitVal = (byte)Math.Clamp(value, 0, 255);
            byte current = ctx.Get(i);

            switch (option)
            {
                case CorruptionOptions.AND:
                    ctx.Set(i, (byte)(current & bitVal));
                    break;
                case CorruptionOptions.OR:
                    ctx.Set(i, (byte)(current | bitVal));
                    break;
                case CorruptionOptions.XOR:
                    ctx.Set(i, (byte)(current ^ bitVal));
                    break;
                case CorruptionOptions.NOT:
                    ctx.Set(i, (byte)~current);
                    break;
                case CorruptionOptions.NAND:
                    ctx.Set(i, (byte)~(current & bitVal));
                    break;
                case CorruptionOptions.NOR:
                    ctx.Set(i, (byte)~(current | bitVal));
                    break;
                case CorruptionOptions.SWAP:
                    //Swaps this byte with the next byte. Skipped at the end of the file.
                    if (!ctx.InFile(i + 1)) break;
                    byte next = ctx.Get(i + 1);
                    ctx.Set(i, next);
                    ctx.Set(i + 1, current);
                    break;
                case CorruptionOptions.SHIFT:
                    //Copies the byte that is <value> bytes ahead into this byte. Skipped if that would go past the end of the file.
                    if (!ctx.InFile(i + bitVal)) break;
                    ctx.Set(i, ctx.Get(i + bitVal));
                    break;
                default:
                    throw new InvalidOperationException($"The Logic Engine doesn't support the operation {option}.");
            }
        }
    }
}
