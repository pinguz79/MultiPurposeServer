using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class CaselloDto(Casello casello)
    {
        public Guid Id { get; set; } = casello.Id;
        public string Name { get; set; } = casello.Name;
    }
}
