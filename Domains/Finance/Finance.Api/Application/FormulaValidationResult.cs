namespace Finance.Api.Application
{
    public sealed record FormulaValidationResult(
        string Formula,
        IReadOnlyList<string> Dependencies,
        IReadOnlyList<string> Errors)
    {
        public bool IsValid => Errors.Count == 0;
    }
}
