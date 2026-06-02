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
        public static byte[]? ROM; //Used to store the file that is loaded into the program
        public static int MaxByte; //This stores the maxium amount of bytes in the file that is loaded
        public static int StartByte; //This stores the start byte that the user sets
        public static int EndByte; //This stores the end byte that the user sets]
        public static double rawFileSize; //This stores the file size of the loaded ROM, used for some calculations in the engines
        public static string fileSizeUnit; //This stores the unit of the file size (E.g. MB, GB) used for some calculations in the engines
        private static readonly Random rnd = new();

        /// <summary>
        /// Ensures that byte values don't go under 0 or above 255, this is used in some of the engines to prevent overflow and underflow errors that can cause the program to crash.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static byte ClampByte(int x) 
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
        /// <summary>
        /// Loads ROM into memory interally, this doesn't handle GUI updates.
        /// </summary>
        /// <param name="FileLocation"></param>
        /// <returns></returns>
        public static bool LoadROM(string FileLocation)
        {
            var fileInfo = new FileInfo(FileLocation);
            if (fileInfo.Length < 2147483648) //Check if file is less than 2GB
            {
                if (fileInfo.Length == 0)
                {
                    MessageBox.Show("The selected file is empty. Please select a valid ROM file.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                ROM = File.ReadAllBytes(FileLocation);
                MaxByte = ROM.Length - 1;
                long fileSize;
                fileSize = fileInfo.Length;
                if (fileSize > 1073741824) // Greater than 1 GB
                {
                    fileSizeUnit = "GB";
                    rawFileSize = Math.Round((double)fileSize / 1073741824, 2);
                }
                else if (fileSize < 1000000) // Less than 1 MB
                {
                    fileSizeUnit = "KB";
                    rawFileSize = Math.Round((double)fileSize / 1000, 2);
                }
                else // Less than 1 GB but more than or equal to 1 MB
                {
                    fileSizeUnit = "MB";
                    rawFileSize = Math.Round((double)fileSize / 1000000, 2);
                }
                return true;
            }
            else
            {
                MessageBox.Show("The selected file is too large. Please select a ROM file smaller than 2GB.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        /// <summary>
        /// Main corruption function.
        /// </summary>
        /// <param name="ROM"></param> The untouched ROM.
        /// <param name="StartByte"></param> The start byte that the user sets, this is used in the engines to determine where to start corrupting from.
        /// <param name="EndByte"></param> The end byte that the user sets, this is used in the engines to determine where to stop corrupting.
        /// <param name="CorruptNthByte"></param> Whether the user has selected to corrupt every nth byte or to use intensity mode, this is used in the engines to determine how to corrupt the ROM.
        /// <param name="Intensity"></param> The intensity that the user sets, this is used in the engines to determine how many bytes to corrupt or how many bytes to skip when corrupting every nth byte.
        /// <param name="CorruptionEngine"></param> The corruption engine that the user selects, this is used to determine which engine to use when corrupting the ROM.
        /// <returns>Corrupted ROM</returns>
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