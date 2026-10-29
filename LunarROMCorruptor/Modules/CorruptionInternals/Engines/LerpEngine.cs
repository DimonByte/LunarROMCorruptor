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
    public class LerpEngine
    {
        public sealed class Settings
        {
            public double InterpolateValue = 0.5; //Clamped between 0.0 and 1.0
        }

        private static double LinearInterpolationCalculation(double v0, double v1, double t)
        {
            return Math.Round(Math.Abs(v0 + (t * (v1 - v0))));
        }

        public static void CorruptByte(CorruptionContext ctx, Settings s, long i)
        {
            double t = Math.Clamp(s.InterpolateValue, 0.0, 1.0);
            //Use the neighbouring bytes, falling back to the byte itself at the start/end of the file.
            byte next = ctx.InFile(i + 1) ? ctx.Get(i + 1) : ctx.Get(i);
            byte previous = ctx.InFile(i - 1) ? ctx.Get(i - 1) : ctx.Get(i);
            ctx.Set(i, CorruptionCore.ClampByte((long)LinearInterpolationCalculation(next, previous, t)));
        }
    }
}
