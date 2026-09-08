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
    public class MovimentoController(IMovimentoService service) : FinanceBackEndControllerBase
    {
        [HttpPost("Consolida")]
        public async Task<IActionResult> Consolidate()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            try
            {
                await using IApplicationOperation operation = await service.BeginOperation();
                int count = await service.Consolidate(today);
                await operation.Complete();

                return Ok(new ConsolidamentoMovimentiDto(count));
            }
            catch (FormulaEvaluationException exception)
            {
                return UnprocessableEntity(new FormulaEvaluationErrorDto(exception));
            }
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMovimentoRequest request)
        {
            try
            {
                Movimento movimento = await service.Update(
                    id,
                    request.Date,
                    request.Description,
                    request.Formula,
                    request.CategoryName,
                    request.ClearCategory);

                return Ok(new MovimentoConfigurationDto(movimento));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid movement", Detail = exception.Message });
            }
        }
    }
}
