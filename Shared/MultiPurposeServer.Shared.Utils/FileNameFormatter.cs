using System.Globalization;
using System.Text.RegularExpressions;

namespace MultiPurposeServer.Shared.Utils
{
    public sealed class FileNameFormatter(string? fileName, FileNameFormatOptions? options = null)
    {
        private static readonly Regex ExtensionRegex = new(@"\.[a-zA-Z0-9]+$", RegexOptions.Compiled);
        private static readonly Regex LeadingIndexRegex = new(@"^\s*\d{1,3}\s*[-_ ]+\s*", RegexOptions.Compiled);
        private static readonly Regex TrailingIndexRegex = new(@"\s*[-_ ]+\d{2,5}\s*$", RegexOptions.Compiled);
        private static readonly Regex CamelCaseRegex = new(@"(?<=[\p{Ll}])(?=[\p{Lu}])", RegexOptions.Compiled); private static readonly Regex LetterDigitRegex = new(@"(?<=[A-Za-z])(?=\d)|(?<=\d)(?=[A-Za-z])", RegexOptions.Compiled);
        private static readonly Regex StandaloneSmallNumberRegex = new(@"\b\d{1,3}\b", RegexOptions.Compiled);
        private static readonly Regex UnderscoreRegex = new(@"[_]+", RegexOptions.Compiled);
        private static readonly Regex HyphenRegex = new(@"\s*-\s*", RegexOptions.Compiled);
        private static readonly Regex MultiSpaceRegex = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex DashSpacingRegex = new(@"\s*–\s*", RegexOptions.Compiled);
        private static readonly Regex MultiDashRegex = new(@"(?:\s*–\s*){2,}", RegexOptions.Compiled);

        public string HumanizedName
        {
            get
            {
                options ??= FileNameFormatOptions.Default;

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return options.EmptyFallback;
                }

                var name = Path.GetFileName(fileName);

                if (options.RemoveExtension)
                {
                    name = ExtensionRegex.Replace(name, string.Empty);
                }

                if (options.RemoveLeadingNumericIndex)
                {
                    name = LeadingIndexRegex.Replace(name, string.Empty);
                }

                if (options.RemoveTrailingNumericIndex)
                {
                    name = TrailingIndexRegex.Replace(name, string.Empty);
                }

                if (options.NormalizeUnderscores)
                {
                    name = UnderscoreRegex.Replace(name, " ");
                }

                if (options.NormalizeHyphens)
                {
                    name = HyphenRegex.Replace(name, " – ");
                }

                if (options.SplitCamelCase)
                {
                    name = CamelCaseRegex.Replace(name, " ");
                }

                if (options.SplitLetterDigitBoundaries)
                {
                    name = LetterDigitRegex.Replace(name, " ");
                }

                if (options.RemoveStandaloneSmallNumbers)
                {
                    name = StandaloneSmallNumberRegex.Replace(name, " ");
                }

                name = ApplyTokenMap(name, options);
                name = NormalizeSpacing(name);

                if (options.ApplyTitleCase)
                {
                    name = ToTitleCase(name);
                }

                return string.IsNullOrWhiteSpace(name) ? options.EmptyFallback : name;
            }
        }

        private static string ApplyTokenMap(string value, FileNameFormatOptions options)
        {
            if (options.TokenMap.Count == 0)
            {
                return value;
            }

            var tokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var output = new List<string>();

            foreach (var token in tokens)
            {
                var cleanToken = token.Trim();

                if (options.TokenMap.TryGetValue(cleanToken, out var replacement))
                {
                    if (!string.IsNullOrWhiteSpace(replacement))
                    {
                        output.Add(replacement);
                    }

                    continue;
                }

                output.Add(cleanToken);
            }

            return string.Join(' ', output);
        }

        private static string NormalizeSpacing(string value)
        {
            value = MultiSpaceRegex.Replace(value, " ").Trim();
            value = DashSpacingRegex.Replace(value, " – ");
            value = MultiDashRegex.Replace(value, " – ");

            return value.Trim(' ', '–');
        }

        private static string ToTitleCase(string value)
        {
            var culture = CultureInfo.GetCultureInfo("it-IT");

            return string.Join(" ", value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(word =>
            {
                if (word == "–")
                {
                    return word;
                }

                if (word.Length <= 2 && word.All(char.IsUpper))
                {
                    return word;
                }

                return culture.TextInfo.ToTitleCase(word.ToLower(culture));
            }));
        }
    }
}
