using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Persistence.Operations;

namespace Finance.Api.Controllers.BackEnd
{
    [Route("Finance/BackEnd/Conto/{contoName}/[controller]")]
    [ApiController]
    public class ParametroController(IParametroContoService service) : FinanceBackEndControllerBase
    {
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string contoName, string name)
        {
            try
            {
                await service.Delete(contoName, name);

                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ParametroContoReferencedException exception)
            {
                return Conflict(new ProblemDetails { Title = "Account parameter is in use", Detail = exception.Message });
            }
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> Get(string contoName, string name)
        {
            try
            {
                IReadOnlyList<ParametroConto> definitions = await service.GetByName(contoName, name);

                return definitions.Count == 0 ? NotFound() : Ok(Map(definitions));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetList(string contoName)
        {
            try
            {
                return Ok((await service.GetAll(contoName)).Select(Map).OrderBy(parametro => parametro.Name, StringComparer.OrdinalIgnoreCase));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(string contoName, [FromBody] CreateParametroContoRequest request)
        {
            try
            {
                IReadOnlyList<ParametroConto> definitions = await service.Create(contoName, request.Name, request.Type, request.Definitions);

                return CreatedAtAction(nameof(Get), new { contoName, name = definitions[0].Name }, Map(definitions));
            }
            catch (DuplicateNameException exception)
            {
                return Conflict(new ProblemDetails { Title = "Name already exists", Detail = exception.Message, Extensions = { ["field"] = "Name" } });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid account parameter", Detail = exception.Message });
            }
        }

        [HttpPatch("{name}")]
        public async Task<IActionResult> Update(string contoName, string name, [FromBody] UpdateParametroContoRequest request)
        {
            try
            {
                await using IApplicationOperation operation = await service.BeginOperation();
                IReadOnlyList<ParametroConto> definitions = await service.Update(contoName, name, request.Definitions);
                await operation.Complete();

                return Ok(Map(definitions));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid account parameter", Detail = exception.Message });
            }
        }

        private static ParametroContoDto Map(IReadOnlyList<ParametroConto> definitions)
            => new(definitions, DateOnly.FromDateTime(DateTime.Today));
    }
}
