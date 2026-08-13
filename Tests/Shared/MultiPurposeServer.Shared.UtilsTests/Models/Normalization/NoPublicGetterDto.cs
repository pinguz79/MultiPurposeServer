using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class NoPublicGetterDto
    {
        [Normalize]
        public string? Value { private get; set; }

        public void SetValue(string? value) => Value = value;
    }
}

