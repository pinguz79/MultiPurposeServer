using Finance.Contracts.Requests;
using Finance.Contracts.Responses;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public interface IPianificazioneService
    {
        Task<IApplicationOperation> BeginOperation();
        Task<CreatePianificazioneDto> Create(CreatePianificazioneRequest request);
        Task<PianificazionePreviewDto> Preview(CreatePianificazioneRequest request);
    }
}
