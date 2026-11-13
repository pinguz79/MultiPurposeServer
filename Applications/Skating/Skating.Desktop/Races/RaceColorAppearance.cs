using Skating.Core;

namespace Skating.Desktop.Races
{
    internal static class RaceColorAppearance
    {
        internal static Color Resolve(string? name)
        {
            var color = RaceColor.Palette.SingleOrDefault(color => color.Name == name);
            return color is null ? Color.White : Color.FromArgb(255, (color.Rgb >> 16) & 255, (color.Rgb >> 8) & 255, color.Rgb & 255);
        }

        internal static void Apply(DataGridViewCell cell, RaceKey key, IReadOnlyDictionary<RaceKey, string> colors)
        {
            if (colors.TryGetValue(key, out var name))
            {
                cell.Style.BackColor = Resolve(name);
                cell.Style.ForeColor = Color.Black;
                cell.ToolTipText = $"Colore gara: {name}";
            }
        }
    }
}
