using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace Portfolio.Api.Tests.Application.Bulk
{
    public sealed record TestBulkItem(Guid Id, [property: Required] string? Value) : IRequest;
}
