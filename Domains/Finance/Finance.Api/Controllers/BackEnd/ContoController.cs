using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers.BackEnd
{
    [ApiController]
    [Route("Finance/BackEnd/[controller]")]
    public sealed class ContoController(IContoService service) : FinanceBackEndControllerBase
    {
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateContoRequest request)
        {
            try
            {
                var conto = await service.Create(request.Name, request.DisplayName, request.InitialBalance);

                return CreatedAtAction(nameof(Get), new { contoId = conto.Id }, MapConfiguration(conto));
            }
            catch (DuplicateNameException exception)
            {
                return Conflict(new ProblemDetails { Title = "Name already exists", Detail = exception.Message, Extensions = { ["field"] = "Name" } });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid Conto", Detail = exception.Message });
            }
        }

        [HttpGet("{contoId:guid}")]
        public async Task<IActionResult> Get(Guid contoId)
        {
            var conto = await service.Get(contoId);

            return conto is null ? NotFound() : Ok(MapConfiguration(conto));
        }

        [HttpPatch("{contoId:guid}")]
        public async Task<IActionResult> Update(Guid contoId, [FromBody] UpdateContoRequest request)
        {
            try
            {
                return Ok(MapConfiguration(await service.Update(contoId, request.DisplayName, request.InitialBalance)));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid Conto", Detail = exception.Message });
            }
        }

        private static ContoConfigurationDto MapConfiguration(Conto conto) => new(conto.Id, conto.Name, conto.DisplayName, conto.InitialBalance, conto.Balance);
    }
}
