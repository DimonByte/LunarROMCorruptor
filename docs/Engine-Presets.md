# Engine Presets

[← Back to the README](../README.md) · [Getting Started](Getting-Started.md) · [Corruption Engines](Corruption-Engines.md) · [Stashes, Saves & Bisect](Stash-and-Saves.md) · [ByteView](ByteView.md) · [Automation & Multi-File](Automation-and-Multi-File.md)

A preset is a saved set of engine settings. The **Preset** bar is at the top of the Corruption Engine tab:

| Control | What it does |
| :--- | :--- |
| **Preset** list | Pick a preset to load it into the engine right away. ★ = built into LRC, 👤 = yours. The line underneath describes the selected preset. |
| **Save As...** | Saves the current settings of the engine as a 👤 user preset (asks for a name and an optional description). Saving with the same name replaces it. |
| **Delete** | Deletes the selected user preset (built-in presets can't be deleted). |
| **Import...** | Adds `.lrcpreset` files someone shared, switches to their engine and loads them. Dragging a `.lrcpreset` onto LRC does the same. |
| **Open Folder** | Opens the `UserCorruptionEnginePresets` folder. |

Presets only contain **engine settings**. The byte range, intensity and the file are not saved, because they depend on the file you corrupt. Descriptions of the built-in presets suggest a range or intensity where it matters. The Merge Engine's merge file path isn't saved either (it only exists on your computer).

---

## Sharing presets

User presets are stored as small JSON files in `UserCorruptionEnginePresets` (next to `LunarROMCorruptor.exe`), named `<Engine> - <Name>.lrcpreset`. To share one, send the file. To use someone else's, **Import** it, drag it onto LRC, or copy it into the folder (subfolders are fine).

```json
{
  "Format": "LunarROMCorruptor engine preset",
  "Version": 1,
  "Engine": "Vector2 Engine",
  "Name": "Mario 64 jelly",
  "Description": "Wobbly N64 models. Range 0x101000 to the end, intensity 300.",
  "Settings": {
    "VectorModeComboBox": "NUDGE",
    "VectorNudgeNumericUpDown": "40",
    "VectorPairsChkBox": "true"
  }
}
```

`Settings` maps each setting (by its control name) to its value. A preset may leave settings out: the missing ones keep their current values. Settings LRC doesn't know (e.g. from a newer version) are ignored and listed in the log.

---

## Built-in presets

| Engine | ★ Preset | What it does |
| :--- | :--- | :--- |
| Nightmare+ | Classic Nightmare | The original Nightmare Engine: random bytes, nothing excluded. |
| | Safe (auto-detect platform) | Random bytes, the detected platform's header/boot code protected, crash-prone values avoided. A good first try for any ROM. |
| | Gentle tilt | ± 3 instead of random values: colours, stats and positions drift. Header protected. |
| | N64 safe mix | RANDOMTILT ± 8 with the N64 header, IPL3 and entry code protected. |
| | NES / SNES / Genesis safe | Random values, header + interrupt vectors of the detected console protected. |
| | Disc image (PS1) safe | Raw PS1 BIN with the system area and file system intact. |
| Merge | Overlay / Add (wrap around) / Difference / Random merge bytes | Copy, add (mod 256), difference, or random bytes from the merge file. |
| Logic | Bit flip (XOR 1) | The smallest possible change. |
| | Colour scramble (XOR 128) | Flips the top bit: palettes jump. |
| | Darken (AND 0x0F) | Values get smaller. |
| | Byte swap / Logic chaos | Swap with the next byte / random operation and value. |
| Lerp | Even blend (0.5) / Copy next byte (0.1) / Copy previous byte (0.9) | Average or smear neighbouring bytes. |
| Manual | Increment +1 / Decrement -1 / Zero out / Halve values / Repeat chunks / Paste random bytes | One clear operation each. |
| Vector2 | N64 models (big-endian) | Nudges N64 floats by up to 25%, code skipped. |
| | N64 wild | Random floats between −1000 and 1000. |
| | Generic 3D model (little-endian) | PC / PlayStation / Xbox / Switch model files, each float nudged up to 15%. |
| | Generic 3D model (big-endian) | GameCube / Wii / PS3 / 360 model files. |
| | Subtle wobble / Mirror (negate) / Snap to -1 / 0 / 1 / Swap X/Y | Small nudges, flipped signs, snapped values, swapped pairs. |
| Blocks | Pixel sort (RGB) / Pixel sort (RGBA / 32-bit) | Classic pixel sorting for raw images. |
| | Audio echo (16-bit stereo) / Audio stutter | Echo 0.1 s back / repeated samples for WAV audio. |
| | Shuffle chunks / Random block glitch | Mixed-up tiles, tables and text. |
| Pointer | Auto: swap in tables (safe) | The most stable pointer corruption for any supported platform. |
| | N64 texture swap / N64 model mix | Swaps textures / vertices + model parts in N64 display lists. |
| | GBA asset swap / SNES table swap / NES table swap | Platform specific table swaps. |
| | Off by a bit (shift) / Chaos (random targets) | Shifted assets / anything goes. |
| Compression | All formats, random / N64 gentle tilt / GBA / DS (LZ10/LZ11) | Corrupt inside compressed blocks. |
| Datamosh | Classic datamosh / Bloom / Heavy mosh / Hard cut | Melt, bloom, both, or keyframes dropped. |
| Source Code | Config tweak / Logic flip / Text glitch / Magic numbers / Total chaos | Numbers only, booleans + operators, strings, magic integers, everything. |

Built-in presets are part of LRC and can't be changed. Load one, tweak it, and **Save As...** to make your own version.
