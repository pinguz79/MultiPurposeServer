using MultiPurposeServer.Shared.Utils.Resources;

namespace MultiPurposeServer.Shared.Utils.Validation.Rules
{
    internal sealed class RequiredAtLeastOneValidationRule(
        string[] propertyNames,
        Func<object, object?>[] getters) : ValidationRule
    {
        public override void Validate(object instance, ValidationContext context)
        {
            if (getters.Any(getter => !ValidationValue.IsMissing(getter(instance))))
            {
                return;
            }

            var message = string.Format(ValidationMessages.RequiredAtLeastOne, string.Join(", ", propertyNames));

            context.AddError(propertyNames, message);
        }
    }
}
