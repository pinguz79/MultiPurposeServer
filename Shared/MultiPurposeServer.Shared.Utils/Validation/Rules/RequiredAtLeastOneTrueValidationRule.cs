using MultiPurposeServer.Shared.Utils.Resources;

namespace MultiPurposeServer.Shared.Utils.Validation.Rules
{
    internal sealed class RequiredAtLeastOneTrueValidationRule(string[] propertyNames, Func<object, object?>[] getters) : ValidationRule
    {
        public override void Validate(object instance, ValidationContext context)
        {
            if (getters.Any(getter => getter(instance) is true))
            {
                return;
            }

            string message = string.Format(ValidationMessages.RequiredAtLeastOneTrue, string.Join(", ", propertyNames));
            context.AddError(propertyNames, message);
        }
    }
}
