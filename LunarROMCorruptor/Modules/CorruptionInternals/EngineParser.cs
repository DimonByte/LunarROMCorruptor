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
    public enum CorruptionEngineType
    {
        MergeEngine,
        LogicEngine,
        NightmareEngine,
        LerpEngine,
        VectorEngine,
        ManualEngine,
        ExclusionEngine,
        INVALID
    }
    internal class EngineParser
    {
        // Helper method to convert enum to display text
        public static string GetEngineDisplayName(CorruptionEngineType engineType)
        {
            return engineType switch
            {
                CorruptionEngineType.MergeEngine => "Merge Engine",
                CorruptionEngineType.LogicEngine => "Logic Engine",
                CorruptionEngineType.NightmareEngine => "Nightmare Engine",
                CorruptionEngineType.LerpEngine => "Lerp Engine",
                CorruptionEngineType.VectorEngine => "Vector2 Engine",
                CorruptionEngineType.ManualEngine => "Manual Engine",
                CorruptionEngineType.ExclusionEngine => "Exclusion Engine",
                _ => engineType.ToString()
            };
        }

        // Helper method to convert display text back to enum
        public static CorruptionEngineType ParseEngineType(string displayName)
        {
            return displayName switch
            {
                "Merge Engine" => CorruptionEngineType.MergeEngine,
                "Logic Engine" => CorruptionEngineType.LogicEngine,
                "Nightmare Engine" => CorruptionEngineType.NightmareEngine,
                "Lerp Engine" => CorruptionEngineType.LerpEngine,
                "Vector2 Engine" => CorruptionEngineType.VectorEngine,
                "Manual Engine" => CorruptionEngineType.ManualEngine,
                "Exclusion Engine" => CorruptionEngineType.ExclusionEngine,
                _ => CorruptionEngineType.INVALID // default fallback
            };
        }

        public static CorruptionOptions? ParseCorruptionOptions(string comboBoxText)
        {
            if (Enum.TryParse(comboBoxText, out CorruptionOptions corruptionType))
            {
                return corruptionType;
            }

            TraceLogger.Log($"Failed to parse corruption type from combo box text: '{comboBoxText}'", StatusSeverityType.Error, true);
            return null;
        }
    }
}
