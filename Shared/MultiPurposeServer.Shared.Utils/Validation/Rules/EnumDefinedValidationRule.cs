using MultiPurposeServer.Shared.Utils.Resources;

namespace MultiPurposeServer.Shared.Utils.Validation.Rules
{
    internal sealed class EnumDefinedValidationRule(string propertyName, Type enumType, Func<object, object?> getter) : ValidationRule
    {
        public override void Validate(object instance, ValidationContext context)
        {
            object? value = getter(instance);

            if (value is not null && !Enum.IsDefined(enumType, value))
            {
                context.AddError(propertyName, ValidationMessages.NotSupported);
            }
        }
    }
}
