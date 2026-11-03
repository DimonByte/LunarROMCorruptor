namespace LunarROMCorruptor
{
    public partial class InputBox : Form
    {
        public InputBox()
        {
            InitializeComponent();
            CancelButton.Click -= Button1_Click;
            CancelButton.Click += (_, _) => { Cancelled = true; Hide(); };
        }

        /// <summary>
        /// True when the user closed the box with Cancel.
        /// </summary>
        public bool Cancelled { get; private set; }

        /// <summary>
        /// Asks for a line of text. Returns null when cancelled.
        /// </summary>
        public static string? Ask(string title, string initialText = "")
        {
            using var box = new InputBox { Text = title, StartPosition = FormStartPosition.CenterParent };
            box.InputBoxTxtBox.Text = initialText;
            box.ShowDialog();
            return box.Cancelled ? null : box.InputBoxTxtBox.Text;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { Hide(); }
        }
    }
}