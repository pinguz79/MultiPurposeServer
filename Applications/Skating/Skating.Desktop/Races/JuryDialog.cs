using System.Data.Common;

using Skating.Core;
using Skating.Desktop.Common;

namespace Skating.Desktop.Races
{
    internal sealed partial class JuryDialog : Form
    {
        private readonly IRaceStore _store;
        private readonly Race _race;

        public JuryDialog(IRegistryStore registry, IRaceStore store, Race race,
            string title, IEnumerable<string> choreographies)
        {
            _store = store;
            _race = race;
            InitializeComponent();
            Text = $"Giuria — {title}";
            foreach (var judge in registry.GetJudges())
            {
                judgesList.Items.Add(new NamedChoice(judge.Id, judge.Name), race.JudgeIds.Contains(judge.Id));
            }

            foreach (var name in choreographies)
            {
                choreographiesList.Items.Add(name);
            }
        }

        private void SaveButtonClick(object? sender, EventArgs e) => Save(false);

        private void DefaultButtonClick(object? sender, EventArgs e) => Save(true);

        private void Save(bool useDefault)
        {
            try
            {
                if (useDefault)
                {
                    _store.UseDefaultJury(_race.Key);
                }
                else
                {
                    _store.SetJury(_race.Key, judgesList.CheckedItems.OfType<NamedChoice>().Select(entry => entry.Id).ToArray());
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DbException or InvalidOperationException or ArgumentException)
            {
                MessageBox.Show(this, $"Salvataggio non completato.\r\n\r\n{exception.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
