using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class ArrayParentDto
    {
        [NormalizeChildren]
        public StringDto[]? Children { get; set; }
    }
}

