using Finance.Desktop.Models;

namespace Finance.Desktop
{
    public partial class ParametroContoDefinitionDialog : Form
    {
        private readonly Guid? _id;
        private readonly int _index;
        private readonly TipoParametroConto _type;

        public ParametroContoDefinition Definition { get; private set; }

        public ParametroContoDefinitionDialog(ParametroContoDefinition definition, TipoParametroConto type)
        {
            _id = definition.Id;
            _index = definition.Index;
            _type = type;
            Definition = definition;
            InitializeComponent();
            ConfigureValueInput();
            displayNameTextBox.Text = definition.DisplayName;
            valueInput.Value = ToDisplayValue(definition.Value);
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
                MessageBox.Show(this, "Compilare il nome e verificare l'intervallo temporale.", "Finance", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Definition = new ParametroContoDefinition(_id, displayName, FromDisplayValue(valueInput.Value), validFrom, validTo, _index);
            DialogResult = DialogResult.OK;
        }

        private void ConfigureValueInput()
        {
            valueInput.DecimalPlaces = _type switch
            {
                TipoParametroConto.Intero => 0,
                TipoParametroConto.Decimale => 6,
                _ => 2,
            };
            valueSuffixLabel.Text = _type switch
            {
                TipoParametroConto.Importo => "€",
                TipoParametroConto.Percentuale => "%",
                _ => string.Empty,
            };
        }

        private decimal FromDisplayValue(decimal value) => _type == TipoParametroConto.Percentuale ? value / 100m : value;

        private decimal ToDisplayValue(decimal value) => _type == TipoParametroConto.Percentuale ? value * 100m : value;

        private static DateOnly? GetDate(DateTimePicker input) => input.Checked ? DateOnly.FromDateTime(input.Value) : null;

        private static void SetDate(DateTimePicker input, DateOnly? value)
        {
            input.Checked = value is not null;
            input.Value = (value ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue);
        }
    }
}
