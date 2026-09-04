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
                if (conto is null)
                {
                    return NotFound();
                }

                ContoStatus status = await contoService.GetStatus(conto);

                return Ok(Map(conto, status));
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
                    ContoStatus status = await contoService.GetStatus(conto);
                    result.Add(Map(conto, status));
                }

                return Ok(result);
            }
            catch (FormulaEvaluationException exception)
            {
                return UnprocessableEntity(new FormulaEvaluationErrorDto(exception));
            }
        }

        private static ContoDto Map(Conto conto, ContoStatus status)
            => new(conto, status.Balance, status.FirstNegativeBalanceDate, status.FirstNegativeBalance, Map(status.CycleIndicators));

        private static CycleIndicatorsDto? Map(CycleIndicators? indicators) => indicators is null
            ? null
            : new CycleIndicatorsDto(
                indicators.From,
                indicators.To,
                indicators.CurrentCycleSpent,
                indicators.Plafond,
                indicators.OverdraftPercentage,
                indicators.Overdraft,
                indicators.RemainingPlafond,
                indicators.RemainingIncludingOverdraft,
                indicators.PendingDebit,
                Map(indicators.FirstPlafondExceeded),
                Map(indicators.FirstTotalLimitExceeded));

        private static CycleThresholdDto? Map(CycleThreshold? threshold)
            => threshold is null ? null : new CycleThresholdDto(threshold.Date, threshold.Balance, threshold.Excess);
    }
}
