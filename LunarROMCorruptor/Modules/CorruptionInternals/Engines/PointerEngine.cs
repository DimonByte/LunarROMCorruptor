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
        public enum PointerProfile
        {
            Auto,
            N64Ram, //0x80000000 - 0x807FFFFF, big-endian
            N64DisplayLists, //Segmented addresses in F3D/F3DEX/F3DEX2 display list commands (textures, vertices, sub display lists)
            GbaRom, //0x08000000 - 0x09FFFFFF, little-endian, points into the ROM file
            NdsRam, //0x02000000 - 0x023FFFFF, little-endian
            Ps1Ram, //0x80000000 - 0x801FFFFF, little-endian
            GameCubeWiiRam, //0x80000000 - 0x817FFFFF, big-endian
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
            public bool SkipFunctionPointers = true; //Skip pointers to code (N64 boot segment functions, GBA/DS Thumb pointers)
            public DisplayListTargets DisplayListTargets = DisplayListTargets.Textures;
            public uint CustomMin = 0x80000000;
            public uint CustomMax = 0x807FFFFF;
            public bool CustomBigEndian = true;
        }

        private enum WordOrder { Little, Big, ByteSwapped16 }

        private readonly record struct Candidate(long Offset, uint Value, byte Kind);

        /// <summary>
        /// Everything needed to recognise a pointer for the resolved profile.
        /// </summary>
        private sealed class Rules
        {
            public PointerProfile Profile;
            public WordOrder Order;
            public uint Min, Max;
            public uint Alignment = 4;
            public long FileBase = -1; //Pointer value of file offset 0 (GBA ROM / file offsets), -1 when pointers don't point into the file
            public uint N64EntryPoint; //RAM address of ROM offset 0x1000 (to detect function pointers into the N64 boot segment)
        }

        public static void Run(CorruptionContext ctx, Settings s)
        {
            var data = ctx.Data!;
            var rules = Resolve(data, s);
            TraceLogger.Log($"Pointer Engine profile: {rules.Profile} ({rules.Order}{(rules.Profile == PointerProfile.N64DisplayLists ? $", {s.DisplayListTargets}" : $", 0x{rules.Min:X8} - 0x{rules.Max:X8}")}).");
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

        private static Rules Resolve(FileBuffer d, Settings s)
        {
            var profile = s.Profile;
            var n64 = N64Rom.DetectByteOrder(d);
            if (profile == PointerProfile.Auto)
            {
                string? detected = ExclusionEngine.DetectProfile(d.FilePath, d);
                profile = detected switch
                {
                    "N64" => PointerProfile.N64Ram,
                    "GBA" => PointerProfile.GbaRom,
                    "NDS" => PointerProfile.NdsRam,
                    "PS1" => PointerProfile.Ps1Ram,
                    "GAMECUBE" or "WII" => PointerProfile.GameCubeWiiRam,
                    _ => PointerProfile.FileOffsetsLE
                };
            }
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
                case PointerProfile.FileOffsetsLE:
                case PointerProfile.FileOffsetsBE:
                    rules.Order = profile == PointerProfile.FileOffsetsBE ? WordOrder.Big : WordOrder.Little;
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

        private static bool WriteWord(CorruptionContext ctx, long p, uint value, WordOrder order)
        {
            if (ctx.IsExcluded(p, 4)) return false;
            Span<byte> b = stackalloc byte[4];
            if (order == WordOrder.Little) System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(b, value);
            else System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(b, value);
            for (int k = 0; k < 4; k++)
            {
                long i = p + (order == WordOrder.ByteSwapped16 ? k ^ 1 : k);
                if (ctx.Get(i) != b[k]) ctx.Set(i, b[k]);
            }
            return true;
        }

        private static bool LooksLikePointer(FileBuffer d, Rules r, Settings s, uint v)
        {
            if (v < r.Min || v > r.Max) return false;
            if (s.SkipFunctionPointers && (v & 1) != 0 && r.Profile is PointerProfile.GbaRom or PointerProfile.NdsRam) return false; //Thumb function pointer
            if ((v & (r.Alignment - 1)) != 0) return false;
            if (r.FileBase >= 0 && v - r.FileBase >= d.Length) return false;
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
            var list = new List<Candidate>();
            long first = (ctx.Start + 3) & ~3L, last = ctx.End - 3;
            if (N64Rom.DetectByteOrder(d) != null) first = Math.Max(first, N64Rom.BootCodeEnd); //Never touch the N64 header/boot code
            bool prevIsPointer = false;
            long prevOffset = -8;
            uint prevValue = 0;
            for (long p = first; p <= last; p += 4)
            {
                if (((p - first) & 0xFFFFF) == 0) ctx.Token.ThrowIfCancellationRequested();
                uint v = ReadWord(d, p, r.Order);
                bool isPointer = LooksLikePointer(d, r, s, v);
                if (isPointer && !s.TablesOnly)
                {
                    list.Add(new Candidate(p, v, 0));
                }
                else if (isPointer)
                {
                    //In a table: the previous word is a pointer too (both are added) or the next one is
                    if (prevIsPointer && prevOffset == p - 4)
                    {
                        if (list.Count == 0 || list[^1].Offset != prevOffset) list.Add(new Candidate(prevOffset, prevValue, 0));
                        list.Add(new Candidate(p, v, 0));
                    }
                }
                prevIsPointer = isPointer;
                prevOffset = p;
                prevValue = v;
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
            bool IsGbi(long at) => at >= 0 && at + 8 <= d.Length && GbiOpcodes.Contains((byte)(ReadWord(d, at, r.Order) >> 24));
            for (long p = first; p <= last; p += 8)
            {
                if (((p - first) & 0xFFFFF) == 0) ctx.Token.ThrowIfCancellationRequested();
                uint hi = ReadWord(d, p, r.Order);
                byte op = (byte)(hi >> 24);
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
            }
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
                        if (b < 0 || ctx.IsExcluded(c[b].Offset, 4) || ctx.IsExcluded(me.Offset, 4)) return false;
                        (values[a], values[b]) = (values[b], values[a]);
                        WriteWord(ctx, me.Offset, values[a], r.Order);
                        WriteWord(ctx, c[b].Offset, values[b], r.Order);
                        return true;
                    }
                case PointerMode.DUPLICATE:
                    {
                        int b = Partner(ctx.Rnd, c, values, a, s.TableWindow, false);
                        if (b < 0) return false;
                        values[a] = values[b];
                        return WriteWord(ctx, me.Offset, values[a], r.Order);
                    }
                case PointerMode.RANDOMTARGET:
                    {
                        for (int tries = 0; tries < 16; tries++)
                        {
                            int b = ctx.Rnd.Next(c.Count);
                            if (b == a || c[b].Kind != me.Kind || values[b] == values[a]) continue;
                            values[a] = values[b];
                            return WriteWord(ctx, me.Offset, values[a], r.Order);
                        }
                        return false;
                    }
                default: //SHIFT
                    {
                        uint align = r.Profile == PointerProfile.N64DisplayLists ? 8u : r.Alignment;
                        int steps = Math.Max(1, s.ShiftAmount / (int)align);
                        long delta = (long)align * ctx.Rnd.Next(1, steps + 1) * (ctx.Rnd.Next(2) == 0 ? -1 : 1);
                        long moved = values[a] + delta;
                        if (r.Profile == PointerProfile.N64DisplayLists)
                        {
                            //Stay inside the same segment
                            if (moved < 0 || (moved >> 24) != (values[a] >> 24)) return false;
                        }
                        else if (moved < r.Min || moved > r.Max) return false;
                        values[a] = (uint)moved;
                        return WriteWord(ctx, me.Offset, values[a], r.Order);
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
