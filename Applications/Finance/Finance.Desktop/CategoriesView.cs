using Finance.Desktop.Models;
using Finance.Desktop.Services;

namespace Finance.Desktop
{
    public partial class CategoriesView : UserControl
    {
        private readonly FinanceApiClient _client;
        private IReadOnlyList<Categoria> _categories = [];

        public CategoriesView(FinanceApiClient client)
        {
            _client = client;
            InitializeComponent();
            ConfigureGrid();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await RefreshCategories();
        }

        private async void AddButtonClick(object? sender, EventArgs e)
        {
            using var dialog = new CategoriaDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                await Execute(async () => await _client.CreateCategoria(dialog.Value));
            }
        }

        private async void CategoriesGridCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            Categoria categoria = _categories[e.RowIndex];
            switch (categoriesGrid.Columns[e.ColumnIndex].Name)
            {
                case "Edit":
                    using (var dialog = new CategoriaDialog(categoria))
                    {
                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            await Execute(async () => await _client.UpdateCategoria(categoria.Name, new UpdateCategoria(dialog.Value.DisplayName)));
                        }
                    }
                    return;
                case "Delete":
                    await Delete(categoria);
                    return;
            }
        }

        private void ConfigureGrid()
        {
            categoriesGrid.Columns.Add("Name", "Nome");
            categoriesGrid.Columns.Add("DisplayName", "Nome visualizzato");
            categoriesGrid.Columns.Add("UsageCount", "Utilizzi");
            categoriesGrid.Columns.Add(CreateActionColumn("Edit", "Modifica categoria", GridActionIcons.Edit));
            categoriesGrid.Columns.Add(CreateActionColumn("Delete", "Elimina categoria", GridActionIcons.Delete));
        }

        private async Task Delete(Categoria categoria)
        {
            try
            {
                CategoriaUsage? usage = await _client.DeleteCategoria(categoria.Name, false);
                if (usage is not null)
                {
                    string message = $"La categoria è utilizzata da {usage.VociRicorrenti} voci ricorrenti, {usage.Pianificazioni} pianificazioni e {usage.Movimenti} movimenti. "
                        + "Eliminandola, questi elementi rimarranno senza categoria. Continuare?";
                    if (MessageBox.Show(this, message, "Finance", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        return;
                    }

                    await _client.DeleteCategoria(categoria.Name, true);
                }

                await RefreshCategories();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task Execute(Func<Task> operation)
        {
            try
            {
                UseWaitCursor = true;
                await operation();
                await RefreshCategories();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Finance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async Task RefreshCategories()
        {
            _categories = await _client.GetCategorie();
            categoriesGrid.Rows.Clear();

            foreach (Categoria categoria in _categories)
            {
                categoriesGrid.Rows.Add(categoria.Name, categoria.DisplayName, categoria.UsageCount);
            }
        }

        private static DataGridViewImageColumn CreateActionColumn(string name, string toolTipText, Image icon) => new()
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            HeaderText = string.Empty,
            Image = icon,
            ImageLayout = DataGridViewImageCellLayout.Normal,
            Name = name,
            ToolTipText = toolTipText,
            Width = 42,
        };
    }
}
