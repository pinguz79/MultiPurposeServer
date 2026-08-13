using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class ReadOnlyParentDto(IReadOnlyList<StringDto> children)
    {
        [NormalizeChildren]
        public IReadOnlyList<StringDto> Children { get; } = children;
    }
}

