using Finance.Desktop.Models;

namespace Finance.Desktop
{
    public partial class VoceRicorrenteDefinitionDialog : Form
    {
        private readonly Guid? _id;
        private readonly int _index;

        public VoceRicorrenteDefinition Definition { get; private set; }

        public VoceRicorrenteDefinitionDialog(VoceRicorrenteDefinition definition)
        {
            _id = definition.Id;
            _index = definition.Index;
            Definition = definition;
            InitializeComponent();
            displayNameTextBox.Text = definition.DisplayName;
            valueInput.Value = definition.Value;
            SetDate(validFromInput, definition.ValidFrom);
            SetDate(validToInput, definition.ValidTo);
        }

        private void SaveButtonClick(object? sender, EventArgs e)
        {
            string displayName = displayNameTextBox.Text.Trim();
            DateOnly? validFrom = GetDate(validFromInput);
            DateOnly? validTo = GetDate(validToInput);

            if (displayName.Length == 0 || validFrom > validTo)
            {
                MessageBox.Show(this, "Compilare il nome e verificare l'intervallo temporale.", "Finance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Definition = new VoceRicorrenteDefinition(_id, displayName, valueInput.Value, validFrom, validTo, _index);
            DialogResult = DialogResult.OK;
        }

        private static DateOnly? GetDate(DateTimePicker input) => input.Checked ? DateOnly.FromDateTime(input.Value) : null;

        private static void SetDate(DateTimePicker input, DateOnly? value)
        {
            input.Checked = value is not null;
            input.Value = (value ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue);
        }
    }
}
