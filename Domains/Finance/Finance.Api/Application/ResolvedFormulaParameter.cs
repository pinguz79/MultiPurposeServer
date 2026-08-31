namespace Finance.Api.Application
{
    public sealed record ResolvedFormulaParameter(string Name, decimal Value, bool HasUncoveredInterval);
}
