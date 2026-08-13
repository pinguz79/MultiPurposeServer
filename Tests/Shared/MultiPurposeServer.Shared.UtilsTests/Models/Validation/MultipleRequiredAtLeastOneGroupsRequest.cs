using FluentAssertions;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Extensions;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class MultipleRequiredAtLeastOneGroupsRequest
    {
        [RequiredAtLeastOne("FirstGroup")]
        public string? A { get; set; }

        [RequiredAtLeastOne("FirstGroup")]
        public string? B { get; set; }

        [RequiredAtLeastOne("SecondGroup")]
        public string? C { get; set; }

        [RequiredAtLeastOne("SecondGroup")]
        public string? D { get; set; }
    }
}


