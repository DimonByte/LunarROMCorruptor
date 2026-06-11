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

namespace LunarROMCorruptor.Modules.Helpers
{
    internal class FileOverrideHelper
    {
        public static void AttemptProtectedFileOverride(string targetFilePath)
        {
            TraceLogger.Log($"Attempting protected file override for file: {targetFilePath}");
            if (string.IsNullOrEmpty(targetFilePath))
            {
                TraceLogger.Log("LRC - Corruption Core: Target file path is null or empty.", StatusSeverityType.Error);
                return;
            }
            try
            {
                bool isAdmin = CommandRunner.IsRunAsAdmin();

                // Check if file exists and is protected
                if (!File.Exists(targetFilePath))
                {
                    TraceLogger.Log($"LRC - Corruption Core: Target file does not exist: {targetFilePath}", StatusSeverityType.Error);
                    return;
                }

                if (!isAdmin)
                {
                    // Run the entire override process elevated
                    TraceLogger.Log("LRC - Corruption Core: Not running as admin. Attempting to elevate for file ownership override.", StatusSeverityType.Warning);

                    string takeOwnCommand = $"takeown /F \"{targetFilePath}\"";
                    string icaclsCommand = $"icacls \"{targetFilePath}\" /grant {Environment.UserDomainName}\\{Environment.UserName}:(OI)(CI)F /T";

                    // Create a PowerShell command that will execute both commands
                    string fullCommand = $"{takeOwnCommand} ; {icaclsCommand}";

                    bool elevatedSuccess = CommandRunner.RunElevatedCommand("cmd.exe", $"/c \"{fullCommand}\"");

                    if (elevatedSuccess)
                    {
                        TraceLogger.Log("LRC - Corruption Core: File ownership and permission override completed successfully with elevated permissions.", StatusSeverityType.Information);
                    }
                    else
                    {
                        TraceLogger.Log("LRC - Corruption Core: Failed to complete file override with elevated permissions.", StatusSeverityType.Error);
                    }
                }
                else
                {
                    // Already running as admin, proceed directly
                    TraceLogger.Log("LRC - Corruption Core: Running as admin. Proceeding with file ownership override.", StatusSeverityType.Information);

                    using (Process cmdProcess = new())
                    {
                        cmdProcess.StartInfo.FileName = "CMD.exe";
                        cmdProcess.StartInfo.Arguments = $"/c takeown /F \"{targetFilePath}\" && icacls \"{targetFilePath}\" /grant {Environment.UserDomainName}\\{Environment.UserName}:(OI)(CI)F /T";
                        cmdProcess.StartInfo.CreateNoWindow = true;
                        cmdProcess.StartInfo.UseShellExecute = false;
                        cmdProcess.Start();
                        cmdProcess.WaitForExit();
                    }
                    TraceLogger.Log("LRC - Corruption Core: File ownership and permission override completed successfully.", StatusSeverityType.Information);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"LRC - Corruption Core: Error during File Ownership and Permission Override. Exception: {ex.Message}", StatusSeverityType.Error);
            }
        }
    }
}
