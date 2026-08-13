using MultiPurposeServer.Shared.Contracts;
using MultiPurposeServer.Shared.Contracts.Requests;

namespace MultiPurposeServer.Shared.ContractsTests
{
    internal sealed record TestBulkRequest(
        BulkOptions Options,
        IReadOnlyCollection<TestBulkItem> Items)
        : BulkRequest<TestBulkItem>(Options, Items);
}
