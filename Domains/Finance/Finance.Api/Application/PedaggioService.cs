using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public class PedaggioService(
        IPedaggioRepository repository,
        ITariffarioRepository tariffarioRepository,
        ITariffarioService tariffarioService,
        IMovimentoService movimentoService,
        IContoRepository contoRepository,
        IParametroContoService parametroService,
        IFormulaEvaluator evaluator) : IPedaggioService
    {
        public Task<Pedaggio?> Get(Guid movimentoId) => repository.GetByMovimento(movimentoId);

        public async Task<Pedaggio> Create(SavePedaggioRequest request)
        {
            Conto conto = await contoRepository.GetByName(request.ContoName) ?? throw new KeyNotFoundException("Conto non trovato.");
            ParametroConto? enabled = await parametroService.Resolve(conto.Id, "AbilitaPedaggi", DateOnly.FromDateTime(DateTime.Today));
            if (enabled is null || enabled.Type != TipoParametroConto.Booleano || enabled.Value != 1m)
            {
                throw new ArgumentException("Il conto non è abilitato ai pedaggi.");
            }

            string formula = await PrepareFormula(request);
            string description = await GetDescription(request);
            MovimentoConfigurationDto movimento = await movimentoService.Create(new SaveMovimentoRequest(request.ContoName, request.Date, description, formula, request.IsConfirmed, request.CategoryName));
            var pedaggio = new Pedaggio { MovimentoId = movimento.Id, CaselloEntrataId = request.EntrataId, CaselloUscitaId = request.UscitaId };
            await repository.Save(pedaggio);
            return pedaggio;
        }

        public async Task<Pedaggio> Update(Guid movimentoId, SavePedaggioRequest request)
        {
            Pedaggio pedaggio = await repository.GetByMovimento(movimentoId) ?? throw new KeyNotFoundException("Pedaggio non trovato.");
            if (!string.Equals(pedaggio.Movimento.Conto.Name, request.ContoName, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Non è possibile trasferire il pedaggio a un altro conto.");
            }

            string formula = await PrepareFormula(request with { IsConfirmed = request.IsConfirmed && !pedaggio.Movimento.IsConfirmed });
            pedaggio.CaselloEntrataId = request.EntrataId;
            pedaggio.CaselloUscitaId = request.UscitaId;
            await repository.Save(pedaggio);
            await movimentoService.Update(movimentoId, request.Date, await GetDescription(request), pedaggio.Movimento.IsConfirmed ? null : formula,
                request.CategoryName, request.CategoryName is null ? true : null);
            if (pedaggio.Movimento.IsConfirmed != request.IsConfirmed)
            {
                await movimentoService.SetConfirmation(movimentoId, request.IsConfirmed);
            }

            return pedaggio;
        }

        private async Task<string> PrepareFormula(SavePedaggioRequest request)
        {
            if ((await tariffarioService.GetTratta(request.EntrataId, request.UscitaId)).Count == 0)
            {
                await tariffarioService.SaveTratta(request.EntrataId, request.UscitaId, [new TariffaTrattaDefinitionRequest(0m)]);
            }

            string formula = PedaggioFormula.Create(request.EntrataId, request.UscitaId);
            if (request.IsConfirmed)
            {
                FormulaEvaluationResult result = await evaluator.Evaluate(formula, request.Date);
                if (result.Error is not null || result.Value is null or 0m)
                {
                    throw new ArgumentException("Tariffa non disponibile: salvare il pedaggio da confermare.");
                }
            }

            return formula;
        }

        private async Task<string> GetDescription(SavePedaggioRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                return request.Description.Trim();
            }

            Casello entrata = await tariffarioRepository.GetCasello(request.EntrataId) ?? throw new KeyNotFoundException("Entrata non trovata.");
            Casello uscita = await tariffarioRepository.GetCasello(request.UscitaId) ?? throw new KeyNotFoundException("Uscita non trovata.");
            return $"{entrata.Name} → {uscita.Name}";
        }
    }
}
