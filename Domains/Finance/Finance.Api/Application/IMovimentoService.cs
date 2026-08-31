using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public interface IMovimentoService
    {
        Task<IApplicationOperation> BeginOperation();
        Task<Movimento> Create(Guid contoId, DateOnly date, string description, string formula);
        Task<ContoMovimentiDto> GetTimeline(string contoName, int month, int year);
        Task<Movimento> Update(
            Guid id,
            DateOnly? date,
            string? description,
            string? formula,
            string? categoryName = null,
            bool? clearCategory = null);
    }
}
