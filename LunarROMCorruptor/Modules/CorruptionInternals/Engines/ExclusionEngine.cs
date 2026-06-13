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
    internal class ExclusionEngine
    {
        private static readonly Random rnd = new();
        public static readonly Dictionary<string, List<(long start, long end)>> SafeLocations = new()
        {
            ["NES"] =
            [
                (0x0000, 0x0017),   // iNES header - critical for loading
                (0x0018, 0x001B),   // PRG-ROM size, CHR-ROM size
                (0x001C, 0x001F),   // Mapper, Mirroring, Trainer info
            ],
            ["N64"] =
            [
                (0x00000000, 0x00000013),   // ROM header - critical for loading
                (0x00000014, 0x00000017),   // Bus speed, IPL ID
                (0x00000018, 0x00000023),   // ROM name and manufacturer data
                (0x00000024, 0x0000002B),   // Checksums - vital for integrity
            ],
            ["SNES"] =
            [
                (0x000000, 0x000007),   // SNES header - absolutely critical 
                (0x000008, 0x00000F),   // ROM size and hardware info
                (0x000010, 0x000013),   // SRAM size and version
            ],
            ["EXE"] =
            [
                (0x00000000, 0x0000003F),   // DOS header - critical for Windows loading
                (0x00000040, 0x000001FF),   // PE header structure 
                (0x00000200, 0x000003FF),   // Optional header - contains execution info
            ],
            ["GAMECUBE"] =
            [
                (0x00000000, 0x00000040),   // IOS header - critical for launch
                (0x00000040, 0x00000043),   // Entry point address - vital
                (0x00000080, 0x000000BF),   // Disc header and metadata
            ],
            ["PS2"] =
            [
                (0x00000000, 0x0000003F),   // PS2 disc header - vital for loading
                (0x00000040, 0x0000007F),   // ELF header structure 
                (0x00000080, 0x000000FF),   // Program header info
            ],
            ["PS1"] =
            [
                (0x00000000, 0x0000000F),   // CD-ROM header - critical
                (0x00000010, 0x0000001B),   // Boot sector information
                (0x00000020, 0x0000003F),   // Disc ID and system parameters
            ],
            ["SEGA"] =
            [
                (0x00000000, 0x0000003F),   // Sega header information - critical  
                (0x00000040, 0x00000047),   // ROM size and hardware info
            ],
            ["GAMEBOY"] =
            [
                (0x00000000, 0x0000000F),   // GB header - critical for loading
                (0x00000010, 0x00000013),   // Entry point address and parameters
            ],
            ["WII"] =
            [
                (0x00000000, 0x0000003F),   // Wii boot header - critical for launch  
                (0x00000040, 0x0000007F),   // Entry point and system configuration
            ],
            ["DREAMCAST"] =
            [
                (0x00000000, 0x0000003F),   // Dreamcast disc header - critical for loading
                (0x00000040, 0x0000007F),   // Boot code information and entry points
            ],
            ["BIOS"] =
            [
                (0x00000000, 0x000001FF),   // BIOS header - critical for boot process
                (0x00000200, 0x000003FF),   // Boot code and startup info 
            ],
        };

        /// <summary>
        /// Corrupts a byte at given position with safe location checks
        /// </summary>
        public static byte[] CorruptByte(byte[] ROM, CorruptionOptions CorruptOption, string Profile, long i)
        {
            if (i < 0 || i >= ROM.Length)
                return ROM;

            var safeLocations = SafeLocations.GetValueOrDefault(Profile, new List<(long, long)>());

            if (IsSafeLocation(i, safeLocations))
            {
                return ROM;
            }

            switch (CorruptOption)
            {
                case CorruptionOptions.RANDOM:
                    byte newValue = (byte)new Random().Next(0, 256);

                    if (ShouldAvoidValue(newValue))
                        newValue = GetAlternativeValue(newValue);

                    ROM[i] = newValue;
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                    break;

                case CorruptionOptions.RANDOMTILT:
                    switch (new Random().Next(0, 3))
                    {
                        case 0:
                            ROM[i] = (byte)new Random().Next(0, 256);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;
                        case 1:
                            ROM[i] = CorruptionCore.ClampByte(ROM[i] + (int)Program.Form.ExclusionEngineFrame.IncreDecrenumbnightmare.Value);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;
                        case 2:
                            ROM[i] = CorruptionCore.ClampByte(ROM[i] - (int)Program.Form.ExclusionEngineFrame.IncreDecrenumbnightmare.Value);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;
                        default:
                            throw new InvalidOperationException("Unexpected random tilt result");
                    }
                    break;

                case CorruptionOptions.TILT:
                    switch (new Random().Next(0, 2))
                    {
                        case 0:
                            ROM[i] = CorruptionCore.ClampByte(ROM[i] + (int)Program.Form.ExclusionEngineFrame.IncreDecrenumbnightmare.Value);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;
                        case 1:
                            ROM[i] = CorruptionCore.ClampByte(ROM[i] - (int)Program.Form.ExclusionEngineFrame.IncreDecrenumbnightmare.Value);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;
                    }
                    break;

                default:
                    throw new InvalidOperationException("Unknown corruption option");
            }

            return ROM;
        }

        /// <summary>
        /// Checks if an address falls within any safe region
        /// </summary>
        public static bool IsSafeLocation(long position, List<(long start, long end)> safeLocations)
        {
            foreach (var (start, end) in safeLocations)
            {
                if (start <= end && position >= start && position <= end)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if a corruption value should be avoided
        /// </summary>
        private static bool ShouldAvoidValue(byte value)
        {
            return value == 0x00 || value == 0xFF ||
                 (value >= 0xF0 && value <= 0xFF); // High crash potential bytes
        }

        /// <summary>
        /// Returns an alternative value when avoiding problematic ones
        /// </summary>
        private static byte GetAlternativeValue(byte currentValue)
        {
            byte[] alternativeValues = [
                0x7F, 0x80, 0x42, 0x69, 0xA5, 0xCD, 0x13, 0x88,
                0x2A, 0x76, 0x65, 0x8C, 0x7F, 0x7F
            ];

            return alternativeValues[new Random().Next(alternativeValues.Length)];
        }
    }
}