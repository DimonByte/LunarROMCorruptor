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

namespace LunarROMCorruptor.Modules.Helpers
{
    internal class CommandRunner
    {
        public static void OpenFolderInExplorer(string folderPath)
        {
            try
            {
                TraceLogger.Log($"Attempting to open folder in Explorer: {folderPath}");
                if (string.IsNullOrEmpty(folderPath))
                {
                    TraceLogger.Log("Folder path is null or empty.", StatusSeverityType.Error);
                    return;
                }

                string normalizedPath = Path.GetFullPath(folderPath);
                TraceLogger.Log($"Normalized folder path: {normalizedPath}");

                ProcessStartInfo startInfo = new()
                {
                    FileName = "explorer.exe",
                    UseShellExecute = true,
                    CreateNoWindow = true
                };

                if (File.Exists(normalizedPath))
                {
                    string? directoryPath = Path.GetDirectoryName(normalizedPath);
                    if (string.IsNullOrEmpty(directoryPath))
                    {
                        TraceLogger.Log("Null path before opening, aborting.");
                        return;
                    }
                    TraceLogger.Log($"The specified path is a file. Attempting to open its directory: {directoryPath}");
                    startInfo.Arguments = $"{directoryPath}";
                }
                else if (Directory.Exists(normalizedPath))
                {
                    TraceLogger.Log("The specified path is a directory. Attempting to open it directly in Explorer.");
                    startInfo.Arguments = $"{normalizedPath}";
                }
                else
                {
                    TraceLogger.Log($"The specified path does not exist as a file or directory: {normalizedPath}", StatusSeverityType.Warning);
                    string directoryPath = Path.GetDirectoryName(normalizedPath);
                    if (Directory.Exists(directoryPath))
                    {
                        TraceLogger.Log($"Attempting to open parent directory: {directoryPath}");
                        startInfo.Arguments = $"{directoryPath}";
                    }
                    else
                    {
                        TraceLogger.Log($"The specified path does not exist as a file or directory: {normalizedPath}", StatusSeverityType.Error, true);
                        return;
                    }
                }

                using Process? process = Process.Start(startInfo);
                if (process == null)
                {
                    TraceLogger.Log("Failed to start process to open folder in Explorer.", StatusSeverityType.Error, true);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error opening folder in Explorer: {ex}", StatusSeverityType.Error);
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

        public static void TerminateEmulator(string EmulatorLocation)
        {
            try
            {
                if (string.IsNullOrEmpty(EmulatorLocation))
                {
                    TraceLogger.Log("Emulator string passed was null.", StatusSeverityType.Error);
                    return;
                }
                var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(EmulatorLocation));
                if (processes.Length > 0)
                    processes[0].Kill();
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Failure to terminate running emulator. {ex}", StatusSeverityType.Error);
            }
        }

        public static void StartEmulator(bool ReopenProgram, string EmulatorLocation, string FileLocation, bool OverrideArgumentsChk, string OverrideArguments)
        {
            TraceLogger.Log($"Starting emulator with the following settings:\nReopen Program: {ReopenProgram}\nEmulator Location: {EmulatorLocation}\nFile Location: {FileLocation}\nOverride Arguments: {OverrideArgumentsChk}\nArguments: {OverrideArguments}");
            try
            {
                if (ReopenProgram)
                {
                    TerminateEmulator(EmulatorLocation);
                }

                ProcessStartInfo startInfo = new()
                {
                    FileName = EmulatorLocation,
                    UseShellExecute = false,
                    Arguments = OverrideArgumentsChk ? OverrideArguments : $"\"{FileLocation}\""
                };

                Process p = new()
                {
                    StartInfo = startInfo
                };

                p.Start();

                TraceLogger.Log("Emulator started successfully.");
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error when starting emulator: {ex}", StatusSeverityType.Error, true);
            }
        }
    }
}