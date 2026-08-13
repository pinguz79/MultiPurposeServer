using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class InvalidRequiredAtLeastOneTrueRequest
    {
        [RequiredAtLeastOneTrue]
        public bool Flag { get; set; }

        [RequiredAtLeastOneTrue]
        public string? Value { get; set; }
    }
}


