using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class RequiredEnumerableRequest
    {
        [Required]
        public IEnumerable<int>? Items { get; set; }
    }
}


