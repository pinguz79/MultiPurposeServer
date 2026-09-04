using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public class CycleIndicatorsService(
        IParametroContoService parametroService,
        IMovimentoRepository movimentoRepository,
        IFormulaEvaluator formulaEvaluator) : ICycleIndicatorsService
    {
        private static readonly string[] RequiredParameters =
        [
            "Plafond",
            "PercentualeScoperto",
            "ChiusuraCiclo",
            "Addebito",
            "RipristinoPlafond",
        ];

        public async Task<CycleIndicators?> Get(Conto conto, decimal balance, DateOnly date)
        {
            Dictionary<string, ParametroConto>? parameters = await ResolveProfile(conto.Id, date);
            if (parameters is null)
            {
                return null;
            }

            ValidateProfile(parameters);
            decimal plafond = parameters["Plafond"].Value;
            decimal overdraftPercentage = parameters["PercentualeScoperto"].Value;
            decimal overdraft = decimal.Round(plafond * overdraftPercentage, 2, MidpointRounding.AwayFromZero);
            int closingDay = decimal.ToInt32(parameters["ChiusuraCiclo"].Value);
            DateOnly cycleTo = GetCycleClosingDate(date, closingDay);
            DateOnly cycleFrom = GetPreviousClosingDate(cycleTo, closingDay).AddDays(1);
            decimal currentCycleSpent = await SumVisibleMovements(conto.Id, cycleFrom, date);
            decimal? pendingDebit = await GetPendingDebit(conto, parameters, date);
            (CycleThreshold? plafondExceeded, CycleThreshold? totalLimitExceeded) = await GetFutureThresholds(
                conto.Id,
                date,
                balance,
                plafond,
                overdraft,
                balance > plafond,
                balance > plafond + overdraft);

            return new CycleIndicators(
                cycleFrom,
                cycleTo,
                currentCycleSpent,
                plafond,
                overdraftPercentage,
                overdraft,
                plafond - balance,
                plafond + overdraft - balance,
                pendingDebit == currentCycleSpent ? null : pendingDebit,
                plafondExceeded,
                totalLimitExceeded);
        }

        private async Task<Dictionary<string, ParametroConto>?> ResolveProfile(Guid contoId, DateOnly date)
        {
            var result = new Dictionary<string, ParametroConto>(StringComparer.OrdinalIgnoreCase);

            foreach (string name in RequiredParameters)
            {
                ParametroConto? parameter = await parametroService.Resolve(contoId, name, date);

                if (parameter is null)
                {
                    return null;
                }

                result.Add(name, parameter);
            }

            return result;
        }

        private static void ValidateProfile(IReadOnlyDictionary<string, ParametroConto> parameters)
        {
            ValidateType(parameters, "Plafond", TipoParametroConto.Importo);
            ValidateType(parameters, "PercentualeScoperto", TipoParametroConto.Percentuale);
            ValidateType(parameters, "ChiusuraCiclo", TipoParametroConto.Intero);
            ValidateType(parameters, "Addebito", TipoParametroConto.Intero);
            ValidateType(parameters, "RipristinoPlafond", TipoParametroConto.Intero);

            if (parameters["Plafond"].Value <= 0)
            {
                throw new InvalidOperationException("Plafond must be positive.");
            }

            if (parameters["PercentualeScoperto"].Value is < 0 or > 1)
            {
                throw new InvalidOperationException("PercentualeScoperto must be between zero and one.");
            }

            int closingDay = ValidateDay(parameters["ChiusuraCiclo"]);
            int debitDay = ValidateDay(parameters["Addebito"]);
            int resetDay = ValidateDay(parameters["RipristinoPlafond"]);

            if (resetDay < debitDay)
            {
                throw new InvalidOperationException("RipristinoPlafond cannot precede Addebito.");
            }

            _ = closingDay;
        }

        private async Task<decimal> SumVisibleMovements(Guid contoId, DateOnly from, DateOnly to)
        {
            IReadOnlyList<Movimento> movements = await movimentoRepository.GetByContoThrough(contoId, to);
            decimal result = 0m;

            foreach (Movimento movimento in movements.Where(movement => movement.Date >= from && !IsTechnical(movement)))
            {
                result += await Evaluate(movimento);
            }

            return decimal.Round(result, 2, MidpointRounding.AwayFromZero);
        }

        private async Task<decimal?> GetPendingDebit(Conto conto, IReadOnlyDictionary<string, ParametroConto> parameters, DateOnly date)
        {
            int closingDay = decimal.ToInt32(parameters["ChiusuraCiclo"].Value);
            DateOnly lastClosingDate = GetLastClosingDate(date, closingDay);
            DateOnly resetMonth = lastClosingDate.AddMonths(1);
            int resetDay = decimal.ToInt32(parameters["RipristinoPlafond"].Value);
            var resetDate = new DateOnly(resetMonth.Year, resetMonth.Month, Math.Min(resetDay, DateTime.DaysInMonth(resetMonth.Year, resetMonth.Month)));

            if (date >= resetDate)
            {
                return null;
            }

            FormulaEvaluationResult result = await formulaEvaluator.Evaluate($"[{conto.Name}.{FormulaResolver.SaldoUltimoCicloChiuso}]", date);

            return result.Error is null && result.Value != 0m ? result.Value : null;
        }

        private async Task<(CycleThreshold? Plafond, CycleThreshold? Total)> GetFutureThresholds(
            Guid contoId,
            DateOnly date,
            decimal currentBalance,
            decimal plafond,
            decimal overdraft,
            bool plafondAlreadyExceeded,
            bool totalAlreadyExceeded)
        {
            IReadOnlyList<Movimento> movements = await movimentoRepository.GetByContoAfter(contoId, date);
            decimal balance = currentBalance;
            bool plafondExceeded = plafondAlreadyExceeded;
            bool totalLimitExceeded = totalAlreadyExceeded;
            CycleThreshold? firstPlafondExceeded = null;
            CycleThreshold? firstTotalLimitExceeded = null;

            foreach (IGrouping<DateOnly, Movimento> dailyMovements in movements.GroupBy(movement => movement.Date))
            {
                foreach (Movimento movement in dailyMovements)
                {
                    balance += await Evaluate(movement);
                }

                bool exceedsPlafond = balance > plafond;
                bool exceedsTotalLimit = balance > plafond + overdraft;

                if (!plafondExceeded && firstPlafondExceeded is null && exceedsPlafond)
                {
                    firstPlafondExceeded = new CycleThreshold(dailyMovements.Key, balance, balance - plafond);
                }

                if (!totalLimitExceeded && firstTotalLimitExceeded is null && exceedsTotalLimit)
                {
                    firstTotalLimitExceeded = new CycleThreshold(dailyMovements.Key, balance, balance - plafond - overdraft);
                }

                plafondExceeded = exceedsPlafond;
                totalLimitExceeded = exceedsTotalLimit;

                if (firstPlafondExceeded is not null && firstTotalLimitExceeded is not null)
                {
                    break;
                }
            }

            return (firstPlafondExceeded, firstTotalLimitExceeded);
        }

        private async Task<decimal> Evaluate(Movimento movimento)
        {
            FormulaEvaluationResult result = await formulaEvaluator.Evaluate(movimento.Formula, movimento.Date);

            return result.Error is null
                ? result.Value!.Value
                : throw new FormulaEvaluationException(movimento, new InvalidOperationException(result.Error));
        }

        private static DateOnly GetCycleClosingDate(DateOnly date, int closingDay)
        {
            int currentClosingDay = Math.Min(closingDay, DateTime.DaysInMonth(date.Year, date.Month));
            DateOnly month = date.Day <= currentClosingDay ? new DateOnly(date.Year, date.Month, 1) : new DateOnly(date.Year, date.Month, 1).AddMonths(1);

            return new DateOnly(month.Year, month.Month, Math.Min(closingDay, DateTime.DaysInMonth(month.Year, month.Month)));
        }

        private static DateOnly GetLastClosingDate(DateOnly date, int closingDay)
        {
            int currentClosingDay = Math.Min(closingDay, DateTime.DaysInMonth(date.Year, date.Month));
            DateOnly month = date.Day >= currentClosingDay ? new DateOnly(date.Year, date.Month, 1) : new DateOnly(date.Year, date.Month, 1).AddMonths(-1);

            return new DateOnly(month.Year, month.Month, Math.Min(closingDay, DateTime.DaysInMonth(month.Year, month.Month)));
        }

        private static DateOnly GetPreviousClosingDate(DateOnly closingDate, int closingDay)
        {
            DateOnly month = closingDate.AddMonths(-1);

            return new DateOnly(month.Year, month.Month, Math.Min(closingDay, DateTime.DaysInMonth(month.Year, month.Month)));
        }

        private static bool IsTechnical(Movimento movimento)
            => string.Equals(movimento.Categoria?.Name, "Tecnico", StringComparison.OrdinalIgnoreCase);

        private static int ValidateDay(ParametroConto parameter)
        {
            int value = decimal.ToInt32(parameter.Value);

            return value is >= 1 and <= 31 ? value : throw new InvalidOperationException($"{parameter.Name} must be between 1 and 31.");
        }

        private static void ValidateType(IReadOnlyDictionary<string, ParametroConto> parameters, string name, TipoParametroConto type)
        {
            if (parameters[name].Type != type)
            {
                throw new InvalidOperationException($"{name} must have type {type}.");
            }
        }
    }
}
