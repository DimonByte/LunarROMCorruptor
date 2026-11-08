# Corruption Engines

[← Back to the README](../README.md) · [Getting Started](Getting-Started.md) · [Engine Presets](Engine-Presets.md) · [Stashes, Saves & Bisect](Stash-and-Saves.md) · [ByteView](ByteView.md) · [Automation & Multi-File](Automation-and-Multi-File.md)

Every engine corrupts the bytes between **Start byte** and **End byte** (Target Parameters tab). How *much* it corrupts comes from the corruption mode:

| Mode | What it means for most engines |
| :--- | :--- |
| **Random byte mode** (Intensity) | Do *Intensity* corruptions at random places in the range. |
| **Corrupt every nth byte mode** | Corrupt every *N*th byte from Start to End (N = 1 corrupts every byte). |

Engines that understand the data count in their own units instead of bytes. The Vector2 Engine counts floats, the Pointer Engine pointers, the Blocks Engine blocks, the Datamosh Engine frames and the Source Code Engine values. Each section below says what its unit is.

> **Tip:** every engine has [built-in presets](Engine-Presets.md#built-in-presets) (★ in the *Preset* list at the top of the engine tab). Loading one is the quickest way to see what a sensible set of settings looks like.

**Engines:** [Nightmare+](#nightmare-engine) · [Merge](#merge-engine) · [Logic](#logic-engine) · [Lerp](#lerp-engine) · [Manual](#manual-engine) · [Vector2](#vector2-engine) · [Blocks](#blocks-engine) · [Pointer](#pointer-engine) · [Compression](#compression-engine) · [Datamosh](#datamosh-engine) · [Source Code](#source-code-engine)

---

<a id="nightmare-engine"></a>
## Nightmare+ Engine

The classic corruption engine (from the Windows Glitch Harvester) merged with the old **Exclusion Engine**. With *Exclusion Type: None* it is exactly the original Nightmare Engine. With an exclusion type it leaves the parts of the file that make it boot alone, so corruptions crash a lot less.

| Setting | What it does |
| :--- | :--- |
| **Corruption Type** | `RANDOM`: the byte gets a random value (0-255).<br>`RANDOMTILT`: per byte, one of three things at random: a random value, *+ Increment/Decrement number*, or *− Increment/Decrement number*.<br>`TILT`: adds or subtracts the *Increment/Decrement number* (50/50). |
| **Increment/Decrement number** | How much `TILT` / `RANDOMTILT` add or subtract (1-255). Results are clamped to 0-255, they don't wrap around (250 + 10 = 255). Small numbers (1-5) give colour shifts and stat changes, big numbers break things faster. |
| **Exclusion Type** | `None`: every byte in the range can be corrupted (classic Nightmare).<br>`Auto`: picks the type from the loaded file's header/extension (falls back to None if the file isn't recognised).<br>Any platform: the protected ranges of that platform are never corrupted (see the table below). Picking a type also ticks *Avoid crash-prone values*. |
| **Avoid crash-prone values** | Random values are never `0x00` or `0xF0`-`0xFF`. On many CPUs these are BRK/NOP/return/branch opcodes or "end of list" markers, so they crash games more often. A safe value is used instead. This was always on in the old Exclusion Engine. |
| **Excluded Addresses** (list) | Shows the protected ranges for the selected type. Ranges that depend on the file (e.g. the NES trainer, the SNES LoROM/HiROM header, GameCube DOL/FST offsets) are read from the loaded file. |

With an exclusion type, every corruption still lands on an allowed byte: Intensity 100 really means 100 corrupted bytes, none of them wasted on a protected byte. Excluded ranges from Bisect Mode / stash `EXCLUDE` lines are left out the same way.

<details>
<summary><b>Exclusion types and what they protect</b></summary>

| Exclusion Type | Protected (always) | Also read from the file |
| :--- | :--- | :--- |
| `NES` | iNES / NES 2.0 header (0x00-0x0F) | 512 byte trainer, NMI/RESET/IRQ vectors at the end of PRG-ROM |
| `SNES` | LoROM (0x7FB0-0x7FFF) and HiROM (0xFFB0-0xFFFF) headers + vectors | 512 byte copier header (the header ranges then move 0x200 bytes up), ExHiROM |
| `N64` | ROM header, IPL3 boot code (0x0-0xFFF), game entry stub (0x1000-0x10FF). The boot checksum is rewritten after corrupting ([Fix Checksums](Getting-Started.md#fix-checksums)). | Checks the byte order (.z64/.v64/.n64) |
| `N64 (STRICT CRC)` | Header, IPL3 and the whole first 1 MB of game code (the part the boot checksum covers) | For ROMs whose checksum LRC can't rewrite (unknown CIC / bad dump) |
| `GAMEBOY` | RST/interrupt vectors, entry point, Nintendo logo, cartridge header + checksums (0x0-0x14F) | |
| `GBA` | ARM entry branch, Nintendo logo, header + complement check, multiboot entry (0x0-0xDF) | |
| `NDS` | Cartridge header, reserved header area, secure area (0x0-0x7FFF) | File name table (FNT), file allocation table (FAT) |
| `GAMECUBE` | boot.bin + bi2.bin (0x0-0x243F) | Apploader, DOL header, FST |
| `WII` | Disc header, partition tables and region settings | Partition headers |
| `PS1` | First 24 sectors: system area, license data, volume descriptors, path tables, root directory | Raw (2352) vs cooked ISO (2048) sector size, the PS-X EXE header |
| `PS2` | First 24 sectors: system area, volume descriptors, path tables, root directory | Raw vs cooked sector size, the boot ELF header + program/section tables |
| `SEGA` | 68000 vector table + Genesis / Mega Drive header (0x0-0x1FF) | |
| `SMS/GG` | Z80 RST + interrupt vectors (0x0-0xFF) | `TMR SEGA` header |
| `SEGA CD/SATURN` | First 24 sectors: system area / IP.BIN, volume descriptors, path tables, root directory | Raw vs cooked sector size |
| `DREAMCAST` | IP.BIN boot sectors, volume descriptors (data track) | Raw vs cooked sector size |
| `PC ENGINE` | HuC6280 interrupt + reset vectors | 512 byte copier header (the vectors then move 0x200 bytes up) |
| `ATARI 2600` / `VIRTUAL BOY` / `WONDERSWAN` | (nothing fixed) | 6502 vectors at the end of each bank / ROM header + vectors / boot jump + ROM footer |
| `ATARI 7800` / `ATARI LYNX` | A78 / LNX header (+ Lynx boot loader) | 7800: 6502 vectors at the end of the file. The A78 header is only protected when the file has one (headerless `.bin` dumps start with game data) |

**Auto** detects disc images from their contents, not just the extension: Sega CD, Saturn and Dreamcast discs by the system ID in IP.BIN (cooked or raw), PS1 vs PS2 discs (both say `PLAYSTATION`) by `SYSTEM.CNF` (`cdrom0:` = PS2, `cdrom:` = PS1).
| `EXE` | DOS (MZ) header | DOS stub, PE signature, COFF/optional header, data directories and section headers |
| `ELF` | ELF identification + header | Program and section header tables |
| `BIOS` | Start of the image (option ROM / flash descriptor header) | Boot block + reset vector at the end of the image |
| `BMP` / `WAV` / `JPEG` | File + info header / RIFF + fmt chunk / start of image marker | Everything before the pixel data (incl. palette) / every chunk before the samples / every marker and table up to the scan data, plus the end marker |

</details>

---

<a id="merge-engine"></a>
## Merge Engine

Mixes the bytes of a second file (the *merge file*) into the file you are corrupting. The merge file is only read, never changed. Try a picture, a song, another ROM, or random noise.

| Setting | What it does |
| :--- | :--- |
| **File Selection** / **Open File** | The file the bytes are taken from. |
| **Replace byte with the byte at the same position** | On: the merge byte comes from the **same offset** in the merge file. If the merge file is smaller, it wraps around (offset mod merge file size). Off: a **random byte** from anywhere in the merge file is used. |
| **Corruption Type** | `NONE`: the byte is replaced with the merge byte (a straight copy/overlay).<br>`RANGE`: the byte becomes the difference between the two: `|byte − merge byte|`. |
| **Mod 256** | Switches to wrapping arithmetic. With `NONE` the byte becomes `(byte + merge byte) mod 256` (adds the files together). With `RANGE` it becomes `(byte − merge byte) mod 256` (a signed difference that wraps instead of being made positive). |

---

<a id="logic-engine"></a>
## Logic Engine

Applies a bitwise operation to each corrupted byte.

| Setting | What it does |
| :--- | :--- |
| **Operation Type** | `AND`: keeps only the bits that are also set in *Value* (values get smaller).<br>`OR`: sets the bits of *Value* (values get bigger).<br>`XOR`: flips the bits of *Value*. XOR 1 is the smallest possible change, XOR 128 flips the top bit.<br>`NOT`: flips every bit (ignores *Value*).<br>`NAND` / `NOR`: NOT of AND / OR.<br>`SWAP`: swaps the byte with the **next** byte (ignores *Value*).<br>`SHIFT`: copies the byte that is *Value* bytes **ahead** into this byte (Value 0 does nothing). |
| **Value** | The second operand (0-255). |
| **Randomize Value** | Uses a random value (0-255) for every byte instead of *Value*. |
| **Select random operation type** | Uses a random operation for every byte. Turn both on for maximum chaos. |

---

<a id="lerp-engine"></a>
## Lerp Engine

**L**inear int**erp**olation: every corrupted byte is replaced with a blend of its two neighbours. This smooths and smears data instead of adding noise.

| Setting | What it does |
| :--- | :--- |
| **Split Value** (0.0 - 1.0) | Where between the neighbours the new value lands: `new = next + Split × (previous − next)`.<br>`0.0` = copy of the **next** byte, `0.5` = the **average** of both neighbours, `1.0` = copy of the **previous** byte. The slider moves in steps of 0.1, type any decimal (both `0.25` and `0,25` work). |

At the very start/end of the file the byte itself is used in place of the missing neighbour.

---

<a id="manual-engine"></a>
## Manual Engine

You choose exactly what happens to each corrupted byte. Every ticked option is applied, in this order:

| Option | What it does |
| :--- | :--- |
| **Increment byte** (*Increment Int*) | Adds the number to the byte (negative numbers subtract). Clamped to 0-255. |
| **Shift Right X bytes** (*Shift int*) | Copies the byte **X bytes further** into the file (only if that position is still inside the selected range). Repeats data forwards. |
| **Make byte equal** | Sets the byte to that value (e.g. 0 to blank things out). |
| **Replace** *A* **with** *B* | Only bytes that are exactly *A* become *B*. Everything else is left alone. Great for swapping one tile, colour or command for another. |
| **Paste byte to Random Place** | The byte takes the value of a random byte from the selected range. |
| **Repeat byte** (*Selection Length*) | Copies *Selection Length* bytes starting at this byte to that many random places in the range. Spreads chunks of data around. |
| **Do math operations on byte** (`*`, `/`, `^` *by*) | Multiplies (capped at 255), divides (whole numbers) or raises the byte to a power (wrapped mod 256). |

---

<a id="vector2-engine"></a>
## Vector2 Engine

Finds **32-bit floating point numbers** in the file and corrupts them. Floats are how most 3D games store positions, sizes, rotations, speeds, camera settings, physics and model vertices. Corrupting them gives stretched, wobbly, mirrored or exploded geometry instead of the noise you get from byte engines.

A file doesn't say which bytes are floats, so the engine **guesses**. It only treats 4 bytes as a float when they decode to a "sensible" number (inside the magnitude limits below), and optionally only when the data around them looks like float data too. Most of the settings tune that guess.

**Unit:** Intensity = number of floats (or X,Y pairs) to corrupt. For each one, up to 256 random positions are tried until a float that passes the checks is found. *Every Nth* = every Nth float/vector position.

| Setting | What it does |
| :--- | :--- |
| **Corruption Type** | `NUDGE`: multiplies the float by a random factor between *(1 − Nudge %)* and *(1 + Nudge %)*. The most controllable mode.<br>`REPLACE`: replaces the float with a random number from *Replacement values*.<br>`NEGATE`: flips the sign (mirrors positions, reverses speeds).<br>`RANDOM`: a random number between **−Up to** and **+Up to** (see below, so lower *Up to* for this mode).<br>`SWAPXY`: swaps the two floats of an X,Y pair (needs *Treat as Vector2*). |
| **Replacement values** | The numbers `REPLACE` picks from, separated by commas (spaces and `;` also work), always with a `.` as the decimal point. Example: `-1, 0, 1` snaps things to -1/0/1, `0` flattens, `1000` makes things huge. Only used by `REPLACE`. |
| **Nudge amount (%)** | How far `NUDGE` can move a value. 25 = up to 25% bigger or smaller (a random amount each time). 5 is subtle, 100 can double or zero a value, 1000+ is wild. If *Include 0.0 values* is on, zeros are nudged to a random value between −N/100 and +N/100. |
| **Only floats from** (minimum magnitude) | Floats whose **absolute value is smaller** than this are not treated as floats. Tiny numbers like `1.4e-45` are almost always integers, flags or random bytes that happen to decode as a float, so this filters them out. Default `0.0001`. Lower it to include very small real values. |
| **Up to (magnitude)** | Floats whose **absolute value is bigger** than this are not treated as floats (huge numbers like `3e+21` are usually text or code). Default `100000`. Also the range of the `RANDOM` mode (−Up to … +Up to). |
| **Treat as Vector2 (X,Y pairs)** | Works on two floats next to each other (X and Y). Both have to pass the checks and both are corrupted. *Every Nth* then counts pairs. Turn it off for single values, or for 3D vertex data (X, Y, Z) so each float is checked on its own. |
| **4-byte aligned floats only** | Only looks at offsets that are a multiple of 4. Real float data is nearly always aligned, so this cuts false hits a lot. Turn it off for packed formats. |
| **Big-endian (GC/Wii/PS3/360)** | Byte order of the floats. Little-endian (off) is used by PC, PS1/PS2/PSP, DS/3DS, Switch and Xbox. Big-endian by GameCube, Wii, Wii U, PS3, Xbox 360 and N64. For N64 ROMs the byte order is detected from the header (.z64/.v64/.n64) and overrides this checkbox. |
| **Include 0.0 values** | Lets zeros be corrupted too. Off by default, because zero is mostly padding/empty data, not a float. |
| **Skip code / compressed data** | Checks the 8 words on each side of a candidate. They must look like float/struct data: at least 40% float-like, at least 60% "data-like" (floats, zeros, small integers, RAM pointers) and fewer than 2 MIPS instruction patterns. Code and compressed data decode as plausible floats surprisingly often, so leave this on unless you're corrupting a plain float table. |

For N64 ROMs the header and IPL3 boot code (0x0-0xFFF) are always skipped.

---

<a id="blocks-engine"></a>
## Blocks Engine

Works on whole **blocks** of data instead of single bytes, like the pixel sorting and data moshing techniques used in glitch art. A block is split into **elements**, and the engine reorders or rewrites the elements. Only bytes that actually change are written to the stash.

**Unit:** Intensity = number of random blocks. *Every Nth* = every Nth block from the start byte (1 = every block).

| Setting | What it does |
| :--- | :--- |
| **Block operation** | `SORT`: sorts the elements by value (by the sum of their bytes for multi-byte elements, roughly brightness for pixels). The classic pixel sort.<br>`REVERSE`: reverses the order of the elements.<br>`SHUFFLE`: shuffles the elements randomly.<br>`ROTATE`: moves every element along by a random amount (wrapping around).<br>`SMEAR`: repeats one random element across the whole block.<br>`ECHO`: averages each byte with the byte *Echo distance* bytes earlier (a delay/echo effect).<br>`RANDOM`: a random operation for every block. |
| **Block size (bytes)** | How big each block is (2 bytes to 16 MB). It's rounded down to whole elements and to the selected range. For images, about one or a few rows of pixels (width × bytes per pixel) gives the best streaks. |
| **Element size (bytes)** | How many bytes move together: `1` = bytes, `2` = 16-bit audio samples, `3` = 24-bit RGB pixels, `4` = 32-bit RGBA pixels / 16-bit stereo audio / 32-bit values. Getting this right keeps colours and samples intact while they're moved. |
| **Echo distance (bytes)** | `ECHO` only: how far back the echo comes from. For 44.1 kHz 16-bit stereo audio, 17640 bytes = 0.1 s. |
| **Sort descending** | `SORT` only: sorts from high to low instead. |

Tip: use [ByteView](ByteView.md) to find the image/audio data and drag over it to target it. For compressed video use the Datamosh Engine. Blocks only gives a datamosh look on uncompressed video or raw images.

---

<a id="pointer-engine"></a>
## Pointer Engine

Finds **pointers** (numbers in the file that are the address of something else: a texture, model, sound, string, script, level...) and swaps, shifts or copies them. The data itself stays intact. The game just loads a *different* asset than it asked for, which gives "wrong texture / wrong model / wrong text" corruptions that crash much less than byte corruption.

Pointers that sit in **tables** (several pointers in a row) usually point to the same kind of asset, so swapping two entries of one table is the most stable corruption.

**Unit:** Intensity = number of pointer operations. *Every Nth* = every Nth pointer found.

| Setting | What it does |
| :--- | :--- |
| **Pointer type** | Which platform's pointers to look for (see the table below). `Auto` picks one from the file header. The line under the settings describes the selected type. |
| **Corruption Type** | `SWAP`: swaps two pointers (a partner of the same kind from the same table when possible): asset A shows asset B and the other way round.<br>`SHIFT`: moves a pointer a few bytes forward/back: shifted textures, offset animations, broken palettes.<br>`DUPLICATE`: copies a neighbouring pointer: two entries use the same asset.<br>`RANDOMTARGET`: points at the target of any other pointer of the same kind. Wilder, crashes more. |
| **Max shift (bytes)** | `SHIFT` only: the largest move. Moves keep the platform's alignment (4 bytes for most, 8 for N64 display lists, 1 byte for 16/24-bit pointers). |
| **Table window (bytes)** | `SWAP` / `DUPLICATE` look for a partner within this many bytes. Smaller = partners from the same table (more stable), bigger = more variety. `SWAP` falls back to any pointer of the same kind if none is close. |
| **Only pointers in tables** | Only uses pointers that have other pointers right next to them. Strongly recommended: a lone number that looks like an address is often just a number. For 16-bit and 24-bit pointers a table needs 3-4 entries in a row that point close to each other. |
| **Skip function pointers** | Leaves pointers to **code** alone (swapping those jumps into the wrong function and crashes). GBA/DS: odd addresses (Thumb code). N64: pointers to a function start in the boot segment. Windows EXE: pointers into code sections. Turned off, GBA/DS Thumb pointers (odd addresses) are included too. |
| **Custom range (hex)** / **Custom range is big-endian** | `Custom range` only: the lowest and highest address that counts as a pointer, and the byte order. Use it for platforms not in the list (e.g. `02000000`-`023FFFFF` for DS RAM). |
| **N64 display list targets** | `N64 display lists` only: which display list commands to change. **Textures** (`G_SETTIMG`), **Vertices** (`G_VTX`, mixes up model shapes), **Sub display lists** (`G_DL`, swaps whole model parts). |

<details open>
<summary><b>Pointer types</b></summary>

| Pointer type | Size / byte order | Addresses | Notes |
| :--- | :--- | :--- | :--- |
| `N64 RAM pointers` | 32-bit, big-endian | `0x80000400`-`0x807FFFFF` | .z64/.v64/.n64 byte order detected, header + boot code skipped |
| `N64 display lists` | 32-bit, big-endian | Segmented addresses in F3D/F3DEX/F3DEX2 commands | The neighbouring commands must be valid display list commands too. The microcode (F3D/F3DEX or F3DEX2) is detected from the `G_ENDDL` commands, so only its own opcodes are used (in F3D `0x01` is a matrix, not a vertex buffer) |
| `GBA ROM pointers` | 32-bit, little-endian | `0x08000000`+ (must point inside the ROM) | |
| `DS RAM pointers` | 32-bit, little-endian | `0x02000000`-`0x023FFFFF` | |
| `PS1 RAM pointers` | 32-bit, little-endian | `0x80010000`-`0x801FFFFF` | |
| `GameCube/Wii RAM pointers` | 32-bit, big-endian | `0x80003100`-`0x817FFFFF` | |
| `NES 16-bit pointers` | 16-bit, little-endian | `0x8000`-`0xFFF9` | Only PRG-ROM is scanned (not CHR graphics or the header). Tables of 4+ |
| `SNES 24-bit pointers` | 24-bit (bank + address), little-endian | Must map into the ROM | LoROM / HiROM detected from the internal header, copier header skipped, WRAM (banks 7E/7F) ignored |
| `Game Boy 16-bit pointers` | 16-bit, little-endian | `0x0150`-`0x7FFF` | Bank 0 + switchable bank. Tables of 4+ |
| `Genesis/Mega Drive ROM pointers` | 32-bit, big-endian | `0x200`-`0x3FFFFF` (= file offsets) | Word-aligned (68000), vector table + header skipped |
| `Saturn RAM pointers` | 32-bit, big-endian | `0x06000000`-`0x060FFFFF` (high work RAM) | Auto picks it for Saturn discs (Sega CD discs get file offsets) |
| `Dreamcast RAM pointers` | 32-bit, little-endian | `0x8C010000`-`0x8CFFFFFF` | |
| `PS2 RAM pointers` | 32-bit, little-endian | `0x00100000`-`0x01FFFFFF` | Best on ELFs / files extracted from the disc |
| `PSP RAM pointers` | 32-bit, little-endian | `0x08804000`-`0x09FFFFFF` | Best on EBOOT/PRX files. Auto detects PSP ISOs |
| `Windows EXE/DLL (32-bit)` | 32-bit, little-endian | ImageBase … ImageBase + SizeOfImage | Read from the PE header. Must point into a section, PE headers are skipped |
| `File offsets (little/big-endian)` | 32-bit | `0x100` … file size | For any file format that stores offsets to its own data (archives, models, save files) |
| `Custom range` | 32-bit | Your range | |

</details>

Compressed data can't be seen by this engine. Use the [Compression Engine](#compression-engine) for it.

---

<a id="compression-engine"></a>
## Compression Engine

Many games compress their textures, models and levels. Corrupting compressed bytes directly usually just crashes the decompressor. This engine **unpacks** each compressed block, corrupts the real data inside, **packs it again** and writes it back into the same space. Textures, models and levels then glitch instead of crashing.

**Unit:** Intensity = number of byte corruptions, spread over the blocks in the selected range (bigger blocks get more). *Every Nth* = every Nth **unpacked** byte of each block.

| Setting | What it does |
| :--- | :--- |
| **Corruption Type** / **Tilt amount** | The same as the Nightmare+ Engine (`RANDOM`, `RANDOMTILT`, `TILT` ± tilt amount), applied to the unpacked data. |
| **Formats** | Which compression formats to look for. `Yaz0` (N64/GameCube/Wii), `Yay0` and `MIO0` (N64) are found by their signature. `LZ10` (GBA/DS) and `LZ11` (DS) have no signature, so they're only used at DS file system entries, GBA ROM pointer targets or offset 0. |
| **Scan for blocks** | Lists which compressed blocks the engine can see in the loaded file, without corrupting anything. |

If a corrupted block packs bigger than the original space, the engine retries with fewer corruptions. If it still doesn't fit, it corrupts the *literal* bytes inside the compressed stream instead. The compression stays valid, it's just less precise.

---

<a id="datamosh-engine"></a>
## Datamosh Engine

Real datamoshing for **AVI** videos. Instead of changing bytes it removes and repeats whole frames:

* Removing **keyframes** (I-frames) makes the motion of the next scene smear over the old picture (the classic "melt").
* Repeating **motion frames** (P-frames) keeps pushing the pixels in the same direction ("bloom").

Frame types are read from the video data: MPEG-4 I/P/B-VOPs, and for H.264 the IDR slices plus the slice type of every other slice (so non-IDR I-frames from open-GOP encodes count as keyframes too). B-frames and non-reference frames are never repeated, because nothing is predicted from them: repeating one only freezes the picture. Frames that overlap a Bisect / stash `EXCLUDE` range are left alone.

**Unit:** *Every Nth* = every Nth keyframe/frame (Every Nth = 1 removes them all), Intensity = that many random ones.

| Setting | What it does |
| :--- | :--- |
| **Mosh type** | `REMOVE_KEYFRAMES`: removes keyframes (melt).<br>`DUPLICATE_FRAMES`: repeats P-frames (bloom). B-frames are skipped.<br>`BOTH`: both at once. |
| **Keep first keyframes** | How many keyframes at the start are never removed, so the video starts with a picture (1 is usual). |
| **Repeat each frame (times)** | `DUPLICATE_FRAMES` / `BOTH`: how many times each chosen P-frame is repeated. |
| **Replace removed keyframes with the previous frame** | On: a removed keyframe is replaced by a copy of the previous frame, so the video keeps its length and the audio stays in sync. Off: keyframes are dropped (shorter video, the audio can drift). |

**Preparing a video:** Xvid/DivX/MPEG-4 AVIs work best. H.264 AVIs work with `DUPLICATE_FRAMES`, but players may skip frames after removed keyframes. Convert with few keyframes and no B-frames:

```
ffmpeg -i input.mp4 -c:v mpeg4 -vtag XVID -q:v 2 -g 600 -bf 0 -c:a libmp3lame output.avi
```

Then use `REMOVE_KEYFRAMES` + *Corrupt every nth byte* = 1 + *Keep first keyframes* = 1. Play the result in VLC/mpv, or bake it into an MP4 with `ffmpeg -i moshed.avi -c:v libx264 -crf 18 final.mp4`.

---

<a id="source-code-engine"></a>
## Source Code Engine

Reads the file as **text** and corrupts the **values** in it instead of raw bytes, so the file stays readable and (mostly) valid. Good for game config files, JSON, INI, Lua/Python scripts and source code. Comments are skipped, and JSON keys are never changed.

**Unit:** Intensity = number of values to corrupt. *Every Nth* = every Nth value. Start/End byte limit which part of the file is used.

| Setting | What it does |
| :--- | :--- |
| **Values to corrupt** | Which kinds of values can be changed: **Booleans** (`true` ↔ `false`), **Integers** (`10`, `0xFF`), **Decimals** (`3.14`), **Strings** (`"text"`), **Null values** (`null`/`nil`/`None` become `0`, `1` or `""`), **Operators** (`==` ↔ `!=`, `<` ↔ `>`, `<=` ↔ `>=`, `+` ↔ `-`, `*` ↔ `/`, `++` ↔ `--`, `&&` ↔ `\|\|`, `+=` ↔ `-=` …), **Variable names** (swapped with other names from the same file). |
| **Number corruption** | How integers and decimals change:<br>`NUDGE`: ± up to *Strength* % of the value.<br>`SCALE`: multiplied or divided by 1 + *Strength* × (0.5 to 1.5).<br>`SIGNFLIP`: `5` → `-5`.<br>`MAGIC`: "interesting" values (`0`, `1`, `-1`, `255`, `65535`, `2147483647` …). For decimals it either rounds (3.14 → 3 or 4) or picks a magic value.<br>`DIGITS`: changes one digit.<br>`RANDOM`: one of the above, picked for every number. |
| **Strength (%)** | How strong `NUDGE` and `SCALE` are, and how many letters `SUBSTITUTE` changes in a string (Strength 100 = up to half of the letters). |
| **String corruption** | `SHUFFLE` the characters, `REVERSE` them, `TRUNCATE` (cut off the end), `SWAP` with another string from the file (same quote type), `SUBSTITUTE` random letters, `REPEAT` a piece of the string, or `RANDOM` (one of these each time). Escape sequences (`\n`, `\"`) are kept together, so the quotes never break. |
| **Comment style** | Which comments to skip. `AUTO` picks from the file extension. `CLIKE` (`//`, `/* */`), `HASH` (`#`), `DASHDASH` (`--`), `SEMICOLON` (`;`), `XML` (`<!-- -->`), `BASIC` (`'` / `REM` style), `NONE` (nothing is a comment). |

Every change is saved to the stash as `[x] File(offset).VARLINE("name",line).SET("new text")`, so source code stashes replay, edit and mutate like any other stash.
