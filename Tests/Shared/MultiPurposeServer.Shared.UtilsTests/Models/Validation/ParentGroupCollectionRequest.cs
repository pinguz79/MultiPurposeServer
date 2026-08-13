using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class ParentGroupCollectionRequest
    {
        [ValidateChildren]
        public List<RequiredAtLeastOneRequest> Children { get; set; } = [];
    }
}


