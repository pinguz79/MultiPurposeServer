using MultiPurposeServer.Shared.Contracts.Abstractions;
using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.Contracts.Requests
{
    public abstract record BulkRequest<TItem>([property: Required, ValidateChildren] BulkOptions Options, [property: NormalizeChildren, Required, UniqueBy("Id")] IReadOnlyCollection<TItem> Items) : IRequest, IBulk<TItem>
        where TItem : IRequest
    {
    }
}
