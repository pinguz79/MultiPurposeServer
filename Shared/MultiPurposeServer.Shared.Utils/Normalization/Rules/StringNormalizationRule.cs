namespace MultiPurposeServer.Shared.Utils.Normalization.Rules
{
    internal sealed class StringNormalizationRule(Func<object, string?> getter, Action<object, string?> setter) : NormalizationRule
    {
        public override void Execute(object instance)
        {
            string? currentValue = getter(instance);
            string? normalizedValue = Normalize(currentValue);

            if (!string.Equals(currentValue, normalizedValue, StringComparison.Ordinal))
            {
                setter(instance, normalizedValue);
            }
        }

        private static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            string[] lines = value.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            string normalized = string.Join("\r\n", lines.Select(NormalizeLine)).Trim();

            return normalized.Length == 0 ? null : normalized;
        }

        private static string NormalizeLine(string value)
        {
            var result = new System.Text.StringBuilder(value.Length);
            var pendingSpace = false;

            foreach (char character in value.Trim())
            {
                if (character == '\t' || char.IsWhiteSpace(character))
                {
                    pendingSpace = result.Length > 0;
                }
                else
                {
                    if (pendingSpace)
                    {
                        result.Append(' ');
                        pendingSpace = false;
                    }

                    result.Append(character);
                }
            }

            return result.ToString();
        }
    }
}
