using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class RequiredStringRequest
    {
        [Required]
        public string? Value { get; set; }
    }
}


