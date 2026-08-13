using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class WrongChildrenStringDto
    {
        [NormalizeChildren]
        public string? Value { get; set; }
    }
}

