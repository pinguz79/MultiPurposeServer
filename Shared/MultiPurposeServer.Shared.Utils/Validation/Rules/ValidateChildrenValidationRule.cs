using System.Collections;

using MultiPurposeServer.Shared.Utils.Validation.Rules;

namespace MultiPurposeServer.Shared.Utils.Validation
{
    internal sealed class ValidateChildrenValidationRule(string propertyName, Func<object, object?> getter) : ValidationRule
    {
        public override void Validate(object instance, ValidationContext context)
        {
            object? value = getter(instance);

            if (value is null)
            {
                return;
            }

            if (value is IEnumerable enumerable && value is not string)
            {
                int index = 0;

                foreach (object child in enumerable)
                {
                    if (child is not null)
                    {
                        Validator.Validate(child, context.CreateCollectionItem(propertyName, index));
                    }

                    index++;
                }

                return;
            }

            Validator.Validate(value, context.CreateChild(propertyName));
        }
    }
}
