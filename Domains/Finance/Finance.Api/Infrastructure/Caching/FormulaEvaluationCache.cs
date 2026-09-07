using Finance.Api.Application;

namespace Finance.Api.Infrastructure.Caching
{
    public sealed class FormulaEvaluationCache
    {
        private readonly Dictionary<(string Formula, DateOnly Date), FormulaEvaluationResult> _results = [];
        private readonly Dictionary<(Guid ContoId, DateOnly ClosingDate), decimal> _closingBalances = [];

        public bool CanReuseAcrossEvaluations { get; private set; } = true;

        public bool TryGetResult(string formula, DateOnly date, out FormulaEvaluationResult? result) => _results.TryGetValue((formula, date), out result);

        public bool TryGetClosingBalance(Guid contoId, DateOnly closingDate, out decimal balance) => _closingBalances.TryGetValue((contoId, closingDate), out balance);

        public void SetResult(string formula, DateOnly date, FormulaEvaluationResult result)
        {
            if (result.Error is null)
            {
                _results[(formula, date)] = result;
            }
        }

        public void SetClosingBalance(Guid contoId, DateOnly closingDate, decimal balance) => _closingBalances[(contoId, closingDate)] = balance;

        public void ClearResults()
        {
            _results.Clear();
            _closingBalances.Clear();
        }

        public void Invalidate()
        {
            ClearResults();
            // Dopo una scrittura riutilizziamo i risultati solo dentro il singolo calcolo, mai oltre commit o rollback.
            CanReuseAcrossEvaluations = false;
        }
    }
}
