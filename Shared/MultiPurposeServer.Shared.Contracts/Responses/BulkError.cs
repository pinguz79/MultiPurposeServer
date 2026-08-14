using MultiPurposeServer.Shared.Contracts.Enums;

namespace MultiPurposeServer.Shared.Contracts.Responses
{
    public sealed record BulkError(BulkErrorKind Kind, string Code, string Message);
}
