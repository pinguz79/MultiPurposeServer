namespace Finance.DataModel.Models
{
    public sealed class FormulaEvaluationException(Movimento movimento, Exception innerException)
        : Exception($"Formula '{movimento.Formula}' cannot be evaluated for movimento '{movimento.Id}'.", innerException)
    {
        public Movimento Movimento { get; } = movimento;
    }
}
