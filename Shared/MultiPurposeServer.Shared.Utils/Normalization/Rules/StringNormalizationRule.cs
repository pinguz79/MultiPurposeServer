namespace MultiPurposeServer.Shared.Utils.Normalization.Rules;

internal sealed class StringNormalizationRule(Func<object, string?> getter, Action<object, string?> setter) : NormalizationRule
{
    public override void Execute(object instance)
    {
        string? currentValue = getter(instance);
        string? normalizedValue = Normalize(currentValue);

        if (!string.Equals(currentValue, normalizedValue, StringComparison.Ordinal))
            setter(instance, normalizedValue);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}