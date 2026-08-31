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
                return conto is null ? NotFound() : Ok(new ContoDto(conto, await contoService.GetBalance(conto)));
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
                var result = new List<ContoDto>();

                foreach (Conto conto in await contoService.GetConti())
                {
                    result.Add(new ContoDto(conto, await contoService.GetBalance(conto)));
                }

                return Ok(result);
            }
            catch (FormulaEvaluationException exception)
            {
                return UnprocessableEntity(new FormulaEvaluationErrorDto(exception));
            }
        }
    }
}
