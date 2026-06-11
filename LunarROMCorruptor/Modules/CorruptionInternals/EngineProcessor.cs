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

namespace LunarROMCorruptor.Modules.CorruptionInternals
{
    internal class EngineProcessor
    {
        private static readonly Random rnd = new();
        public static byte[]? ProcessNightmareEngine(byte[] ROM, int StartByte, int EndByte, bool CorruptNthByte, int Intensity)
        {
            var corruptionType = EngineParser.ParseCorruptionOptions(Program.Form.NightmareEngineFrame.NightmareComboBox.Text);
            if (corruptionType == null) return null;

            if (CorruptNthByte)
            {
                for (int i = StartByte; i <= EndByte; i += Intensity)
                {
                    NightmareEngine.CorruptByte(ROM, corruptionType.Value, i);
                }
            }
            else
            {
                for (int i = 0; i < Intensity; i++)
                {
                    int randomIndex = rnd.Next(StartByte, EndByte);
                    NightmareEngine.CorruptByte(ROM, corruptionType.Value, randomIndex);
                }
            }

            return ROM;
        }

        public static byte[]? ProcessLerpEngine(byte[] ROM, int StartByte, int EndByte, bool CorruptNthByte, int Intensity)
        {
            if (CorruptNthByte)
            {
                for (int i = StartByte; i <= EndByte; i += Intensity)
                {
                    LerpEngine.CorruptByte(ROM, i);
                }
            }
            else
            {
                for (int i = 0; i < Intensity; i++)
                {
                    int randomIndex = rnd.Next(StartByte, EndByte);
                    LerpEngine.CorruptByte(ROM, randomIndex);
                }
            }

            return ROM;
        }

        public static byte[]? ProcessExclusionEngine(byte[] ROM, int StartByte, int EndByte, bool CorruptNthByte, int Intensity, string Profile)
        {
            var corruptionType = EngineParser.ParseCorruptionOptions(Program.Form.ExclusionEngineFrame.NightmareComboBox.Text);
            if (corruptionType == null) return null;
            var safeLocations = ExclusionEngine.SafeLocations.GetValueOrDefault(Profile, []);
            if (CorruptNthByte)
            {
                bool corrupted = false;
                for (int i = StartByte; i <= EndByte; i += Intensity)
                {
                    // Check if this byte is in a safe location, if so skip it
                    if (!ExclusionEngine.IsSafeLocation(i, safeLocations))
                    {
                        corrupted = true;
                        ExclusionEngine.CorruptByte(ROM, corruptionType.Value, Profile, i);
                    }
                    //else
                    //{
                    //    TraceLogger.Log($"{i} is not safe! Ignoring that address.");
                    //}
                }
                if (!corrupted)
                {
                    TraceLogger.Log($"CorruptNthByte reached end of file with profile {Profile} but did not find any safe locations to corrupt.", StatusSeverityType.Warning);
                }
            }
            else
            {
                for (int i = 0; i < Intensity; i++)
                {
                    // Keep trying to get a random index that's not in a safe location
                    int tries = 0;
                    int maxTries = 1000; // Prevent infinite loop

                    do
                    {
                        int randomIndex = new Random().Next(StartByte, EndByte);
                        // If index is safe, try again unless we've exceeded max attempts
                        if (ExclusionEngine.IsSafeLocation(randomIndex, safeLocations))
                        {
                            tries++;
                            //TraceLogger.Log($"{i} is not safe! Ignoring that address.");
                            continue;
                        }

                        // We found a valid index for the selected profile - corrupt it! AA
                        ExclusionEngine.CorruptByte(ROM, corruptionType.Value, Profile, randomIndex);
                        break;
                    } while (tries < maxTries);

                    // If we've reached max tries, show an error since we don't want infinity here.
                    if (tries >= maxTries)
                    {
                        TraceLogger.Log("Max tries (" + maxTries + ") reached for random selection in ExclusionEngine with profile " + Profile, StatusSeverityType.Warning);
                    }
                }
            }

            return ROM;
        }

