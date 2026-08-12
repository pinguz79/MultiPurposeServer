namespace MultiPurposeServer.Shared.Utils
{
    public sealed class NamingConventions(string originalName)
    {
        public string Original => originalName;
        public string NameWithoutExtension => Path.GetFileNameWithoutExtension(originalName);
        public IReadOnlyList<string> Tokens => Path.GetFileNameWithoutExtension(originalName).Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        public string? Suffix => Tokens.LastOrDefault();
        public string? SelectionCode => Suffix is not null && char.IsDigit(Suffix[0]) ? Suffix : null;
    }
}
