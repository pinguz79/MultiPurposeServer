using System.Collections;

using FluentAssertions;

using Moq;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Extensions;
using MultiPurposeServer.Shared.Utils.Normalization;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Normalization
{
    internal sealed class ConflictingAttributesDto
    {
        [Normalize]
        [NormalizeChildren]
        public List<StringDto>? Value { get; set; }
    }
}

