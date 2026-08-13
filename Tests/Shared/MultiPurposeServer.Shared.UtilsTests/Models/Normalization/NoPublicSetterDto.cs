using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class NoPublicSetterDto(string? value)
    {
        [Normalize]
        public string? Value { get; private set; } = value;
    }
}

