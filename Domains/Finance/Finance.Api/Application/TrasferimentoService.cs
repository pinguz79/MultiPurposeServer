using System.Globalization;

using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public class TrasferimentoService(
        IContoRepository conti,
        IMovimentoService movimenti,
        IMovimentoRepository repository,
        IGruppoMovimentiRepository groups,
        IParametroContoService parameters) : ITrasferimentoService
    {
        public async Task<IReadOnlyList<Movimento>> Create(CreateTrasferimentoRequest request)
        {
            if (request.Amount <= 0 || request.Amount > long.MaxValue / 100m || decimal.Round(request.Amount, 2) != request.Amount
                || string.IsNullOrWhiteSpace(request.Description))
            {
                throw new ArgumentException("Importo positivo con massimo due decimali e descrizione obbligatoria.");
            }
            Conto origin = await conti.GetByName(ContoService.NormalizeName(request.OrigineName)) ?? throw new KeyNotFoundException("Conto origine non trovato.");
            Conto destination = await conti.GetByName(ContoService.NormalizeName(request.DestinazioneName)) ?? throw new KeyNotFoundException("Conto destinazione non trovato.");
            if (origin.Id == destination.Id)
            {
                throw new ArgumentException("Origine e destinazione devono essere diverse.");
            }
            decimal originAmount = await IsDebtAccount(origin.Id, request.Date) ? request.Amount : -request.Amount;
            decimal destinationAmount = await IsDebtAccount(destination.Id, request.Date) ? -request.Amount : request.Amount;
            Movimento first = await movimenti.Create(origin.Id, request.Date, request.Description, originAmount.ToString("0.00", CultureInfo.InvariantCulture));
            Movimento second = await movimenti.Create(destination.Id, request.Date, request.Description, destinationAmount.ToString("0.00", CultureInfo.InvariantCulture));
            first.Conto = origin;
            second.Conto = destination;
            if (request.IsConfirmed)
            {
                await repository.SetConfirmation(first.Id, true);
                await repository.SetConfirmation(second.Id, true);
            }
            else
            {
                await groups.Create([first, second]);
            }
            return [first, second];
        }

        public async Task<GruppoMovimenti> ResolveGroup(Guid id) => await groups.GetById(id) ?? throw new KeyNotFoundException("Gruppo non trovato.");

        public async Task ValidateSelection(Guid groupId, IReadOnlyList<Guid> ids)
        {
            if (ids is null || ids.Count == 0 || ids.Any(id => id == Guid.Empty) || ids.Distinct().Count() != ids.Count)
            {
                throw new ArgumentException("Selezionare movimenti distinti e non vuoti.");
            }
            GruppoMovimenti group = await ResolveGroup(groupId);
            if (ids.Any(id => !group.Movimenti.Any(item => item.Id == id)))
            {
                throw new ArgumentException("La selezione contiene movimenti non appartenenti al gruppo.");
            }
        }

        private async Task<bool> IsDebtAccount(Guid id, DateOnly date)
            => await ContoSignConvention.IsDebtAccount(parameters, id, date);
    }
}
