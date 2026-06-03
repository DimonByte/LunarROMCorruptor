using LunarROMCorruptor.Modules.CorruptionInternals.Engines;

namespace LunarROMCorruptor.Modules.CorruptionInternals
{
    internal class EngineProcessor
    {
        private static readonly Random rnd = new();
        public static byte[]? ProcessNightmareEngine(byte[] ROM, int StartByte, int EndByte, bool CorruptNthByte, int Intensity)
        {
            var corruptionType = EngineParser.ParseCorruptionOptions(Program.Form.CorruptionEngineFrame.NightmareComboBox.Text);
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

        public static byte[]? ProcessMergeEngine(byte[] ROM, int StartByte, int EndByte, bool CorruptNthByte, int Intensity)
        {
            if (string.IsNullOrEmpty(Program.Form.CorruptionEngineFrame.MergeFileLocationTxt.Text))
            {
                TraceLogger.Log("Merge file location is empty. Please select a merge file. Aborting merge corruption.", StatusSeverityType.Error, true);
                return null;
            }

            byte[] ROMmerge = File.ReadAllBytes(Program.Form.CorruptionEngineFrame.MergeFileLocationTxt.Text);

            var corruptionType = EngineParser.ParseCorruptionOptions(Program.Form.CorruptionEngineFrame.CorrTypeMerge.Text);
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
            var corruptionType = EngineParser.ParseCorruptionOptions(Program.Form.CorruptionEngineFrame.BitwiseComboBox.Text);
            if (corruptionType == null) return null;

            if (CorruptNthByte)
            {
                for (int i = StartByte; i <= EndByte; i += Intensity)
                {
                    UpdateLogicControls();
                    LogicEngine.CorruptByte(ROM, corruptionType.Value, i, (int)Program.Form.CorruptionEngineFrame.ValueBitwise.Value);
                }
            }
            else
            {
                for (int i = 0; i < Intensity; i++)
                {
                    UpdateLogicControls();
                    int randomIndex = rnd.Next(StartByte, EndByte);
                    LogicEngine.CorruptByte(ROM, corruptionType.Value, randomIndex, (int)Program.Form.CorruptionEngineFrame.ValueBitwise.Value);
                }
            }

            return ROM;
        }

        public static byte[]? ProcessFractalEngine(byte[] ROM, int StartByte, int EndByte, bool CorruptNthByte, int Intensity)
        {
            if (CorruptNthByte)
            {
                for (int i = StartByte; i <= EndByte; i += Intensity)
                {
                    FractalEngine.CorruptByte(ROM, i);
                }
            }
            else
            {
                for (int i = 0; i < Intensity; i++)
                {
                    int randomIndex = rnd.Next(StartByte, EndByte);
                    FractalEngine.CorruptByte(ROM, randomIndex);
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
            if (Program.Form.CorruptionEngineFrame.LogicRandomizeTypeCheckbox.Checked)
            {
                int randomIndex = rnd.Next(1, Program.Form.CorruptionEngineFrame.BitwiseComboBox.Items.Count - 1);
                Program.Form.CorruptionEngineFrame.BitwiseComboBox.SelectedIndex = randomIndex;
            }

            if (Program.Form.CorruptionEngineFrame.LogicRandomizeValueCheckBox.Checked)
            {
                int randomValue = rnd.Next((int)Program.Form.CorruptionEngineFrame.ValueBitwise.Minimum, (int)Program.Form.CorruptionEngineFrame.ValueBitwise.Maximum);
                Program.Form.CorruptionEngineFrame.ValueBitwise.Value = randomValue;
            }
        }
    }
}
