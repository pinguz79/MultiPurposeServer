using System.Globalization;
using System.Text.RegularExpressions;

using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Requests;
using Finance.DataModel;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.EntityFramework;
using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public class TariffarioService(
        ITariffarioRepository repository,
        EntityFrameworkPersistenceCoordinator<FinanceContext> persistence) : ITariffarioService
    {
        public async Task<IApplicationOperation> BeginOperation() => new ApplicationOperation(await persistence.BeginTransaction());

        public Task<IReadOnlyList<Casello>> GetCaselli() => repository.GetCaselli();

        public Task<Casello> CreateCasello(string name) => SaveCasello(new Casello(), name);

        public async Task<Casello> UpdateCasello(Guid id, string name)
            => await SaveCasello(await repository.GetCasello(id) ?? throw new KeyNotFoundException("Stazione non trovata."), name);

        public Task<IReadOnlyList<TariffaTratta>> GetTariffe() => repository.GetTariffe();

        public async Task<IReadOnlyList<TariffaTratta>> GetTratta(Guid entrataId, Guid uscitaId)
        {
            (Guid a, Guid b) = await ResolvePair(entrataId, uscitaId);
            return await repository.GetTariffe(a, b);
        }

        public async Task<IReadOnlyList<TariffaTratta>> SaveTratta(Guid entrataId, Guid uscitaId, IReadOnlyList<TariffaTrattaDefinitionRequest> definitions)
        {
            if (definitions is null || definitions.Count == 0 || definitions.Count(item => item.ValidFrom is null && item.ValidTo is null) != 1
                || definitions[^1].ValidFrom is not null || definitions[^1].ValidTo is not null)
            {
                throw new ArgumentException("È richiesta una sola tariffa base senza limiti temporali, in ultima posizione.", nameof(definitions));
            }

            if (definitions.Any(item => item.ValidFrom > item.ValidTo || item.Value < 0 || decimal.Round(item.Value, 2) != item.Value))
            {
                throw new ArgumentException("Verificare le date e gli importi: non negativi e con al massimo due decimali.", nameof(definitions));
            }

            (Guid a, Guid b) = await ResolvePair(entrataId, uscitaId);
            return await repository.Replace(a, b, [.. definitions.Select(item => new TariffaTratta
            {
                Formula = item.Value.ToString("0.00", CultureInfo.InvariantCulture), ValidFrom = item.ValidFrom, ValidTo = item.ValidTo,
            })]);
        }

        private async Task<Casello> SaveCasello(Casello casello, string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            string normalized = Regex.Replace(name.Trim(), @"\s+", " ");
            return await repository.NameExists(normalized, casello.Id)
                ? throw new DuplicateNameException(normalized)
                : await repository.SaveCasello(casello, normalized);
        }

        private async Task<(Guid A, Guid B)> ResolvePair(Guid entrataId, Guid uscitaId)
        {
            if (entrataId == uscitaId)
            {
                throw new ArgumentException("Entrata e uscita devono essere diverse.");
            }

            return await repository.GetCasello(entrataId) is null || await repository.GetCasello(uscitaId) is null
                ? throw new KeyNotFoundException("Stazione non trovata.")
                : ((Guid A, Guid B))(entrataId.CompareTo(uscitaId) < 0 ? (entrataId, uscitaId) : (uscitaId, entrataId));
        }
    }
}
