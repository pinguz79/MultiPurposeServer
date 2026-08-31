using Finance.Api.Application;
using Finance.Contracts.Requests;

using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Controllers.FrontEnd
{
    [Route("Finance/FrontEnd/[controller]")]
    [ApiController]
    public class PianificazioneController(IPianificazioneService service) : FinanceFrontEndControllerBase
    {
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
