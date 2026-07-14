namespace MultiPurposeServer.Shared.Utils
{
    public class FileNameFormatOptions
    {
        public static FileNameFormatOptions Default => new();
        public bool RemoveExtension { get; set; } = true;
        public bool RemoveLeadingNumericIndex { get; set; } = true;
        public bool RemoveTrailingNumericIndex { get; set; } = true;
        public bool RemoveStandaloneSmallNumbers { get; set; } = true;
        public bool SplitCamelCase { get; set; } = true;
        public bool SplitLetterDigitBoundaries { get; set; } = true;
        public bool NormalizeUnderscores { get; set; } = true;
        public bool NormalizeHyphens { get; set; } = true;
        public string EmptyFallback { get; set; } = "File";
        public bool ApplyTitleCase { get; set; } = true;
        public Dictionary<string, string?> TokenMap { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}