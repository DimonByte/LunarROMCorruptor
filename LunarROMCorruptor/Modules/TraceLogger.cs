using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace LunarROMCorruptor.Modules
{
    public enum StatusSeverityType
    {
        Information = 0,
        Warning = 1,
        Error = 2,
        Debug = 3,
        Fatal = 4
    }
    public static class TraceLogger
    {
        private static readonly Lock _lock = new();
        private static readonly string _logDirectory = "Logs";
        private static string _currentDate = DateTime.Now.ToString("dd-MM-yyyy");
        private static DateTime _lastDateCheck = DateTime.MinValue;
        private static readonly bool EnableDiskLogging = false;
        private static bool _isDirectoryCreated = false;
        public static Action<string, StatusSeverityType> OnLogEntryAdded;
        static TraceLogger()
        {
            EnsureLogDirectoryExists();
        }

        private static void EnsureLogDirectoryExists()
        {
            if (_isDirectoryCreated || !EnableDiskLogging) return;

            try
            {
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                    Console.WriteLine($"Created log directory: {_logDirectory}");
                }
                _isDirectoryCreated = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create log directory '{_logDirectory}': {ex.Message}");
            }
        }

        public static void PurgeAllLogs()
        {
            foreach (string file in Directory.GetFiles(_logDirectory))
            {
                Console.WriteLine($"Deleting all logs. Currently deleting: {file}");
                File.Delete(file);
            }
        }
        public static void ClearExpiredLogs()
        {
            lock (_lock)
            {
                _lastDateCheck = DateTime.MinValue;
                try
                {
                    if (!Directory.Exists(_logDirectory))
                        return;
                    var logFiles = Directory.GetFiles(_logDirectory, "*.log");
                    DateTime expiryDate = DateTime.Now.AddDays(-7);
                    foreach (var logFile in logFiles)
                    {
                        FileInfo fileInfo = new(logFile);
                        if (fileInfo.CreationTime < expiryDate)
                        {
                            fileInfo.Delete();
                            Log($"Deleted expired log file: {fileInfo.Name}");
                            Debug.WriteLine($"Deleted expired log file: {fileInfo.Name}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Failed to clear expired logs: {ex}", StatusSeverityType.Error);
                    Debug.WriteLine($"Failed to clear expired logs: {ex}");
                }
            }
        }

        public static void Log(string message, StatusSeverityType severity = StatusSeverityType.Information, bool reportErrorAsMessageBox = false,
                              [CallerMemberName] string memberName = "",
                              [CallerFilePath] string filePath = "",
                              [CallerLineNumber] int lineNumber = 0)
        {

            string className = ExtractClassName(filePath);
            if (string.IsNullOrEmpty(message))
            {
                Log($"The function {memberName} in {className} class has called the TraceLogger.Log at line {lineNumber} but hasn't defined any of the log variables! That class may be malfunctioning.", StatusSeverityType.Warning);
            }
            string logEntry = string.Empty;
            string filePathLog = Path.Combine(_logDirectory, $"{_currentDate}.log");
            try
            {
                DateTime now = DateTime.Now;
                var currentDate = now.ToString("dd-MM-yyyy");
                if (now.Subtract(_lastDateCheck).TotalSeconds > 10)
                {
                    _currentDate = currentDate;
                    _lastDateCheck = now;
                }
                string timestamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string severityText = severity.ToString().ToUpper();
                string processID = Environment.ProcessId.ToString();
                logEntry = $"[{timestamp}] [PID: {processID}] [{severityText}] [{className}] [{memberName}] [Line: {lineNumber}]: {message}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to prepare log entry: {ex}");
            }
            if (reportErrorAsMessageBox)
            {
                MessageBox.Show($"An {severity} message occurred in LRC at {className}.{memberName}: {message}", $"{severity} occurred in LRC", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Debug.WriteLine(logEntry);
            OnLogEntryAdded?.Invoke(logEntry, severity);
            lock (_lock)
            {
                try
                {
                    if (EnableDiskLogging)
                    {
                        File.AppendAllText(filePathLog, $"{logEntry}{Environment.NewLine}", Encoding.UTF8);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to write to log: {ex}");
                }
            }
        }

        private static string ExtractClassName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "UnknownClass";

            try
            {
                string fileName = Path.GetFileName(filePath);

                int lastDot = fileName.LastIndexOf('.');
                if (lastDot > 0)
                    fileName = fileName[..lastDot];

                int lastDotInPath = filePath.LastIndexOf(Path.DirectorySeparatorChar);
                if (lastDotInPath > 0)
                {
                    string directoryPath = filePath[..lastDotInPath];
                    int lastDirectorySeparator = directoryPath.LastIndexOf(Path.DirectorySeparatorChar);
                    if (lastDirectorySeparator > 0)
                    {
                        string className = fileName;
                        return className;
                    }
                }

                return fileName;
            }
            catch
            {
                return "UnknownClass";
            }
        }
    }
}
