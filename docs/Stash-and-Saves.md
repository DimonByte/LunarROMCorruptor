# Stashes, Saves & Bisect

[← Back to the README](../README.md) · [Getting Started](Getting-Started.md) · [Corruption Engines](Corruption-Engines.md) · [Engine Presets](Engine-Presets.md) · [ByteView](ByteView.md) · [Automation & Multi-File](Automation-and-Multi-File.md)

LRC has two ways to keep a corruption:

| | **Stash** | **File Save** |
| :--- | :--- | :--- |
| Stores | Only the changed bytes (plus which file it was made for) | A full copy of the corrupted file |
| Size | Tiny | As big as the file |
| Editable | Yes, in the Stash Editor (change, remove, mutate) | No |
| Folder | `CorruptionStashList` | `Saves` |

---

## Stashes

With **Enable Stash Saves** on, every corruption's changes are listed in the box next to the stash list.

| Control | What it does |
| :--- | :--- |
| **▶** (between the two lists) | Saves the current corruption as a `.stash` file in the stash list. |
| **Stash Editor** | Opens the selected stash (or an empty editor). See [Stash Editor](#stash-editor). |
| **Corrupt File and Stash** | **Quick load.** Reads the stash headers, loads the file the stash was made for, sets the save location it used, then applies the stash. You get exactly the same corruption without loading anything yourself. |
| **Double-click a stash** | Same as *Corrupt File and Stash*. |
| **Corrupt using Selected Stash** | Applies the stash to whatever file is loaded **right now**, saved to the current save location. Use it to try a stash on another version of a game, or on a file that has moved. |
| **Rename / Delete / Refresh** | Manage the stash files (also on the right-click menu). |
| **Corruption History ▾** | The last 3 corruptions (this session): re-apply, save to the stash list, or open in the Stash Editor. Handy when you forgot to save a good one. |

### Stash file format

A stash is a text file with one entry per line:

| Line | Meaning |
| :--- | :--- |
| `[x] File.OpenLocation("C:\Games\game.z64")` | **Header:** the file that was loaded. Used by *Corrupt File and Stash*. |
| `[x] File.SaveAsLocation("C:\Games\CorruptedFile.z64")` | **Header:** where the corrupted file was saved. |
| `[x] File.EXCLUDE(start,end)` | Bytes start to end (inclusive) are excluded. They're never changed, and applying the stash adds them to the session's excluded ranges. |
| `[x] File(offset).SET(value)` | Set the byte at *offset* to *value* (decimal). Most engines write this. |
| `[x] File(offset).VARLINE("name",line).SET("text")` | Source Code Engine: replace the value at that position with *text*. |
| `[x] File(offset).FRAME(frame).SET(DROP\|REPLACE\|REPEAT,n)` | Datamosh Engine: drop, replace or repeat a video frame. |

The headers are written at the top of every new stash. Stashes made before headers existed still work with *Corrupt using Selected Stash*. *Corrupt File and Stash* tells you when a stash has no headers, or when its file has moved. If the save location's folder no longer exists, the default save location next to the file is used.

---

<a id="stash-editor"></a>
## Stash Editor

| Tool | What it does |
| :--- | :--- |
| **Open File** | Opens a stash file. |
| **Create new stash item** (*Location*, *Value*, **Add**) | Adds a `SET` entry by hand. |
| **Remove / Remove Selected / Remove 50%** | Moves entries to *Removed items* (Remove 50% moves the first half, handy for finding which half of a corruption does something). **Restore** puts them back, **Permanently Remove ALL** clears the removed list. |
| **Mutate stash** | Makes a **variation** of the corruption: changes *Mutate %* of the byte entries. *Random value* gives them a new random value, *Nudge value* adds/subtracts up to *Amount*, *Move address* moves them up to *Amount* bytes. **Mutate list** changes the list, **Mutate, save new & corrupt** saves it as a new stash and applies it right away. |
| **Append stash file...** | Adds the entries of another stash (combine two corruptions). |
| **Sort by address** | Sorts the byte entries (headers and other lines stay at the top). The result doesn't change. |
| **Remove duplicates** | Removes entries that a later entry for the same address overwrites. The result doesn't change. |
| **Save as new stash / Save stash and Corrupt** | Saves your changes (and applies them). |

---

<a id="bisect-mode"></a>
## Bisect Mode

Found a corruption that is *almost* perfect, but crashes? **Bisect Mode** (checkbox above the stash list) finds the changes that cause the crash.

1. Corrupt as usual until you get a crash.
2. Tick **Bisect Mode** and press **Start bisect from the current stash**.
3. LRC applies the stash **without** half of the suspect changes and runs the emulator. Answer **Still crashes** or **Works now**. Each answer halves the suspects (**Undo last answer** and **Re-run this test** if you're unsure).
4. After a few rounds (about 13 for 5,000 changes) only the culprits are left. **Exclude suspects** adds them as excluded ranges, or **Exclude + apply the rest** also re-applies everything else. *Exclude whole 4-byte words* widens them to the full instruction/pointer/float.

ByteView shows the half that was left out in cyan. Excluded ranges are shown in red, no engine will change them until you **Reset exclusions**, and they're saved in new stashes as `EXCLUDE` lines. They're cleared when a different file is loaded.

---

## File Saves Tab

| Control | What it does |
| :--- | :--- |
| **Enable Auto File Saves** | Copies every corrupted file into `Saves` automatically. |
| **Save** | Copies the current corrupted file into `Saves`. |
| **Copy Save to Corrupted File** | Puts the selected save back at the save location (e.g. to run it again). |
| **Rename / Delete / Reload list / Open Folder Location** | Manage the saves. |
