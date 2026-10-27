namespace Skating.Desktop.Registry
{
    internal sealed record RegistryRow(Guid Id, string Name, string Detail, bool IsPro = false, bool IsAbsent = false);
}
