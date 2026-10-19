namespace Finance.Client
{
    /// <summary>Disponibilità residua della carta revolving, con e senza scoperto.</summary>
    public sealed record RevolvingIndicators(decimal Plafond, decimal Overdraft, decimal RemainingPlafond, decimal RemainingIncludingOverdraft);
}
