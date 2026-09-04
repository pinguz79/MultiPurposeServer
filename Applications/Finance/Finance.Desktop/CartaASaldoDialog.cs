using Finance.Desktop.Models;

namespace Finance.Desktop
{
    public partial class CartaASaldoDialog : Form
    {
        private readonly Conto _carta;

        public ConfigureCartaASaldo Value { get; private set; } = null!;

        public CartaASaldoDialog(Conto carta, IReadOnlyList<Conto> conti)
        {
            _carta = carta;
            InitializeComponent();
            contoAddebitoComboBox.Items.AddRange([.. conti.Where(conto => conto.Id != carta.Id).Cast<object>()]);
            contoAddebitoComboBox.DisplayMember = nameof(Conto.DisplayName);
            contoAddebitoComboBox.SelectedIndex = contoAddebitoComboBox.Items.Count == 0 ? -1 : 0;
            percentualeScopertoInput.Value = 10m;
            chiusuraCicloInput.Value = 21m;
            addebitoInput.Value = 5m;
            ripristinoPlafondInput.Value = 6m;
            validFromInput.Value = DateTime.Today;
            validToInput.Value = new DateTime(DateTime.Today.Year + 10, 12, 31);
        }

        private void SaveButtonClick(object? sender, EventArgs e)
        {
            if (plafondInput.Value <= 0 || contoAddebitoComboBox.SelectedItem is not Conto contoAddebito
                || validFromInput.Value.Date > validToInput.Value.Date)
            {
                MessageBox.Show(this, "Compilare plafond, conto di addebito e intervallo temporale.", "Finance", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (ripristinoPlafondInput.Value < addebitoInput.Value)
            {
                MessageBox.Show(this, "Il ripristino del plafond non può precedere l'addebito.", "Finance", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Value = new ConfigureCartaASaldo(
                plafondInput.Value,
                percentualeScopertoInput.Value / 100m,
                decimal.ToInt32(chiusuraCicloInput.Value),
                decimal.ToInt32(addebitoInput.Value),
                decimal.ToInt32(ripristinoPlafondInput.Value),
                contoAddebito.Name,
                DateOnly.FromDateTime(validFromInput.Value),
                DateOnly.FromDateTime(validToInput.Value));
            DialogResult = DialogResult.OK;
        }
    }
}
