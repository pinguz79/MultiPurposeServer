using MultiPurposeServer.Shared.Utils.Attributes.Abstractions;

namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class UnsupportedValidationAttribute : ValidationAttribute
    {
    }
}


