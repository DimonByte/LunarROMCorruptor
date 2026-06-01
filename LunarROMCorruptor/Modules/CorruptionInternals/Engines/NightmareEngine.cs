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

using LunarROMCorruptor.Modules;
using LunarROMCorruptor.Modules.CorruptionInternals;

namespace LunarROMCorruptor.Modules.CorruptionInternals.Engines
{
    internal class NightmareEngine
    {
        private static readonly Random rnd = new();

        public static byte[] CorruptByte(byte[] ROM, CorruptionOptions CorruptOption, long i)
        {
            switch (CorruptOption)
            {
                case CorruptionOptions.RANDOM:
                    ROM[i] = (byte)rnd.Next(0, 256);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                    break;

                case CorruptionOptions.RANDOMTILT:
                    switch (rnd.Next(0, 3))
                    {
                        case 0:
                            ROM[i] = (byte)((byte)rnd.Next(0, 256) % (Byte.MaxValue + 1));
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        case 1:
                            ROM[i] = CorruptionCore.ClampByte(ROM[i] + (int)Program.Form.CorruptionEngineFrame.IncreDecrenumbnightmare.Value);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        case 2:
                            ROM[i] = CorruptionCore.ClampByte(ROM[i] - (int)Program.Form.CorruptionEngineFrame.IncreDecrenumbnightmare.Value);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        default:
                            if (MessageBox.Show("The Nightmare Engine returned a result that wasn't expected! Click yes to close this program or no to continue anyway.", "ERROR", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                Application.Exit();
                            }
                            break;
                    }
                    break;

                case CorruptionOptions.TILT:
                    switch (rnd.Next(0, 2))
                    {
                        case 0:
                            ROM[i] = CorruptionCore.ClampByte(ROM[i] + (int)Program.Form.CorruptionEngineFrame.IncreDecrenumbnightmare.Value);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        case 1:
                            ROM[i] = CorruptionCore.ClampByte(ROM[i] - (int)Program.Form.CorruptionEngineFrame.IncreDecrenumbnightmare.Value);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;
                    }
                    break;

                default:
                    if (MessageBox.Show("The Nightmare Engine returned a result that wasn't expected! Click yes to close this program or no to continue anyway.", "ERROR", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                    break;
            }
            return ROM;
        }
    }
}