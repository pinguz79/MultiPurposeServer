using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class RequiredAtLeastOneRequest
    {
        [RequiredAtLeastOne]
        public string? First { get; set; }

        [RequiredAtLeastOne]
        public string? Second { get; set; }
    }
}


