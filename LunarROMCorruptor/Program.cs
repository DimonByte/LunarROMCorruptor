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

namespace LunarROMCorruptor
{
    internal static class Program
    {
        public static MainCorruptionForm Form = null!;

        [STAThread]
        private static void Main()
        {
            //Unexpected errors (e.g. in an async event handler) are logged and shown instead of closing LRC without a word
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) => ReportUnhandled(e.Exception, canContinue: true);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => ReportUnhandled(e.ExceptionObject as Exception, canContinue: false);
            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                Modules.TraceLogger.Log($"Unobserved background error: {e.Exception.GetBaseException().Message}\n{e.Exception}", Modules.StatusSeverityType.Error);
                e.SetObserved();
            };
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Form = new MainCorruptionForm();
            Application.Run(Form);
        }

        private static void ReportUnhandled(Exception? ex, bool canContinue)
        {
            try
            {
                Modules.TraceLogger.Log($"Unexpected error: {ex?.Message}\n{ex}", Modules.StatusSeverityType.Error);
                MessageBox.Show($"Something went wrong:\n\n{ex?.Message}\n\n{(canContinue ? "LRC will keep running. The details are in the Logs tab." : "LRC has to close. If a file was corrupted in place (save location = the original file), check it and restore it from a backup if needed.")}",
                    $"{nameof(LunarROMCorruptor)} - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                //Nothing else can be done here
            }
        }
    }
}