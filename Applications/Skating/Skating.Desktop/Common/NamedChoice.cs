namespace Skating.Desktop.Common
{
    internal sealed record NamedChoice(Guid Id, string Name)
    {
        public override string ToString() => Name;
    }
}
