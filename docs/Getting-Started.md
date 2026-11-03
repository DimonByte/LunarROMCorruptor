# Getting Started

[← Back to the README](../README.md) · [Corruption Engines](Corruption-Engines.md) · [Engine Presets](Engine-Presets.md) · [Stashes, Saves & Bisect](Stash-and-Saves.md) · [ByteView](ByteView.md) · [Automation & Multi-File](Automation-and-Multi-File.md)

## Your first corruption

1. **Load a file.** Click **Open File** or drag the file onto the window. LRC doesn't load the file into memory, so any size works.
2. **Check the save location.** It's set to `CorruptedFile.<ext>` in the same folder, so your original is never touched. **Change Save Point** picks another file. If you pick the original file itself, LRC warns you and keeps an undo journal so **Restore File** still works while LRC is open.
3. **Pick an engine.** Use the *Engine* list at the top. Not sure which? Start with **Nightmare+** and the ★ *Safe (auto-detect platform)* preset on the Corruption Engine tab. Then try **Vector2** on 3D games or **Pointer** for "wrong asset" corruptions. See [Corruption Engines](Corruption-Engines.md).
4. **Set the amount.** On Target Parameters, choose *Random byte mode* and an **Intensity** of about 10-100 for a first try. Raise it until things get interesting.
5. **Corrupt.** Click **Corrupt File (size)**. The button shows the progress and doubles as **Cancel**. Cancelling puts the output back to the original.
6. **Test it.** Turn on *Run program after corruption* (see [below](#run-emulator-program)) so your emulator opens the result automatically.
7. **Keep the good ones.** Every corruption's stash is shown in the list under *Enable Stash Saves*. Click **▶** to save it to the stash list (with a screenshot of the emulator if it's running). Later, select it and click **Corrupt stash's file using selected stash** to load its file and get exactly the same corruption again. Double-click a stash to see its details and screenshots. See [Stashes, Saves & Bisect](Stash-and-Saves.md).

Every corruption starts from the **original** file. Corruptions don't stack, so pressing Corrupt again gives a new corruption of the clean file.

---

## Main window

| Control | What it does |
| :--- | :--- |
| **Open File** | Loads a file. In Multi-File mode this button is **Edit Queue...** instead. |
| **Change Save Point** | Where the corrupted file is written. |
| **Restore File** | Puts the save location back to the original. In Multi-File mode it restores every queue file. |
| **Multi-File** | Corrupts a list of files at once. See [Automation & Multi-File](Automation-and-Multi-File.md#multi-file-corruption-queue). |
| **Engine** | The corruption engine. Its settings are on the Corruption Engine tab. |
| **Mute** | No sound after a successful corruption. |
| <a id="fix-checksums"></a>**Fix Checksums** | After corrupting, recalculates the checksums that consoles/emulators check, so the file still boots: **N64** (boot CRC, CIC 6101/6102/6103/6105/6106), **Game Boy** (header + global checksum), **GBA** (header complement), **SNES** (checksum + complement), **Genesis** and **raw CD sectors** (Mode 1 / Mode 2 EDC + ECC, only for the sectors that changed). A checksum is only rewritten when LRC could reproduce the original one, so unknown or bad dumps are never touched. Checksum bytes aren't added to the stash (they're recalculated every time). |
| **Take Ownership** | If the file can't be read or written (protected/system file), LRC tries to take ownership and grant itself access. Use with care. |

### Target Parameters

| Setting | What it does |
| :--- | :--- |
| **Start byte / End byte** | The part of the file that can be corrupted (both included). Use the sliders, type a number, or drag on the [ByteView](ByteView.md) image. Leaving out the start of a file (the header) is the easiest way to get fewer crashes. |
| **HEX** | Shows/enters Start and End byte in hexadecimal. |
| **Random byte mode** + **Intensity** | Makes *Intensity* corruptions at random places in the range. |
| **Corrupt every nth byte mode** | Corrupts every *N*th byte from Start to End (1 = every byte, 1000 = one in a thousand). Gives the same spread every time. |
| **Allow Large Intensity** | Raises the limit of Intensity / N from 1,000 to 10,000,000. |
| **Exclude Range** | Excludes the bytes from Start to End: no engine changes them from the next corruption on. They're red on ByteView, and added to the current stash as an `EXCLUDE` line, so a stash you save keeps them. Bytes the last corruption already changed stay changed until you corrupt again. |
| **Delete Selected Range** | Un-excludes every excluded byte between Start and End, and removes it from the current stash. A range that is only partly selected is trimmed (or split), so you can cut a hole in a big excluded area. |
| **Byte details** (text under the range) | Start, End and length of the range, how many bytes will be corrupted, how many stash entries are listed, and the excluded ranges (from Bisect / `EXCLUDE` stash lines). |
| **Stash list area** | See [Stashes, Saves & Bisect](Stash-and-Saves.md). |

Engines that understand the data use Intensity / N in their own unit (floats, pointers, blocks, frames, source code values). The [engine page](Corruption-Engines.md) says which.

### Corruption Engine

The settings of the selected engine, plus the **Preset** bar at the top: pick a ★ built-in or user preset to load it, **Save As...** to keep the current settings, **Import...** to add a preset someone shared, **Open Folder** to see your preset files. See [Engine Presets](Engine-Presets.md).

### <a id="run-emulator-program"></a>Run Emulator Program

| Setting | What it does |
| :--- | :--- |
| **Run program after corruption** + **Select Program** | Starts the program (usually an emulator) after every successful corruption, with the corrupted file as the argument. You can also drag an `.exe` onto LRC to set it. |
| **Close if running and re-open** | Closes the program first if it's still running from the last corruption, so each test starts fresh. |
| **Override Arguments** | Uses your own command line instead of just the file path. |
| **Terminate Running Emulator** | Closes the program LRC started. |

### Guides

The guides you're reading are also on the **Guides** tab inside LRC. Each guide is downloaded from GitHub when you open it, so it's always the latest version. Without internet the copy that came with LRC is shown. Links between guides open inside LRC, **Open on GitHub** opens the page in your browser.

### Logs

Everything LRC does is written here, including what each engine found (e.g. *"Pointer Engine found 1,204 pointer(s)"*). If an engine changes nothing, the log usually says why. **Save Log** saves it to a file.

---

## Tips

* **Crashes immediately?** Lower the intensity, move the Start byte past the header, use an exclusion type in Nightmare+, keep *Fix Checksums* on, or use [Bisect Mode](Stash-and-Saves.md#bisect-mode) to find the bytes that crash it.
* **Nothing changes?** Check the log. The data might be compressed (use the [Compression Engine](Corruption-Engines.md#compression-engine)), or the engine didn't find its kind of data in the range.
* **Found a great one?** Save the stash, then use the Stash Editor's **Mutate** to make variations of it.
