using Finance.Contracts.Requests;
using Finance.Contracts.Responses;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Application
{
    public interface ICartaRevolvingService
    {
        Task<IApplicationOperation> BeginOperation();
        Task<CartaRevolvingDto> Configure(string contoName, ConfigureCartaRevolvingRequest request);
    }
}
