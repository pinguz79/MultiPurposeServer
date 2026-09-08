namespace Finance.Contracts.Responses
{
    public class ConsolidamentoMovimentiDto(int consolidatedCount)
    {
        public int ConsolidatedCount { get; set; } = consolidatedCount;
    }
}
