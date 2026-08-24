namespace Finance.Contracts.Requests
{
    public sealed record UpdateContoRequest(string? DisplayName, decimal? InitialBalance);
}
