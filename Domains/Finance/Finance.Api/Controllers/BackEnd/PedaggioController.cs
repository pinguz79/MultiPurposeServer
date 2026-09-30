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
    public class PedaggioController(
        IPedaggioService service,
        IMovimentoService movimentoService) : FinanceBackEndControllerBase
    {
        [HttpGet("{movimentoId:guid}")]
        public async Task<IActionResult> Get(Guid movimentoId)
        {
            Pedaggio? pedaggio = await service.Get(movimentoId);
            return pedaggio is null ? NotFound() : Ok(new PedaggioDto(pedaggio));
        }

        [HttpPost]
        public Task<IActionResult> Create([FromBody] SavePedaggioRequest request) => Save(null, request);

        [HttpPatch("{movimentoId:guid}")]
        public Task<IActionResult> Update(Guid movimentoId, [FromBody] SavePedaggioRequest request) => Save(movimentoId, request);

        private async Task<IActionResult> Save(Guid? id, SavePedaggioRequest request)
        {
            try
            {
                await using IApplicationOperation operation = await movimentoService.BeginOperation();
                Pedaggio pedaggio = id is null ? await service.Create(request) : await service.Update(id.Value, request);
                await operation.Complete();
                return id is null ? StatusCode(201, new PedaggioDto(pedaggio)) : Ok(new PedaggioDto(pedaggio));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Pedaggio non valido", Detail = exception.Message });
            }
        }
    }
}
