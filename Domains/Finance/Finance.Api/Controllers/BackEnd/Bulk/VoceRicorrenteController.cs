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
    public class VoceRicorrenteController(IVoceRicorrenteService service) : FinanceBackEndControllerBase
    {
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] BulkCreateVoceRicorrenteRequest request)
        {
            try
            {
                string[] names = [.. request.Items.Select(item => ContoService.NormalizeName(item.Name))];
                if (names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Length)
                {
                    return BadRequest(new ProblemDetails { Title = "Invalid recurring entries", Detail = "Names must be unique after normalization." });
                }
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new ProblemDetails { Title = "Invalid recurring entries", Detail = exception.Message });
            }

            BulkResponse<int, VoceRicorrenteDto> response = await BulkOperationExecutor.Execute(
                request.Items,
                request.Options,
                item => item.RequestId,
                async item =>
                {
                    if (item.RequestId <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(item.RequestId), "RequestId must be positive.");
                    }

                    IReadOnlyList<VoceRicorrente> definitions = await service.Create(item.Name, item.Definitions);

                    return new VoceRicorrenteDto(definitions, Today());
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
            ArgumentException argument
                => new BulkError(BulkErrorKind.Validation, "InvalidRecurringEntry", argument.Message),
            _ => null,
        };

        private static DateOnly Today()
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Rome");

            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        }
    }
}
