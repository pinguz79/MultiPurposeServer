namespace Finance.Contracts.Responses
{
    public sealed record ContoDto(Guid Id, string Name, string DisplayName, decimal Balance);
}
