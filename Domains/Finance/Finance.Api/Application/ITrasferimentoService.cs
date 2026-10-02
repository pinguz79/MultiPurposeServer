using Finance.Contracts.Requests;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public interface ITrasferimentoService
    {
        Task<IReadOnlyList<Movimento>> Create(CreateTrasferimentoRequest request);
        Task<GruppoMovimenti> ResolveGroup(Guid id);
        Task ValidateSelection(Guid groupId, IReadOnlyList<Guid> ids);
    }
}
