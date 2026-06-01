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
