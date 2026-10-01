using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;

using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers.BackEnd
{
    [Route("Finance/BackEnd/[controller]")]
    [ApiController]
    public class FinanziamentoController(IFinanziamentoService service) : FinanceBackEndControllerBase
    {
        [HttpGet("List")]
        public async Task<IActionResult> GetList([FromQuery] bool includeClosed = false)
            => Ok((await service.GetAll(includeClosed)).Select(loan => new FinanziamentoDto(loan)).ToList());

        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            try
            {
                return Ok(new FinanziamentoDto(await service.Resolve(name)));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFinanziamentoRequest request)
        {
            try
            {
                var loan = await service.Create(request);
                return CreatedAtAction(nameof(Get), new { name = loan.Name }, new FinanziamentoDto(loan));
            }
            catch (DuplicateNameException exception)
            {
                return Conflict(new ProblemDetails { Title = "Name already exists", Detail = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid loan", Detail = exception.Message });
            }
        }

        [HttpPatch("{name}")]
        public async Task<IActionResult> Update(string name, [FromBody] UpdateFinanziamentoRequest request)
        {
            try
            {
                return Ok(new FinanziamentoDto(await service.Update(name, request)));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (FinanziamentoLockedException exception)
            {
                return Conflict(new ProblemDetails { Title = "Contractual data is locked", Detail = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid loan", Detail = exception.Message });
            }
        }

        [HttpPost("{name}/Riallineamento/{number:int}")]
        public Task<IActionResult> CreateAlignment(string name, int number, [FromBody] SaveRiallineamentoRequest request) => SaveAlignment(name, number, request, true);

        [HttpPatch("{name}/Riallineamento/{number:int}")]
        public Task<IActionResult> UpdateAlignment(string name, int number, [FromBody] SaveRiallineamentoRequest request) => SaveAlignment(name, number, request, false);

        [HttpDelete("{name}/Riallineamento/{number:int}")]
        public async Task<IActionResult> DeleteAlignment(string name, int number)
        {
            try
            {
                await service.DeleteAlignment(name, number);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        private async Task<IActionResult> SaveAlignment(string name, int number, SaveRiallineamentoRequest request, bool create)
        {
            try
            {
                await service.SaveAlignment(name, number, request.Principal, DateOnly.FromDateTime(DateTime.Today), create);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DuplicateNameException exception)
            {
                return Conflict(new ProblemDetails { Title = "Alignment already exists", Detail = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid alignment", Detail = exception.Message });
            }
        }
    }
}
