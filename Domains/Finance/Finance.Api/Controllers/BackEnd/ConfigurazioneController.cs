using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Controllers.BackEnd
{
    [Route("Finance/BackEnd/Conto/{contoName}/[controller]")]
    [ApiController]
    public class ConfigurazioneController(
        ICartaASaldoService service,
        ICartaRevolvingService revolvingService) : FinanceBackEndControllerBase
    {
        [HttpPost("CartaRevolving")]
        public async Task<IActionResult> ConfigureCartaRevolving(string contoName, [FromBody] ConfigureCartaRevolvingRequest request)
        {
            try
            {
                await using IApplicationOperation operation = await revolvingService.BeginOperation();
                CartaRevolvingDto result = await revolvingService.Configure(contoName, request);
                await operation.Complete();
                return result.Created ? StatusCode(StatusCodes.Status201Created, result) : Ok(result);
            }
            catch (CartaRevolvingConflictException exception)
            {
                return Conflict(new ProblemDetails { Title = "Configurazione revolving incompatibile", Detail = exception.Message });
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new ProblemDetails { Title = "Conto non trovato", Detail = exception.Message });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Configurazione revolving non valida", Detail = exception.Message });
            }
        }

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
