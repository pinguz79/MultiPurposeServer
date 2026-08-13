namespace MultiPurposeServer.Shared.UtilsTests.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class UnsupportedValidationAttribute : MultiPurposeServer.Shared.Utils.Attributes.Abstractions.ValidationAttribute
    {
    }
}


