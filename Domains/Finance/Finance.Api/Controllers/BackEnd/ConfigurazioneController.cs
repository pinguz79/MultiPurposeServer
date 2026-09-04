using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers.BackEnd
{
    [Route("Finance/BackEnd/Conto/{contoName}/[controller]")]
    [ApiController]
    public class ConfigurazioneController(ICartaASaldoService service) : FinanceBackEndControllerBase
    {
        [HttpPost("CartaASaldo")]
        public async Task<IActionResult> ConfigureCartaASaldo(string contoName, [FromBody] ConfigureCartaASaldoRequest request)
        {
            try
            {
                CartaASaldoDto result = await service.Configure(contoName, request);

                return result.Created ? StatusCode(StatusCodes.Status201Created, result) : Ok(result);
            }
            catch (CartaASaldoConflictException exception)
            {
                return Conflict(new ProblemDetails { Title = "Incompatible card configuration", Detail = exception.Message });
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = "Account not found", Detail = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid card configuration", Detail = exception.Message });
            }
        }
    }
}
