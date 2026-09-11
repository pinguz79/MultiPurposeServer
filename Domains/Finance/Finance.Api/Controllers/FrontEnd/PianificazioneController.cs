using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;

using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Controllers.FrontEnd
{
    [Route("Finance/FrontEnd/[controller]")]
    [ApiController]
    public class PianificazioneController(IPianificazioneService service) : FinanceFrontEndControllerBase
    {
        [HttpGet("List")]
        public async Task<IActionResult> GetList([FromQuery] string? contoName = null) => Ok((await service.GetList(contoName)).Select(item => new PianificazioneDto(item)).ToList());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var pianificazione = await service.Get(id);
            return pianificazione is null ? NotFound() : Ok(new PianificazioneDto(pianificazione));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool deleteMovimenti = false)
        {
            await using IApplicationOperation operation = await service.BeginOperation();
            bool deleted = await service.Delete(id, deleteMovimenti);
            await operation.Complete();
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePianificazioneRequest request)
        {
            try
            {
                await using IApplicationOperation operation = await service.BeginOperation();
                var result = await service.Create(request);
                await operation.Complete();

                return Created($"/Finance/FrontEnd/Pianificazione/{result.Id}", result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid planning", Detail = exception.Message });
            }
        }

        [HttpPost("Preview")]
        public async Task<IActionResult> Preview([FromBody] CreatePianificazioneRequest request)
        {
            try
            {
                return Ok(await service.Preview(request));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid planning", Detail = exception.Message });
            }
        }
    }
}
