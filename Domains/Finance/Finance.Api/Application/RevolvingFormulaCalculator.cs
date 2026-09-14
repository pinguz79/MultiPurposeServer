using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public class RevolvingFormulaCalculator(
        IContoRepository contoRepository,
        IMovimentoRepository movimentoRepository,
        IParametroContoRepository parametroContoRepository)
    {
        public async Task<decimal> Calculate(string dependency, DateOnly date, Func<Movimento, Task<decimal>> evaluateMovement)
        {
            string[] parts = dependency.Split('.');
            Conto conto = await contoRepository.GetByName(parts[0]) ?? throw new KeyNotFoundException($"Conto '{parts[0]}' non trovato.");
            IReadOnlyList<ParametroConto> parameters = await parametroContoRepository.GetAll(conto.Id);
            int closingDay = GetClosingDay(parameters, date);
            DateOnly closingDate = FormulaEvaluator.GetClosingDate(date, closingDay);
            IReadOnlyList<Movimento> movements = await movimentoRepository.GetByContoThrough(conto.Id, closingDate);

            return parts[1] switch
            {
                FormulaResolver.InteressiCiclo => await CalculateInterest(conto.InitialBalance, movements, parameters, closingDate, evaluateMovement),
                FormulaResolver.BolloCiclo => RevolvingCalculator.CalculateStampDuty(
                    await CalculateBalance(conto.InitialBalance, movements.Where(movement => movement.Date != closingDate || movement.Natura != NaturaMovimento.Bollo), evaluateMovement),
                    GetParameter(parameters, "Bollo", closingDate), GetParameter(parameters, "SogliaBollo", closingDate)),
                FormulaResolver.RataUltimoCicloChiuso => RevolvingCalculator.CalculatePayment(await CalculateBalance(conto.InitialBalance, movements, evaluateMovement),
                    GetParameter(parameters, "Plafond", closingDate), GetParameter(parameters, "QuotaRata", closingDate), GetParameter(parameters, "RataMinima", closingDate)),
                _ => throw new ArgumentException($"Proprietà revolving '{dependency}' non riconosciuta.", nameof(dependency)),
            };
        }

        private static async Task<decimal> CalculateInterest(decimal initialBalance, IReadOnlyList<Movimento> movements, IReadOnlyList<ParametroConto> parameters,
            DateOnly closingDate, Func<Movimento, Task<decimal>> evaluateMovement)
        {
            DateOnly previousMonth = new DateOnly(closingDate.Year, closingDate.Month, 1).AddMonths(-1);
            DateOnly previousMonthEnd = previousMonth.AddMonths(1).AddDays(-1);
            DateOnly start = FormulaEvaluator.GetClosingDate(previousMonthEnd, GetClosingDay(parameters, previousMonthEnd)).AddDays(1);
            decimal capital = initialBalance;
            decimal charges = 0m;
            var capitalDays = new List<RevolvingCapitalDay>();
            DateOnly nextDay = start;

            // A parità di data gli oneri precedono il rimborso; il capitale è sempre quello di fine giornata.
            foreach (IGrouping<DateOnly, Movimento> day in movements.OrderBy(movement => movement.Date).GroupBy(movement => movement.Date))
            {
                while (nextDay < day.Key && nextDay <= closingDate)
                {
                    capitalDays.Add(new RevolvingCapitalDay(nextDay, capital, GetParameter(parameters, "Tan", nextDay)));
                    nextDay = nextDay.AddDays(1);
                }

                foreach (Movimento movement in day.OrderBy(movement => movement.Natura == NaturaMovimento.Rimborso ? 1 : 0).ThenBy(movement => movement.Id))
                {
                    // Gli oneri della chiusura corrente non concorrono alla propria base interessi.
                    if (movement.Date == closingDate && movement.Natura is NaturaMovimento.Interessi or NaturaMovimento.Bollo)
                    {
                        continue;
                    }

                    decimal amount = await evaluateMovement(movement);
                    switch (movement.Natura)
                    {
                        case NaturaMovimento.Ordinario:
                            capital += amount;
                            break;
                        case NaturaMovimento.Interessi:
                        case NaturaMovimento.Bollo:
                            charges += amount;
                            if (charges < 0m)
                            {
                                throw new InvalidOperationException($"Il movimento '{movement.Id}' porta gli oneri residui sotto zero.");
                            }
                            break;
                        case NaturaMovimento.Rimborso:
                            if (amount > 0m)
                            {
                                throw new InvalidOperationException($"Il rimborso '{movement.Id}' deve avere importo negativo o nullo.");
                            }
                            RevolvingPaymentAllocation allocation = RevolvingCalculator.AllocatePayment(-amount, charges);
                            charges -= allocation.Charges;
                            capital -= allocation.Capital;
                            break;
                        default:
                            throw new InvalidOperationException($"Natura contabile non riconosciuta per il movimento '{movement.Id}'.");
                    }
                }
            }

            while (nextDay <= closingDate)
            {
                capitalDays.Add(new RevolvingCapitalDay(nextDay, capital, GetParameter(parameters, "Tan", nextDay)));
                nextDay = nextDay.AddDays(1);
            }

            return RevolvingCalculator.CalculateInterest(capitalDays);
        }

        private static async Task<decimal> CalculateBalance(decimal initialBalance, IEnumerable<Movimento> movements, Func<Movimento, Task<decimal>> evaluateMovement)
        {
            decimal balance = initialBalance;
            foreach (Movimento movement in movements)
            {
                balance += await evaluateMovement(movement);
            }

            return balance;
        }

        private static decimal GetParameter(IReadOnlyList<ParametroConto> parameters, string name, DateOnly date)
            => parameters.Where(parameter => string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase) && (parameter.ValidFrom is null || parameter.ValidFrom <= date) && (parameter.ValidTo is null || parameter.ValidTo >= date)).OrderBy(parameter => parameter.Index).FirstOrDefault()?.Value ?? throw new InvalidOperationException($"Parametro '{name}' non disponibile al {date:dd/MM/yyyy}.");

        private static int GetClosingDay(IReadOnlyList<ParametroConto> parameters, DateOnly date)
        {
            decimal value = GetParameter(parameters, "ChiusuraCiclo", date);
            return value is >= 1m and <= 31m && value == decimal.Truncate(value) ? decimal.ToInt32(value) : throw new InvalidOperationException("Il giorno di chiusura deve essere un intero da 1 a 31.");
        }
    }
}
