namespace Finance.Desktop.Models
{
    public sealed record Ciclo(DateOnly From, DateOnly To, decimal Total, IReadOnlyList<MovimentoCiclo> Items);
}
