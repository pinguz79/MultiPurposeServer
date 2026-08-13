using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    public sealed class StringDto
    {
        [Normalize]
        public string? Value { get; set; }
    }
}

