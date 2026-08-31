using System.Globalization;

using Finance.Desktop.Models;

namespace Finance.Desktop
{
    public partial class VoceRicorrenteDialog : Form
    {
        private static readonly CultureInfo ItalianCulture = CultureInfo.GetCultureInfo("it-IT");
        private readonly List<VoceRicorrenteDefinition> _definitions;
        private readonly IReadOnlyList<Categoria> _categories;
        private bool _displayNameEdited;

        public SaveVoceRicorrente Value { get; private set; }

        public VoceRicorrenteDialog(IReadOnlyList<Categoria> categories, VoceRicorrente? source = null)
        {
            _categories = categories;
            string name = source?.Name ?? string.Empty;
            _definitions = source is null
                ? [new VoceRicorrenteDefinition(null, string.Empty, 0, null, null, 0, null)]
                : [.. source.Definitions];
            Value = new SaveVoceRicorrente(name, []);
            InitializeComponent();
            nameTextBox.Text = name;
            nameTextBox.ReadOnly = source is not null;
            nameTextBox.TextChanged += NameTextBoxTextChanged;
            ConfigureGrid();
            RenderDefinitions();
        }

        private void AddButtonClick(object? sender, EventArgs e)
        {
            VoceRicorrenteDefinition source = _definitions[^1] with { Id = null, Index = 0 };
            using var dialog = new VoceRicorrenteDefinitionDialog(source, _categories);

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

            using var dialog = new VoceRicorrenteDefinitionDialog(_definitions[index], _categories);
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
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || _definitions.Count == 0)
            {
                MessageBox.Show(this, "Nome e almeno una definizione sono obbligatori.", "Finance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Value = new SaveVoceRicorrente(nameTextBox.Text, [.. _definitions.Select(definition => new SaveVoceRicorrenteDefinition(
                definition.Id,
                definition.DisplayName,
                definition.Value,
                definition.ValidFrom,
                definition.ValidTo,
                definition.Category?.Name))]);
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
                VoceRicorrenteDefinition definition = _definitions[index] with { Index = index };
                _definitions[index] = definition;
                definitionsGrid.Rows.Add(
                    definition.DisplayName,
                    definition.Value.ToString("N2", ItalianCulture) + " €",
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

        private int SelectedIndex() => definitionsGrid.CurrentRow?.Index ?? -1;
    }
}
