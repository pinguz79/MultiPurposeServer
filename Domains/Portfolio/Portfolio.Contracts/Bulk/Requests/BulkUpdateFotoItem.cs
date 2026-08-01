using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Portfolio.Contracts.Bulk.Requests
{
    public sealed record BulkUpdateFotoItem([property: Required] Guid Id, [property: Normalize, Required] string? Description) : IRequest
    {
    }
}