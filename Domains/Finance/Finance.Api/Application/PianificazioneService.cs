using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public class PianificazioneService(
        IContoRepository contoRepository,
        IMovimentoRepository movimentoRepository,
        IPianificazioneRepository pianificazioneRepository,
        IFormulaEvaluator formulaEvaluator,
        EntityFrameworkPersistenceCoordinator<DataModel.FinanceContext> persistence) : IPianificazioneService
    {
        public async Task<IApplicationOperation> BeginOperation() => new ApplicationOperation(await persistence.BeginTransaction());

        public async Task<CreatePianificazioneDto> Create(CreatePianificazioneRequest request)
        {
            PianificazionePreviewDto preview = await Preview(request);

            if (!preview.IsValid)
            {
                throw new ArgumentException(string.Join(" ", preview.Errors), nameof(request));
            }

            Conto conto = await contoRepository.GetByName(request.ContoName)
                ?? throw new KeyNotFoundException($"Conto '{request.ContoName}' was not found.");
            Periodicita periodicita = await pianificazioneRepository.GetOrCreateMonthlyPeriodicity(request.Interval, request.DayOfMonth, request.EndOfMonth);
            Pianificazione pianificazione = await pianificazioneRepository.Create(
                conto.Id,
                periodicita.Id,
                NormalizeDescription(request.Description, nameof(request.Description)),
                NormalizeDescription(request.MovimentoDescription, nameof(request.MovimentoDescription)),
                preview.Formula,
                request.ValidFrom,
                request.ValidTo);
            var movimentoIds = new List<Guid>(preview.Occurrences.Count);

            foreach (PianificazioneOccurrenceDto occurrence in preview.Occurrences)
            {
                Movimento movimento = await movimentoRepository.Create(
                    conto.Id,
                    occurrence.Date,
                    pianificazione.MovimentoDescription,
                    preview.Formula,
                    pianificazione.Id);
                movimentoIds.Add(movimento.Id);
            }

            return new CreatePianificazioneDto(
                pianificazione.Id,
                preview.Formula,
                preview.Description,
                preview.OccurrenceCount,
                preview.IsValid,
                preview.Errors,
                preview.Dependencies,
                preview.Occurrences,
                movimentoIds);
        }

        public async Task<PianificazionePreviewDto> Preview(CreatePianificazioneRequest request)
        {
            var errors = ValidateRequest(request);
            FormulaValidationResult validation = await formulaEvaluator.Validate(request.Formula);
            errors.AddRange(validation.Errors);
            string description = request.Description.Trim();

            if (description.Length == 0)
            {
                errors.Add("Description cannot be empty.");
            }

            if (request.MovimentoDescription.Trim().Length == 0)
            {
                errors.Add("MovimentoDescription cannot be empty.");
            }

            IReadOnlyList<DateOnly> dates = errors.Count == 0 ? GetMonthlyOccurrences(request) : [];
            var occurrences = new List<PianificazioneOccurrenceDto>(dates.Count);

            foreach (DateOnly date in dates)
            {
                FormulaEvaluationResult result = await formulaEvaluator.Evaluate(validation.Formula, date);
                var messages = new List<string>();
                string status = "Valida";

                if (result.Error is not null)
                {
                    status = "Errore";
                    messages.Add(result.Error);
                }
                else if (result.HasUncoveredInterval)
                {
                    status = "IntervalloScoperto";
                    messages.Add("Nessuna definizione temporale copre questa data; viene utilizzato 0,00.");
                }

                occurrences.Add(new PianificazioneOccurrenceDto(date, result.Value, status, messages));
            }

            return new PianificazionePreviewDto(
                validation.Formula,
                description,
                occurrences.Count,
                errors.Count == 0,
                errors,
                [.. validation.Dependencies.Select(dependency => new FormulaDependencyDto(dependency, "VoceRicorrente"))],
                occurrences);
        }

        private static IReadOnlyList<DateOnly> GetMonthlyOccurrences(CreatePianificazioneRequest request)
        {
            var result = new List<DateOnly>();
            var cursor = new DateOnly(request.ValidFrom.Year, request.ValidFrom.Month, 1);

            while (cursor <= request.ValidTo)
            {
                int day = request.EndOfMonth ? DateTime.DaysInMonth(cursor.Year, cursor.Month)
                    : Math.Min(request.DayOfMonth!.Value, DateTime.DaysInMonth(cursor.Year, cursor.Month));
                var occurrence = new DateOnly(cursor.Year, cursor.Month, day);

                if (occurrence >= request.ValidFrom && occurrence <= request.ValidTo)
                {
                    result.Add(occurrence);
                }

                cursor = cursor.AddMonths(request.Interval);
            }

            return result;
        }

        private static string NormalizeDescription(string value, string parameterName)
        {
            string normalized = value.Trim();

            return normalized.Length == 0 ? throw new ArgumentException("Description cannot be empty.", parameterName) : normalized;
        }

        private static List<string> ValidateRequest(CreatePianificazioneRequest request)
        {
            var errors = new List<string>();

            if (request.ValidFrom > request.ValidTo)
            {
                errors.Add("ValidFrom cannot be later than ValidTo.");
            }

            if (request.Interval < 1)
            {
                errors.Add("Interval must be greater than zero.");
            }

            if (request.EndOfMonth == request.DayOfMonth.HasValue)
            {
                errors.Add("Choose either DayOfMonth or EndOfMonth.");
            }

            if (request.DayOfMonth is < 1 or > 31)
            {
                errors.Add("DayOfMonth must be between 1 and 31.");
            }

            return errors;
        }
    }
}
