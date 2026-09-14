using Finance.Contracts.Requests;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public static class CartaRevolvingConfiguration
    {
        public static IReadOnlyList<ParametroCartaRevolvingConfiguration> GetParameters(ConfigureCartaRevolvingRequest request) => [
            new("Plafond", "Plafond", TipoParametroConto.Importo, request.Plafond),
            new("PercentualeScoperto", "Percentuale scoperto", TipoParametroConto.Percentuale, request.PercentualeScoperto),
            new("QuotaRata", "Quota rata", TipoParametroConto.Percentuale, request.QuotaRata),
            new("RataMinima", "Rata minima", TipoParametroConto.Importo, request.RataMinima),
            new("Tan", "TAN", TipoParametroConto.Percentuale, request.Tan),
            new("Bollo", "Bollo", TipoParametroConto.Importo, request.Bollo),
            new("SogliaBollo", "Soglia bollo", TipoParametroConto.Importo, request.SogliaBollo),
            new("ChiusuraCiclo", "Chiusura ciclo", TipoParametroConto.Intero, request.ChiusuraCiclo),
            new("Addebito", "Addebito e rimborso", TipoParametroConto.Intero, request.Addebito),
        ];

        public static void Validate(ConfigureCartaRevolvingRequest request)
        {
            if (request.Plafond <= 0m || new[] { request.Plafond, request.RataMinima, request.Bollo, request.SogliaBollo }.Any(value => value < 0m || decimal.Round(value, 2) != value))
            {
                throw new ArgumentException("Il plafond deve essere positivo; gli importi devono essere non negativi e avere al massimo due decimali.", nameof(request));
            }

            if (new[] { request.PercentualeScoperto, request.QuotaRata, request.Tan }.Any(value => value is < 0m or > 1m))
            {
                throw new ArgumentException("Le percentuali devono essere coefficienti compresi fra zero e uno.", nameof(request));
            }

            if (request.ChiusuraCiclo is < 1 or > 31 || request.Addebito is < 1 or > 31 || request.Addebito <= request.ChiusuraCiclo)
            {
                throw new ArgumentException("L'addebito deve seguire la chiusura nello stesso mese, con giorni compresi fra 1 e 31.", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.ContoAddebitoName) || request.ValidFrom > request.ValidTo)
            {
                throw new ArgumentException("Indicare un conto di addebito e un periodo valido.", nameof(request));
            }

            DateOnly month = new(request.ValidFrom.Year, request.ValidFrom.Month, 1);
            while (month <= request.ValidTo)
            {
                int lastDay = DateTime.DaysInMonth(month.Year, month.Month);
                if (Math.Min(request.ChiusuraCiclo, lastDay) == Math.Min(request.Addebito, lastDay))
                {
                    throw new ArgumentException("Chiusura e addebito coinciderebbero in un mese del periodo richiesto.", nameof(request));
                }

                if (month.Year == 9999 && month.Month == 12)
                {
                    break;
                }
                month = month.AddMonths(1);
            }
        }
    }
}
