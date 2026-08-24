using Finance.Api.Application;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers.FrontEnd
{
    [ApiController]
    [Route("Finance/FrontEnd/[controller]")]
    public sealed class ContoController(IContoService service) : FinanceFrontEndControllerBase
    {
        [HttpGet("{contoId:guid}")]
        public async Task<IActionResult> Get(Guid contoId)
        {
            var conto = await service.Get(contoId);

            return conto is null ? NotFound() : Ok(Map(conto));
        }

        [HttpGet("List")]
        public async Task<IReadOnlyList<ContoDto>> GetList() => [.. (await service.GetAll()).Select(Map)];

        private static ContoDto Map(Conto conto) => new(conto.Id, conto.Name, conto.DisplayName, conto.Balance);
    }
}
