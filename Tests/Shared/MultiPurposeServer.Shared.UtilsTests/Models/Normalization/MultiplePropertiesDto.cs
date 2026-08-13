using MultiPurposeServer.Shared.Utils.Attributes;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class MultiplePropertiesDto
    {
        [Normalize]
        public string? FirstName { get; set; }

        [Normalize]
        public string? LastName { get; set; }

        public string? Notes { get; set; }
    }
}

