using System.Text.RegularExpressions;

namespace Finance.ExpressionEngineSpike;

internal static partial class FinanceFormulaTranslator
{
    public static IReadOnlyList<string> GetDependencies(string formula) => FinanceReferenceRegex()
        .Matches(formula)
        .Select(match => match.Groups[1].Value)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    public static string ForNCalc(string formula) => formula;

    public static string ForDynamicExpresso(string formula) => FinanceReferenceRegex()
        .Replace(formula, match => ToIdentifier(match.Groups[1].Value))
        .Replace("Min(", "min(", StringComparison.Ordinal)
        .Replace("Max(", "max(", StringComparison.Ordinal);

    public static string ToIdentifier(string reference) => reference.Replace('.', '_');

    [GeneratedRegex(@"\[([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)\]")]
    private static partial Regex FinanceReferenceRegex();
}
