namespace Finance.Api.Application
{
    public interface IFormulaEvaluator
    {
        Task<FormulaEvaluationResult> Evaluate(string formula, DateOnly date);
        Task<FormulaValidationResult> Validate(string formula);
    }
}
