# Automation & Multi-File

[← Back to the README](../README.md) · [Getting Started](Getting-Started.md) · [Corruption Engines](Corruption-Engines.md) · [Engine Presets](Engine-Presets.md) · [Stashes, Saves & Bisect](Stash-and-Saves.md) · [ByteView](ByteView.md)

## Automation

Automation runs a list of tasks over and over on a timer, so you can sit back and watch corruptions roll in (combine it with *Run program after corruption*).

| Control | What it does |
| :--- | :--- |
| **Task** list + **Add action** | Adds a task: `Randomize Intensity`, `Randomize Start Byte`, `Randomize End Byte`, `Randomize both Start byte and End byte`, `Randomize Corruption Engine` (byte engines only, Merge only if a merge file is set), `Run Corruption`. |
| **Move Task Up / Down**, **Remove Selected Task** | Order the list. Tasks run from top to bottom on every tick. |
| **Delay in seconds (tick speed)** | Time between ticks. Give the emulator enough time to start. |
| **Start Automation** | Starts/stops. Error message boxes are suppressed while it runs (they're still in the log). |

A typical list: `Randomize Intensity` → `Randomize both Start byte and End byte` → `Run Corruption`. Every corruption is kept in the [Corruption History](Stash-and-Saves.md#stashes) (last 3), so you can save a good one after the fact.

---

<a id="multi-file-corruption-queue"></a>
## Multi-File (corruption queue)

Tick **Multi-File** (or drop several files / a folder onto LRC) to corrupt a list of files with the same settings in one go. Good for games that are split into many files (PC games, extracted disc images).

| Control (Corruption Queue window) | What it does |
| :--- | :--- |
| **Add File(s) / Add Folder** | Adds files to the queue. |
| **Remove Selected File(s) / Clear List** | Removes files. |
| **Send files to corruptor** | Uses the queue for the next corruption. |
| **Run selected file in emulator after corruption** / **Clear selected run file** | Which file the emulator should start (the first one by default). |

* Files are corrupted **in place**, but each one is journaled first: every run starts from the originals (runs don't stack), cancelling restores them, and **Restore File** puts every queue file back while LRC stays open.
* When *End byte* is the end of the file, it means "the end of each file". Excluded ranges belong to the single loaded file and aren't used in the queue.
* Stashes, auto saves, stash files and Save As are single-file features and are switched off in Multi-File mode. Replaying a stash on its own file (**Corrupt stash's file using selected stash**) switches back to single-file mode.

---

## Running an emulator after corruption

See [Getting Started → Run Emulator Program](Getting-Started.md#run-emulator-program).
