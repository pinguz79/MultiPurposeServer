using System.Globalization;

using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public class MovimentoService(
        IContoRepository contoRepository,
        IMovimentoRepository movimentoRepository,
        IFormulaEvaluator formulaEvaluator,
        EntityFrameworkPersistenceCoordinator<DataModel.FinanceContext> persistence,
        ICategoriaService? categoriaService = null,
        IParametroContoService? parametroContoService = null) : IMovimentoService
    {
        private const int MinimumMovementsOutsideSelectedPeriod = 15;

        #region Operazioni

        public async Task<IApplicationOperation> BeginOperation() => new ApplicationOperation(await persistence.BeginTransaction());

        public async Task<int> Consolidate(DateOnly today)
        {
            IReadOnlyList<Movimento> movements = await movimentoRepository.GetBefore(today);
            var changes = new List<(Guid Id, string Formula)>();

            // Valutiamo tutto prima di scrivere: le dipendenze tra conti vedono gli stessi dati e beneficiano della cache.
            foreach (Movimento movimento in movements)
            {
                decimal amount = await EvaluateFormula(movimento);
                string formula = amount.ToString("0.00", CultureInfo.InvariantCulture);
                if (movimento.Formula != formula || movimento.PianificazioneId is not null)
                {
                    changes.Add((movimento.Id, formula));
                }
            }

            foreach ((Guid id, string formula) in changes)
            {
                await movimentoRepository.Consolidate(id, formula);
            }

            return changes.Count;
        }

        public async Task<Movimento> Create(Guid contoId, DateOnly date, string description, string formula)
            => await movimentoRepository.Create(contoId, date, description, await NormalizeFormula(formula));

        public async Task<ContoCicliDto> GetCycleTimeline(string contoName, int month, int year)
        {
            Conto conto = await contoRepository.GetByName(contoName) ?? throw new KeyNotFoundException($"Conto '{contoName}' was not found.");

            return await GetCycleTimeline(conto, month, year);
        }

        public async Task<ContoCicliDto> GetCurrentCycleTimeline(string contoName)
        {
            Conto conto = await contoRepository.GetByName(contoName) ?? throw new KeyNotFoundException($"Conto '{contoName}' was not found.");
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            int closingDay = await GetClosingDay(conto.Id, new DateOnly(today.Year, today.Month, 1));
            DateOnly closingDate = GetCycleTo(today, closingDay);

            return await GetCycleTimeline(conto, closingDate.Month, closingDate.Year);
        }

        private async Task<ContoCicliDto> GetCycleTimeline(Conto conto, int month, int year)
        {
            DateOnly selectedMonth = new(year, month, 1);
            int closingDay = await GetClosingDay(conto.Id, selectedMonth);
            DateOnly selectedTo = GetClosingDate(selectedMonth, closingDay);
            DateOnly selectedFrom = GetClosingDate(selectedMonth.AddMonths(-1), closingDay).AddDays(1);
            DateOnly previousCycleFrom = GetClosingDate(selectedMonth.AddMonths(-2), closingDay).AddDays(1);
            DateOnly nextCycleTo = GetClosingDate(selectedMonth.AddMonths(1), closingDay);
            IReadOnlyList<DateOnly> previousDates = await movimentoRepository.GetPreviousDates(conto.Id, selectedFrom, MinimumMovementsOutsideSelectedPeriod);
            IReadOnlyList<DateOnly> nextDates = await movimentoRepository.GetNextDates(conto.Id, selectedTo, MinimumMovementsOutsideSelectedPeriod);
            DateOnly from = GetCycleFrom(GetExtendedFrom(previousDates, previousCycleFrom), closingDay);
            DateOnly to = GetCycleTo(GetExtendedTo(nextDates, nextCycleTo), closingDay);
            IReadOnlyList<Movimento> movements = await movimentoRepository.GetByContoThrough(conto.Id, to);
            decimal balance = conto.InitialBalance;
            decimal openingBalance = balance;
            var visibleMovements = new List<(Movimento Movement, decimal Amount, decimal Balance)>();

            foreach (Movimento movement in movements)
            {
                decimal amount = await EvaluateFormula(movement);

                if (movement.Date < from)
                {
                    balance += amount;
                    openingBalance = balance;
                    continue;
                }

                balance += amount;
                if (!IsTechnical(movement))
                {
                    visibleMovements.Add((movement, amount, balance));
                }
            }

            IReadOnlyList<CicloDto> cycles = CreateCycles(from, to, closingDay, visibleMovements);

            return new ContoCicliDto(new ContoDto(conto, balance), month, year, from, to, openingBalance, balance, cycles);
        }

        public async Task<ContoMovimentiDto> GetTimeline(string contoName, int month, int year)
        {
            Conto conto = await contoRepository.GetByName(contoName) ?? throw new KeyNotFoundException($"Conto '{contoName}' was not found.");
            var selectedFrom = new DateOnly(year, month, 1);
            DateOnly selectedTo = selectedFrom.AddMonths(1).AddDays(-1);
            IReadOnlyList<DateOnly> previousDates = await movimentoRepository.GetPreviousDates(conto.Id, selectedFrom, MinimumMovementsOutsideSelectedPeriod);
            IReadOnlyList<DateOnly> nextDates = await movimentoRepository.GetNextDates(conto.Id, selectedTo, MinimumMovementsOutsideSelectedPeriod);
            DateOnly from = GetFrom(previousDates, selectedFrom);
            DateOnly to = GetTo(nextDates, selectedTo);
            IReadOnlyList<Movimento> movements = await movimentoRepository.GetByContoThrough(conto.Id, to);
            decimal balance = conto.InitialBalance;
            decimal openingBalance = balance;
            var items = new List<MovimentoDto>();

            foreach (Movimento movimento in movements)
            {
                decimal amount = await EvaluateFormula(movimento);

                if (movimento.Date < from)
                {
                    balance += amount;
                    openingBalance = balance;
                    continue;
                }

                balance += amount;
                items.Add(new MovimentoDto(movimento.Id, movimento.Date, movimento.Description, amount, balance));
            }

            return new ContoMovimentiDto(new ContoDto(conto, balance), month, year, from, to, openingBalance, balance, items);
        }

        public async Task<Movimento> Update(
            Guid id,
            DateOnly? date,
            string? description,
            string? formula,
            string? categoryName = null,
            bool? clearCategory = null)
        {
            if (categoryName is not null && clearCategory is not null)
            {
                throw new ArgumentException("CategoryName and ClearCategory are mutually exclusive.");
            }

            if (clearCategory == false)
            {
                throw new ArgumentException("ClearCategory must be true when provided.", nameof(clearCategory));
            }

            Guid? categoriaId = categoryName is null ? null
                : (await (categoriaService ?? throw new InvalidOperationException("Category service is not available.")).Resolve(categoryName)).Id;

            return await movimentoRepository.Update(
                id,
                date,
                description,
                formula is null ? null : await NormalizeFormula(formula),
                categoriaId,
                clearCategory == true);
        }

        #endregion

        #region Formule

        private async Task<decimal> EvaluateFormula(Movimento movimento)
        {
            FormulaEvaluationResult result = await formulaEvaluator.Evaluate(movimento.Formula, movimento.Date);

            return result.Error is null
                ? result.Value!.Value
                : throw new FormulaEvaluationException(movimento, new InvalidOperationException(result.Error));
        }

        private async Task<string> NormalizeFormula(string formula)
        {
            FormulaValidationResult validation = await formulaEvaluator.Validate(formula);

            return validation.IsValid ? validation.Formula : throw new ArgumentException(string.Join(" ", validation.Errors), nameof(formula));
        }

        #endregion

        #region Intervallo temporale

        private async Task<int> GetClosingDay(Guid contoId, DateOnly month)
        {
            IParametroContoService service = parametroContoService
                ?? throw new InvalidOperationException("Account parameter service is not available.");
            DateOnly referenceDate = new(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));
            ParametroConto parameter = await service.Resolve(contoId, "ChiusuraCiclo", referenceDate)
                ?? throw new InvalidOperationException("ChiusuraCiclo is not configured for the selected period.");
            int value = decimal.ToInt32(parameter.Value);

            return parameter.Type == TipoParametroConto.Intero && value is >= 1 and <= 31
                ? value
                : throw new InvalidOperationException("ChiusuraCiclo must be an integer between 1 and 31.");
        }

        private static IReadOnlyList<CicloDto> CreateCycles(
            DateOnly from,
            DateOnly to,
            int closingDay,
            IReadOnlyList<(Movimento Movement, decimal Amount, decimal Balance)> movements)
        {
            var result = new List<CicloDto>();
            DateOnly cycleFrom = GetCycleFrom(from, closingDay);

            while (cycleFrom <= to)
            {
                DateOnly cycleTo = GetClosingDate(cycleFrom.AddMonths(1), closingDay);
                decimal cycleBalance = 0m;
                MovimentoCicloDto[] items =
                [
                    .. movements
                        .Where(item => item.Movement.Date >= cycleFrom && item.Movement.Date <= cycleTo)
                        .Select(item =>
                        {
                            cycleBalance += item.Amount;
                            return new MovimentoCicloDto(
                                item.Movement.Id,
                                item.Movement.Date,
                                item.Movement.Description,
                                item.Amount,
                                item.Balance,
                                cycleBalance);
                        }),
                ];
                result.Add(new CicloDto(cycleFrom, cycleTo, cycleBalance, items));
                cycleFrom = cycleTo.AddDays(1);
            }

            return result;
        }

        private static DateOnly GetCycleFrom(DateOnly date, int closingDay)
        {
            DateOnly closingDate = GetClosingDate(new DateOnly(date.Year, date.Month, 1), closingDay);

            return date <= closingDate ? GetClosingDate(closingDate.AddMonths(-1), closingDay).AddDays(1) : closingDate.AddDays(1);
        }

        private static DateOnly GetCycleTo(DateOnly date, int closingDay)
        {
            DateOnly closingDate = GetClosingDate(new DateOnly(date.Year, date.Month, 1), closingDay);

            return date <= closingDate ? closingDate : GetClosingDate(closingDate.AddMonths(1), closingDay);
        }

        private static DateOnly GetClosingDate(DateOnly month, int closingDay)
            => new(month.Year, month.Month, Math.Min(closingDay, DateTime.DaysInMonth(month.Year, month.Month)));

        private static DateOnly GetExtendedFrom(IReadOnlyList<DateOnly> previousDates, DateOnly defaultFrom)
        {
            DateOnly threshold = previousDates.Count < MinimumMovementsOutsideSelectedPeriod ? defaultFrom : previousDates[^1];

            return threshold < defaultFrom ? threshold : defaultFrom;
        }

        private static DateOnly GetExtendedTo(IReadOnlyList<DateOnly> nextDates, DateOnly defaultTo)
        {
            DateOnly threshold = nextDates.Count < MinimumMovementsOutsideSelectedPeriod ? defaultTo : nextDates[^1];

            return threshold > defaultTo ? threshold : defaultTo;
        }

        private static DateOnly GetFrom(IReadOnlyList<DateOnly> previousDates, DateOnly selectedFrom)
        {
            DateOnly defaultFrom = selectedFrom.AddMonths(-1);
            DateOnly threshold = previousDates.Count < MinimumMovementsOutsideSelectedPeriod ? defaultFrom : previousDates[^1];

            return threshold < defaultFrom ? threshold : defaultFrom;
        }

        private static DateOnly GetTo(IReadOnlyList<DateOnly> nextDates, DateOnly selectedTo)
        {
            DateOnly nextMonth = selectedTo.AddMonths(1);
            DateOnly defaultTo = new(nextMonth.Year, nextMonth.Month, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
            DateOnly threshold = nextDates.Count < MinimumMovementsOutsideSelectedPeriod ? defaultTo : nextDates[^1];

            return threshold > defaultTo ? threshold : defaultTo;
        }

        private static bool IsTechnical(Movimento movimento)
            => string.Equals(movimento.Categoria?.Name, "Tecnico", StringComparison.OrdinalIgnoreCase);

        #endregion
    }
}
