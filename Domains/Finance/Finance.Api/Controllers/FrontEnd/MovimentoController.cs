using Finance.Api.Application;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers.FrontEnd
{
    [Route("Finance/FrontEnd/Conto/{contoName}/[controller]")]
    [ApiController]
    public class MovimentoController(IMovimentoService movimentoService) : FinanceFrontEndControllerBase
    {
        [HttpGet("List")]
        public async Task<IActionResult> GetList(string contoName, [FromQuery] int? month, [FromQuery] int? year)
        {
            DateTime today = DateTime.Today;
            int selectedMonth = month ?? today.Month;
            int selectedYear = year ?? today.Year;

            if (selectedMonth is < 1 or > 12 || selectedYear is < 1 or > 9999
                || selectedYear == 1 && selectedMonth == 1 || selectedYear == 9999 && selectedMonth == 12)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid period", Detail = "Month or year is outside the supported range." });
            }

            try
            {
                return Ok(await movimentoService.GetTimeline(contoName, selectedMonth, selectedYear));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (FormulaEvaluationException exception)
            {
                return UnprocessableEntity(new FormulaEvaluationErrorDto(exception));
            }
        }
    }
}
