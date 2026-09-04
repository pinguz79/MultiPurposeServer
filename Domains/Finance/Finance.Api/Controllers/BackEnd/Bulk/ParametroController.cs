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
    [Route("Finance/BackEnd/Bulk/Conto/{contoName}/[controller]")]
    [ApiController]
    public class ParametroController(IParametroContoService service) : FinanceBackEndControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(string contoName, [FromBody] BulkCreateParametroContoRequest request)
        {
            IActionResult? invalidNames = ValidateUniqueNames(request.Items.Select(item => item.Name));
            if (invalidNames is not null)
            {
                return invalidNames;
            }

            BulkResponse<int, ParametroContoDto> response = await BulkOperationExecutor.Execute(
                request.Items,
                request.Options,
                item => item.RequestId,
                async item =>
                {
                    if (item.RequestId <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(item.RequestId), "RequestId must be positive.");
                    }

                    IReadOnlyList<ParametroConto> definitions = await service.Create(contoName, item.Name, item.Type, item.Definitions);

                    return new ParametroContoDto(definitions, Today());
                },
                service.BeginOperation,
                MapError);

            return Ok(response);
        }

        [HttpPatch("Update")]
        public async Task<IActionResult> Update(string contoName, [FromBody] BulkUpdateParametroContoRequest request)
        {
            IActionResult? invalidNames = ValidateUniqueNames(request.Items.Select(item => item.Name));
            if (invalidNames is not null)
            {
                return invalidNames;
            }

            BulkResponse<string, ParametroContoDto> response = await BulkOperationExecutor.Execute(
                request.Items,
                request.Options,
                item => item.Name,
                async item => new ParametroContoDto(await service.Update(contoName, item.Name, item.Definitions), Today()),
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
                => new BulkError(BulkErrorKind.Persistence, "ParametroContoNotFound", notFound.Message),
            ParametroContoReferencedException referenced
                => new BulkError(BulkErrorKind.Persistence, "ParametroContoReferenced", referenced.Message),
            ArgumentException argument
                => new BulkError(BulkErrorKind.Validation, "InvalidAccountParameter", argument.Message),
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
                        Title = "Invalid account parameters",
                        Detail = "Names must be unique after normalization.",
                    });
            }
            catch (ArgumentException exception)
            {
                return new BadRequestObjectResult(new ProblemDetails { Title = "Invalid account parameters", Detail = exception.Message });
            }
        }

        private static DateOnly Today()
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");

            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        }
    }
}
