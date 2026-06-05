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

namespace LunarROMCorruptor
{
    public partial class SelectProcess : Form
    {
        public int SelectedProcessID = 999999; //Set the starting variable number to 999999 so the program can tell if the user has selected a process or not.
        public SelectProcess()
        {
            InitializeComponent();
        }

        private void SelectProcess_Load(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void RefreshBtn_Click(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            ProcessList.Items.Clear();
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                string processName = process.ProcessName;
                int processId = process.Id;
                ProcessList.Items.Add(processName + " (ID: " + processId + ")");
            }
        }

        private void CorruptBTN_Click(object sender, EventArgs e)
        {
            // Get the selected item from the listbox
            string selectedItem = ProcessList.SelectedItem.ToString();

            // Extract the ID number from the selected item
            int startIndex = selectedItem.IndexOf("(ID: ") + 5;
            int endIndex = selectedItem.IndexOf(")", startIndex);
            string idString = selectedItem.Substring(startIndex, endIndex - startIndex);
            SelectedProcessID = int.Parse(idString);
            Close();
        }

        private void SelectProcess_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
