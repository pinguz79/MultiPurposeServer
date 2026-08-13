using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class RequiredAtLeastOneTrueRequest
    {
        [RequiredAtLeastOneTrue]
        public bool First { get; set; }

        [RequiredAtLeastOneTrue]
        public bool Second { get; set; }

        [RequiredAtLeastOneTrue]
        public bool Third { get; set; }
    }
}


