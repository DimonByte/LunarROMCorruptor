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

namespace LunarROMCorruptor.Modules.CorruptionInternals
{
    public class ProcessCorruptionCore
    {
        //[DllImport("kernel32.dll")]
        //static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead); //For the Process Memory Corruption

        //[DllImport("kernel32.dll")]
        //static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesWritten); //For the Process Memory Corruption

        //public static bool CorruptSelectedProcess(int processID)
        //{
        //    // Get the process by ID
        //    Process process = null;
        //    try
        //    {
        //        process = Process.GetProcessById(processID);

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Invaild Operation", "LRC");
        //        return false;
        //    }

        //    ProcessModule module = process.MainModule;

        //    // Get the base address of the module
        //    var baseAddress = module.BaseAddress;

        //    // Allocate a buffer to hold the memory we want to read
        //    byte[] buffer = new byte[512];

        //    // Read the memory
        //    ReadProcessMemory(process.Handle, baseAddress, buffer, buffer.Length, out IntPtr bytesRead);

        //    // Display the memory contents
        //    Console.WriteLine("Memory contents:");
        //    for (int i = 0; i < bytesRead.ToInt64(); i++)
        //    {
        //        Console.Write(buffer[i].ToString("X2") + " ");
        //    }
        //    Console.WriteLine();

        //    IntPtr dataAddress = module.BaseAddress + 0x0;

        //    IntPtr byteValue;
        //    int bytesWritten = 0;
        //    //WriteProcessMemory(process.Handle, dataAddress, ref byteValue, 1, out bytesWritten);

        //    return false;
        //}
    }
}
