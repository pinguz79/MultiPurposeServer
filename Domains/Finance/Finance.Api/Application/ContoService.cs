using System.Globalization;
using System.Text;

using Finance.Api.Infrastructure.Persistence;
using Finance.DataModel.Models;

namespace Finance.Api.Application
{
    public class ContoService(
        IContoRepository contoRepository,
        IMovimentoRepository movimentoRepository,
        IFormulaEvaluator formulaEvaluator) : IContoService
    {
        public async Task<Conto> CreateConto(string name, string displayName, decimal initialBalance)
        {
            var normalizedName = NormalizeName(name);
            var normalizedDisplayName = NormalizeDisplayName(displayName);
            ValidateAmount(initialBalance, nameof(initialBalance));

            return await contoRepository.NameExists(normalizedName) ? throw new DuplicateNameException(normalizedName)
                : await contoRepository.CreateConto(normalizedName, normalizedDisplayName, initialBalance);
        }

        public Task<Conto?> GetById(Guid id) => contoRepository.GetById(id);

        public async Task<decimal> GetBalance(Conto conto)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            IReadOnlyList<Movimento> movements = await movimentoRepository.GetByContoThrough(conto.Id, today);
            decimal balance = conto.InitialBalance;

            foreach (Movimento movimento in movements)
            {
                FormulaEvaluationResult result = await formulaEvaluator.Evaluate(movimento.Formula, movimento.Date);

                if (result.Error is not null)
                {
                    throw new FormulaEvaluationException(movimento, new InvalidOperationException(result.Error));
                }

                balance += result.Value!.Value;
            }

            return balance;
        }

        public async Task<IReadOnlyList<Conto>> GetConti()
        {
            var conti = await contoRepository.GetConti();

            return [.. conti.OrderBy(conto => conto.DisplayName, StringComparer.CurrentCultureIgnoreCase).ThenBy(conto => conto.Name, StringComparer.OrdinalIgnoreCase)];
        }

        public async Task<Conto> UpdateConto(Guid id, string? name, string? displayName, decimal? initialBalance)
        {
            if (name is null && displayName is null && initialBalance is null)
            {
                throw new ArgumentException("At least one field must be provided.");
            }

            name = name is null ? null : NormalizeName(name);
            displayName = displayName is null ? null : NormalizeDisplayName(displayName);

            if (name is not null && await contoRepository.NameExists(name, id))
            {
                throw new DuplicateNameException(name);
            }

            if (initialBalance is not null)
            {
                ValidateAmount(initialBalance.Value, nameof(initialBalance));
            }

            return await contoRepository.UpdateConto(id, name, displayName, initialBalance);
        }

        public static string NormalizeName(string value)
        {
            var tokens = SplitTokens(value);

            return tokens.Count == 0 || !char.IsLetter(tokens[0][0]) ? throw new ArgumentException("Name must start with a letter and contain letters or numbers.", nameof(value))
                : string.Concat(tokens.Select(token => char.ToUpper(token[0], CultureInfo.InvariantCulture) + token[1..].ToLowerInvariant()));
        }

        private static List<string> SplitTokens(string value)
        {
            var tokens = new List<string>();
            var token = new StringBuilder();

            foreach (var character in value.Trim())
            {
                if (char.IsLetterOrDigit(character))
                {
                    if (char.IsUpper(character) && token.Length > 0 && char.IsLower(token[^1]))
                    {
                        tokens.Add(token.ToString());
                        token.Clear();
                    }

                    token.Append(character);
                }
                else if (token.Length > 0)
                {
                    tokens.Add(token.ToString());
                    token.Clear();
                }
            }

            if (token.Length > 0)
            {
                tokens.Add(token.ToString());
            }

            return tokens;
        }

        private static string NormalizeDisplayName(string value)
        {
            var normalized = value.Trim();

            return normalized.Length == 0 ? throw new ArgumentException("DisplayName cannot be empty.", nameof(value)) : normalized;
        }

        private static void ValidateAmount(decimal value, string parameterName)
        {
            if (decimal.Round(value, 2, MidpointRounding.AwayFromZero) != value)
            {
                throw new ArgumentException("Monetary amounts cannot contain more than two decimal places.", parameterName);
            }
        }
    }
}
