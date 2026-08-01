using MultiPurposeServer.Shared.Utils.Validation.Rules;

namespace MultiPurposeServer.Shared.Utils.Validation
{
    internal sealed class ValidationPlan(IReadOnlyList<ValidationRule> rules)
    {
        public void Validate(object instance, ValidationContext context)
        {
            foreach (ValidationRule rule in rules)
                rule.Validate(instance, context);
        }
    }
}