using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;

using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Controllers.BackEnd
{
    [Route("Finance/BackEnd/[controller]")]
    [ApiController]
    public class GruppoMovimentiController(
        ITrasferimentoService service,
        IMovimentoService movimenti) : FinanceBackEndControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                return Ok(new GruppoMovimentiDto(await service.ResolveGroup(id)));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGruppoMovimentiRequest request)
        {
            try
            {
                await using IApplicationOperation operation = await movimenti.BeginOperation();
                if (request.Items is null || request.Items.Any(item => item is null || item.Changes is null || item.Changes.IsConfirmed is not null))
                {
                    throw new ArgumentException("Specificare modifiche valide; per la conferma usare Movimento/Consolida.");
                }
                await service.ValidateSelection(id, request.Items.Select(item => item.Id).ToList());
                foreach (UpdateMovimentoCorrelatoItem item in request.Items)
                {
                    UpdateMovimentoRequest change = item.Changes;
                    if (change.Date is null && change.Description is null && change.Formula is null && change.CategoryName is null
                        && change.ClearCategory is null && change.Natura is null)
                    {
                        throw new ArgumentException("Specificare almeno una proprietà da modificare.");
                    }
                    if (change.Description is not null && string.IsNullOrWhiteSpace(change.Description))
                    {
                        throw new ArgumentException("La descrizione non può essere vuota.");
                    }
                    await movimenti.Update(item.Id, change.Date, change.Description, change.Formula, change.CategoryName, change.ClearCategory, change.Natura);
                }
                await operation.Complete();
                return Ok(new GruppoMovimentiDto(await service.ResolveGroup(id)));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid group update", Detail = exception.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, [FromBody] DeleteGruppoMovimentiRequest request)
        {
            try
            {
                await using IApplicationOperation operation = await movimenti.BeginOperation();
                await service.ValidateSelection(id, request.Ids);
                foreach (Guid movementId in request.Ids)
                {
                    await movimenti.Delete(movementId);
                }
                await operation.Complete();
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid group selection", Detail = exception.Message });
            }
        }
    }
}
