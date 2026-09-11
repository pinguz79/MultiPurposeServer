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
        IVoceRicorrenteRepository voceRicorrenteRepository,
        IFormulaEvaluator formulaEvaluator,
        ICategoriaService categoriaService,
        EntityFrameworkPersistenceCoordinator<DataModel.FinanceContext> persistence) : IPianificazioneService
    {
        public async Task<IApplicationOperation> BeginOperation() => new ApplicationOperation(await persistence.BeginTransaction());

        public Task<Pianificazione?> Get(Guid id) => pianificazioneRepository.Get(id);

        public Task<IReadOnlyList<Pianificazione>> GetList(string? contoName) => pianificazioneRepository.GetList(contoName);

        public async Task<bool> Delete(Guid id, bool deleteMovimenti)
        {
            Pianificazione? pianificazione = await pianificazioneRepository.Get(id);
            if (pianificazione is null)
            {
                return false;
            }

            await pianificazioneRepository.Delete(pianificazione, deleteMovimenti);
            return true;
        }

        public async Task<CreatePianificazioneDto> Create(CreatePianificazioneRequest request)
        {
            PianificazionePreviewDto preview = await Preview(request);

            if (!preview.IsValid)
            {
                throw new ArgumentException(string.Join(" ", preview.Errors), nameof(request));
            }

            Conto conto = await contoRepository.GetByName(request.ContoName)
                ?? throw new KeyNotFoundException($"Conto '{request.ContoName}' was not found.");
            Periodicita periodicita = request.Frequency == FrequenzaPeriodicita.Settimanale
                ? await pianificazioneRepository.GetOrCreateWeeklyPeriodicity(request.Interval, request.DayOfWeek!.Value)
                : await pianificazioneRepository.GetOrCreateMonthlyPeriodicity(request.Interval, request.DayOfMonth, request.EndOfMonth);
            var categoryErrors = new List<string>();
            (string? sourceName, Categoria? explicitCategory) = await ResolveCategoryConfiguration(request, categoryErrors);
            if (categoryErrors.Count > 0)
            {
                throw new ArgumentException(string.Join(" ", categoryErrors), nameof(request));
            }
            Pianificazione pianificazione = await pianificazioneRepository.Create(
                conto.Id,
                periodicita.Id,
                NormalizeDescription(request.Description, nameof(request.Description)),
                NormalizeDescription(request.MovimentoDescription, nameof(request.MovimentoDescription)),
                preview.Formula,
                request.ValidFrom,
                request.ValidTo,
                request.CategoryMode,
                sourceName,
                explicitCategory?.Id);
            var movimentoIds = new List<Guid>(preview.Occurrences.Count);

            foreach (PianificazioneOccurrenceDto occurrence in preview.Occurrences)
            {
                Guid? categoriaId = occurrence.Category is null ? null : (await categoriaService.Resolve(occurrence.Category.Name)).Id;
                Movimento movimento = await movimentoRepository.Create(
                    conto.Id,
                    occurrence.Date,
                    pianificazione.MovimentoDescription,
                    preview.Formula,
                    pianificazione.Id,
                    categoriaId);
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
            (string? sourceName, Categoria? explicitCategory) = await ResolveCategoryConfiguration(request, errors);
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

            IReadOnlyList<DateOnly> dates = errors.Count != 0 ? [] : request.Frequency == FrequenzaPeriodicita.Settimanale ? GetWeeklyOccurrences(request) : GetMonthlyOccurrences(request);
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

                Categoria? categoria = await ResolveOccurrenceCategory(request.CategoryMode, sourceName, explicitCategory, date);
                occurrences.Add(new PianificazioneOccurrenceDto(
                    date,
                    result.Value,
                    status,
                    messages,
                    categoria is null ? null : new CategoriaReferenceDto(categoria)));
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

        private static IReadOnlyList<DateOnly> GetWeeklyOccurrences(CreatePianificazioneRequest request)
        {
            var result = new List<DateOnly>();
            int offset = ((int)request.DayOfWeek!.Value - (int)request.ValidFrom.DayOfWeek + 7) % 7;
            long last = request.ValidTo.DayNumber;
            for (long day = (long)request.ValidFrom.DayNumber + offset; day <= last; day += (long)request.Interval * 7)
            {
                result.Add(DateOnly.FromDayNumber((int)day));
            }

            return result;
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

        private async Task<(string? SourceName, Categoria? ExplicitCategory)> ResolveCategoryConfiguration(
            CreatePianificazioneRequest request,
            List<string> errors)
        {
            if (!Enum.IsDefined(request.CategoryMode))
            {
                errors.Add("CategoryMode is invalid.");
                return (null, null);
            }

            if (request.CategoryMode == ModalitaCategoria.Ereditata)
            {
                if (request.CategoryName is not null || request.CategorySourceName is null)
                {
                    errors.Add("Inherited category requires CategorySourceName and does not accept CategoryName.");
                    return (null, null);
                }

                IReadOnlyList<VoceRicorrente> definitions = await voceRicorrenteRepository.GetByName(request.CategorySourceName);
                if (definitions.Count == 0)
                {
                    errors.Add($"Recurring entry '{request.CategorySourceName}' was not found.");
                    return (null, null);
                }

                return (definitions[0].Name, null);
            }

            if (request.CategoryMode == ModalitaCategoria.Nessuna)
            {
                if (request.CategoryName is not null || request.CategorySourceName is not null)
                {
                    errors.Add("No-category mode does not accept CategoryName or CategorySourceName.");
                }

                return (null, null);
            }

            if (request.CategoryName is null || request.CategorySourceName is not null)
            {
                errors.Add("Explicit category requires CategoryName and does not accept CategorySourceName.");
                return (null, null);
            }

            try
            {
                return (null, await categoriaService.Resolve(request.CategoryName));
            }
            catch (KeyNotFoundException exception)
            {
                errors.Add(exception.Message);
                return (null, null);
            }
        }

        private async Task<Categoria?> ResolveOccurrenceCategory(
            ModalitaCategoria mode,
            string? sourceName,
            Categoria? explicitCategory,
            DateOnly date)
        {
            if (mode != ModalitaCategoria.Ereditata || sourceName is null)
            {
                return mode == ModalitaCategoria.Esplicita ? explicitCategory : null;
            }

            IReadOnlyList<VoceRicorrente> definitions = await voceRicorrenteRepository.GetByName(sourceName);
            VoceRicorrente? definition = definitions.FirstOrDefault(item => (item.ValidFrom is null || item.ValidFrom <= date)
                && (item.ValidTo is null || item.ValidTo >= date));

            return definition?.Categoria;
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

            if (request.Frequency is not (FrequenzaPeriodicita.Mensile or FrequenzaPeriodicita.Settimanale))
            {
                errors.Add("La frequenza deve essere mensile o settimanale.");
            }

            if (request.Frequency == FrequenzaPeriodicita.Settimanale)
            {
                if (request.DayOfWeek is null || !Enum.IsDefined(request.DayOfWeek.Value))
                {
                    errors.Add("Selezionare un giorno della settimana valido.");
                }

                if (request.DayOfMonth is not null || request.EndOfMonth)
                {
                    errors.Add("La cadenza settimanale non accetta giorno del mese o fine mese.");
                }

                return errors;
            }

            if (request.DayOfWeek is not null)
            {
                errors.Add("La cadenza mensile non accetta il giorno della settimana.");
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
