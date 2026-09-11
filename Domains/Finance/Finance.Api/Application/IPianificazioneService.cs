using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public interface IPianificazioneService
    {
        Task<IApplicationOperation> BeginOperation();
        Task<Pianificazione?> Get(Guid id);
        Task<IReadOnlyList<Pianificazione>> GetList(string? contoName);
        Task<bool> Delete(Guid id, bool deleteMovimenti);
        Task<CreatePianificazioneDto> Create(CreatePianificazioneRequest request);
        Task<PianificazionePreviewDto> Preview(CreatePianificazioneRequest request);
    }
}
