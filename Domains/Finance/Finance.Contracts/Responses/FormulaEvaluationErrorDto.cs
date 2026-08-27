using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class FormulaEvaluationErrorDto(FormulaEvaluationException exception)
    {
        public Guid MovimentoId { get; set; } = exception.Movimento.Id;
        public DateOnly Date { get; set; } = exception.Movimento.Date;
        public string Description { get; set; } = exception.Movimento.Description;
        public string Formula { get; set; } = exception.Movimento.Formula;
        public string ErrorCode { get; set; } = "FormulaEvaluationFailed";
        public string ErrorMessage { get; set; } = exception.Message;
    }
}
