using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Attributes.Abstractions;
using MultiPurposeServer.Shared.Utils.Validation.Exceptions;
using MultiPurposeServer.Shared.Utils.Validation.Rules;

namespace MultiPurposeServer.Shared.Utils.Validation
{
    internal static class Validator
    {
        private static readonly ConcurrentDictionary<Type, ValidationPlan> Plans = [];

        #region Validazione

        public static void Validate(object instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

            ValidationResult result = new();

            Validate(instance, new ValidationContext(result));

            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        internal static void Validate(object instance, ValidationContext context)
        {
            ValidationPlan plan = Plans.GetOrAdd(instance.GetType(), CreatePlan);
            plan.Validate(instance, context);
        }

        #endregion

        #region Creazione piano e regole

        private static ValidationPlan CreatePlan(Type declaringType)
        {
            List<ValidationRule> rules = [];
            HashSet<string> requiredAtLeastOneGroups = [];
            HashSet<string> requiredAtLeastOneTrueGroups = [];

            foreach (PropertyInfo property in declaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.GetMethod is not { IsPublic: true })
                {
                    continue;
                }

                rules.AddRange(CreateRules(declaringType, property, requiredAtLeastOneGroups, requiredAtLeastOneTrueGroups));
            }

            return new ValidationPlan(rules);
        }

        private static IEnumerable<ValidationRule> CreateRules(Type declaringType, PropertyInfo property, HashSet<string> requiredAtLeastOneGroups, HashSet<string> requiredAtLeastOneTrueGroups)
        {
            List<ValidationRule> rules = [];

            foreach (ValidationAttribute attribute in property.GetCustomAttributes<ValidationAttribute>())
            {
                switch (attribute)
                {
                    case RequiredAtLeastOneAttribute requiredAtLeastOne when requiredAtLeastOneGroups.Contains(requiredAtLeastOne.Group):
                        continue;
                    case RequiredAtLeastOneTrueAttribute requiredAtLeastOneTrue when requiredAtLeastOneTrueGroups.Contains(requiredAtLeastOneTrue.Group):
                        continue;
                }

                rules.Add(CreateRule(declaringType, property, attribute, requiredAtLeastOneGroups, requiredAtLeastOneTrueGroups));
            }

            return rules;
        }
        private static ValidationRule CreateRule(
            Type declaringType, PropertyInfo property, ValidationAttribute attribute,
            HashSet<string> requiredAtLeastOneGroups, HashSet<string> requiredAtLeastOneTrueGroups) => attribute switch
            {
                RequiredAttribute => CreateRequiredRule(declaringType, property),
                RequiredAtLeastOneAttribute requiredAtLeastOne => CreateRequiredAtLeastOneRule(declaringType, requiredAtLeastOne, requiredAtLeastOneGroups),
                RequiredAtLeastOneTrueAttribute requiredAtLeastOneTrue => CreateRequiredAtLeastOneTrueRule(declaringType, requiredAtLeastOneTrue, requiredAtLeastOneTrueGroups),
                ValidateChildrenAttribute => CreateValidateChildrenRule(declaringType, property),
                _ => throw new NotSupportedException($"Validation attribute type {attribute.GetType().Name} is not supported.")
            };

        private static ValidationRule CreateValidateChildrenRule(Type declaringType, PropertyInfo property) => new ValidateChildrenValidationRule(property.Name, CreateGetter(declaringType, property));

        private static ValidationRule CreateRequiredRule(Type declaringType, PropertyInfo property) => new RequiredValidationRule(property.Name, CreateGetter(declaringType, property));

        private static ValidationRule CreateRequiredAtLeastOneRule(Type declaringType, RequiredAtLeastOneAttribute attribute, HashSet<string> requiredAtLeastOneGroups)
        {
            PropertyInfo[] properties = [.. declaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.GetCustomAttributes<RequiredAtLeastOneAttribute>()
                    .Any(candidate => candidate.Group == attribute.Group))];

            requiredAtLeastOneGroups.Add(attribute.Group);

            return new RequiredAtLeastOneValidationRule([.. properties.Select(property => property.Name)],
                [.. properties.Select(property => CreateGetter(declaringType, property))]);
        }

        private static ValidationRule CreateRequiredAtLeastOneTrueRule(Type declaringType, RequiredAtLeastOneTrueAttribute attribute, HashSet<string> requiredAtLeastOneTrueGroups)
        {
            PropertyInfo[] properties = [.. declaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.GetCustomAttributes<RequiredAtLeastOneTrueAttribute>()
                    .Any(candidate => candidate.Group == attribute.Group))];

            PropertyInfo? invalidProperty = properties.FirstOrDefault(property => property.PropertyType != typeof(bool));

            if (invalidProperty is not null)
            {
                throw new InvalidOperationException($"Property '{declaringType.FullName}.{invalidProperty.Name}' belongs to validation group '{attribute.Group}' marked with [{nameof(RequiredAtLeastOneTrueAttribute)}], but is not a boolean property.");
            }

            requiredAtLeastOneTrueGroups.Add(attribute.Group);

            return new RequiredAtLeastOneTrueValidationRule([.. properties.Select(property => property.Name)], [.. properties.Select(property => CreateGetter(declaringType, property))]);
        }
        private static Func<object, object?> CreateGetter(Type declaringType, PropertyInfo property)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            UnaryExpression typedInstance = Expression.Convert(instance, declaringType);
            MemberExpression propertyAccess = Expression.Property(typedInstance, property);
            UnaryExpression convertedProperty = Expression.Convert(propertyAccess, typeof(object));

            return Expression.Lambda<Func<object, object?>>(convertedProperty, instance).Compile();
        }
        #endregion

    }
}
