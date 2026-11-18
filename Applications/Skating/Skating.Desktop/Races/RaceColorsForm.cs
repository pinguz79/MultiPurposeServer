using System.Data.Common;

using Skating.Core;
using Skating.Desktop.Common;

namespace Skating.Desktop.Races
{
    internal sealed class RaceColorsForm : PopupForm
    {
        private readonly IRaceStore _store;
        private readonly IRegistryStore _registry;
        private readonly DataGridView _grid = new()
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false,
            MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        };
        private readonly ComboBox _colors = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        private readonly Label _preview = new() { Text = "Anteprima testo", Width = 150, Height = 28, TextAlign = ContentAlignment.MiddleCenter };

        public RaceColorsForm(IRaceStore store, IRegistryStore registry)
        {
            _store = store;
            _registry = registry;
            Text = "Colori e legenda delle gare";
            ClientSize = new Size(900, 550);
            MinimumSize = new Size(750, 420);
            StartPosition = FormStartPosition.CenterParent;
            _grid.Columns.Add("Race", "Disciplina / Categoria / Livello");
            _grid.Columns.Add("Color", "Colore");
            _grid.Columns[0].FillWeight = 75;
            _grid.Columns[1].FillWeight = 25;
            _grid.SelectionChanged += (_, _) => LoadChoices();
            _colors.SelectedIndexChanged += (_, _) => _preview.BackColor = RaceColorAppearance.Resolve(_colors.SelectedItem as string);
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            toolbar.Controls.Add(_colors);
            toolbar.Controls.Add(_preview);
            var save = new Button { Text = "Applica colore", AutoSize = true };
            save.Click += (_, _) => Save();
            toolbar.Controls.Add(save);
            Controls.Add(_grid);
            Controls.Add(toolbar);
            Controls.Add(new Label
            {
                Dock = DockStyle.Top, Height = 52, Padding = new Padding(8),
                Text = "Selezionare una gara e un colore. Sono disponibili solo i colori liberi e quello già assegnato.\r\nSenza colore mantiene l'aspetto neutro. A palette esaurita occorre liberare un colore.",
            });
            RefreshRows();
        }

        private void RefreshRows()
        {
            var selected = _grid.CurrentRow?.Tag as RaceKey;
            var colors = _store.GetColors();
            var disciplines = _registry.GetDisciplines().ToDictionary(item => item.Id, item => item.Name);
            var categories = _registry.GetCategories().ToDictionary(item => item.Id, item => item.Name);
            var levels = _registry.GetLevels().ToDictionary(item => item.Id, item => item.Name);
            _grid.Rows.Clear();
            foreach (var race in _store.GetRaces().OrderBy(race => disciplines[race.Key.DisciplineId]).ThenBy(race => categories[race.Key.CategoryId]).ThenBy(race => levels[race.Key.LevelId]))
            {
                var color = colors.GetValueOrDefault(race.Key);
                var row = _grid.Rows[_grid.Rows.Add($"{disciplines[race.Key.DisciplineId]} / {categories[race.Key.CategoryId]} / {levels[race.Key.LevelId]}", color ?? "Senza colore")];
                row.Tag = race.Key;
                row.Cells[1].Style.BackColor = RaceColorAppearance.Resolve(color);
                row.Cells[1].Style.SelectionBackColor = row.Cells[1].Style.BackColor;
                row.Cells[1].Style.SelectionForeColor = Color.Black;
                if (race.Key == selected)
                {
                    _grid.CurrentCell = row.Cells[0];
                }
            }

            LoadChoices();
        }

        private void LoadChoices()
        {
            _colors.Items.Clear();
            _colors.Items.Add("Senza colore");
            var colors = _store.GetColors();
            var current = _grid.CurrentRow?.Tag is RaceKey key ? colors.GetValueOrDefault(key) : null;
            foreach (var color in RaceColor.Palette.Where(color => color.Name == current || !colors.Values.Contains(color.Name)).OrderBy(color => color.Name))
            {
                _colors.Items.Add(color.Name);
            }

            _colors.SelectedItem = current ?? "Senza colore";
        }

        private void Save()
        {
            if (_grid.CurrentRow?.Tag is not RaceKey key)
            {
                return;
            }

            try
            {
                _store.SetColor(key, _colors.SelectedIndex == 0 ? null : _colors.SelectedItem as string);
                RefreshRows();
            }
            catch (Exception exception) when (exception is DbException or InvalidOperationException or ArgumentException or IOException)
            {
                MessageBox.Show(this, exception.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