        public static byte[]? ProcessMergeEngine(byte[] ROM, int StartByte, int EndByte, bool CorruptNthByte, int Intensity)
        {
            if (string.IsNullOrEmpty(Program.Form.MergeEngineFrame.MergeFileLocationTxt.Text))
            {
                TraceLogger.Log("Merge file location is empty. Please select a merge file. Aborting merge corruption.", StatusSeverityType.Error, true);
                return null;
            }

            byte[] ROMmerge = File.ReadAllBytes(Program.Form.MergeEngineFrame.MergeFileLocationTxt.Text);

            var corruptionType = EngineParser.ParseCorruptionOptions(Program.Form.MergeEngineFrame.CorrTypeMerge.Text);
            if (corruptionType == null) return null;

            if (CorruptNthByte)
            {
                for (int i = StartByte; i <= EndByte; i += Intensity)
                {
                    MergeEngine.CorruptByte(ROM, ROMmerge, corruptionType.Value, i);
                }
            }
            else
            {
                for (int i = 0; i < Intensity; i++)
                {
                    int randomIndex = rnd.Next(StartByte, EndByte);
                    MergeEngine.CorruptByte(ROM, ROMmerge, corruptionType.Value, randomIndex);
                }
            }

            return ROM;
        }

        public static byte[]? ProcessLogicEngine(byte[] ROM, int StartByte, int EndByte, bool CorruptNthByte, int Intensity)
        {
            var corruptionType = EngineParser.ParseCorruptionOptions(Program.Form.LogicEngineFrame.BitwiseComboBox.Text);
            if (corruptionType == null) return null;

            if (CorruptNthByte)
            {
                for (int i = StartByte; i <= EndByte; i += Intensity)
                {
                    UpdateLogicControls();
                    LogicEngine.CorruptByte(ROM, corruptionType.Value, i, (int)Program.Form.LogicEngineFrame.ValueBitwise.Value);
                }
            }
            else
            {
                for (int i = 0; i < Intensity; i++)
                {
                    UpdateLogicControls();
                    int randomIndex = rnd.Next(StartByte, EndByte);
                    LogicEngine.CorruptByte(ROM, corruptionType.Value, randomIndex, (int)Program.Form.LogicEngineFrame.ValueBitwise.Value);
                }
            }

            return ROM;
        }

        public static byte[]? ProcessManualEngine(byte[] ROM, int StartByte, int EndByte, bool CorruptNthByte, int Intensity)
        {
            if (CorruptNthByte)
            {
                for (int i = StartByte; i <= EndByte; i += Intensity)
                {
                    ManualEngine.CorruptByte(ROM, i, StartByte, EndByte);
                }
            }
            else
            {
                for (int i = 0; i < Intensity; i++)
                {
                    long randomIndex = rnd.Next(StartByte, EndByte);
                    ManualEngine.CorruptByte(ROM, randomIndex, StartByte, EndByte);
                }
            }

            return ROM;
        }

        private static void UpdateLogicControls()
        {
            if (Program.Form.LogicEngineFrame.LogicRandomizeTypeCheckbox.Checked)
            {
                int randomIndex = rnd.Next(1, Program.Form.LogicEngineFrame.BitwiseComboBox.Items.Count - 1);
                Program.Form.LogicEngineFrame.BitwiseComboBox.SelectedIndex = randomIndex;
            }

            if (Program.Form.LogicEngineFrame.LogicRandomizeValueCheckBox.Checked)
            {
                int randomValue = rnd.Next((int)Program.Form.LogicEngineFrame.ValueBitwise.Minimum, (int)Program.Form.LogicEngineFrame.ValueBitwise.Maximum);
                Program.Form.LogicEngineFrame.ValueBitwise.Value = randomValue;
            }
        }
    }
}
