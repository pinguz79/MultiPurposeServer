using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class UniqueByRequest
    {
        [UniqueBy(nameof(UniqueByItem.Id))]
        public IReadOnlyCollection<UniqueByItem> Items { get; init; } = [];
    }
}
