using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class MultipleRequiredPropertiesRequest
    {
        [Required]
        public string? First { get; set; }

        [Required]
        public string? Second { get; set; }
    }
}


