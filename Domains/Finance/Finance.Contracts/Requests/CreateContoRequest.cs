namespace Finance.Contracts.Requests
{
    public sealed record CreateContoRequest(string Name, string DisplayName, decimal InitialBalance);
}
