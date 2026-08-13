using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class WrongChildrenScalarDto
    {
        [NormalizeChildren]
        public StringDto? Child { get; set; }
    }
}

