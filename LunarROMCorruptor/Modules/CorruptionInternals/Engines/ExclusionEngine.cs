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
        // Safe byte locations for each console/profile - these are ranges (start, end)
        // Note: these maaaaay be wrong but I tried. From what I can gather these are at least some of the locations that are indeed critical.
        public static readonly Dictionary<string, List<(long start, long end)>> SafeLocations = new()
        {
            ["NES"] =
            [
                (0x0000, 0x07FF),   // RAM
                (0x2000, 0x2007),   // PPU registers
                (0x4000, 0x4017),   // APU and I/O registers
                (0x4018, 0x401F),   // Expansion RAM and control registers
            ],
            ["N64"] =
            [
                (0x00000000, 0x00000FFF), // RDRAM
                (0x04000000, 0x040000FF), // PI registers
                (0x04000100, 0x040001FF), // SI registers
                (0x04000200, 0x040002FF), // AI registers
                (0x04000300, 0x040003FF), // VI registers
                (0x04000400, 0x040004FF), // PI registers - repeated but important
                (0x04000500, 0x040005FF), // RI registers
                (0x04000600, 0x040006FF), // AI registers - repeated
                (0x04001000, 0x04001FFF), // SP registers 
                (0x04002000, 0x04002FFF), // DP registers
                (0x08000000, 0x08000FFF), // Cartridge ROM (critical)
                (0x10000000, 0x1000003F), // Boot code
            ],
            ["SNES"] =
            [
                (0x000000, 0x001FFF),   // WRAM
                (0x002000, 0x003FFF),   // WRAM mirror
                (0x004000, 0x007FFF),   // WRAM
                (0x008000, 0x00FFFF),   // WRAM mirror
                (0x4000, 0x401F),       // CPU registers
                (0x00200000, 0x003FFFFF), // High WRAM
                (0xFF00, 0xFFFF),       // CPU internal registers  
            ],
            ["EXE"] =
            [
                (0x00000000, 0x0000003F),   // DOS header
                (0x00000040, 0x000001FF),   // PE header
                (0x00000200, 0x000003FF),   // Optional header
                (0x00000400, 0x000004FF),   // Section headers
                (0x00000500, 0x000005FF),   // Import table
            ],
            ["GAMECUBE"] =
            [
                (0x00000000, 0x0000FFFF), // System RAM
                (0x01000000, 0x01FFFFFF), // Game RAM
                (0x03000000, 0x04000000), // I/O Registers
                (0x05000000, 0x05000FFF), // Hardware registers 
            ],
            ["PS2"] =
            [
                (0x00000000, 0x0000FFFF), // RAM
                (0x10000000, 0x100003FF), // SPU registers
                (0x10001000, 0x100013FF), // GPU registers
                (0x10002000, 0x100023FF), // I/O registers
                (0x10003000, 0x100033FF), // Timer register
                (0x10004000, 0x100043FF), // DMA registers
            ],
            ["PS1"] =
            [
                (0x00000000, 0x0000FFFF), // RAM
                (0x1F800000, 0x1F8003FF), // Control registers
                (0x1F801000, 0x1F8013FF), // Display registers
                (0x1F801400, 0x1F8017FF), // SPU registers
                (0x1F802000, 0x1F8023FF), // CD-ROM registers
            ],
            ["SEGA"] =
            [
                (0x00000000, 0x0000FFFF), // RAM
                (0x00002000, 0x000027FF), // VDP registers 
                (0x00004000, 0x000043FF), // I/O registers
                (0x00008000, 0x00008001), // Video RAM address
            ],
            ["GAMEBOY"] =
            [
                (0x00000000, 0x00007FFF), // RAM
                (0x00008000, 0x00009FFF), // Video RAM
                (0x0000A000, 0x0000BFFF), // External RAM
                (0x0000C000, 0x0000DFFF), // Work RAM
                (0x0000E000, 0x0000FDFF), // Echo RAM
                (0x0000FE00, 0x0000FE9F), // OAM
                (0x0000FF00, 0x0000FFFF), // I/O registers
                (0x00010000, 0x00011FFF), // Video RAM (second bank)
            ],
            ["WII"] =
            [
                (0x00000000, 0x01FFFFFF), // System RAM
                (0x03000000, 0x04000000), // I/O registers
                (0x06000000, 0x06FFFFFF), // Game RAM
                (0x10000000, 0x10FF0000), // Boot ROM
            ],
            ["DREAMCAST"] =
            [
                (0x00000000, 0x007FFFFF), // RAM
                (0x00800000, 0x00803FFF), // VDP registers
                (0x00804000, 0x00807FFF), // I/O registers
                (0x00A00000, 0x00A0FFFF), // BIOS ROM
                (0x00C00000, 0x00DFFFFF), // Expansion RAM
            ],
            ["BIOS"] =
            [
                (0x00000000, 0x00000FFF), // Bootstrap code
                (0x00001000, 0x00001FFF), // Configuration data
                (0x00002000, 0x00003FFF), // System registers
                (0x00004000, 0x00005FFF), // Initialization routines
            ],
        };

        /// <summary>
        /// Corrupts a byte at given position with intelligent value selection
        /// </summary>
        public static byte[] CorruptByte(byte[] ROM, CorruptionOptions CorruptOption, string Profile, long i)
        {
            if (i < 0 || i >= ROM.Length)
                return ROM;

            var safeLocations = SafeLocations.GetValueOrDefault(Profile, new List<(long, long)>());

            byte originalValue = ROM[i];

            switch (CorruptOption)
            {
                case CorruptionOptions.RANDOM:
                    // Improved random corruption with intelligent value selection
                    byte newValue = (byte)new Random().Next(0, 256);

                    // Avoid values that commonly cause crashes or invalid states
                    if (ShouldAvoidValue(newValue))
                        newValue = GetAlternativeValue(newValue);

                    ROM[i] = newValue;
                    Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                    break;

                case CorruptionOptions.RANDOMTILT:
                    // Random tilt with better handling of edge cases
                    switch (new Random().Next(0, 3))
                    {
                        case 0:
                            ROM[i] = (byte)new Random().Next(0, 256);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;
                        case 1:
                            ROM[i] = ClampByte(ROM[i] + (int)Program.Form.ExclusionEngineFrame.IncreDecrenumbnightmare.Value);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;
                        case 2:
                            ROM[i] = ClampByte(ROM[i] - (int)Program.Form.ExclusionEngineFrame.IncreDecrenumbnightmare.Value);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;
                        default:
                            throw new InvalidOperationException("Unexpected random tilt result");
                    }
                    break;

                case CorruptionOptions.TILT:
                    // Improved tilt functionality with more interesting effects
                    switch (new Random().Next(0, 2))
                    {
                        case 0:
                            ROM[i] = ClampByte(ROM[i] + (int)Program.Form.ExclusionEngineFrame.IncreDecrenumbnightmare.Value);
                            Program.Form.InternalStashItems.Add("[x] File(" + i + ").SET(" + ROM[i] + ")");
                            break;
                        case 1:
                            ROM[i] = ClampByte(ROM[i] - (int)Program.Form.ExclusionEngineFrame.IncreDecrenumbnightmare.Value);
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
                // Validate that range makes sense before checking
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
            // Avoid common crash-inducing values
            return value == 0x00 || value == 0xFF ||
                 (value >= 0xF0 && value <= 0xFF); // High crash potential bytes
        }

        /// <summary>
        /// Returns an alternative value when avoiding problematic ones
        /// </summary>
        private static byte GetAlternativeValue(byte currentValue)
        {
            byte[] alternativeValues = {
                0x7F, 0x80, 0x42, 0x69, 0xA5, 0xCD, 0x13, 0x88,
                0x2A, 0x76, 0x65, 0x8C, 0x7F, 0x7F
            };

            // Simple approach: return a random alternative from our list
            return alternativeValues[new Random().Next(alternativeValues.Length)];
        }

        /// <summary>
        /// Ensures byte value stays within valid range (0-255)
        /// </summary>
        private static byte ClampByte(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }
    }
}
