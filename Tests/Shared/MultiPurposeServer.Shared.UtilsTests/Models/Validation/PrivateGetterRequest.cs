using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class PrivateGetterRequest
    {
        [Required]
        public string? Value { private get; set; }
    }
}


