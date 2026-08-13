using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.Contracts.Requests
{
    public abstract record BulkRequest<TItem>([property: Required] BulkOptions Options, [property: NormalizeChildren, Required, UniqueBy("Id"), ValidateChildren] IReadOnlyCollection<TItem> Items) : IRequest, IBulk<TItem>
        where TItem : IRequest
    {
    }
}
