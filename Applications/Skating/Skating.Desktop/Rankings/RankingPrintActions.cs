using System.ComponentModel;
using System.Drawing.Printing;

namespace Skating.Desktop.Rankings
{
    internal static class RankingPrintActions
    {
        internal static void Add(FlowLayoutPanel toolbar, Form owner, DataGridView grid, Func<string> title, Func<string> status, ComboBox view)
        {
            foreach (var pdf in new[] { false, true })
            {
                var button = new Button { Text = pdf ? "Esporta PDF..." : "Stampa...", AutoSize = true };
                button.Click += (_, _) => Print(owner, grid, title(), status() + " · " + view.Text, pdf);
                toolbar.Controls.Add(button);
            }
        }

        private static void Print(Form owner, DataGridView grid, string title, string status, bool pdf)
        {
            try
            {
                var columns = grid.Columns.Cast<DataGridViewColumn>().Where(column => column.Visible).OrderBy(column => column.DisplayIndex).ToArray();
                var rows = grid.Rows.Cast<DataGridViewRow>().Where(row => row.Visible && !row.IsNewRow)
                    .Select(row => columns.Select(column => Convert.ToString(row.Cells[column.Index].FormattedValue) ?? "").ToArray()).ToArray();
                using var document = new RankingPrintDocument(title, status, [.. columns.Select(column => column.HeaderText)], rows, [.. columns.Select(column => (float)column.Width)]);
                if (pdf)
                {
                    const string printer = "Microsoft Print to PDF";
                    if (!PrinterSettings.InstalledPrinters.Cast<string>().Contains(printer, StringComparer.OrdinalIgnoreCase))
                    {
                        MessageBox.Show(owner, "Microsoft Print to PDF non è disponibile. Abilitarla in Windows oppure usare Stampa con un'altra stampante PDF.", "Esportazione PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    using var save = new SaveFileDialog { Filter = "Documento PDF (*.pdf)|*.pdf", DefaultExt = "pdf", AddExtension = true, FileName = "Classifica.pdf", OverwritePrompt = true };
                    if (save.ShowDialog(owner) != DialogResult.OK)
                    {
                        return;
                    }

                    document.PrinterSettings.PrinterName = printer;
                    document.PrinterSettings.PrintToFile = true;
                    document.PrinterSettings.PrintFileName = save.FileName;
                }
                else
                {
                    using var dialog = new PrintDialog { Document = document, UseEXDialog = true, AllowSomePages = false };
                    if (dialog.ShowDialog(owner) != DialogResult.OK)
                    {
                        return;
                    }
                }

                document.Print();
            }
            catch (Exception exception) when (exception is InvalidOperationException or IOException or UnauthorizedAccessException or Win32Exception or InvalidPrinterException)
            {
                MessageBox.Show(owner, $"Stampa non completata: {exception.Message}", "Stampa classifica", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
