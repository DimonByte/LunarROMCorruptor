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

using LunarROMCorruptor.CorruptionInternals;

namespace LunarROMCorruptor.CorruptionEngines
{
    internal class ManualEngine
    {
        private static readonly Random rnd = new();
        private static readonly List<byte> list = [];
        public static byte[] CorruptByte(byte[] ROM, long i, int StartByte, int EndByte)
        {
            if (Program.Form.IncrementCHECK.Checked)
            {
                ROM[i] = CorruptionCore.ClampByte(ROM[i] + (int)Program.Form.CorruptionEngineFrame.IncreDecrenumbnightmare.Value);
                Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
            }
            if (Program.Form.SHIFTBYTECHECK.Checked)
            {
                long j = (long)(i + Program.Form.ShiftNumericUpDown.Value);
                if (j >= StartByte && j <= EndByte)
                {
                    ROM[j] = ROM[i];
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                }
            }
            if (Program.Form.MakeBitEqualCHECK.Checked)
            {
                ROM[i] = (byte)Program.Form.ByteEqualNumericUpDown.Value;
                Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
            }
            if (Program.Form.ReplaceCHECK.Checked)
            {
                if (ROM[i] == (byte)Program.Form.ReplaceNumericUpDown1.Value)
                {
                    ROM[i] = (byte)Program.Form.ReplaceNumericUpDown2.Value;
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                }
            }
            if (Program.Form.PasterandombitCHECK.Checked)
            {
                byte copy = ROM[rnd.Next(StartByte, EndByte)];
                ROM[i] = copy;
                Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + copy + ")");
            }
            if (Program.Form.RepeatRandomBitCHECK.Checked)
            {
                list.Clear();
                for (int i2 = 0; i2 <= Program.Form.RepeatNumericUpDown.Value - 1; i2++)
                    list.Add(ROM[i + i2]);
                // ListBox3.Items.Add(String.Join(" ", list))
                foreach (var itemcon in list)
                {
                    long final = rnd.Next(StartByte, EndByte);
                    ROM[final] = itemcon;
                    Program.Form.InternalStashItems.Add("[x] File(" + final + ").SET(" + itemcon + ")");
                }
            }
            if (Program.Form.MULTIORDIVIDECHeck.Checked)
            {
                if (Program.Form.MultiRadio.Checked)
                {
                    ROM[i] = CorruptionCore.ClampByte(ROM[i] * (int)Program.Form.MathOperationNumericUpDown.Value);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                }
                if (Program.Form.DivideRadio.Checked)
                {
                    ROM[i] = CorruptionCore.ClampByte(ROM[i] / (int)Program.Form.MathOperationNumericUpDown.Value);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                }
                if (Program.Form.DoubleCheck.Checked)
                {
                    ROM[i] = (byte)((byte)Math.Pow(ROM[i], (double)Program.Form.MathOperationNumericUpDown.Value) % (byte.MaxValue + 1));
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                }
            }
            return ROM;
        }
    }
}