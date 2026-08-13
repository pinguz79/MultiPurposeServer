using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class RequiredNullableValueTypeRequest
    {
        [Required]
        public int? Value { get; set; }
    }
}


