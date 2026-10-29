using LunarROMCorruptor.Modules.CorruptionInternals.Engines;

namespace LunarROMCorruptor.EngineControls
{
    public partial class BlocksEngineControl : UserControl
    {
        public BlocksEngineControl()
        {
            InitializeComponent();
        }

        public BlocksEngine.Settings GetSettings()
        {
            if (!Enum.TryParse(BlocksModeComboBox.Text, out BlocksEngine.BlockMode mode))
                throw new InvalidOperationException($"Invalid Blocks Engine operation '{BlocksModeComboBox.Text}'.");
            int element = (int)BlocksElementNumericUpDown.Value;
            int block = (int)BlocksSizeNumericUpDown.Value;
            if (block < element * 2)
                throw new InvalidOperationException("The Blocks Engine block size must be at least two elements (block size >= 2 x element size).");
            return new BlocksEngine.Settings
            {
                Mode = mode,
                BlockSize = block,
                ElementSize = element,
                EchoDistance = (long)BlocksEchoNumericUpDown.Value,
                Descending = BlocksDescendingChkBox.Checked
            };
        }
    }
}
