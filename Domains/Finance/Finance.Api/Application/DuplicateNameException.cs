namespace Finance.Api.Application
{
    public sealed class DuplicateNameException(string name) : Exception($"A Conto with Name '{name}' already exists.")
    {
        public string Name { get; } = name;
    }
}
