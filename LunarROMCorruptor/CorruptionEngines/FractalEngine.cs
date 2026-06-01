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

namespace LunarROMCorruptor.CorruptionEngines
{
    internal class FractalEngine
    {

        public static byte[] CorruptByte(byte[] ROM, long i)
        {
            // Set up parameters for the Lorenz Attractor algorithm
            double x = 0.1, y = 0.0, z = 0.0;
            double dt = 0.01, sigma = 10.0, rho = 28.0, beta = 8.0 / 3.0;

            // Calculate the next value in the Lorenz Attractor sequence
            double dx = sigma * (y - x);
            double dy = x * (rho - z) - y;
            double dz = x * y - beta * z;
            x += dx * dt;
            y += dy * dt;
            z += dz * dt;
            double value = x;

            // Use the value to modify the byte at the current location in the file
            byte oldValue = ROM[i];
            byte newValue = (byte)(oldValue + value);
            ROM[i] = newValue;

            return ROM;
        }
    }
}
