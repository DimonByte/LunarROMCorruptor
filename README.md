<div align="center">

<img src="static/images/icon.png" alt="LunarROMCorruptor Icon" width="128" height="128" />

# LunarROMCorruptor

**Versatile, multi-engine file corruption for ROMs and any binary data.**

[![GitHub issues](https://img.shields.io/github/issues/DimonByte/LunarROMCorruptor?distro=true)](https://github.com/lloyd99901/LunarROMCorruptor/issues)
[![GitHub stars](https://img.shields.io/github/stars/DimonByte/LunarROMCorruptor)](https://github.com/lloyd99901/LunarROMCorruptor/stargazers)
[![GitHub license](https://img.shields.io/github/license/DimonByte/LunarROMCorruptor)](https://github.com/lloyd99901/LunarROMCorruptor/blob/master/LICENSE)
![GitHub CodeQL](https://github.com/DimonByte/LunarROMCorruptor/actions/workflows/codeql.yml/badge.svg)
![GitHub DOTNET](https://github.com/DimonByte/LunarROMCorruptor/actions/workflows/dotnet-desktop.yml/badge.svg)

</div>

---

## Overview

LunarROMCorruptor is a advanced tool designed to corrupt binary data through various algorithmic engines. While optimized for ROM hacking, its capabilities extend to **any file format**. Whether you are looking for random bit-flips or complex mathematical interpolation, LunarROMCorruptor provides the precision tools needed for high-quality glitch art.

<div align="center">
    <img src="https://raw.githubusercontent.com/lloyd99901/LunarROMCorruptor/master/static/images/MainInterface.png" alt="Main Interface Screenshot" width="60%" />
</div>

---

## Key Features

| Feature | Description |
| :--- | :--- |
| **Multi-Engine Support** | Switch between Nightmare, Merge, Logic, Lerp, Manual, Exclusion, Vector2 and Source Code engines seamlessly. |
| **Any File Size** | Files are no longer loaded into memory, only the changed bytes are written. There is no 2 GB limit anymore. |
| **Background Corruption** | Corruption runs in the background with a progress bar, click the corrupt button again to cancel (the output is reverted). |
| **Precision Control** | Use "Every Nth Byte" or "Intensity Mode" for predictable or chaotic corruption. |
| **Smart Saving** | Use **File Saves** for full copies or **Stash Saves** (address-only) to save massive amounts of space. |
| **Stash Editor** | Fine-tune specific corrupted bytes without re-running the entire process. |
| **ByteView** | Visualize binary data as RGB/Grayscale pixels to "see" the bytes change. **Diff** mode highlights every changed byte, **Entropy** mode shows where text, tables, code and compressed data are. |
| **ByteView Targeting** | Drag over the ByteView image to set the Start/End byte to exactly that area of the file. |
| **Automation** | Automate randomization of intensity, byte ranges, and engine selection. |
| **Batch Processing** | Drag-and-drop entire folders or multiple files for mass corruption. |

---

## Corruption Engines

LunarROMCorruptor utilizes distinct algorithms to achieve different "flavors" of corruption.

| Engine | Logic Type | Best For... |
| :--- | :--- | :--- |
| **Nightmare** | Random/Tilt | Classic, chaotic bit-flipping and value shifting. |
| **Merge** | Pattern Injection |  Injection	Overwriting random ROM addresses with data from any source file (e.g., injecting video noise into a ROM). |
| **Logic** | Bitwise Ops | Advanced manipulation using `AND`, `OR`, `XOR`, `NAND`, `NOR`, `SWAP`, `SHIFT`, etc. |
| **Lerp** | Interpolation | Smooth, gradient-based transitions between byte values. |
| **Manual** | User Defined | Complete manual control over every single operation. |
| **Exclusion** | Header Protection | Nightmare-style corruption that never touches the headers/vectors of the selected platform or file format (NES, SNES, N64, GB, GBA, NDS, GameCube, Wii, PS1, PS2, Genesis, SMS/GG, Saturn, Dreamcast, PC Engine, Atari, EXE, ELF, BMP, WAV, JPEG and more). |
| **Blocks** | Glitch Art | Sorts, reverses, shuffles, rotates, smears or echoes whole blocks of data (pixel sorting / data moshing). Element size keeps RGB pixels or audio samples intact. |
| **Vector2** | Float Corruption | Finds 32-bit floats (positions, speeds, sizes) and nudges, negates, swaps or replaces them. |
| **Source Code** | Text Literals | Corrupts values in source/text files: `true` becomes `false`, `3.14` becomes `4`, strings get shuffled. Comments are skipped. |

#### The Nightmare Engine Modes:
*   `RANDOM`: Sets selected bytes to a random value (0-255).
*   `RANDOMTILT`: Either generates a random byte value, or randomly adds or subtracts a user-defined offset.
*   `TILT`: Adds or subtracts a fixed mathematical shift (e.g., `Value + 10`).

#### The Merge Engine Modes:
*   `File Selection`: The file that will have its bytes copied from. This file wont be changed.
*   `Replace byte with the byte at the same position`: The merge file and the ROM will be checked to see if there is a byte in the random address LRC chose. If there is the merge file byte will override the ROM byte at the ROM byte address selected.
*   `Corruption Type`: NONE or RANGE.
*   `MOD 256`

#### Stash format:
*   Byte engines: `[x] File(offset).SET(value)`
*   Source Code Engine: `[x] File(offset).VARLINE("name",line).SET("new value")`

#### Restore:
*   If the save location is a different file, the original is never modified and Restore copies the original back.
*   If the save location is the original file, the original bytes are kept in a temporary undo journal so Restore works while LRC is open.

#### The Lerp Engine Modes:
*   `Split Value`: Sets the interpolation value.

#### The Logic Engine Modes:
*   `Operation Type`: AND, OR, XOR, NOT, NAND, NOR, SWAP, SHIFT

---

## Visualizing Data with ByteView

The built-in **ByteView** transforms binary data into a visual image. By mapping byte values to RGB or grayscale pixels, you can observe patterns that are invisible in a standard hex editor. The view updates in real-time as you apply corruption engines.

<div align="center">
    <img src="https://raw.githubusercontent.com/lloyd99901/LunarROMCorruptor/master/static/images/ByteView.png" alt="ByteView Screenshot" width="50%" />
</div>

---

## Safety & Disclaimer

> [!CAUTION]
> **CORRUPTION WARNING**
>
> *   **Epilepsy Warning:** Corruption can result in rapid, flashing imagery. Use with caution.
> *   **System Stability:** Corrupting critical system files (like `system32`) can cause irreversible OS damage. Always use a Virtual Machine for testing.
*   **Anti-Cheat Risk:** Using this tool on online games or protected software **will likely result in permanent account bans**. 
*   **No Warranty:** This software is provided **"AS IS"**. The developer is not responsible for any data loss, hardware instability (BSOD), or loss of access to digital accounts.

---

## Technical Requirements

*   **Runtime:** `.NET Core 10.0`
*   **Development Environment:** `Microsoft Visual Studio 2026` (Recommended)
*   **Platform:** Windows

## Contributing

Contributions welcome!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/Feature`)
3. Commit your Changes (`git commit -m 'Add some Feature'`)
4. Push to the Branch (`git push origin feature/Feature`)
5. Open a Pull Request

## License

Distributed under the **MIT License**. See `LICENSE` for more information.

<div align="center">
<sub>Built for Corruption</sub>
</div>
