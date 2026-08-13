using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class DerivedDto : BaseDto
    {
        [Normalize]
        public string? DerivedValue { get; set; }
    }
}

