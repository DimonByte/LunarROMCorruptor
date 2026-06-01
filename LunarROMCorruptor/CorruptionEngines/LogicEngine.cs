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
    internal class LogicEngine
    {
        public static byte[] CorruptByte(byte[] ROM, CorruptionOptions CorruptOption, long i, int bitwiseval)
        {
            byte BitVal = Convert.ToByte(bitwiseval);
            switch (CorruptOption)
            {
                case CorruptionOptions.AND:
                    ROM[i] = (byte)(ROM[i] & BitVal);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                    //Console.WriteLine("[x] AND trigger");
                    break;
                case CorruptionOptions.OR:
                    ROM[i] = (byte)(ROM[i] | BitVal);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                    //Console.WriteLine("[x] OR trigger");
                    break;
                case CorruptionOptions.XOR:
                    ROM[i] = (byte)(ROM[i] ^ BitVal);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                    //Console.WriteLine("[x] XOR trigger");
                    break;
                case CorruptionOptions.NOT:
                    ROM[i] = (byte)~(ROM[i]);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                    //Console.WriteLine("[x] NOT trigger");
                    break;
                case CorruptionOptions.NAND:
                    ROM[i] = (byte)~(ROM[i] & BitVal);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                    //Console.WriteLine("[x] NAND trigger");
                    break;
                case CorruptionOptions.NOR:
                    ROM[i] = (byte)~(ROM[i] | BitVal);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                    //Console.WriteLine("[x] NOR trigger");
                    break;
                case CorruptionOptions.SWAP:
                    byte swap1 = ROM[i];
                    byte swap2;
                    //Console.WriteLine("[x] SWAP trigger");
                    try
                    {
                        swap2 = ROM[i + 1];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        //Console.WriteLine("[x] Error: Index out of range.");
                        break;
                    }
                    //Console.WriteLine("[x] Passed check");
                    ROM[i] = swap2;
                    ROM[i + 1] = swap1;
                    int i1 = (int)(i + BitVal);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + swap2 + ")");
                    Program.Form.InternalStashItems.Add("[x] File(" + i1 + ").SET(" + swap1 + ")");
                    break;
                case CorruptionOptions.SHIFT:
                    ROM[i] = ROM[i + BitVal];
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                    //Console.WriteLine("[x] SHIFT trigger");
                    break;
                default:
                    if (MessageBox.Show("The Logic Engine returned a result that wasn't expected! Click yes to close this program or no to continue anyway.", "ERROR", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                    break;
            }
            return ROM;
        }
    }
}
