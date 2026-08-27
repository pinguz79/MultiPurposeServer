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
    public class VoceRicorrenteController(IVoceRicorrenteService service) : FinanceBackEndControllerBase
    {
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            try
            {
                await service.Delete(name);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            IReadOnlyList<VoceRicorrente> definitions = await service.GetByName(name);

            return definitions.Count == 0 ? NotFound() : Ok(Map(definitions));
        }

        [HttpGet("List")]
        public async Task<IReadOnlyList<VoceRicorrenteDto>> GetList()
            => [.. (await service.GetAll()).Select(Map).OrderBy(voce => voce.Name, StringComparer.OrdinalIgnoreCase)];

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVoceRicorrenteRequest request)
        {
            try
            {
                IReadOnlyList<VoceRicorrente> definitions = await service.Create(request.Name, request.Definitions);

                return CreatedAtAction(nameof(Get), new { name = definitions[0].Name }, Map(definitions));
            }
            catch (DuplicateNameException exception)
            {
                return Conflict(new ProblemDetails { Title = "Name already exists", Detail = exception.Message, Extensions = { ["field"] = "Name" } });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid recurring entry", Detail = exception.Message });
            }
        }

        [HttpPatch("{name}")]
        public async Task<IActionResult> Update(string name, [FromBody] UpdateVoceRicorrenteRequest request)
        {
            try
            {
                await using IApplicationOperation operation = await service.BeginOperation();
                IReadOnlyList<VoceRicorrente> definitions = await service.Update(name, request.Name, request.Definitions);
                await operation.Complete();

                return Ok(Map(definitions));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DuplicateNameException exception)
            {
                return Conflict(new ProblemDetails { Title = "Name already exists", Detail = exception.Message, Extensions = { ["field"] = "Name" } });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid recurring entry", Detail = exception.Message });
            }
        }

        private static VoceRicorrenteDto Map(IReadOnlyList<VoceRicorrente> definitions)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");
            DateOnly today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));

            return new VoceRicorrenteDto(definitions, today);
        }
    }
}
