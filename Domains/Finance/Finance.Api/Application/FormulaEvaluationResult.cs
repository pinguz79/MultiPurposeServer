namespace Finance.Api.Application
{
    public sealed record FormulaEvaluationResult(decimal? Value, bool HasUncoveredInterval, string? Error);
}
