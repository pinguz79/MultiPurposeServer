using Finance.Api.Infrastructure.Caching;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public class CartaRevolvingService(
        IContoRepository contoRepository,
        IParametroContoService parametroService,
        IPianificazioneRepository pianificazioneRepository,
        IMovimentoRepository movimentoRepository,
        ICartaRevolvingRepository repository,
        IFormulaEvaluator evaluator,
        FormulaEvaluationCache cache,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence) : ICartaRevolvingService
    {
        public async Task<IApplicationOperation> BeginOperation() => new ApplicationOperation(await persistence.BeginTransaction());

        public async Task<CartaRevolvingDto> Configure(string contoName, ConfigureCartaRevolvingRequest request)
        {
            if (!persistence.IsTransactionActive)
            {
                throw new InvalidOperationException("La configurazione revolving richiede un'operazione attiva.");
            }

            CartaRevolvingConfiguration.Validate(request);
            Conto carta = await contoRepository.GetByName(contoName) ?? throw new KeyNotFoundException($"Conto '{contoName}' non trovato.");
            Conto contoAddebito = await contoRepository.GetByName(request.ContoAddebitoName)
                ?? throw new KeyNotFoundException($"Conto '{request.ContoAddebitoName}' non trovato.");
            if (carta.Id == contoAddebito.Id)
            {
                throw new ArgumentException("Il conto di addebito deve essere diverso dalla carta.", nameof(request));
            }

            string prefix = $"Revolving {carta.Name}: ";
            PianificazioneCartaRevolvingConfiguration[] definitions = [
                new(carta.Id, $"{prefix}Interessi", $"Interessi {carta.DisplayName}", $"[{carta.Name}.{FormulaResolver.InteressiCiclo}]", NaturaMovimento.Interessi, request.ChiusuraCiclo),
                new(carta.Id, $"{prefix}Bollo", $"Bollo {carta.DisplayName}", $"[{carta.Name}.{FormulaResolver.BolloCiclo}]", NaturaMovimento.Bollo, request.ChiusuraCiclo),
                new(carta.Id, $"{prefix}Rimborso", $"Rimborso {carta.DisplayName}", $"-[{carta.Name}.{FormulaResolver.RataUltimoCicloChiuso}]", NaturaMovimento.Rimborso, request.Addebito),
                new(contoAddebito.Id, $"{prefix}Addebito", carta.DisplayName, $"-[{carta.Name}.{FormulaResolver.RataUltimoCicloChiuso}]", NaturaMovimento.Ordinario, request.Addebito),
            ];
            IReadOnlyList<Pianificazione> existing = await repository.GetPlans(prefix);
            if (existing.Count != 0 && (existing.Count != definitions.Length || definitions.Any(definition => existing.Count(plan => Matches(plan, definition, request)) != 1)))
            {
                throw new CartaRevolvingConflictException("Le pianificazioni revolving esistenti sono incomplete o incompatibili: nessuna modifica applicata.");
            }

            await ConfigureParameters(carta, request);
            var plans = new List<Pianificazione>();
            foreach (PianificazioneCartaRevolvingConfiguration definition in definitions)
            {
                plans.Add(existing.Count == 0 ? await CreatePlan(definition, request) : existing.Single(plan => plan.Description == definition.Description));
            }

            if (existing.Count == 0)
            {
                await repository.CreateCorrelation(plans[2].Id, plans[3].Id);
            }
            else if (!await repository.HasCorrelation(plans[2].Id, plans[3].Id))
            {
                throw new CartaRevolvingConflictException("Manca la correlazione fra rimborso e addebito: configurazione da verificare.");
            }

            await repository.Save();
            // Tutte le dipendenze devono esistere prima della valutazione, senza anticipare il commit.
            await cache.RunReadOnlyBatch(async () =>
            {
                foreach (Movimento movement in plans.SelectMany(plan => plan.Movimenti).OrderBy(movement => movement.Date))
                {
                    FormulaEvaluationResult result = await evaluator.Evaluate(movement.Formula, movement.Date);
                    if (result.Error is not null)
                    {
                        throw new ArgumentException($"Calcolo del movimento '{movement.Id}' non riuscito: {result.Error}", nameof(request));
                    }
                }
            });

            return new CartaRevolvingDto(carta.Name, plans[0].Id, plans[1].Id, plans[2].Id, plans[3].Id, existing.Count == 0);
        }

        private async Task ConfigureParameters(Conto carta, ConfigureCartaRevolvingRequest request)
        {
            IReadOnlyList<ParametroCartaRevolvingConfiguration> parameters = CartaRevolvingConfiguration.GetParameters(request);
            IReadOnlyList<IReadOnlyList<ParametroConto>> existing = await parametroService.GetAll(carta.Name);
            Dictionary<string, IReadOnlyList<ParametroConto>> byName = existing.ToDictionary(group => group[0].Name, StringComparer.OrdinalIgnoreCase);
            int count = parameters.Count(parameter => byName.ContainsKey(parameter.Name));
            if (byName.ContainsKey("RipristinoPlafond") || (request.Rata is null ? byName.ContainsKey("Rata") : byName.ContainsKey("QuotaRata") || byName.ContainsKey("RataMinima"))
                || (count != 0 && count != parameters.Count))
            {
                throw new CartaRevolvingConflictException("La carta ha un profilo a saldo o parametri revolving incompleti. Non vengono convertiti automaticamente.");
            }

            foreach (ParametroCartaRevolvingConfiguration parameter in parameters)
            {
                if (!byName.TryGetValue(parameter.Name, out IReadOnlyList<ParametroConto>? definitions))
                {
                    await parametroService.Create(carta.Name, parameter.Name, parameter.Type, [new ParametroContoDefinitionRequest(null, parameter.DisplayName, parameter.Value, null, null)]);
                    continue;
                }

                if (definitions.Any(definition => definition.Type != parameter.Type) || definitions.Count(definition => definition.ValidFrom is null && definition.ValidTo is null) != 1)
                {
                    throw new CartaRevolvingConflictException($"Il parametro '{parameter.Name}' ha un tipo o una copertura permanente incompatibile.");
                }

                if (parameter.Name is "ChiusuraCiclo" or "Addebito" && definitions.Any(definition => (definition.ValidFrom is not null || definition.ValidTo is not null)
                    && (definition.ValidFrom is null || definition.ValidFrom <= request.ValidTo) && (definition.ValidTo is null || definition.ValidTo >= request.ValidFrom)
                    && definition.Value != parameter.Value))
                {
                    throw new CartaRevolvingConflictException($"Il parametro '{parameter.Name}' contiene variazioni di calendario non compatibili con le pianificazioni richieste.");
                }

                await parametroService.Update(carta.Name, parameter.Name, [.. definitions.Select(definition => new ParametroContoDefinitionRequest(
                    definition.Id, definition.DisplayName, definition.ValidFrom is null && definition.ValidTo is null ? parameter.Value : definition.Value, definition.ValidFrom, definition.ValidTo))]);
            }
        }

        private async Task<Pianificazione> CreatePlan(PianificazioneCartaRevolvingConfiguration definition, ConfigureCartaRevolvingRequest request)
        {
            Periodicita periodicita = await pianificazioneRepository.GetOrCreateMonthlyPeriodicity(1, definition.Day, false);
            Pianificazione plan = await pianificazioneRepository.Create(definition.ContoId, periodicita.Id, definition.Description, definition.MovimentoDescription, definition.Formula,
                request.ValidFrom, request.ValidTo, ModalitaCategoria.Nessuna, null, null, definition.Natura);
            DateOnly month = new(request.ValidFrom.Year, request.ValidFrom.Month, 1);
            while (month <= request.ValidTo)
            {
                DateOnly date = new(month.Year, month.Month, Math.Min(definition.Day, DateTime.DaysInMonth(month.Year, month.Month)));
                if (date >= request.ValidFrom && date <= request.ValidTo)
                {
                    await movimentoRepository.Create(definition.ContoId, date, definition.MovimentoDescription, definition.Formula, plan.Id, natura: definition.Natura);
                }

                if (month.Year == 9999 && month.Month == 12)
                {
                    break;
                }
                month = month.AddMonths(1);
            }

            await repository.Save();
            return plan;
        }

        private static bool Matches(Pianificazione plan, PianificazioneCartaRevolvingConfiguration definition, ConfigureCartaRevolvingRequest request)
            => plan.Description == definition.Description && plan.ContoId == definition.ContoId && plan.MovimentoFormula == definition.Formula && plan.MovimentoNatura == definition.Natura && plan.ValidFrom == request.ValidFrom && plan.ValidTo == request.ValidTo && plan.ModalitaCategoria == ModalitaCategoria.Nessuna && plan.CategoriaId is null && plan.Periodicita.Frequenza == FrequenzaPeriodicita.Mensile && plan.Periodicita.Intervallo == 1 && plan.Periodicita.GiornoMese == definition.Day && !plan.Periodicita.FineMese;
    }
}
