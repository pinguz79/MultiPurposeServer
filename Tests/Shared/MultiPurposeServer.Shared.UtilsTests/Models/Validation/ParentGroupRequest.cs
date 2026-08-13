using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class ParentGroupRequest
    {
        [ValidateChildren]
        public RequiredAtLeastOneRequest? Child { get; set; }
    }
}


