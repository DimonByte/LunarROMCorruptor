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
    /// A range of bytes (inclusive) that the Exclusion Engine will never corrupt.
    /// </summary>
    public readonly record struct ProtectedRange(long Start, long End, string Description)
    {
        public override string ToString() => $"0x{Start:X6} - 0x{End:X6}  {Description}";
    }

    public class ExclusionEngine
    {
        public sealed class Settings
        {
            public string Profile = "NES";
            public CorruptionOptions Option = CorruptionOptions.RANDOM;
            public int TiltAmount = 1;
        }

        private const long RawSector = 2352; //Raw CD sector (BIN/CUE)
        private const long IsoSector = 2048; //Cooked CD/DVD sector (ISO)

        /// <summary>
        /// Fixed protected regions per platform. Regions that depend on the file contents (headers that store offsets, copier headers,
        /// raw vs cooked disc images, etc.) are added at runtime by <see cref="GetProtectedRanges"/>.
        /// </summary>
        public static readonly Dictionary<string, List<ProtectedRange>> Profiles = new()
        {
            ["NES"] =
            [
                new(0x0000, 0x000F, "iNES / NES 2.0 header"),
            ],
            ["SNES"] =
            [
                new(0x7FB0, 0x7FFF, "LoROM internal header + interrupt vectors"),
                new(0xFFB0, 0xFFFF, "HiROM internal header + interrupt vectors"),
            ],
            //The boot checksum (CRC1/CRC2) over 0x1000 - 0x100FFF is rewritten after corrupting (see N64Rom), so the normal profile
            //can corrupt the first 1 MB. STRICT CRC is for ROMs where LRC can't rewrite the checksum (unknown CIC / bad header CRC).
            ["N64"] =
            [
                new(0x0000, 0x003F, "ROM header (PI settings, entry point, CRC1/CRC2, title, game ID)"),
                new(0x0040, 0x0FFF, "IPL3 boot code (CIC)"),
                new(0x1000, 0x10FF, "Game entry point (first code run after IPL3: clears BSS, sets the stack, jumps to the game)"),
            ],
            ["N64 (STRICT CRC)"] =
            [
                new(0x0000, 0x003F, "ROM header (PI settings, entry point, CRC1/CRC2, title, game ID)"),
                new(0x0040, 0x0FFF, "IPL3 boot code (CIC)"),
                new(0x1000, 0x100FFF, "First 1 MB of game code - covered by the boot CRC check (only needed if LRC can't update the checksum)"),
            ],
            ["GAMEBOY"] =
            [
                new(0x0000, 0x00FF, "RST and interrupt vectors"),
                new(0x0100, 0x0103, "Entry point"),
                new(0x0104, 0x0133, "Nintendo logo (checked by the boot ROM)"),
                new(0x0134, 0x014F, "Title, CGB/SGB flags, cartridge type, ROM/RAM size, header + global checksums"),
            ],
            ["GBA"] =
            [
                new(0x0000, 0x0003, "ROM entry point (ARM branch)"),
                new(0x0004, 0x009F, "Nintendo logo (checked by the BIOS)"),
                new(0x00A0, 0x00BF, "Title, game code, maker code, fixed value, header complement check"),
                new(0x00C0, 0x00DF, "Multiboot / RAM entry points"),
            ],
            ["NDS"] =
            [
                new(0x0000, 0x017F, "Cartridge header (ARM9/ARM7 offsets, logo, header CRC)"),
                new(0x0180, 0x3FFF, "Reserved header area"),
                new(0x4000, 0x7FFF, "Secure area"),
            ],
            ["GAMECUBE"] =
            [
                new(0x0000, 0x043F, "Disc header (boot.bin): game ID, magic word, DOL/FST offsets"),
                new(0x0440, 0x243F, "Disc header information (bi2.bin)"),
            ],
            ["WII"] =
            [
                new(0x0000, 0x03FF, "Disc header: game ID, magic word, title"),
                new(0x40000, 0x4FFFF, "Partition tables and region settings"),
            ],
            ["PS1"] =
            [
                new(0x0000, 24 * RawSector - 1, "System area, license data, volume descriptors, path tables, root directory (raw BIN)"),
            ],
            ["PS2"] =
            [
                new(0x0000, 24 * IsoSector - 1, "System area, volume descriptors, path tables, root directory (ISO)"),
            ],
            ["SEGA"] =
            [
                new(0x0000, 0x00FF, "68000 vector table (stack pointer, reset vector, exceptions)"),
                new(0x0100, 0x01FF, "Genesis / Mega Drive ROM header (\"SEGA\" is checked by the TMSS)"),
            ],
            ["SMS/GG"] =
            [
                new(0x0000, 0x00FF, "Z80 RST and interrupt vectors (reset, IRQ at 0x38, NMI at 0x66)"),
            ],
            ["SEGA CD/SATURN"] =
            [
                new(0x0000, 18 * IsoSector - 1, "System area / IP.BIN boot header, volume descriptors (ISO)"),
            ],
            ["DREAMCAST"] =
            [
                new(0x0000, 0x7FFF, "IP.BIN boot sectors (data track)"),
                new(0x8000, 0x8FFF, "Volume descriptors"),
            ],
            ["PC ENGINE"] =
            [
                new(0x1FF6, 0x1FFF, "HuC6280 interrupt + reset vectors (end of the first 8 KB bank)"),
            ],
            ["ATARI 2600"] = [],
            ["ATARI 7800"] =
            [
                new(0x0000, 0x007F, "A78 header"),
            ],
            ["ATARI LYNX"] =
            [
                new(0x0000, 0x003F, "LNX header"),
                new(0x0040, 0x01BF, "Encrypted boot loader (approximate)"),
            ],
            ["VIRTUAL BOY"] = [],
            ["WONDERSWAN"] = [],
            ["EXE"] =
            [
                new(0x0000, 0x003F, "DOS (MZ) header incl. e_lfanew"),
            ],
            ["ELF"] =
            [
                new(0x0000, 0x0033, "ELF identification and header (32-bit size)"),
            ],
            ["BIOS"] =
            [
                new(0x0000, 0x01FF, "Start of image (option ROM / flash descriptor header)"),
            ],
            ["BMP"] =
            [
                new(0x0000, 0x0035, "Bitmap file header + info header"),
            ],
            ["WAV"] =
            [
                new(0x0000, 0x002B, "RIFF/WAVE header + fmt chunk"),
            ],
            ["JPEG"] =
            [
                new(0x0000, 0x0001, "Start of image marker"),
            ],
        };

        /// <summary>
        /// Returns every protected range for the profile, including the ranges calculated from the file itself.
        /// </summary>
        public static List<ProtectedRange> GetProtectedRanges(string profile, FileBuffer? data)
        {
            var ranges = new List<ProtectedRange>(Profiles.GetValueOrDefault(profile, []));
            if (data == null || data.Length == 0)
                return ranges;
            try
            {
                AddDynamicRanges(profile, data, ranges);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Exclusion Engine: unable to read the {profile} header from the file, only the fixed ranges will be used. {ex.Message}", StatusSeverityType.Warning);
            }
            long max = data.Length - 1;
            return [.. ranges.Where(r => r.Start <= max && r.End >= r.Start).Select(r => r with { End = Math.Min(r.End, max) }).OrderBy(r => r.Start)];
        }

        private static bool HasMagic(FileBuffer data, long offset, string magic)
        {
            Span<byte> b = stackalloc byte[magic.Length];
            if (data.Read(offset, b) < magic.Length) return false;
            for (int i = 0; i < magic.Length; i++)
                if (b[i] != (byte)magic[i]) return false;
            return true;
        }

        private static void AddDynamicRanges(string profile, FileBuffer d, List<ProtectedRange> r)
        {
            long len = d.Length;
            switch (profile)
            {
                case "NES":
                    if (HasMagic(d, 0, "NES\x1A"))
                    {
                        Span<byte> h = stackalloc byte[16];
                        d.Read(0, h);
                        bool trainer = (h[6] & 0x04) != 0;
                        long prgStart = 16 + (trainer ? 512 : 0);
                        if (trainer) r.Add(new(0x0010, 0x020F, "512 byte trainer"));
                        long prgSize = h[4] * 16384L;
                        if (prgSize > 0 && prgStart + prgSize <= len)
                            r.Add(new(prgStart + prgSize - 6, prgStart + prgSize - 1, "NMI / RESET / IRQ vectors (end of PRG-ROM)"));
                    }
                    break;

                case "SNES":
                    {
                        long b = len % 1024 == 512 ? 512 : 0; //SMC/SWC copier header
                        if (b > 0)
                        {
                            r.Add(new(0x0000, 0x01FF, "512 byte copier (SMC/SWC) header"));
                            r.Add(new(b + 0x7FB0, b + 0x7FFF, "LoROM internal header + vectors (after copier header)"));
                            r.Add(new(b + 0xFFB0, b + 0xFFFF, "HiROM internal header + vectors (after copier header)"));
                        }
                        if (len - b > 0x400000)
                            r.Add(new(b + 0x40FFB0, b + 0x40FFFF, "ExHiROM internal header + vectors"));
                    }
                    break;

                case "N64":
                case "N64 (STRICT CRC)":
                    if (N64Rom.DetectByteOrder(d) == null)
                        TraceLogger.Log("Exclusion Engine: the file doesn't start with an N64 header (.z64/.v64/.n64), the N64 ranges may not match this file.", StatusSeverityType.Warning);
                    break;

                case "NDS":
                    {
                        uint fntOffset = d.ReadUInt32(0x40, false), fntSize = d.ReadUInt32(0x44, false);
                        uint fatOffset = d.ReadUInt32(0x48, false), fatSize = d.ReadUInt32(0x4C, false);
                        if (fntSize > 0 && fntOffset + (long)fntSize <= len) r.Add(new(fntOffset, fntOffset + fntSize - 1, "File name table (FNT)"));
                        if (fatSize > 0 && fatOffset + (long)fatSize <= len) r.Add(new(fatOffset, fatOffset + fatSize - 1, "File allocation table (FAT)"));
                    }
                    break;

                case "GAMECUBE":
                    if (d.ReadUInt32(0x1C, true) == 0xC2339F3D) //GameCube magic word
                    {
                        uint dolOffset = d.ReadUInt32(0x420, true);
                        uint fstOffset = d.ReadUInt32(0x424, true);
                        uint fstSize = d.ReadUInt32(0x428, true);
                        uint apploaderSize = d.ReadUInt32(0x2454, true);
                        uint apploaderTrailer = d.ReadUInt32(0x2458, true);
                        long apploaderEnd = 0x2460L + apploaderSize + apploaderTrailer;
                        if (apploaderEnd <= len) r.Add(new(0x2440, apploaderEnd - 1, "Apploader"));
                        if (dolOffset > 0 && dolOffset + 0x100L <= len) r.Add(new(dolOffset, dolOffset + 0xFF, "Main executable (DOL) header"));
                        if (fstSize > 0 && fstOffset + (long)fstSize <= len) r.Add(new(fstOffset, fstOffset + fstSize - 1, "File system table (FST)"));
                    }
                    break;

                case "WII":
                    if (d.ReadUInt32(0x18, true) == 0x5D1C9EA3) //Wii magic word
                    {
                        //Partition table info at 0x40000: 4 groups of (count, offset >> 2)
                        for (int group = 0; group < 4; group++)
                        {
                            uint count = d.ReadUInt32(0x40000 + group * 8, true);
                            long tableOffset = (long)d.ReadUInt32(0x40004 + group * 8, true) << 2;
                            for (uint p = 0; p < Math.Min(count, 16u); p++)
                            {
                                long partition = (long)d.ReadUInt32(tableOffset + p * 8, true) << 2;
                                if (partition <= 0 || partition >= len) continue;
                                long dataOffset = (long)d.ReadUInt32(partition + 0x2B8, true) << 2;
                                if (dataOffset <= 0 || dataOffset > 0x100000) dataOffset = 0x20000;
                                r.Add(new(partition, partition + dataOffset - 1, $"Partition {group}.{p} header (ticket, TMD, certificates, H3 table)"));
                            }
                        }
                    }
                    break;

                case "PS1":
                case "PS2":
                case "SEGA CD/SATURN":
                    {
                        bool raw = IsRawDisc(d);
                        long sector = raw ? RawSector : IsoSector;
                        int sectors = profile == "SEGA CD/SATURN" ? 18 : 24;
                        r.Add(new(0, sectors * sector - 1, raw ? $"First {sectors} raw sectors (system area, volume descriptors, directory)" : $"First {sectors} ISO sectors (system area, volume descriptors, directory)"));
                        if (profile == "PS1")
                        {
                            long exe = FindSignature(d, "PS-X EXE", sector, raw ? 24 : 0, 64L * 1024 * 1024);
                            if (exe >= 0) r.Add(new(exe, exe + 0x7FF, "PS-X EXE executable header"));
                        }
                        else if (profile == "PS2")
                        {
                            long elf = FindSignature(d, "\x7F" + "ELF", sector, raw ? 24 : 0, 64L * 1024 * 1024);
                            if (elf >= 0) AddElfRanges(d, elf, r, "Boot ELF ");
                        }
                    }
                    break;

                case "DREAMCAST":
                    if (IsRawDisc(d))
                        r.Add(new(0, 18 * RawSector - 1, "IP.BIN boot sectors + volume descriptors (raw track)"));
                    break;

                case "SMS/GG":
                    foreach (long headerOffset in new long[] { 0x1FF0, 0x3FF0, 0x7FF0 })
                    {
                        if (HasMagic(d, headerOffset, "TMR SEGA"))
                            r.Add(new(headerOffset, headerOffset + 0xF, "\"TMR SEGA\" ROM header (checksum, region, size)"));
                    }
                    break;

                case "PC ENGINE":
                    if (len % 8192 == 512)
                    {
                        r.Add(new(0x0000, 0x01FF, "512 byte copier header"));
                        r.Add(new(0x0200 + 0x1FF6, 0x0200 + 0x1FFF, "HuC6280 interrupt + reset vectors (after copier header)"));
                    }
                    break;

                case "ATARI 2600":
                    {
                        long bank = Math.Min(len, 4096);
                        for (long end = bank; end <= len; end += bank)
                            r.Add(new(end - 6, end - 1, "NMI / RESET / IRQ vectors (end of bank)"));
                    }
                    break;

                case "ATARI 7800":
                    if (len > 6) r.Add(new(len - 6, len - 1, "6502 NMI / RESET / IRQ vectors"));
                    break;

                case "VIRTUAL BOY":
                    if (len >= 0x220) r.Add(new(len - 0x220, len - 1, "ROM header (title, maker, game code) + interrupt/reset vectors"));
                    break;

                case "WONDERSWAN":
                    if (len >= 16) r.Add(new(len - 16, len - 1, "Boot jump + ROM footer (publisher, game ID, size, save type, checksum)"));
                    break;

                case "EXE":
                    if (HasMagic(d, 0, "MZ"))
                    {
                        long pe = d.ReadUInt32(0x3C, false);
                        if (pe > 0 && pe + 24 <= len && HasMagic(d, pe, "PE\0\0"))
                        {
                            if (pe > 0x40) r.Add(new(0x40, pe - 1, "DOS stub"));
                            ushort sections = d.ReadUInt16(pe + 6, false);
                            ushort optionalSize = d.ReadUInt16(pe + 20, false);
                            long headersEnd = pe + 24 + optionalSize + sections * 40L;
                            r.Add(new(pe, headersEnd - 1, $"PE signature, COFF header, optional header (data directories) and {sections} section headers"));
                        }
                    }
                    break;

                case "ELF":
                    if (HasMagic(d, 0, "\x7F" + "ELF"))
                        AddElfRanges(d, 0, r, "");
                    break;

                case "BIOS":
                    if (len >= 0x2000) r.Add(new(len - 0x2000, len - 1, "Boot block + reset vector (end of image, approximate)"));
                    break;

                case "BMP":
                    if (HasMagic(d, 0, "BM"))
                    {
                        uint pixelOffset = d.ReadUInt32(0x0A, false);
                        if (pixelOffset > 0x0E && pixelOffset <= len) r.Add(new(0, pixelOffset - 1, "Headers + colour palette (everything before the pixel data)"));
                    }
                    break;

                case "WAV":
                    if (HasMagic(d, 0, "RIFF") && HasMagic(d, 8, "WAVE"))
                    {
                        long pos = 12;
                        while (pos + 8 <= len)
                        {
                            bool isData = HasMagic(d, pos, "data");
                            uint size = d.ReadUInt32(pos + 4, false);
                            if (isData)
                            {
                                r.Add(new(0, pos + 7, "RIFF header and every chunk before the sample data"));
                                break;
                            }
                            pos += 8 + size + (size & 1);
                        }
                    }
                    break;

                case "JPEG":
                    {
                        if (d.ReadUInt16(0, true) != 0xFFD8) break;
                        long pos = 2;
                        while (pos + 4 <= len)
                        {
                            if (d[pos] != 0xFF) break;
                            byte marker = d[pos + 1];
                            if (marker == 0xFF) { pos++; continue; }
                            ushort size = d.ReadUInt16(pos + 2, true);
                            if (marker == 0xDA) //Start of scan: protect everything up to the end of the SOS header
                            {
                                r.Add(new(0, pos + 1 + size, "JPEG markers/tables up to the end of the start-of-scan header"));
                                break;
                            }
                            pos += 2 + size;
                        }
                        if (len >= 2 && d.ReadUInt16(len - 2, true) == 0xFFD9)
                            r.Add(new(len - 2, len - 1, "End of image marker"));
                    }
                    break;
            }
        }

        /// <summary>
        /// Guesses the Exclusion Engine profile for a file from its header (and the extension when the header has no signature). Returns null when unknown.
        /// </summary>
        public static string? DetectProfile(string? path, FileBuffer d)
        {
            try
            {
                long len = d.Length;
                if (N64Rom.DetectByteOrder(d) != null) return "N64";
                if (HasMagic(d, 0, "NES\x1A")) return "NES";
                if (len >= 0x150 && d[0x104] == 0xCE && d[0x105] == 0xED && d[0x106] == 0x66 && d[0x107] == 0x66) return "GAMEBOY";
                if (len >= 0xC0 && d[0xB2] == 0x96 && d[0x04] == 0x24 && d[0x05] == 0xFF && d[0x06] == 0xAE) return "GBA";
                if (len >= 0x200 && d.ReadUInt16(0x15C, false) == 0xCF56) return "NDS";
                if (len >= 0x440 && d.ReadUInt32(0x1C, true) == 0xC2339F3D) return "GAMECUBE";
                if (len >= 0x50000 && d.ReadUInt32(0x18, true) == 0x5D1C9EA3) return "WII";
                if (HasMagic(d, 0x100, "SEGA") || HasMagic(d, 0x101, "SEGA")) return "SEGA";
                foreach (long headerOffset in new long[] { 0x7FF0, 0x3FF0, 0x1FF0 })
                    if (HasMagic(d, headerOffset, "TMR SEGA")) return "SMS/GG";
                if (HasMagic(d, 1, "ATARI7800")) return "ATARI 7800";
                if (HasMagic(d, 0, "LYNX")) return "ATARI LYNX";
                if (HasMagic(d, 0, "\x7F" + "ELF")) return "ELF";
                if (HasMagic(d, 0, "MZ")) return "EXE";
                if (HasMagic(d, 0, "BM")) return "BMP";
                if (HasMagic(d, 0, "RIFF") && HasMagic(d, 8, "WAVE")) return "WAV";
                if (len >= 2 && d.ReadUInt16(0, true) == 0xFFD8) return "JPEG";
                string ext = Path.GetExtension(path ?? "").ToLowerInvariant();
                if (IsRawDisc(d)) return ext == ".gdi" || ext == ".cdi" ? "DREAMCAST" : "PS1";
                return ext switch
                {
                    ".sfc" or ".smc" or ".swc" or ".fig" => "SNES",
                    ".pce" => "PC ENGINE",
                    ".a26" => "ATARI 2600",
                    ".vb" => "VIRTUAL BOY",
                    ".ws" or ".wsc" => "WONDERSWAN",
                    ".iso" => "PS2",
                    ".rom" or ".bios" => "BIOS",
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        public enum RegionKind
        {
            Protected, //Header/boot ranges the Exclusion Engine protects
            Code,
            Data,
            Graphics,
            Compressed,
            Excluded //Bisect / EXCLUDE stash ranges
        }

        public readonly record struct NamedRegion(long Start, long End, string Name, RegionKind Kind);

        /// <summary>
        /// Named regions of a file for the ByteView Regions mode: the profile's protected ranges plus code/data/graphics areas read from the
        /// header and any compressed blocks (Yaz0/Yay0/MIO0) found in the file.
        /// </summary>
        public static List<NamedRegion> GetNamedRegions(string profile, FileBuffer d, CancellationToken token = default)
        {
            var regions = GetProtectedRanges(profile, d).Select(r => new NamedRegion(r.Start, r.End, r.Description, RegionKind.Protected)).ToList();
            long len = d.Length;
            void Add(long start, long end, string name, RegionKind kind)
            {
                end = Math.Min(end, len - 1);
                if (start >= 0 && start <= end) regions.Add(new NamedRegion(start, end, name, kind));
            }
            try
            {
                switch (profile)
                {
                    case "N64":
                    case "N64 (STRICT CRC)":
                        Add(0x1000, 0x100FFF, "Boot segment: main game code (the first 1 MB, covered by the boot checksum)", RegionKind.Code);
                        Add(0x101000, len - 1, "Game data / assets", RegionKind.Data);
                        break;
                    case "NES":
                        if (HasMagic(d, 0, "NES\x1A"))
                        {
                            long prgStart = 16 + ((d[6] & 0x04) != 0 ? 512 : 0);
                            long prg = d[4] * 16384L, chr = d[5] * 8192L;
                            Add(prgStart, prgStart + prg - 1, "PRG-ROM (code + data)", RegionKind.Code);
                            if (chr > 0) Add(prgStart + prg, prgStart + prg + chr - 1, "CHR-ROM (tile graphics)", RegionKind.Graphics);
                        }
                        break;
                    case "GAMEBOY":
                        Add(0x150, len - 1, "ROM (code + data)", RegionKind.Code);
                        break;
                    case "GBA":
                        Add(0xE0, len - 1, "ROM (code + data, referenced by 0x08xxxxxx pointers)", RegionKind.Code);
                        break;
                    case "NDS":
                        {
                            uint arm9 = d.ReadUInt32(0x20, false), arm9Size = d.ReadUInt32(0x2C, false);
                            uint arm7 = d.ReadUInt32(0x30, false), arm7Size = d.ReadUInt32(0x3C, false);
                            uint banner = d.ReadUInt32(0x68, false);
                            uint fatOffset = d.ReadUInt32(0x48, false), fatSize = d.ReadUInt32(0x4C, false);
                            if (arm9Size > 0) Add(arm9, arm9 + (long)arm9Size - 1, "ARM9 binary (main game code)", RegionKind.Code);
                            if (arm7Size > 0) Add(arm7, arm7 + (long)arm7Size - 1, "ARM7 binary (sound/wifi code)", RegionKind.Code);
                            if (banner > 0) Add(banner, banner + 0x83F, "Icon + title banner", RegionKind.Graphics);
                            if (fatSize >= 8 && fatOffset + (long)fatSize <= len)
                            {
                                long first = long.MaxValue, last = -1;
                                for (long e = fatOffset; e + 8 <= fatOffset + fatSize; e += 8)
                                {
                                    uint s = d.ReadUInt32(e, false), en = d.ReadUInt32(e + 4, false);
                                    if (en <= s || en > len) continue;
                                    first = Math.Min(first, s);
                                    last = Math.Max(last, en - 1);
                                }
                                if (last >= 0) Add(first, last, "File system data (files from the FAT)", RegionKind.Data);
                            }
                        }
                        break;
                    case "GAMECUBE":
                        if (d.ReadUInt32(0x1C, true) == 0xC2339F3D)
                        {
                            uint dol = d.ReadUInt32(0x420, true);
                            if (dol > 0 && dol + 0x100L <= len)
                            {
                                long dolEnd = dol + 0x100L;
                                for (int k = 0; k < 18; k++) //7 text + 11 data sections: offsets at 0x00, sizes at 0x90
                                {
                                    uint off = d.ReadUInt32(dol + k * 4, true), size = d.ReadUInt32(dol + 0x90 + k * 4, true);
                                    if (off > 0 && size > 0) dolEnd = Math.Max(dolEnd, dol + (long)off + size);
                                }
                                Add(dol + 0x100, dolEnd - 1, "Main executable (DOL): game code", RegionKind.Code);
                            }
                        }
                        break;
                }
                if (profile is "N64" or "N64 (STRICT CRC)" or "GAMECUBE" or "WII" or "NDS" or "GBA")
                {
                    var formats = profile switch
                    {
                        "NDS" or "GBA" => CompressionEngine.Formats.None, //LZ10/LZ11 have no signature, they're found by the Compression Engine through pointers/FAT
                        _ => CompressionEngine.Formats.Yaz0 | CompressionEngine.Formats.Yay0 | CompressionEngine.Formats.MIO0
                    };
                    if (formats != CompressionEngine.Formats.None)
                    {
                        foreach (var block in CompressionEngine.FindBlocks(d, formats, 0, len - 1, token, 4096))
                            Add(block.Offset, block.Offset + block.CompressedLength - 1, $"{block.Format} compressed block (unpacks to {CorruptionCore.FormatSize(block.DecompressedSize, out _, out _)})", RegionKind.Compressed);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"ByteView regions: unable to read every region from the {profile} header: {ex.Message}", StatusSeverityType.Warning);
            }
            return regions;
        }

        private static bool IsRawDisc(FileBuffer d)
        {
            //Raw CD sectors start with the sync pattern 00 FF FF FF FF FF FF FF FF FF FF 00
            Span<byte> sync = stackalloc byte[12];
            if (d.Read(0, sync) < 12) return false;
            if (sync[0] != 0 || sync[11] != 0) return false;
            for (int i = 1; i < 11; i++)
                if (sync[i] != 0xFF) return false;
            return true;
        }

        private static long FindSignature(FileBuffer d, string signature, long sectorSize, long dataOffsetInSector, long searchLimit)
        {
            long limit = Math.Min(d.Length, searchLimit);
            for (long sector = 0; sector * sectorSize + dataOffsetInSector + signature.Length <= limit; sector++)
            {
                long pos = sector * sectorSize + dataOffsetInSector;
                if (HasMagic(d, pos, signature))
                    return pos;
            }
            return -1;
        }

        private static void AddElfRanges(FileBuffer d, long baseOffset, List<ProtectedRange> r, string prefix)
        {
            byte elfClass = d[baseOffset + 4]; //1 = 32-bit, 2 = 64-bit
            bool bigEndian = d[baseOffset + 5] == 2;
            if (elfClass == 2)
            {
                long phoff = (long)d.ReadUInt32(baseOffset + 0x20, bigEndian) | ((long)d.ReadUInt32(baseOffset + 0x24, bigEndian) << 32);
                long shoff = (long)d.ReadUInt32(baseOffset + 0x28, bigEndian) | ((long)d.ReadUInt32(baseOffset + 0x2C, bigEndian) << 32);
                r.Add(new(baseOffset, baseOffset + 0x3F, prefix + "ELF64 header"));
                AddTable(r, baseOffset, phoff, d.ReadUInt16(baseOffset + 0x36, bigEndian), d.ReadUInt16(baseOffset + 0x38, bigEndian), prefix + "Program header table", d.Length);
                AddTable(r, baseOffset, shoff, d.ReadUInt16(baseOffset + 0x3A, bigEndian), d.ReadUInt16(baseOffset + 0x3C, bigEndian), prefix + "Section header table", d.Length);
            }
            else
            {
                long phoff = d.ReadUInt32(baseOffset + 0x1C, bigEndian);
                long shoff = d.ReadUInt32(baseOffset + 0x20, bigEndian);
                r.Add(new(baseOffset, baseOffset + 0x33, prefix + "ELF32 header"));
                AddTable(r, baseOffset, phoff, d.ReadUInt16(baseOffset + 0x2A, bigEndian), d.ReadUInt16(baseOffset + 0x2C, bigEndian), prefix + "Program header table", d.Length);
                AddTable(r, baseOffset, shoff, d.ReadUInt16(baseOffset + 0x2E, bigEndian), d.ReadUInt16(baseOffset + 0x30, bigEndian), prefix + "Section header table", d.Length);
            }
        }

        private static void AddTable(List<ProtectedRange> r, long baseOffset, long tableOffset, int entrySize, int count, string description, long fileLength)
        {
            if (tableOffset <= 0 || entrySize <= 0 || count <= 0) return;
            long start = baseOffset + tableOffset;
            long end = start + (long)entrySize * count - 1;
            if (end < fileLength) r.Add(new(start, end, description));
        }

        public static bool IsProtected(long position, List<ProtectedRange> ranges)
        {
            foreach (var range in ranges)
            {
                if (position >= range.Start && position <= range.End)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Runs the Exclusion Engine. Instead of picking random addresses and retrying when they're protected, the allowed parts of the
        /// selected range are calculated once, so every corruption lands on an allowed byte and there's no retry limit.
        /// </summary>
        public static void Run(CorruptionContext ctx, Settings s)
        {
            var ranges = GetProtectedRanges(s.Profile, ctx.Data);
            TraceLogger.Log($"Exclusion Engine profile {s.Profile}: {ranges.Count} protected range(s).\n{string.Join("\n", ranges)}");
            if (ctx.Excluded != null)
            {
                //Bisect / EXCLUDE stash ranges are left out of the allowed area too, so every corruption still lands on an allowed byte
                ranges.AddRange(ctx.Excluded.Select(x => new ProtectedRange(x.Start, x.End, "Excluded (Bisect / stash EXCLUDE)")));
                ranges.Sort((a, b) => a.Start.CompareTo(b.Start));
            }

            //Build the allowed intervals = [Start, End] minus the protected ranges.
            var allowed = new List<(long Start, long End)>();
            long cursor = ctx.Start;
            foreach (var range in ranges.OrderBy(x => x.Start))
            {
                if (range.End < cursor) continue;
                if (range.Start > ctx.End) break;
                if (range.Start > cursor) allowed.Add((cursor, Math.Min(range.Start - 1, ctx.End)));
                cursor = Math.Max(cursor, range.End + 1);
                if (cursor > ctx.End) break;
            }
            if (cursor <= ctx.End) allowed.Add((cursor, ctx.End));

            long totalAllowed = allowed.Sum(a => a.End - a.Start + 1);
            if (totalAllowed == 0)
            {
                TraceLogger.Log($"The selected byte range is completely protected by the {s.Profile} profile. Nothing was corrupted.", StatusSeverityType.Warning, true);
                return;
            }

            if (ctx.EveryNth)
            {
                long step = ctx.Intensity;
                long total = (ctx.End - ctx.Start) / step + 1;
                long done = 0;
                for (long i = ctx.Start; i <= ctx.End; i += step)
                {
                    if (!IsProtected(i, ranges))
                        CorruptByte(ctx, s, i);
                    if (++done % 65536 == 0) ctx.Tick(done, total);
                }
            }
            else
            {
                //Prefix sums so a random number in [0, totalAllowed) maps to an allowed address
                var prefix = new long[allowed.Count];
                long running = 0;
                for (int k = 0; k < allowed.Count; k++)
                {
                    prefix[k] = running;
                    running += allowed[k].End - allowed[k].Start + 1;
                }
                for (long n = 0; n < ctx.Intensity; n++)
                {
                    long pick = ctx.Rnd.NextInt64(0, totalAllowed);
                    int idx = Array.BinarySearch(prefix, pick);
                    if (idx < 0) idx = ~idx - 1;
                    CorruptByte(ctx, s, allowed[idx].Start + (pick - prefix[idx]));
                    if ((n + 1) % 65536 == 0) ctx.Tick(n + 1, ctx.Intensity);
                }
            }
        }

        public static void CorruptByte(CorruptionContext ctx, Settings s, long i)
        {
            switch (s.Option)
            {
                case CorruptionOptions.RANDOM:
                    byte newValue = (byte)ctx.Rnd.Next(0, 256);
                    if (ShouldAvoidValue(newValue))
                        newValue = GetAlternativeValue(ctx.Rnd);
                    ctx.Set(i, newValue);
                    break;

                case CorruptionOptions.RANDOMTILT:
                    switch (ctx.Rnd.Next(0, 3))
                    {
                        case 0:
                            byte value = (byte)ctx.Rnd.Next(0, 256);
                            if (ShouldAvoidValue(value))
                                value = GetAlternativeValue(ctx.Rnd);
                            ctx.Set(i, value);
                            break;
                        case 1:
                            ctx.Set(i, CorruptionCore.ClampByte(ctx.Get(i) + s.TiltAmount));
                            break;
                        default:
                            ctx.Set(i, CorruptionCore.ClampByte(ctx.Get(i) - s.TiltAmount));
                            break;
                    }
                    break;

                case CorruptionOptions.TILT:
                    if (ctx.Rnd.Next(0, 2) == 0)
                        ctx.Set(i, CorruptionCore.ClampByte(ctx.Get(i) + s.TiltAmount));
                    else
                        ctx.Set(i, CorruptionCore.ClampByte(ctx.Get(i) - s.TiltAmount));
                    break;

                default:
                    throw new InvalidOperationException($"The Exclusion Engine doesn't support the corruption type {s.Option}.");
            }
        }

        /// <summary>
        /// Determines if a corruption value should be avoided (0x00 and 0xF0-0xFF have a high crash potential)
        /// </summary>
        private static bool ShouldAvoidValue(byte value)
        {
            return value == 0x00 || value >= 0xF0;
        }

        private static readonly byte[] AlternativeValues =
        [
            0x7F, 0x80, 0x42, 0x69, 0xA5, 0xCD, 0x13, 0x88,
            0x2A, 0x76, 0x65, 0x8C
        ];

        private static byte GetAlternativeValue(Random rnd)
        {
            return AlternativeValues[rnd.Next(AlternativeValues.Length)];
        }
    }
}
