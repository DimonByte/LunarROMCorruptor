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

//Advisories:
//This program can irreversibly corrupt personal or critical system data if you're not careful.
//This program comes with no warranty of ANY kind and is provided 'AS IS'.
//You're responsible for backing up your data before use and for any damage that comes with the use or misuse of this program.
//Anti-cheat software may be triggered if corruption is used on multiplayer games.
//You may get banned if this corruptor is used in online games or games with anti-cheat software.
//Don't use this on system32 or any other system files...

using LunarROMCorruptor.Modules;
using LunarROMCorruptor.Modules.CorruptionInternals;
using LunarROMCorruptor.Properties;
using System.Media;

namespace LunarROMCorruptor
{
    public partial class MainCorruptionForm : Form
    {
        private readonly Random rnd = new(); //Used for random number generation
        private readonly string vernumber = $"v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}"; //"v1.0.4 - Build Number: " + 
        public List<string> InternalStashItems = []; //Adding to this list will make corruptions faster as it's not in the GUI so it doesn't have to render every item update.
        readonly CorruptionQueueForm CorruptionQueueFormSettings = new(); //Creates the corruption queue form and then it will be read later by the main form.
        public readonly CorruptionEngineOptions CorruptionEngineFrame = new() //This is the form that will be used to set the options for the corruption engine. It will be embedded in the main form.
        {
            TopLevel = false
        };
        public int MainSelectedProcessID = 999999;
        public MainCorruptionForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //MainTabControl.TabPages.Remove(MemCorruptPage); // For now, remove process memory corruption tab
            Text = $"{nameof(LunarROMCorruptor)} - " + vernumber;
            AboutVerLabel.Text = vernumber;
            if (!Directory.Exists(Application.StartupPath + "\\Saves\\")) //If file doesn't exist, assume it's the first time the program has been run and create the directory.
            {
                try
                {
                    Directory.CreateDirectory(Application.StartupPath + "\\Saves\\");
                    Directory.CreateDirectory(Application.StartupPath + "\\CorruptionStashList\\");
                    MessageBox.Show($"Welcome to {nameof(LunarROMCorruptor)}!{Environment.NewLine}{Environment.NewLine}Disclaimer:{Environment.NewLine}{nameof(LunarROMCorruptor)} is distributed under an MIT license.{Environment.NewLine}{Environment.NewLine}By clicking OK, you agree to that license and also understand the risks and disclaimers provided.{Environment.NewLine}{Environment.NewLine}This program can irreversibly corrupt personal or critical system data if you're not careful.{Environment.NewLine}This program comes with no warranty of ANY kind and is provided 'AS IS'.{Environment.NewLine}You're responsible for backing up your data before use and for any damage that comes with the use or misuse of this program.{Environment.NewLine}{Environment.NewLine}This message will not show up again but you can read the license again by going to the 'About' tab.{Environment.NewLine}{Environment.NewLine}Enjoy!", $"{nameof(LunarROMCorruptor)} - INFO", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error trying to create directory folders. Your anti-virus or ransomware protection may be enabled and is blocking LRC from creating these directories. Error: {ex}", $"{nameof(LunarROMCorruptor)} - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            CorruptionEngineComboBox.Text = EngineEnums.GetEngineDisplayName(CorruptionEngineType.NightmareEngine);
            AllowDrop = true;
            BrowseEmulatorbutton.Enabled = false;
            EmulatorLocationtxt.BackColor = Color.Gray;
            IOManager.RefreshCorruptionStashList(StashFileList);
            IOManager.RefreshFileSaves(FilesaveList);
            LoadSettings();
            CorruptionEngineFrame.TopLevel = false;
            CorruptionEngineTab.Controls.Add(CorruptionEngineFrame); //Adds the CorruptionEngineFrame to the CorruptionEngineTab.
            CorruptionEngineFrame.Dock = DockStyle.Fill;
            CorruptionEngineFrame.Show();
            AttentionPictureBox.BringToFront();
            //This code under this comment fixes a .net bug where the trackbar allocates a huge amount of memory if the trackbar maximum value is set to a large amount.
            EndByteTrackbar.TickStyle = TickStyle.None;
            StartByteTrackBar.TickStyle = TickStyle.None;
            CorrupteverynthbyteTrackbar.TickStyle = TickStyle.None;
            IntensityTrackbar.TickStyle = TickStyle.None;
        }

        public void LoadSettings()
        {
            // Read & check if the emulator path is valid
            if (!string.IsNullOrEmpty(Settings.Default.EmulatorPath) && File.Exists(Settings.Default.EmulatorPath))
            {
                EmulatorLocationtxt.Text = Settings.Default.EmulatorPath;
            }
            // Read and set checkbox states
            SilentCorruptionchbox.Checked = Settings.Default.SilentCorruption;
            EnableStashSavesChkbox.Checked = Settings.Default.StashSavesEnabled;
            UseHexchbox.Checked = Settings.Default.UseHex;
            Runemulatorchbox.Checked = Settings.Default.RunEmulator;
            ReopenChbox.Checked = Settings.Default.ReopenEmulator;
            FilesaveEnableAutoSaves.Checked = Settings.Default.AutoFileSaveEnabled;
            byteViewColourChkbox.Checked = Settings.Default.ByteViewColourMode;
            ByteViewupdateWhenCorruptedChkBox.Checked = Settings.Default.ByteViewUpdateOnCorruption;
            AttemptProtectedFileOverrideChkBox.Checked = Settings.Default.AttemptProtectedFileOverride;
        }

        public void LoadFile(string FileLocation)
        {
            AttentionPictureBox.Image = Resources.Loading;
            AttentionPictureBox.Show();
            Refresh();
            if (CorruptionCore.LoadROM(FileLocation))
            {
                StartByteTrackBar.Value = 0;
                StartByteTrackBar.Maximum = CorruptionCore.MaxByte;
                EndByteTrackbar.Maximum = CorruptionCore.MaxByte;
                EndByteTrackbar.Value = CorruptionCore.MaxByte;
                EndByteNumb.Maximum = CorruptionCore.MaxByte;
                EndByteNumb.Value = CorruptionCore.MaxByte;
                StartByteNumb.Maximum = CorruptionCore.MaxByte;
                StartByteNumb.Value = 0;
                FileSelectiontxt.Text = FileLocation;
                SaveasTxt.Text = FileLocation;
                MainSaveFileDialog.FileName = Path.GetDirectoryName(SaveasTxt.Text);
                string exc = Path.GetExtension(FileLocation);
                SaveasTxt.Text = SaveasTxt.Text.Replace(Path.GetFileName(FileLocation), "CorruptedFile" + exc);
                CorruptButton.Text = $"Corrupt File ({CorruptionCore.rawFileSize} {CorruptionCore.fileSizeUnit})";
                AttentionPictureBox.Hide();
                AttentionPictureBox.Image = Resources.dragicon;
            }
            else
            {
                MessageBox.Show("Failed to load the file. Please ensure it's a valid file and try again.", $"{nameof(LunarROMCorruptor)} - ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Openfilebtn_Click(object sender, EventArgs e)
        {
            if (CorruptionQueueChkbox.Checked)
            {
                CorruptionQueueFormSettings.ShowDialog();
                return;
            }
            //Main Function - If the user didn't cancel, load file.
            if (MainOpenFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                LoadFile(MainOpenFileDialog.FileName);
            }
        }

        private void StartByteTrackBar_Scroll(object sender, EventArgs e)
        {
            StartByteNumb.Value = StartByteTrackBar.Value;
        }

        private void EndByteTrackbar_Scroll(object sender, EventArgs e)
        {
            EndByteNumb.Value = EndByteTrackbar.Value;
        }

        private void CorrupteverynthbyteTrackbar_Scroll(object sender, EventArgs e)
        {
            EveryNthByte.Value = CorrupteverynthbyteTrackbar.Value;
        }

        private void IntensityTrackbar_Scroll(object sender, EventArgs e)
        {
            Intensity.Value = IntensityTrackbar.Value;
        }

        private void Intensity_ValueChanged(object sender, EventArgs e)
        {
            IntensityTrackbar.Value = (int)Intensity.Value;
        }

        private void EveryNthByte_ValueChanged(object sender, EventArgs e)
        {
            CorrupteverynthbyteTrackbar.Value = (int)EveryNthByte.Value;
        }

        private void AllowLargeIntensity_CheckedChanged(object sender, EventArgs e)
        {
            int maxIntensity = AllowLargeIntensity.Checked ? 99999 : 1000; //This changes the value based on the checked state of the checkbox
            Intensity.Maximum = maxIntensity;
            IntensityTrackbar.Maximum = maxIntensity;
            CorrupteverynthbyteTrackbar.Maximum = maxIntensity;
            EveryNthByte.Maximum = maxIntensity;
        }

        private void UseHexchbox_CheckedChanged(object sender, EventArgs e)
        {
            StartByteNumb.Hexadecimal = UseHexchbox.Checked;
            EndByteNumb.Hexadecimal = UseHexchbox.Checked;
            //Save changes to settings
            Settings.Default.UseHex = UseHexchbox.Checked;
            Settings.Default.Save();
        }

        private void Changesaveasbtn_Click(object sender, EventArgs e)
        {
            string exc1 = Path.GetExtension(FileSelectiontxt.Text);
            MainSaveFileDialog.FileName = "CorruptedFile" + exc1;
            if (MainSaveFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                SaveasTxt.Text = MainSaveFileDialog.FileName;
            }
        }

        private void CorruptionEngineComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Main Form - Change what the CorruptionEngineFrame displays.
            var selectedEngine = EngineEnums.ParseEngineType(CorruptionEngineComboBox.Text);
            CorruptionEngineFrame.MergeEnginePanel.Visible = selectedEngine == CorruptionEngineType.MergeEngine;
            CorruptionEngineFrame.LogicEnginePanel.Visible = selectedEngine == CorruptionEngineType.LogicEngine;
            CorruptionEngineFrame.NightmareEnginePanel.Visible = selectedEngine == CorruptionEngineType.NightmareEngine;
            CorruptionEngineFrame.LerpEnginePanel.Visible = selectedEngine == CorruptionEngineType.LerpEngine;
            CorruptionEngineFrame.Vector2EnginePanel.Visible = selectedEngine == CorruptionEngineType.Vector2Engine;

            if (selectedEngine == CorruptionEngineType.ManualEngine) // Assuming "Manual" was meant to be Vector2Engine
            {
                CorruptionEngineFrame.Hide();
                ManualEnginePanel.Show();
            }
            else
            {
                ManualEnginePanel.Hide();
                CorruptionEngineFrame.Show();
            }
        }

        private void BrowseEmulatorbutton_Click(object sender, EventArgs e)
        {
            if (EmulatorFileDialog.ShowDialog() != DialogResult.Cancel)
            {
                EmulatorLocationtxt.Text = EmulatorFileDialog.FileName;
                //Save selected path to settings
                Settings.Default.EmulatorPath = EmulatorFileDialog.FileName;
                Settings.Default.Save();
            }
        }

        private void OverrideArgumentschbox_CheckedChanged(object sender, EventArgs e)
        {
            OverrideArguments.Enabled = OverrideArgumentschbox.Checked;
        }

        private void Runemulatorchbox_CheckedChanged(object sender, EventArgs e)
        {
            if (Runemulatorchbox.Checked)
            {
                EmulatorLocationtxt.BackColor = Color.White;
                BrowseEmulatorbutton.Enabled = true;
            }
            else
            {
                EmulatorLocationtxt.BackColor = Color.Gray;
                BrowseEmulatorbutton.Enabled = false;
            }
            //Save changes to settings
            Settings.Default.RunEmulator = Runemulatorchbox.Checked;
            Settings.Default.Save();
        }

        private void CorruptButton_Click(object sender, EventArgs e)
        {
            //Main Function - Starts the corruption process.

            //---Check if the corruption queue has been enabled--- If so, use this multi-corruption code.
            if (CorruptionQueueChkbox.Checked) //Multiple File Corruption
            {
                // Validate queue is not empty and handle multiple files selection
                if (CorruptionQueueFormSettings.CorruptionQueueList.Items.Count == 0)
                {
                    MessageBox.Show("The corruption queue is empty.\n\nPlease add files to the queue.", "Empty Queue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (FileSelectiontxt.Text != "---Multiple files selected---")
                {
                    MessageBox.Show("There are files waiting in the corruption queue, but they have not been sent to the corruptor.\n\nYou need to open the corruption queue and click 'Send files to the corruptor' before you can corrupt them.", "Corruption Queue Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (CorruptionQueueFormSettings.CorruptionQueueList.Items.Count >= 100)
                {
                    MessageBox.Show("The corruption queue is rather full of files.\n\nBe aware that this will impact corruption times.", "Full Queue", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Validate each file in the queue exists
                foreach (var item in CorruptionQueueFormSettings.CorruptionQueueList.Items.Cast<ListViewItem>().Select(i => i.Text))
                {
                    if (!File.Exists(item))
                    {
                        MessageBox.Show($"The file {item} does not exist.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Validate Start and End bytes
                if (StartByteNumb.Value > EndByteNumb.Value)
                {
                    MessageBox.Show("Start Byte cannot be greater than End Byte!", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                CorruptionCore.StartByte = (int)Math.Min(StartByteNumb.Value, CorruptionCore.MaxByte);
                CorruptionCore.StartByte = (int)EndByteNumb.Value;

                // Determine corruption intensity
                int tmpintensity = CorruptEverynthByteRadioBtn.Checked ? (int)EveryNthByte.Value : (int)Intensity.Value;

                // Clear and trim stash list
                StashBytesList.Items.Clear();
                InternalStashItems.Clear();
                InternalStashItems.TrimExcess();

                // Process each file in the corruption queue
                foreach (var filePath in CorruptionQueueFormSettings.CorruptionQueueList.Items.Cast<ListViewItem>().Select(i => i.Text))
                {
                    // Load the file into ROM byte array
                    //OPTIMIZE AND REFACTOR: This code is loading a ROM without using CorruptionCore.LoadROM, which means it doesn't have the error handling and file validation that LoadROM provides. It also means that if the file is large, it will allocate a large byte array in memory which could cause issues.
                    CorruptionCore.ROM = File.ReadAllBytes(filePath);
                    CorruptionCore.MaxByte = CorruptionCore.ROM.Length - 1;
                    StartByteTrackBar.Value = 0;
                    StartByteTrackBar.Maximum = CorruptionCore.MaxByte;
                    EndByteTrackbar.Maximum = CorruptionCore.MaxByte;
                    EndByteTrackbar.Value = CorruptionCore.MaxByte;
                    EndByteNumb.Maximum = CorruptionCore.MaxByte;
                    EndByteNumb.Value = CorruptionCore.MaxByte;
                    StartByteNumb.Maximum = CorruptionCore.MaxByte;
                    StartByteNumb.Value = 0;
                    FileSelectiontxt.Text = filePath;
                    SaveasTxt.Text = filePath;

                    // Attempt File Owner Override if enabled
                    if (AttemptProtectedFileOverrideChkBox.Checked && File.Exists(SaveasTxt.Text))
                    {
                        CorruptionCore.AttemptProtectedFileOverride(SaveasTxt.Text);
                    }

                    // Start Corruption in CorruptionCore

                    CorruptionCore.ROM = CorruptionCore.StartCorruption(CorruptionCore.ROM, CorruptionCore.StartByte, CorruptionCore.EndByte, CorruptEverynthByteRadioBtn.Checked, tmpintensity, EngineEnums.GetEngineDisplayName(EngineEnums.ParseEngineType(CorruptionEngineComboBox.Text)));

                    // Attempt to save corrupted file

                    if (CorruptionCore.ROM != null)
                    {
                        try
                        {
                            File.WriteAllBytes(filePath, CorruptionCore.ROM);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"The file {filePath} could not be saved.\n\n{ex}", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"The file {filePath} could not be corrupted. ROM returned null.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                // Reset UI for multiple files
                FileSelectiontxt.Text = "---Multiple files selected---";
                SaveasTxt.Text = "---Corruption will be applied on the files selected---";

                // Run emulator if enabled and exists
                if (Runemulatorchbox.Checked)
                {
                    if (File.Exists(EmulatorLocationtxt.Text))
                    {
                        CorruptionCore.StartEmulator(ReopenChbox.Checked, EmulatorLocationtxt.Text, CorruptionQueueFormSettings.SelectedRunFileTXT.Text, OverrideArgumentschbox.Checked, OverrideArguments.Text);
                    }
                    else
                    {
                        MessageBox.Show("Emulator doesn't exist.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else //Use normal, one file corruption code.
            {
                // Simplified and optimized code

                // Validate Start and End bytes
                if (StartByteNumb.Value > EndByteNumb.Value)
                {
                    MessageBox.Show("Start Byte cannot be greater than End Byte!", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                CorruptionCore.StartByte = (int)Math.Min(StartByteNumb.Value, CorruptionCore.MaxByte);
                CorruptionCore.EndByte = (int)EndByteNumb.Value;

                // Determine corruption intensity
                int tmpintensity = CorruptEverynthByteRadioBtn.Checked ? (int)EveryNthByte.Value : (int)Intensity.Value;

                // Validate file selection
                if (string.IsNullOrEmpty(FileSelectiontxt.Text) || FileSelectiontxt.Text == "No file selected." || !File.Exists(FileSelectiontxt.Text))
                {
                    MessageBox.Show("Please select a valid file to corrupt.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Clear and trim stash list
                StashBytesList.Items.Clear();
                InternalStashItems.Clear();
                InternalStashItems.TrimExcess();

                // Attempt File Owner Override if enabled
                if (AttemptProtectedFileOverrideChkBox.Checked && File.Exists(SaveasTxt.Text))
                {
                    CorruptionCore.AttemptProtectedFileOverride(SaveasTxt.Text);
                }

                // Check if ROM is null and initialize it if needed
                if (CorruptionCore.ROM == null)
                {
                    MessageBox.Show($"ROM File isn't loaded into LRC, please load a file. (ROM is null when attempting to corrupt.)", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Clone ROM and start corruption
                byte[]? FinROM = (byte[])CorruptionCore.ROM.Clone(); //Gets duplicated so original ROM can be restored. FINROM is the one that gets corrupted and saved.
                FinROM = CorruptionCore.StartCorruption(FinROM, CorruptionCore.StartByte, CorruptionCore.EndByte, CorruptEverynthByteRadioBtn.Checked, tmpintensity, EngineEnums.GetEngineDisplayName(EngineEnums.ParseEngineType(CorruptionEngineComboBox.Text)));

                // Validate corruption result
                if (FinROM == null) //Something went wrong here...
                {
                    MessageBox.Show("The corrupted ROM returned from the corruption engine with NULL. Something went wrong. No data has been saved and corruption has been aborted.", $"Fatal Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Attempt to save corrupted ROM
                try
                {
                    File.WriteAllBytes(SaveasTxt.Text, FinROM);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to save file.\n{ex}", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (FilesaveEnableAutoSaves.Checked) //Save the corrupted file copy into the file saves if auto saves is enabled.
                { 
                    IOManager.SaveCorruptedFileCopy(SaveasTxt.Text, FilesaveList);
                }

                // Run emulator if enabled and exists
                if (Runemulatorchbox.Checked)
                {
                    if (File.Exists(EmulatorLocationtxt.Text))
                    {
                        CorruptionCore.StartEmulator(ReopenChbox.Checked, EmulatorLocationtxt.Text, SaveasTxt.Text, OverrideArgumentschbox.Checked, OverrideArguments.Text);
                    }
                    else
                    {
                        MessageBox.Show("Emulator doesn't exist.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                if (EnableStashSavesChkbox.Checked)
                {

                    if (InternalStashItems.Count > 50000)
                    {
                        StashBytesList.Items.Add("LargeStash");
                    }
                    else
                    {
                        // take a snapshot so the background thread works on a stable collection
                        var snapshot = InternalStashItems.ToArray();

                        // run conversion/packing off the UI thread
                        Task.Run(() =>
                        {
                            var objects = Array.ConvertAll(snapshot, item => (object)item);

                            // marshal the minimal UI work back to the UI thread
                            StashBytesList.BeginInvoke(new Action(() =>
                            {
                                StashBytesList.BeginUpdate();
                                StashBytesList.Items.AddRange(objects);
                                StashBytesList.EndUpdate();
                            }));
                        });
                    }
                }

                // Update ByteView if enabled
                if (ByteViewupdateWhenCorruptedChkBox.Checked)
                {
                    ByteViewPictureBox.Image = ByteView.ConvertByteToImage(FinROM, byteViewColourChkbox.Checked);
                    if (flipVerticalChkbox.Checked || flipHorizontalChkbox.Checked)
                    {
                        //FIXME: Converting null literal to non-nullable type. This should be fixed by ensuring that ByteView.ConvertByteToImage never returns null.
                        ByteViewPictureBox.Image = ByteView.FlipImage((Bitmap)ByteViewPictureBox.Image, flipHorizontalChkbox.Checked, flipVerticalChkbox.Checked);
                    }
                }
                // Clear FINROM and clean memory
                FinROM = null;
                GC.Collect();
            }

            CorruptButton.BackColor = Color.Green; //Change colour of the corrupt button
            if (!SilentCorruptionchbox.Checked) //check if silent corruption is on. if it is, don't play the sound.
            {
                using var soundPlayer = new SoundPlayer(Resources.success3);
                soundPlayer.Play();
            }

            CorruptButtonColorChanger.Start(); //Change the corruption button colour back to normal after the timer ticks.
            Console.WriteLine("Corruption Complete: Number of Stash Items: " + InternalStashItems.Count); //Debugging
        }

        private void CorruptButtonColorChanger_Tick(object sender, EventArgs e)
        {
            CorruptButton.BackColor = Color.FromArgb(255, 32, 32, 32);
            CorruptButtonColorChanger.Stop(); //On tick, change the corrupt button colour back to normal and stop the timer.
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            AttentionPictureBox.Hide();
            var data = e.Data?.GetData(DataFormats.FileDrop);

            if (data == null)
                return;

            string[] files = (string[])data;

            if (files.Length > 1)
            {
                CorruptionQueueChkbox.Checked = true;
                // Add files to the listbox in the CorruptionQueue
                foreach (var item in files)
                {
                    CorruptionQueueFormSettings.CorruptionQueueList.Items.Add(item);
                }
                CorruptionQueueFormSettings.TopMost = true;
                CorruptionQueueFormSettings.ShowDialog();
                CorruptionQueueFormSettings.TopMost = false;
            }
            else
            {
                foreach (var path in files)
                {
                    LoadFile(path);
                }
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            var data = e.Data?.GetDataPresent(DataFormats.FileDrop);
            if (data == null)
                return;
            AttentionPictureBox.Show();
            e.Effect = DragDropEffects.Copy;
        }

        private void Restorefilebtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (CorruptionCore.ROM == null)
                {
                    MessageBox.Show("Error: Cannot restore file. No ROM loaded.", "Restore File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (SaveasTxt.Text == "No save location set.")
                {
                    MessageBox.Show("No save location set! Cannot restore.", "Restore File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                File.WriteAllBytes(SaveasTxt.Text, CorruptionCore.ROM);// Try to write the ROM that is in memory into the save as file.
                CorruptButton.BackColor = Color.Green;
                if (!SilentCorruptionchbox.Checked)
                {
                    using var soundPlayer = new SoundPlayer(Resources.success3);
                    soundPlayer.Play();
                }
                CorruptButtonColorChanger.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when restoring file: " + ex.ToString());
            }
        }

        private void TransferStash_Click(object sender, EventArgs e) //This saves the stash
        {
            IOManager.TransferStash(StashBytesList, StashFileList, InternalStashItems, FileSelectiontxt.Text, rnd);
        }

        private void StashEditorbtn_Click(object sender, EventArgs e)
        {
            StashEditor frm1 = new();
            if (StashFileList.SelectedItems.Count > 0)
            {
                try
                {
                    frm1.AttemptStashLoad(Application.StartupPath + "\\CorruptionStashList\\" + StashFileList.SelectedItems[0]!.ToString());
                }
                catch { }
            }
            frm1.ShowDialog();
        }

        private void RenameStash_Click(object sender, EventArgs e)
        {
            IOManager.RenameStash(StashFileList);
        }

        private void RefreshStash_Click(object sender, EventArgs e)
        {
            IOManager.RefreshCorruptionStashList(StashFileList);
        }

        private void DeleteStash_Click(object sender, EventArgs e)
        {
            IOManager.DeleteStash(StashFileList);
        }

        private void CopySaveToCorruptedFileBtn_Click(object sender, EventArgs e)
        {
            IOManager.CopySaveToCorruptedFile(SaveasTxt.Text, FilesaveList);
        }

        private void FilesaveReloadbtn_Click(object sender, EventArgs e)
        {
            IOManager.RefreshFileSaves(FilesaveList);
        }

        private void FilesaveSavebtn_Click(object sender, EventArgs e)
        {
            IOManager.SaveCorruptedFileCopy(SaveasTxt.Text, FilesaveList);
        }

        private void FilesaveRenameBtn_Click(object sender, EventArgs e)
        {
            IOManager.RenameFileSave(FilesaveList);
        }

        private void FilesaveDelete_Click(object sender, EventArgs e)
        {
            IOManager.DeleteFileSave(FilesaveList);
        }

        private void Corruptusingstash_Click(object sender, EventArgs e)
        {
            //Check if a StashFile is selected before running code
            if (StashFileList.SelectedItems.Count == 0)
            {
                MessageBox.Show("No StashFile selected!", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            CorruptUsingStashFile(Application.StartupPath + "\\CorruptionStashList\\" + StashFileList.GetItemText(StashFileList.SelectedItem));
        }

        public void CorruptUsingStashFile(string StashFileLocation)
        {
            string[] separator = [".", "("];
            string[] separatorArray = ["(", ")"];

            //Check if ROM isn't null before running code
            if (CorruptionCore.ROM == null)
            {
                MessageBox.Show("No ROM loaded!" + Environment.NewLine + "Please load a ROM first.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (CorruptionCore.ROM.Clone() is not byte[] FinROM)
            {
                MessageBox.Show("Unexpected error: ROM clone failed and returned null when corrupting using stash file.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            foreach (var line in File.ReadAllLines(StashFileLocation))
            {
                Object[] splitStrings;
                Object[] Instructions;
                try
                {
                    splitStrings = line.Split(separatorArray, StringSplitOptions.None);
                    Instructions = line.Split(separator, StringSplitOptions.None);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Split error in line: {line} {Environment.NewLine} Error: {ex.Message}", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Object i; //Location of byte to be changed
                Object result; //The value that the byte needs to be set to
                try
                {
                    i = splitStrings[1];
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Location error in line: {line} {Environment.NewLine} Error: {ex.Message}", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                try
                {
                    result = splitStrings[3];
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Corruption error in line: {line} {Environment.NewLine} Error: {ex.Message}", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //Check if the byte location is within the correct margins. If not, throw an error.
                if (Convert.ToInt32(i) > FinROM.Length)
                {
                    MessageBox.Show($"Byte location is out of bounds! {Environment.NewLine}Byte location that is invalid: {i}", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //Check if byte value is out of bounds
                bool isByteValid = byte.TryParse(result.ToString(), out byte output);
                if (isByteValid == false)
                {
                    MessageBox.Show($"Byte value is out of bounds! {Environment.NewLine}Invalid byte value: {result}", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                try
                {
                    // Better approach - check for null before parsing
                    string? iString = i?.ToString();
                    string? resultString = result?.ToString();

                    if (string.IsNullOrEmpty(iString) || string.IsNullOrEmpty(resultString))
                    {
                        MessageBox.Show("Invalid stash item data - null values found!",
                            $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    FinROM[int.Parse(iString)] = (byte)int.Parse(resultString);
                }
                catch (IndexOutOfRangeException ex1)
                {
                    MessageBox.Show($"Stash item location index is invalid! {Environment.NewLine} Error: {ex1}",
                        $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ROM byte stash corruption failed! Corruption cannot continue. {Environment.NewLine} Error: {ex}",
                        $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            File.WriteAllBytes(SaveasTxt.Text, FinROM);
            CorruptButton.BackColor = Color.Green;
            if (!SilentCorruptionchbox.Checked)
            {
                using var soundPlayer = new SoundPlayer(Resources.success3);
                soundPlayer.Play();
            }
            CorruptButtonColorChanger.Start();
            if (Runemulatorchbox.Checked && string.IsNullOrEmpty(EmulatorLocationtxt.Text))
            {
                CorruptionCore.StartEmulator(ReopenChbox.Checked, EmulatorLocationtxt.Text, SaveasTxt.Text, OverrideArgumentschbox.Checked, OverrideArguments.Text);
            }
            //Set StashByteList to the files contents, each line is each item on the list
            StashBytesList.Items.Clear();
            foreach (var item in File.ReadAllLines(StashFileLocation))
            {
                StashBytesList.Items.Add(item);
            }
        }

        private void EnableStashSavesChkbox_CheckedChanged(object sender, EventArgs e)
        {
            StashBytesList.Enabled = EnableStashSavesChkbox.Checked;
            TransferStash.Enabled = EnableStashSavesChkbox.Checked;
            //Save changes to settings
            Settings.Default.StashSavesEnabled = EnableStashSavesChkbox.Checked;
            Settings.Default.Save();
        }

        private void StartByteNumb_ValueChanged(object sender, EventArgs e)
        {
            StartByteTrackBar.Value = (int)StartByteNumb.Value;
        }

        private void EndByteNumb_ValueChanged(object sender, EventArgs e)
        {
            EndByteTrackbar.Value = (int)EndByteNumb.Value;
        }

        private void Form1_DragLeave(object sender, EventArgs e)
        {
            AttentionPictureBox.Hide();
        }

        private void StashList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                StashFileList.SelectedIndex = StashFileList.IndexFromPoint(e.X, e.Y);
                contextStripStash.Show(Cursor.Position);
            }
        }

        private void SilentCorruptionchbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.SilentCorruption = SilentCorruptionchbox.Checked;
            Settings.Default.Save();
        }

        private void ReopenChbox_CheckedChanged(object sender, EventArgs e)
        {
            //Save changes to settings
            Settings.Default.ReopenEmulator = ReopenChbox.Checked;
            Settings.Default.Save();
        }

        private void FilesaveEnableAutoSaves_CheckedChanged(object sender, EventArgs e)
        {
            //Save changes to settings
            Settings.Default.AutoFileSaveEnabled = FilesaveEnableAutoSaves.Checked;
            Settings.Default.Save();
        }

        private void CorruptionQueueChkbox_CheckedChanged(object sender, EventArgs e)
        {
            //Disable Stash Saves if queue is enabled as well as file saves
            if (CorruptionQueueChkbox.Checked)
            {
                Openfilebtn.Image = Resources.edit_pencil;
                Openfilebtn.Text = "Edit Queue...";
                EnableStashSavesChkbox.Checked = false;
                FilesaveEnableAutoSaves.Checked = false;
            }
            else
            {
                Openfilebtn.Text = "Open File";
                Openfilebtn.Image = Resources.upload;
            }
            //Disable the ability to change the checkboxes
            EnableStashSavesChkbox.Enabled = !CorruptionQueueChkbox.Checked;
            FilesaveEnableAutoSaves.Enabled = !CorruptionQueueChkbox.Checked;
            //Disable restore button
            Restorefilebtn.Enabled = !CorruptionQueueChkbox.Checked;
            //Disable corrupt using stash file
            Corruptusingstashbtn.Enabled = !CorruptionQueueChkbox.Checked;
            //Disable the save as button
            Changesaveasbtn.Enabled = !CorruptionQueueChkbox.Checked;
            //Disable stash editor button
            StashEditorbtn.Enabled = !CorruptionQueueChkbox.Checked;
        }

        private void FileSaveOpenLocationBtn_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Application.StartupPath + @"\Saves\");
        }

        private void StartAutomationBtn_Click(object sender, EventArgs e)
        {
            //If the buttons text is "Start Automation", then start automation and change the text and color to stop and OrangeRed
            if (StartAutomationBtn.Text == "Start Automation")
            {
                StartAutomationBtn.Text = "Stop Automation";
                StartAutomationBtn.ForeColor = Color.OrangeRed;
                AutomationTimer.Start();
            }
            else
            {
                StartAutomationBtn.Text = "Start Automation";
                StartAutomationBtn.ForeColor = Color.SpringGreen;
                AutomationTimer.Stop();
            }
        }

        private void RemoveTaskBtn_Click(object sender, EventArgs e)
        {
            //Removes selected item from the automation list
            if (AutomationList.SelectedItem != null)
                AutomationList.Items.Remove(AutomationList.SelectedItem);
        }

        private void AddTaskBtn_Click(object sender, EventArgs e)
        {
            //Adds automation task from the combo box into the automation list
            if (AutomationTaskComboBox.SelectedItem != null)
                AutomationList.Items.Add(AutomationTaskComboBox.SelectedItem);
        }

        private void DelayInAutomationNUD_ValueChanged(object sender, EventArgs e)
        {
            //Converts the NumericUpDown number from seconds to miliseconds to use on the timer
            AutomationTimer.Interval = (int)DelayInAutomationNUD.Value * 1000;
        }

        private void AutomationTimer_Tick(object sender, EventArgs e)
        {
            //For each task on the automation list, execute.
            foreach (var item in AutomationList.Items)
            {
                //Switch statement: Corrupt, Randomize Intensity, Randomize Start Byte, Randomize End Byte, Randomize both Start byte and End byte
                switch (item)
                {
                    case "Run Corruption":
                        //Click Corrupt Button
                        CorruptButton.PerformClick();
                        break;
                    case "Randomize Intensity":
                        //Check if nth Intensity is enabled
                        if (CorruptEverynthByteRadioBtn.Checked)
                        {
                            if (AllowLargeIntensity.Checked)
                            {
                                CorrupteverynthbyteTrackbar.Value = rnd.Next(1, 99999);
                                EveryNthByte.Value = rnd.Next(1, 99999);
                            }
                            else
                            {
                                CorrupteverynthbyteTrackbar.Value = rnd.Next(1, 1000);
                                EveryNthByte.Value = rnd.Next(1, 1000);
                            }
                        }
                        else
                        {
                            //If allow large intensity is checked, make the max random larger
                            if (AllowLargeIntensity.Checked)
                            {
                                IntensityTrackbar.Value = rnd.Next(1, 99999);
                                Intensity.Value = rnd.Next(1, 99999);
                            }
                            else
                            {
                                IntensityTrackbar.Value = rnd.Next(1, 1000);
                                Intensity.Value = rnd.Next(1, 1000);
                            }
                        }

                        break;
                    case "Randomize Start Byte":
                        //Randomizes Start byte by the maximum value of the trackbar
                        StartByteTrackBar.Value = rnd.Next(1, StartByteTrackBar.Maximum);
                        StartByteNumb.Value = rnd.Next(1, StartByteTrackBar.Maximum);
                        break;
                    case "Randomize End Byte":
                        EndByteTrackbar.Value = rnd.Next(1, EndByteTrackbar.Maximum);
                        EndByteNumb.Value = rnd.Next(1, EndByteTrackbar.Maximum);
                        break;
                    case "Randomize both Start byte and End byte":
                        //Randomizes both start byte and end byte, but making sure that the start byte is not higher than the end byte

                        StartByteTrackBar.Value = rnd.Next(1, StartByteTrackBar.Maximum);
                        EndByteTrackbar.Value = rnd.Next(StartByteTrackBar.Value, EndByteTrackbar.Maximum);

                        StartByteNumb.Value = rnd.Next(1, StartByteTrackBar.Maximum);
                        EndByteNumb.Value = rnd.Next(StartByteTrackBar.Value, EndByteTrackbar.Maximum);

                        break;
                    case "Randomize Corruption Engine":
                        //Select a random item in the CorruptionEngineComboBox
                        //Is Combobox NULL??
                        if (CorruptionEngineComboBox?.Items != null && CorruptionEngineComboBox.Items.Count > 0)
                        {
                            var selectedItem = CorruptionEngineComboBox.Items[rnd.Next(0, CorruptionEngineComboBox.Items.Count)];
                            CorruptionEngineComboBox.Text = selectedItem?.ToString() ?? EngineEnums.GetEngineDisplayName(CorruptionEngineType.NightmareEngine);
                        }
                        else
                        {
                            MessageBox.Show("Corruption Engine ComboBox is empty or null! Cannot randomize corruption engine.", $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            AutomationTimer.Stop();
                            return;
                        }
                        if (CorruptionEngineComboBox.Text == EngineEnums.GetEngineDisplayName(CorruptionEngineType.MergeEngine) && Program.Form.CorruptionEngineFrame.MergeFileLocationTxt.Text == null) // Prevents corruption from halting if there is no file in the Merge Engine while in Automation mode.
                        {
                            CorruptionEngineComboBox.Text = EngineEnums.GetEngineDisplayName(CorruptionEngineType.NightmareEngine);
                        }
                        break;
                }
            }
        }
        private void MoveTaskUpBtn_Click(object sender, EventArgs e)
        {
            try
            {
                //Moves selected item on automation list up
                if (AutomationList.SelectedIndex > 0)
                {
                    int index = AutomationList.SelectedIndex;
                    object? item = AutomationList.SelectedItem;
                    //is item null??
                    if (item == null)
                        return;
                    AutomationList.Items.Remove(item);
                    AutomationList.Items.Insert(index - 1, item);
                    AutomationList.SelectedIndex = index - 1;
                }
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("LRC Move Task Up - Insert cannot be null");
            }
        }

        private void MoveTaskDownBtn_Click(object sender, EventArgs e)
        {
            try
            {
                //Moves selected item on automation list down
                if (AutomationList.SelectedIndex < AutomationList.Items.Count - 1)
                {
                    int index = AutomationList.SelectedIndex;
                    object? item = AutomationList.SelectedItem;
                    //is item null??
                    if (item == null)
                        return;
                    AutomationList.Items.Remove(item);
                    AutomationList.Items.Insert(index + 1, item);
                    AutomationList.SelectedIndex = index + 1;
                }
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("LRC Move Task Down - Insert cannot be null");
            }
        }

        private void RandomByteCorruptRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            EverynthbyteGroupbox.Visible = false;
        }

        private void CorruptEverynthByteRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            EverynthbyteGroupbox.Visible = true;
        }

        private void EnableProcessMemCorruptChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EnableProcessMemCorruptChkBox.Checked)
            {
                //Unload ROMS
                UnloadROMFromMemory();
                StartEmulatorPanel.Visible = false;
                CheckBoxUpperPanel.Visible = false;
                CorruptButton.Text = "Start";
                Openfilebtn.Text = "Load Process";
                Changesaveasbtn.Visible = false;
                Restorefilebtn.Visible = false;
            }
            else
            {
                StartEmulatorPanel.Visible = true;
                CheckBoxUpperPanel.Visible = true;
                CorruptButton.Text = "Corrupt File";
                Openfilebtn.Text = "Open File";
                Changesaveasbtn.Visible = true;
                Restorefilebtn.Visible = true;
            }
        }


        private void UnloadROMFromMemory()
        {
            CorruptionCore.ROM = null;
            GC.Collect();
            CorruptionCore.MaxByte = 1000; //Set back to default values
            StartByteTrackBar.Value = 0;
            StartByteTrackBar.Maximum = CorruptionCore.MaxByte;
            EndByteTrackbar.Maximum = CorruptionCore.MaxByte;
            EndByteTrackbar.Value = 0;
            EndByteNumb.Maximum = CorruptionCore.MaxByte;
            EndByteNumb.Value = 0;
            StartByteNumb.Maximum = CorruptionCore.MaxByte;
            StartByteNumb.Value = 0;
            FileSelectiontxt.Text = "No file selected.";
            SaveasTxt.Text = "No save location set.";
            MainSaveFileDialog.FileName = Path.GetDirectoryName(SaveasTxt.Text);
            CorruptButton.Text = "Corrupt File";
        }

        private void StashAndAutoSaveHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show($"Auto Saves and Stash Files are two different ways of saving your corruptions.{Environment.NewLine}{Environment.NewLine}Auto Saves save the entire file after corruption in the Auto Saves folder in the {nameof(LunarROMCorruptor)} directory.{Environment.NewLine}{Environment.NewLine}Stash files only save the bytes that are changed, so it takes up less storage space and allow the user to also edit each byte that was affected by using the Stash Editor. (e.g. changing a characters colour)", $"{nameof(LunarROMCorruptor)} - Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void ByteModeHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show($"With Corrupt Every Nth Byte, you can apply regular corruptions to your file by specifying the interval to corrupt (e.g., every 1st byte, every 2nd byte, etc.).{Environment.NewLine}{Environment.NewLine}On the other hand, Intensity allows for randomized corruptions, where the corruptor selects random addresses to modify.", $"{nameof(LunarROMCorruptor)} - Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void ByteViewHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show($"ByteView is a utility that offers users a visual representation of binary content. ByteView allows users to see their corruptions to the byte data, ByteView is particularly useful for those seeking a non-intrusive approach to examining the underlying byte information within files. Covert/Update will convert the file that is currently loaded in LRC to grayscale or colour bytes that you can see.", $"{nameof(LunarROMCorruptor)} - Information", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void ByteViewSaveImageBtn_Click(object sender, EventArgs e)
        {
            if (ByteViewPictureBox.Image != null)
            {
                using SaveFileDialog saveFileDialog = new();
                saveFileDialog.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Image";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the Bitmap from the PictureBox
                    Bitmap imageToSave = new(ByteViewPictureBox.Image);

                    // Save the image to the specified path
                    imageToSave.Save(saveFileDialog.FileName);

                    // Dispose the image to free up resources
                    imageToSave.Dispose();
                }
            }
        }

        private void ByteViewRefreshBtn_Click(object sender, EventArgs e)
        {
            if (CorruptionCore.ROM == null)
            {
                MessageBox.Show("No file loaded! Please load a file to use ByteView.", $"{nameof(LunarROMCorruptor)} - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ByteViewPictureBox.Image = ByteView.ConvertByteToImage(CorruptionCore.ROM, byteViewColourChkbox.Checked);
            if (flipVerticalChkbox.Checked || flipHorizontalChkbox.Checked)
            {
                ByteViewPictureBox.Image = ByteView.FlipImage((Bitmap)ByteViewPictureBox.Image!, flipHorizontalChkbox.Checked, flipVerticalChkbox.Checked);
            }
            GC.Collect(); //Forces Garbage collection
        }

        private void byteViewColourChkbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.ByteViewColourMode = byteViewColourChkbox.Checked;
            Settings.Default.Save();
        }

        private void ByteViewupdateWhenCorruptedChkBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.ByteViewUpdateOnCorruption = ByteViewupdateWhenCorruptedChkBox.Checked;
            Settings.Default.Save();
        }

        private void AttemptProtectedFileOverrideChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AttemptProtectedFileOverrideChkBox.Checked == true && Settings.Default.AttemptProtectedFileOverride == false) //User has enabled this option and it hasn't been saved to settings, so this is not the program setting it up via LoadSettings
            {
                if (MessageBox.Show("WARNING: You are about to enable ownership override. This action can potentially allow corruption of critical protected system files if you select the wrong file(s). Ensure you understand the implications before proceeding.\nDo you wish to enable this?", "WARNING - Critical Action", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    AttemptProtectedFileOverrideChkBox.Checked = false;
                }
            }
            Settings.Default.AttemptProtectedFileOverride = AttemptProtectedFileOverrideChkBox.Checked;
            Settings.Default.Save();
        }
    }
}