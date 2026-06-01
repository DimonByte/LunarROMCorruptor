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

using System.Text;

namespace LunarROMCorruptor.Modules
{
    public class IOManager
    {
        private static readonly string SavesDirectory = Path.Combine(Application.StartupPath, "Saves");
        private static readonly string CorruptionStashDirectory = Path.Combine(Application.StartupPath, "CorruptionStashList");

        // Common validation for file names
        private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5",
            "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4",
            "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        private static readonly HashSet<char> InvalidChars = ['\\', '/', ':', '*', '?', '"', '<', '>', '|'];

        public static void RefreshCorruptionStashList(ListBox stashFileList)
        {
            RefreshFileList(stashFileList, CorruptionStashDirectory);
        }

        public static void RefreshFileSaves(ListBox filesaveList)
        {
            RefreshFileList(filesaveList, SavesDirectory);
        }

        private static void RefreshFileList(ListBox listBox, string directoryPath)
        {
            try
            {
                listBox?.Items?.Clear();

                if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
                {
                    return;
                }

                string[] files = Directory.GetFiles(directoryPath);
                if (files != null && files.Length > 0)
                {
                    listBox?.Items?.AddRange([.. files.Select(Path.GetFileName)!]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error enumerating {Path.GetFileName(directoryPath)} list. This folder may not exist or your anti-virus/ransomware protection may be enabled and is blocking LRC from searching those directories. {ex.Message}",
                    $"{nameof(LunarROMCorruptor)} - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void SaveCorruptedFileCopy(string saveAsTxt, ListBox filesaveList)
        {
            if (string.IsNullOrEmpty(saveAsTxt) || saveAsTxt == "No save location set.")
            {
                return;
            }

            if (!File.Exists(saveAsTxt))
            {
                MessageBox.Show("The file you're trying to save doesn't exist. Please load a file first.",
                    $"{nameof(LunarROMCorruptor)} - ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string fileName = Path.GetFileName(saveAsTxt);
                string extension = Path.GetExtension(fileName);
                string savesPath = SavesDirectory;

                // Create directory if it doesn't exist
                if (!Directory.Exists(savesPath))
                {
                    Directory.CreateDirectory(savesPath);
                }

                // Get file count to generate unique name
                int fileCount = Directory.GetFiles(savesPath).Length;
                string newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}{fileCount + 1}{extension}";

                File.Copy(saveAsTxt, Path.Combine(savesPath, newFileName));
                filesaveList?.Items?.Add(newFileName);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("SaveCorruptedFile Argument Error: Is there a file loaded?");
            }
        }

        public static void TransferStash(ListBox stashBytesList, ListBox stashFileList, List<string> internalStashItems, string fileSelectionTxt, Random rnd)
        {
            if (stashBytesList?.Items?.Count == 0)
            {
                return;
            }

            var builder = new StringBuilder();

            if (stashBytesList?.Items?.Count > 0 && stashBytesList.Items[0].ToString() == "LargeStash")
            {
                if (MessageBox.Show("This is a large stash which may take awhile to load in the future. Are you sure you want to save anyway?",
                    $"Warning - {nameof(LunarROMCorruptor)}", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    if (internalStashItems != null)
                    {
                        foreach (var listItem in internalStashItems)
                        {
                            builder.Append(listItem);
                            builder.AppendLine();
                        }
                    }
                    return;
                }
            }
            else
            {
                if (internalStashItems != null)
                {
                    foreach (var listItem in internalStashItems)
                    {
                        builder.Append(listItem);
                        builder.AppendLine();
                    }
                }
            }

            // Ensure directory exists
            if (!Directory.Exists(CorruptionStashDirectory))
            {
                Directory.CreateDirectory(CorruptionStashDirectory);
            }

            // Generate stash file name
            string fileNameWithoutExtension = string.IsNullOrEmpty(fileSelectionTxt) ?
                "unnamed" : Path.GetFileNameWithoutExtension(fileSelectionTxt);
            string randomFileName = Path.Combine(CorruptionStashDirectory, $"{fileNameWithoutExtension}-{rnd.Next(1000, 999999999)}.stash");

            File.WriteAllText(randomFileName, builder.ToString());

            // Refresh stash file list
            RefreshFileList(stashFileList, CorruptionStashDirectory);
        }

        public static void RenameStash(ListBox stashFileList)
        {
            RenameFile(stashFileList, CorruptionStashDirectory, "Rename to?");
        }

        public static void RenameFileSave(ListBox filesaveList)
        {
            RenameFile(filesaveList, SavesDirectory, "Rename to?");
        }

        private static void RenameFile(ListBox listBox, string directoryPath, string dialogTitle)
        {
            // Create and show input dialog
            InputBox input = new()
            {
                Text = dialogTitle
            };
            input.ShowDialog();

            // Validate input
            if (string.IsNullOrEmpty(input?.InputBoxTxtBox?.Text))
            {
                input?.Dispose();
                return;
            }

            try
            {
                // Get selected item path
                string? selectedItemText = listBox?.GetItemText(listBox?.SelectedItem);
                if (string.IsNullOrEmpty(selectedItemText))
                {
                    MessageBox.Show("No item selected!");
                    input?.Dispose();
                    return;
                }

                string selectedItemPath = Path.Combine(directoryPath, selectedItemText);

                // Validate file exists
                if (!File.Exists(selectedItemPath))
                {
                    MessageBox.Show("File doesn't exist!");
                    input?.Dispose();
                    return;
                }

                // Validate file name
                if (!IsValidFileName(input.InputBoxTxtBox.Text))
                {
                    MessageBox.Show("Invalid File Name!");
                    input?.Dispose();
                    return;
                }

                // Generate new path
                string extension = Path.GetExtension(selectedItemPath);
                string destinationPath = Path.Combine(directoryPath, input.InputBoxTxtBox.Text + extension);

                // Move file
                File.Move(selectedItemPath, destinationPath);

                if (listBox?.Items != null)
                {
                    int selectedIndex = listBox.SelectedIndex;
                    listBox.Items[selectedIndex] = input.InputBoxTxtBox.Text + extension;
                }

                // Refresh file list
                RefreshFileList(listBox!, directoryPath);

                input?.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error renaming file: {ex.Message}");
                input?.Dispose();
            }
        }

        public static void DeleteFileSave(ListBox filesaveList)
        {
            DeleteFile(filesaveList, SavesDirectory);
        }

        public static void DeleteStash(ListBox stashFileList)
        {
            DeleteFile(stashFileList, CorruptionStashDirectory);
        }

        private static void DeleteFile(ListBox listBox, string directoryPath)
        {
            try
            {
                if (listBox?.SelectedItems?.Count == 0)
                {
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete the selected items?", "",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (var item in listBox!.SelectedItems.Cast<object>())
                    {
                        string? itemText = item?.ToString();
                        if (!string.IsNullOrEmpty(itemText))
                        {
                            string fullPath = Path.Combine(directoryPath, itemText);
                            File.Delete(fullPath);
                        }
                    }

                    // Refresh list
                    RefreshFileList(listBox, directoryPath);
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access Denied or nothing was selected for deletion. Check if you can write or have authorization to that directory.",
                    $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void CopySaveToCorruptedFile(string saveAsTxt, ListBox filesaveList)
        {
            if (string.IsNullOrEmpty(saveAsTxt) || saveAsTxt == "No save location set.")
            {
                return;
            }

            try
            {
                string? selectedItemText = filesaveList?.GetItemText(filesaveList?.SelectedItem);
                if (string.IsNullOrEmpty(selectedItemText))
                {
                    MessageBox.Show("No item selected!",
                        $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string sourcePath = Path.Combine(SavesDirectory, selectedItemText);
                File.Copy(sourcePath, saveAsTxt, true);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Argument error (FileSave). Did you select an item?",
                    $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("File not found (FileSave). Did you select an item?",
                    $"Error - {nameof(LunarROMCorruptor)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool IsValidFileName(string fileName)
        {
            // Check for reserved names
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            if (ReservedNames.Contains(fileName))
            {
                return false;
            }

            // Check for invalid characters
            return !fileName.Any(InvalidChars.Contains);
        }
    }
}