using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class ParentRequest
    {
        [ValidateChildren]
        public ChildRequest? Child { get; set; }
    }
}


