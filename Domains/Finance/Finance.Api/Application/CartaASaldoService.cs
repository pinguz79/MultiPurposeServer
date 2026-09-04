using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public class CartaASaldoService(
        IContoRepository contoRepository,
        IParametroContoService parametroService,
        ICategoriaService categoriaService,
        IPianificazioneService pianificazioneService,
        ICartaASaldoRepository repository,
        EntityFrameworkPersistenceCoordinator<DataModel.FinanceContext> persistence) : ICartaASaldoService
    {
        public async Task<CartaASaldoDto> Configure(string contoName, ConfigureCartaASaldoRequest request)
        {
            CartaASaldoConfiguration.Validate(request);
            Conto carta = await contoRepository.GetByName(contoName)
                ?? throw new KeyNotFoundException($"Conto '{contoName}' was not found.");
            Conto contoAddebito = await contoRepository.GetByName(request.ContoAddebitoName)
                ?? throw new KeyNotFoundException($"Conto '{request.ContoAddebitoName}' was not found.");

            if (carta.Id == contoAddebito.Id)
            {
                throw new ArgumentException("The debit account must differ from the card account.", nameof(request));
            }

            await using IApplicationOperation operation = new ApplicationOperation(await persistence.BeginTransaction());
            await ConfigureParameters(carta, request);
            Categoria technicalCategory = await GetOrCreateTechnicalCategory();
            await repository.Save();

            string debitDescription = $"Addebito {carta.DisplayName}";
            string resetDescription = $"Ripristino plafond {carta.DisplayName}";
            IReadOnlyList<Pianificazione> persistedPlans = await repository.GetPlans(debitDescription, resetDescription);
            (Pianificazione debitPlan, Pianificazione resetPlan, bool created) = persistedPlans.Count == 0
                ? await CreatePlans(carta, contoAddebito, technicalCategory, request, debitDescription, resetDescription)
                : await ValidateExistingPlans(carta, contoAddebito, technicalCategory, request, persistedPlans, debitDescription, resetDescription);

            await operation.Complete();

            return new CartaASaldoDto(carta.Name, debitPlan.Id, resetPlan.Id, created);
        }

        private async Task ConfigureParameters(Conto carta, ConfigureCartaASaldoRequest request)
        {
            IReadOnlyList<IReadOnlyList<ParametroConto>> existing = await parametroService.GetAll(carta.Name);
            Dictionary<string, IReadOnlyList<ParametroConto>> byName = existing.ToDictionary(item => item[0].Name, StringComparer.OrdinalIgnoreCase);
            int existingConventionalCount = CartaASaldoConfiguration.Parameters.Count(parameter => byName.ContainsKey(parameter.Name));

            if (existingConventionalCount is > 0 and < 5)
            {
                throw new CartaASaldoConflictException("The card account has a partial conventional parameter profile.");
            }

            foreach (ParametroCartaASaldoConfiguration parameter in CartaASaldoConfiguration.Parameters)
            {
                decimal value = CartaASaldoConfiguration.GetValue(parameter.Name, request);

                if (!byName.TryGetValue(parameter.Name, out IReadOnlyList<ParametroConto>? definitions))
                {
                    await parametroService.Create(carta.Name, parameter.Name, parameter.Type,
                        [new ParametroContoDefinitionRequest(null, parameter.DisplayName, value, null, null)]);
                    continue;
                }

                if (definitions.Any(definition => definition.Type != parameter.Type))
                {
                    throw new CartaASaldoConflictException($"Account parameter '{parameter.Name}' has an incompatible type.");
                }

                ParametroConto[] permanent = [.. definitions.Where(definition => definition.ValidFrom is null && definition.ValidTo is null)];
                if (permanent.Length != 1)
                {
                    throw new CartaASaldoConflictException($"Account parameter '{parameter.Name}' does not have one permanent definition.");
                }

                await parametroService.Update(carta.Name, parameter.Name, [.. definitions.Select(definition => new ParametroContoDefinitionRequest(
                    definition.Id,
                    definition.Id == permanent[0].Id ? parameter.DisplayName : definition.DisplayName,
                    definition.Id == permanent[0].Id ? value : definition.Value,
                    definition.ValidFrom,
                    definition.ValidTo))]);
            }
        }

        private async Task<Categoria> GetOrCreateTechnicalCategory()
        {
            (Categoria Categoria, CategoriaUsage Usage)? existing = await categoriaService.GetByName("Tecnico");

            return existing?.Categoria ?? await categoriaService.Create("Tecnico", "Tecnico");
        }

        private async Task<(Pianificazione Debit, Pianificazione Reset, bool Created)> CreatePlans(
            Conto carta,
            Conto contoAddebito,
            Categoria technicalCategory,
            ConfigureCartaASaldoRequest request,
            string debitDescription,
            string resetDescription)
        {
            string formula = $"-[{carta.Name}.{FormulaResolver.SaldoUltimoCicloChiuso}]";
            CreatePianificazioneDto debit = await pianificazioneService.Create(new CreatePianificazioneRequest(
                contoAddebito.Name,
                debitDescription,
                carta.DisplayName,
                formula,
                request.ValidFrom,
                request.ValidTo,
                1,
                request.Addebito,
                false));
            await repository.Save();
            CreatePianificazioneDto reset = await pianificazioneService.Create(new CreatePianificazioneRequest(
                carta.Name,
                resetDescription,
                resetDescription,
                formula,
                request.ValidFrom,
                request.ValidTo,
                1,
                request.RipristinoPlafond,
                false,
                ModalitaCategoria.Esplicita,
                technicalCategory.Name));
            await repository.Save();
            await repository.CreateCorrelation(debit.Id, reset.Id);
            Pianificazione[] plans = [.. await repository.GetPlans(debitDescription, resetDescription)];

            return (Find(plans, debitDescription), Find(plans, resetDescription), true);
        }

        private async Task<(Pianificazione Debit, Pianificazione Reset, bool Created)> ValidateExistingPlans(
            Conto carta,
            Conto contoAddebito,
            Categoria technicalCategory,
            ConfigureCartaASaldoRequest request,
            IReadOnlyList<Pianificazione> plans,
            string debitDescription,
            string resetDescription)
        {
            if (plans.Count != 2)
            {
                throw new CartaASaldoConflictException("The card account has a partial or ambiguous bootstrap plan pair.");
            }

            Pianificazione debit = Find(plans, debitDescription);
            Pianificazione reset = Find(plans, resetDescription);
            string formula = $"-[{carta.Name}.{FormulaResolver.SaldoUltimoCicloChiuso}]";

            bool compatible = Matches(debit, contoAddebito.Id, carta.DisplayName, formula, request.ValidFrom, request.ValidTo, request.Addebito,
                    ModalitaCategoria.Nessuna, null)
                && Matches(reset, carta.Id, resetDescription, formula, request.ValidFrom, request.ValidTo, request.RipristinoPlafond,
                    ModalitaCategoria.Esplicita, technicalCategory.Id)
                && await repository.GetCorrelation(debit.Id, reset.Id) is not null;

            return compatible ? (debit, reset, false)
                : throw new CartaASaldoConflictException("The existing card bootstrap is incompatible with the requested configuration.");
        }

        private static bool Matches(
            Pianificazione plan,
            Guid contoId,
            string movimentoDescription,
            string formula,
            DateOnly validFrom,
            DateOnly validTo,
            int day,
            ModalitaCategoria categoryMode,
            Guid? categoryId)
            => plan.ContoId == contoId
                && plan.MovimentoDescription == movimentoDescription
                && plan.MovimentoFormula == formula
                && plan.ValidFrom == validFrom
                && plan.ValidTo == validTo
                && plan.ModalitaCategoria == categoryMode
                && plan.CategoriaId == categoryId
                && plan.Periodicita.Frequenza == FrequenzaPeriodicita.Mensile
                && plan.Periodicita.Intervallo == 1
                && plan.Periodicita.GiornoMese == day
                && !plan.Periodicita.FineMese;

        private static Pianificazione Find(IEnumerable<Pianificazione> plans, string description)
            => plans.SingleOrDefault(plan => plan.Description == description)
                ?? throw new CartaASaldoConflictException($"Plan '{description}' was not found in the expected pair.");

    }
}
