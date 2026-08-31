using Finance.Api.Application;
using Finance.Api.Application.Bulk;
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
    public class CategoriaController(ICategoriaService service) : FinanceBackEndControllerBase
    {
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] BulkCreateCategoriaRequest request)
        {
            IActionResult? invalidNames = ValidateUniqueNames(request.Items.Select(item => item.Name));
            if (invalidNames is not null)
            {
                return invalidNames;
            }

            BulkResponse<int, CategoriaDto> response = await BulkOperationExecutor.Execute(
                request.Items,
                request.Options,
                item => item.RequestId,
                async item =>
                {
                    if (item.RequestId <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(item.RequestId), "RequestId must be positive.");
                    }

                    Categoria categoria = await service.Create(item.Name, item.DisplayName);

                    return new CategoriaDto(categoria.Name, categoria.DisplayName, 0);
                },
                service.BeginOperation,
                MapError);

            return Ok(response);
        }

        [HttpPatch("Update")]
        public async Task<IActionResult> Update([FromBody] BulkUpdateCategoriaRequest request)
        {
            IActionResult? invalidNames = ValidateUniqueNames(request.Items.Select(item => item.Name));
            if (invalidNames is not null)
            {
                return invalidNames;
            }

            BulkResponse<string, CategoriaDto> response = await BulkOperationExecutor.Execute(
                request.Items,
                request.Options,
                item => item.Name,
                async item =>
                {
                    Categoria categoria = await service.Update(item.Name, item.DisplayName);
                    CategoriaUsage usage = (await service.GetByName(categoria.Name))!.Value.Usage;

                    return new CategoriaDto(categoria.Name, categoria.DisplayName, usage.Total);
                },
                service.BeginOperation,
                MapError);

            return Ok(response);
        }

        private static BulkError? MapError(Exception exception) => exception switch
        {
            ArgumentOutOfRangeException argument when argument.ParamName == "RequestId"
                => new BulkError(BulkErrorKind.Validation, "InvalidRequestId", argument.Message),
            DuplicateNameException duplicate
                => new BulkError(BulkErrorKind.Persistence, "DuplicateName", duplicate.Message),
            KeyNotFoundException notFound
                => new BulkError(BulkErrorKind.Persistence, "CategoriaNotFound", notFound.Message),
            ArgumentException argument
                => new BulkError(BulkErrorKind.Validation, "InvalidCategory", argument.Message),
            _ => null,
        };

        private static IActionResult? ValidateUniqueNames(IEnumerable<string> values)
        {
            try
            {
                string[] names = [.. values.Select(ContoService.NormalizeName)];

                return names.Distinct(StringComparer.OrdinalIgnoreCase).Count() == names.Length
                    ? null
                    : new BadRequestObjectResult(new ProblemDetails
                    {
                        Title = "Invalid categories",
                        Detail = "Names must be unique after normalization.",
                    });
            }
            catch (ArgumentException exception)
            {
                return new BadRequestObjectResult(new ProblemDetails { Title = "Invalid categories", Detail = exception.Message });
            }
        }
    }
}
