using FluentAssertions;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Extensions;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class ParentNullableCollectionRequest
    {
        [ValidateChildren]
        public List<ChildRequest?> Children { get; set; } = [];
    }
}


