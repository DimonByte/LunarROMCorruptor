using LunarROMCorruptor.Modules.CorruptionInternals.Engines;
using System.Globalization;

namespace LunarROMCorruptor.EngineControls
{
    public partial class LerpEngineControl : UserControl
    {
        public LerpEngineControl()
        {
            InitializeComponent();
        }

        private bool _updating;

        private void LerpSplitValueTrackBar_Scroll(object sender, EventArgs e)
        {
            _updating = true;
            LerpValueTxt.Text = (LerpSplitValueTrackBar.Value / 10.0).ToString(CultureInfo.CurrentCulture);
            _updating = false;
        }

        private void LerpValueTxt_TextChanged(object sender, EventArgs e)
        {
            if (_updating) return;
            if (TryParseValue(out double value))
            {
                LerpSplitValueTrackBar.Value = Math.Clamp((int)Math.Round(value * 10), LerpSplitValueTrackBar.Minimum, LerpSplitValueTrackBar.Maximum);
            }
        }

        /// <summary>
        /// Accepts both "0.5" and "0,5" no matter what the system locale is.
        /// </summary>
        private bool TryParseValue(out double value)
        {
            string text = LerpValueTxt.Text.Trim();
            return double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
                || double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        public LerpEngine.Settings GetSettings()
        {
            if (!TryParseValue(out double value))
                throw new InvalidOperationException($"The Lerp Engine split value '{LerpValueTxt.Text}' isn't a number. Enter a value between 0.0 and 1.0.");
            return new LerpEngine.Settings { InterpolateValue = Math.Clamp(value, 0.0, 1.0) };
        }
    }
}
