using System.Collections;
using System.Reflection;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Resources;

namespace MultiPurposeServer.Shared.Utils.Validation.Rules
{
    internal sealed class UniqueByValidationRule(string propertyName, string keyPropertyName, Func<object, object?> getter) : ValidationRule
    {
        public override void Validate(object instance, ValidationContext context)
        {
            if (getter(instance) is not IEnumerable items)
            {
                return;
            }

            HashSet<object?> keys = [];

            foreach (object? item in items)
            {
                if (item is null)
                {
                    continue;
                }

                PropertyInfo keyProperty = item.GetType().GetProperty(keyPropertyName, BindingFlags.Instance | BindingFlags.Public)
                    ?? throw new InvalidOperationException($"Property '{item.GetType().FullName}.{keyPropertyName}' configured by [{nameof(UniqueByAttribute)}] does not exist or is not public.");

                if (!keys.Add(keyProperty.GetValue(item)))
                {
                    context.AddError(propertyName, ValidationMessages.DuplicateValue);
                    return;
                }
            }
        }
    }
}
