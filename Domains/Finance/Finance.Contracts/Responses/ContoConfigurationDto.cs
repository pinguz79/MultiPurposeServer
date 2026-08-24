namespace Finance.Contracts.Responses
{
    public sealed record ContoConfigurationDto(Guid Id, string Name, string DisplayName, decimal InitialBalance, decimal Balance);
}
