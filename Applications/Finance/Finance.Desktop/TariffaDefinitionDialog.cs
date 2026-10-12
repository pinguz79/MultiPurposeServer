using System.Globalization;

using Finance.Desktop.Models;

namespace Finance.Desktop
{
    public partial class TariffaDefinitionDialog : Form
    {
        private readonly bool _isBase;

        public SaveTariffaDefinition Definition { get; private set; }

        public TariffaDefinitionDialog(SaveTariffaDefinition definition, bool isBase)
        {
            _isBase = isBase;
            Definition = definition;
            InitializeComponent();
            Text = isBase ? "Tariffa base" : "Eccezione temporale";
            valueInput.Value = definition.Value;
            fromInput.Value = (definition.ValidFrom ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue);
            toInput.Value = (definition.ValidTo ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue);
            fromInput.Checked = definition.ValidFrom.HasValue;
            toInput.Checked = definition.ValidTo.HasValue;
            fromInput.Enabled = toInput.Enabled = !isBase;
        }

        private void SaveClick(object? sender, EventArgs e)
        {
            DateOnly? from = !_isBase && fromInput.Checked ? DateOnly.FromDateTime(fromInput.Value) : null;
            DateOnly? to = !_isBase && toInput.Checked ? DateOnly.FromDateTime(toInput.Value) : null;
            if ((!_isBase && from is null && to is null) || from > to)
            {
                MessageBox.Show(this, "Specificare almeno un limite per l'eccezione e verificare che Dal non superi Al.", "Finance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Definition = new SaveTariffaDefinition(valueInput.Value, from, to);
            DialogResult = DialogResult.OK;
        }

        private void ValueEnter(object? sender, EventArgs e)
        {
            BeginInvoke(() =>
            {
                if (!IsDisposed && valueInput.ContainsFocus)
                {
                    valueInput.Select(0, valueInput.Text.Length);
                }
            });
        }

        private void ValueKeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.')
            {
                e.KeyChar = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            }
        }
    }
}
