using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class RequiredCollectionRequest
    {
        [Required]
        public List<string?>? Items { get; set; }
    }
}


