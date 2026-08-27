using Finance.Api.Application;
using Finance.Api.Application.Bulk;
using Finance.Api.Infrastructure.Persistence;
using Finance.Contracts.Bulk.Requests;
using Finance.Contracts.Responses;
using Finance.DataModel.Models;

using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Contracts.Enums;
using MultiPurposeServer.Shared.Contracts.Responses;

namespace Finance.Api.Controllers.BackEnd.Bulk
{
    [Route("Finance/BackEnd/Bulk/[controller]")]
    [ApiController]
    public class ContoController(IContoRepository contoRepository, IMovimentoService movimentoService) : FinanceBackEndControllerBase
    {
        [HttpPost("{contoName}/Movimento/Create")]
        public async Task<IActionResult> CreateMovimenti(string contoName, [FromBody] BulkCreateMovimentoRequest request)
        {
            Conto? conto = await contoRepository.GetByName(contoName);

            if (conto is null)
            {
                return NotFound();
            }

            BulkResponse<int, MovimentoConfigurationDto> response = await BulkOperationExecutor.Execute(
                request.Items,
                request.Options,
                item => item.RequestId,
                async item =>
                {
                    if (item.RequestId <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(item.RequestId), "RequestId must be positive.");
                    }

                    Movimento movimento = await movimentoService.Create(conto.Id, item.Date, item.Description, item.Formula);
                    movimento.Conto = conto;

                    return new MovimentoConfigurationDto(movimento);
                },
                movimentoService.BeginOperation,
                MapError);

            return Ok(response);
        }

        private static BulkError? MapError(Exception exception) => exception switch
        {
            ArgumentOutOfRangeException argument when argument.ParamName == "RequestId" => new BulkError(BulkErrorKind.Validation, "InvalidRequestId", argument.Message),
            ArgumentException argument => new BulkError(BulkErrorKind.Validation, "InvalidFormula", argument.Message),
            _ => null,
        };
    }
}
