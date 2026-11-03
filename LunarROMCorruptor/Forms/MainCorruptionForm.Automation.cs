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

using LunarROMCorruptor.EngineControls;
using LunarROMCorruptor.Modules;
using LunarROMCorruptor.Modules.CorruptionInternals;
using LunarROMCorruptor.Modules.CorruptionInternals.Engines;
using LunarROMCorruptor.Modules.Helpers;
using LunarROMCorruptor.Properties;
using System.Media;

namespace LunarROMCorruptor
{
    /// <summary>
    /// Automation tasks (part of the main form).
    /// </summary>
    public partial class MainCorruptionForm
    {
        #region Automation

        private void StartAutomationBtn_Click(object sender, EventArgs e)
        {
            //If the buttons text is "Start Automation", then start automation and change the text and color to stop and OrangeRed
            if (StartAutomationBtn.Text == "Start Automation")
            {
                StartAutomationBtn.Text = "Stop Automation";
                StartAutomationBtn.ForeColor = Color.OrangeRed;
                TraceLogger.SuppressMessageBoxes = true; //Errors go to the log instead of stacking up message boxes
                AutomationTimer.Start();
            }
            else
            {
                StopAutomation();
            }
        }

        private void StopAutomation()
        {
            StartAutomationBtn.Text = "Start Automation";
            StartAutomationBtn.ForeColor = Color.SpringGreen;
            AutomationTimer.Stop();
            TraceLogger.SuppressMessageBoxes = false;
        }

        private void RemoveTaskBtn_Click(object sender, EventArgs e)
        {
            //Removes selected item from the automation list
            if (AutomationList.SelectedItem != null)
                AutomationList.Items.Remove(AutomationList.SelectedItem);
        }

        private void AddTaskBtn_Click(object sender, EventArgs e)
        {
            //Adds automation task from the combo box into the automation list
            if (AutomationTaskComboBox.SelectedItem != null)
                AutomationList.Items.Add(AutomationTaskComboBox.SelectedItem);
        }

        private void DelayInAutomationNUD_ValueChanged(object sender, EventArgs e)
        {
            //Converts the NumericUpDown number from seconds to miliseconds to use on the timer
            AutomationTimer.Interval = (int)Math.Max(1, DelayInAutomationNUD.Value * 1000);
        }

        private long RandomDecimal(decimal min, decimal max) => rnd.NextInt64((long)min, (long)max + 1);

        private async void AutomationTimer_Tick(object sender, EventArgs e)
        {
            if (_busy || _automationTickRunning) return; //Don't stack corruptions if the previous one is still running
            _automationTickRunning = true;
            try
            {
                //Snapshot the task list, the user can change it while a corruption is running
                var tasks = AutomationList.Items.Cast<object>().Select(i => i?.ToString() ?? "").ToList();
                foreach (var item in tasks)
                {
                    if (!AutomationTimer.Enabled) break;
                    switch (item)
                    {
                        case "Run Corruption":
                            var result = await RunCorruptionAsync();
                            if (result == null || result == CorruptionResult.Failed)
                            {
                                StopAutomation();
                                TraceLogger.Log("Automation stopped because the corruption couldn't run. Check the log for the reason.", StatusSeverityType.Error, true);
                                return;
                            }
                            break;
                        case "Randomize Intensity":
                            if (CorruptEverynthByteRadioBtn.Checked)
                                EveryNthByte.Value = RandomDecimal(EveryNthByte.Minimum, EveryNthByte.Maximum);
                            else
                                Intensity.Value = RandomDecimal(Intensity.Minimum, Intensity.Maximum);
                            break;
                        case "Randomize Start Byte":
                            //Keeps the start byte at or below the end byte
                            StartByteNumb.Value = RandomDecimal(StartByteNumb.Minimum, EndByteNumb.Value);
                            break;
                        case "Randomize End Byte":
                            EndByteNumb.Value = RandomDecimal(StartByteNumb.Value, EndByteNumb.Maximum);
                            break;
                        case "Randomize both Start byte and End byte":
                            {
                                long start = RandomDecimal(StartByteNumb.Minimum, StartByteNumb.Maximum);
                                long end = rnd.NextInt64(start, (long)EndByteNumb.Maximum + 1);
                                StartByteNumb.Value = Math.Min(start, (long)StartByteNumb.Maximum);
                                EndByteNumb.Value = Math.Min(end, (long)EndByteNumb.Maximum);
                            }
                            break;
                        case "Randomize Corruption Engine":
                            {
                                //Only byte engines are randomized. Merge needs a merge file and the Source Code Engine only makes sense for text files.
                                var engines = new List<CorruptionEngineType> { CorruptionEngineType.NightmareEngine, CorruptionEngineType.LerpEngine, CorruptionEngineType.LogicEngine, CorruptionEngineType.ManualEngine, CorruptionEngineType.VectorEngine, CorruptionEngineType.BlocksEngine };
                                if (File.Exists(MergeEngineFrame.MergeFileLocationTxt.Text))
                                    engines.Add(CorruptionEngineType.MergeEngine);
                                CorruptionEngineComboBox.Text = EngineParser.GetEngineDisplayName(engines[rnd.Next(engines.Count)]);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                StopAutomation();
                TraceLogger.Log($"Automation stopped because of an error: {ex.Message}", StatusSeverityType.Error, true);
            }
            finally
            {
                _automationTickRunning = false;
            }
        }

        private void MoveTaskUpBtn_Click(object sender, EventArgs e)
        {
            //Moves selected item on automation list up
            if (AutomationList.SelectedIndex > 0 && AutomationList.SelectedItem is object item)
            {
                int index = AutomationList.SelectedIndex;
                AutomationList.Items.RemoveAt(index);
                AutomationList.Items.Insert(index - 1, item);
                AutomationList.SelectedIndex = index - 1;
            }
        }

        private void MoveTaskDownBtn_Click(object sender, EventArgs e)
        {
            //Moves selected item on automation list down
            if (AutomationList.SelectedIndex >= 0 && AutomationList.SelectedIndex < AutomationList.Items.Count - 1 && AutomationList.SelectedItem is object item)
            {
                int index = AutomationList.SelectedIndex;
                AutomationList.Items.RemoveAt(index);
                AutomationList.Items.Insert(index + 1, item);
                AutomationList.SelectedIndex = index + 1;
            }
        }

        #endregion
    }
}
