namespace MultiPurposeServer.Shared.Utils.Validation.Rules
{
    internal abstract class ValidationRule
    {
        public abstract void Validate(object instance, ValidationContext context);
    }
}