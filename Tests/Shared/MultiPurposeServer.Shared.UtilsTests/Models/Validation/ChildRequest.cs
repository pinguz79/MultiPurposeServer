using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class ChildRequest
    {
        [Required]
        public string? Name { get; set; }
    }
}


