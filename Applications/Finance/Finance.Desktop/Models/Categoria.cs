namespace Finance.Desktop.Models
{
    public sealed record Categoria(string Name, string DisplayName, int UsageCount)
    {
        public override string ToString() => DisplayName;
    }
}
