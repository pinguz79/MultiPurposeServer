using Skating.Core;

namespace Skating.Desktop.Rankings
{
    internal sealed class RankingScoreCard : UserControl
    {
        private readonly Label _heading = new() { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8), AutoEllipsis = true };
        private readonly FlowLayoutPanel _judges = new() { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = false, Padding = new Padding(8) };

        public RankingScoreCard()
        {
            BackColor = Color.FromArgb(244, 246, 249);
            Controls.Add(_judges);
            Controls.Add(_heading);
            Clear();
        }

        public void Clear()
        {
            while (_judges.Controls.Count > 0)
            {
                _judges.Controls[0].Dispose();
            }

            _heading.Text = "Selezionare una coreografia per visualizzare la scheda voti.";
        }

        public void ShowEntry(RankingEntry entry, string name, IRegistryStore registry)
        {
            Clear();
            var total = entry.TotalTenths is int value ? ScoreValue.Format(value) : "—";
            _heading.Text = $"{name} · Totale in questa classifica: {total} · Sola lettura";
            var judges = registry.GetJudges().ToDictionary(judge => judge.Id, judge => judge.Name);
            var criteria = registry.GetCriteria();
            foreach (var group in entry.Scores.GroupBy(score => score.JudgeId).OrderBy(group => judges[group.Key]))
            {
                var reason = entry.Status == PerformanceStatus.Disqualified ? "Squalificata"
                    : entry.Status == PerformanceStatus.NotPresented ? "Non presentata"
                    : !entry.IsComplete ? "Scheda incompleta"
                    : entry.ExcludedJudgeIds.Contains(group.Key) ? "Giudice escluso da questa classifica" : "Conteggiato";
                var included = reason == "Conteggiato";
                var card = new FlowLayoutPanel
                {
                    Width = 200, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.TopDown, WrapContents = false,
                    MinimumSize = new Size(200, 0), MaximumSize = new Size(200, 0), Padding = new Padding(10), Margin = new Padding(0, 0, 10, 0),
                    BackColor = included ? Color.White : Color.FromArgb(232, 234, 237), ForeColor = included ? Color.FromArgb(30, 43, 57) : Color.DimGray,
                };
                AddText(card, judges[group.Key]);
                AddText(card, reason);
                foreach (var criterion in criteria)
                {
                    var score = group.SingleOrDefault(score => score.CriterionId == criterion.Id);
                    AddText(card, $"{criterion.Name}: {(score is null ? "—" : ScoreValue.Format(score.Tenths))}");
                }

                AddText(card, $"{(entry.IsComplete ? "Totale giudice" : "Parziale inserito")}: {ScoreValue.Format(group.Sum(score => score.Tenths))}");
                _judges.Controls.Add(card);
            }

            if (entry.Scores.Count == 0)
            {
                AddText(_judges, "Nessun voto inserito.");
            }
        }

        private static void AddText(Control parent, string text) => parent.Controls.Add(new Label { Text = text, AutoSize = true, MaximumSize = new Size(180, 0), Margin = new Padding(0, 0, 0, 8) });
    }
}
