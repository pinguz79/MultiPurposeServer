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
    public class TariffaController(ITariffarioService service) : FinanceBackEndControllerBase
    {
        [HttpGet("List")]
        public async Task<IActionResult> GetList() => Ok((await service.GetTariffe()).Select(tariffa => new TariffaTrattaDto(tariffa)).ToList());

        [HttpGet("{entrataId:guid}/{uscitaId:guid}")]
        public async Task<IActionResult> Get(Guid entrataId, Guid uscitaId)
        {
            try
            {
                return Ok((await service.GetTratta(entrataId, uscitaId)).Select(tariffa => new TariffaTrattaDto(tariffa)).ToList());
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Tratta non valida", Detail = exception.Message });
            }
        }

        [HttpPatch("{entrataId:guid}/{uscitaId:guid}")]
        public Task<IActionResult> Update(Guid entrataId, Guid uscitaId, [FromBody] SaveTariffaTrattaRequest request) => Save(entrataId, uscitaId, request, false);

        [HttpPost("{entrataId:guid}/{uscitaId:guid}")]
        public Task<IActionResult> Create(Guid entrataId, Guid uscitaId, [FromBody] SaveTariffaTrattaRequest request) => Save(entrataId, uscitaId, request, true);

        [HttpDelete("{entrataId:guid}/{uscitaId:guid}")]
        public async Task<IActionResult> Delete(Guid entrataId, Guid uscitaId)
        {
            try
            {
                await using var operation = await service.BeginOperation();
                bool deleted = await service.DeleteTratta(entrataId, uscitaId);
                await operation.Complete();
                return deleted ? NoContent() : NotFound();
            }
            catch (TariffarioInUseException exception)
            {
                return Conflict(new ProblemDetails { Title = "Tratta in uso", Detail = exception.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Tratta non valida", Detail = exception.Message });
            }
        }

        private async Task<IActionResult> Save(Guid entrataId, Guid uscitaId, SaveTariffaTrattaRequest request, bool create)
        {
            try
            {
                await using IApplicationOperation operation = await service.BeginOperation();
                bool exists = (await service.GetTratta(entrataId, uscitaId)).Count > 0;
                if (create && exists)
                {
                    return Conflict(new ProblemDetails { Title = "Tratta già presente" });
                }

                if (!create && !exists)
                {
                    return NotFound();
                }

                IReadOnlyList<TariffaTratta> definitions = await service.SaveTratta(entrataId, uscitaId, request.Definitions);
                await operation.Complete();
                var result = definitions.Select(tariffa => new TariffaTrattaDto(tariffa)).ToList();
                return create ? StatusCode(201, result) : Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Tariffa non valida", Detail = exception.Message });
            }
        }
    }
}
