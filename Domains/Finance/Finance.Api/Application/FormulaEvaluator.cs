using System.Globalization;
using System.Text.RegularExpressions;

using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

using NCalc;

namespace Finance.Api.Application
{
    public partial class FormulaEvaluator : IFormulaEvaluator
    {
        private readonly IFormulaResolver _resolver;
        private readonly IContoRepository? _contoRepository;
        private readonly IMovimentoRepository? _movimentoRepository;
        private readonly IParametroContoRepository? _parametroContoRepository;
        private readonly Dictionary<string, Expression> _expressions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, FormulaValidationResult> _validations = new(StringComparer.Ordinal);

        public FormulaEvaluator(IFormulaResolver resolver)
        {
            _resolver = resolver;
        }

        public FormulaEvaluator(
            IFormulaResolver resolver,
            IContoRepository contoRepository,
            IMovimentoRepository movimentoRepository,
            IParametroContoRepository parametroContoRepository)
        {
            _resolver = resolver;
            _contoRepository = contoRepository;
            _movimentoRepository = movimentoRepository;
            _parametroContoRepository = parametroContoRepository;
        }

        public Task<FormulaEvaluationResult> Evaluate(string formula, DateOnly date) => Evaluate(formula, date, []);

        private async Task<FormulaEvaluationResult> Evaluate(string formula, DateOnly date, HashSet<Guid> evaluationPath)
        {
            string normalizedConstant = NormalizeConstant(formula.Trim());
            if (decimal.TryParse(normalizedConstant, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out decimal constant))
            {
                return new FormulaEvaluationResult(decimal.Round(constant, 2, MidpointRounding.AwayFromZero), false, null);
            }

            FormulaValidationResult validation = await Validate(formula);

            if (!validation.IsValid)
            {
                return new FormulaEvaluationResult(null, false, string.Join(" ", validation.Errors));
            }

            try
            {
                string[] calculatedDependencies = [.. validation.Dependencies.Where(IsSaldoUltimoCicloChiuso)];
                string[] resolvedDependencies = [.. validation.Dependencies.Where(dependency => !IsSaldoUltimoCicloChiuso(dependency))];
                IReadOnlyList<ResolvedFormulaParameter> parameters = resolvedDependencies.Length == 0
                    ? []
                    : await _resolver.Resolve(resolvedDependencies, date);
                Expression expression = GetExpression(validation.Formula);

                foreach (ResolvedFormulaParameter parameter in parameters)
                {
                    expression.Parameters[parameter.Name] = parameter.Value;
                }

                foreach (string dependency in calculatedDependencies)
                {
                    expression.Parameters[dependency] = await CalculateSaldoUltimoCicloChiuso(dependency, date, evaluationPath);
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
            if (_validations.TryGetValue(formula, out FormulaValidationResult? validation))
            {
                return validation;
            }

            validation = await ValidateCore(formula);
            _validations.Add(formula, validation);

            return validation;
        }

        private async Task<FormulaValidationResult> ValidateCore(string formula)
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
                string? canonicalName = await _resolver.ResolveCanonicalName(dependency);

                if (canonicalName is null)
                {
                    errors.Add(dependency.Contains('.')
                        ? $"Account parameter '{dependency}' does not exist."
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

        private async Task<decimal> CalculateSaldoUltimoCicloChiuso(string dependency, DateOnly date, HashSet<Guid> evaluationPath)
        {
            if (_contoRepository is null || _movimentoRepository is null || _parametroContoRepository is null)
            {
                throw new InvalidOperationException("Calculated account properties are not available in this evaluation context.");
            }

            string contoName = dependency[..dependency.IndexOf('.')];
            Conto conto = await _contoRepository.GetByName(contoName)
                ?? throw new KeyNotFoundException($"Account '{contoName}' was not found.");
            IReadOnlyList<ParametroConto> definitions = await _parametroContoRepository.GetByName(conto.Id, "ChiusuraCiclo");
            ParametroConto closingDayDefinition = definitions.FirstOrDefault(definition => (definition.ValidFrom is null || definition.ValidFrom <= date)
                && (definition.ValidTo is null || definition.ValidTo >= date))
                ?? throw new KeyNotFoundException($"Account parameter '{conto.Name}.ChiusuraCiclo' was not found for {date:yyyy-MM-dd}.");
            int closingDay = decimal.ToInt32(closingDayDefinition.Value);

            if (closingDay is < 1 or > 31)
            {
                throw new InvalidOperationException($"Account parameter '{conto.Name}.ChiusuraCiclo' must be between 1 and 31.");
            }

            DateOnly closingDate = GetClosingDate(date, closingDay);
            IReadOnlyList<Movimento> movements = await _movimentoRepository.GetByContoThrough(conto.Id, closingDate);
            decimal balance = conto.InitialBalance;

            foreach (Movimento movimento in movements)
            {
                if (!evaluationPath.Add(movimento.Id))
                {
                    throw new InvalidOperationException($"Circular formula reference detected at movement '{movimento.Id}'.");
                }

                FormulaEvaluationResult result = await Evaluate(movimento.Formula, movimento.Date, evaluationPath);
                evaluationPath.Remove(movimento.Id);

                if (result.Error is not null)
                {
                    throw new InvalidOperationException(result.Error);
                }

                balance += result.Value!.Value;
            }

            return decimal.Round(balance, 2, MidpointRounding.AwayFromZero);
        }

        private static DateOnly GetClosingDate(DateOnly date, int closingDay)
        {
            DateOnly month = date.Day >= Math.Min(closingDay, DateTime.DaysInMonth(date.Year, date.Month))
                ? new DateOnly(date.Year, date.Month, 1)
                : new DateOnly(date.Year, date.Month, 1).AddMonths(-1);

            return new DateOnly(month.Year, month.Month, Math.Min(closingDay, DateTime.DaysInMonth(month.Year, month.Month)));
        }

        private static bool IsSaldoUltimoCicloChiuso(string dependency)
            => dependency.EndsWith($".{FormulaResolver.SaldoUltimoCicloChiuso}", StringComparison.OrdinalIgnoreCase);

        private Expression GetExpression(string formula)
        {
            if (!_expressions.TryGetValue(formula, out Expression? expression))
            {
                expression = new Expression(formula);
                _expressions.Add(formula, expression);
            }

            return expression;
        }

        [GeneratedRegex(@"\[([^\]]+)\]", RegexOptions.CultureInvariant)]
        private static partial Regex ParameterRegex();

        [GeneratedRegex(@"\b([A-Za-z_]\w*)\s*\(", RegexOptions.CultureInvariant)]
        private static partial Regex FunctionRegex();
    }
}
