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

using System.Diagnostics;
using System.Security.Principal;

namespace LunarROMCorruptor.Modules
{
    internal class CommandRunner
    {
        public static void OpenFolderInExplorer(string folderPath)
        {
            try
            {
                TraceLogger.Log($"Attempting to open folder in Explorer: {folderPath}");
                // Validate input
                if (string.IsNullOrEmpty(folderPath))
                {
                    TraceLogger.Log("Folder path is null or empty.", StatusSeverityType.Error);
                    return;
                }



                // Get the full path to normalize it
                string normalizedPath = Path.GetFullPath(folderPath);
                TraceLogger.Log($"Normalized folder path: {normalizedPath}");
                // Check if the path represents a file
                if (File.Exists(normalizedPath))
                {
                    // If it's a file, get its directory and open that instead
                    string directoryPath = Path.GetDirectoryName(normalizedPath);
                    TraceLogger.Log($"The specified path is a file. Attempting to open its directory: {directoryPath}");

                    ProcessStartInfo startInfo = new()
                    {
                        FileName = "explorer.exe",
                        Arguments = $"{directoryPath}",
                        UseShellExecute = true,
                        CreateNoWindow = true
                    };
                    using Process? process = Process.Start(startInfo);
                    if (process == null)
                    {
                        TraceLogger.Log("Failed to start process to open folder in Explorer.", StatusSeverityType.Error);
                    }
                }
                else if (Directory.Exists(normalizedPath))
                {
                    TraceLogger.Log("The specified path is a directory. Attempting to open it directly in Explorer.");
                    // If it's a directory, open it directly
                    ProcessStartInfo startInfo = new()
                    {
                        FileName = "explorer.exe",
                        Arguments = $"{normalizedPath}",
                        UseShellExecute = true,
                        CreateNoWindow = true
                    };
                    using Process? process = Process.Start(startInfo);
                    if (process == null)
                    {
                        TraceLogger.Log("Failed to start process to open folder in Explorer.", StatusSeverityType.Error);
                    }
                }
                else
                {
                    TraceLogger.Log($"The specified path does not exist as a file or directory: {normalizedPath}", StatusSeverityType.Warning);
                    // Try to get the directory of the file path first
                    string directoryPath = Path.GetDirectoryName(normalizedPath);
                    if (Directory.Exists(directoryPath))
                    {


                        ProcessStartInfo startInfo = new()
                        {
                            FileName = "explorer.exe",
                            Arguments = $"{directoryPath}",
                            UseShellExecute = true,
                            CreateNoWindow = true
                        };
                        using Process? process = Process.Start(startInfo);
                        if (process == null)
                        {
                            TraceLogger.Log("Failed to start process to open folder in Explorer.", StatusSeverityType.Error);
                        }
                    }
                    else
                    {
                        TraceLogger.Log($"The specified path does not exist as a file or directory: {normalizedPath}", StatusSeverityType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error opening folder in Explorer: {ex}", StatusSeverityType.Error);
            }
        }
        public static string RunCommand(string command)
        {
            try
            {
                TraceLogger.Log($"Running command: {command}");
                var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using Process process = Process.Start(processInfo) ?? throw new InvalidOperationException("Failed to start process for command execution.");
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                TraceLogger.Log("Waiting for command to exit...");
                process.WaitForExit();
                TraceLogger.Log($"Command exited with code: {process.ExitCode}");
                if (process.ExitCode != 0)
                    throw new InvalidOperationException($"Command execution failed with exit code {process.ExitCode}: {error}");
                TraceLogger.Log($"Command output: {output}");
                return output;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error running command '{command}': {ex}", StatusSeverityType.Error);
                return string.Empty;
            }
        }
        public static bool IsRunAsAdmin()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new(identity);
                TraceLogger.Log($"IsRunAsAdmin check: User={identity.Name}, IsAdmin={principal.IsInRole(WindowsBuiltInRole.Administrator)}");
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                TraceLogger.Log("Failed to determine if running as admin.", StatusSeverityType.Warning);
                return false;
            }
        }

        public static bool RunElevatedCommands(string[] commands)
        {
            try
            {
                string allCommands = string.Join(";", commands);
                TraceLogger.Log($"Attempting to run elevated commands: {allCommands}");
                ProcessStartInfo startInfo = new()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -WindowStyle Hidden -Command \"{allCommands}\"",
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                using Process? process = Process.Start(startInfo);
                if (process == null)
                {
                    TraceLogger.Log("Failed to start process for elevated commands.", StatusSeverityType.Error);
                    return false;
                }
                TraceLogger.Log("Waiting for elevated commands to complete...");
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    TraceLogger.Log($"Command failure! Elevated commands exited with exit code: {process.ExitCode}", StatusSeverityType.Error);
                    return false;
                }
                else
                {
                    TraceLogger.Log("Elevated commands executed successfully. Returned exit code 0.");
                    return true;
                }
            }
            catch (OperationCanceledException ex1)
            {
                TraceLogger.Log($"The elevated command was cancelled by the user. Command run failed! {ex1.Message}", StatusSeverityType.Error);
                return false;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Failed to run elevated commands: {ex}", StatusSeverityType.Error);
                return false;
            }
        }

        public static bool RunElevatedCommand(string command, string arguments = "")
        {
            try
            {
                string fullArguments = $"{arguments}";
                TraceLogger.Log($"Attempting to run elevated command: {command} {fullArguments}");
                ProcessStartInfo startInfo = new()
                {
                    FileName = command,
                    Arguments = fullArguments,
                    Verb = "runas",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                using Process? process = Process.Start(startInfo);
                if (process == null)
                {
                    TraceLogger.Log("Failed to start process for elevated command.", StatusSeverityType.Error);
                    return false;
                }
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    TraceLogger.Log($"Elevated command exited with code: {process.ExitCode}", StatusSeverityType.Error);
                }
                else
                {
                    TraceLogger.Log("Elevated command executed successfully.");
                }
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Failed to run elevated command: {ex}", StatusSeverityType.Error);
                return false;
            }
        }

        public static bool SaveToFileWithElevation(string filePath, string content)
        {
            try
            {
                string tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, content);
                TraceLogger.Log($"Temporary file created at {tempFilePath} for saving content with elevation.");
                bool success = RunElevatedCommand("cmd.exe", $"/c move /Y \"{tempFilePath}\" \"{filePath}\"");
                if (success)
                {
                    TraceLogger.Log($"Content saved to {filePath} successfully with elevation.");
                }
                else
                {
                    TraceLogger.Log($"Failed to save content to {filePath} with elevation.", StatusSeverityType.Error);
                }
                return success;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error saving to file with elevation: {ex}", StatusSeverityType.Error);
                return false;
            }
        }
    }
}
