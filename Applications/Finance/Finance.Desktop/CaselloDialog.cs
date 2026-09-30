namespace Finance.Desktop
{
    public partial class CaselloDialog : Form
    {
        public string StationName => nameInput.Text.Trim();

        public CaselloDialog() => InitializeComponent();

        private void NameChanged(object? sender, EventArgs e) => saveButton.Enabled = !string.IsNullOrWhiteSpace(nameInput.Text);
    }
}
