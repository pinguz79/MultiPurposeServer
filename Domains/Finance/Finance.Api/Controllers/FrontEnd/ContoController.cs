using Finance.Api.Application;
using Finance.Contracts.Responses;

using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers.FrontEnd
{
    [Route("Finance/FrontEnd/[controller]")]
    [ApiController]
    public class ContoController(IContoService contoService) : FinanceFrontEndControllerBase
    {
        [HttpGet("{contoId:guid}")]
        public async Task<IActionResult> Get(Guid contoId)
        {
            var conto = await contoService.GetById(contoId);

            return conto is null ? NotFound() : Ok(new ContoDto(conto));
        }

        [HttpGet("Conti")]
        public async Task<IActionResult> GetConti() => Ok((await contoService.GetConti()).Select(conto => new ContoDto(conto)).ToList());
    }
}
