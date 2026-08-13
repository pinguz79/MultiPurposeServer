using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class ParentDto
    {
        [NormalizeChildren]
        public List<StringDto?>? Children { get; set; }
    }
}

