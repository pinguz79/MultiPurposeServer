namespace Finance.ExpressionEngineSpike;

internal sealed class FormulaContext
{
    private readonly Dictionary<string, decimal> values = new(StringComparer.OrdinalIgnoreCase);

    public decimal this[string reference]
    {
        get => values[reference];
        set => values[reference] = value;
    }
}
