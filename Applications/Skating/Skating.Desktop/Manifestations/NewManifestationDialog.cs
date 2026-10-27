namespace Skating.Desktop.Manifestations
{
    internal sealed partial class NewManifestationDialog : Form
    {
        public string ManifestationName => nameTextBox.Text.Trim();

        public NewManifestationDialog()
        {
            InitializeComponent();
        }

        private void CreateButtonClick(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ManifestationName))
            {
                MessageBox.Show(this, "Inserire il nome della manifestazione.", "Nuova manifestazione", MessageBoxButtons.OK, MessageBoxIcon.Information);
                nameTextBox.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
