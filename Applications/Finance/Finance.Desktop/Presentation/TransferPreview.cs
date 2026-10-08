using Finance.Desktop.Models;

namespace Finance.Desktop.Presentation
{
    internal static class TransferPreview
    {
        // La convenzione dipende dai parametri validi alla data scelta, non dal saldo corrente.
        internal static bool IsDebtAccount(IReadOnlyList<ParametroConto> parameters, DateOnly date)
        {
            bool Has(string name) => parameters.Any(parameter => string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase)
                && parameter.Definitions.Any(definition => (definition.ValidFrom is null || definition.ValidFrom <= date)
                    && (definition.ValidTo is null || definition.ValidTo >= date)));

            if (!Has("Plafond") || !Has("PercentualeScoperto"))
            {
                return false;
            }
            return Has("RipristinoPlafond") ? Has("ChiusuraCiclo") && Has("Addebito") : Has("QuotaRata") || Has("Rata");
        }
    }
}
