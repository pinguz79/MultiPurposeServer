using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class ParentCollectionRequest
    {
        [ValidateChildren]
        public List<ChildRequest> Children { get; set; } = [];
    }
}


