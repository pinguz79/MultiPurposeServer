using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class ContoConfigurationDto(Conto conto, decimal balance)
    {
        public Guid Id { get; set; } = conto.Id;
        public string Name { get; set; } = conto.Name;
        public string DisplayName { get; set; } = conto.DisplayName;
        public decimal InitialBalance { get; set; } = conto.InitialBalance;
        public decimal Balance { get; set; } = balance;
    }
}
