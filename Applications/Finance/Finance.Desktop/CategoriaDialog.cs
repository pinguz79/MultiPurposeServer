using Finance.Desktop.Models;

namespace Finance.Desktop
{
    public partial class CategoriaDialog : Form
    {
        private bool _displayNameEdited;

        public SaveCategoria Value { get; private set; }

        public CategoriaDialog(Categoria? source = null)
        {
            Value = new SaveCategoria(source?.Name ?? string.Empty, source?.DisplayName ?? string.Empty);
            InitializeComponent();
            nameTextBox.Text = Value.Name;
            nameTextBox.ReadOnly = source is not null;
            displayNameTextBox.Text = Value.DisplayName;
        }

        private void DisplayNameTextBoxTextChanged(object? sender, EventArgs e) => _displayNameEdited = true;

        private void NameTextBoxTextChanged(object? sender, EventArgs e)
        {
            if (!_displayNameEdited)
            {
                displayNameTextBox.Text = nameTextBox.Text;
                _displayNameEdited = false;
            }
        }

        private void SaveButtonClick(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || string.IsNullOrWhiteSpace(displayNameTextBox.Text))
            {
                MessageBox.Show(this, "Nome e nome visualizzato sono obbligatori.", "Finance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Value = new SaveCategoria(nameTextBox.Text, displayNameTextBox.Text);
            DialogResult = DialogResult.OK;
        }
    }
}
