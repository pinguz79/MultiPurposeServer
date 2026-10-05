using Finance.Contracts.Responses;
using Finance.Contracts.Requests;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public interface IMovimentoService
    {
        Task<IApplicationOperation> BeginOperation();
        Task<int> Confirm(IReadOnlyList<Guid> ids);
        Task SetConfirmation(Guid id, bool confirmed);
        Task<IReadOnlyList<MovimentoConfigurationDto>> GetForReview(bool pendingOnly, string? contoName, DateOnly? from, DateOnly? to);
        Task<MovimentoConfigurationDto> Create(SaveMovimentoRequest request);
        Task<bool> Delete(Guid id);
        Task<Movimento> UpdateOnAccount(Guid id, UpdateMovimentoRequest request);
        Task<Movimento> Create(Guid contoId, DateOnly date, string description, string formula, NaturaMovimento natura = NaturaMovimento.Ordinario, string? categoryName = null);
        Task<ContoCicliDto> GetCurrentCycleTimeline(string contoName);
        Task<ContoCicliDto> GetCycleTimeline(string contoName, int month, int year);
        Task<ContoMovimentiDto> GetTimeline(string contoName, int month, int year);
        Task<Movimento> Update(
            Guid id,
            DateOnly? date,
            string? description,
            string? formula,
            string? categoryName = null,
            bool? clearCategory = null,
            NaturaMovimento? natura = null);
    }
}
