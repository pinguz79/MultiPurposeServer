using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal class BaseDto
    {
        [Normalize]
        public string? BaseValue { get; set; }
    }
}

