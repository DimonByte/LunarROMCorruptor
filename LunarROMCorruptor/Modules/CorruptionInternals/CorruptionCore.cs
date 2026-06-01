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

using LunarROMCorruptor.Modules.CorruptionInternals.Engines;
using LunarROMCorruptor.Properties;
using System.Diagnostics;

namespace LunarROMCorruptor.Modules.CorruptionInternals
{
    public class CorruptionCore
    {
        private static readonly Random rnd = new();
        //Main Corruption Core:
        //This is where the corruption happens, this function below corrupts bytes by going to their respective corruption engines (E.g. NightmareEngine.cs) and using the output of that engine to corrupt the ROM.
        //Inside the engine is where the code that manipulates the byte happens. Once it has set the byte, it writes a new item to the internal stash list which is used to store the changes made to the ROM.
        public static byte ClampByte(int x) //This is to prevent the byte from going over 255 or going under 0
        {
            if (x < 0)
                return 0;
            if (x > 255)
                return 255;
            return (byte)x;
        }
        public static void AttemptProtectedFileOverride(string FileLocation)
        {
            if (Settings.Default.AttemptProtectedFileOverride)
            {
                try
                {
                    Console.WriteLine("LRC - Corruption Core: Attempting Protected File Ownership Override.");
                    using (Process cmdProcess = new())
                    {
                        cmdProcess.StartInfo.FileName = "CMD.exe";
                        cmdProcess.StartInfo.Arguments = $"/c takeown /F \"{FileLocation}\" && icacls \"{FileLocation}\" /grant {Environment.UserDomainName}\\{Environment.UserName}:(OI)(CI)F /T";
                        cmdProcess.StartInfo.CreateNoWindow = true;
                        cmdProcess.StartInfo.UseShellExecute = false;
                        cmdProcess.Start();
                        cmdProcess.WaitForExit();
                    }
                    Console.WriteLine("LRC - Corruption Core: File Ownership and Permission Override Completed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"LRC - Corruption Core: Error during File Ownership and Permission Override. Exception: {ex.Message}");
                }
            }
        }


        public static byte[]? StartCorruption(byte[] ROM, int StartByte, int EndByte, bool CorruptNthByte, int Intensity, string CorruptionEngine)
        {
            switch (CorruptionEngine)
            {
                case "Nightmare Engine":
                    if (CorruptNthByte)
                    {
                        //CorruptNTH selected
                        int i1 = StartByte;
                        bool enumParseCheck = Enum.TryParse(Program.Form.CorruptionEngineFrame.NightmareComboBox.Text, out CorruptionOptions corruptiontype);
                        if (!enumParseCheck)
                        {
                            MessageBox.Show("Failed to parse corruption type. Please check your selection.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        while (i1 <= EndByte)
                        {
                            NightmareEngine.CorruptByte(ROM, corruptiontype, i1);
                            i1 += Intensity;
                        }
                    }
                    else //Intensity Mode
                    {
                        bool enumParseCheck = Enum.TryParse(Program.Form.CorruptionEngineFrame.NightmareComboBox.Text, out CorruptionOptions corruptiontype);
                        if (!enumParseCheck)
                        {
                            MessageBox.Show("Failed to parse corruption type. Please check your selection.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        for (int i1 = 0; i1 <= Intensity - 1; i1++)
                        {
                            NightmareEngine.CorruptByte(ROM, corruptiontype, rnd.Next(StartByte, EndByte));
                        }
                    }
                    break;

                case "Lerp Engine":
                    if (CorruptNthByte)
                    {
                        //CorruptNTH selected
                        int i1 = StartByte;
                        while (i1 <= EndByte)
                        {
                            LerpEngine.CorruptByte(ROM, i1);
                            i1 += Intensity;
                        }
                    }
                    else //Intensity Mode
                    {
                        for (int i1 = 0; i1 <= Intensity - 1; i1++)
                        {
                            LerpEngine.CorruptByte(ROM, rnd.Next(StartByte, EndByte));
                        }
                    }
                    break;

                case "Merge Engine":
                    //Check if the merge file exists, if not throw an error
                    if (string.IsNullOrEmpty(Program.Form.CorruptionEngineFrame.MergeFileLocationTxt.Text))
                    {
                        MessageBox.Show("Merge file location is empty. Please select a file.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }

                    byte[] ROMmerge = File.ReadAllBytes(Program.Form.CorruptionEngineFrame.MergeFileLocationTxt.Text);
                    if (CorruptNthByte)
                    {
                        //CorruptNTH selected
                        int i1 = StartByte;
                        bool enumParseCheck = Enum.TryParse(Program.Form.CorruptionEngineFrame.CorrTypeMerge.Text, out CorruptionOptions corruptiontype);
                        if (!enumParseCheck)
                        {
                            MessageBox.Show("Failed to parse corruption type. Please check your selection.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        while (i1 <= EndByte)
                        {
                            MergeEngine.CorruptByte(ROM, ROMmerge, corruptiontype, i1);
                            i1 += Intensity;
                        }
                    }
                    else //Intensity Mode
                    {
                        bool enumParseCheck = Enum.TryParse(Program.Form.CorruptionEngineFrame.CorrTypeMerge.Text, out CorruptionOptions corruptiontype);
                        if (!enumParseCheck)
                        {
                            MessageBox.Show("Failed to parse corruption type. Please check your selection.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        for (int i1 = 0; i1 <= Intensity - 1; i1++)
                        {
                            MergeEngine.CorruptByte(ROM, ROMmerge, corruptiontype, rnd.Next(StartByte, EndByte));
                        }
                    }
                    break;

                case "Logic Engine":
                    if (CorruptNthByte)
                    {
                        //CorruptNTH selected
                        bool enumParseCheck = Enum.TryParse(Program.Form.CorruptionEngineFrame.BitwiseComboBox.Text, out CorruptionOptions corruptiontype);
                        if (!enumParseCheck)
                        {
                            MessageBox.Show("Failed to parse corruption type. Please check your selection.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        int i1 = StartByte;
                        while (i1 <= EndByte)
                        {
                            if (Program.Form.CorruptionEngineFrame.LogicRandomizeTypeCheckbox.Checked) //Randomize selection
                            {
                                int randomIndex = rnd.Next(1, Program.Form.CorruptionEngineFrame.BitwiseComboBox.Items.Count - 1);
                                Program.Form.CorruptionEngineFrame.BitwiseComboBox.SelectedIndex = randomIndex;
                            }
                            if (Program.Form.CorruptionEngineFrame.LogicRandomizeValueCheckBox.Checked) //Randomize Value
                            {
                                int randomValue = rnd.Next((int)Program.Form.CorruptionEngineFrame.ValueBitwise.Minimum, (int)Program.Form.CorruptionEngineFrame.ValueBitwise.Maximum);
                                Program.Form.CorruptionEngineFrame.ValueBitwise.Value = randomValue;
                            }
                            LogicEngine.CorruptByte(ROM, corruptiontype, i1, (int)Program.Form.CorruptionEngineFrame.ValueBitwise.Value);
                            i1 += Intensity;
                        }
                    }
                    else //Intensity Mode
                    {
                        bool enumParseCheck = Enum.TryParse(Program.Form.CorruptionEngineFrame.BitwiseComboBox.Text, out CorruptionOptions corruptiontype);
                        if (!enumParseCheck)
                        {
                            MessageBox.Show("Failed to parse corruption type. Please check your selection.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                        for (int i1 = 0; i1 <= Intensity - 1; i1++)
                        {
                            if (Program.Form.CorruptionEngineFrame.LogicRandomizeTypeCheckbox.Checked) //Randomize selection
                            {
                                int randomIndex = rnd.Next(1, Program.Form.CorruptionEngineFrame.BitwiseComboBox.Items.Count - 1);
                                Program.Form.CorruptionEngineFrame.BitwiseComboBox.SelectedIndex = randomIndex;
                            }
                            if (Program.Form.CorruptionEngineFrame.LogicRandomizeValueCheckBox.Checked) //Randomize Value
                            {
                                int randomValue = rnd.Next((int)Program.Form.CorruptionEngineFrame.ValueBitwise.Minimum, (int)Program.Form.CorruptionEngineFrame.ValueBitwise.Maximum);
                                Program.Form.CorruptionEngineFrame.ValueBitwise.Value = randomValue;
                            }
                            //MessageBox.Show(StartByte.ToString() + EndByte.ToString());
                            LogicEngine.CorruptByte(ROM, corruptiontype, rnd.Next(StartByte, EndByte), (int)Program.Form.CorruptionEngineFrame.ValueBitwise.Value);
                        }
                    }
                    break;

                case "Fractal Engine":
                    if (CorruptNthByte)
                    {
                        //CorruptNTH selected
                        int i1 = StartByte;
                        while (i1 <= EndByte)
                        {
                            FractalEngine.CorruptByte(ROM, i1);
                            i1 += Intensity;
                        }
                    }
                    else //Intensity Mode
                    {
                        for (int i1 = 0; i1 <= Intensity - 1; i1++)
                        {
                            FractalEngine.CorruptByte(ROM, rnd.Next(StartByte, EndByte));
                        }
                    }
                    break;
                case "Manual Engine":
                    if (CorruptNthByte) //CorruptNTH mode
                    {
                        int i = StartByte;
                        while (i <= EndByte)
                        {
                            ManualEngine.CorruptByte(ROM, i, StartByte, EndByte);
                            i += Intensity;
                        }
                    }
                    else //Intense mode
                    {
                        for (int i1 = 0; i1 <= Intensity - 1; i1++)
                        {
                            long i = rnd.Next(StartByte, EndByte);
                            ManualEngine.CorruptByte(ROM, i, StartByte, EndByte);
                        }
                    }
                    break;

                default:
                    //This should not happen. Something went really wrong.
                    if (MessageBox.Show("Corruption engine not found! The program has paused to prevent unwanted corruption.\nClick yes to close this program (Recommended) or no to continue anyway. (The corrupted ROM HASN'T been saved to the file yet)", "Fatal Error - LRC", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                    break;
            }
            return ROM;
        }
        public static void StartEmulator(bool ReopenProgram, string EmulatorLocation, string FileLocation, bool OverrideArgumentsChk, string OverrideArguments)
        {
            try
            {
                if (ReopenProgram)
                {
                    var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(EmulatorLocation));
                    if (processes.Length > 0)
                        processes[0].Kill();
                }

                ProcessStartInfo startInfo = new()
                {
                    FileName = EmulatorLocation,
                    UseShellExecute = false, // Start process directly without going to windows shell
                    Arguments = OverrideArgumentsChk ? OverrideArguments : $"\"{FileLocation}\""
                };

                Process p = new()
                {
                    StartInfo = startInfo
                };

                p.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error when starting process/emulator: {ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}