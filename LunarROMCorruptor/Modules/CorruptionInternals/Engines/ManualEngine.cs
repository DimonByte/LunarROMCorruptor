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
    internal class ManualEngine
    {
        private static readonly Random rnd = new();
        private static readonly List<byte> list = [];
        public static byte[] CorruptByte(byte[] ROM, long i, int StartByte, int EndByte)
        {
            if (Program.Form.ManualEngineFrame.IncrementCHECK.Checked)
            {
                ROM[i] = CorruptionCore.ClampByte(ROM[i] + (int)Program.Form.ManualEngineFrame.IncrementNumericUpDown.Value);
                Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
            }
            if (Program.Form.ManualEngineFrame.SHIFTBYTECHECK.Checked)
            {
                long j = (long)(i + Program.Form.ManualEngineFrame.ShiftNumericUpDown.Value);
                if (j >= StartByte && j <= EndByte)
                {
                    ROM[j] = ROM[i];
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                }
            }
            if (Program.Form.ManualEngineFrame.MakeBitEqualCHECK.Checked)
            {
                ROM[i] = (byte)Program.Form.ManualEngineFrame.ByteEqualNumericUpDown.Value;
                Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
            }
            if (Program.Form.ManualEngineFrame.ReplaceCHECK.Checked)
            {
                //FIXME: Overflow possible.
                if (ROM[i] == (byte)Program.Form.ManualEngineFrame.ReplaceNumericUpDown1.Value)
                {
                    ROM[i] = (byte)Program.Form.ManualEngineFrame.ReplaceNumericUpDown2.Value;
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                }
            }
            if (Program.Form.ManualEngineFrame.PasterandombitCHECK.Checked)
            {
                byte copy = ROM[rnd.Next(StartByte, EndByte)];
                ROM[i] = copy;
                Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + copy + ")");
            }
            if (Program.Form.ManualEngineFrame.RepeatRandomBitCHECK.Checked)
            {
                list.Clear();
                for (int i2 = 0; i2 <= Program.Form.ManualEngineFrame.RepeatNumericUpDown.Value - 1; i2++)
                    list.Add(ROM[i + i2]);
                // ListBox3.Items.Add(String.Join(" ", list))
                foreach (var itemcon in list)
                {
                    long final = rnd.Next(StartByte, EndByte);
                    ROM[final] = itemcon;
                    Program.Form.InternalStashItems.Add("[x] File(" + final + ").SET(" + itemcon + ")");
                }
            }
            if (Program.Form.ManualEngineFrame.MULTIORDIVIDECHeck.Checked)
            {
                if (Program.Form.ManualEngineFrame.MultiRadio.Checked)
                {
                    ROM[i] = CorruptionCore.ClampByte(ROM[i] * (int)Program.Form.ManualEngineFrame.MathOperationNumericUpDown.Value);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                }
                if (Program.Form.ManualEngineFrame.DivideRadio.Checked)
                {
                    ROM[i] = CorruptionCore.ClampByte(ROM[i] / (int)Program.Form.ManualEngineFrame.MathOperationNumericUpDown.Value);
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                }
                if (Program.Form.ManualEngineFrame.DoubleCheck.Checked)
                {
                    ROM[i] = (byte)((byte)Math.Pow(ROM[i], (double)Program.Form.ManualEngineFrame.MathOperationNumericUpDown.Value) % (byte.MaxValue + 1));
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                }
            }
            return ROM;
        }
    }
}