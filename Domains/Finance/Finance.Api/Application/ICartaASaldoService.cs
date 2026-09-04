using Finance.Contracts.Requests;
using Finance.Contracts.Responses;

namespace Finance.Api.Application
{
    public interface ICartaASaldoService
    {
        Task<CartaASaldoDto> Configure(string contoName, ConfigureCartaASaldoRequest request);
    }
}
