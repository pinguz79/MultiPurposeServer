using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class RequiredValueTypeRequest
    {
        [Required]
        public int Value { get; set; }
    }
}


