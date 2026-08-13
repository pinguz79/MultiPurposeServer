using FluentAssertions;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Extensions;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class MultipleRequiredAtLeastOneTrueGroupsRequest
    {
        [RequiredAtLeastOneTrue("FirstGroup")]
        public bool A { get; set; }

        [RequiredAtLeastOneTrue("FirstGroup")]
        public bool B { get; set; }

        [RequiredAtLeastOneTrue("SecondGroup")]
        public bool C { get; set; }

        [RequiredAtLeastOneTrue("SecondGroup")]
        public bool D { get; set; }
    }
}


