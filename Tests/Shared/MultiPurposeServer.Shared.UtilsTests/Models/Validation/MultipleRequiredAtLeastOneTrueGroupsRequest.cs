using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class MultipleRequiredAtLeastOneTrueGroupsRequest
    {
        [RequiredAtLeastOneTrue("FirstGroup")]
        public bool A { get; set; }

        [RequiredAtLeastOneTrue("FirstGroup")]
        public bool B { get; set; }

        [RequiredAtLeastOneTrue("SecondGroup")]
        public bool C { get; set; }

        [RequiredAtLeastOneTrue("SecondGroup")]
        public bool D { get; set; }
    }
}


