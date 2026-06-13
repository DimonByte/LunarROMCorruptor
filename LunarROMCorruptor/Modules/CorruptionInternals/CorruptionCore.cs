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

namespace LunarROMCorruptor.Modules.CorruptionInternals
{
    public class CorruptionCore
    {
        public static byte[]? ROM; //Used to store the file that is loaded into the program, If single file corruption is used, this will contain the original ROM and FINROM will be the corrupted ROM. If multiple file corruption is used, this will contain the ROM that is being corrupted.
        public static byte[]? FINROM; //Used to store the file that is loaded into the program. This is the corrupted ROM if single file corruption is used.
        public static int MaxByte; //This stores the maxium amount of bytes in the file that is loaded
        public static double rawFileSize; //This stores the file size of the loaded ROM, used for some calculations in the engines
        public static string? fileSizeUnit; //This stores the unit of the file size (E.g. MB, GB) used for some calculations in the engines
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

        /// <summary>
        /// Loads ROM into memory interally, this doesn't handle GUI updates.
        /// </summary>
        /// <param name="FileLocation"></param>
        /// <returns></returns>
        public static bool LoadROM(string FileLocation)
        {
            TraceLogger.Log($"Attempting to load ROM from: {FileLocation}");
            var fileInfo = new FileInfo(FileLocation);
            if (fileInfo.Length < 2147483648) //Check if file is less than 2GB
            {
                TraceLogger.Log($"File size: {fileInfo.Length} bytes");
                if (fileInfo.Length == 0)
                {
                    TraceLogger.Log("The selected file is empty. Aborting load.", StatusSeverityType.Error, true);
                    return false;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                TraceLogger.Log("Loading ROM into memory...");
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
                TraceLogger.Log($"File size: {rawFileSize} {fileSizeUnit}");
                return true;
            }
            else
            {
                TraceLogger.Log("The selected file is too large, file size limit is 2 GB. Aborting load.", StatusSeverityType.Error, true);
                return false;
            }
        }
        /// <summary>
        /// Main corruption function.
        /// </summary>
        /// <param name="ROM">The untouched ROM.</param>
        /// <param name="StartByte">Start byte index for corruption.</param>
        /// <param name="EndByte">End byte index for corruption.</param>
        /// <param name="CorruptNthByte">Whether to corrupt every nth byte or use intensity mode.</param>
        /// <param name="Intensity">Number of operations or step size.</param>
        /// <param name="CorruptionEngine">Name of the engine to use.</param>
        /// <returns>Corrupted ROM as a byte array.</returns>
        public static byte[]? StartCorruption(
            byte[] ROM,
            int StartByte,
            int EndByte,
            bool CorruptNthByte,
            int Intensity,
            string CorruptionEngine)
        {
            TraceLogger.Log($"Starting corruption with the following settings:\nStart Byte: {StartByte}\nEnd Byte: {EndByte}\nCorrupt Every Nth Byte: {CorruptNthByte}\nIntensity: {Intensity}\nCorruption Engine: {CorruptionEngine}");

            if (string.IsNullOrEmpty(CorruptionEngine))
            {
                TraceLogger.Log("Invalid corruption engine selected. Null engine selected.", StatusSeverityType.Error, true);
                return null;
            }

            switch (EngineParser.ParseEngineType(CorruptionEngine))
            {
                case CorruptionEngineType.NightmareEngine:
                    return EngineProcessor.ProcessNightmareEngine(ROM, StartByte, EndByte, CorruptNthByte, Intensity);
                case CorruptionEngineType.LerpEngine:
                    return EngineProcessor.ProcessLerpEngine(ROM, StartByte, EndByte, CorruptNthByte, Intensity);
                case CorruptionEngineType.MergeEngine:
                    return EngineProcessor.ProcessMergeEngine(ROM, StartByte, EndByte, CorruptNthByte, Intensity);
                case CorruptionEngineType.LogicEngine:
                    return EngineProcessor.ProcessLogicEngine(ROM, StartByte, EndByte, CorruptNthByte, Intensity);
                case CorruptionEngineType.ManualEngine:
                    return EngineProcessor.ProcessManualEngine(ROM, StartByte, EndByte, CorruptNthByte, Intensity);
                case CorruptionEngineType.ExclusionEngine:
                    return EngineProcessor.ProcessExclusionEngine(ROM, StartByte, EndByte, CorruptNthByte, Intensity, Program.Form.ExclusionEngineFrame.ExclusionTypeComboBox.Text);
                default:
                    TraceLogger.Log("Invalid corruption engine selected. This should never happen. Aborting corruption.", StatusSeverityType.Fatal, true);
                    if (MessageBox.Show(
                        "Corruption engine not found! The program has paused to prevent unwanted corruption.\nClick yes to close this program (Recommended) or no to continue anyway. (The corrupted ROM HASN'T been saved to the file yet)",
                        "Fatal Error - LRC", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                    return ROM;
            }
        }

        public static void SaveROM(byte[] ROM, string FileLocation)
        {
            TraceLogger.Log($"Saving ROM to: {FileLocation}");
            try
            {
                File.WriteAllBytes(FileLocation, ROM);
                TraceLogger.Log("ROM saved successfully.");
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error when saving ROM: {ex}", StatusSeverityType.Error, true);
            }
        }

        public static void RestoreROM(string FileLocation, byte[] ROM)
        {
            TraceLogger.Log($"Restoring ROM from: {FileLocation}");
            try
            {
                if (ROM == null)
                {
                    TraceLogger.Log("No ROM loaded to restore.", StatusSeverityType.Warning, true);
                    return;
                }
                File.WriteAllBytes(FileLocation, ROM);
                TraceLogger.Log("ROM restored successfully.");
                MessageBox.Show("ROM restored successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error when restoring ROM: {ex}", StatusSeverityType.Error, true);
            }
        }
    }
}