using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public interface ITariffarioService
    {
        Task<IApplicationOperation> BeginOperation();
        Task<IReadOnlyList<Casello>> GetCaselli();
        Task<Casello> CreateCasello(string name);
        Task<Casello> UpdateCasello(Guid id, string name);
        Task<IReadOnlyList<TariffaTratta>> GetTariffe();
        Task<IReadOnlyList<TariffaTratta>> GetTratta(Guid entrataId, Guid uscitaId);
        Task<IReadOnlyList<TariffaTratta>> SaveTratta(Guid entrataId, Guid uscitaId, IReadOnlyList<TariffaTrattaDefinitionRequest> definitions);
    }
}
