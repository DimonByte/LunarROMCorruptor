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

                //Open directory but also select the file if it exists
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
                    startInfo.Arguments = $"/select,\"{normalizedPath}\"";
                }
                else if (Directory.Exists(normalizedPath))
                {
                    TraceLogger.Log("The specified path is a directory. Attempting to open it directly in Explorer.");
                    startInfo.Arguments = $"\"{normalizedPath}\"";
                }
                else
                {
                    TraceLogger.Log($"The specified path does not exist as a file or directory: {normalizedPath}", StatusSeverityType.Warning);
                    if (normalizedPath == null)
                    {
                        TraceLogger.Log("Normalized path is null, aborting.");
                        return;
                    }
                    string directoryPath = Path.GetDirectoryName(normalizedPath)!;
                    if (Directory.Exists(directoryPath))
                    {
                        TraceLogger.Log($"Attempting to open parent directory: {directoryPath}");
                        startInfo.Arguments = $"\"{directoryPath}\"";
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

        private static Process? _emulator; //The emulator LRC started last (used for screenshots and "close if running")
        private static readonly Lock _emulatorLock = new();

        /// <summary>
        /// The emulator process LRC started (or another running instance of the same program), null if it isn't running.
        /// </summary>
        public static Process? FindEmulatorProcess(string? emulatorLocation)
        {
            lock (_emulatorLock)
            {
                try
                {
                    if (_emulator != null && !_emulator.HasExited) return _emulator;
                }
                catch { }
            }
            if (string.IsNullOrEmpty(emulatorLocation)) return null;
            try
            {
                return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(emulatorLocation)).FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
            }
            catch
            {
                return null;
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
                //Every running instance is closed (the old code only killed the first one), and LRC waits for them to exit so they don't keep the file locked
                var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(EmulatorLocation));
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(3000);
                    }
                    catch (Exception ex)
                    {
                        TraceLogger.Log($"Unable to close {process.ProcessName}: {ex.Message}", StatusSeverityType.Warning);
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
                if (processes.Length > 0) TraceLogger.Log($"Terminated running emulator: {EmulatorLocation}");
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Failure to terminate running emulator. {ex}", StatusSeverityType.Error);
            }
        }

        public static void StartEmulator(bool ReopenProgram, string EmulatorLocation, string FileLocation, bool OverrideArgumentsChk, string OverrideArguments)
        {
            TraceLogger.Log($"Starting emulator with the following settings: Reopen Program: {ReopenProgram} | Emulator Location: {EmulatorLocation} | File Location: {FileLocation} | Override Arguments: {OverrideArgumentsChk} | Arguments: {OverrideArguments}");
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
                    //Many emulators look for their cores/BIOS/config next to the exe, so they're started in their own folder
                    WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(EmulatorLocation)) ?? "",
                    Arguments = OverrideArgumentsChk ? OverrideArguments : $"\"{FileLocation}\""
                };

                var p = Process.Start(startInfo);
                lock (_emulatorLock)
                {
                    _emulator?.Dispose();
                    _emulator = p;
                }

                TraceLogger.Log("Emulator started successfully.");
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error when starting emulator: {ex}", StatusSeverityType.Error, true);
            }
        }
    }

    /// <summary>
    /// Takes screenshots of another program's window (the emulator) for stash previews.
    /// </summary>
    internal static class WindowCapture
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdc, uint flags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const uint PW_CLIENTONLY = 1, PW_RENDERFULLCONTENT = 2;

        /// <summary>
        /// Screenshot of the client area (the game picture, without the title bar) of a process' main window. Returns null when it isn't
        /// possible (no window, minimised). PrintWindow is tried first so the window doesn't have to be in front; if that only gives a
        /// black image (some GPU-rendered windows), the window is brought to the front for a moment and copied from the screen.
        /// </summary>
        public static Bitmap? CaptureMainWindow(Process process, IntPtr returnFocusTo)
        {
            try
            {
                process.Refresh();
                IntPtr hwnd = process.MainWindowHandle;
                if (hwnd == IntPtr.Zero || IsIconic(hwnd) || !GetClientRect(hwnd, out var rect)) return null;
                int w = rect.Right - rect.Left, h = rect.Bottom - rect.Top;
                if (w <= 0 || h <= 0) return null;
                var bitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                bool ok;
                using (var g = Graphics.FromImage(bitmap))
                {
                    IntPtr hdc = g.GetHdc();
                    try { ok = PrintWindow(hwnd, hdc, PW_CLIENTONLY | PW_RENDERFULLCONTENT); }
                    finally { g.ReleaseHdc(hdc); }
                }
                if (ok && !IsBlank(bitmap)) return bitmap;
                //Fallback: copy it from the screen
                var origin = new POINT();
                if (!ClientToScreen(hwnd, ref origin)) { bitmap.Dispose(); return null; }
                SetForegroundWindow(hwnd);
                Thread.Sleep(250);
                using (var g = Graphics.FromImage(bitmap))
                    g.CopyFromScreen(origin.X, origin.Y, 0, 0, new Size(w, h));
                if (returnFocusTo != IntPtr.Zero) SetForegroundWindow(returnFocusTo);
                if (IsBlank(bitmap)) { bitmap.Dispose(); return null; }
                return bitmap;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to take a screenshot of the emulator: {ex.Message}", StatusSeverityType.Warning);
                return null;
            }
        }

        /// <summary>
        /// True if every sampled pixel is (almost) the same colour, e.g. the all-black image PrintWindow gives for some GPU windows.
        /// </summary>
        private static bool IsBlank(Bitmap bitmap)
        {
            Color first = bitmap.GetPixel(0, 0);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Color c = bitmap.GetPixel(x * (bitmap.Width - 1) / 15, y * (bitmap.Height - 1) / 15);
                    if (Math.Abs(c.R - first.R) + Math.Abs(c.G - first.G) + Math.Abs(c.B - first.B) > 24) return false;
                }
            }
            return true;
        }
    }
}