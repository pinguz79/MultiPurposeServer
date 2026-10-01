using Finance.Api.Application;
using Finance.Contracts.Responses;

using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers.FrontEnd
{
    [Route("Finance/FrontEnd/[controller]")]
    [ApiController]
    public class FinanziamentoController(IFinanziamentoService service) : FinanceFrontEndControllerBase
    {
        [HttpGet("List")]
        public async Task<IActionResult> GetList() => Ok((await service.GetAll()).Select(loan => new FinanziamentoDto(loan)).ToList());

        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            try
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                var loan = await service.Resolve(name);
                return Ok(new PianoFinanziamentoDto(loan, FinanziamentoCalculator.Calculate(loan, today)));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
