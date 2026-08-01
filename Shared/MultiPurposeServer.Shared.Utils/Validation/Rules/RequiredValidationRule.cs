using MultiPurposeServer.Shared.Utils.Resources;
using MultiPurposeServer.Shared.Utils.Validation.Rules;

namespace MultiPurposeServer.Shared.Utils.Validation
{
    internal sealed class RequiredValidationRule(string propertyName, Func<object, object?> getter) : ValidationRule
    {
        public override void Validate(object instance, ValidationContext context)
        {
            if (ValidationValue.IsMissing(getter(instance)))
                context.AddError(propertyName, ValidationMessages.Required);
        }
    }
}