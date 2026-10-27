using System.Text.Json;

using Skating.Core;

namespace Skating.Desktop.Configuration
{
    internal sealed class ApplicationSettingsDialog : Form
    {
        private readonly string _path;
        private readonly Action<string> _alert;
        private readonly ComboBox _aggregation = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };

        public ApplicationSettingsDialog(string path, AwardAggregation? activeAggregation,
            Action<string>? alert = null)
        {
            _path = path;
            _alert = alert ?? (message => MessageBox.Show(this, message, "Impostazioni applicazione", MessageBoxButtons.OK, MessageBoxIcon.Information));
            var saved = File.Exists(path) ? AwardOptions.Load(path).IntermediateAwardAggregation : null;
            Text = "Impostazioni applicazione";
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(640, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(16) };
            layout.Controls.Add(new Label { AutoSize = true, Text = "Aggregazione dei premi intermedi" });
            _aggregation.Items.AddRange(["Non configurata", "Per disciplina", "Per categoria"]);
            _aggregation.SelectedIndex = saved is null ? 0 : (int)saved;
            layout.Controls.Add(_aggregation);
            layout.Controls.Add(new Label { AutoSize = true, Margin = new Padding(3, 12, 3, 8), Text = $"Configurazione attiva: {Describe(activeAggregation)}" });
            layout.Controls.Add(new Label
            {
                AutoSize = true, MaximumSize = new Size(600, 0),
                Text = "Riavviare l'applicazione per rendere effettive le modifiche.\r\n\r\nLe manifestazioni già avviate conservano il proprio criterio: cambiare questa impostazione non modifica le classifiche esistenti.",
            });
            var buttons = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 18, 0, 0) };
            var save = new Button { Text = "Salva", AutoSize = true };
            var cancel = new Button { Text = "Annulla", AutoSize = true, DialogResult = DialogResult.Cancel };
            save.Click += (_, _) => Save();
            buttons.Controls.AddRange([save, cancel]);
            layout.Controls.Add(buttons);
            Controls.Add(layout);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private static string Describe(AwardAggregation? aggregation) => aggregation switch
        {
            AwardAggregation.Discipline => "per disciplina",
            AwardAggregation.Category => "per categoria",
            _ => "non configurata",
        };

        private void Save()
        {
            try
            {
                new AwardOptions(_aggregation.SelectedIndex == 0 ? null : (AwardAggregation)_aggregation.SelectedIndex).Save(_path);
                _alert("Impostazioni salvate. Riavviare l'applicazione per rendere effettive le modifiche.");
                DialogResult = DialogResult.OK;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
            {
                _alert($"Impossibile salvare le impostazioni. Il file precedente non è stato sostituito.\r\n\r\n{exception.Message}");
            }
        }
    }
}
