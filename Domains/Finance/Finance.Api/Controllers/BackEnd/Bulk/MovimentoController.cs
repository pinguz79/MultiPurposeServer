using Finance.Api.Application;
using Finance.Api.Application.Bulk;
using Finance.Contracts.Bulk.Requests;
using Finance.Contracts.Responses;

using Microsoft.AspNetCore.Mvc;

using MultiPurposeServer.Shared.Contracts.Enums;
using MultiPurposeServer.Shared.Contracts.Responses;

namespace Finance.Api.Controllers.BackEnd.Bulk
{
    [Route("Finance/BackEnd/Bulk/[controller]")]
    [ApiController]
    public class MovimentoController(IMovimentoService movimentoService) : FinanceBackEndControllerBase
    {
        [HttpPatch("Update")]
        public async Task<IActionResult> Update([FromBody] BulkUpdateMovimentoRequest request)
        {
            BulkResponse<Guid, MovimentoConfigurationDto> response = await BulkOperationExecutor.Execute(
                request.Items,
                request.Options,
                item => item.Id,
                async item => new MovimentoConfigurationDto(await movimentoService.Update(
                    item.Id,
                    item.Date,
                    item.Description,
                    item.Formula,
                    item.CategoryName,
                    item.ClearCategory)),
                movimentoService.BeginOperation,
                MapError);

            return Ok(response);
        }

        private static BulkError? MapError(Exception exception) => exception switch
        {
            KeyNotFoundException notFound when notFound.Message.StartsWith("Categoria", StringComparison.Ordinal)
                => new BulkError(BulkErrorKind.Persistence, "CategoriaNotFound", notFound.Message),
            KeyNotFoundException => new BulkError(BulkErrorKind.Persistence, "MovimentoNotFound", "Movimento not found."),
            ArgumentException argument => new BulkError(BulkErrorKind.Validation, "InvalidFormula", argument.Message),
            _ => null,
        };
    }
}
