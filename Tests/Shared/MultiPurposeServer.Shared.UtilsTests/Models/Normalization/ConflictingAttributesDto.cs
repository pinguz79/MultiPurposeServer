using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class ConflictingAttributesDto
    {
        [Normalize]
        [NormalizeChildren]
        public List<StringDto>? Value { get; set; }
    }
}

