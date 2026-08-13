using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal class BaseRequiredRequest
    {
        [Required]
        public string? Value { get; set; }
    }
}


