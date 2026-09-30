namespace Finance.Api.Application
{
    public static class PedaggioFormula
    {
        public static string Create(Guid entrataId, Guid uscitaId) => $"[{GetDependency(entrataId, uscitaId)}]";

        public static string GetDependency(Guid entrataId, Guid uscitaId)
            => entrataId.CompareTo(uscitaId) < 0 ? $"Pedaggio:{entrataId:N}:{uscitaId:N}" : $"Pedaggio:{uscitaId:N}:{entrataId:N}";

        public static bool TryParse(string dependency, out Guid a, out Guid b)
        {
            a = b = Guid.Empty;
            string[] parts = dependency.Split(':');
            return parts.Length == 3 && string.Equals(parts[0], "Pedaggio", StringComparison.OrdinalIgnoreCase)
                && Guid.TryParseExact(parts[1], "N", out a) && Guid.TryParseExact(parts[2], "N", out b) && a != b;
        }
    }
}
