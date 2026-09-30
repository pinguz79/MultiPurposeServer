using System.Globalization;

using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
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
        IParametroContoService? parametroContoService = null,
        IPedaggioRepository? pedaggioRepository = null) : IMovimentoService
    {
        private const int MinimumMovementsOutsideSelectedPeriod = 15;

        #region Operazioni

        public async Task<IApplicationOperation> BeginOperation() => new ApplicationOperation(await persistence.BeginTransaction());

        public async Task<int> Confirm(IReadOnlyList<Guid> ids)
        {
            if (ids is null || ids.Count == 0 || ids.Any(id => id == Guid.Empty) || ids.Distinct().Count() != ids.Count)
            {
                throw new ArgumentException("Selezionare identificativi distinti e non vuoti.", nameof(ids));
            }

            var changes = new List<(Guid Id, string Formula)>();
            foreach (Guid id in ids)
            {
                Movimento movimento = await movimentoRepository.GetById(id) ?? throw new KeyNotFoundException($"Movimento '{id}' non trovato.");
                if (!movimento.IsConfirmed)
                {
                    await ValidatePedaggioConfirmation(movimento);
                    changes.Add((id, (await EvaluateFormula(movimento)).ToString("0.00", CultureInfo.InvariantCulture)));
                }
            }

            foreach ((Guid id, string formula) in changes)
            {
                await movimentoRepository.Consolidate(id, formula);
            }

            return changes.Count;
        }

        public async Task SetConfirmation(Guid id, bool confirmed)
        {
            Movimento movimento = await movimentoRepository.GetById(id) ?? throw new KeyNotFoundException("Movimento non trovato.");
            if (confirmed)
            {
                if (!movimento.IsConfirmed)
                {
                    await ValidatePedaggioConfirmation(movimento);
                }
                await movimentoRepository.Consolidate(id, (await EvaluateFormula(movimento)).ToString("0.00", CultureInfo.InvariantCulture));
            }
            else
            {
                Pedaggio? pedaggio = pedaggioRepository is null ? null : await pedaggioRepository.GetByMovimento(id);
                if (pedaggio is not null)
                {
                    await movimentoRepository.Update(id, null, null, PedaggioFormula.Create(pedaggio.CaselloEntrataId, pedaggio.CaselloUscitaId), null, false, null);
                }
                await movimentoRepository.SetConfirmation(id, false);
            }
        }

        public async Task<IReadOnlyList<MovimentoConfigurationDto>> GetForReview(bool pendingOnly, string? contoName, DateOnly? from, DateOnly? to)
        {
            if (from > to)
            {
                throw new ArgumentException("Intervallo temporale non valido.");
            }

            IReadOnlyList<Movimento> movements = await movimentoRepository.GetForReview(pendingOnly, DateOnly.FromDateTime(DateTime.Today), contoName, from, to);
            IReadOnlyList<Pedaggio> pedaggi = pedaggioRepository is null || movements.Count == 0 ? [] : await pedaggioRepository.GetByMovimenti([.. movements.Select(movimento => movimento.Id)]);
            Dictionary<Guid, Pedaggio> pedaggiByMovement = pedaggi.ToDictionary(pedaggio => pedaggio.MovimentoId);
            var result = new List<MovimentoConfigurationDto>();
            foreach (Movimento movimento in movements)
            {
                FormulaEvaluationResult evaluation = await formulaEvaluator.Evaluate(movimento.Formula, movimento.Date);
                result.Add(new MovimentoConfigurationDto(movimento) { Amount = evaluation.Value, EvaluationError = evaluation.Error });
                if (pedaggiByMovement.TryGetValue(movimento.Id, out Pedaggio? pedaggio))
                {
                    result[^1].Pedaggio = new PedaggioDto(pedaggio);
                    result[^1].CanConfirm = movimento.IsConfirmed || evaluation.Error is null && evaluation.Value is not (null or 0m);
                    result[^1].ConfirmationWarning = result[^1].CanConfirm ? null : "Tariffa non disponibile";
                }
            }

            return result;
        }

        public async Task<MovimentoConfigurationDto> Create(SaveMovimentoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                throw new ArgumentException("La descrizione è obbligatoria.");
            }

            Conto conto = await contoRepository.GetByName(request.ContoName) ?? throw new KeyNotFoundException("Conto non trovato.");
            string formula = await NormalizeFormula(request.Formula);
            if (request.IsConfirmed)
            {
                FormulaEvaluationResult evaluation = await formulaEvaluator.Evaluate(formula, request.Date);
                formula = evaluation.Error is null ? evaluation.Value!.Value.ToString("0.00", CultureInfo.InvariantCulture)
                    : throw new ArgumentException(evaluation.Error);
            }

            Movimento saved = await Create(conto.Id, request.Date, request.Description, formula, categoryName: request.CategoryName);
            await movimentoRepository.SetConfirmation(saved.Id, request.IsConfirmed);
            saved.Conto = conto;
            return new MovimentoConfigurationDto(saved);
        }

        public Task<bool> Delete(Guid id) => movimentoRepository.Delete(id);

        public async Task<Movimento> Create(Guid contoId, DateOnly date, string description, string formula, NaturaMovimento natura = NaturaMovimento.Ordinario, string? categoryName = null)
        {
            ValidateNatura(natura);
            Guid? categoriaId = categoryName is null ? null
                : (await (categoriaService ?? throw new InvalidOperationException("Category service is not available.")).Resolve(categoryName)).Id;

            return await movimentoRepository.Create(contoId, date, description, await NormalizeFormula(formula), categoriaId: categoriaId, natura: natura);
        }

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

            foreach (Movimento movement in movements.OrderBy(item => item.Date).ThenByDescending(item => item.IsConfirmed))
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

            foreach (Movimento movimento in movements.OrderBy(item => item.Date).ThenByDescending(item => item.IsConfirmed))
            {
                decimal amount = await EvaluateFormula(movimento);

                if (movimento.Date < from)
                {
                    balance += amount;
                    openingBalance = balance;
                    continue;
                }

                balance += amount;
                items.Add(new MovimentoDto(movimento.Id, movimento.Date, movimento.Description, amount, balance, movimento.IsConfirmed));
            }

            return new ContoMovimentiDto(new ContoDto(conto, balance), month, year, from, to, openingBalance, balance, items);
        }

        public async Task<Movimento> Update(
            Guid id,
            DateOnly? date,
            string? description,
            string? formula,
            string? categoryName = null,
            bool? clearCategory = null,
            NaturaMovimento? natura = null)
        {
            if (natura is NaturaMovimento value)
            {
                ValidateNatura(value);
            }

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

            Pedaggio? pedaggio = pedaggioRepository is null ? null : await pedaggioRepository.GetByMovimento(id);
            if (pedaggio is not null && !pedaggio.Movimento.IsConfirmed)
            {
                formula = PedaggioFormula.Create(pedaggio.CaselloEntrataId, pedaggio.CaselloUscitaId);
            }

            return await movimentoRepository.Update(
                id,
                date,
                description,
                formula is null ? null : await NormalizeFormula(formula),
                categoriaId,
                clearCategory == true,
                natura);
        }

        private static void ValidateNatura(NaturaMovimento natura)
        {
            if (!Enum.IsDefined(natura))
            {
                throw new ArgumentOutOfRangeException(nameof(natura), "Natura contabile non valida.");
            }
        }

        #endregion

        #region Formule

        private async Task ValidatePedaggioConfirmation(Movimento movimento)
        {
            Pedaggio? pedaggio = pedaggioRepository is null ? null : await pedaggioRepository.GetByMovimento(movimento.Id);
            if (pedaggio is null)
            {
                return;
            }

            FormulaEvaluationResult result = await formulaEvaluator.Evaluate(PedaggioFormula.Create(pedaggio.CaselloEntrataId, pedaggio.CaselloUscitaId), movimento.Date);
            if (result.Error is not null || result.Value is null or 0m)
            {
                throw new ArgumentException("Tariffa non disponibile: il pedaggio deve restare da confermare.");
            }
        }

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
                                cycleBalance,
                                item.Movement.IsConfirmed);
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
