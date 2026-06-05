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
    internal class LerpEngine
    {
        static public double LinearInterpolationCalculation(double v0, double v1, double t)
        {
            return Math.Round(Math.Abs(v0 + (t * (v1 - v0))));
        }

        public static byte[] CorruptByte(byte[] ROM, long i)
        {
            byte byteminus;
            byte byteplus;
            double interpolateVal = Convert.ToDouble(Program.Form.LerpEngineFrame.LerpValueTxt.Text);
            //Check if the Bytes selected in i are in range.
            try
            {
                byteminus = ROM[i + 1];
            }
            catch (IndexOutOfRangeException)
            {
                byteminus = ROM[i];
            }
            try
            {
                byteplus = ROM[i - 1];
            }
            catch (IndexOutOfRangeException)
            {
                byteplus = ROM[i];
            }
            //Check if the interpolateVal is in range, cannot be higher than 1.0 and cannot be lower than 0.0
            if (interpolateVal > 1.0)
            {
                interpolateVal = 1.0;
            }
            if (interpolateVal < 0.0)
            {
                interpolateVal = 0.0;
            }
            //Calculate the new value
            ROM[i] = (byte)LinearInterpolationCalculation(byteminus, byteplus, interpolateVal);
            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
            //Trace log the bytes selected and the interpolate value, then the result, e.g. byte 1 = 0 byte 2 = 255, interpolate value = 0.5, result = 128
            //TraceLogger.Log("[x] Lerp Engine trigger: byte 1 = " + byteminus + " byte 2 = " + byteplus + " interpolate value = " + interpolateVal + " result = " + ROM[i]);
            return ROM;
        }
    }
}