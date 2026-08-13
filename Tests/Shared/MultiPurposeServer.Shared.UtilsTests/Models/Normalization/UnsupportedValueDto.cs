using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class UnsupportedValueDto
    {
        [Normalize]
        public int Value { get; set; }
    }
}

