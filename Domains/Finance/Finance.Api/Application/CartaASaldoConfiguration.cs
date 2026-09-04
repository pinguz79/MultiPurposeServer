using Finance.Contracts.Requests;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public static class CartaASaldoConfiguration
    {
        public static readonly IReadOnlyList<ParametroCartaASaldoConfiguration> Parameters =
        [
            new("Plafond", "Plafond", TipoParametroConto.Importo),
            new("PercentualeScoperto", "Percentuale scoperto", TipoParametroConto.Percentuale),
            new("ChiusuraCiclo", "Chiusura ciclo", TipoParametroConto.Intero),
            new("Addebito", "Addebito", TipoParametroConto.Intero),
            new("RipristinoPlafond", "Ripristino plafond", TipoParametroConto.Intero),
        ];

        public static decimal GetValue(string name, ConfigureCartaASaldoRequest request) => name switch
        {
            "Plafond" => request.Plafond,
            "PercentualeScoperto" => request.PercentualeScoperto,
            "ChiusuraCiclo" => request.ChiusuraCiclo,
            "Addebito" => request.Addebito,
            "RipristinoPlafond" => request.RipristinoPlafond,
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };

        public static void Validate(ConfigureCartaASaldoRequest request)
        {
            if (request.Plafond <= 0 || decimal.Round(request.Plafond, 2, MidpointRounding.AwayFromZero) != request.Plafond)
            {
                throw new ArgumentException("Plafond must be positive and contain at most two decimal places.", nameof(request));
            }

            if (request.PercentualeScoperto is < 0 or > 1)
            {
                throw new ArgumentException("PercentualeScoperto must be between zero and one.", nameof(request));
            }

            if (request.ChiusuraCiclo is < 1 or > 31 || request.Addebito is < 1 or > 31 || request.RipristinoPlafond is < 1 or > 31)
            {
                throw new ArgumentException("Cycle days must be between 1 and 31.", nameof(request));
            }

            if (request.RipristinoPlafond < request.Addebito)
            {
                throw new ArgumentException("RipristinoPlafond cannot precede Addebito.", nameof(request));
            }

            if (request.ValidFrom > request.ValidTo)
            {
                throw new ArgumentException("ValidFrom cannot be later than ValidTo.", nameof(request));
            }
        }
    }
}
