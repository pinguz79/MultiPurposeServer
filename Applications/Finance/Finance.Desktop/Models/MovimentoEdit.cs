namespace Finance.Desktop.Models
{
    public sealed record MovimentoEdit(Guid Id, DateOnly Date, string Description, string Formula, string ContoName, bool IsConfirmed, Categoria? Category, decimal? Amount, string? EvaluationError);
}
