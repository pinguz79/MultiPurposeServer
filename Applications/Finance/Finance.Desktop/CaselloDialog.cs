namespace Finance.Desktop
{
    public partial class CaselloDialog : Form
    {
        public string StationName => nameInput.Text.Trim();

        public CaselloDialog(string? name = null)
        {
            InitializeComponent();
            nameInput.Text = name ?? string.Empty;
            Text = name is null ? "Nuova stazione" : "Rinomina stazione";
        }

        private void NameChanged(object? sender, EventArgs e) => saveButton.Enabled = !string.IsNullOrWhiteSpace(nameInput.Text);
    }
}
