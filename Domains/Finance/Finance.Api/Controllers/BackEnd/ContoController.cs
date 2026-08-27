using Finance.Api.Application;
using Finance.Contracts.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers.BackEnd
{
    [Route("Finance/BackEnd/[controller]")]
    [ApiController]
    public class ContoController(IContoService contoService) : FinanceBackEndControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateContoRequest request)
        {
            try
            {
                var conto = await contoService.CreateConto(request.Name, request.DisplayName, request.InitialBalance);

                return CreatedAtAction(nameof(Get), new { contoId = conto.Id }, new ContoConfigurationDto(conto));
            }
            catch (DuplicateNameException exception)
            {
                return Conflict(new ProblemDetails { Title = "Name already exists", Detail = exception.Message, Extensions = { ["field"] = "Name" } });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid Conto", Detail = exception.Message });
            }
            catch (FormulaEvaluationException exception)
            {
                return UnprocessableEntity(new FormulaEvaluationErrorDto(exception));
            }
        }

        [HttpGet("{contoId:guid}")]
        public async Task<IActionResult> Get(Guid contoId)
        {
            var conto = await contoService.GetById(contoId);

            try
            {
                return conto is null ? NotFound() : Ok(new ContoConfigurationDto(conto));
            }
            catch (FormulaEvaluationException exception)
            {
                return UnprocessableEntity(new FormulaEvaluationErrorDto(exception));
            }
        }

        [HttpPatch("{contoId:guid}")]
        public async Task<IActionResult> Update(Guid contoId, [FromBody] UpdateContoRequest request)
        {
            try
            {
                return Ok(new ContoConfigurationDto(await contoService.UpdateConto(contoId, request.Name, request.DisplayName, request.InitialBalance)));
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
                return BadRequest(new ProblemDetails { Title = "Invalid Conto", Detail = exception.Message });
            }
            catch (FormulaEvaluationException exception)
            {
                return UnprocessableEntity(new FormulaEvaluationErrorDto(exception));
            }
        }
    }
}
