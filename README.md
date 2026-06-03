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
| **Multi-Engine Support** | Switch between Nightmare, Merge, Logic, and Lerp engines seamlessly. |
| **Precision Control** | Use "Every Nth Byte" or "Intensity Mode" for predictable or chaotic corruption. |
| **Smart Saving** | Use **File Saves** for full copies or **Stash Saves** (address-only) to save massive amounts of space. |
| **Stash Editor** | Fine-tune specific corrupted bytes without re-running the entire process. |
| **ByteView** | Visualize binary data as RGB/Grayscale pixels to "see" the bytes change. |
| **Automation** | Automate randomization of intensity, byte ranges, and engine selection. |
| **Batch Processing** | Drag-and-drop entire folders or multiple files for mass corruption. |

---

## Corruption Engines

LunarROMCorruptor utilizes distinct algorithms to achieve different "flavors" of corruption.

| Engine | Logic Type | Best For... |
| :--- | :--- | :--- |
| **Nightmare** | Random/Tilt | Classic, chaotic bit-flipping and value shifting. |
| **Merge** | Pattern Injection |  Injection	Overwriting random ROM addresses with data from any source file (e.g., injecting video noise into a ROM). |
| **Logic** | Bitwise Ops | Advanced manipulation using `AND`, `OR`, `XNOR`, `SHIFT`, etc. |
| **Lerp** | Interpolation | Smooth, gradient-based transitions between byte values. |
| **Manual** | User Defined | Complete manual control over every single operation. |

#### The Nightmare Engine Modes:
*   `RANDOM`: Sets selected bytes to a random value (0-255).
*   `RANDOMTILT`: Randomly adds or subtracts a user-defined offset.
*   `TILT`: Applies a fixed mathematical shift (e.g., `Value + 10`).

#### The Merge Engine Modes:
*   `File Selection`: The file that will have its bytes copied from. This file wont be changed.
*   `Replace byte with the byte at the same position`: The merge file and the ROM will be checked to see if there is a byte in the random address LRC chose. If there is the merge file byte will override the ROM byte at the ROM byte address selected.
*   `Corruption Type`: NONE or RANGE.
*   `MOD 256`

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
