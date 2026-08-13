using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class RootDto
    {
        [NormalizeChildren]
        public List<BranchDto>? Branches { get; set; }
    }
}

