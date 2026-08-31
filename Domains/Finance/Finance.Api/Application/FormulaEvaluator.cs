using System.Globalization;
using System.Text.RegularExpressions;

using NCalc;

namespace Finance.Api.Application
{
    public partial class FormulaEvaluator(IFormulaResolver resolver) : IFormulaEvaluator
    {
        public async Task<FormulaEvaluationResult> Evaluate(string formula, DateOnly date)
        {
            FormulaValidationResult validation = await Validate(formula);

            if (!validation.IsValid)
            {
                return new FormulaEvaluationResult(null, false, string.Join(" ", validation.Errors));
            }

            try
            {
                IReadOnlyList<ResolvedFormulaParameter> parameters = validation.Dependencies.Count == 0
                    ? []
                    : await resolver.Resolve(validation.Dependencies, date);
                var expression = new Expression(validation.Formula);

                foreach (ResolvedFormulaParameter parameter in parameters)
                {
                    expression.Parameters[parameter.Name] = parameter.Value;
                }

                decimal value = Convert.ToDecimal(expression.Evaluate(), CultureInfo.InvariantCulture);

                return new FormulaEvaluationResult(decimal.Round(value, 2, MidpointRounding.AwayFromZero), parameters.Any(parameter => parameter.HasUncoveredInterval), null);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return new FormulaEvaluationResult(null, false, exception.Message);
            }
        }

        public async Task<FormulaValidationResult> Validate(string formula)
        {
            var errors = new List<string>();
            string normalized = NormalizeConstant(formula.Trim());
            MatchCollection matches = ParameterRegex().Matches(normalized);
            var dependencies = new List<string>();

            foreach (Match match in FunctionRegex().Matches(normalized))
            {
                if (!string.Equals(match.Groups[1].Value, "Min", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(match.Groups[1].Value, "Max", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Function '{match.Groups[1].Value}' is not supported.");
                }
            }

            foreach (Match match in matches)
            {
                string dependency = match.Groups[1].Value.Trim();
                string? canonicalName = await resolver.ResolveCanonicalName(dependency);

                if (canonicalName is null)
                {
                    errors.Add(dependency.Contains('.')
                        ? $"Reference '{dependency}' is not supported by the current Finance slice."
                        : $"Recurring entry '{dependency}' does not exist.");
                    continue;
                }

                normalized = normalized.Replace(match.Value, $"[{canonicalName}]", StringComparison.Ordinal);

                if (!dependencies.Contains(canonicalName, StringComparer.OrdinalIgnoreCase))
                {
                    dependencies.Add(canonicalName);
                }
            }

            if (normalized.Length == 0)
            {
                errors.Add("Formula cannot be empty.");
            }
            else if (errors.Count == 0)
            {
                try
                {
                    var expression = new Expression(normalized);

                    if (expression.HasErrors())
                    {
                        errors.Add(expression.Error?.Message ?? "Formula syntax is invalid.");
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    errors.Add(exception.Message);
                }
            }

            return new FormulaValidationResult(normalized, dependencies, errors);
        }

        private static string NormalizeConstant(string formula)
        {
            string compact = formula.Replace(" ", string.Empty);

            if (compact.StartsWith('+'))
            {
                compact = compact[1..];
            }

            string candidate = compact.Contains(',') ? compact.Replace(".", string.Empty).Replace(',', '.') : compact;

            return decimal.TryParse(candidate, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal value)
                ? value.ToString("0.00", CultureInfo.InvariantCulture)
                : formula.Trim();
        }

        [GeneratedRegex(@"\[([^\]]+)\]", RegexOptions.CultureInvariant)]
        private static partial Regex ParameterRegex();

        [GeneratedRegex(@"\b([A-Za-z_]\w*)\s*\(", RegexOptions.CultureInvariant)]
        private static partial Regex FunctionRegex();
    }
}
