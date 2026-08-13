using FluentAssertions;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Extensions;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class RequiredAtLeastOneTrueRequest
    {
        [RequiredAtLeastOneTrue]
        public bool First { get; set; }

        [RequiredAtLeastOneTrue]
        public bool Second { get; set; }

        [RequiredAtLeastOneTrue]
        public bool Third { get; set; }
    }
}


