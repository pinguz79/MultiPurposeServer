using FluentAssertions;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Extensions;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    internal sealed class UnsupportedValidationRequest
    {
        [UnsupportedValidation]
        public string? Value { get; set; }
    }
}


