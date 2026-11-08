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
    /// <summary>
    /// Pointer Engine: finds pointers (addresses stored in the data) and swaps, shifts or duplicates them. Instead of breaking code, the game
    /// ends up loading a different texture, model, sound, animation or text than it asked for. Pointers in tables (next to other pointers)
    /// usually point to the same kind of asset, so swapping inside a table gives the most stable results.
    /// </summary>
    public class PointerEngine
    {
        //The combobox in PointerEngineControl lists these in the same order, add new profiles before FileOffsetsLE and update the combobox too
        public enum PointerProfile
        {
            Auto,
            N64Ram, //0x80000000 - 0x807FFFFF, big-endian
            N64DisplayLists, //Segmented addresses in F3D/F3DEX/F3DEX2 display list commands (textures, vertices, sub display lists)
            GbaRom, //0x08000000 - 0x09FFFFFF, little-endian, points into the ROM file
            NdsRam, //0x02000000 - 0x023FFFFF, little-endian
            Ps1Ram, //0x80000000 - 0x801FFFFF, little-endian
            GameCubeWiiRam, //0x80000000 - 0x817FFFFF, big-endian
            Nes16, //16-bit little-endian CPU addresses 0x8000 - 0xFFFF (PRG-ROM)
            Snes24, //24-bit little-endian long addresses (bank:address), LoROM/HiROM mapped into the ROM file
            GameBoy16, //16-bit little-endian addresses 0x0150 - 0x7FFF (ROM bank 0 + switchable bank)
            Genesis, //32-bit big-endian ROM addresses 0x000200 - 0x3FFFFF (= file offsets)
            SaturnRam, //32-bit big-endian high work RAM 0x06000000 - 0x060FFFFF
            DreamcastRam, //32-bit little-endian main RAM 0x8C010000 - 0x8CFFFFFF
            Ps2Ram, //32-bit little-endian EE RAM 0x00100000 - 0x01FFFFFF
            PspRam, //32-bit little-endian user RAM 0x08804000 - 0x09FFFFFF
            WindowsPe32, //32-bit Windows EXE/DLL: virtual addresses ImageBase .. ImageBase + SizeOfImage (read from the PE header)
            FileOffsetsLE, //32-bit offsets into the file itself
            FileOffsetsBE,
            Custom
        }

        public enum PointerMode
        {
            SWAP, //Swap two pointers (from the same table when possible): asset A shows asset B and the other way around
            SHIFT, //Move a pointer forwards/backwards a few bytes: shifted/garbled textures, offset animations, broken palettes
            DUPLICATE, //Copy a neighbouring pointer: two table entries use the same asset
            RANDOMTARGET //Point at the target of any other pointer of the same kind (wilder, crashes more often)
        }

        [Flags]
        public enum DisplayListTargets
        {
            None = 0,
            Textures = 1, //G_SETTIMG
            Vertices = 2, //G_VTX
            DisplayLists = 4, //G_DL (sub display lists: whole parts of a model)
            All = Textures | Vertices | DisplayLists
        }

        public sealed class Settings
        {
            public PointerProfile Profile = PointerProfile.Auto;
            public PointerMode Mode = PointerMode.SWAP;
            public int ShiftAmount = 64; //Maximum shift in bytes (SHIFT mode)
            public int TableWindow = 64; //SWAP/DUPLICATE look for a partner pointer within this many bytes
            public bool TablesOnly = true; //Only pointers that have another pointer right next to them
            public bool SkipFunctionPointers = true; //Skip pointers to code (N64 boot segment functions, GBA/DS Thumb pointers, PE code sections)
            public DisplayListTargets DisplayListTargets = DisplayListTargets.Textures;
            public uint CustomMin = 0x80000000;
            public uint CustomMax = 0x807FFFFF;
            public bool CustomBigEndian = true;
        }

        /// <summary>
        /// Short description of each profile (shown in the UI and the docs).
        /// </summary>
        public static string Describe(PointerProfile p) => p switch
        {
            PointerProfile.Auto => "Picks the profile from the file header (N64, GBA, DS, PS1, GC/Wii, NES, SNES, GB, Genesis, Saturn, Dreamcast, PS2, PSP, Windows EXE), otherwise little-endian file offsets.",
            PointerProfile.N64Ram => "N64: 32-bit big-endian RAM addresses (0x80000400 - 0x807FFFFF). .z64/.v64/.n64 byte order is detected.",
            PointerProfile.N64DisplayLists => "N64: segmented texture/vertex/display list addresses inside F3D/F3DEX2 model display lists.",
            PointerProfile.GbaRom => "GBA: 32-bit little-endian ROM addresses (0x08000000+) that point into the ROM file.",
            PointerProfile.NdsRam => "DS: 32-bit little-endian main RAM addresses (0x02000000 - 0x023FFFFF).",
            PointerProfile.Ps1Ram => "PS1: 32-bit little-endian RAM addresses (0x80010000 - 0x801FFFFF).",
            PointerProfile.GameCubeWiiRam => "GameCube/Wii: 32-bit big-endian MEM1 addresses (0x80003100 - 0x817FFFFF).",
            PointerProfile.Nes16 => "NES: 16-bit little-endian CPU addresses (0x8000 - 0xFFFF) in tables of 4+ pointers. Only PRG-ROM is scanned.",
            PointerProfile.Snes24 => "SNES: 24-bit little-endian long addresses (bank + address) that map into the ROM (LoROM/HiROM detected, copier header skipped).",
            PointerProfile.GameBoy16 => "Game Boy / Color: 16-bit little-endian addresses (0x0150 - 0x7FFF) in tables of 4+ pointers.",
            PointerProfile.Genesis => "Genesis / Mega Drive: 32-bit big-endian ROM addresses (0x200 - 0x3FFFFF, the same as file offsets).",
            PointerProfile.SaturnRam => "Saturn: 32-bit big-endian high work RAM addresses (0x06000000 - 0x060FFFFF).",
            PointerProfile.DreamcastRam => "Dreamcast: 32-bit little-endian main RAM addresses (0x8C010000 - 0x8CFFFFFF).",
            PointerProfile.Ps2Ram => "PS2: 32-bit little-endian EE RAM addresses (0x00100000 - 0x01FFFFFF). Use it on ELFs/files extracted from the disc.",
            PointerProfile.PspRam => "PSP: 32-bit little-endian user RAM addresses (0x08804000 - 0x09FFFFFF). Use it on EBOOT/PRX files.",
            PointerProfile.WindowsPe32 => "Windows 32-bit EXE/DLL: addresses between ImageBase and ImageBase + SizeOfImage (from the PE header). Pointers into code sections count as function pointers.",
            PointerProfile.FileOffsetsLE => "Any file: 32-bit little-endian numbers that are valid offsets into the file (0x100+).",
            PointerProfile.FileOffsetsBE => "Any file: 32-bit big-endian numbers that are valid offsets into the file (0x100+).",
            _ => "Custom: 32-bit values in the hex range below, with the byte order from the checkbox."
        };

        private enum WordOrder { Little, Big, ByteSwapped16 }

        private readonly record struct Candidate(long Offset, uint Value, byte Kind);

        /// <summary>
        /// Everything needed to recognise a pointer for the resolved profile.
        /// </summary>
        private sealed class Rules
        {
            public PointerProfile Profile;
            public WordOrder Order;
            public int Size = 4; //Pointer size in bytes (2, 3 or 4)
            public uint Min, Max;
            public uint Alignment = 4; //Pointer values must be a multiple of this
            public int ScanStep = 4; //Offsets that are checked: multiples of this (1 = every byte)
            public int MinRun = 2; //"Only pointers in tables": at least this many pointers in a row
            public uint MaxSpread = uint.MaxValue; //Pointers in one table can't be further apart than this (small pointers only, filters noise)
            public long FileBase = -1; //Pointer value of file offset 0 (GBA ROM / file offsets), -1 when pointers don't point into the file
            public Func<uint, long>? MapToFile; //Maps a pointer to a file offset (-1 = not in the file); the target has to be inside the file
            public long ScanStart = 0, ScanEnd = long.MaxValue; //Part of the file that can contain pointers (e.g. NES PRG-ROM, skips headers)
            public List<(uint Start, uint End, bool Code)>? Sections; //PE: valid address ranges and whether they are code
            public uint N64EntryPoint; //RAM address of ROM offset 0x1000 (to detect function pointers into the N64 boot segment)
        }

        public static void Run(CorruptionContext ctx, Settings s)
        {
            var data = ctx.Data!;
            var rules = Resolve(data, s);
            TraceLogger.Log($"Pointer Engine profile: {rules.Profile} ({rules.Size * 8}-bit {rules.Order}{(rules.Profile == PointerProfile.N64DisplayLists ? $", {s.DisplayListTargets}" : $", 0x{rules.Min:X} - 0x{rules.Max:X}")}).");
            var candidates = rules.Profile == PointerProfile.N64DisplayLists ? FindDisplayListPointers(ctx, rules, s) : FindPointers(ctx, rules, s);
            if (candidates.Count == 0)
            {
                TraceLogger.Log("Pointer Engine didn't find any pointers in the selected range. Try another profile, a wider range or disable 'Only pointers in tables'. (Compressed data has to be corrupted with the Compression Engine.)", StatusSeverityType.Warning, true);
                return;
            }
            TraceLogger.Log($"Pointer Engine found {candidates.Count:N0} pointer(s).");

            //Live values: a pointer can be changed more than once
            var values = candidates.Select(c => c.Value).ToArray();
            long changed = 0, operations = ctx.EveryNth ? (candidates.Count - 1) / ctx.Intensity + 1 : ctx.Intensity;
            for (long n = 0; n < operations; n++)
            {
                int a = ctx.EveryNth ? (int)(n * ctx.Intensity) : ctx.Rnd.Next(candidates.Count);
                if (a >= candidates.Count) break;
                if (Apply(ctx, s, rules, candidates, values, a)) changed++;
                if ((n + 1) % 4096 == 0) ctx.Tick(n + 1, operations);
            }
            ctx.Tick(operations, operations);
            TraceLogger.Log($"Pointer Engine changed {changed:N0} pointer(s) ({s.Mode}).");
        }

        #region Profiles

        private static bool HasMagic(FileBuffer d, long offset, string magic)
        {
            if (offset < 0 || offset + magic.Length > d.Length) return false;
            for (int i = 0; i < magic.Length; i++)
                if (d[offset + i] != (byte)magic[i]) return false;
            return true;
        }

        /// <summary>
        /// Picks the pointer profile that fits the file (used by Auto).
        /// </summary>
        public static PointerProfile DetectProfile(FileBuffer d)
        {
            string? detected = ExclusionEngine.DetectProfile(d.FilePath, d);
            string ext = Path.GetExtension(d.FilePath ?? "").ToLowerInvariant();
            //PSP UMD images are ISO9660 with "PSP GAME" as the system identifier
            if (HasMagic(d, 0x8008, "PSP GAME")) return PointerProfile.PspRam;
            if (ext == ".prx" || (ext == ".bin" && HasMagic(d, 0, "~PSP"))) return PointerProfile.PspRam;
            return detected switch
            {
                "N64" => PointerProfile.N64Ram,
                "GBA" => PointerProfile.GbaRom,
                "NDS" => PointerProfile.NdsRam,
                "PS1" => PointerProfile.Ps1Ram,
                "GAMECUBE" or "WII" => PointerProfile.GameCubeWiiRam,
                "NES" => PointerProfile.Nes16,
                "SNES" => PointerProfile.Snes24,
                "GAMEBOY" => PointerProfile.GameBoy16,
                "SEGA" => PointerProfile.Genesis,
                //Sega CD discs share the exclusion type with Saturn, but they're 68000 code with different RAM: use file offsets for them
                "SEGA CD/SATURN" when ExclusionEngine.DetectSegaDisc(d) == ExclusionEngine.SegaDisc.Saturn => PointerProfile.SaturnRam,
                "DREAMCAST" => PointerProfile.DreamcastRam,
                "PS2" => PointerProfile.Ps2Ram,
                "EXE" when ReadPe32(d) != null => PointerProfile.WindowsPe32,
                _ => PointerProfile.FileOffsetsLE
            };
        }

        private sealed record PeInfo(uint ImageBase, uint SizeOfImage, uint SizeOfHeaders, List<(uint Start, uint End, bool Code)> Sections);

        /// <summary>
        /// Reads ImageBase/SizeOfImage and the section table of a 32-bit PE file. Returns null for anything else (64-bit PE has 64-bit pointers).
        /// </summary>
        private static PeInfo? ReadPe32(FileBuffer d)
        {
            try
            {
                if (d.Length < 0x100 || !HasMagic(d, 0, "MZ")) return null;
                long pe = d.ReadUInt32(0x3C, false);
                if (pe <= 0 || pe + 0xF8 > d.Length || !HasMagic(d, pe, "PE\0\0")) return null;
                int sectionCount = d.ReadUInt16(pe + 6, false);
                int optSize = d.ReadUInt16(pe + 20, false);
                long opt = pe + 24;
                if (d.ReadUInt16(opt, false) != 0x10B) return null; //PE32 only
                uint imageBase = d.ReadUInt32(opt + 28, false);
                uint sizeOfImage = d.ReadUInt32(opt + 56, false);
                uint sizeOfHeaders = d.ReadUInt32(opt + 60, false);
                var sections = new List<(uint, uint, bool)>();
                long sec = opt + optSize;
                for (int k = 0; k < sectionCount && sec + 40 <= d.Length; k++, sec += 40)
                {
                    uint virtualSize = d.ReadUInt32(sec + 8, false), rva = d.ReadUInt32(sec + 12, false), rawSize = d.ReadUInt32(sec + 16, false);
                    uint characteristics = d.ReadUInt32(sec + 36, false);
                    uint size = Math.Max(virtualSize, rawSize);
                    if (size == 0) continue;
                    bool code = (characteristics & 0x20000020) != 0; //IMAGE_SCN_CNT_CODE / IMAGE_SCN_MEM_EXECUTE
                    sections.Add((imageBase + rva, imageBase + rva + size - 1, code));
                }
                return sizeOfImage == 0 ? null : new PeInfo(imageBase, sizeOfImage, sizeOfHeaders, sections);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// SNES: works out LoROM vs HiROM from the internal header (checksum + complement pair and map mode byte) and the copier header.
        /// </summary>
        private static (bool HiRom, long CopierHeader) DetectSnesMapping(FileBuffer d)
        {
            long copier = d.Length % 1024 == 512 ? 512 : 0;
            int Score(long header)
            {
                if (header + 0x40 > d.Length) return -1;
                int score = 0;
                ushort complement = d.ReadUInt16(header + 0x1C, false), checksum = d.ReadUInt16(header + 0x1E, false);
                if ((ushort)(complement ^ checksum) == 0xFFFF) score += 4;
                byte map = d[header + 0x15];
                if ((map & 0xE0) == 0x20) score += 2;
                if ((map & 0x01) == (header - copier == 0xFFC0 ? 1 : 0)) score += 1;
                ushort reset = d.ReadUInt16(header + 0x3C, false);
                if (reset >= 0x8000) score += 1;
                return score;
            }
            int lo = Score(copier + 0x7FC0), hi = Score(copier + 0xFFC0);
            return (hi > lo, copier);
        }

        private static Rules Resolve(FileBuffer d, Settings s)
        {
            var profile = s.Profile;
            var n64 = N64Rom.DetectByteOrder(d);
            if (profile == PointerProfile.Auto)
                profile = DetectProfile(d);
            var rules = new Rules { Profile = profile };
            switch (profile)
            {
                case PointerProfile.N64Ram:
                case PointerProfile.N64DisplayLists:
                    rules.Order = n64 switch
                    {
                        N64Rom.ByteOrder.ByteSwapped => WordOrder.ByteSwapped16,
                        N64Rom.ByteOrder.LittleEndian => WordOrder.Little,
                        _ => WordOrder.Big
                    };
                    rules.Min = 0x80000400;
                    rules.Max = 0x807FFFFF;
                    if (n64 != null) rules.N64EntryPoint = ReadWord(d, 8, rules.Order);
                    break;
                case PointerProfile.GbaRom:
                    rules.Order = WordOrder.Little;
                    rules.Min = 0x08000000;
                    rules.Max = (uint)Math.Min(0x09FFFFFFL, 0x08000000L + d.Length - 1);
                    rules.FileBase = 0x08000000;
                    rules.ScanStart = 0xC0;
                    break;
                case PointerProfile.NdsRam:
                    rules.Order = WordOrder.Little;
                    rules.Min = 0x02000000;
                    rules.Max = 0x023FFFFF;
                    break;
                case PointerProfile.Ps1Ram:
                    rules.Order = WordOrder.Little;
                    rules.Min = 0x80010000;
                    rules.Max = 0x801FFFFF;
                    break;
                case PointerProfile.GameCubeWiiRam:
                    rules.Order = WordOrder.Big;
                    rules.Min = 0x80003100;
                    rules.Max = 0x817FFFFF;
                    break;
                case PointerProfile.Nes16:
                    rules.Order = WordOrder.Little;
                    rules.Size = 2;
                    rules.Min = 0x8000;
                    rules.Max = 0xFFF9; //0xFFFA-0xFFFF are the vectors themselves
                    rules.Alignment = 1;
                    rules.ScanStep = 1;
                    rules.MinRun = 4;
                    rules.MaxSpread = 0x1000; //Entries of one table point close to each other (same bank)
                    if (HasMagic(d, 0, "NES\x1A"))
                    {
                        //Only PRG-ROM can hold pointers, CHR-ROM is tile graphics
                        long prgStart = 16 + ((d[6] & 0x04) != 0 ? 512 : 0);
                        rules.ScanStart = prgStart;
                        rules.ScanEnd = prgStart + d[4] * 16384L - 7; //Leave the NMI/RESET/IRQ vectors alone
                    }
                    break;
                case PointerProfile.Snes24:
                    {
                        rules.Order = WordOrder.Little;
                        rules.Size = 3;
                        rules.Alignment = 1;
                        rules.ScanStep = 1;
                        rules.MinRun = 3;
                        rules.MaxSpread = 0x20000; //Entries of one table point into the same or neighbouring banks
                        rules.Min = 0x000000;
                        rules.Max = 0xFFFFFF;
                        var (hiRom, copier) = DetectSnesMapping(d);
                        long romSize = d.Length - copier;
                        rules.ScanStart = copier;
                        rules.MapToFile = v =>
                        {
                            uint bank = v >> 16, addr = v & 0xFFFF;
                            if (bank is 0x7E or 0x7F) return -1; //WRAM
                            long offset;
                            if (hiRom)
                            {
                                if (bank >= 0xC0 || bank is >= 0x40 and < 0x7E) offset = ((bank & 0x3F) << 16) | addr;
                                else if (addr >= 0x8000) offset = ((bank & 0x3F) << 16) | addr; //Banks 00-3F/80-BF mirror the upper half
                                else return -1;
                            }
                            else
                            {
                                if (addr < 0x8000) return -1; //Lower halves are RAM/registers/SRAM
                                offset = ((bank & 0x7F) * 0x8000L) + (addr - 0x8000);
                            }
                            return offset < romSize ? offset + copier : -1;
                        };
                        TraceLogger.Log($"Pointer Engine: SNES {(hiRom ? "HiROM" : "LoROM")} mapping{(copier > 0 ? " (512 byte copier header skipped)" : "")}.");
                    }
                    break;
                case PointerProfile.GameBoy16:
                    rules.Order = WordOrder.Little;
                    rules.Size = 2;
                    rules.Min = 0x0150;
                    rules.Max = 0x7FFF;
                    rules.Alignment = 1;
                    rules.ScanStep = 1;
                    rules.MinRun = 4;
                    rules.MaxSpread = 0x1000;
                    rules.ScanStart = 0x150; //Vectors + cartridge header
                    break;
                case PointerProfile.Genesis:
                    rules.Order = WordOrder.Big;
                    rules.Min = 0x200;
                    rules.Max = (uint)Math.Min(0x3FFFFFL, d.Length - 1);
                    rules.Alignment = 2;
                    rules.ScanStep = 2; //68000 longs are word aligned
                    rules.FileBase = 0;
                    rules.ScanStart = 0x200; //Vector table + ROM header
                    break;
                case PointerProfile.SaturnRam:
                    rules.Order = WordOrder.Big;
                    rules.Min = 0x06000000;
                    rules.Max = 0x060FFFFF;
                    break;
                case PointerProfile.DreamcastRam:
                    rules.Order = WordOrder.Little;
                    rules.Min = 0x8C010000;
                    rules.Max = 0x8CFFFFFF;
                    break;
                case PointerProfile.Ps2Ram:
                    rules.Order = WordOrder.Little;
                    rules.Min = 0x00100000;
                    rules.Max = 0x01FFFFFF;
                    break;
                case PointerProfile.PspRam:
                    rules.Order = WordOrder.Little;
                    rules.Min = 0x08804000;
                    rules.Max = 0x09FFFFFF;
                    break;
                case PointerProfile.WindowsPe32:
                    {
                        rules.Order = WordOrder.Little;
                        rules.Alignment = 1; //Pointers to strings don't have to be aligned
                        var pe = ReadPe32(d);
                        if (pe == null)
                        {
                            TraceLogger.Log("Pointer Engine: this isn't a 32-bit Windows EXE/DLL, using 32-bit little-endian file offsets instead.", StatusSeverityType.Warning);
                            profile = PointerProfile.FileOffsetsLE;
                            goto case PointerProfile.FileOffsetsLE;
                        }
                        rules.Min = pe.ImageBase + pe.SizeOfHeaders;
                        rules.Max = (uint)Math.Min(uint.MaxValue, (ulong)pe.ImageBase + pe.SizeOfImage - 1);
                        rules.Sections = pe.Sections;
                        rules.ScanStart = pe.SizeOfHeaders; //Never touch the PE headers/section table
                        TraceLogger.Log($"Pointer Engine: PE32 ImageBase 0x{pe.ImageBase:X8}, {pe.Sections.Count} section(s).");
                    }
                    break;
                case PointerProfile.FileOffsetsLE:
                case PointerProfile.FileOffsetsBE:
                    rules.Profile = profile;
                    rules.Order = profile == PointerProfile.FileOffsetsBE ? WordOrder.Big : WordOrder.Little;
                    rules.Alignment = 4;
                    rules.Min = 0x100; //Small numbers are counts/sizes far more often than offsets
                    rules.Max = (uint)Math.Min(uint.MaxValue, d.Length - 1);
                    rules.FileBase = 0;
                    break;
                default:
                    rules.Order = s.CustomBigEndian ? WordOrder.Big : WordOrder.Little;
                    rules.Min = Math.Min(s.CustomMin, s.CustomMax);
                    rules.Max = Math.Max(s.CustomMin, s.CustomMax);
                    break;
            }
            return rules;
        }

        #endregion

        #region Finding pointers

        private static uint ReadWord(FileBuffer d, long p, WordOrder order)
        {
            Span<byte> b = stackalloc byte[4];
            if (order == WordOrder.ByteSwapped16)
            {
                b[0] = d[p + 1]; b[1] = d[p]; b[2] = d[p + 3]; b[3] = d[p + 2];
            }
            else d.Read(p, b);
            return order == WordOrder.Little ? System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(b) : System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(b);
        }

        private static uint ReadValue(FileBuffer d, long p, Rules r)
        {
            if (r.Size == 4) return ReadWord(d, p, r.Order);
            uint v = 0;
            for (int k = 0; k < r.Size; k++)
            {
                uint b = d[p + k];
                v = r.Order == WordOrder.Big ? (v << 8) | b : v | (b << (8 * k));
            }
            return v;
        }

        private static bool WriteValue(CorruptionContext ctx, Rules r, long p, uint value)
        {
            if (ctx.IsExcluded(p, r.Size)) return false;
            Span<byte> b = stackalloc byte[4];
            if (r.Order == WordOrder.Little) System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(b, value);
            else System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(b, value << (8 * (4 - r.Size)));
            for (int k = 0; k < r.Size; k++)
            {
                long i = p + (r.Order == WordOrder.ByteSwapped16 ? k ^ 1 : k);
                if (ctx.Get(i) != b[k]) ctx.Set(i, b[k]);
            }
            return true;
        }

        /// <summary>
        /// True when a value can be a pointer of this profile (range, alignment, inside the file / a PE section, not a function pointer).
        /// </summary>
        private static bool LooksLikePointer(FileBuffer d, Rules r, Settings s, uint v)
        {
            if (v < r.Min || v > r.Max) return false;
            bool thumb = (v & 1) != 0 && r.Profile is PointerProfile.GbaRom or PointerProfile.NdsRam;
            if (thumb && s.SkipFunctionPointers) return false; //Thumb function pointer
            if (!thumb && (v & (r.Alignment - 1)) != 0) return false; //Thumb pointers are a halfword aligned address + 1
            if (r.FileBase >= 0 && v - r.FileBase >= d.Length) return false;
            if (r.MapToFile != null)
            {
                long target = r.MapToFile(v);
                if (target < 0 || target >= d.Length) return false;
            }
            if (r.Sections != null)
            {
                bool found = false;
                foreach (var (start, end, code) in r.Sections)
                {
                    if (v < start || v > end) continue;
                    if (code && s.SkipFunctionPointers) return false;
                    found = true;
                    break;
                }
                if (!found) return false;
            }
            if (s.SkipFunctionPointers && r.Profile == PointerProfile.N64Ram && r.N64EntryPoint != 0)
            {
                //Pointer into the boot segment (game code loaded at the entry point)? Skip it if it points at a function start (addiu sp, sp, -x)
                long rom = 0x1000L + ((long)v - r.N64EntryPoint);
                if (rom >= 0x1000 && rom < Math.Min(d.Length - 3, 0x101000L))
                {
                    uint w = ReadWord(d, rom, r.Order);
                    if ((w & 0xFFFF8000) == 0x27BD8000) return false;
                }
            }
            return true;
        }

        private static List<Candidate> FindPointers(CorruptionContext ctx, Rules r, Settings s)
        {
            var d = ctx.Data!;
            long first = Math.Max(ctx.Start, r.ScanStart), last = Math.Min(ctx.End, r.ScanEnd) - (r.Size - 1);
            if (N64Rom.DetectByteOrder(d) != null) first = Math.Max(first, N64Rom.BootCodeEnd); //Never touch the N64 header/boot code
            if (r.ScanStep > 1) first = (first + r.ScanStep - 1) / r.ScanStep * r.ScanStep;
            var list = new List<Candidate>();
            if (first > last) return list;
            int minRun = s.TablesOnly ? Math.Max(2, r.MinRun) : 1;

            //Pointers in a table sit right next to each other, so tables are followed in steps of the pointer size. Unaligned (small)
            //pointers can start at any byte, so every phase is scanned and overlapping hits are removed at the end.
            int phases = r.ScanStep >= r.Size ? 1 : r.Size / r.ScanStep;
            int stride = r.ScanStep >= r.Size ? r.ScanStep : r.Size;
            var run = new List<Candidate>();
            uint runMin = 0, runMax = 0;
            void FlushRun()
            {
                if (run.Count >= minRun && (run.Count < 2 || runMax != runMin || minRun == 1)) list.AddRange(run); //A "table" of one repeated value is fill data
                run.Clear();
            }
            for (int phase = 0; phase < phases; phase++)
            {
                long start = first + phase * r.ScanStep;
                run.Clear();
                long count = 0;
                for (long p = start; p <= last; p += stride)
                {
                    if ((++count & 0x3FFFF) == 0) ctx.Token.ThrowIfCancellationRequested();
                    uint v = ReadValue(d, p, r);
                    if (!LooksLikePointer(d, r, s, v))
                    {
                        FlushRun();
                        continue;
                    }
                    if (run.Count > 0)
                    {
                        uint newMin = Math.Min(runMin, v), newMax = Math.Max(runMax, v);
                        if (newMax - newMin > r.MaxSpread)
                        {
                            FlushRun();
                        }
                        else
                        {
                            runMin = newMin;
                            runMax = newMax;
                            run.Add(new Candidate(p, v, 0));
                            continue;
                        }
                    }
                    runMin = runMax = v;
                    run.Add(new Candidate(p, v, 0));
                }
                FlushRun();
            }
            if (phases > 1)
            {
                //Remove hits that overlap an earlier one
                list.Sort((a, b) => a.Offset.CompareTo(b.Offset));
                var kept = new List<Candidate>(list.Count);
                long nextFree = long.MinValue;
                foreach (var c in list)
                {
                    if (c.Offset < nextFree) continue;
                    kept.Add(c);
                    nextFree = c.Offset + r.Size;
                }
                list = kept;
            }
            return list;
        }

        //F3D + F3DEX2 opcodes that show up in display lists (used to make sure a "command" is really inside a display list)
        private static readonly HashSet<byte> GbiOpcodes = [0x00, 0x01, 0x03, 0x04, 0x05, 0x06, 0x07, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF,
            0xD7, 0xD8, 0xD9, 0xDA, 0xDB, 0xDC, 0xDD, 0xDE, 0xDF, 0xE1, 0xE2, 0xE3, 0xE4, 0xE5, 0xE6, 0xE7, 0xE8, 0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF,
            0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE, 0xFF];

        private static List<Candidate> FindDisplayListPointers(CorruptionContext ctx, Rules r, Settings s)
        {
            var d = ctx.Data!;
            var list = new List<Candidate>();
            long first = Math.Max((ctx.Start + 7) & ~7L, N64Rom.BootCodeEnd), last = ctx.End - 7;
            var ops = new List<byte>();
            long f3dEnds = 0, f3dex2Ends = 0;
            bool IsGbi(long at) => at >= 0 && at + 8 <= d.Length && GbiOpcodes.Contains((byte)(ReadWord(d, at, r.Order) >> 24));
            for (long p = first; p <= last; p += 8)
            {
                if (((p - first) & 0xFFFFF) == 0) ctx.Token.ThrowIfCancellationRequested();
                uint hi = ReadWord(d, p, r.Order);
                byte op = (byte)(hi >> 24);
                if ((hi == 0xB8000000 || hi == 0xDF000000) && ReadWord(d, p + 4, r.Order) == 0)
                {
                    //G_ENDDL tells the microcode family apart: 0xB8 = F3D/F3DEX, 0xDF = F3DEX2
                    if (hi == 0xB8000000) f3dEnds++; else f3dex2Ends++;
                    continue;
                }
                var target = op switch
                {
                    0xFD => DisplayListTargets.Textures, //G_SETTIMG
                    0x04 or 0x01 => DisplayListTargets.Vertices, //G_VTX (F3D / F3DEX2)
                    0xDE or 0x06 => DisplayListTargets.DisplayLists, //G_DL (F3DEX2 / F3D)
                    _ => DisplayListTargets.None
                };
                if (target == DisplayListTargets.None || (s.DisplayListTargets & target) == 0) continue;
                uint lo = ReadWord(d, p + 4, r.Order);
                byte segment = (byte)(lo >> 24);
                if (!(segment is >= 0x01 and <= 0x0F || segment == 0x80) || (lo & 7) != 0) continue;
                if (target == DisplayListTargets.DisplayLists && (hi & 0x00FEFFFF) != 0) continue; //G_DL only uses the "branch" flag
                //Both neighbouring commands have to look like display list commands too
                if (!IsGbi(p - 8) || !IsGbi(p + 8)) continue;
                list.Add(new Candidate(p + 4, lo, (byte)target));
                ops.Add(op);
            }

            //The same opcode means different things in F3D and F3DEX2: 0x01 is G_MTX (a matrix pointer) in F3D but G_VTX in F3DEX2,
            //0x04 is G_VTX in F3D but G_BRANCH_Z in F3DEX2, 0x06 is G_DL in F3D but G_TRI2 in F3DEX2. Keep only the opcodes of the
            //microcode the display lists use, so a matrix is never swapped with a vertex buffer.
            bool? f3dex2 = f3dex2Ends > f3dEnds * 4 ? true : f3dEnds > f3dex2Ends * 4 ? false : null;
            if (f3dex2 != null)
            {
                byte vtx = f3dex2.Value ? (byte)0x01 : (byte)0x04, dl = f3dex2.Value ? (byte)0xDE : (byte)0x06;
                var kept = new List<Candidate>(list.Count);
                for (int k = 0; k < list.Count; k++)
                    if (ops[k] == 0xFD || ops[k] == vtx || ops[k] == dl) kept.Add(list[k]);
                list = kept;
            }
            TraceLogger.Log($"Pointer Engine: display list microcode {(f3dex2 == null ? "unknown (F3D and F3DEX2 opcodes are both used)" : f3dex2.Value ? "F3DEX2" : "F3D/F3DEX")} ({f3dEnds} F3D / {f3dex2Ends} F3DEX2 end commands).");
            return list;
        }

        #endregion

        #region Changing pointers

        private static bool Apply(CorruptionContext ctx, Settings s, Rules r, List<Candidate> c, uint[] values, int a)
        {
            var me = c[a];
            switch (s.Mode)
            {
                case PointerMode.SWAP:
                    {
                        int b = Partner(ctx.Rnd, c, values, a, s.TableWindow, true);
                        if (b < 0 || ctx.IsExcluded(c[b].Offset, r.Size) || ctx.IsExcluded(me.Offset, r.Size)) return false;
                        (values[a], values[b]) = (values[b], values[a]);
                        WriteValue(ctx, r, me.Offset, values[a]);
                        WriteValue(ctx, r, c[b].Offset, values[b]);
                        return true;
                    }
                case PointerMode.DUPLICATE:
                    {
                        int b = Partner(ctx.Rnd, c, values, a, s.TableWindow, false);
                        if (b < 0) return false;
                        values[a] = values[b];
                        return WriteValue(ctx, r, me.Offset, values[a]);
                    }
                case PointerMode.RANDOMTARGET:
                    {
                        for (int tries = 0; tries < 16; tries++)
                        {
                            int b = ctx.Rnd.Next(c.Count);
                            if (b == a || c[b].Kind != me.Kind || values[b] == values[a]) continue;
                            values[a] = values[b];
                            return WriteValue(ctx, r, me.Offset, values[a]);
                        }
                        return false;
                    }
                default: //SHIFT
                    {
                        uint align = r.Profile == PointerProfile.N64DisplayLists ? 8u : Math.Max(1u, r.Alignment);
                        int steps = Math.Max(1, s.ShiftAmount / (int)align);
                        long delta = (long)align * ctx.Rnd.Next(1, steps + 1) * (ctx.Rnd.Next(2) == 0 ? -1 : 1);
                        long moved = values[a] + delta;
                        if (r.Profile == PointerProfile.N64DisplayLists)
                        {
                            //Stay inside the same segment
                            if (moved < 0 || (moved >> 24) != (values[a] >> 24)) return false;
                        }
                        else if (moved < r.Min || moved > r.Max || !LooksLikePointer(ctx.Data!, r, s, (uint)moved)) return false;
                        values[a] = (uint)moved;
                        return WriteValue(ctx, r, me.Offset, values[a]);
                    }
            }
        }

        /// <summary>
        /// Finds another pointer of the same kind within the table window (falls back to any pointer of the same kind for SWAP).
        /// </summary>
        private static int Partner(Random rnd, List<Candidate> c, uint[] values, int a, int window, bool allowFar)
        {
            var me = c[a];
            int lo = a, hi = a;
            while (lo > 0 && me.Offset - c[lo - 1].Offset <= window) lo--;
            while (hi < c.Count - 1 && c[hi + 1].Offset - me.Offset <= window) hi++;
            var near = Enumerable.Range(lo, hi - lo + 1).Where(i => i != a && c[i].Kind == me.Kind && values[i] != values[a]).ToList();
            if (near.Count > 0) return near[rnd.Next(near.Count)];
            if (!allowFar) return -1;
            for (int tries = 0; tries < 16; tries++)
            {
                int b = rnd.Next(c.Count);
                if (b != a && c[b].Kind == me.Kind && values[b] != values[a]) return b;
            }
            return -1;
        }

        #endregion
    }
}
