namespace Finance.Desktop.Models
{
    public sealed record MovimentoEdit(Guid Id, DateOnly Date, string Description, string Formula, string ContoName, bool IsConfirmed, Categoria? Category, decimal? Amount, string? EvaluationError,
        Pedaggio? Pedaggio = null, bool CanConfirm = true, string? ConfirmationWarning = null)
    {
        public string? ReviewWarning => EvaluationError ?? ConfirmationWarning;
    }
}
