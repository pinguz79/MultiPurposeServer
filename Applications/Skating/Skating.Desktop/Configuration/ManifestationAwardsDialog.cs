using System.Data.Common;

using Skating.Core;
using Skating.Desktop.Common;

namespace Skating.Desktop.Configuration
{
    internal sealed class ManifestationAwardsDialog : PopupForm
    {
        private readonly IIntermediateAwardStore _store;
        private readonly Func<string, bool> _confirm;
        private readonly Action<string> _alert;
        private readonly ComboBox _aggregation = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };

        public ManifestationAwardsDialog(IIntermediateAwardStore store, Func<string, bool>? confirm = null,
            Action<string>? alert = null)
        {
            _store = store;
            _confirm = confirm ?? (message => MessageBox.Show(this, message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes);
            _alert = alert ?? (message => MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning));
            Text = "Premi della manifestazione";
            ClientSize = new Size(620, 260);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            var layout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(16) };
            layout.Controls.Add(new Label { Text = "Raggruppamento dei vincitori per i premi intermedi", AutoSize = true });
            _aggregation.Items.AddRange(["Per disciplina", "Per categoria"]);
            _aggregation.SelectedIndex = store.GetAggregation() is AwardAggregation mode ? (int)mode - 1 : -1;
            layout.Controls.Add(_aggregation);
            layout.Controls.Add(new Label
            {
                AutoSize = true, MaximumSize = new Size(575, 0), Margin = new Padding(3, 12, 3, 12),
                Text = "La scelta è salvata in questo archivio e si applica subito.\r\nIntermedi e assoluto Open saranno ricalcolati; gare, voti e percorso PRO resteranno invariati.",
            });
            var buttons = new FlowLayoutPanel { AutoSize = true };
            var save = new Button { Text = "Applica", AutoSize = true };
            var cancel = new Button { Text = "Annulla", AutoSize = true, DialogResult = DialogResult.Cancel };
            save.Click += (_, _) => Save();
            buttons.Controls.AddRange([save, cancel]);
            layout.Controls.Add(buttons);
            Controls.Add(layout);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private void Save()
        {
            if (_aggregation.SelectedIndex < 0)
            {
                _alert("Scegliere un criterio.");
                return;
            }

            try
            {
                var mode = (AwardAggregation)(_aggregation.SelectedIndex + 1);
                var revoke = mode != _store.GetAggregation() && _store.HasValidatedAwards();
                if (revoke && !_confirm("Il cambio criterio revoca le convalide dei premi intermedi e dell'assoluto Open, che dovranno essere ricontrollati. Continuare?"))
                {
                    return;
                }

                _store.Configure(mode, revoke);
                DialogResult = DialogResult.OK;
            }
            catch (Exception exception) when (exception is DbException or IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
            {
                _alert(exception.Message);
            }
        }
    }
}
