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
                TraceLogger.Log($"Refreshing file list for directory: {directoryPath}");
                listBox?.Items?.Clear();

                if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
                {
                    TraceLogger.Log($"Directory does not exist: {directoryPath}", StatusSeverityType.Warning);
                    return;
                }
                TraceLogger.Log($"Directory exists: {directoryPath}. Attempting to get files.");
                string[] files = Directory.GetFiles(directoryPath);
                if (files != null && files.Length > 0)
                {
                    listBox?.Items?.AddRange([.. files.Select(Path.GetFileName)!]);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error enumerating files in directory: {directoryPath}. Exception: {ex.Message}", StatusSeverityType.Error);
            }
        }

        public static void SaveCorruptedFileCopy(string saveAsTxt, ListBox filesaveList)
        {
            if (string.IsNullOrEmpty(saveAsTxt) || saveAsTxt == "No save location set.")
            {
                TraceLogger.Log("No valid file path provided. Aborting save operation.", StatusSeverityType.Warning);
                return;
            }

            if (!File.Exists(saveAsTxt))
            {
                TraceLogger.Log($"File does not exist at path: {saveAsTxt}. Aborting save operation.", StatusSeverityType.Error);
                return;
            }

            try
            {
                TraceLogger.Log($"Attempting to save corrupted file copy from: {saveAsTxt}");
                string fileName = Path.GetFileName(saveAsTxt);
                string extension = Path.GetExtension(fileName);
                string savesPath = SavesDirectory;

                // Create directory if it doesn't exist
                if (!Directory.Exists(savesPath))
                {
                    TraceLogger.Log($"Saves directory does not exist. Creating directory at: {savesPath}");
                    Directory.CreateDirectory(savesPath);
                }

                // Get file count to generate unique name
                int fileCount = Directory.GetFiles(savesPath).Length;
                string newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}{fileCount + 1}{extension}";

                File.Copy(saveAsTxt, Path.Combine(savesPath, newFileName));
                filesaveList?.Items?.Add(newFileName);
                TraceLogger.Log($"File copy saved successfully as: {newFileName}");
            }
            catch (ArgumentException)
            {
                TraceLogger.Log("Argument error (FileSave). Did you select an item?", StatusSeverityType.Error, true);
            }
        }

        public static void TransferStash(ListBox stashBytesList, ListBox stashFileList, List<string> internalStashItems, string fileSelectionTxt, Random rnd)
        {
            if (stashBytesList?.Items?.Count == 0)
            {
                TraceLogger.Log("No stash bytes to transfer. Aborting stash save operation.", StatusSeverityType.Warning);
                return;
            }

            var builder = new StringBuilder();

            if (stashBytesList?.Items?.Count > 0 && stashBytesList.Items[0].ToString() == "LargeStash")
            {
                TraceLogger.Log("Large stash detected. Prompting user for confirmation before saving.", StatusSeverityType.Warning);
                if (MessageBox.Show("This is a large stash which may take awhile to load in the future. Are you sure you want to save anyway?",
                    $"Warning - {nameof(LunarROMCorruptor)}", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    if (internalStashItems != null)
                    {
                        TraceLogger.Log("User confirmed saving large stash. Adding internal stash items to the file content.", StatusSeverityType.Information);
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
                    TraceLogger.Log("Adding internal stash items to the file content.", StatusSeverityType.Information);
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
                TraceLogger.Log($"Corruption stash directory does not exist. Creating directory at: {CorruptionStashDirectory}", StatusSeverityType.Information);
                Directory.CreateDirectory(CorruptionStashDirectory);
            }

            // Generate stash file name
            string fileNameWithoutExtension = string.IsNullOrEmpty(fileSelectionTxt) ?
                "unnamed" : Path.GetFileNameWithoutExtension(fileSelectionTxt);
            string randomFileName = Path.Combine(CorruptionStashDirectory, $"{fileNameWithoutExtension}-{rnd.Next(1000, 999999999)}.stash");

            TraceLogger.Log($"Saving stash to file: {randomFileName}", StatusSeverityType.Information);
            try
            {
                File.WriteAllText(randomFileName, builder.ToString());
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error writing stash to file: {randomFileName}. Exception: {ex.Message}", StatusSeverityType.Error);
            }
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
            TraceLogger.Log($"Prompting user for new file name with dialog title: {dialogTitle}", StatusSeverityType.Information);
            input.ShowDialog();

            // Validate input
            if (string.IsNullOrEmpty(input?.InputBoxTxtBox?.Text))
            {
                TraceLogger.Log("No file name entered. Aborting rename operation.", StatusSeverityType.Warning, true);
                input?.Dispose();
                return;
            }

            try
            {
                // Get selected item path
                string? selectedItemText = listBox?.GetItemText(listBox?.SelectedItem);
                if (string.IsNullOrEmpty(selectedItemText))
                {
                    TraceLogger.Log("No item selected for renaming. Aborting rename operation.", StatusSeverityType.Warning, true);
                    //MessageBox.Show("No item selected!");
                    input?.Dispose();
                    return;
                }

                string selectedItemPath = Path.Combine(directoryPath, selectedItemText);

                // Validate file exists
                if (!File.Exists(selectedItemPath))
                {
                    TraceLogger.Log($"Selected file does not exist at path: {selectedItemPath}. Aborting rename operation.", StatusSeverityType.Error, true);
                    input?.Dispose();
                    return;
                }

                // Validate file name
                if (!IsValidFileName(input.InputBoxTxtBox.Text))
                {
                    TraceLogger.Log($"Invalid file name entered: {input.InputBoxTxtBox.Text}. Aborting rename operation.", StatusSeverityType.Warning, true);
                    input?.Dispose();
                    return;
                }

                // Generate new path
                string extension = Path.GetExtension(selectedItemPath);
                string destinationPath = Path.Combine(directoryPath, input.InputBoxTxtBox.Text + extension);

                // Move file
                TraceLogger.Log($"Renaming file from: {selectedItemPath} to: {destinationPath}", StatusSeverityType.Information);
                File.Move(selectedItemPath, destinationPath);

                if (listBox?.Items != null)
                {
                    TraceLogger.Log($"Updating ListBox item for renamed file. New name: {input.InputBoxTxtBox.Text + extension}", StatusSeverityType.Information);
                    int selectedIndex = listBox.SelectedIndex;
                    listBox.Items[selectedIndex] = input.InputBoxTxtBox.Text + extension;
                }

                // Refresh file list
                RefreshFileList(listBox!, directoryPath);

                input?.Dispose();
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error renaming file. Exception: {ex.Message}", StatusSeverityType.Error, true);
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
                    TraceLogger.Log("No items selected for deletion. Aborting delete operation.", StatusSeverityType.Warning, true);
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete the selected items?", "",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    TraceLogger.Log($"User confirmed deletion of selected items. Deleting files from directory: {directoryPath}", StatusSeverityType.Information);
                    foreach (var item in listBox!.SelectedItems.Cast<object>())
                    {
                        string? itemText = item?.ToString();
                        if (!string.IsNullOrEmpty(itemText))
                        {
                            string fullPath = Path.Combine(directoryPath, itemText);
                            TraceLogger.Log($"Attempting to delete file at path: {fullPath}", StatusSeverityType.Information);
                            File.Delete(fullPath);
                        }
                    }

                    // Refresh list
                    RefreshFileList(listBox, directoryPath);
                }
            }
            catch (UnauthorizedAccessException)
            {
                TraceLogger.Log("Access denied when attempting to delete file. Check permissions for the directory.", StatusSeverityType.Error, true);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error deleting file. Exception: {ex.Message}", StatusSeverityType.Error, true);
            }
        }

        public static void CopySaveToCorruptedFile(string saveAsTxt, ListBox filesaveList)
        {
            if (string.IsNullOrEmpty(saveAsTxt) || saveAsTxt == "No save location set.")
            {
                TraceLogger.Log("No valid file path provided. Aborting copy operation.", StatusSeverityType.Warning);
                return;
            }

            try
            {
                string? selectedItemText = filesaveList?.GetItemText(filesaveList?.SelectedItem);
                if (string.IsNullOrEmpty(selectedItemText))
                {
                    TraceLogger.Log("No item selected for copying. Aborting copy operation.", StatusSeverityType.Warning, true);
                    return;
                }

                string sourcePath = Path.Combine(SavesDirectory, selectedItemText);
                File.Copy(sourcePath, saveAsTxt, true);
                TraceLogger.Log($"File copied successfully from: {sourcePath} to: {saveAsTxt}", StatusSeverityType.Information);
            }
            catch (ArgumentException)
            {
                TraceLogger.Log("Argument error (FileSave). Did you select an item?", StatusSeverityType.Error, true);
            }
            catch (DirectoryNotFoundException)
            {
                TraceLogger.Log("Directory not found (FileSave). Check if the source directory exists.", StatusSeverityType.Error, true);
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