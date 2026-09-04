namespace Finance.Contracts.Responses
{
    public sealed record CicloDto(
        DateOnly From,
        DateOnly To,
        decimal Total,
        IReadOnlyList<MovimentoCicloDto> Items);
}
