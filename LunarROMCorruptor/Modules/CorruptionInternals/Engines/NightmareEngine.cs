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
    public class NightmareEngine
    {
        public sealed class Settings
        {
            public CorruptionOptions Option = CorruptionOptions.RANDOM;
            public int TiltAmount = 1; //Increment/Decrement number
        }

        public static void CorruptByte(CorruptionContext ctx, Settings s, long i)
        {
            switch (s.Option)
            {
                case CorruptionOptions.RANDOM:
                    ctx.Set(i, (byte)ctx.Rnd.Next(0, 256));
                    break;

                case CorruptionOptions.RANDOMTILT:
                    switch (ctx.Rnd.Next(0, 3))
                    {
                        case 0:
                            ctx.Set(i, (byte)ctx.Rnd.Next(0, 256));
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
                    throw new InvalidOperationException($"The Nightmare Engine doesn't support the corruption type {s.Option}.");
            }
        }
    }
}
