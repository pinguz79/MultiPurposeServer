using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class MultipleRequiredAtLeastOneGroupsRequest
    {
        [RequiredAtLeastOne("FirstGroup")]
        public string? A { get; set; }

        [RequiredAtLeastOne("FirstGroup")]
        public string? B { get; set; }

        [RequiredAtLeastOne("SecondGroup")]
        public string? C { get; set; }

        [RequiredAtLeastOne("SecondGroup")]
        public string? D { get; set; }
    }
}


