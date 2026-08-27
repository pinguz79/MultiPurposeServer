using Finance.Api.Application;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers.FrontEnd
{
    [Route("Finance/FrontEnd/[controller]")]
    [ApiController]
    public class ContoController(IContoService contoService) : FinanceFrontEndControllerBase
    {
        [HttpGet("{contoId:guid}")]
        public async Task<IActionResult> Get(Guid contoId)
        {
            var conto = await contoService.GetById(contoId);

            try
            {
                return conto is null ? NotFound() : Ok(new ContoDto(conto));
            }
            catch (FormulaEvaluationException exception)
            {
                return UnprocessableEntity(new FormulaEvaluationErrorDto(exception));
            }
        }

        [HttpGet("List")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                return Ok((await contoService.GetConti()).Select(conto => new ContoDto(conto)).ToList());
            }
            catch (FormulaEvaluationException exception)
            {
                return UnprocessableEntity(new FormulaEvaluationErrorDto(exception));
            }
        }
    }
}
