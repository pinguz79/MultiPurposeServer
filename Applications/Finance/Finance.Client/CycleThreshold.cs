namespace Finance.Client
{
    /// <summary>Data, saldo e importo di superamento di una soglia prevista.</summary>
    public sealed record CycleThreshold(DateOnly Date, decimal Balance, decimal Excess);
}
