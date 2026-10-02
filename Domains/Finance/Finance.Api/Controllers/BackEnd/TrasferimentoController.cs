using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Controllers.BackEnd
{
    [Route("Finance/BackEnd/[controller]")]
    [ApiController]
    public class TrasferimentoController(
        ITrasferimentoService service,
        IMovimentoService movimenti) : FinanceBackEndControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTrasferimentoRequest request)
        {
            try
            {
                await using IApplicationOperation operation = await movimenti.BeginOperation();
                IReadOnlyList<Movimento> items = await service.Create(request);
                await operation.Complete();
                return StatusCode(201, items.Select(item => new MovimentoConfigurationDto(item)).ToList());
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid transfer", Detail = exception.Message });
            }
        }
    }
}
