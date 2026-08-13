using System.Collections;

using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Extensions;
using MultiPurposeServer.Shared.Utils.Normalization;

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

