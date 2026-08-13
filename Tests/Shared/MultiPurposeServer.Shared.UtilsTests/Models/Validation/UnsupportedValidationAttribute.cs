using FluentAssertions;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Extensions;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class UnsupportedValidationAttribute : MultiPurposeServer.Shared.Utils.Attributes.Abstractions.ValidationAttribute
    {
    }
}


