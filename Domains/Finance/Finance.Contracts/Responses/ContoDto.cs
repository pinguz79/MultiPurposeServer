using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class ContoDto(
        Conto conto,
        decimal balance,
        DateOnly? firstNegativeBalanceDate = null,
        decimal? firstNegativeBalance = null)
    {
        public Guid Id { get; set; } = conto.Id;
        public string Name { get; set; } = conto.Name;
        public string DisplayName { get; set; } = conto.DisplayName;
        public decimal Balance { get; set; } = balance;
        public DateOnly? FirstNegativeBalanceDate { get; set; } = firstNegativeBalanceDate;
        public decimal? FirstNegativeBalance { get; set; } = firstNegativeBalance;
    }
}
