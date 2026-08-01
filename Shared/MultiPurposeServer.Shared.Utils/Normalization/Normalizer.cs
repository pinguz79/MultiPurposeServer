using MultiPurposeServer.Shared.Utils.Attributes;
using MultiPurposeServer.Shared.Utils.Normalization.Rules;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace MultiPurposeServer.Shared.Utils.Normalization;

public static class Normalizer
{
    private static readonly ConcurrentDictionary<Type, NormalizationPlan> Plans = [];

    public static void Normalize(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        NormalizationPlan plan = Plans.GetOrAdd(instance.GetType(), CreatePlan);
        plan.Execute(instance);
    }

    public static void Normalize<T>(IEnumerable<T> instances) where T : class
    {
        ArgumentNullException.ThrowIfNull(instances);

        NormalizationPlan plan = Plans.GetOrAdd(typeof(T), CreatePlan);

        foreach (T instance in instances)
        {
            if (instance is not null)
                plan.Execute(instance);
        }
    }

    private static NormalizationPlan CreatePlan(Type type)
    {
        List<NormalizationRule> rules = [];

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            bool normalizeValue = property.IsDefined(typeof(NormalizeAttribute), true);
            bool normalizeChildren = property.IsDefined(typeof(NormalizeChildrenAttribute), true);

            if (!normalizeValue && !normalizeChildren)
                continue;

            if (normalizeValue && normalizeChildren)
                throw new InvalidOperationException($"Property '{type.FullName}.{property.Name}' cannot use both [{nameof(NormalizeAttribute)}] and [{nameof(NormalizeChildrenAttribute)}].");

            rules.Add(normalizeValue ? CreateValueNormalizationRule(type, property) : CreateChildrenNormalizationRule(type, property));
        }

        return new NormalizationPlan(rules);
    }
    private static NormalizationRule CreateChildrenNormalizationRule(Type declaringType, PropertyInfo property)
    {
        EnsurePublicGetter(declaringType, property);

        if (property.PropertyType == typeof(string))
            throw new InvalidOperationException($"Property '{declaringType.FullName}.{property.Name}' is a string and cannot use [{nameof(NormalizeChildrenAttribute)}].");

        if (!typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            throw new InvalidOperationException($"Property '{declaringType.FullName}.{property.Name}' uses [{nameof(NormalizeChildrenAttribute)}] but its type '{property.PropertyType.FullName}' does not implement {nameof(IEnumerable)}.");

        return new CollectionNormalizationRule(CreateCollectionGetter(declaringType, property));
    }
    private static NormalizationRule CreateValueNormalizationRule(Type declaringType, PropertyInfo property)
    {
        EnsurePublicGetter(declaringType, property);
        EnsurePublicSetter(declaringType, property);

        return property.PropertyType switch
        {
            Type t when t == typeof(string) => new StringNormalizationRule(CreateStringGetter(declaringType, property), CreateStringSetter(declaringType, property)),
            Type t when typeof(IEnumerable).IsAssignableFrom(t) => throw new InvalidOperationException($"Property '{declaringType.FullName}.{property.Name}' is a collection. Use [{nameof(NormalizeChildrenAttribute)}] instead of [{nameof(NormalizeAttribute)}]."),
            _ => throw new NotSupportedException($"Normalization is not supported for property '{declaringType.FullName}.{property.Name}' of type '{property.PropertyType.FullName}'.")
        };
    }
    private static void EnsurePublicGetter(Type declaringType, PropertyInfo property)
    {
        if (property.GetMethod is not { IsPublic: true })
            throw new InvalidOperationException($"Property '{declaringType.FullName}.{property.Name}' must have a public getter.");
    }
    private static void EnsurePublicSetter(Type declaringType, PropertyInfo property)
    {
        if (property.SetMethod is not { IsPublic: true })
            throw new InvalidOperationException($"Property '{declaringType.FullName}.{property.Name}' uses [{nameof(NormalizeAttribute)}] but does not have a public setter.");
    }
    private static Func<object, string?> CreateStringGetter(Type declaringType, PropertyInfo property)
    {
        ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
        UnaryExpression typedInstance = Expression.Convert(instance, declaringType);
        MemberExpression propertyAccess = Expression.Property(typedInstance, property);

        return Expression.Lambda<Func<object, string?>>(propertyAccess, instance).Compile();
    }
    private static Func<object, IEnumerable?> CreateCollectionGetter(Type declaringType, PropertyInfo property)
    {
        ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
        UnaryExpression typedInstance = Expression.Convert(instance, declaringType);
        MemberExpression propertyAccess = Expression.Property(typedInstance, property);
        UnaryExpression convertedProperty = Expression.Convert(propertyAccess, typeof(IEnumerable));

        return Expression.Lambda<Func<object, IEnumerable?>>(convertedProperty, instance).Compile();
    }

    private static Action<object, string?> CreateStringSetter(Type declaringType, PropertyInfo property)
    {
        ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
        ParameterExpression value = Expression.Parameter(typeof(string), "value");
        UnaryExpression typedInstance = Expression.Convert(instance, declaringType);
        MemberExpression propertyAccess = Expression.Property(typedInstance, property);
        BinaryExpression assignment = Expression.Assign(propertyAccess, value);

        return Expression.Lambda<Action<object, string?>>(assignment, instance, value).Compile();
    }
}