using LunarROMCorruptor.Modules.CorruptionInternals.Engines;

namespace LunarROMCorruptor.EngineControls
{
    public partial class ManualEngineControl : UserControl
    {
        public ManualEngineControl()
        {
            InitializeComponent();
        }

        public ManualEngine.Settings GetSettings()
        {
            return new ManualEngine.Settings
            {
                Increment = IncrementCHECK.Checked,
                IncrementValue = (int)IncrementNumericUpDown.Value,
                Shift = SHIFTBYTECHECK.Checked,
                ShiftValue = (long)ShiftNumericUpDown.Value,
                MakeEqual = MakeBitEqualCHECK.Checked,
                EqualValue = (byte)Math.Clamp(ByteEqualNumericUpDown.Value, 0, 255),
                Replace = ReplaceCHECK.Checked,
                ReplaceFrom = (byte)Math.Clamp(ReplaceNumericUpDown1.Value, 0, 255),
                ReplaceTo = (byte)Math.Clamp(ReplaceNumericUpDown2.Value, 0, 255),
                PasteRandom = PasterandombitCHECK.Checked,
                RepeatRandom = RepeatRandomBitCHECK.Checked,
                RepeatCount = (int)RepeatNumericUpDown.Value,
                MathEnabled = MULTIORDIVIDECHeck.Checked,
                Operation = DivideRadio.Checked ? ManualEngine.MathOperation.Divide : DoubleCheck.Checked ? ManualEngine.MathOperation.Power : ManualEngine.MathOperation.Multiply,
                MathValue = (long)MathOperationNumericUpDown.Value
            };
        }
    }
}
