using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public class RevolvingIndicatorsService(IParametroContoService parameters) : IRevolvingIndicatorsService
    {
        public async Task<RevolvingIndicators?> Get(Conto conto, decimal balance, DateOnly date)
        {
            if (await parameters.Resolve(conto.Id, "QuotaRata", date) is null || await parameters.Resolve(conto.Id, "RipristinoPlafond", date) is not null)
            {
                return null;
            }

            ParametroConto? plafond = await parameters.Resolve(conto.Id, "Plafond", date);
            ParametroConto? percentage = await parameters.Resolve(conto.Id, "PercentualeScoperto", date);
            if (plafond is null || plafond.Type != TipoParametroConto.Importo || plafond.Value <= 0m
                || percentage is null || percentage.Type != TipoParametroConto.Percentuale || percentage.Value is < 0m or > 1m)
            {
                throw new InvalidOperationException("Il profilo revolving richiede plafond e percentuale di scoperto validi alla data richiesta.");
            }

            decimal overdraft = decimal.Round(plafond.Value * percentage.Value, 2, MidpointRounding.AwayFromZero);
            return new RevolvingIndicators(plafond.Value, overdraft, plafond.Value - balance, plafond.Value + overdraft - balance);
        }
    }
}
