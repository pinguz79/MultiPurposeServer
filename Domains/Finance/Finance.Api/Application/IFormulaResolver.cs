namespace Finance.Api.Application
{
    public interface IFormulaResolver
    {
        Task<IReadOnlyList<ResolvedFormulaParameter>> Resolve(IReadOnlyList<string> dependencies, DateOnly date);
        Task<string?> ResolveCanonicalName(string dependency);
    }
}
