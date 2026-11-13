using Skating.Core;

namespace Skating.Desktop.Rankings
{
    internal static class PodiumAppearance
    {
        internal static ComboBox CreateSelector()
        {
            var selector = new ComboBox { Name = "rankingViewSelector", DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
            selector.Items.AddRange(["Classifica completa", "Solo podio"]);
            selector.SelectedIndex = 0;
            return selector;
        }

        internal static bool IsOnPodium(RankingEntry entry) => entry.Status == PerformanceStatus.Active && entry.Position is >= 1 and <= 3;

        internal static void Apply(DataGridViewRow row, RankingEntry entry)
        {
            if (!IsOnPodium(entry))
            {
                return;
            }

            var color = entry.Position switch { 1 => Color.FromArgb(255, 220, 110), 2 => Color.FromArgb(215, 223, 232), _ => Color.FromArgb(225, 175, 130) };
            var cell = row.Cells[0];
            cell.Style.BackColor = color;
            cell.Style.ForeColor = Color.Black;
            cell.Style.SelectionBackColor = color;
            cell.Style.SelectionForeColor = Color.Black;
            cell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            cell.ToolTipText = $"{entry.Position}° posto{(entry.IsTied ? " — parimerito" : "")}";
            row.Height = Math.Max(row.Height, 30);
        }
    }
}
