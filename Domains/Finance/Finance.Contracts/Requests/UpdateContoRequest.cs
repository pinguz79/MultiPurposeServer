namespace Finance.Contracts.Requests
{
    public sealed record UpdateContoRequest(string? Name, string? DisplayName, decimal? InitialBalance);
}
