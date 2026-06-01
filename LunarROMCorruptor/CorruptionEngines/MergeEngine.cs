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
    internal class MergeEngine
    {
        private static readonly Random rnd = new();

        public static byte[] CorruptByte(byte[] ROM, byte[] ROMMerge, CorruptionOptions CorruptionOption, long i)
        {
            if (Program.Form.CorruptionEngineFrame.ReplaceByteWithSamePos.Checked)
            {
                if (Program.Form.CorruptionEngineFrame.Mod256MergeEnginechkbox.Checked)
                {
                    switch (CorruptionOption)
                    {
                        case CorruptionOptions.NONE:
                            ROM[i] = (byte)(ROMMerge[i] % Byte.MaxValue + 1);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        case CorruptionOptions.RANGE:
                            ROM[i] = (byte)Math.Abs(ROM[i] - ROMMerge[i] % byte.MaxValue + 1);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        default:
                            if (MessageBox.Show("The Merge Engine returned a result that wasn't expected! Click yes to close this program or no to continue anyway.", "ERROR", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                Application.Exit();
                            }
                            break;
                    }
                }
                else
                {
                    switch (CorruptionOption)
                    {
                        case CorruptionOptions.NONE:
                            ROM[i] = ROMMerge[i];
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        case CorruptionOptions.RANGE:
                            ROM[i] = (byte)Math.Abs(ROM[i] - ROMMerge[i]);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        default:
                            if (MessageBox.Show("The Merge Engine returned a result that wasn't expected! Click yes to close this program or no to continue anyway.", "ERROR", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                Application.Exit();
                            }
                            break;
                    }
                }
            }
            else
            {
                if (Program.Form.CorruptionEngineFrame.Mod256MergeEnginechkbox.Checked)
                {
                    switch (CorruptionOption)
                    {
                        case CorruptionOptions.NONE:
                            ROM[i] = (byte)(ROMMerge[rnd.Next(0, ROMMerge.Length)] % Byte.MaxValue + 1);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        case CorruptionOptions.RANGE:
                            ROM[i] = (byte)Math.Abs(ROM[rnd.Next(0, ROMMerge.Length)] - ROMMerge[i] % byte.MaxValue + 1);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        default:
                            if (MessageBox.Show("The Merge Engine returned a result that wasn't expected! Click yes to close this program or no to continue anyway.", "ERROR", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                Application.Exit();
                            }
                            break;
                    }
                }
                else
                {
                    switch (CorruptionOption)
                    {
                        case CorruptionOptions.NONE:
                            ROM[i] = ROMMerge[rnd.Next(0, ROMMerge.Length)];
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        case CorruptionOptions.RANGE:
                            ROM[i] = (byte)Math.Abs(ROM[i] - ROMMerge[rnd.Next(0, ROMMerge.Length)]);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;

                        default:
                            if (MessageBox.Show("The Merge Engine returned a result that wasn't expected! Click yes to close this program or no to continue anyway.", "ERROR", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                Application.Exit();
                            }
                            break;
                    }
                }
            }
            return ROM;
        }
    }
}