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
        EntityFrameworkPersistenceCoordinator<DataModel.FinanceContext> persistence) : IMovimentoService
    {
        private const int MinimumMovementsOutsideSelectedMonth = 15;

        #region Operazioni

        public async Task<IApplicationOperation> BeginOperation() => new ApplicationOperation(await persistence.BeginTransaction());

        public async Task<Movimento> Create(Guid contoId, DateOnly date, string description, string formula)
            => await movimentoRepository.Create(contoId, date, description, await NormalizeFormula(formula));

        public async Task<ContoMovimentiDto> GetTimeline(string contoName, int month, int year)
        {
            Conto conto = await contoRepository.GetByName(contoName) ?? throw new KeyNotFoundException($"Conto '{contoName}' was not found.");
            var selectedFrom = new DateOnly(year, month, 1);
            DateOnly selectedTo = selectedFrom.AddMonths(1).AddDays(-1);
            IReadOnlyList<DateOnly> previousDates = await movimentoRepository.GetPreviousDates(conto.Id, selectedFrom, MinimumMovementsOutsideSelectedMonth);
            IReadOnlyList<DateOnly> nextDates = await movimentoRepository.GetNextDates(conto.Id, selectedTo, MinimumMovementsOutsideSelectedMonth);
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

        public async Task<Movimento> Update(Guid id, DateOnly? date, string? description, string? formula)
            => await movimentoRepository.Update(id, date, description, formula is null ? null : await NormalizeFormula(formula));

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

        private static DateOnly GetFrom(IReadOnlyList<DateOnly> previousDates, DateOnly selectedFrom)
        {
            DateOnly defaultFrom = selectedFrom.AddMonths(-1);
            DateOnly threshold = previousDates.Count < MinimumMovementsOutsideSelectedMonth ? defaultFrom : previousDates[^1];

            return threshold < defaultFrom ? threshold : defaultFrom;
        }

        private static DateOnly GetTo(IReadOnlyList<DateOnly> nextDates, DateOnly selectedTo)
        {
            DateOnly nextMonth = selectedTo.AddMonths(1);
            DateOnly defaultTo = new(nextMonth.Year, nextMonth.Month, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
            DateOnly threshold = nextDates.Count < MinimumMovementsOutsideSelectedMonth ? defaultTo : nextDates[^1];

            return threshold > defaultTo ? threshold : defaultTo;
        }

        #endregion
    }
}
