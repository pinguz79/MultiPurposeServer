using System.Globalization;

namespace Skating.Core
{
    /// <summary>Conversione dei voti 0-10 con un decimale. Vuoto significa mancante.</summary>
    public static class ScoreValue
    {
        public static int? Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var normalized = text.Trim().Replace(',', '.');
            var parts = normalized.Split('.');
            if (parts.Length > 2 || parts.Any(part => part.Length == 0 || part.Any(character => character is < '0' or > '9')) || parts.Length == 2 && parts[1].Length > 1
                || !decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value) || value is < 0 or > 10)
            {
                throw new ArgumentException("Inserire un voto da 0 a 10 con al massimo un decimale, oppure lasciare vuoto.", nameof(text));
            }

            return (int)(value * 10);
        }

        public static string Format(int? tenths) => tenths is null ? string.Empty : (tenths.Value / 10m).ToString("0.0", CultureInfo.GetCultureInfo("it-IT"));
    }
}
