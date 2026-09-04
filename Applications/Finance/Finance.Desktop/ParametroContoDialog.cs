using System.Globalization;

using Finance.Desktop.Models;

namespace Finance.Desktop
{
    public partial class ParametroContoDialog : Form
    {
        private static readonly CultureInfo ItalianCulture = CultureInfo.GetCultureInfo("it-IT");
        private readonly List<ParametroContoDefinition> _definitions;
        private bool _displayNameEdited;

        public string NameValue => nameTextBox.Text;
        public TipoParametroConto TypeValue => (TipoParametroConto)typeComboBox.SelectedItem!;
        public IReadOnlyList<SaveParametroContoDefinition> Definitions => [.. _definitions.Select(definition
            => new SaveParametroContoDefinition(definition.Id, definition.DisplayName, definition.Value, definition.ValidFrom, definition.ValidTo))];

        public ParametroContoDialog(ParametroConto? source = null)
        {
            _definitions = source is null
                ? [new ParametroContoDefinition(null, string.Empty, 0m, null, null, 0)]
                : [.. source.Definitions];
            InitializeComponent();
            typeComboBox.Items.AddRange([.. Enum.GetValues<TipoParametroConto>().Cast<object>()]);
            typeComboBox.SelectedItem = source?.Type ?? TipoParametroConto.Importo;
            typeComboBox.Enabled = source is null;
            nameTextBox.Text = source?.Name ?? string.Empty;
            nameTextBox.ReadOnly = source is not null;
            nameTextBox.TextChanged += NameTextBoxTextChanged;
            ConfigureGrid();
            RenderDefinitions();
        }

        private void AddButtonClick(object? sender, EventArgs e)
        {
            ParametroContoDefinition source = _definitions[^1] with { Id = null, Index = 0 };
            using var dialog = new ParametroContoDefinitionDialog(source, TypeValue);

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _definitions.Insert(0, dialog.Definition);
                RenderDefinitions();
            }
        }

        private void DeleteButtonClick(object? sender, EventArgs e)
        {
            int index = SelectedIndex();
            if (index >= 0 && _definitions.Count > 1)
            {
                _definitions.RemoveAt(index);
                RenderDefinitions();
            }
        }

        private void DownButtonClick(object? sender, EventArgs e) => MoveDefinition(1);

        private void EditButtonClick(object? sender, EventArgs e)
        {
            int index = SelectedIndex();
            if (index < 0)
            {
                return;
            }

            using var dialog = new ParametroContoDefinitionDialog(_definitions[index], TypeValue);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _definitions[index] = dialog.Definition;
                _displayNameEdited = true;
                RenderDefinitions(index);
            }
        }

        private void NameTextBoxTextChanged(object? sender, EventArgs e)
        {
            if (!_displayNameEdited && _definitions.Count > 0)
            {
                _definitions[0] = _definitions[0] with { DisplayName = nameTextBox.Text };
                RenderDefinitions();
            }
        }

        private void SaveButtonClick(object? sender, EventArgs e)
        {
            int permanentCount = _definitions.Count(definition => definition.ValidFrom is null && definition.ValidTo is null);
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || permanentCount != 1)
            {
                MessageBox.Show(this, "Nome e una sola definizione permanente sono obbligatori.", "Finance", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void UpButtonClick(object? sender, EventArgs e) => MoveDefinition(-1);

        private void ConfigureGrid()
        {
            definitionsGrid.Columns.Add("DisplayName", "Nome visualizzato");
            definitionsGrid.Columns.Add("Value", "Valore");
            definitionsGrid.Columns.Add("ValidFrom", "Dal");
            definitionsGrid.Columns.Add("ValidTo", "Al");
        }

        private void MoveDefinition(int offset)
        {
            int index = SelectedIndex();
            int target = index + offset;
            if (index < 0 || target < 0 || target >= _definitions.Count)
            {
                return;
            }

            (_definitions[index], _definitions[target]) = (_definitions[target], _definitions[index]);
            RenderDefinitions(target);
        }

        private void RenderDefinitions(int selectedIndex = 0)
        {
            definitionsGrid.Rows.Clear();
            for (var index = 0; index < _definitions.Count; index++)
            {
                ParametroContoDefinition definition = _definitions[index] with { Index = index };
                _definitions[index] = definition;
                definitionsGrid.Rows.Add(
                    definition.DisplayName,
                    FormatValue(definition.Value),
                    definition.ValidFrom?.ToString("dd/MM/yy", ItalianCulture) ?? "-∞",
                    definition.ValidTo?.ToString("dd/MM/yy", ItalianCulture) ?? "+∞");
            }

            if (definitionsGrid.Rows.Count > 0)
            {
                definitionsGrid.Rows[Math.Clamp(selectedIndex, 0, definitionsGrid.Rows.Count - 1)].Selected = true;
            }

            deleteButton.Enabled = _definitions.Count > 1;
            coveragePanel.SetDefinitions(_definitions);
        }

        private string FormatValue(decimal value) => TypeValue switch
        {
            TipoParametroConto.Importo => value.ToString("N2", ItalianCulture) + " €",
            TipoParametroConto.Percentuale => (value * 100m).ToString("N2", ItalianCulture) + " %",
            TipoParametroConto.Intero => value.ToString("N0", ItalianCulture),
            _ => value.ToString("N6", ItalianCulture),
        };

        private int SelectedIndex() => definitionsGrid.CurrentRow?.Index ?? -1;
    }
}
