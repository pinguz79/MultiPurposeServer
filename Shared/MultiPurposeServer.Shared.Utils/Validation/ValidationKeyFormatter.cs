namespace MultiPurposeServer.Shared.Utils.Validation
{
    internal static class ValidationKeyFormatter
    {
        private const char GroupSeparator = '|';

        public static string FormatProperty(IReadOnlyList<ValidationPathSegment> path, string propertyName) => Append(FormatPath(path), propertyName);

        public static string FormatGroup(IReadOnlyList<ValidationPathSegment> path, IEnumerable<string> propertyNames) => string.Join(GroupSeparator, propertyNames.Select(propertyName => Append(FormatPath(path), propertyName)));

        private static string FormatPath(IEnumerable<ValidationPathSegment> path) => string.Join(".", path.Select(FormatSegment));

        private static string FormatSegment(ValidationPathSegment segment) => segment.Index is null ? segment.PropertyName : $"{segment.PropertyName}[{segment.Index}]";

        private static string Append(string path, string propertyName) => string.IsNullOrEmpty(path) ? propertyName : $"{path}.{propertyName}";
    }
}
