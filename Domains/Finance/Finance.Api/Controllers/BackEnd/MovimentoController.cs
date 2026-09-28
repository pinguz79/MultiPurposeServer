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
        public async Task<IActionResult> Consolidate([FromBody] ConfirmMovimentiRequest request)
        {
            try
            {
                await using IApplicationOperation operation = await service.BeginOperation();
                int count = await service.Confirm(request.Ids);
                await operation.Complete();

                return Ok(new ConsolidamentoMovimentiDto(count));
            }
            catch (FormulaEvaluationException exception)
            {
                return UnprocessableEntity(new FormulaEvaluationErrorDto(exception));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid selection", Detail = exception.Message });
            }
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetList([FromQuery] bool pendingOnly = true, [FromQuery] string? contoName = null, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null)
        {
            if (from > to)
            {
                return BadRequest();
            }

            return Ok(await service.GetForReview(pendingOnly, contoName, from, to));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await using IApplicationOperation operation = await service.BeginOperation();
            bool deleted = await service.Delete(id);
            await operation.Complete();
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveMovimentoRequest request)
        {
            try
            {
                await using IApplicationOperation operation = await service.BeginOperation();
                MovimentoConfigurationDto saved = await service.Create(request);
                await operation.Complete();
                return StatusCode(201, saved);
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

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMovimentoRequest request)
        {
            try
            {
                await using IApplicationOperation operation = await service.BeginOperation();
                Movimento movimento = await service.Update(
                    id,
                    request.Date,
                    request.Description,
                    request.Formula,
                    request.CategoryName,
                    request.ClearCategory,
                    request.Natura);

                if (request.IsConfirmed is bool confirmed)
                {
                    await service.SetConfirmation(id, confirmed);
                }

                await operation.Complete();

                return Ok(new MovimentoConfigurationDto(movimento));
            }
            catch (FormulaEvaluationException exception)
            {
                return UnprocessableEntity(new FormulaEvaluationErrorDto(exception));
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
