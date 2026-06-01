namespace LunarROMCorruptor.Modules
{
    public enum CorruptionEngineType
    {
        MergeEngine,
        LogicEngine,
        NightmareEngine,
        LerpEngine,
        Vector2Engine,
        ManualEngine
    }
    internal class EngineEnums
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
                CorruptionEngineType.Vector2Engine => "Vector2 Engine",
                CorruptionEngineType.ManualEngine => "Manual Engine",
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
                "Vector2 Engine" => CorruptionEngineType.Vector2Engine,
                "Manual Engine" => CorruptionEngineType.ManualEngine,
                _ => CorruptionEngineType.NightmareEngine // default fallback
            };
        }
    }
}
