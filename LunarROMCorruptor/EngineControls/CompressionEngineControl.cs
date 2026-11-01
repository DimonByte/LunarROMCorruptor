using LunarROMCorruptor.Modules;
using LunarROMCorruptor.Modules.CorruptionInternals;
using LunarROMCorruptor.Modules.CorruptionInternals.Engines;

namespace LunarROMCorruptor.EngineControls
{
    public partial class CompressionEngineControl : UserControl
    {
        public CompressionEngineControl()
        {
            InitializeComponent();
        }

        private CompressionEngine.Formats SelectedFormats()
        {
            var f = CompressionEngine.Formats.None;
            if (CompressionYaz0ChkBox.Checked) f |= CompressionEngine.Formats.Yaz0;
            if (CompressionYay0ChkBox.Checked) f |= CompressionEngine.Formats.Yay0;
            if (CompressionMio0ChkBox.Checked) f |= CompressionEngine.Formats.MIO0;
            if (CompressionLz10ChkBox.Checked) f |= CompressionEngine.Formats.LZ10;
            if (CompressionLz11ChkBox.Checked) f |= CompressionEngine.Formats.LZ11;
            return f;
        }

        public CompressionEngine.Settings GetSettings()
        {
            var option = EngineParser.ParseCorruptionOptions(CompressionOptionComboBox.Text) ?? throw new InvalidOperationException($"Invalid Compression Engine corruption type '{CompressionOptionComboBox.Text}'.");
            var formats = SelectedFormats();
            if (formats == CompressionEngine.Formats.None)
                throw new InvalidOperationException("Select at least one compression format for the Compression Engine.");
            return new CompressionEngine.Settings
            {
                Formats = formats,
                Option = option,
                TiltAmount = (int)CompressionTiltNumericUpDown.Value
            };
        }

        private async void CompressionScanBtn_Click(object? sender, EventArgs e)
        {
            if (!CorruptionCore.IsLoaded || CorruptionCore.SourcePath == null || !File.Exists(CorruptionCore.SourcePath))
            {
                CompressionScanResultLbl.Text = "Load a file first.";
                return;
            }
            var formats = SelectedFormats();
            string path = CorruptionCore.SourcePath;
            CompressionScanBtn.Enabled = false;
            CompressionScanResultLbl.Text = "Scanning...";
            try
            {
                var blocks = await Task.Run(() =>
                {
                    using var data = FileBuffer.OpenRead(path);
                    return CompressionEngine.FindBlocks(data, formats, 0, data.Length - 1);
                });
                CompressionScanResultLbl.Text = blocks.Count == 0
                    ? "No compressed blocks of the selected formats were found."
                    : $"{blocks.Count} block(s): {string.Join(", ", blocks.GroupBy(b => b.Format).Select(g => $"{g.Count()} {g.Key}"))}\nUnpacked: {CorruptionCore.FormatSize(blocks.Sum(b => (long)b.DecompressedSize), out _, out _)}\nThey're shown in purple in the ByteView Regions mode.";
                TraceLogger.Log($"Compression Engine scan: {CompressionScanResultLbl.Text.Replace('\n', ' ')}");
            }
            catch (Exception ex)
            {
                CompressionScanResultLbl.Text = $"Scan failed: {ex.Message}";
            }
            finally
            {
                CompressionScanBtn.Enabled = true;
            }
        }
    }
}
