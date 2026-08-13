using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class WrongCollectionAttributeDto
    {
        [Normalize]
        public List<StringDto>? Children { get; set; }
    }
}

