using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class BranchDto
    {
        [Normalize]
        public string? Name { get; set; }

        [NormalizeChildren]
        public List<StringDto>? Leaves { get; set; }
    }
}

