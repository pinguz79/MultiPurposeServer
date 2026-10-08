using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers.BackEnd
{
    [Route("Finance/BackEnd/[controller]")]
    [ApiController]
    public class CaselloController(ITariffarioService service) : FinanceBackEndControllerBase
    {
        [HttpGet("List")]
        public async Task<IActionResult> GetList() => Ok((await service.GetCaselli()).Select(casello => new CaselloDto(casello)).ToList());

        [HttpPost]
        public Task<IActionResult> Create([FromBody] SaveCaselloRequest request) => Save(null, request);

        [HttpPatch("{id:guid}")]
        public Task<IActionResult> Update(Guid id, [FromBody] SaveCaselloRequest request) => Save(id, request);

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await using var operation = await service.BeginOperation();
                bool deleted = await service.DeleteCasello(id);
                await operation.Complete();
                return deleted ? NoContent() : NotFound();
            }
            catch (TariffarioInUseException exception)
            {
                return Conflict(new ProblemDetails { Title = "Stazione in uso", Detail = exception.Message });
            }
        }

        private async Task<IActionResult> Save(Guid? id, SaveCaselloRequest request)
        {
            try
            {
                Casello casello = id is null ? await service.CreateCasello(request.Name) : await service.UpdateCasello(id.Value, request.Name);
                return id is null ? StatusCode(201, new CaselloDto(casello)) : Ok(new CaselloDto(casello));
            }
            catch (DuplicateNameException exception)
            {
                return Conflict(new ProblemDetails { Title = "Stazione già presente", Detail = exception.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Stazione non valida", Detail = exception.Message });
            }
        }
    }
}
