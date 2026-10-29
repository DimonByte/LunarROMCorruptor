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


using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace LunarROMCorruptor.Modules.Helpers
{
    internal class FileOverrideHelper
    {
        /// <summary>
        /// Checks if a file can be opened with the given access. Returns a human readable reason if it can't.
        /// </summary>
        public static bool CanOpen(string path, FileAccess access, out string? reason)
        {
            reason = null;
            try
            {
                using var stream = new FileStream(path, FileMode.Open, access, FileShare.ReadWrite | FileShare.Delete, 1);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                reason = File.Exists(path) && File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly) && access != FileAccess.Read
                    ? "The file is marked as read-only."
                    : $"Access denied ({ex.Message}). Enabling 'Take File Ownership' may fix this.";
                return false;
            }
            catch (IOException ex) when ((ex.HResult & 0xFFFF) is 32 or 33) //ERROR_SHARING_VIOLATION / ERROR_LOCK_VIOLATION
            {
                reason = "The file is being used by another process (is the emulator or another program still running?). Permission override can't fix this.";
                return false;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Checks if new files can be created in a folder.
        /// </summary>
        public static bool CanCreateIn(string directory, out string? reason)
        {
            reason = null;
            string probe = Path.Combine(directory, $".lrc-write-test-{Guid.NewGuid():N}.tmp");
            try
            {
                using (new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose)) { }
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                reason = $"Access denied ({ex.Message}). Enabling 'Take File Ownership' may fix this.";
                return false;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }
            finally
            {
                try { if (File.Exists(probe)) File.Delete(probe); } catch { }
            }
        }

        /// <summary>
        /// Takes ownership of a file or folder and grants the current user full control over it (takeown + icacls), elevating with UAC if needed.
        /// Returns true if the commands completed successfully.
        /// </summary>
        public static bool AttemptProtectedFileOverride(string targetPath)
        {
            TraceLogger.Log($"Attempting protected file override for: {targetPath}");
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                TraceLogger.Log("Target path is null or empty.", StatusSeverityType.Error);
                return false;
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(targetPath).TrimEnd('\\');
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Invalid path for permission override: {ex.Message}", StatusSeverityType.Error);
                return false;
            }

            bool isFile = File.Exists(fullPath);
            bool isDirectory = !isFile && Directory.Exists(fullPath);
            if (!isFile && !isDirectory)
            {
                TraceLogger.Log($"Target does not exist: {fullPath}", StatusSeverityType.Error);
                return false;
            }
            if (fullPath.Contains('"'))
            {
                TraceLogger.Log("The path contains a quote character, permission override has been skipped.", StatusSeverityType.Error);
                return false;
            }

            //Use the SID of the current (non-elevated) user. When UAC elevation is done with a different admin account,
            //%USERNAME% inside the elevated process would be that admin, not the person running LRC.
            string? sid = WindowsIdentity.GetCurrent().User?.Value;
            if (string.IsNullOrEmpty(sid))
            {
                TraceLogger.Log("Unable to get the current user's SID, permission override has been skipped.", StatusSeverityType.Error);
                return false;
            }

            //Clear the read-only attribute first (often enough on its own)
            if (isFile)
            {
                try
                {
                    var attributes = File.GetAttributes(fullPath);
                    if (attributes.HasFlag(FileAttributes.ReadOnly))
                    {
                        File.SetAttributes(fullPath, attributes & ~FileAttributes.ReadOnly);
                        TraceLogger.Log("Cleared the read-only attribute.");
                        if (CanOpen(fullPath, FileAccess.ReadWrite, out _))
                            return true;
                    }
                }
                catch (Exception ex)
                {
                    TraceLogger.Log($"Unable to clear the read-only attribute yet ({ex.Message}), continuing with ownership override.", StatusSeverityType.Warning);
                }
            }

            //Notes on the command:
            // - cmd uses && to chain commands (the old code used ';' which is PowerShell syntax, so icacls never ran).
            // - (OI)(CI) and /T are for folders only, files just get :F.
            // - cmd /c "<...>" strips the outer quotes, so the inner quoted paths stay intact.
            string attrib = isFile ? $"attrib -r \"{fullPath}\" & " : "";
            string grant = isFile ? $"\"*{sid}:F\"" : $"\"*{sid}:(OI)(CI)F\"";
            string commands = $"{attrib}takeown /F \"{fullPath}\" && icacls \"{fullPath}\" /grant {grant}";

            bool isAdmin = CommandRunner.IsRunAsAdmin();
            int exitCode;
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
                    Arguments = $"/c \"{commands}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                };
                if (isAdmin)
                {
                    TraceLogger.Log("Running as admin, taking ownership directly.");
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardError = true;
                }
                else
                {
                    //Output can't be captured from an elevated process, the result is verified afterwards instead.
                    TraceLogger.Log("Not running as admin, requesting elevation (UAC) for the ownership override.", StatusSeverityType.Warning);
                    startInfo.UseShellExecute = true;
                    startInfo.Verb = "runas";
                }

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    TraceLogger.Log("Failed to start the permission override process.", StatusSeverityType.Error);
                    return false;
                }
                Task<string>? stdout = isAdmin ? process.StandardOutput.ReadToEndAsync() : null;
                Task<string>? stderr = isAdmin ? process.StandardError.ReadToEndAsync() : null;
                if (!process.WaitForExit(60000))
                {
                    TraceLogger.Log("The permission override process didn't finish within 60 seconds.", StatusSeverityType.Error);
                    return false;
                }
                exitCode = process.ExitCode;
                if (stdout != null && stderr != null)
                {
                    string output = (stdout.Result + Environment.NewLine + stderr.Result).Trim();
                    if (output.Length > 0) TraceLogger.Log($"Permission override output:\n{output}");
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223) //ERROR_CANCELLED
            {
                TraceLogger.Log("The UAC prompt was cancelled, permission override has been skipped.", StatusSeverityType.Warning, true);
                return false;
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Error during file ownership and permission override: {ex.Message}", StatusSeverityType.Error);
                return false;
            }

            if (exitCode != 0)
            {
                TraceLogger.Log($"Permission override failed (exit code {exitCode}).{(isAdmin ? " See the output above for the reason." : "")}", StatusSeverityType.Error);
                return false;
            }

            bool works = isFile ? CanOpen(fullPath, FileAccess.ReadWrite, out string? reason) : CanCreateIn(fullPath, out reason);
            if (works)
                TraceLogger.Log("File ownership and permission override completed successfully.");
            else
                TraceLogger.Log($"Ownership was taken but the {(isFile ? "file" : "folder")} still can't be written: {reason}", StatusSeverityType.Error);
            return works;
        }
    }
}
